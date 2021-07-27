using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Sparrow.Json;

namespace PgRvn.Server
{
    public class RqlQuery : PgQuery
    {
        private readonly IDocumentStore _documentStore;
        private readonly IAsyncDocumentSession _session;
        private QueryResult _result;
        private bool _hasId;
        private bool _hasIncludes;
        private bool _isInitialPowerBIQuery;
        private Operation _operation;

        public RqlQuery(string queryString, int[] parametersDataTypes, IDocumentStore documentStore, bool isInitialPowerBIQuery) : base(queryString, parametersDataTypes)
        {
            _documentStore = documentStore;
            _session = documentStore.OpenAsyncSession();
            _isInitialPowerBIQuery = isInitialPowerBIQuery; // TODO: If true, limit results to 0 (don't send results, just schema)
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements = false)
        {
            if (IsEmptyQuery)
            {
                return default;
            }

            // todo: handle allowMultipleStatements

            await RunRqlQuery();
            return GenerateSchema();
        }

        public async Task RunRqlQuery()
        {
            var query = _session.Advanced.AsyncRawQuery<BlittableJsonReaderObject>(QueryString);
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
                _operation = _documentStore.Operations.Send(new PatchByQueryOperation(new IndexQuery
                {
                    Query = QueryString,
                    QueryParameters = patchParams
                }));
            }
        }

