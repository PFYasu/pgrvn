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
            var regexStr = @"(?is)^\s*(?:select\s+(?:\*|(?:(?:(?:""(\$Table|_)""\.)?""(?<columns>[^""]+)""(?:\s+as\s+""[^""]+"")?|replace\(""_"".""(?<columns>(?<replace_columns>[^""]+))"",\s+'(?<replace_inputs>[^']*)',\s+'(?<replace_texts>[^']*)'\)\s+as\s+""[^""]+"")(?:\s|,)*)+)\s+from\s+(?:(?:\((?:\s|,)*)(?<inner_query>.*)\s*\)|""public"".""(?<table_name>.+)""))\s+""(?:\$Table|_)""(?:\s+limit\s+(?<limit>[0-9]+))?\s*$";
            var regex = new Regex(regexStr);
            var match = regex.Match(queryText);

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
            if (innerQuery.Success)
            {
                var rql = innerQuery.Value;

                // Handle double nested SQL
                var innerMatch = regex.Match(rql);
                if (innerMatch.Success)
                {
                    var newInnerQuery = innerMatch.Groups["inner_query"];
                    if (newInnerQuery.Success)
                    {
                        rql = newInnerQuery.Value;
                    }
                }

                var rqlRegexStr = @"^(?is)(?<rql>.*from\s+(?<collection>\S+)(?:\s+as\s+(?<as>\S*))?.*?(?:\s+select\s+(?<select>(?<js_select>{\s*(?<js_select_inside>(?<js_select_field>(?<js_select_field_key>\S+\s*)(?::\s*(?<js_select_field_value>\S+))?\s*,\s*)*(?<js_select_field>(?<js_select_field_key>\S+\s*):\s*(?<js_select_field_value>\S+)\s*))?\s*})|(?<simple_select>((?<simple_select_fields>\S+),\s*)*(?<simple_select_fields>[^\s{}:=]+)))(\s.*)?)?(?:\s+include\s+(?<include>.*))?)$";
                var rqlRegex = new Regex(rqlRegexStr);
                var rqlMatch = rqlRegex.Match(rql);

                if (!rqlMatch.Success)
                {
                    pgQuery = null;
                    return false;
                    //throw new PgErrorException(PgErrorCodes.SyntaxError, "Received an RQL query with unsupported/invalid syntax.");
                }

                // No need to for projection if there's no replace columns
                if (replaceColumns.Success)
                {
                    var collection = rqlMatch.Groups["collection"];

                    // We must have an "as" clause
                    var as_clause = rqlMatch.Groups["as"];
                    var as_value = as_clause.Value;

                    var as_full_value = " as x";
                    var as_index = collection.Index + collection.Length;
                    if (!as_clause.Success)
                    {
                        rql = rql.Insert(as_index, as_full_value);
                        as_value = "x";
                    }

                    var where = rqlMatch.Groups["where"];

                    // Find index in RQL where it's safe to insert the select clause
                    var lastIndexBeforeSelect =
                        (where.Success ? where.Index : (int?)null) ??
                        (as_index + as_full_value.Length);

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
                        newSelect = GenerateProjectionString(columns, replaceColumns, replaceInputs, replaceTexts, as_value);
                    }

                    rql = rql.Insert(lastIndexBeforeSelect, $" {newSelect} ");
                }

                //rql = innerQuery.Value; // todo: delete this line
                Console.WriteLine("RQL: " + rql + "\n");
                pgQuery = new RqlQuery(rql, parametersDataTypes, documentStore, limit.Success ? int.Parse(limit.Value) : null);
                return true;
            }

            pgQuery = null;
            return false;
        }

        private static string GenerateProjectionString(Group columns, Group replaceColumns, Group replaceInputs, Group replaceTexts, string as_value, Group selectFieldValues=null)
        {
            // TODO: if columns.Success == false throw, also for each of the other groups

            if (replaceColumns.Captures.Count != replaceInputs.Captures.Count &&
                replaceColumns.Captures.Count != replaceTexts.Captures.Count &&
                (selectFieldValues != null && replaceColumns.Captures.Count != selectFieldValues.Captures.Count))
            {
                // todo: provide these values in the error message
                throw new PgErrorException(PgErrorCodes.StatementTooComplex, "replace(..) function in SQL statement wasn't provided the expected parameters.");
            }
            
            // todo: temp solution
            var replaceColumnsSet = new Dictionary<string, (string, string)>();

            for (int i = 0; i < replaceColumns.Captures.Count; i++)
            {
                replaceColumnsSet.Add(replaceColumns.Captures[i].Value, (replaceInputs.Captures[i].Value, replaceTexts.Captures[i].Value));
            }

            var projection = "select { ";
            for (int i = 0; i < columns.Captures.Count; i++)
            {
                var columnName = columns.Captures[i].Value;

                if (selectFieldValues != null)
                {
                    projection += $"\"{columnName}\": ({selectFieldValues.Captures[i].Value})";
                }
                else if (columnName.ToLower() == "id()")
                {
                    // Nothing we can do, this is automatically generated on the Execute stage
                    continue;
                    //projection += $"\"{columnName}\": {as_value}[\"@metadata\"][\"@id\"]";
                }
                else if (columnName.ToLower() == "json()")
                {
                    // Nothing we can do, this is automatically generated on the Execute stage
                    continue;
                }
                else
                {
                    projection += $"\"{columnName}\": {as_value}.{columnName}";
                }

                if (replaceColumnsSet.TryGetValue(columnName, out var replacement))
                {
                    projection += $".toString().replace(\"{replacement.Item1}\", \"{replacement.Item2}\")";
                }

                projection += ", ";
            }

            projection = projection[0..^2];
            projection += " }";

            return projection;
        }
    }
}
