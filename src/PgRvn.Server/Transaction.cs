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

            CurrentQuery = PgQuery.CreateInstance(message.Query.Replace("$", "$p"), message.ParametersDataTypes, DocumentStore);

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

            CurrentQuery.Bind(message.Parameters, message.ParameterFormatCodes);

            // Note: We ignore message.ResultColumnFormatCodes

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

            // todo: handle EmptyQueryResponse

            State = TransactionState.Idle;
            return default;
        }

        public async Task<ReadOnlyMemory<byte>> Query(Query message, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            // TODO: Handle query
            var query = PgQuery.CreateInstance(message.QueryString, null, DocumentStore);

            var schema = await query.Init();
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
