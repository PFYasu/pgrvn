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
        private static readonly Regex _regex = new(@"(?is)^\s*(?:select\s+(?:\*|(?:(?:(?:""(\$Table|_)""\.)?""[^""]+""(?:\s+as\s+""(?<columns>[^""]+)"")?|replace\(""_"".""(?<replace_source_columns>[^""]+)"",\s+'(?<replace_inputs>[^']*)',\s+'(?<replace_texts>[^']*)'\)\s+as\s+""(?<replace_dest_columns>[^""]+)"")(?:\s|,)*)+)\s+from\s+(?:(?:\((?:\s|,)*)(?<inner_query>.*)\s*\)|""public"".""(?<table_name>.+)""))\s+""(?:\$Table|_)""(?:\s+limit\s+(?<limit>[0-9]+))?\s*$",
            RegexOptions.Compiled);
        private static readonly Regex _rqlRegex = new(@"^(?is)(?<rql>\s*(?:/\*rql\*/\s*)?from\s+(?<collection>[^\s\(\)]+)(?:\s+as\s+(?<alias>\S*))?.*?(?:\s+select\s+(?<select>(?<js_select>{\s*(?<js_select_inside>(?<js_select_field>(?<js_select_field_key>\S+\s*)(?::\s*(?<js_select_field_value>\S+))?\s*,\s*)*(?<js_select_field>(?<js_select_field_key>\S+\s*):\s*(?<js_select_field_value>\S+)\s*))?\s*})|(?<simple_select>((?<simple_select_fields>\S+),\s*)*(?<simple_select_fields>[^\s{}:=]+)))(\s.*)?)?(?:\s+include\s+(?<include>.*))?)$",
            RegexOptions.Compiled);


        private static string GetProjectionField(Capture column)
        {
            return "";
        }

        private static void PopulateReplaceFields(Dictionary<string, string> projectionFields, Match match, string alias)
        {
            Group replaceDestColumns = match.Groups["replace_dest_columns"];
            Group replaceSourceColumns = match.Groups["replace_source_columns"];
            Group replaceInputs = match.Groups["replace_inputs"];
            Group replaceTexts = match.Groups["replace_texts"];

            for (int i = 0; i < replaceDestColumns.Captures.Count; i++)
            {
                var destColumnName = replaceDestColumns.Captures[i].Value;
                var srcColumnName = replaceSourceColumns.Captures[i].Value;
                var replaceInput = replaceInputs.Captures[i].Value;
                var replaceText = replaceTexts.Captures[i].Value;

                var fieldValueStart = $"{alias}[\"{srcColumnName}\"]";
                if (!srcColumnName.Equals(destColumnName) && projectionFields.TryGetValue(srcColumnName, out string val))
                {
                    fieldValueStart = $"({val})";
                }

                projectionFields.TryAdd(destColumnName, $"{fieldValueStart}.toString().replace(\"{replaceInput}\", \"{replaceText}\")");
            }
        }

        private static bool TryGetMatches(string queryText, out List<Match> outMatches, out Match rql)
        {
            var matches = new List<Match>();
            var queryToMatch = queryText;
            Group innerQuery;

            rql = null;

            // Queries can have inner queries that we need to parse, so here we collect those
            do
            {
                var match = _regex.Match(queryToMatch);

                if (!match.Success)
                {
                    outMatches = null;
                    return false;
                }

                matches.Add(match);

                innerQuery = match.Groups["inner_query"];
                queryToMatch = match.Groups["inner_query"].Value;
            } while (innerQuery.Success && !IsRql(queryToMatch, out rql));

            outMatches = matches;
            return true;
        }

        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out PgQuery pgQuery)
        {
            // Match queries sent by PowerBI, either RQL queries wrapped in an SQL statement OR generic SQL queries
            if (!TryGetMatches(queryText, out var matches, out Match rql))
            {
                pgQuery = null;
                return false;
            }

            if (rql.Success)
            {
                // Handle RQL type query
                var alias = rql.Groups["alias"].Success ? rql.Groups["alias"].Value : "x";

                var projectionFields = new Dictionary<string, string>();

                // TODO: Populate another dictionary with the existing RQL select fields
                var simpleSelectFields = rql.Groups["simple_select_fields"];

                // Populate the columns starting from the inner-most SQL
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match match = matches[i];
                    var columns = match.Groups["columns"].Captures;
                    foreach (Capture column in columns)
                    {
                        if (column.Value.Equals("id()", StringComparison.OrdinalIgnoreCase) ||
                            column.Value.Equals("json()", StringComparison.OrdinalIgnoreCase))
                        {
                            projectionFields.TryAdd(column.Value, "\"test\"");
                            continue;
                        }

                        projectionFields.TryAdd(column.Value, $"{alias}[\"{column.Value}\"]");
                    }

                    PopulateReplaceFields(projectionFields, match, alias);
                }

                var newRql = GenerateProjectedRql(rql, projectionFields);

                // TODO: Add limit

                pgQuery = new RqlQuery(newRql, parametersDataTypes, documentStore);
                return true;
            }
            else if (matches[0].Groups["table_name"].Success)
            {
                // Handle generic type query
                // TODO: handle this
            }


            //var limit = match.Groups["limit"];
            //var tableName = match.Groups["table_name"];
            //var columns = match.Groups["columns"];
            //var replaceSourceColumns = match.Groups["replace_source_columns"];
            //var replaceDestColumns = match.Groups["replace_dest_columns"];
            //var replaceInputs = match.Groups["replace_inputs"];
            //var replaceTexts = match.Groups["replace_texts"];

            //// Handle generic query
            //if (tableName.Success)
            //{
            //    // Note: We assume PowerBI never modifies the initial query for generic queries (e.g. no replace, filtering..)
            //    // TODO: Provide these as parameters to prevent SQL injection (depends on RavenDB-17075)
            //    var rql = $"from {tableName.Value}";

            //    if (columns.Success)
            //    {
            //        rql += " select ";
            //        for (int i = 0; i < columns.Captures.Count; i++)
            //        {
            //            rql += columns.Captures[i];

            //            if (i != columns.Captures.Count - 1)
            //                rql += ", ";
            //        }
            //    }

            //    if (limit.Success)
            //        rql += $" limit {limit.Value}";

            //    pgQuery = new RqlQuery(rql, parametersDataTypes, documentStore);
            //    return true;
            //}

            //// Handle RQL query
            //if (innerQuery.Success)
            //{
            //    var rql = innerQuery.Value;

            //    var rqlMatch = _rqlRegex.Match(rql);

            //    if (!rqlMatch.Success)
            //    {
            //        pgQuery = null;
            //        return false;
            //        //throw new PgErrorException(PgErrorCodes.SyntaxError, "Received an RQL query with unsupported/invalid syntax.");
            //    }

            //    // No need to for projection if there's no replace columns
            //    if (replaceSourceColumns.Success)
            //    {
            //        var collection = rqlMatch.Groups["collection"];

            //        // We must have an "as" clause
            //        var alias = rqlMatch.Groups["alias"];
            //        var aliasVal = alias.Value;

            //        var alias_full_value = " as x";
            //        var alias_index = collection.Index + collection.Length;
            //        if (!alias.Success)
            //        {
            //            rql = rql.Insert(alias_index, alias_full_value);
            //            aliasVal = "x";
            //        }

            //        var where = rqlMatch.Groups["where"];

            //        // Find index in RQL where it's safe to insert the select clause
            //        var lastIndexBeforeSelect =
            //            (where.Success ? where.Index : (int?)null) ??
            //            (alias_index + alias_full_value.Length);

            //        var newSelect = "";
            //        var select = match.Groups["select"];

            //        if (select.Success)
            //        {
            //            //var jsSelect = match.Groups["js_select"];
            //            //var simpleSelect = match.Groups["simple_select"];
            //            //var simpleSelectFields = match.Groups["simple_select_fields"];

            //            //if (jsSelect.Success)
            //            //{

            //            //}
            //            //else if (simpleSelect.Success && simpleSelectFields.Success)
            //            //{
            //            //    newSelect = "select ";

            //            //    for (int i = 0; i < simpleSelectFields.Captures.Count; i++)
            //            //    {
            //            //        newSelect += simpleSelectFields.Captures[i];

            //            //        if (i != simpleSelectFields.Captures.Count - 1)
            //            //        {
            //            //            newSelect += ", ";
            //            //        }
            //            //    }

            //            //    var sqlWithNewRql = queryText.Replace(regex, "simpleSelect", newSelect);
            //            //}
            //        }
            //        else
            //        {
            //            newSelect = GenerateProjectionString(columns, replaceSourceColumns, replaceDestColumns, replaceInputs, replaceTexts, aliasVal);
            //        }

            //        rql = rql.Insert(lastIndexBeforeSelect, $" {newSelect} ");
            //    }

            //    Console.WriteLine("RQL: " + rql + "\nLIMIT: " + limit.Value + "\n");
            //    pgQuery = new RqlQuery(rql, parametersDataTypes, documentStore, limit.Success ? int.Parse(limit.Value) : null);
            //    return true;
            //}

            pgQuery = null;
            return false;
        }

        private static string GenerateProjectedRql(Match rqlMatch, Dictionary<string, string> projectionFields)
        {
            string rql = rqlMatch.Value;

            var collection = rqlMatch.Groups["collection"];
            var alias = rqlMatch.Groups["alias"];
            var where = rqlMatch.Groups["where"];

            // Insert an alias if doesn't exist
            string newAliasClause = " as x";
            if (!alias.Success)
            {
                rql = rql.Insert(collection.Index + collection.Length, newAliasClause);
            }

            // Remove existing select clauses
            int? selectIndex = null;
            var simpleSelect = rqlMatch.Groups["simple_select"];
            if (simpleSelect.Success)
            {
                rql = rql.Remove(simpleSelect.Index, simpleSelect.Length);
                selectIndex = simpleSelect.Index;
            }

            var jsSelect = rqlMatch.Groups["js_select"];
            if (jsSelect.Success)
            {
                rql = rql.Remove(jsSelect.Index, jsSelect.Length);
                selectIndex = jsSelect.Index;
            }

            // Generate new select clause
            if (projectionFields.Count != 0)
            {
                // Find select start index
                if (selectIndex == null)
                {
                    // Find index where it's safe to insert the select clause
                    var aliasIndexEnd = alias.Success ? alias.Index + alias.Length : collection.Index + collection.Length + newAliasClause.Length;
                    selectIndex = where.Success ? where.Index + where.Length : aliasIndexEnd;
                }

                var projection = " select { ";

                foreach (var field in projectionFields)
                {
                    // TODO: This fixes an issue of the id() column being null, fix it and remove this
                    //if (field.Key.Equals("id()", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    continue;
                    //}

                    //if (field.Key.Equals("id()", StringComparison.OrdinalIgnoreCase) ||
                    //    field.Key.Equals("json()", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    projection += $"\"{field.Key}\": \"\", ";
                    //    continue;
                    //}

                    projection += $"\"{field.Key}\": {field.Value}, ";
                }

                projection = projection[0..^2];
                projection += " } ";

                rql = rql.Insert(selectIndex.Value, projection);
            }

            return rql;
        }

        private static bool IsRql(string queryToMatch, out Match rql)
        {
            rql = _rqlRegex.Match(queryToMatch);
            return rql.Success;
        }

        //private static string GenerateProjectionString(Match match, string alias, Group selectFieldValues=null)
        //{
        //    // TODO: if columns.Success == false throw, also for each of the other groups

        //    //if (replaceSourceColumns.Captures.Count != replaceInputs.Captures.Count &&
        //    //    replaceSourceColumns.Captures.Count != replaceTexts.Captures.Count &&
        //    //    replaceSourceColumns.Captures.Count != replaceDestColumns.Captures.Count &&
        //    //    (selectFieldValues != null && replaceSourceColumns.Captures.Count != selectFieldValues.Captures.Count))
        //    //{
        //    //    // todo: provide these values and the SQL in the error message
        //    //    throw new PgErrorException(PgErrorCodes.StatementTooComplex, "replace(..) function in SQL statement wasn't provided the expected parameters.");
        //    //}

        //    var replaceDict = new Dictionary<string, string>();
        //    PopulateReplaceFields(replaceDict, match, alias);

        //    var projection = "select { ";
        //    for (int i = 0; i < columns.Captures.Count; i++)
        //    {
        //        var columnName = columns.Captures[i].Value;

        //        // We have to project these because otherwise the order of the fields gets mixed up
        //        if (columnName.Equals("id()", StringComparison.OrdinalIgnoreCase) ||
        //            columnName.Equals("json()", StringComparison.OrdinalIgnoreCase)) 
        //        {
        //            projection += $"\"{columnName}\": \"\"";
        //        }

        //        if (replaceDict.TryGetValue(columnName, out var value))
        //        {
        //            projection += $"\"{columnName}\": {value}";
        //        }
        //        //else if (selectFieldValues != null) // TODO: Support this, i don't think it works as it is
        //        //{
        //        //    projection += $"\"{columnName}\": ({selectFieldValues.Captures[i].Value})";
        //        //}
        //        else
        //        {
        //            projection += $"\"{columnName}\": {alias}.{columnName}";
        //        }

        //        projection += ", ";
        //    }

        //    projection = projection[0..^2];
        //    projection += " }";

        //    return projection;
        //}
    }
}
