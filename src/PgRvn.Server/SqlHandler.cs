using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSQL;
using TSQL.Statements;
using TSQL.Tokens;

namespace PgRvn.Server
{
    class SqlHandler
    {
        public TSQLStatement ParseSingleStatement(string query)
        {
            var sqlStatements = TSQLStatementReader.ParseStatements(query);
            switch (sqlStatements.Count)
            {
                case 0:
                    throw new InvalidOperationException("Expected at least one statement in query, got zero.");
                case 1:
                    break;
                default:
                    throw new InvalidOperationException("Didn't expect more than one SQL statement in query, got: " + sqlStatements.Count);
            }

            return sqlStatements[0];
        }

        public PgColumnData[] Execute(TSQLStatement statement, out int rowsOperated)
        {
            //if (statement.AsSelect.From == null)
            int offset = 1;
            return SelectField(statement, ref offset, out var _, out rowsOperated);
        }

        public PgColumn[] Describe(TSQLStatement statement)
        {
            int offset = 1;
            SelectField(statement, ref offset, out var columns, out _);
            return columns;
        }

        private static PgColumnData[] SelectField(TSQLStatement stmt, ref int offset, out PgColumn[] columns, out int rowsOperated)
        {
            columns = Array.Empty<PgColumn>();

            var identifier = stmt.AsSelect.Select.Tokens[offset];
            switch (identifier.Type)
            {
                case TSQLTokenType.Identifier:
                    if (offset + 1 < stmt.AsSelect.Select.Tokens.Count)
                    {
                        if (stmt.AsSelect.Select.Tokens[offset + 1].Text == "(")
                        {
                            offset += 2;
                            var result = methods[identifier.Text](stmt.AsSelect.Select.Tokens, ref offset, out var outColumns);
                            columns = outColumns;
                            rowsOperated = 1;
                            return new []
                            {
                                new PgColumnData
                                {
                                    Data = result
                                }
                            };
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException(identifier.ToString());
            }

            rowsOperated = 0;
            return Array.Empty<PgColumnData>();
        }

        private delegate ReadOnlyMemory<byte> SqlMethodDelegate(List<TSQLToken> tokens, ref int offset, out PgColumn[] columns);

        private static readonly Dictionary<string, SqlMethodDelegate> methods = new(StringComparer.OrdinalIgnoreCase)
        {
            ["version"] = VersionMethod
        };

        private static ReadOnlyMemory<byte> VersionMethod(List<TSQLToken> tokens, ref int offset, out PgColumn[] columns)
        {
            if (tokens[offset].Text != ")")
                throw new InvalidOperationException("version() expects no arguments.");

            columns = new[]
            {
                new PgColumn
                {
                    Name = "version",
                    TableObjectId = 0,
                    ColumnIndex = 0,
                    TypeObjectId = PgTypeOIDs.Text,
                    DataTypeSize = -1, // or -2 if null terminated
                    TypeModifier = -1,
                    FormatCode = PgFormat.Text
                }
            };

            offset++;
            return Encoding.ASCII.GetBytes("0.10-alpga2");
        }
    }
}
