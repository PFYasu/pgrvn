﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PgRvn.Server.Messages;
using PgRvn.Server.Types;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Sparrow.Json;
using Sparrow.Json.Parsing;

namespace PgRvn.Server
{
    public class RqlQuery : PgQuery
    {
        protected readonly IDocumentStore DocumentStore;
        private readonly IAsyncDocumentSession _session;
        private QueryResult _result;
        private readonly int? _limit;
        private Operation _operation;

        public RqlQuery(string queryString, int[] parametersDataTypes, IDocumentStore documentStore, int? limit = null) : base(queryString, parametersDataTypes)
        {
            DocumentStore = documentStore;
            _session = documentStore.OpenAsyncSession();
            _result = null;
            _limit = limit;
            _operation = null;
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements = false)
        {
            if (IsEmptyQuery)
            {
                return default;
            }

            await RunRqlQuery();
            return GenerateSchema();
        }

        public async Task RunRqlQuery()
        {
            var query = _session.Advanced.AsyncRawQuery<BlittableJsonReaderObject>(QueryString);

            // If limit is 0, fetch one document for the schema generation
            if (_limit != null)
                query.Take(_limit.Value == 0 ? 1 : _limit.Value);

            // TODO: Support skipping (check how/if powerbi sends it, probably using the incremental refresh feature)
            // query.Skip(..)

            var patchParams = new Raven.Client.Parameters();
            if (Parameters != null)
            {
                foreach (var (key, value) in Parameters)
                {
                    query.AddParameter(key, value);
                    patchParams.Add(key, value);
                }
            }

            // TODO: Find a better solution for running Patch RQL
            try
            {
                _result = await ((AsyncDocumentQuery<BlittableJsonReaderObject>)query).GetQueryResultAsync();
            }
            catch (Raven.Client.Exceptions.RavenException)
            {
                _operation = await DocumentStore.Operations.SendAsync(new PatchByQueryOperation(new IndexQuery
                {
                    Query = QueryString,
                    QueryParameters = patchParams
                }));
            }
        }

        private ICollection<PgColumn> GenerateSchema()
        {
            if (_result?.Results == null || _result?.Results.Length == 0)
            {
                return Array.Empty<PgColumn>();
            }

            var resultsFormat = GetDefaultResultsFormat();
            var sample = (BlittableJsonReaderObject)_result.Results[0];

            if (sample.TryGet("@metadata", out BlittableJsonReaderObject metadata) && metadata.TryGet("@id", out string _))
            {
                Columns["id()"] = new PgColumn("id()", (short)Columns.Count, PgText.Default, resultsFormat);
            }

            // Go over sample's columns
            var properties = sample.GetPropertyNames();
            BlittableJsonReaderObject.PropertyDetails prop = default;
            for (var i = 0; i < properties.Length; i++)
            {
                // Using GetPropertyIndex to get the properties in the right order
                var propIndex = sample.GetPropertyIndex(properties[i]);
                sample.GetPropertyByIndex(propIndex, ref prop);

                // Skip this column, will be added later to json() column
                if (prop.Name == "@metadata")
                    continue;

                PgType pgType = (prop.Token & BlittableJsonReaderBase.TypesMask) switch
                {
                    BlittableJsonToken.CompressedString => PgText.Default,
                    BlittableJsonToken.String => PgText.Default,
                    BlittableJsonToken.Boolean => PgBool.Default,
                    BlittableJsonToken.EmbeddedBlittable => PgJson.Default,
                    BlittableJsonToken.Integer => PgInt8.Default,
                    BlittableJsonToken.LazyNumber => PgFloat8.Default,
                    BlittableJsonToken.Null => PgJson.Default,
                    BlittableJsonToken.StartArray => PgJson.Default,
                    BlittableJsonToken.StartObject => PgJson.Default,
                    _ => throw new NotSupportedException()
                };

                var processedString = (prop.Token & BlittableJsonReaderBase.TypesMask) switch
                {
                    BlittableJsonToken.CompressedString => (string)prop.Value,
                    BlittableJsonToken.String => (LazyStringValue)prop.Value,
                    _ => null
                };

                if (processedString != null && TryConvertStringValue(processedString, out var output))
                {
                    pgType = output switch
                    {
                        DateTime dt => (dt.Kind == DateTimeKind.Utc) ? PgTimestampTz.Default : PgTimestamp.Default,
                        DateTimeOffset => PgTimestampTz.Default,
                        TimeSpan => PgInterval.Default,
                        _ => pgType
                    };
                }

                Columns.TryAdd(prop.Name, new PgColumn(prop.Name, (short)Columns.Count, pgType, resultsFormat));
            }

            if (Columns.TryGetValue("json()", out var jsonColumn))
            {
                jsonColumn.PgType = PgJson.Default;
            }
            else
            {
                Columns["json()"] = new PgColumn("json()", (short)Columns.Count, PgJson.Default, resultsFormat);
            }

            return Columns.Values;
        }

        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out RqlQuery rqlQuery)
        {
            // TODO: Use QueryParser to try and parse the query
            if (queryText.StartsWith("from", StringComparison.CurrentCultureIgnoreCase) ||
                queryText.StartsWith("/*rql*/", StringComparison.CurrentCultureIgnoreCase))
            {
                rqlQuery = new RqlQuery(queryText, parametersDataTypes, documentStore);
                return true;
            }

            rqlQuery = null;
            return false;
        }

