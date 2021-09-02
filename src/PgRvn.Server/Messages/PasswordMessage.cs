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
    class PasswordMessage : Message
    {
        public string Password;

        protected override async Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            if (!Password.Equals("12345678"))
            {
                throw new PgFatalException(PgErrorCodes.InvalidPassword, "Authentication failed, password is invalid.");
            }

            await writer.WriteAsync(messageBuilder.AuthenticationOk(), token);
        }

        protected override async Task<int> InitMessage(MessageReader messageReader, PipeReader reader, CancellationToken token, int msgLen)
        {
            var len = 0;

            var (password, passwordLength) = await messageReader.ReadNullTerminatedString(reader, token);
            len += passwordLength;

            Password = password;

            return len;
        }
    }
}
