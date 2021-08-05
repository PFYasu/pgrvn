using Raven.Client.Documents.Session;
using Sparrow.Json;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;

namespace PgRvn.Server
{
    public abstract class PgQuery : IDisposable
    {
        protected readonly string QueryString;
        public readonly int[] ParametersDataTypes;
        protected readonly bool IsEmptyQuery;
        public Dictionary<string, object> Parameters;
        protected readonly Dictionary<string, PgColumn> Columns;
        private short[] _resultColumnFormatCodes;

        protected PgQuery(string queryString, int[] parametersDataTypes)
        {
            QueryString = queryString;
            ParametersDataTypes = parametersDataTypes;
            IsEmptyQuery = string.IsNullOrWhiteSpace(QueryString);
            Parameters = new Dictionary<string, object>();
            Columns = new Dictionary<string, PgColumn>();
            _resultColumnFormatCodes = Array.Empty<short>();
        }

        public static PgQuery CreateInstance(string queryText, int[] parametersDataTypes, IDocumentStore documentStore)
        {
            Console.WriteLine(">> Received query:\n" + queryText + "\n");

            if (RqlQuery.TryParse(queryText, parametersDataTypes, documentStore, out var rqlQuery))
            {
                return rqlQuery;
            }

            if (PowerBIQuery.TryParse(queryText, parametersDataTypes, documentStore, out var powerBiQuery))
            {
                return powerBiQuery;
            }

            return new SqlQuery(queryText, parametersDataTypes);
        }

        protected PgFormat GetDefaultResultsFormat()
        {
            return _resultColumnFormatCodes.Length switch
            {
                0 => PgFormat.Text,
                1 => _resultColumnFormatCodes[0] == 0 ? PgFormat.Text : PgFormat.Binary,
                _ => throw new NotSupportedException(
                    "No support for column format code count that isn't 0 or 1, got: " +
                    _resultColumnFormatCodes.Length)
            };
        }

        public abstract Task<ICollection<PgColumn>> Init(bool allowMultipleStatements = false);

        public abstract Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token);

        public abstract void Dispose();

        public virtual void Bind(IEnumerable<byte[]> parameters, short[] parameterFormatCodes, short[] resultColumnFormatCodes)
        {
            _resultColumnFormatCodes = resultColumnFormatCodes;

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
                else
                {
                    processedParameter = PgTypeConverter.FromBytes[(0, dataFormat)](parameter);
                }

                Parameters.Add($"p{i + 1}", processedParameter);
                i++;
            }
        }
    }
}