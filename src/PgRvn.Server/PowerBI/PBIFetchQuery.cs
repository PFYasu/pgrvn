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
        private static readonly Regex _regex = new(@"(?is)^\s*(?:select\s+(?:\*|(?:(?:(?:""(\$Table|_)""\.)?""[^""]+""(?:\s+as\s+""(?<columns>[^""]+)"")?|replace\(""_"".""(?<replace_source_columns>[^""]+)"",\s+'(?<replace_inputs>[^']*)',\s+'(?<replace_texts>[^']*)'\)\s+as\s+""(?<columns>(?<replace_dest_columns>[^""]+))"")(?:\s|,)*)+)\s+from\s+(?:(?:\((?:\s|,)*)(?<inner_query>.*)\s*\)|""public"".""(?<table_name>.+)""))\s+""(?:\$Table|_)""(?:\s+limit\s+(?<limit>[0-9]+))?\s*$",
            RegexOptions.Compiled);
        private static readonly Regex _rqlRegex = new(@"^(?is)(?<rql>.*from\s+(?<collection>\S+)(?:\s+as\s+(?<alias>\S*))?.*?(?:\s+select\s+(?<select>(?<js_select>{\s*(?<js_select_inside>(?<js_select_field>(?<js_select_field_key>\S+\s*)(?::\s*(?<js_select_field_value>\S+))?\s*,\s*)*(?<js_select_field>(?<js_select_field_key>\S+\s*):\s*(?<js_select_field_value>\S+)\s*))?\s*})|(?<simple_select>((?<simple_select_fields>\S+),\s*)*(?<simple_select_fields>[^\s{}:=]+)))(\s.*)?)?(?:\s+include\s+(?<include>.*))?)$",
            RegexOptions.Compiled);

        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out PgQuery pgQuery)
        {
            // Match queries sent by PowerBI, either RQL queries wrapped in an SQL statement OR generic SQL queries
            var match = _regex.Match(queryText);

            if (!match.Success)
            {
                pgQuery = null;
                return false;
            }

            var limit = match.Groups["limit"];
            var tableName = match.Groups["table_name"];
            var innerQuery = match.Groups["inner_query"];
            var columns = match.Groups["columns"];
            var replaceSourceColumns = match.Groups["replace_source_columns"];
            var replaceDestColumns = match.Groups["replace_dest_columns"];
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
                var innerMatch = _regex.Match(rql);
                if (innerMatch.Success)
                {
                    var newInnerQuery = innerMatch.Groups["inner_query"];
                    if (newInnerQuery.Success)
                    {
                        rql = newInnerQuery.Value;
                    }
                }

                var rqlMatch = _rqlRegex.Match(rql);

                if (!rqlMatch.Success)
                {
                    pgQuery = null;
                    return false;
                    //throw new PgErrorException(PgErrorCodes.SyntaxError, "Received an RQL query with unsupported/invalid syntax.");
                }

                // No need to for projection if there's no replace columns
                if (replaceSourceColumns.Success)
                {
                    var collection = rqlMatch.Groups["collection"];

                    // We must have an "as" clause
                    var alias = rqlMatch.Groups["alias"];
                    var aliasVal = alias.Value;

                    var alias_full_value = " as x";
                    var alias_index = collection.Index + collection.Length;
                    if (!alias.Success)
                    {
                        rql = rql.Insert(alias_index, alias_full_value);
                        aliasVal = "x";
                    }

                    var where = rqlMatch.Groups["where"];

                    // Find index in RQL where it's safe to insert the select clause
                    var lastIndexBeforeSelect =
                        (where.Success ? where.Index : (int?)null) ??
                        (alias_index + alias_full_value.Length);

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
                        newSelect = GenerateProjectionString(columns, replaceSourceColumns, replaceDestColumns, replaceInputs, replaceTexts, aliasVal);
                    }

                    rql = rql.Insert(lastIndexBeforeSelect, $" {newSelect} ");
                }

                Console.WriteLine("RQL: " + rql + "\nLIMIT: " + limit.Value + "\n");
                pgQuery = new RqlQuery(rql, parametersDataTypes, documentStore, limit.Success ? int.Parse(limit.Value) : null);
                return true;
            }

            pgQuery = null;
            return false;
        }

        private static string GenerateProjectionString(Group columns, Group replaceSourceColumns, Group replaceDestColumns, Group replaceInputs, Group replaceTexts, string alias, Group selectFieldValues=null)
        {
            // TODO: if columns.Success == false throw, also for each of the other groups

            if (replaceSourceColumns.Captures.Count != replaceInputs.Captures.Count &&
                replaceSourceColumns.Captures.Count != replaceTexts.Captures.Count &&
                replaceSourceColumns.Captures.Count != replaceDestColumns.Captures.Count &&
                (selectFieldValues != null && replaceSourceColumns.Captures.Count != selectFieldValues.Captures.Count))
            {
                // todo: provide these values and the SQL in the error message
                throw new PgErrorException(PgErrorCodes.StatementTooComplex, "replace(..) function in SQL statement wasn't provided the expected parameters.");
            }
            
            var replaceDestColumnsSet = new Dictionary<string, (string, string, string)>();
            for (int i = 0; i < replaceDestColumns.Captures.Count; i++)
            {
                replaceDestColumnsSet.Add(replaceDestColumns.Captures[i].Value, 
                    (replaceSourceColumns.Captures[i].Value, replaceInputs.Captures[i].Value, replaceTexts.Captures[i].Value));
            }

            var projection = "select { ";
            for (int i = 0; i < columns.Captures.Count; i++)
            {
                var columnName = columns.Captures[i].Value;

                // We have to project these because otherwise the order of the fields gets mixed up
                if (columnName.Equals("id()", StringComparison.OrdinalIgnoreCase) ||
                    columnName.Equals("json()", StringComparison.OrdinalIgnoreCase)) 
                {
                    projection += $"\"{columnName}\": \"\"";
                    continue; 
                }

                if (replaceDestColumnsSet.TryGetValue(columnName, out var value))
                {
                    var (sourceColumn, replaceInput, replaceText) = value;
                    projection += $"\"{columnName}\": {alias}.{sourceColumn}.toString().replace(\"{replaceInput}\", \"{replaceText}\")";
                }
                //else if (selectFieldValues != null) // TODO: Support this, i don't think it works as it is
                //{
                //    projection += $"\"{columnName}\": ({selectFieldValues.Captures[i].Value})";
                //}
                else
                {
                    projection += $"\"{columnName}\": {alias}.{columnName}";
                }

                projection += ", ";
            }

            projection = projection[0..^2];
            projection += " }";

            return projection;
        }
    }
}
