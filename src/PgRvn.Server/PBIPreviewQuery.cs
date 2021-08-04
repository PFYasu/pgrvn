using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public class PBIPreviewQuery : RqlQuery
    {
        public string _tableName;

        private List<PgDataRow> _results;

        public PBIPreviewQuery(int[] parameterDataTypes, IDocumentStore documentStore, string tableName) 
            : base($"from {tableName} limit 1", parameterDataTypes, documentStore)
        {
            _tableName = tableName;
            _results = new List<PgDataRow>();
        }

        public override async Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token)
        {
            foreach (var dataRow in _results)
            {
                await writer.WriteAsync(builder.DataRow(dataRow.ColumnData.Span), token);
            }

            await writer.WriteAsync(builder.CommandComplete($"SELECT {_results.Count}"), token);
        }

        public override void Bind(IEnumerable<byte[]> parameters, short[] parameterFormatCodes, short[] resultColumnFormatCodes)
        {
            // Note: We don't call base.Bind(..) because we only support parameters for this custom RQL, not the original SQL

            // TODO: Once RavenDB-17075 is merged, uncomment this and remove the workaround
            //Parameters.Add("$tableName", _tableName);
        }

        public override async Task<ICollection<PgColumn>> Init(bool allowMultipleStatements = false)
        {
            var schema = await base.Init(allowMultipleStatements);

            int i = 0;
            foreach (var column in schema)
            {
                _results.Add(new PgDataRow
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                            Encoding.UTF8.GetBytes(column.Name),
                            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(i)),
                            Encoding.UTF8.GetBytes("YES"),
                            Encoding.UTF8.GetBytes(""), // Note: Should be the column datatype name, but this works too
                    }
                });
                i++;
            }

            return new List<PgColumn>
            {
                new PgColumn
                {
                    ColumnIndex = 0,
                    DataTypeSize = -1,
                    FormatCode = PgFormat.Text,
                    Name = "column_name",
                    TableObjectId = 0,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Name,
                },
                new PgColumn
                {
                    ColumnIndex = 1,
                    DataTypeSize = -1,
                    FormatCode = PgFormat.Binary, // TODO: Make sure it's the requested format
                    Name = "ordinal_position",
                    TableObjectId = 0,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Int4,
                },
                new PgColumn
                {
                    ColumnIndex = 2,
                    DataTypeSize = -1,
                    FormatCode = PgFormat.Text,
                    Name = "is_nullable",
                    TableObjectId = 0,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Varchar,
                },
                new PgColumn
                {
                    ColumnIndex = 3,
                    DataTypeSize = -1,
                    FormatCode = PgFormat.Text,
                    Name = "data_type",
                    TableObjectId = 0,
                    TypeModifier = -1,
                    TypeObjectId = PgTypeOIDs.Varchar,
                },
            };
        }
    }
}
