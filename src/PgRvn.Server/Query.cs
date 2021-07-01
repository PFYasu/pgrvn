using Raven.Client.Documents.Session;
using Sparrow.Json;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
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


        public async Task Init(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            await RunQuery();
            var schema = GenerateSchema();
            await writer.WriteAsync(builder.RowDescription(schema), token);
        }

        public async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            var buffer = new byte[8192];
            Memory<byte> mem = buffer;
            foreach (var result in _results)
            {
                var row = new List<PgColumnData>();
                foreach (var (_, val) in result)
                {
                    if (val is null)
                    {
                        row.Add(PgColumnData.Null);
                        continue;
                    }

                    if (val is int i)
                    {
                        MemoryMarshal.Cast<byte, int>(mem.Span)[0] = i;
                        row.Add(new PgColumnData
                        {
                            Data = mem[..sizeof(int)]
                        });
                        mem = mem[sizeof(int)..];
                        continue;
                    }
                    if (val is long l)
                    {
                        MemoryMarshal.Cast<byte, long>(mem.Span)[0] = l;
                        row.Add(new PgColumnData
                        {
                            Data = mem[..sizeof(long)]
                        });
                        mem = mem[sizeof(long)..];
                        continue;
                    }

                    if (val is string s)
                    {
                        row.Add(new PgColumnData
                        {
                            Data = Encoding.UTF8.GetBytes(s)
                        });
                        continue;
                    }

                    throw new NotSupportedException();
                }

                await writer.WriteAsync(builder.DataRow(row), token);
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


            BlittableJsonReaderObject.PropertyDetails prop = default;
            for (int i = 0; i < sample.Count; i++)
            {
                sample.GetPropertyByIndex(i, ref prop, false);
                var (type, size, format) = (prop.Token & BlittableJsonReaderBase.TypesMask) switch
                {
                    BlittableJsonToken.Boolean => (PgTypeOIDs.Bool, sizeof(bool), PgFormat.Binary),
                    BlittableJsonToken.CompressedString => (PgTypeOIDs.Text, -1, PgFormat.Text),
                    BlittableJsonToken.EmbeddedBlittable => (PgTypeOIDs.Json, -1, PgFormat.Text),
                    BlittableJsonToken.Integer => (PgTypeOIDs.Int8, sizeof(long), PgFormat.Binary),
                    BlittableJsonToken.LazyNumber => (PgTypeOIDs.Float8, sizeof(double), PgFormat.Binary),
                    BlittableJsonToken.Null => (PgTypeOIDs.Json, -1, PgFormat.Text),
                    BlittableJsonToken.String => (PgTypeOIDs.Text, -1, PgFormat.Text),
                    BlittableJsonToken.StartArray => (PgTypeOIDs.Json,-1, PgFormat.Text),
                    BlittableJsonToken.StartObject => (PgTypeOIDs.Json,-1, PgFormat.Text),
                    _ => throw new NotSupportedException()
                };
                Columns[prop.Name] = new PgColumn
                {
                    Name = prop.Name,
                    FormatCode = format,
                    TypeModifier = -1,
                    TypeObjectId = type,
                    DataTypeSize = (short)size,
                    ColumnIndex = (short)i,
                    TableObjectId = 0
                };
            }

            return Columns.Values;
        }
    }
}