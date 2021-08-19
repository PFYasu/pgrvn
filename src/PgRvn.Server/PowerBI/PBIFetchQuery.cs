﻿using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PgRvn.Server.Exceptions;

namespace PgRvn.Server.PowerBI
{
    public static class PBIFetchQuery
    {
        private static readonly Regex FetchSqlRegex = new(@"(?is)^\s*(?:select\s+(?:\*|(?:(?:(?:""(\$Table|_)""\.)?""(?<src_columns>[^""]+)""(?:\s+as\s+""(?<all_columns>(?<dest_columns>[^""]+))"")?(?<replace>)|(?<replace>replace)\(""_"".""(?<src_columns>[^""]+)"",\s+'(?<replace_inputs>[^']*)',\s+'(?<replace_texts>[^']*)'\)\s+as\s+""(?<all_columns>(?<dest_columns>[^""]+))"")(?:\s|,)*)+)\s+from\s+(?:(?:\((?:\s|,)*)(?<inner_query>.*)\s*\)|""public"".""(?<table_name>.+)""))\s+""(?:\$Table|_)""(\s+where\s+(?<where>.*?))?(?:\s+limit\s+(?<limit>[0-9]+))?\s*$",
            RegexOptions.Compiled);

        private static readonly Regex RqlRegex = new(@"^(?is)\s*(?<rql>(?:/\*rql\*/\s*)?from\s+(?<collection>[^\s\(\)]+)(?:\s+as\s+(?<alias>\S+))?.*?(?<select>\s+select\s+((?<js_select>({\s*(?<js_fields>(?<js_keys>.+?)(\s*:\s*((?<js_vals>.+?))|(?<js_vals>))\s*,\s*)*(?<js_fields>(?<js_keys>.+?)((\s*:\s*(?<js_vals>.+?))|(?<js_vals>))\s*)}))|(?<simple_select>((?<simple_keys>.+?)\s*,\s*)*(((?<simple_keys>\S+)|(?<simple_keys>"".* ""))(\s*as\s*(\S+|"".* "")\s*)?))))?(?:\s+include\s+(?<include>.*))?\s*)$",
            RegexOptions.Compiled);

        private static readonly Regex WhereColumnRegex = new(@"""_""\.""(?<column>.*?)""", RegexOptions.Compiled);

        private static readonly Regex WhereOperatorRegex = new(@"(?=.*?\s+)is(\s+not)?(?=\s+.+?)", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> OperatorMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "is", "=" },
            { "is not", "!=" },
        };

