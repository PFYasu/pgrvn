using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server.Messages
{
    public class Sync : ExtendedProtocolMessage
    {
        protected override async Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            transaction.Sync();
            await writer.WriteAsync(messageBuilder.ReadyForQuery(transaction.State), token);
        }
    }
}
