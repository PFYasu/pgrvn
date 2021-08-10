using Raven.Client.Documents;
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
    public enum TransactionState : byte
    {
        Idle = (byte)'I',
        InTransaction = (byte)'T',
        Failed = (byte)'E'
    }

    class Transaction
    {
        public TransactionState State { get; private set; } = TransactionState.Idle;
        public IDocumentStore DocumentStore { get; }
        public PgQuery CurrentQuery;
        
        private static readonly Regex _paramRegex = new(@"(?<=(\$[0-9]))(::[A-Za-z0-9]+)?", RegexOptions.Compiled);

        public Transaction(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }
        
        public ReadOnlyMemory<byte> Parse(Parse message, MessageBuilder messageBuilder)
        {
            if (!string.IsNullOrEmpty(message.StatementName))
            {
                State = TransactionState.Failed;
                return messageBuilder.ErrorResponse(PgSeverity.Error,
                    PgErrorCodes.FeatureNotSupported,
                    "Named statements are not supported.");
            }

            // Extract optional parameter types (e.g. $1::int4)
            var foundParamTypes = new List<string>();
            // todo: add _ to supported matches (e.g. for stuff like big_int if they ever exist)
            var cleanQueryText = _paramRegex.Replace(message.Query, new MatchEvaluator((Match match) =>
            {
                foundParamTypes.Add(match.Value);
                return "";
            }));

            if (message.ParametersDataTypes.Length < foundParamTypes.Count)
            {
                var arr = message.ParametersDataTypes;
                message.ParametersDataTypes = new int[foundParamTypes.Count];
                arr.CopyTo(message.ParametersDataTypes.AsSpan());
            }

            for (int i = 0; i < foundParamTypes.Count; i++)
            {
                if (message.ParametersDataTypes[i] == 0)
                {
                    message.ParametersDataTypes[i] = foundParamTypes[i] switch
                    {
                        "::int8" => PgTypeOIDs.Int8,
                        "::bytea" => PgTypeOIDs.Bytea,
                        "::bit" => PgTypeOIDs.Bit,
                        "::timestamptz" => PgTypeOIDs.TimestampTz,
                        "::timestamp" => PgTypeOIDs.Timestamp,
                        "::interval" => PgTypeOIDs.Interval,
                        _ => throw new PgErrorException(PgErrorCodes.AmbiguousParameter, 
                                "Couldn't determine parameter type, try explicitly providing it in your query " +
                                "(e.g. from Orders where Freight = $1::double)")
                    };
                }
            }

            // Change $1 to $p1 because RQL doesn't accept numeric named paramters
            // TODO: Remove this once project is integrated into raven
            cleanQueryText = new Regex(@"(?<=\$)([0-9])").Replace(cleanQueryText, "p$0");

            CurrentQuery = PgQuery.CreateInstance(cleanQueryText, message.ParametersDataTypes, DocumentStore);

            State = TransactionState.InTransaction;
            return messageBuilder.ParseComplete();
        }

        public ReadOnlyMemory<byte> Bind(Bind message, MessageBuilder messageBuilder)
        {
            if (IsTransactionInactive(messageBuilder, out var errorResponse))
                return errorResponse;

            // TODO: Support named statements/portals
            if (!string.IsNullOrEmpty(message.StatementName) || !string.IsNullOrEmpty(message.PortalName))
            {
                State = TransactionState.Failed;
                return messageBuilder.ErrorResponse(PgSeverity.Error,
                    PgErrorCodes.FeatureNotSupported,
                    "Named statements/portals are not supported.");
            }

            if (message.ParameterFormatCodes.Length != message.Parameters.Count &&
                message.ParameterFormatCodes.Length != 0 &&
                message.ParameterFormatCodes.Length != 1)
            {
                State = TransactionState.Failed;
                return messageBuilder.ErrorResponse(PgSeverity.Error,
                    PgErrorCodes.ProtocolViolation,
                    $"Parameter format code amount is {message.ParameterFormatCodes.Length} when expected " +
                    $"to be 0, 1 or equal to the parameters count {message.Parameters.Count}.");
            }

            CurrentQuery.Bind(message.Parameters, message.ParameterFormatCodes, message.ResultColumnFormatCodes);
            
            return messageBuilder.BindComplete();
        }

        public async Task<ReadOnlyMemory<byte>> Describe(Describe message, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            if (IsTransactionInactive(messageBuilder, out var errorResponse))
                return errorResponse;

            if (!string.IsNullOrEmpty(message.ObjectName))
            {
                State = TransactionState.Failed;
                return messageBuilder.ErrorResponse(PgSeverity.Error,
                    PgErrorCodes.FeatureNotSupported,
                    "Describe: Named statements/portals are not supported.");
            }

            var schema = await CurrentQuery.Init();

            if (message.PgObjectType == PgObjectType.PreparedStatement)
            {
                await writer.WriteAsync(messageBuilder.ParameterDescription(CurrentQuery.ParametersDataTypes), token);
            }

            return schema.Count == 0 ? messageBuilder.NoData() : messageBuilder.RowDescription(schema);
        }

        public async Task<ReadOnlyMemory<byte>> Execute(MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            await CurrentQuery.Execute(messageBuilder, writer, token);

            State = TransactionState.Idle;
            return default;
        }

        public async Task<ReadOnlyMemory<byte>> Query(Query message, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            // TODO: Support multiple statements in one query
            var query = PgQuery.CreateInstance(message.QueryString, null, DocumentStore);

            var schema = await query.Init(true);
            if (schema.Count != 0)
            {
                await writer.WriteAsync(messageBuilder.RowDescription(schema), token);
            }

            await query.Execute(messageBuilder, writer, token);

            await writer.WriteAsync(messageBuilder.ReadyForQuery(State), token);
            return default;
        }

        public ReadOnlyMemory<byte> Close(Close message, MessageBuilder messageBuilder)
        {
            // Note: It's not an error to close a non existing named portal/statement
            if (string.IsNullOrEmpty(message.ObjectName))
            {
                State = TransactionState.Idle;
            }
            
            return messageBuilder.CloseComplete();
        }

        public async Task<ReadOnlyMemory<byte>> Flush(PipeWriter writer, CancellationToken token)
        {
            await writer.FlushAsync(token);
            return default;
        }

        public ReadOnlyMemory<byte> Sync(MessageBuilder messageBuilder)
        {
            State = TransactionState.Idle;
            return messageBuilder.ReadyForQuery(State);
        }

        private bool IsTransactionInactive(MessageBuilder messageBuilder, out ReadOnlyMemory<byte> errorResponse)
        {
            switch (State)
            {
                case TransactionState.Idle:
                    errorResponse = messageBuilder.ErrorResponse(PgSeverity.Error,
                        PgErrorCodes.NoActiveSqlTransaction,
                        "Message was sent when no transaction is taking place.");
                    return true;
                case TransactionState.Failed:
                    errorResponse = messageBuilder.ErrorResponse(PgSeverity.Error,
                        PgErrorCodes.InFailedSqlTransaction,
                        "Message was sent when transaction has failed.");
                    return true;
                default:
                    errorResponse = null;
                    return false;
            }
        }
    }
}
