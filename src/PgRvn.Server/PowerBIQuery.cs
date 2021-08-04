using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public static class PowerBIQuery
    {
        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out PgQuery pgQuery)
        {
            // Match RQL queries sent by PowerBI, the RQL is wrapped in an SQL statement (e.g. select * from ( from Orders ) "_" limit 0)
            var regexStr = @"(?i)^(?:\n|\r|\t| )*(?:select(?:\n|\r|\t| )+(?:\*|(?:(?:""(\$Table|_)""\.""(?<column>[^""]+)"" as ""[^""]+""|replace\(""_"".""(?<replace_column>[^""]+)"", '(?<replace_input>[^']+)', '(?<replace_text>[^']+)'\) as ""[^""]+"")(?:\n|\r|\t| |,)*)+)(?:\n|\r|\t| )+from(?:\n|\r|\t| )+(?:(?:\((?:\n|\r|\t| |,)*)(?<rql>(?:.|\n|\r|\t)*)(?:\n|\r|\t| )*\)|""public"".""(?<table_name>.+)""))(?:\n|\r|\t| )+""(?:\$Table|_)""(?:(?:\n|\r|\t| )+limit(?:\n|\r|\t| )+(?<limit>[0-9]+))?(?:\n|\r|\t| )*$";
            var match = new Regex(regexStr).Match(queryText);
            if (match.Success)
            {
                var limit = match.Groups["limit"];
                var tableName = match.Groups["table_name"];
                var rqlMatch = match.Groups["rql"];
                var columns = match.Groups["column"];
                var replaceColumns = match.Groups["replace_column"];
                var replaceInputs = match.Groups["replace_input"];
                var replaceTexts = match.Groups["replace_text"];

                // If there is a table name match, its a preview query
                if (tableName.Success)
                {
                    // TODO: Provide these as parameters to prevent SQL injection (depends on RavenDB-17075)
                    pgQuery = new RqlQuery($"from {tableName.Value} {(limit.Success ? "limit " + limit.Value : "")}", parametersDataTypes, documentStore);
                    return true;
                }
                else if (rqlMatch.Success)
                {
                    // Found RQL
                    pgQuery = new RqlQuery(rqlMatch.Value, parametersDataTypes, documentStore, limit.Success ? int.Parse(limit.Value) : null);
                    return true;
                }
            }

            // Get all collections
            // TODO: Match either \n or \r\n
            var tableQuery = "select TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE\r\nfrom INFORMATION_SCHEMA.tables\r\nwhere TABLE_SCHEMA not in ('information_schema', 'pg_catalog')\r\norder by TABLE_SCHEMA, TABLE_NAME";
            if (queryText == tableQuery)
            {
                var collectionStats = documentStore.Maintenance.Send(new GetCollectionStatisticsOperation());

                var data = new List<PgDataRow>();
                foreach (var collection in collectionStats.Collections.Keys)
                {
                    data.Add(new PgDataRow
                    {
                        ColumnData = new ReadOnlyMemory<byte>?[]
                        {
                            Encoding.UTF8.GetBytes("public"),
                            Encoding.UTF8.GetBytes(collection),
                            Encoding.UTF8.GetBytes("BASE TABLE"),
                        }
                    });
                }

                var results = new PgTable
                {
                    Columns = new List<PgColumn>
                    {
                        new PgColumn
                        {
                            ColumnIndex = 0,
                            DataTypeSize = -1,
                            FormatCode = PgFormat.Text,
                            Name = "table_schema",
                            TableObjectId = 0,
                            TypeModifier = -1,
                            TypeObjectId = PgTypeOIDs.Name,
                        },
                        new PgColumn
                        {
                            ColumnIndex = 1,
                            DataTypeSize = -1,
                            FormatCode = PgFormat.Text,
                            Name = "table_name",
                            TableObjectId = 0,
                            TypeModifier = -1,
                            TypeObjectId = PgTypeOIDs.Name,
                        },
                        new PgColumn
                        {
                            ColumnIndex = 2,
                            DataTypeSize = -1,
                            FormatCode = PgFormat.Text,
                            Name = "table_type",
                            TableObjectId = 0,
                            TypeModifier = -1,
                            TypeObjectId = PgTypeOIDs.Varchar,
                        }
                    },
                    Data = data
                };

                pgQuery = new SqlQuery(queryText, parametersDataTypes, results);
                return true;
            }

            // Get collection preview
            regexStr = "(?i)^(?: |\t|\n|\r)*select(?: |\t|\n|\r)+.*(?: |\t|\n|\r)+from(?: |\t|\n|\r)+INFORMATION_SCHEMA.columns(?: |\t|\n|\r)+where(?: |\t|\n|\r)+TABLE_SCHEMA(?: |\t|\n|\r)+=(?: |\t|\n|\r)+'public'(?: |\t|\n|\r)+and(?: |\t|\n|\r)+TABLE_NAME(?: |\t|\n|\r)+=(?: |\t|\n|\r)+'(?<table_name>[^']+)'(?: |\t|\n|\r)+order(?: |\t|\n|\r)+by(?: |\t|\n|\r)+TABLE_SCHEMA(?: |\t|\n|\r)*,(?: |\t|\n|\r)*TABLE_NAME(?: |\t|\n|\r)*,(?: |\t|\n|\r)*ORDINAL_POSITION(?: |\t|\n|\r)*$";
            match = new Regex(regexStr).Match(queryText);

            if (match.Success)
            {
                var tableName = match.Groups["table_name"].Value;

                pgQuery = new PBIPreviewQuery(parametersDataTypes, documentStore, tableName);
                return true;
            }

            pgQuery = null;
            return false;
        }
    }
}
