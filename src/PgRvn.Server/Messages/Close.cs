using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PgRvn.Server.Messages
{
    public class Close : ExtendedProtocolMessage
    {
        public PgObjectType PgObjectType;
        public string ObjectName;

        protected override async Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            // Note: It's not an error to close a non existing named portal/statement
            if (string.IsNullOrEmpty(ObjectName))
            {
                transaction.Close();
            }

            await writer.WriteAsync(messageBuilder.CloseComplete(), token);
        }
    }
}
