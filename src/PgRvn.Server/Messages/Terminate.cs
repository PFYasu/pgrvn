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
    public class Terminate : Message
    {
        protected override Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            throw new PgTerminateReceivedException();
        }
    }
}
