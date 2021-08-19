using PgRvn.Server.Exceptions;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server.Messages
{
    public abstract class ExtendedProtocolMessage : Message
    {
        public override async Task Handle(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            if (transaction.State == TransactionState.Failed && this is not Sync)
                return;

            await base.Handle(transaction, messageBuilder, writer, token);
        }

        public override async Task HandleError(PgErrorException e, Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            transaction.Fail();
            await base.HandleError(e, transaction, messageBuilder, writer, token);
        }
    }
}
