using Raven.Client.Documents.Session;
using Sparrow.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Queries;

namespace PgRvn.Server
{
    public class PgQuery
    {
        public string QueryText;

        public Dictionary<string, object> Parameters;
        private QueryResult _result;
        public IAsyncDocumentSession Session;
        public Dictionary<string, PgColumn> Columns = new();
        private bool _hasId;
        public int[] ParametersDataTypes;

        public async Task<ICollection<PgColumn>> Init()
        {
            if (string.IsNullOrWhiteSpace(QueryText))
            {
                return default;
            }

            await RunQuery();
            return GenerateSchema();
        }

        public static byte[] TrueBuffer = { 1 }, FalseBuffer = { 0 };
        private bool _hasIncludes;

        public async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(QueryText))
            {
                await writer.WriteAsync(builder.EmptyQueryResponse(), token);
                return;
            }

            if (_result == null)
            {
                await RunQuery();
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

                    foreach (var col in Columns)
                    {
                        var index = result.GetPropertyIndex(col.Key);
                        if (index == -1)
                            continue;
                        result.GetPropertyByIndex(index, ref prop);
                        var value = (prop.Token & BlittableJsonReaderBase.TypesMask, col.Value.TypeObjectId) switch
                        {
                            (BlittableJsonToken.Boolean, PgTypeOIDs.Bool) => (bool)prop.Value ? TrueBuffer : FalseBuffer,
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
                        row[col.Value.ColumnIndex] = value;
                        result.Modifications.Remove(col.Key);
                    }

                    if (result.Modifications.Removals.Count != result.Count)
                    {
                        var modified = Session.Advanced.Context.ReadObject(result, "renew");
                        row[jsonIndex] = Encoding.UTF8.GetBytes(modified.ToString());
                    }

                    if (_hasIncludes)
                    {
                        row[includesIndex] = FalseBuffer;
                    }
                    await writer.WriteAsync(builder.DataRow(row[..Columns.Count]), token);
                }

                for (int i = 0; i < _result.Includes.Count; i++)
                {
                    Array.Clear(row, 0, row.Length);

                    _result.Includes.GetPropertyByIndex(i, ref prop);

                    row[0] = Encoding.UTF8.GetBytes(prop.Name);
                    row[jsonIndex] = Encoding.UTF8.GetBytes(prop.Value.ToString());
                    row[includesIndex] = TrueBuffer;
                    await writer.WriteAsync(builder.DataRow(row[..Columns.Count]), token);
                }
            }
            finally
            {
                ArrayPool<ReadOnlyMemory<byte>?>.Shared.Return(row);
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {_result.Results.Length + _result.Includes.Count}"), token);
        }

        public async Task RunQuery()
        {
            var query = Session.Advanced.AsyncRawQuery<BlittableJsonReaderObject>(QueryText);

            if (Parameters != null)
            {
                foreach (var (key, value) in Parameters)
                {
                    query.AddParameter(key, value);
                }
            }

            _result = await ((AsyncDocumentQuery<BlittableJsonReaderObject>) query).GetQueryResultAsync();
        }

        private ICollection<PgColumn> GenerateSchema()
        {
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

            BlittableJsonReaderObject.PropertyDetails prop = default;
            for (int i = 0; i < sample.Count; i++)
            {
                sample.GetPropertyByIndex(i, ref prop);
                if (prop.Name == "@metadata")
                    continue;

                var (type, size, format) = (prop.Token & BlittableJsonReaderBase.TypesMask) switch
                {
                    BlittableJsonToken.Boolean => (PgTypeOIDs.Bool, sizeof(bool), PgFormat.Binary),
                    BlittableJsonToken.CompressedString => (PgTypeOIDs.Text, -1, PgFormat.Text),
                    BlittableJsonToken.EmbeddedBlittable => (PgTypeOIDs.Json, -1, PgFormat.Text),
                    BlittableJsonToken.Integer => (PgTypeOIDs.Int8, sizeof(long), PgFormat.Binary),
                    BlittableJsonToken.LazyNumber => (PgTypeOIDs.Float8, sizeof(double), PgFormat.Binary),
                    BlittableJsonToken.Null => (PgTypeOIDs.Json, -1, PgFormat.Text),
                    BlittableJsonToken.String => (PgTypeOIDs.Text, -1, PgFormat.Text),
                    BlittableJsonToken.StartArray => (PgTypeOIDs.Json, -1, PgFormat.Text),
                    BlittableJsonToken.StartObject => (PgTypeOIDs.Json, -1, PgFormat.Text),
                    _ => throw new NotSupportedException()
                };
                Columns[prop.Name] = new PgColumn
                {
                    Name = prop.Name,
                    FormatCode = format,
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
    }
}