        private ICollection<PgColumn> GenerateSchema()
        {
            if (_result?.Results == null)
            {
                return Array.Empty<PgColumn>(); // todo: make sure invalid syntax error is sent or smth
            }

            if (_result.Results.Length == 0)
                return Array.Empty<PgColumn>();


            var resultsFormat = GetDefaultResultsFormat();

            var sample = (BlittableJsonReaderObject)_result.Results[0];

            if (_result.Includes.Count > 0 ||
                sample.TryGet("@metadata", out BlittableJsonReaderObject metadata) && metadata.TryGet("@id", out string _))
            {
                _hasId = true;
                Columns["id()"] = new PgColumn
                {
                    Name = "id()",
                    FormatCode = resultsFormat,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Json,
                    DataTypeSize = -1,
                    ColumnIndex = 0,
                    TableObjectId = 0
                };
            }

            BlittableJsonReaderObject.PropertyDetails prop = default;
            for (int i = 0; i < sample.Count; i++)
            {
                sample.GetPropertyByIndex(i, ref prop);
                if (prop.Name == "@metadata")
                    continue;

                var (type, size) = (prop.Token & BlittableJsonReaderBase.TypesMask) switch
                {
                    BlittableJsonToken.CompressedString => (PgTypeOIDs.Text, -1),
                    BlittableJsonToken.String => (PgTypeOIDs.Text, -1),
                    BlittableJsonToken.Boolean => (PgTypeOIDs.Bool, PgConfig.TrueBuffer.Length),
                    BlittableJsonToken.EmbeddedBlittable => (PgTypeOIDs.Json, -1),
                    BlittableJsonToken.Integer => (PgTypeOIDs.Int8, sizeof(long)),
                    BlittableJsonToken.LazyNumber => (PgTypeOIDs.Float8, sizeof(double)),
                    BlittableJsonToken.Null => (PgTypeOIDs.Json, -1),
                    BlittableJsonToken.StartArray => (PgTypeOIDs.Json, -1),
                    BlittableJsonToken.StartObject => (PgTypeOIDs.Json, -1),
                    _ => throw new NotSupportedException()
                };

                string val = (prop.Token & BlittableJsonReaderBase.TypesMask) switch
                {
                    BlittableJsonToken.CompressedString => (string)prop.Value,
                    BlittableJsonToken.String => (LazyStringValue)prop.Value,
                    _ => null
                };

                if (val != null && TryConvertStringValue(val, out object output))
                {
                    type = output switch
                    {
                        DateTime => PgTypeOIDs.Timestamp, // TODO: Maybe use Time ?
                        DateTimeOffset => PgTypeOIDs.TimestampTz, // TODO: Maybe use TimeTz ?
                        TimeSpan => PgTypeOIDs.Time, // TODO: Maybe use Interval ? (see: https://www.npgsql.org/doc/types/datetime.html)
                        _ => type
                    };
                }

                Columns[prop.Name] = new PgColumn
                {
                    Name = prop.Name,
                    FormatCode = resultsFormat,
                    TypeModifier = -1,
                    TypeObjectId = type,
                    DataTypeSize = (short)size,
                    ColumnIndex = (short)Columns.Count,
                    TableObjectId = 0
                };
            }

            //Columns["@metadata"] = new PgColumn
            //{
            //    Name = "@metadata",
            //    FormatCode = resultsFormat,
            //    TypeModifier = -1,
            //    TypeObjectId = PgTypeOIDs.Text,
            //    DataTypeSize = -1,
            //    ColumnIndex = (short)Columns.Count,
            //    TableObjectId = 0
            //};


            Columns["json()"] = new PgColumn
            {
                Name = "json()",
                FormatCode = resultsFormat,
                TypeModifier = -1,
                TypeObjectId = PgTypeOIDs.Json,
                DataTypeSize = -1,
                ColumnIndex = (short)Columns.Count,
                TableObjectId = 0
            };

            if (_result.Includes.Count != 0)
            {
                _hasIncludes = true;
                Columns["is_include()"] = new PgColumn
                {
                    Name = "is_include()",
                    FormatCode = resultsFormat,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Bool,
                    DataTypeSize = 1,
                    ColumnIndex = (short)Columns.Count,
                    TableObjectId = 0
                };
            }

            return Columns.Values;
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

            if (_isInitialPowerBIQuery)
            {
                await writer.WriteAsync(builder.CommandComplete($"SELECT 0"), token);
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

            BlittableJsonReaderObject.PropertyDetails prop = default;
            var row = ArrayPool<ReadOnlyMemory<byte>?>.Shared.Rent(Columns.Count);
            try
            {
                var jsonIndex = _hasIncludes ? Columns.Count - 2 : Columns.Count - 1;
                var includesIndex = Columns.Count - 1;

                foreach (BlittableJsonReaderObject result in _result.Results)
                {
                    Array.Clear(row, 0, row.Length);

                    if (_hasId)
                    {
                        if (result.TryGet("@metadata", out BlittableJsonReaderObject metadata) &&
                            metadata.TryGet("@id", out string id))
                        {
                            row[0] = Encoding.UTF8.GetBytes(id);
                        }
                    }

                    result.Modifications = new Sparrow.Json.Parsing.DynamicJsonValue(result);

                    foreach (var (key, pgColumn) in Columns)
                    {
                        var index = result.GetPropertyIndex(key);
                        if (index == -1)
                            continue;
                        result.GetPropertyByIndex(index, ref prop);

                        byte[] value = null;
                        switch (prop.Token & BlittableJsonReaderBase.TypesMask, pgColumn.TypeObjectId)
                        {
                            case (BlittableJsonToken.Boolean, PgTypeOIDs.Bool):
                            case (BlittableJsonToken.CompressedString, PgTypeOIDs.Text):
                            case (BlittableJsonToken.EmbeddedBlittable, PgTypeOIDs.Json):
                            case (BlittableJsonToken.Integer, PgTypeOIDs.Int8):
                            case (BlittableJsonToken.String, PgTypeOIDs.Text):
                            case (BlittableJsonToken.StartArray, PgTypeOIDs.Json):
                            case (BlittableJsonToken.StartObject, PgTypeOIDs.Json):
                                value = PgTypeConverter.ToBytes[(pgColumn.TypeObjectId, pgColumn.FormatCode)](prop.Value);
                                break;
                            case (BlittableJsonToken.LazyNumber, PgTypeOIDs.Float8):
                                value = PgTypeConverter.ToBytes[(pgColumn.TypeObjectId, pgColumn.FormatCode)]((double)(LazyNumberValue) prop.Value);
                                break;

                            case (BlittableJsonToken.String, PgTypeOIDs.Float8):
                                value = PgTypeConverter.ToBytes[(pgColumn.TypeObjectId, pgColumn.FormatCode)]((double)(LazyStringValue)prop.Value);
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

                    if (_hasIncludes)
                    {
                        row[includesIndex] = PgConfig.FalseBuffer;
                    }
                    await writer.WriteAsync(builder.DataRow(row[..Columns.Count]), token);
                }

                for (int i = 0; i < _result.Includes.Count; i++)
                {
                    Array.Clear(row, 0, row.Length);

                    _result.Includes.GetPropertyByIndex(i, ref prop);

                    row[0] = Encoding.UTF8.GetBytes(prop.Name);
                    row[jsonIndex] = Encoding.UTF8.GetBytes(prop.Value.ToString());
                    row[includesIndex] = PgConfig.TrueBuffer;
                    await writer.WriteAsync(builder.DataRow(row[..Columns.Count]), token);
                }
            }
            finally
            {
                ArrayPool<ReadOnlyMemory<byte>?>.Shared.Return(row);
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {_result.Results.Length + _result.Includes.Count}"), token);
        }

        public override void Dispose()
        {
            _session?.Dispose();
        }
    }
}