        // TODO: Taken from TypeConverter.cs in Raven.Server - use that when migrating
        private static unsafe bool TryConvertStringValue(string value, out object output)
        {
            output = null;

            fixed (char* str = value)
            {
                var result = LazyStringParser.TryParseDateTime(str, value.Length, out DateTime dt, out DateTimeOffset dto);
                if (result == LazyStringParser.Result.DateTime)
                    output = dt;
                if (result == LazyStringParser.Result.DateTimeOffset)
                    output = dto;

                if (LazyStringParser.TryParseTimeSpan(str, value.Length, out var ts))
                    output = ts;
            }

            return output != null;
        }

        public override async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            if (IsEmptyQuery)
            {
                await writer.WriteAsync(builder.EmptyQueryResponse(), token);
                return;
            }

            if (_operation != null)
            {
                // todo: is this a safe cast?
                var result = (BulkOperationResult)await _operation.WaitForCompletionAsync();

                await writer.WriteAsync(builder.CommandComplete($"UPDATE {result.Total}"), token);
                return;
            }

            if (_result == null)
            {
                throw new InvalidOperationException("RqlQuery.Execute was called when _results = null");
            }

            if (_limit == 0 || _result.Results == null || _result.Results.Length == 0)
            {
                await writer.WriteAsync(builder.CommandComplete($"SELECT 0"), token);
                return;
            }

