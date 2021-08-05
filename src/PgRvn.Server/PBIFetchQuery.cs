using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public static class PBIFetchQuery
    {
        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out PgQuery pgQuery)
        {
            // Match queries sent by PowerBI, either RQL queries wrapped in an SQL statement OR generic SQL queries
            var regexStr = @"(?is)^\s*(?:select\s+(?:\*|(?:(?:""(\$Table|_)""\.""(?<columns>[^""]+)""\s+as\s+""[^""]+""|replace\(""_"".""(?<replace_columns>[^""]+)"",\s+'(?<replace_inputs>[^']+)',\s+'(?<replace_texts>[^']+)'\)\s+as\s+""[^""]+"")(?:\s|,)*)+)\s+from\s+(?:(?:\((?:\s|,)*)(?<inner_query>.*)\s*\)|""public"".""(?<table_name>.+)""))\s+""(?:\$Table|_)""(?:\s+limit\s+(?<limit>[0-9]+))?\s*$";
            var match = new Regex(regexStr).Match(queryText);

            if (!match.Success)
            {
                pgQuery = null;
                return false;
            }

            var limit = match.Groups["limit"];
            var tableName = match.Groups["table_name"];
            var innerQuery = match.Groups["inner_query"];
            var columns = match.Groups["columns"];
            var replaceColumns = match.Groups["replace_columns"];
            var replaceInputs = match.Groups["replace_inputs"];
            var replaceTexts = match.Groups["replace_texts"];

            // Handle generic query
            if (tableName.Success)
            {
                // Note: We assume PowerBI never modifies the initial query for generic queries (e.g. no replace, filtering..)
                // TODO: Provide these as parameters to prevent SQL injection (depends on RavenDB-17075)
                var rql = $"from {tableName.Value}";

                if (columns.Success)
                {
                    rql += " select ";
                    for (int i = 0; i < columns.Captures.Count; i++)
                    {
                        rql += columns.Captures[i];

                        if (i != columns.Captures.Count - 1)
                            rql += ", ";
                    }
                }

                if (limit.Success)
                    rql += $" limit {limit.Value}";

                pgQuery = new RqlQuery(rql, parametersDataTypes, documentStore);
                return true;
            }
            
            // Handle RQL query
            if (!innerQuery.Success)
            {
                var rql = innerQuery.Value;
                var rqlRegexStr = @"^(?is)(?<rql>.*from\s+(?<collection>\S+)(?:\s+as\s+(?<as>\S*))?.*?(?:\s+select\s+(?<select>(?<js_select>{\s*(?<js_select_inside>(?<js_select_field>(?<js_select_field_key>\S+\s*)(?::\s*(?<js_select_field_value>\S+))?\s*,\s*)*(?<js_select_field>(?<js_select_field_key>\S+\s*):\s*(?<js_select_field_value>\S+)\s*))?\s*})|(?<simple_select>((?<simple_select_fields>\S+),\s*)*(?<simple_select_fields>[^\s{}:=]+)))(\s.*)?)?)$";
                var rqlMatch = new Regex(rqlRegexStr).Match(rql);

                if (!rqlMatch.Success)
                {
                    throw new PgErrorException(PgErrorCodes.SyntaxError, "Received an RQL query with unsupported/invalid syntax.");
                }

                // No need to for projection if there's no replace columns
                if (replaceColumns.Success)
                {
                    var newSelect = "";
                    var select = match.Groups["select"];

                    if (select.Success)
                    {
                        //var jsSelect = match.Groups["js_select"];
                        //var simpleSelect = match.Groups["simple_select"];
                        //var simpleSelectFields = match.Groups["simple_select_fields"];

                        //if (jsSelect.Success)
                        //{

                        //}
                        //else if (simpleSelect.Success && simpleSelectFields.Success)
                        //{
                        //    newSelect = "select ";

                        //    for (int i = 0; i < simpleSelectFields.Captures.Count; i++)
                        //    {
                        //        newSelect += simpleSelectFields.Captures[i];

                        //        if (i != simpleSelectFields.Captures.Count - 1)
                        //        {
                        //            newSelect += ", ";
                        //        }
                        //    }

                        //    var sqlWithNewRql = queryText.Replace(regex, "simpleSelect", newSelect);
                        //}
                    }
                    else
                    {
                        var as_clause = rqlMatch.Groups["as"];
                        newSelect = GenerateProjectionString(columns, replaceColumns, replaceInputs, replaceTexts, as_clause);
                    }
                }

                pgQuery = new RqlQuery(rqlMatch.Value, parametersDataTypes, documentStore, limit.Success ? int.Parse(limit.Value) : null);
                return true;
            }

            pgQuery = null;
            return false;
        }

        private static string GenerateProjectionString(Group columns, Group replaceColumns, Group replaceInputs, Group replaceTexts, Group as_clause)
        {
            // TODO: if columns.Success == false throw, also for each of the other groups

            if (replaceColumns.Captures.Count != replaceInputs.Captures.Count &&
                replaceColumns.Captures.Count != replaceTexts.Captures.Count)
            {
                throw new PgErrorException(PgErrorCodes.StatementTooComplex, "replace(..) function in SQL statement wasn't provided the expected parameters.");
            }

            var replacements = new Dictionary<string, (string, string)>();
            for (int i = 0; i < replaceColumns.Captures.Count; i++)
            {
                replacements.Add(
                    replaceColumns.Captures[i].Value, 
                    (replaceInputs.Captures[i].Value, replaceTexts.Captures[i].Value));
            }

            var projection = "select { ";
            foreach (var replacement in replacements)
            {
                projection += $"{replacement.Key}: {as_clause.Value}.{replacement.Key}" +
                    $".replace({replacement.Value.Item1}, {replacement.Value.Item1})";
            }

            return projection;
        }
    }
}
