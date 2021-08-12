using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PgRvn.Server
{
    public static class PBIFetchQuery
    {
        private static readonly Regex _regex = new(@"(?is)^\s*(?:select\s+(?:\*|(?:(?:(?:""(\$Table|_)""\.)?""(?<src_columns>[^""]+)""(?:\s+as\s+""(?<all_columns>(?<dest_columns>[^""]+))"")?|replace\(""_"".""(?<replace_source_columns>[^""]+)"",\s+'(?<replace_inputs>[^']*)',\s+'(?<replace_texts>[^']*)'\)\s+as\s+""(?<all_columns>(?<replace_dest_columns>[^""]+))"")(?:\s|,)*)+)\s+from\s+(?:(?:\((?:\s|,)*)(?<inner_query>.*)\s*\)|""public"".""(?<table_name>.+)""))\s+""(?:\$Table|_)""(?:\s+limit\s+(?<limit>[0-9]+))?\s*$",
            RegexOptions.Compiled);
        private static readonly Regex _rqlRegex = new(@"^(?is)(?<rql>\s*(?:/\*rql\*/\s*)?from\s+(?<collection>[^\s\(\)]+)(?:\s+as\s+(?<alias>\S*))?.*?(?:\s+select\s+(?<select>(?<js_select>{\s*(?<js_select_inside>(?<js_select_field>(?<js_select_field_key>\S+\s*)(?::\s*(?<js_select_field_value>\S+))?\s*,\s*)*(?<js_select_field>(?<js_select_field_key>\S+\s*):\s*(?<js_select_field_value>\S+)\s*))?\s*})|(?<simple_select>((?<simple_select_fields>\S+),\s*)*(?<simple_select_fields>[^\s{}:=]+)))(\s.*)?)?(?:\s+include\s+(?<include>.*))?)$",
            RegexOptions.Compiled);


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

                //projectionFields.TryAdd(destColumnName, $"{fieldValueStart}.toString().replace(\"{replaceInput}\", \"{replaceText}\")");
                projectionFields[destColumnName] = $"{fieldValueStart}.toString().replace(\"{replaceInput}\", \"{replaceText}\")";
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

            string newRql = null;
            if (rql != null && rql.Success)
            {
                // Handle RQL type query
                var alias = rql.Groups["alias"].Success ? rql.Groups["alias"].Value : "x";

                // TODO: Populate another dictionary with the existing RQL select fields
                var simpleSelectFields = rql.Groups["simple_select_fields"];

                // Populate the columns starting from the inner-most SQL
                var projectionFields = new Dictionary<string, string>();
                for (int i = matches.Count - 1; i >= 0; i--)
                {
                    Match match = matches[i];
                    PopulateProjectionFields(match, projectionFields, alias);
                }

                // Note: It's crucial that the order of columns that is specified in the outer SQL is preserved.
                var orderedProjectionFields = GetOrderedProjectionFields(matches[0], projectionFields);

                newRql = GenerateProjectedRql(rql, orderedProjectionFields);
            }
            else if (matches[0].Groups["table_name"].Success)
            {
                // TODO: Remove this
                if (matches.Count != 1)
                    throw new Exception("Too many matches, investigate."); 

                // Handle generic type query
                var alias = "x";

                var projectionFields = new Dictionary<string, string>();
                PopulateProjectionFields(matches[0], projectionFields, alias);

                var orderedProjectionFields = GetOrderedProjectionFields(matches[0], projectionFields);

                // TODO: Provide these as parameters to prevent SQL injection (depends on RavenDB-17075)
                newRql = $"from {matches[0].Groups["table_name"].Value} as {alias} ";
                newRql += GenerateProjectionString(orderedProjectionFields);
            }

            if (newRql == null)
            {
                pgQuery = null;
                return false;
            }

            var limit = matches[0].Groups["limit"];
            pgQuery = new RqlQuery(newRql, parametersDataTypes, documentStore, limit.Success ? int.Parse(limit.Value) : null);
            return true;
        }

        private static List<(string, string)> GetOrderedProjectionFields(Match match, Dictionary<string, string> projectionFields)
        {
            var orderedProjectionFields = new List<(string, string)>();
            var orderedColumns = match.Groups["all_columns"].Captures;
            foreach (Capture column in orderedColumns)
            {
                orderedProjectionFields.Add((column.Value, projectionFields[column.Value]));
            }

            return orderedProjectionFields;
        }

        private static void PopulateProjectionFields(Match match, Dictionary<string, string> projectionFields, string alias)
        {
            var destColumns = match.Groups["dest_columns"].Captures;
            var srcColumns = match.Groups["src_columns"].Captures;

            for (int i = 0; i < destColumns.Count; i++)
            {
                var destColumn = destColumns[i].Value;
                var srcColumn = srcColumns[i].Value;

                var fieldStartValue = $"{alias}[\"{destColumn}\"]";
                if (!destColumn.Equals(srcColumn) && projectionFields.TryGetValue(srcColumn, out string val))
                {
                    fieldStartValue = val;

                    // Force the assignment in this case
                    projectionFields[destColumn] = $"{fieldStartValue}";
                }
                else
                {
                    // TODO: Support replacing json()
                    // dest_column.Equals("json()", StringComparison.OrdinalIgnoreCase)

                    // TODO: This won't work with RQLs that has include because of the logic in RqlQuery.Execute
                    if (destColumn.Equals("id()", StringComparison.OrdinalIgnoreCase))
                    {
                        projectionFields.TryAdd(destColumn, $"{alias}[\"@metadata\"][\"@id\"]");
                        continue;
                    }

                    projectionFields.TryAdd(destColumn, $"{fieldStartValue}");
                }
            }

            PopulateReplaceFields(projectionFields, match, alias);
        }

        private static string GenerateProjectedRql(Match rqlMatch, List<(string, string)> projectionFields)
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

                var projection = GenerateProjectionString(projectionFields);
                rql = rql.Insert(selectIndex.Value, projection);
            }

            return rql;
        }

        private static string GenerateProjectionString(IEnumerable<(string, string)> projectionFields)
        {
            var projection = " select { ";

            foreach (var field in projectionFields)
            {
                projection += $"\"{field.Item1}\": {field.Item2}, ";
            }

            projection = projection[0..^2];
            projection += " } ";

            return projection;
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