        private static bool TryGetMatches(string queryText, out List<Match> outMatches, out Match rql)
        {
            var matches = new List<Match>();
            var queryToMatch = queryText;
            Group innerQuery;

            rql = null;

            // Queries can have inner queries that we need to parse, so here we collect those
            do
            {
                var match = FetchSqlRegex.Match(queryToMatch);

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
            if (!TryGetMatches(queryText, out var matches, out var rql))
            {
                pgQuery = null;
                return false;
            }

            string newRql = null;
            if (rql != null && rql.Success)
            {
                // TODO: Integrate with Raven.Server's QueryParser to support things like "select LastName as last" and "select "test""

                var alias = rql.Groups["alias"].Success ? rql.Groups["alias"].Value : "x";

                var projectionFields = new Dictionary<string, string>();
                bool alreadyPopulated = false;

                var simpleSelectKeys = rql.Groups["simple_keys"];
                var jsSelectFields = rql.Groups["js_fields"];

                if (simpleSelectKeys.Success)
                {
                    projectionFields["id()"] = GenerateColumnValue("id()", alias);

                    foreach (Capture selectField in simpleSelectKeys.Captures)
                    {
                        projectionFields[selectField.Value] = GenerateColumnValue(selectField.Value, alias);
                    }

                    projectionFields["json()"] = GenerateColumnValue("json()", alias);

                    alreadyPopulated = true;
                }
                else if (jsSelectFields.Success)
                {
                    var jsSelectKeys = rql.Groups["js_keys"];
                    var jsSelectValues = rql.Groups["js_vals"];

                    projectionFields["id()"] = GenerateColumnValue("id()", alias);

                    for (var i = 0; i < jsSelectKeys.Captures.Count; i++)
                    {
                        var key = jsSelectKeys.Captures[i];

                        if (jsSelectValues.Captures[i].Length == 0)
                        {
                            projectionFields[key.Value] = "null";
                        }
                        else
                        {
                            projectionFields[key.Value] = jsSelectValues.Captures[i].Value;
                        }
                    }

                    projectionFields["json()"] = GenerateColumnValue("json()", alias);

                    alreadyPopulated = true;
                }

                // Populate the columns starting from the inner-most SQL
                for (var i = matches.Count - 1; i >= 0; i--)
                {
                    var match = matches[i];
                    alreadyPopulated = alreadyPopulated || i != matches.Count - 1;
                    PopulateProjectionFields(match, projectionFields, alias, alreadyPopulated);
                }

                // Note: It's crucial that the order of columns that is specified in the outer SQL is preserved.
                var orderedProjectionFields = GetOrderedProjectionFields(matches, projectionFields, rql);

                newRql = GenerateProjectedRql(rql, orderedProjectionFields, matches);
            }
            else if (matches[0].Groups["table_name"].Success)
            {
                if (matches.Count != 1)
                    throw new PgErrorException(PgErrorCodes.StatementTooComplex,
                        "Unexpected PowerBI nested SQL query. Query: " + queryText); 

                // Handle generic query
                var alias = "x";

                var projectionFields = new Dictionary<string, string>();
                PopulateProjectionFields(matches[0], projectionFields, alias, false);

                var orderedProjectionFields = GetOrderedProjectionFields(matches, projectionFields);

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

            Console.WriteLine(">> RQL:\n" + newRql + "\n");
            pgQuery = new RqlQuery(newRql, parametersDataTypes, documentStore, limit.Success ? int.Parse(limit.Value) : null);
            return true;
        }

        private static List<KeyValuePair<string, string>> GetOrderedProjectionFields(IEnumerable<Match> matches, IReadOnlyDictionary<string, string> projectionFields, Match rql=null)
        {
            var orderedProjectionFields = new List<KeyValuePair<string, string>>();

            // Get the order from the outermost SELECT
            ICollection<Capture> orderedColumns = Array.Empty<Capture>();
            foreach (var match in matches)
            {
                orderedColumns = match.Groups["all_columns"].Captures;
                if (orderedColumns.Count != 0)
                    break;
            }

            // If none captured use the RQL projection order
            if (rql != null && orderedColumns.Count == 0)
            {
                if (rql.Groups["simple_keys"].Success)
                    orderedColumns = rql.Groups["simple_keys"].Captures;
                else if (rql.Groups["js_keys"].Success)
                    orderedColumns = rql.Groups["js_keys"].Captures;
            }

            // If there's no RQL projection order use orderedColumns
            if (orderedColumns.Count == 0)
            {
                return projectionFields.ToList();
            }

            foreach (var column in orderedColumns)
            {
                orderedProjectionFields.Add(projectionFields.TryGetValue(column.Value, out var val)
                    ? new KeyValuePair<string, string>(column.Value, val)
                    : new KeyValuePair<string, string>(column.Value, "null"));
            }

            return orderedProjectionFields;
        }

        private static void PopulateProjectionFields(
            Match match, 
            IDictionary<string, string> projectionFields, 
            string alias, 
            bool alreadyPopulatedByPreviousLayer)
        {
            var destColumns = match.Groups["dest_columns"].Captures;
            var srcColumns = match.Groups["src_columns"].Captures;
            var replace = match.Groups["replace"].Captures;
            var replaceInputs = match.Groups["replace_inputs"].Captures;
            var replaceTexts = match.Groups["replace_texts"].Captures;

            var replaceIndex = 0;
            for (var i = 0; i < destColumns.Count; i++)
            {
                var destColumn = destColumns[i].Value;
                var srcColumn = srcColumns[i].Value;

                string destColumnVal = "";

                // Try using the source column if it exists yet
                if (projectionFields.TryGetValue(srcColumn, out string val))
                {
                    destColumnVal = val;
                }
                else
                {
                    // Don't create new fields without a source when projectionFields is 
                    // already populated from a previous SQL/RQL layer
                    if (alreadyPopulatedByPreviousLayer)
                    {
                        continue;
                    }

                    destColumnVal = GenerateColumnValue(destColumn, alias);
                }

                if (replace[i].Value.Length != 0)
                {
                    destColumnVal = $"({destColumnVal}).toString()" +
                        $".replace(\"{replaceInputs[replaceIndex].Value}\", \"{replaceTexts[replaceIndex].Value}\")";
                    replaceIndex++;
                }

                projectionFields[destColumn] = destColumnVal;
            }
        }

        private static string GenerateColumnValue(string column, string alias)
        {
            var val = $"{alias}[\"{column}\"]";

            // TODO: Support replacing json()
            // TODO: Won't work on queries with include because in RqlQuery.Execute included documents' ids are put into id()
            if (column.Equals("id()", StringComparison.OrdinalIgnoreCase))
            {
                val = $"{alias}[\"@metadata\"][\"@id\"]";
            }

            return val;
        }

        private static string GenerateProjectedRql(Match rqlMatch, List<KeyValuePair<string, string>> projectionFields, ICollection<Match> matches)
        {
            var rql = rqlMatch.Value;

            var collection = rqlMatch.Groups["collection"];
            var alias = rqlMatch.Groups["alias"];
            var where = rqlMatch.Groups["where"];
            var select = rqlMatch.Groups["select"];

            var aliasIndex = alias.Success ? (alias.Index + alias.Length) : (collection.Index + collection.Length);
            var aliasLength = alias.Length;

            var whereIndex = where.Success ? where.Index : (aliasIndex + aliasLength);
            var whereLength = where.Length;

            var selectIndex = select.Success ? select.Index : (whereIndex + whereLength);

            // Insert alias clause if doesn't exist
            if (!alias.Success)
            {
                const string newAliasClause = " as x";
                rql = rql.Insert(collection.Index + collection.Length, newAliasClause);

                aliasLength = newAliasClause.Length;
                whereIndex += newAliasClause.Length;
                selectIndex += newAliasClause.Length;
            }

            // Insert new where clause
            // Note: Filtering of projected columns is not supported
            var fullNewWhere = "";
            foreach (var match in matches)
            {
                var sqlWhere = match.Groups["where"];
                if (!sqlWhere.Success) 
                    continue;

                if (fullNewWhere.Length != 0)
                    fullNewWhere += " and ";

                fullNewWhere += $"({sqlWhere.Value})";
            }

            if (fullNewWhere.Length != 0)
            {
                fullNewWhere = WhereColumnRegex.Replace(fullNewWhere, "${column}");
                fullNewWhere = WhereOperatorRegex.Replace(fullNewWhere, (m) =>
                {
                    if (OperatorMap.TryGetValue(m.Value, out var val))
                        return val;

                    return m.Value;
                });

                if (where.Success)
                    fullNewWhere = $" and ({fullNewWhere}) ";
                else
                    fullNewWhere = $"where {fullNewWhere} ";

                rql = rql.Insert(whereIndex + whereLength, fullNewWhere);

                whereLength += fullNewWhere.Length;
                selectIndex += fullNewWhere.Length;
            }

            // Remove existing select clause
            if (select.Success)
            {
                rql = rql.Remove(selectIndex, select.Length);
            }

            // Insert new select clause
            if (projectionFields.Count != 0)
            {
                var projection = GenerateProjectionString(projectionFields);
                rql = rql.Insert(selectIndex, projection);
            }

            return rql;
        }

        private static string GenerateProjectionString(IEnumerable<KeyValuePair<string, string>> projectionFields)
        {
            var projection = " select { ";

            foreach (var (fieldName, fieldValue) in projectionFields)
            {
                projection += $"\"{fieldName}\": {fieldValue}, ";
            }

            projection = projection[0..^2];
            projection += " } ";

            return projection;
        }

        private static bool IsRql(string queryToMatch, out Match rql)
        {
            rql = RqlRegex.Match(queryToMatch);
            return rql.Success;
        }
    }
}
