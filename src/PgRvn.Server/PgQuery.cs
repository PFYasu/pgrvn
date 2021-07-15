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
        protected short[] ResultColumnFormatCodes;

        protected PgQuery(string queryString, int[] parametersDataTypes)
        {
            QueryString = queryString;
            ParametersDataTypes = parametersDataTypes;
            IsEmptyQuery = string.IsNullOrWhiteSpace(QueryString);
            Parameters = new Dictionary<string, object>();
            Columns = new Dictionary<string, PgColumn>();
            ResultColumnFormatCodes = Array.Empty<short>();
        }

        public static PgQuery CreateInstance(string queryText, int[] parametersDataTypes, IDocumentStore documentStore)
        {
            if (queryText.StartsWith("from", StringComparison.CurrentCultureIgnoreCase))
            {
                return new RqlQuery(queryText, parametersDataTypes, documentStore);
            }

            return new SqlQuery(queryText, parametersDataTypes);
        }

        public abstract Task<ICollection<PgColumn>> Init(bool allowMultipleStatements=false);

        public abstract Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token);

        public abstract void Dispose();

        public void Bind(IEnumerable<byte[]> parameters, short[] parameterFormatCodes, short[] resultColumnFormatCodes)
        {
            ResultColumnFormatCodes = resultColumnFormatCodes;

            if (ParametersDataTypes == null)
            {
                // todo throw
                return;
            }

            PgFormat? defaultDataFormat = null;
            switch (parameterFormatCodes.Length)
            {
                case 0:
                    defaultDataFormat = PgFormat.Text;
                    break;
                case 1:
                    defaultDataFormat = parameterFormatCodes[0] == 1 ? PgFormat.Binary : PgFormat.Text;
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

                var dataFormat = defaultDataFormat ?? (parameterFormatCodes[i] == 1 ? PgFormat.Binary : PgFormat.Text);
                object processedParameter = parameter;
                if (PgTypeConverter.FromBytes.TryGetValue((dataType, dataFormat), out var fromBytesFunc))
                {
                    processedParameter = fromBytesFunc(parameter);
                }

                Parameters.Add($"p{i + 1}", processedParameter);
                i++;
            }
        }
    }
}