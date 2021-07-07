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

            CurrentQuery?.Session?.Dispose();
            CurrentQuery = new PgQuery
            {
                QueryText = message.Query.Replace("$", "$p"), // TODO: Remove this once RavenDB-16956 is merged
                Session = DocumentStore.OpenAsyncSession(),
                ParametersDataTypes = message.ParametersDataTypes
            };

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

            short? defaultDataFormat = null;
            switch (message.ParameterFormatCodes.Length)
            {
                case 0:
                    defaultDataFormat = (short)PgFormat.Text;
                    break;
                case 1:
                    defaultDataFormat = message.ParameterFormatCodes[0];
                    break;
                default:
                    if (message.ParameterFormatCodes.Length != message.Parameters.Count)
                    {
                        State = TransactionState.Failed;
                        return messageBuilder.ErrorResponse(PgSeverity.Error,
                            PgErrorCodes.ProtocolViolation,
                            $"Parameter format code amount is {message.ParameterFormatCodes.Length} when expected " +
                            $"to be 0, 1 or equal to the parameters count {message.Parameters.Count}.");
                    }
                    break;
            }

            CurrentQuery.Parameters ??= new Dictionary<string, object>();

            // Note: We ignore message.ResultColumnFormatCodes

            int i = 0;
            foreach (var parameter in message.Parameters)
            {
                int dataType = 0;
                if (i < CurrentQuery.ParametersDataTypes.Length)
                {
                    dataType = CurrentQuery.ParametersDataTypes[i];
                }

                short dataFormat = defaultDataFormat ?? message.ParameterFormatCodes[i];
                object processedParameter = (dataType, dataFormat) switch
                {
                    (PgTypeOIDs.Bool, (short)PgFormat.Binary) => PgQuery.TrueBuffer.SequenceEqual(parameter),
                    (PgTypeOIDs.Text, (short)PgFormat.Text) => Encoding.UTF8.GetString(parameter),
                    (PgTypeOIDs.Json, (short)PgFormat.Text) => Encoding.UTF8.GetString(parameter),
                    (PgTypeOIDs.Int2, (short) PgFormat.Binary) => IPAddress.NetworkToHostOrder(BitConverter.ToInt16(parameter)),
                    (PgTypeOIDs.Int4, (short) PgFormat.Binary) => IPAddress.NetworkToHostOrder(BitConverter.ToInt32(parameter)),
                    (PgTypeOIDs.Int8, (short) PgFormat.Binary) => IPAddress.NetworkToHostOrder(BitConverter.ToInt64(parameter)),
                    (PgTypeOIDs.Float4, (short)PgFormat.Binary) => BitConverter.ToSingle(parameter.Reverse().ToArray()),
                    (PgTypeOIDs.Float8, (short)PgFormat.Binary) => BitConverter.ToDouble(parameter.Reverse().ToArray()),
                    // (PgTypeOIDs.Char, (short)PgFormat.Binary) => BitConverter.ToChar(parameter.Reverse().ToArray()), // TODO: Test char support
                    _ => parameter
                };

                CurrentQuery.Parameters.Add($"p{i+1}", processedParameter);
                i++;
            }

            // TODO: return ErrorMessage if needed

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

            CurrentQuery = null;
            State = TransactionState.Idle;

            return default;
        }

        public async Task<ReadOnlyMemory<byte>> Query(Query query, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            // TODO: Handle query
            // await writer.WriteAsync(messageBuilder.CommandComplete("SET 1"), token);
            await writer.WriteAsync(messageBuilder.EmptyQueryResponse(), token);
            await writer.WriteAsync(messageBuilder.ReadyForQuery(State), token);
            return default;
        }

        public ReadOnlyMemory<byte> Close(Close message, MessageBuilder messageBuilder)
        {
            // Note: It's not an error to close a non existing named portal/statement
            if (string.IsNullOrEmpty(message.ObjectName))
            {
                CurrentQuery = null;
                State = TransactionState.Idle;
            }
            
            return messageBuilder.CloseComplete();
        }

        public async Task<ReadOnlyMemory<byte>> Flush(PipeWriter writer, CancellationToken token)
        {
            await writer.FlushAsync(token);
            return default;
        }

        // public ReadOnlyMemory<byte> CommandComplete(MessageBuilder messageBuilder)
        // {
        //     if (IsTransactionInactive(messageBuilder, out var errorResponse))
        //         return errorResponse;
        //
        //     string tag = null;
        //     if (_statement.AsInsert != null)
        //     {
        //         // OID is always 0
        //         tag = "INSERT 0";
        //     }
        //     else if (_statement.AsDelete != null)
        //     {
        //         tag = "DELETE";
        //     }
        //     else if (_statement.AsUpdate != null)
        //     {
        //         tag = "UPDATE";
        //     }
        //     else if (_statement.AsSelect != null)
        //     {
        //         tag = "SELECT";
        //     }
        //     else
        //     {
        //         State = TransactionState.Failed;
        //         return messageBuilder.ErrorResponse(PgSeverity.Error,
        //             PgErrorCodes.FeatureNotSupported,
        //             "Statement tag could not be determined.",
        //             "Statement: " + _statement);
        //     }
        //     // TODO: Support these tags. See CommandComplete in https://www.postgresql.org/docs/current/protocol-message-formats.html
        //     //else if (_statement.AsMove != null)
        //     //{
        //     //    tag = "MOVE";
        //     //}
        //     //else if (_statement.AsFetch != null)
        //     //{
        //     //    tag = "FETCH";
        //     //}
        //     //else if (_statement.AsCopy != null)
        //     //{
        //     //    tag = "COPY";
        //     //}
        //
        //     return messageBuilder.CommandComplete($"{tag} {_rowsOperated}");
        // }

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
