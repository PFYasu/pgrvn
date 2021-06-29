using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
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
        public TransactionState State = TransactionState.Idle;

        private Parse _parseMessage = null;
        private Bind _bindMessage = null;

        public Transaction()
        {
        }
        
        public ReadOnlyMemory<byte> Parse(Parse message, MessageBuilder messageBuilder)
        {
            _parseMessage = message;

            // TODO: Verify data and return ErrorMessage if needed (and change transaction state)

            State = TransactionState.InTransaction;
            return messageBuilder.ParseComplete();
        }

        public ReadOnlyMemory<byte> Bind(Bind message, MessageBuilder messageBuilder)
        {
            // TODO: Verify data, bind parameters using _parseMessage and return ErrorMessage if needed
            _bindMessage = message;
            return messageBuilder.BindComplete();
        }

        public ReadOnlyMemory<byte> Describe(Describe message, MessageBuilder messageBuilder)
        {
            // TODO: Send response
            if (message.PgObjectType == PgObjectType.Portal)
            {
                return messageBuilder.RowDescription(new []
                {
                    new PgColumn
                    {
                        Name = "?column?",
                        TableObjectId = 0,
                        ColumnIndex = 0,
                        TypeObjectId = 23, // int
                        DataTypeSize = sizeof(int),
                        TypeModifier = -1,
                        FormatCode = PgFormat.Binary
                    }
                });
            }
            else if (message.PgObjectType == PgObjectType.PreparedStatement)
            {
            }
            else
            {
            }

            return ReadOnlyMemory<byte>.Empty;
        }

        public ReadOnlyMemory<byte> Execute(Message message, MessageBuilder messageBuilder)
        {
            Memory<byte> buffer = new byte[sizeof(int)];
            var asInts = MemoryMarshal.Cast<byte, int>(buffer.Span);
            asInts[0] = IPAddress.HostToNetworkOrder(1);

            State = TransactionState.Idle;

            return messageBuilder.DataRows(new []
            {
                new PgColumnData
                {
                    Data = buffer
                }
            });
        }

        public ReadOnlyMemory<byte> CommandComplete(MessageBuilder messageBuilder)
        {
            return messageBuilder.CommandComplete("SELECT 1");
        }
    }
}
