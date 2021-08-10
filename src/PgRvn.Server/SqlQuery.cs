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

        // TODO: Make this the only constructor, add a TryParse method and rename this class - HardcodedQuery ? Maybe not.
        public SqlQuery(string queryString, int[] parametersDataTypes, PgTable results) : base(queryString, parametersDataTypes)
        {
            _result = results;
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

        public bool Parse(bool allowMultipleStatements)
        {
            // TODO: Make this better, shouldn't have seperate logic for previously given result..
            if (_result != null)
            {
                return true;
            }

            var sqlStatements = TSQLStatementReader.ParseStatements(QueryString);
            if (allowMultipleStatements == false && sqlStatements.Count != 1)
            {
                throw new InvalidOperationException("Didn't expect more than one SQL statement in queryString, got: " + sqlStatements.Count);
            }

            foreach (var stmt in sqlStatements)
            {
                var powerBIMatch = PowerBIConfig.TypesQuery;
                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PowerBIConfig.TypesResponse;
                    return true;
                }

                powerBIMatch = PowerBIConfig.CompositeTypesQuery;

                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PowerBIConfig.CompositeTypesResponse;
                    return true;
                }

                powerBIMatch = PowerBIConfig.EnumTypesQuery;

                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PowerBIConfig.EnumTypesResponse;
                    return true;
                }

                powerBIMatch = PowerBIConfig.TableSchemaQuery;
                if (QueryString.StartsWith(powerBIMatch))
                {
                    _result = PowerBIConfig.TableSchemaResponse;
                }

                powerBIMatch = PowerBIConfig.TableSchemaSecondaryQuery;
                if (QueryString.StartsWith(powerBIMatch))
                {
                    _result = PowerBIConfig.TableSchemaSecondaryResponse;
                }

                powerBIMatch = PowerBIConfig.ConstraintsQuery;
                if (QueryString.StartsWith(powerBIMatch))
                {
                    _result = PowerBIConfig.ConstraintsResponse;
                }

                var resultsFormat = GetDefaultResultsFormat();

                powerBIMatch = "select character_set_name from INFORMATION_SCHEMA.character_sets";
                if (QueryString.Equals(powerBIMatch))
                {
                    _result = new PgTable
                    {
                        Columns = new List<PgColumn>
                        {
                            new()
                            {
                                Name = "character_set_name",
                                ColumnIndex = 0,
                                TypeObjectId = PgTypeOIDs.Name,
                                DataTypeSize = -1,
                                FormatCode = resultsFormat
                            },
                        },
                        Data = new List<PgDataRow>
                        {
                            new()
                            {
                                ColumnData = new ReadOnlyMemory<byte>?[]
                                {
                                    Encoding.UTF8.GetBytes("UTF8"),
                                }
                            },
                        }
                    };
                    return true;
                }

                if (stmt.AsSelect != null)
                {
                    int offset = stmt.AsSelect.BeginPosition;
                    HandleSelect(stmt.AsSelect, ref offset);
                }
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
            //Console.WriteLine("\ttype: function, value: " + stmt.Tokens[offset].Text);

            // todo: handle this better (e.g. check that the query is actually select version() or smth)
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
                                Name = "version",
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
                                    Encoding.ASCII.GetBytes("PostgreSQL 13.3, compiled by Visual C++ build 1914, 64-bit")
                                }
                            }
                        }
                    };
                }
            }
            // else if (stmt.Tokens[offset].Text == "set_config" && 
            //          stmt.Tokens[offset - 1].Text.Equals("SELECT", StringComparison.CurrentCultureIgnoreCase))
            // {
            //     // for pgAdmin, it's not actually required, can remove
            //     _result = new PgTable
            //     {
            //         Columns = new()
            //         {
            //             new PgColumn
            //             {
            //                 Name = "set_config",
            //                 ColumnIndex = 0,
            //                 TypeObjectId = PgTypeOIDs.Text,
            //                 DataTypeSize = -1,
            //                 FormatCode = PgFormat.Text
            //
            //             }
            //         },
            //         Data = new List<PgDataRow>
            //         {
            //             new()
            //             {
            //                 ColumnData = new ReadOnlyMemory<byte>?[]
            //                 {
            //                     Encoding.ASCII.GetBytes("hex")
            //                 }
            //             }
            //         }
            //     };
            // }

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

            //Console.WriteLine("\ttype: identifier, value: " + stmt.Tokens[offset].Text);
        }

        private void HandleSelect(TSQLSelectStatement stmt, ref int offset)
        {
            //Console.WriteLine("SELECT:");

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
                        //Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                        break; // todo: throw
                }
            }

            if (stmt.From != null)
            {
                //Console.WriteLine("FROM:");
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

                    //Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
                }
            }

            //if (stmt.Where != null)
            //{
            //    Console.WriteLine("WHERE:");
            //    foreach (TSQLToken token in stmt.Where.Tokens)
            //    {
            //        Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
            //    }
            //}

            //if (stmt.GroupBy != null)
            //{
            //    Console.WriteLine("GROUP BY:");
            //    foreach (TSQLToken token in stmt.GroupBy.Tokens)
            //    {
            //        Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
            //    }
            //}

            //if (stmt.Having != null)
            //{
            //    Console.WriteLine("HAVING:");
            //    foreach (TSQLToken token in stmt.Having.Tokens)
            //    {
            //        Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
            //    }
            //}

            //if (stmt.OrderBy != null)
            //{
            //    Console.WriteLine("ORDER BY:");
            //    foreach (TSQLToken token in stmt.OrderBy.Tokens)
            //    {
            //        Console.WriteLine("\ttype: " + token.Type.ToString() + ", value: " + token.Text);
            //    }
            //}
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements)
        {
            if (IsEmptyQuery)
            {
                return default;
            }

            Parse(allowMultipleStatements); // todo: handle error (return value)

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