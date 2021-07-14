using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents.Linq.Indexing;
using Raven.Client.Documents.Queries;
using Sparrow.Json;
using TSQL;
using TSQL.Statements;
using TSQL.Tokens;


namespace PgRvn.Server
{
    public class SqlQuery : PgQuery
    {
        private PgTable _result;

        public SqlQuery(string queryString, int[] parametersDataTypes) : base(queryString, parametersDataTypes)
        {
        }

        // private static PgColumnData[] Process(TSQLStatement stmt, ref int offset, out PgColumn[] columns, out int rowsOperated)
        // {
        //     // Process SELECT queryString
        //     if (stmt.AsSelect != null)
        //     {
        //         // SELECT without FROM
        //         if (stmt.AsSelect.From == null)
        //         {
        //             var identifier = stmt.AsSelect.Select.Tokens[offset];
        //             switch (identifier.Type)
        //             {
        //                 case TSQLTokenType.Identifier:
        //                     if (offset + 1 < stmt.AsSelect.Select.Tokens.Count)
        //                     {
        //                         if (stmt.AsSelect.Select.Tokens[offset + 1].Text == "(")
        //                         {
        //                             offset += 2;
        //                             var result = methods[identifier.Text](stmt.AsSelect.Select.Tokens, ref offset,
        //                                 out var outColumns);
        //                             columns = outColumns;
        //                             rowsOperated = 1;
        //                             return new[]
        //                             {
        //                                 new PgColumnData
        //                                 {
        //                                     Data = result
        //                                 }
        //                             };
        //                         }
        //                     }
        //
        //                     break;
        //                 case TSQLTokenType.MultilineComment:
        //                 case TSQLTokenType.IncompleteComment:
        //                 case TSQLTokenType.SingleLineComment:
        //                     offset++;
        //                     break;
        //                 default:
        //                     throw new NotSupportedException(identifier.ToString());
        //             }
        //         }
        //         else
        //         {
        //             // SELECT with FROM
        //
        //         }
        //     }
        //
        //     rowsOperated = 0;
        //     columns = Array.Empty<PgColumn>();
        //     return Array.Empty<PgColumnData>();
        // }

        // private delegate ReadOnlyMemory<byte> SqlMethodDelegate(List<TSQLToken> tokens, ref int offset, out PgColumn[] columns);
        //
        // private static readonly Dictionary<string, SqlMethodDelegate> methods = new(StringComparer.OrdinalIgnoreCase)
        // {
        //     ["version"] = VersionMethod
        // };
        //
        // private static ReadOnlyMemory<byte> VersionMethod(List<TSQLToken> tokens, ref int offset, out PgColumn[] columns)
        // {
        //     if (tokens[offset].Text != ")")
        //         throw new InvalidOperationException("version() expects no arguments.");
        //
        //     columns = new[]
        //     {
        //         new PgColumn
        //         {
        //             Name = "version",
        //             TableObjectId = 0,
        //             ColumnIndex = 0,
        //             TypeObjectId = PgTypeOIDs.Text,
        //             DataTypeSize = -1, // or -2 if null terminated
        //             TypeModifier = -1,
        //             FormatCode = PgFormat.Text
        //         }
        //     };
        //
        //     offset++;
        //
        //     return Encoding.ASCII.GetBytes("PostgreSQL 13.3, compiled by Visual C++ build 1914, 64-bit");
        // }

        public bool ParseSingleStatement()
        {
            var sqlStatements = TSQLStatementReader.ParseStatements(QueryString);
            if (sqlStatements.Count != 1)
            {
                throw new InvalidOperationException("Didn't expect more than one SQL statement in queryString, got: " + sqlStatements.Count);
            }

            var stmt = sqlStatements[0];

            //stmt.AsSelect.From - join between pg_namespace and typ_and_elem_type and 
            // queryString.Contains("-- We first do this for the type (innerest-most subquery), and then for its element type")
            // because this marks: https://github.com/npgsql/npgsql/blob/792b144e82b39bd09bb081c5617ffec907f07316/src/Npgsql/PostgresDatabaseInfo.cs#L121
            // now return hard coded response

            if (stmt.AsSelect != null)
            {
                int offset = stmt.AsSelect.BeginPosition;
                HandleSelect(stmt.AsSelect, ref offset);
            }

            return true;
        }

