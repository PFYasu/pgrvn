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

        private static bool IsPowerBIQuery(ref string queryText, out bool isInitialPowerBIQuery)
        {
            // Initial PowerBI query
            var stmtStart = @"select * 
from 
(
    from";

            if (queryText.StartsWith(stmtStart))
            {
                queryText = "from" + queryText.Remove(0, stmtStart.Length);

                var postfix = "\r\n) \"_\"\r\nlimit 0";
                var postfixIndex = queryText.IndexOf(postfix);
                queryText = queryText.Remove(postfixIndex);

                isInitialPowerBIQuery = true;
                return true;
            }

            // Second PowerBI query
            if (queryText.StartsWith("select \"$Table\".\"") ||
                queryText.StartsWith("select \"$pTable\".\""))
            {
                var prefix = "from \r\n(\r\n    ";
                var prefixIndex = queryText.IndexOf(prefix);
                var postfix = "\r\n) \"$Table\"";
                var postfixIndex = queryText.IndexOf(postfix);
                postfixIndex = postfixIndex == -1 ? queryText.IndexOf("\r\n) \"$pTable\"") : postfixIndex;
                // Note: we ignore the "limit 1000" that is usually provided
                queryText = queryText.Substring(prefixIndex + prefix.Length, postfixIndex - (prefixIndex + prefix.Length));

                isInitialPowerBIQuery = false;
                return true;
            }

            isInitialPowerBIQuery = false;
            return false;
        }

        public static PgQuery CreateInstance(string queryText, int[] parametersDataTypes, IDocumentStore documentStore)
        {
            bool isInitialPowerBIQuery = false;
            if (queryText.StartsWith("from", StringComparison.CurrentCultureIgnoreCase) ||
                IsPowerBIQuery(ref queryText, out isInitialPowerBIQuery))
            {
                return new RqlQuery(queryText, parametersDataTypes, documentStore, isInitialPowerBIQuery);
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
                    _resultColumnFormatCodes.Length) // TODO: Add support
            };
        }

        public abstract Task<ICollection<PgColumn>> Init(bool allowMultipleStatements=false);

        public abstract Task Execute(MessageBuilder builder, PipeWriter writer, CancellationToken token);

        public abstract void Dispose();

        public void Bind(IEnumerable<byte[]> parameters, short[] parameterFormatCodes, short[] resultColumnFormatCodes)
        {
            _resultColumnFormatCodes = resultColumnFormatCodes;

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