            BlittableJsonReaderObject.PropertyDetails prop = default;
            var row = ArrayPool<ReadOnlyMemory<byte>?>.Shared.Rent(Columns.Count);
            try
            {
                // TODO: Typically ColumnIndex represents the index in the Postgres table, but we use it here for the order of columns sent
                short? idIndex = null;
                if (Columns.TryGetValue("id()", out var col))
                {
                    idIndex = col.ColumnIndex;
                }

                var jsonIndex = Columns["json()"].ColumnIndex;

                foreach (BlittableJsonReaderObject result in _result.Results)
                {
                    Array.Clear(row, 0, row.Length);

                    if (idIndex != null && 
                        result.TryGet("@metadata", out BlittableJsonReaderObject metadata) &&
                        metadata.TryGet("@id", out string id))
                    {
                        row[idIndex.Value] = Encoding.UTF8.GetBytes(id);
                    }

                    result.Modifications = new DynamicJsonValue(result);

                    foreach (var (key, pgColumn) in Columns)
                    {
                        var index = result.GetPropertyIndex(key);
                        if (index == -1)
                            continue;
                        result.GetPropertyByIndex(index, ref prop);

                        ReadOnlyMemory<byte>? value = null;
                        switch (prop.Token & BlittableJsonReaderBase.TypesMask, pgColumn.PgType.Oid)
                        {
                            case (BlittableJsonToken.Boolean, PgTypeOIDs.Bool):
                            case (BlittableJsonToken.CompressedString, PgTypeOIDs.Text):
                            case (BlittableJsonToken.EmbeddedBlittable, PgTypeOIDs.Json):
                            case (BlittableJsonToken.Integer, PgTypeOIDs.Int8):
                            case (BlittableJsonToken.String, PgTypeOIDs.Text):
                            case (BlittableJsonToken.StartArray, PgTypeOIDs.Json):
                            case (BlittableJsonToken.StartObject, PgTypeOIDs.Json):
                                value = pgColumn.PgType.ToBytes(prop.Value, pgColumn.FormatCode);
                                break;
                            case (BlittableJsonToken.LazyNumber, PgTypeOIDs.Float8):
                                value = pgColumn.PgType.ToBytes((double)(LazyNumberValue)prop.Value, pgColumn.FormatCode);
                                break;

                            case (BlittableJsonToken.CompressedString, PgTypeOIDs.Timestamp):
                            case (BlittableJsonToken.CompressedString, PgTypeOIDs.TimestampTz):
                            case (BlittableJsonToken.CompressedString, PgTypeOIDs.Interval):
                                {
                                    if (((string)prop.Value).Length != 0 && 
                                        TryConvertStringValue((string)prop.Value, out var obj))
                                    {
                                        value = pgColumn.PgType.ToBytes(obj, pgColumn.FormatCode);
                                    }
                                    break;
                                }
                            case (BlittableJsonToken.String, PgTypeOIDs.Timestamp):
                            case (BlittableJsonToken.String, PgTypeOIDs.TimestampTz):
                            case (BlittableJsonToken.String, PgTypeOIDs.Interval):
                                {
                                    if (((LazyStringValue)prop.Value).Length != 0 && 
                                        TryConvertStringValue((string)(LazyStringValue)prop.Value, out object obj))
                                    {
                                        // TODO: Make pretty
                                        // Check for mismatch between column type and our data type
                                        if (obj is DateTime dt)
                                        {
                                            if (dt.Kind == DateTimeKind.Utc && pgColumn.PgType is not PgTimestampTz)
                                            {
                                                break;
                                            }
                                            else if (dt.Kind != DateTimeKind.Utc && pgColumn.PgType is not PgTimestamp)
                                            {
                                                break;
                                            }
                                        }

                                        if (obj is DateTimeOffset && pgColumn.PgType is not PgTimestampTz)
                                        {
                                            break;
                                        }

                                        if (obj is TimeSpan && pgColumn.PgType is not PgInterval)
                                        {
                                            break;
                                        }

                                        value = pgColumn.PgType.ToBytes(obj, pgColumn.FormatCode);
                                    }
                                    break;
                                }
                            case (BlittableJsonToken.String, PgTypeOIDs.Float8):
                                value = pgColumn.PgType.ToBytes(double.Parse((LazyStringValue)prop.Value), pgColumn.FormatCode);
                                break;
                            case (BlittableJsonToken.Null, PgTypeOIDs.Json):
                                value = Array.Empty<byte>();
                                break;
                        }

                        if (value == null)
                        {
                            continue;
                        }
                        row[pgColumn.ColumnIndex] = value;
                        result.Modifications.Remove(key);
                    }

                    if (result.Modifications.Removals.Count != result.Count)
                    {
                        var modified = _session.Advanced.Context.ReadObject(result, "renew");
                        row[jsonIndex] = Encoding.UTF8.GetBytes(modified.ToString());
                    }

                    await writer.WriteAsync(builder.DataRow(row[..Columns.Count]), token);
                }
            }
            finally
            {
                ArrayPool<ReadOnlyMemory<byte>?>.Shared.Return(row);
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {_result.Results.Length}"), token);
        }

        public override void Dispose()
        {
            _session?.Dispose();
        }
    }
}
