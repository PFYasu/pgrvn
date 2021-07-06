using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TSQL.Statements;

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

        public Query CurrentQuery;


        public Transaction(IDocumentStore documentStore)
        {
            DocumentStore = documentStore;
        }
        
        public ReadOnlyMemory<byte> Parse(Parse message, MessageBuilder messageBuilder)
        {
            // TODO: Support named statements, see: https://www.postgresql.org/docs/13/protocol-flow.html#PROTOCOL-FLOW-EXT-QUERY
            if (!string.IsNullOrEmpty(message.StatementName))
            {
                State = TransactionState.Failed;
                return messageBuilder.ErrorResponse(PgSeverity.Error,
                    PgErrorCodes.FeatureNotSupported,
                    "Named statements are not supported.");
            }

            // 1. A parameter data type can be left unspecified by setting it to zero, or by making the array of parameter type OIDs
            //    shorter than the number of parameter symbols ($n) used in the query string.
            // 2. Another special case is that a parameter's type can be specified as the OID of void.
            //    This is meant to allow parameter symbols to be used for function parameters that are actually OUT parameters.
            //    Ordinarily there is no context in which a void parameter could be used, but if such a parameter symbol appears
            //    in a function's parameter list, it is effectively ignored. For example, a function call such as foo($1,$2,$3,$4)
            //    could match a function with two IN and two OUT arguments, if $3 and $4 are specified as having type void.

            // TODO: We currently ignore parameter data types if they are sent. This is probably fine.

            // TODO: Pass message.Parameters
            CurrentQuery?.Session?.Dispose();
            CurrentQuery = new Query
            {
                QueryText = message.Query.Replace("$", "$p"), // TODO: Remove this once RavenDB-16956 is merged
                Session = DocumentStore.OpenAsyncSession(),
                ParametersDataTypes = message.ParametersDataTypes
            };

            // TODO: Verify data and return ErrorMessage if needed (and change transaction state)

            State = TransactionState.InTransaction;
            return messageBuilder.ParseComplete();
        }

        public ReadOnlyMemory<byte> Bind(Bind message, MessageBuilder messageBuilder)
        {
            if (IsTransactionInactive(messageBuilder, out var errorResponse))
                return errorResponse;

            CurrentQuery.Parameters ??= new Dictionary<string, object>();

            // TODO: Handle the rest of the Bind's data (parameter type info, output type info)

            int i = 0;
            foreach (var parameter in message.Parameters)
            {
                int dataType = 0;
                if (i < CurrentQuery.ParametersDataTypes.Length)
                {
                    dataType = CurrentQuery.ParametersDataTypes[i];
                }

                object processedParameter = dataType switch
                {
                    PgTypeOIDs.Bool => Query.TrueBuffer.SequenceEqual(parameter),
                    PgTypeOIDs.Text => Encoding.UTF8.GetString(parameter),
                    PgTypeOIDs.Json => Encoding.UTF8.GetString(parameter),
                    PgTypeOIDs.Int8 => IPAddress.NetworkToHostOrder(BitConverter.ToInt64(parameter)),
                    PgTypeOIDs.Float8 => BitConverter.ToDouble(parameter.Reverse().ToArray()),
                    _ => parameter
                };

                CurrentQuery.Parameters.Add($"p{i+1}", processedParameter);
                i++;
            }

            // TODO: return ErrorMessage if needed

            return messageBuilder.BindComplete();
        }

        public async Task<ReadOnlyMemory<byte>> Describe(MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            await CurrentQuery.Init(messageBuilder, writer, token);
            return default;
        }

        public async Task<ReadOnlyMemory<byte>> Execute(MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            await CurrentQuery.Execute(messageBuilder, writer, token);
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
                        "Bind was called when no transaction is taking place.");
                    return true;
                case TransactionState.Failed:
                    errorResponse = messageBuilder.ErrorResponse(PgSeverity.Error,
                        PgErrorCodes.InFailedSqlTransaction,
                        "Bind was called when transaction has failed.");
                    return true;
                default:
                    errorResponse = null;
                    return false;
            }
        }
    }
}
