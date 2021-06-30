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
        public void ExecuteStatement(string query)
        {
            var tsqlStatements = TSQLStatementReader.ParseStatements(query);

            foreach (var statement in tsqlStatements)
            {
                if (statement.AsSelect.From == null)
                {
                    var offset = 1;
                    SelectField(statement, ref offset);
                }
            }
        }

        private static void SelectField(TSQLStatement stmt, ref int offset)
        {
            var identifier = stmt.AsSelect.Select.Tokens[offset];
            switch (identifier.Type)
            {
                case TSQLTokenType.Identifier:
                    if (offset + 1 < stmt.AsSelect.Select.Tokens.Count)
                    {
                        if (stmt.AsSelect.Select.Tokens[offset + 1].Text == "(")
                        {
                            offset += 2;
                            var result = methods[identifier.Text](stmt.AsSelect.Select.Tokens, ref offset);
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException(identifier.ToString());
            }
        }

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

        private delegate object SqlMethodDelegate(List<TSQLToken> tokens, ref int offset);

        private static Dictionary<string, SqlMethodDelegate> methods = new(StringComparer.OrdinalIgnoreCase)
        {
            ["version"] = VersionMethod
        };

        private static object VersionMethod(List<TSQLToken> tokens, ref int offset)
        {
            offset++; // todo: validate
            return "0.10-alpga2";
        }

    }
}
