﻿using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
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
        public TransactionState State { get; private set; }

        private readonly SqlHandler _sqlHandler = new();
        private TSQLStatement _statement = null;
        private Parse _parseMessage = null;
        private Bind _bindMessage = null;
        private int _rowsOperated = 0;

        public Transaction()
        {
            State = TransactionState.Idle;
        }
        
        public ReadOnlyMemory<byte> Parse(Parse message, MessageBuilder messageBuilder)
        {
            _parseMessage = message;

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
            try
            {
                _statement = _sqlHandler.ParseSingleStatement(message.Query);
            }
            catch (Exception e)
            {
                State = TransactionState.Failed;
                return messageBuilder.ErrorResponse(PgSeverity.Error, 
                    PgErrorCodes.SyntaxError, 
                    "Parsing of SQL statement failed.", 
                    e.Message);
            }

            // TODO: Verify data and return ErrorMessage if needed (and change transaction state)

            State = TransactionState.InTransaction;
            return messageBuilder.ParseComplete();
        }

        public ReadOnlyMemory<byte> Bind(Bind message, MessageBuilder messageBuilder)
        {
            if (IsTransactionInactive(messageBuilder, out var errorResponse))
                return errorResponse;

            // TODO: Verify data, bind parameters using _parseMessage and return ErrorMessage if needed
            _bindMessage = message;

            return messageBuilder.BindComplete();
        }

        public ReadOnlyMemory<byte> Describe(Describe message, MessageBuilder messageBuilder)
        {
            if (IsTransactionInactive(messageBuilder, out var errorResponse))
                return errorResponse;

            // TODO: Send NoData if portal does not contain a query that will return rows

            try
            {
                var columns = _sqlHandler.Describe(_statement);

                switch (message.PgObjectType)
                {
                    case PgObjectType.Portal:
                        return messageBuilder.RowDescription(columns);
                    case PgObjectType.PreparedStatement:
                        throw new NotImplementedException("Prepared statements handling aren't implemented yet.");
                    default:
                        throw new InvalidOperationException("Unsupported PgObjectType.");
                }
            }
            catch (Exception e)
            {
                // TODO: Handle error
                throw;
                // State = TransactionState.Failed;
                // return messageBuilder.ErrorResponse(PgSeverity.Error,
                //     PgErrorCodes.,
                //     "Describe phase failed.",
                //     e.Message);
            }
        }

        public ReadOnlyMemory<byte> Execute(Message message, MessageBuilder messageBuilder)
        {
            if (IsTransactionInactive(messageBuilder, out var errorResponse))
                return errorResponse;

            var data = _sqlHandler.Execute(_statement, out _rowsOperated);
            return messageBuilder.DataRows(data);
        }

        public ReadOnlyMemory<byte> CommandComplete(MessageBuilder messageBuilder)
        {
            if (IsTransactionInactive(messageBuilder, out var errorResponse))
                return errorResponse;

            string tag = null;
            if (_statement.AsInsert != null)
            {
                // OID is always 0
                tag = "INSERT 0";
            }
            else if (_statement.AsDelete != null)
            {
                tag = "DELETE";
            }
            else if (_statement.AsUpdate != null)
            {
                tag = "UPDATE";
            }
            else if (_statement.AsSelect != null)
            {
                tag = "SELECT";
            }
            else
            {
                State = TransactionState.Failed;
                return messageBuilder.ErrorResponse(PgSeverity.Error,
                    PgErrorCodes.FeatureNotSupported,
                    "Statement tag could not be determined.",
                    "Statement: " + _statement);
            }
            // TODO: Support these tags. See CommandComplete in https://www.postgresql.org/docs/current/protocol-message-formats.html
            //else if (_statement.AsMove != null)
            //{
            //    tag = "MOVE";
            //}
            //else if (_statement.AsFetch != null)
            //{
            //    tag = "FETCH";
            //}
            //else if (_statement.AsCopy != null)
            //{
            //    tag = "COPY";
            //}

            State = TransactionState.Idle;
            return messageBuilder.CommandComplete($"{tag} {_rowsOperated}");
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
