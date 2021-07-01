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

namespace PgRvn.Server
{
    public class Query
    {
        public string QueryText;

        public Dictionary<string, object> Parameters;
        private List<BlittableJsonReaderObject> _results;
        public IAsyncDocumentSession Session;
        public Dictionary<string, PgColumn> Columns = new Dictionary<string, PgColumn>();
        private bool _hasId;
        public int[] ParametersDataTypes;

        public async Task Init(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            await RunQuery();
            var schema = GenerateSchema();
            await writer.WriteAsync(builder.RowDescription(schema), token);
        }

        public static byte[] TrueBuffer = new byte[] { 1 }, FalseBuffer = new byte[] { 0 };
        public async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            BlittableJsonReaderObject.PropertyDetails prop = default;
            var row = ArrayPool<ReadOnlyMemory<byte>?>.Shared.Rent(Columns.Count);
            try
            {
                foreach (var result in _results)
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
                        row[^1] = Encoding.UTF8.GetBytes(modified.ToString());
                    }
                    await writer.WriteAsync(builder.DataRow(row[..Columns.Count]), token);
                }
            }
            finally
            {
                ArrayPool<ReadOnlyMemory<byte>?>.Shared.Return(row);
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {_results.Count}"), token);
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

            _results = await query.ToListAsync();
        }

        private ICollection<PgColumn> GenerateSchema()
        {
            if (_results.Count == 0)
                return Array.Empty<PgColumn>();

            var sample = _results[0];
            
            if (sample.TryGet("@metadata", out BlittableJsonReaderObject metadata) && metadata.TryGet("@id", out string _))
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

            return Columns.Values;
        }
    }
}