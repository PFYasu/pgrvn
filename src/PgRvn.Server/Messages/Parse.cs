﻿using PgRvn.Server.Types;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PgRvn.Server.Exceptions;

namespace PgRvn.Server.Messages
{
    public class Parse : ExtendedProtocolMessage
    {
        public string StatementName;
        public string Query;

        /// <summary>
        /// Object ID number of parameter data types specified (can be zero).
        /// </summary>
        /// <remarks>
        /// Note that this is not an indication of the number of parameters that might appear
        /// in the query string, only the number that the frontend wants to prespecify types for.
        /// </remarks>
        public int[] ParametersDataTypes;

        private static readonly Regex ParamRegex = new Regex(@"(?<=(?:\$[0-9]))(?:::(?<type>[A-Za-z0-9]+))?", RegexOptions.Compiled);

        protected override async Task<int> InitMessage(MessageReader messageReader, PipeReader reader, CancellationToken token, int msgLen)
        {
            var len = 0;

            var (statementName, statementLength) = await messageReader.ReadNullTerminatedString(reader, token);
            len += statementLength;

            var (query, queryLength) = await messageReader.ReadNullTerminatedString(reader, token);
            len += queryLength;

            var parametersCount = await messageReader.ReadInt16Async(reader, token);
            len += sizeof(short);

            var parameters = new int[parametersCount];
            for (var i = 0; i < parametersCount; i++)
            {
                parameters[i] = await messageReader.ReadInt32Async(reader, token);
                len += sizeof(int);
            }

            StatementName = statementName;
            Query = query;
            ParametersDataTypes = parameters;

            return len;
        }

        protected override async Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(StatementName))
            {
                throw new PgErrorException(PgErrorCodes.FeatureNotSupported, "Named statements are not supported.");
            }

            // Extract optional parameter types (e.g. $1::int4)
            var foundParamTypes = new List<string>();
            var cleanQueryText = ParamRegex.Replace(Query, new MatchEvaluator((Match match) =>
            {
                foundParamTypes.Add(match.Groups["type"].Value);
                return "";
            }));

            if (ParametersDataTypes.Length < foundParamTypes.Count)
            {
                var arr = ParametersDataTypes;
                ParametersDataTypes = new int[foundParamTypes.Count];
                arr.CopyTo(ParametersDataTypes.AsSpan());
            }

            for (int i = 0; i < foundParamTypes.Count; i++)
            {
                if (ParametersDataTypes[i] == 0)
                {
                    ParametersDataTypes[i] = PgType.Parse(foundParamTypes[i]).Oid;
                }
            }

            // Change $1 to $p1 because RQL doesn't accept numeric named paramters
            // TODO: Remove this once project is integrated into raven
            cleanQueryText = new Regex(@"(?<=\$)([0-9])").Replace(cleanQueryText, "p$0");

            transaction.Init(cleanQueryText, ParametersDataTypes);
            await writer.WriteAsync(messageBuilder.ParseComplete(), token);
        }
    }
}
