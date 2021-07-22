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

        public bool Parse(bool allowMultipleStatements)
        {
            var sqlStatements = TSQLStatementReader.ParseStatements(QueryString);
            if (allowMultipleStatements == false && sqlStatements.Count != 1)
            {
                throw new InvalidOperationException("Didn't expect more than one SQL statement in queryString, got: " + sqlStatements.Count);
            }

            foreach (var stmt in sqlStatements)
            {
                Console.WriteLine("\n" + QueryString + "\n");

                //var powerBIMatch = "\n/*** Load all supported types ***/\nSELECT ns.nspname, a.typname, a.oid, a.typrelid, a.typbasetype,\nCASE WHEN pg_proc.proname='array_recv' THEN "; //'a' ELSE a.typtype END AS type,\nCASE\n  WHEN pg_proc.proname='array_recv' THEN a.typelem\n  WHEN a.typtype='r' THEN rngsubtype\n  ELSE 0\nEND AS elemoid,\nCASE\n  WHEN pg_proc.proname IN ('array_recv','oidvectorrecv') THEN 3    /* Arrays last */\n  WHEN a.typtype='r' THEN 2                                        /* Ranges before */\n  WHEN a.typtype='d' THEN 1                                        /* Domains before */\n  ELSE 0                                                           /* Base types first */\nEND AS ord\nFROM pg_type AS a\nJOIN pg_namespace AS ns ON (ns.oid = a.typnamespace)\nJOIN pg_proc ON pg_proc.oid = a.typreceive\nLEFT OUTER JOIN pg_class AS cls ON (cls.oid = a.typrelid)\nLEFT OUTER JOIN pg_type AS b ON (b.oid = a.typelem)\nLEFT OUTER JOIN pg_class AS elemcls ON (elemcls.oid = b.typrelid)\nLEFT OUTER JOIN pg_range ON (pg_range.rngtypid = a.oid) \nWHERE\n  a.typtype IN ('b', 'r', 'e', 'd') OR         /* Base, range, enum, domain */\n  (a.typtype = 'c' AND cls.relkind='c') OR /* User-defined free-standing composites (not table composites) by default */\n  (pg_proc.proname='array_recv' AND (\n    b.typtype IN ('b', 'r', 'e', 'd') OR       /* Array of base, range, enum, domain */\n    (b.typtype = 'p' AND b.typname IN ('record', 'void')) OR /* Arrays of special supported pseudo-types */\n    (b.typtype = 'c' AND elemcls.relkind='c')  /* Array of user-defined free-standing composites (not table composites) */\n  )) OR\n  (a.typtype = 'p' AND a.typname IN ('record', 'void'))  /* Some special supported pseudo-types */\nORDER BY ord
                var powerBIMatch = "\n/*** Load all supported types ***/\nSELECT ns.nspname, a.typname, a.oid, a.typrelid, a.typbasetype,\nCASE WHEN pg_proc.proname='array_recv' THEN 'a' ELSE a.typtype END AS type,\nCASE\n  WHEN pg_proc.proname='array_recv' THEN a.typelem\n  WHEN a.typtype='r' THEN rngsubtype\n  ELSE 0\nEND AS elemoid,\nCASE\n  WHEN pg_proc.proname IN ('array_recv','oidvectorrecv') THEN 3    /* Arrays last */\n  WHEN a.typtype='r' THEN 2                                        /* Ranges before */\n  WHEN a.typtype='d' THEN 1                                        /* Domains before */\n  ELSE 0                                                           /* Base types first */\nEND AS ord\nFROM pg_type AS a\nJOIN pg_namespace AS ns ON (ns.oid = a.typnamespace)\nJOIN pg_proc ON pg_proc.oid = a.typreceive\nLEFT OUTER JOIN pg_class AS cls ON (cls.oid = a.typrelid)\nLEFT OUTER JOIN pg_type AS b ON (b.oid = a.typelem)\nLEFT OUTER JOIN pg_class AS elemcls ON (elemcls.oid = b.typrelid)\nLEFT OUTER JOIN pg_range ON (pg_range.rngtypid = a.oid) \nWHERE\n  a.typtype IN ('b', 'r', 'e', 'd') OR         /* Base, range, enum, domain */\n  (a.typtype = 'c' AND cls.relkind='c') OR /* User-defined free-standing composites (not table composites) by default */\n  (pg_proc.proname='array_recv' AND (\n    b.typtype IN ('b', 'r', 'e', 'd') OR       /* Array of base, range, enum, domain */\n    (b.typtype = 'p' AND b.typname IN ('record', 'void')) OR /* Arrays of special supported pseudo-types */\n    (b.typtype = 'c' AND elemcls.relkind='c')  /* Array of user-defined free-standing composites (not table composites) */\n  )) OR\n  (a.typtype = 'p' AND a.typname IN ('record', 'void'))  /* Some special supported pseudo-types */\nORDER BY ord";
                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PgConfig.PowerBIInitialQueryResponse;
                    return true;
                }

                powerBIMatch = "/*** Load field definitions for (free-standing) composite types ***/\nSELECT typ.oid, att.attname, att.atttypid\nFROM pg_type AS typ\nJOIN pg_namespace AS ns ON (ns.oid = typ.typnamespace)\nJOIN pg_class AS cls ON (cls.oid = typ.typrelid)\nJOIN pg_attribute AS att ON (att.attrelid = typ.typrelid)\nWHERE\n  (typ.typtype = 'c' AND cls.relkind='c') AND\n  attnum > 0 AND     /* Don't load system attributes */\n  NOT attisdropped\nORDER BY typ.oid, att.attnum";

                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PgConfig.PowerBICompositeTypes;
                    return true;
                }

                powerBIMatch = "/*** Load enum fields ***/\nSELECT pg_type.oid, enumlabel\nFROM pg_enum\nJOIN pg_type ON pg_type.oid=enumtypid\nORDER BY oid, enumsortorder";

                if (QueryString.Equals(powerBIMatch))
                {
                    _result = PgConfig.PowerBIEnumTypes;
                    return true;
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