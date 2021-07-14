using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Sparrow.Json;

namespace PgRvn.Server
{
    public class RqlQuery : PgQuery
    {
        private readonly IAsyncDocumentSession _session;
        private QueryResult _result;
        private bool _hasId;
        private bool _hasIncludes;

        public RqlQuery(string queryString, int[] parametersDataTypes, IDocumentStore documentStore) : base(queryString, parametersDataTypes)
        {
            _session = documentStore.OpenAsyncSession();
        }

        public override async Task<ICollection<PgColumn>> Init()
        {
            await RunRqlQuery();
            return GenerateSchema();
        }

        public async Task RunRqlQuery()
        {
            var query = _session.Advanced.AsyncRawQuery<BlittableJsonReaderObject>(QueryString);

            if (Parameters != null)
            {
                foreach (var (key, value) in Parameters)
                {
                    query.AddParameter(key, value);
                }
            }

            _result = await ((AsyncDocumentQuery<BlittableJsonReaderObject>)query).GetQueryResultAsync();
        }

        private ICollection<PgColumn> GenerateSchema()
        {
            if (_result?.Results == null)
            {
                return Array.Empty<PgColumn>(); // todo: make sure invalid syntax error is sent or smth
            }

            if (_result.Results.Length == 0)
                return Array.Empty<PgColumn>();

            var sample = (BlittableJsonReaderObject)_result.Results[0];

            if (_result.Includes.Count > 0 ||
                sample.TryGet("@metadata", out BlittableJsonReaderObject metadata) && metadata.TryGet("@id", out string _))
            {
                _hasId = true;
                Columns["id()"] = new PgColumn
                {
                    Name = "id()",
                    FormatCode = PgFormat.Text,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Text,
                    DataTypeSize = -1,
                    ColumnIndex = 0,
                    TableObjectId = 0
                };
            }

            var resultsFormat = ResultColumnFormatCodes.Length switch
            {
                0 => PgFormat.Text,
                1 => ResultColumnFormatCodes[0] == 0 ? PgFormat.Text : PgFormat.Binary,
                _ => throw new NotSupportedException(
                    "No support for column format code count that isn't 0 or 1, got: " +
                    ResultColumnFormatCodes.Length) // TODO: Add support
            };

            BlittableJsonReaderObject.PropertyDetails prop = default;
            for (int i = 0; i < sample.Count; i++)
            {
                sample.GetPropertyByIndex(i, ref prop);
                if (prop.Name == "@metadata")
                    continue;

                var (type, size) = (prop.Token & BlittableJsonReaderBase.TypesMask) switch
                {
                    BlittableJsonToken.Boolean => (PgTypeOIDs.Bool, PgConfig.TrueBuffer.Length),
                    BlittableJsonToken.CompressedString => (PgTypeOIDs.Text, -1),
                    BlittableJsonToken.EmbeddedBlittable => (PgTypeOIDs.Json, -1),
                    BlittableJsonToken.Integer => (PgTypeOIDs.Int8, sizeof(long)),
                    BlittableJsonToken.LazyNumber => (PgTypeOIDs.Float8, sizeof(double)),
                    BlittableJsonToken.Null => (PgTypeOIDs.Json, -1),
                    BlittableJsonToken.String => (PgTypeOIDs.Text, -1),
                    BlittableJsonToken.StartArray => (PgTypeOIDs.Json, -1),
                    BlittableJsonToken.StartObject => (PgTypeOIDs.Json, -1),
                    _ => throw new NotSupportedException()
                };
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

            Columns["@metadata"] = new PgColumn
            {
                Name = "@metadata",
                FormatCode = PgFormat.Text,
                TypeModifier = -1,
                TypeObjectId = PgTypeOIDs.Json,
                DataTypeSize = -1,
                ColumnIndex = (short)Columns.Count,
                TableObjectId = 0
            };


            Columns["json()"] = new PgColumn
            {
                Name = "json()",
                FormatCode = PgFormat.Text,
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
                    FormatCode = PgFormat.Binary,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Bool,
                    DataTypeSize = 1,
                    ColumnIndex = (short)Columns.Count,
                    TableObjectId = 0
                };
            }

            return Columns.Values;
        }

        public override async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(QueryString))
            {
                await writer.WriteAsync(builder.EmptyQueryResponse(), token);
                return;
            }

            if (_result == null)
            {
                await RunRqlQuery();
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
                        var value = (prop.Token & BlittableJsonReaderBase.TypesMask, pgColumn.TypeObjectId) switch
                        {
                            (BlittableJsonToken.Boolean, PgTypeOIDs.Bool) => (bool)prop.Value ? PgConfig.TrueBuffer : PgConfig.FalseBuffer,
                            (BlittableJsonToken.CompressedString, PgTypeOIDs.Text) => Encoding.UTF8.GetBytes(prop.Value.ToString()),
                            (BlittableJsonToken.EmbeddedBlittable, PgTypeOIDs.Json) => Encoding.UTF8.GetBytes(prop.Value.ToString()),
                            (BlittableJsonToken.Integer, PgTypeOIDs.Int8) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)prop.Value)),
                            (BlittableJsonToken.LazyNumber, PgTypeOIDs.Float8) => BitConverter.GetBytes((double)(LazyNumberValue)prop.Value).Reverse().ToArray(),
                            (BlittableJsonToken.String, PgTypeOIDs.Text) => Encoding.UTF8.GetBytes(prop.Value.ToString()),
                            (BlittableJsonToken.StartArray, PgTypeOIDs.Json) => Encoding.UTF8.GetBytes(prop.Value.ToString()),
                            (BlittableJsonToken.StartObject, PgTypeOIDs.Json) => Encoding.UTF8.GetBytes(prop.Value.ToString()),
                            (BlittableJsonToken.Null, PgTypeOIDs.Json) => Array.Empty<byte>(),
                            _ => null
                        };

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
