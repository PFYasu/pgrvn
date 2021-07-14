using Raven.Client.Documents.Session;
using Sparrow.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Queries;

namespace PgRvn.Server
{
    public abstract class PgQuery : IDisposable
    {
        protected readonly string QueryString;
        public readonly int[] ParametersDataTypes;
        protected readonly bool IsEmptyQuery;
        protected Dictionary<string, object> Parameters;
        protected readonly Dictionary<string, PgColumn> Columns;

        protected PgQuery(string queryString, int[] parametersDataTypes)
        {
            QueryString = queryString;
            ParametersDataTypes = parametersDataTypes;
            IsEmptyQuery = string.IsNullOrWhiteSpace(QueryString);
            Parameters = new Dictionary<string, object>();
            Columns = new Dictionary<string, PgColumn>();
        }

        public static PgQuery CreateInstance(string queryText, int[] parametersDataTypes, IDocumentStore documentStore)
        {
            if (queryText.StartsWith("from", StringComparison.CurrentCultureIgnoreCase))
            {
                return new RqlQuery(queryText, parametersDataTypes, documentStore);
            }

            return new SqlQuery(queryText, parametersDataTypes);
        }

        public abstract Task<ICollection<PgColumn>> Init();

        public abstract Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token);

        public abstract void Dispose();

        public void Bind(IEnumerable<byte[]> parameters, short[] parameterFormatCodes)
        {
            if (ParametersDataTypes == null)
            {
                // todo throw
                return;
            }

            short? defaultDataFormat = null;
            switch (parameterFormatCodes.Length)
            {
                case 0:
                    defaultDataFormat = (short)PgFormat.Text;
                    break;
                case 1:
                    defaultDataFormat = parameterFormatCodes[0];
                    break;
                default:
                    // todo throw
                    break;
            }

            int i = 0;
            foreach (var parameter in parameters)
            {
                int dataType = 0;
                if (i < ParametersDataTypes.Length)
                {
                    dataType = ParametersDataTypes[i];
                }

                short dataFormat = defaultDataFormat ?? parameterFormatCodes[i];
                object processedParameter = (dataType, dataFormat) switch
                {
                    (PgTypeOIDs.Bool, (short)PgFormat.Binary) => PgConfig.TrueBuffer.SequenceEqual(parameter),
                    (PgTypeOIDs.Text, (short)PgFormat.Text) => Encoding.UTF8.GetString(parameter),
                    (PgTypeOIDs.Json, (short)PgFormat.Text) => Encoding.UTF8.GetString(parameter),
                    (PgTypeOIDs.Int2, (short)PgFormat.Binary) => IPAddress.NetworkToHostOrder(BitConverter.ToInt16(parameter)),
                    (PgTypeOIDs.Int4, (short)PgFormat.Binary) => IPAddress.NetworkToHostOrder(BitConverter.ToInt32(parameter)),
                    (PgTypeOIDs.Int8, (short)PgFormat.Binary) => IPAddress.NetworkToHostOrder(BitConverter.ToInt64(parameter)),
                    (PgTypeOIDs.Float4, (short)PgFormat.Binary) => BitConverter.ToSingle(parameter.Reverse().ToArray()),
                    (PgTypeOIDs.Float8, (short)PgFormat.Binary) => BitConverter.ToDouble(parameter.Reverse().ToArray()),
                    // (PgTypeOIDs.Char, (short)PgFormat.Binary) => BitConverter.ToChar(parameter.Reverse().ToArray()), // TODO: Test char support
                    _ => parameter
                };

                Parameters.Add($"p{i + 1}", processedParameter);
                i++;
            }
        }
    }
}