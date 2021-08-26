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
    public class Query : Message
    {
        public string QueryString;

        protected override async Task<int> InitMessage(MessageReader messageReader, PipeReader reader, CancellationToken token, int msgLen)
        {
            var len = 0;

            var (queryString, queryStringLength) = await messageReader.ReadNullTerminatedString(reader, token);
            len += queryStringLength;

            QueryString = queryString;

            return len;
        }

        protected override async Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            // TODO: Support multiple SELECT statements in one query
            using var query = PgQuery.CreateInstance(QueryString, null, transaction.DocumentStore);

            var schema = await query.Init(true);
            if (schema.Count != 0)
            {
                await writer.WriteAsync(messageBuilder.RowDescription(schema), token);
            }

            await query.Execute(messageBuilder, writer, token);
            await writer.WriteAsync(messageBuilder.ReadyForQuery(transaction.State), token);
        }

        public override async Task HandleError(PgErrorException e, Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            await base.HandleError(e, transaction, messageBuilder, writer, token);
            await writer.WriteAsync(messageBuilder.ReadyForQuery(transaction.State), token);
        }
    }
}
