using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PgRvn.Server.Exceptions;

namespace PgRvn.Server.Messages
{
    public class Execute : ExtendedProtocolMessage
    {
        public string PortalName;
        public int MaxRows;

        protected override async Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            if (transaction.State == TransactionState.Idle)
                throw new PgErrorException(PgErrorCodes.NoActiveSqlTransaction,
                    "Execute message was received when no transaction is taking place.");

            await transaction.Execute(messageBuilder, writer, token);
        }
    }
}
