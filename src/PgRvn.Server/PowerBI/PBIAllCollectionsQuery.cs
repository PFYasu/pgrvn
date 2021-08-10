using Raven.Client.Documents;
using Raven.Client.Documents.Operations;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public class PBIAllCollectionsQuery : RqlQuery
    {
        public PBIAllCollectionsQuery(string queryText, int[] parametersDataTypes, IDocumentStore documentStore) 
            : base(queryText, parametersDataTypes, documentStore)
        {
        }

        public static bool TryParse(string queryText, int[] parametersDataTypes, IDocumentStore documentStore, out PgQuery pgQuery)
        {
            // TODO: Match either \n or \r\n
            var tableQuery = "select TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE\nfrom INFORMATION_SCHEMA.tables\nwhere TABLE_SCHEMA not in ('information_schema', 'pg_catalog')\norder by TABLE_SCHEMA, TABLE_NAME";
            if (queryText.Replace("\r", string.Empty) == tableQuery)
            {
                pgQuery = new PBIAllCollectionsQuery(queryText, parametersDataTypes, documentStore);
                return true;
            }

            pgQuery = null;
            return false;
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements = false)
        {
            return new PgColumn[]
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
            };
        }

        public override async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            var collectionStats = _documentStore.Maintenance.Send(new GetCollectionStatisticsOperation());

            foreach (var collection in collectionStats.Collections.Keys)
            {
                var dataRow = new ReadOnlyMemory<byte>?[]
                {
                        Encoding.UTF8.GetBytes("public"),
                        Encoding.UTF8.GetBytes(collection),
                        Encoding.UTF8.GetBytes("BASE TABLE"),
                };

                await writer.WriteAsync(builder.DataRow(dataRow), token);
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {collectionStats.Collections.Count}"), token);
        }
    }
}
