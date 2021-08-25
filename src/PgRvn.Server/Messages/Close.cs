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
    public class Close : ExtendedProtocolMessage
    {
        public PgObjectType PgObjectType;
        public string ObjectName;

        protected override async Task<int> InitMessage(MessageReader messageReader, PipeReader reader, CancellationToken token, int msgLen)
        {
            var len = 0;

            var objectType = await messageReader.ReadByteAsync(reader, token);
            len += sizeof(byte);

            var pgObjectType = objectType switch
            {
                (byte)PgObjectType.Portal => PgObjectType.Portal,
                (byte)PgObjectType.PreparedStatement => PgObjectType.PreparedStatement,
                _ => throw new PgFatalException(PgErrorCodes.ProtocolViolation,
                    "Expected valid object type ('S' or 'P') but got: '" + objectType)
            };

            var (objectName, objectNameLength) = await messageReader.ReadNullTerminatedString(reader, token);
            len += objectNameLength;

            PgObjectType = pgObjectType;
            ObjectName = objectName;

            return len;
        }

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
