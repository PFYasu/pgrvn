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
        private List<Dictionary<string, object>> _results;

        private List<Dictionary<string, object>> FromEmployees()
        {
            var doc = @"{""LastName"":""Buchanan"",""FirstName"":""Steven"",""Title"":""Sales Manager""}";
            return new List<Dictionary<string, object>>
            {
                new()
                {
                    ["id()"] = "employees/1-A",
                    ["json"] = doc
                }
            };
        }

        public async Task Init(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            var pgColumns = RunQuery();
            await writer.WriteAsync(builder.RowDescription(pgColumns), token);
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

        public List<PgColumn> RunQuery()
        {
            switch (QueryText)
            {
                case "from Employees":
                    _results = FromEmployees();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(QueryText);
            }

            if (_results.Count == 0)
            {
                // TODO: maybe need to default here or something
                return new List<PgColumn>(); // not sure if okay!
            }

            var cols = new List<PgColumn>();

            foreach (var (key,val) in _results[0])
            {
                cols.Add(new PgColumn
                {
                    Name = key,
                    FormatCode = val switch
                    {
                        double or float or int or long or short or byte => PgFormat.Binary,
                        string => PgFormat.Text,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    TypeModifier = -1,
                    TypeObjectId = val switch
                    {
                        int => PgTypeOIDs.Int4,
                        long => PgTypeOIDs.Int8,
                        float => PgTypeOIDs.Float4,
                        double => PgTypeOIDs.Float8,
                        short => PgTypeOIDs.Int2,
                        string => PgTypeOIDs.Text,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    DataTypeSize = val switch
                    {
                        int => sizeof(int),
                        long => sizeof(long),
                        float => sizeof(float),
                        double => sizeof(double),
                        short => sizeof(short),
                        string => -1,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    ColumnIndex = (short)cols.Count,
                });
            }

            return cols;
        }
    }
}