        private void HandleKeyword(TSQLStatement stmt, ref int offset)
        {
            if (stmt.Tokens[offset].Text.Equals("SELECT", StringComparison.CurrentCultureIgnoreCase))
            {
                HandleSelect(stmt as TSQLSelectStatement, ref offset);
            }
        }

        private void HandleFunction(TSQLStatement stmt, ref int offset)
        {
            Console.WriteLine("\ttype: function, value: " + stmt.Tokens[offset].Text);

            if (stmt.Tokens[offset].Text == "version")
            {
                if (stmt.Tokens[offset + 1].Text == "(" &&
                    stmt.Tokens[offset + 2].Text == ")")
                {
                    offset += 2;
                    _result = new PgTable
                    {
                        Columns = new()
                        {
                            new PgColumn
                            {
                                Name = "?column?",
                                ColumnIndex = 0,
                                TypeObjectId = PgTypeOIDs.Text,
                                DataTypeSize = -1,
                                FormatCode = PgFormat.Text
                    
                            }
                        },
                        Data = new List<PgDataRow>
                        {
                            new()
                            {
                                ColumnData = new ReadOnlyMemory<byte>?[]
                                {
                                    Encoding.ASCII.GetBytes("0.10-alpga2")
                                }
                            }
                        }
                    };
                }
            }

            // todo: throw here
        }

        private void HandleIdentifier(TSQLStatement stmt, ref int offset)
        {
            // Is function
            if (stmt.Tokens[offset + 1].Text == "(")
            {
                HandleFunction(stmt, ref offset);
                return;
            }

            Console.WriteLine("\ttype: identifier, value: " + stmt.Tokens[offset].Text);
        }

        private void HandleSelect(TSQLSelectStatement stmt, ref int offset)
        {
            Console.WriteLine("SELECT:");

            // Go over select tokens
            for (; offset < stmt.Select.Tokens.Count ; offset++)
            {
                var token = stmt.Select.Tokens[offset];
                switch (stmt.Tokens[offset].Type)
                {
                    case TSQLTokenType.Identifier:
                        HandleIdentifier(stmt, ref offset);
                        break;

                    case TSQLTokenType.SingleLineComment:
                        break;

                    default:
                        Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                        break; // todo: throw
                }
            }

            if (stmt.From != null)
            {
                Console.WriteLine("FROM:");
                for (var i = 0; i < stmt.From.Tokens.Count; i++)
                {
                    var token = stmt.From.Tokens[i];
                    if (token.Type == TSQLTokenType.Keyword &&
                        token.Text.Equals("JOIN", StringComparison.CurrentCultureIgnoreCase) &&
                        stmt.From.Tokens[i - 1].Text.Equals("typ_and_elem_type", StringComparison.CurrentCultureIgnoreCase) &&
                        stmt.From.Tokens[i + 1].Text.Equals("pg_namespace", StringComparison.CurrentCultureIgnoreCase)
                        // todo && _queryString.Contains(..)
                        )
                    {
                        _result = PgConfig.NpgsqlInitialQueryResponse;
                    }

                    Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                }
            }

            if (stmt.Where != null)
            {
                Console.WriteLine("WHERE:");
                foreach (TSQLToken token in stmt.Where.Tokens)
                {
                    Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                }
            }

            if (stmt.GroupBy != null)
            {
                Console.WriteLine("GROUP BY:");
                foreach (TSQLToken token in stmt.GroupBy.Tokens)
                {
                    Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                }
            }

            if (stmt.Having != null)
            {
                Console.WriteLine("HAVING:");
                foreach (TSQLToken token in stmt.Having.Tokens)
                {
                    Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                }
            }

            if (stmt.OrderBy != null)
            {
                Console.WriteLine("ORDER BY:");
                foreach (TSQLToken token in stmt.OrderBy.Tokens)
                {
                    Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                }
            }
        }

        public override async Task<ICollection<PgColumn>> Init()
        {
            if (IsEmptyQuery)
            {
                return default;
            }

            ParseSingleStatement(); // todo: handle error (return value)

            if (_result != null)
            {
                return _result.Columns;
            }

            return Array.Empty<PgColumn>();
        }

        public override async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            if (_result?.Data != null)
            {
                foreach (var dataRow in _result.Data)
                {
                    await writer.WriteAsync(builder.DataRow(dataRow.ColumnData.Span), token);
                }
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {_result?.Data?.Count ?? 0}"), token);
        }

        public override void Dispose()
        {
        }
    }
}