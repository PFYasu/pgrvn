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
    public class Bind : ExtendedProtocolMessage
    {
        public string PortalName;
        public string StatementName;
        public short[] ParameterFormatCodes;
        public List<byte[]> Parameters;
        public short[] ResultColumnFormatCodes;

        protected override async Task HandleMessage(Transaction transaction, MessageBuilder messageBuilder, PipeWriter writer, CancellationToken token)
        {
            // TODO: Support named statements/portals
            if (!string.IsNullOrEmpty(StatementName) || !string.IsNullOrEmpty(PortalName))
            {
                throw new PgErrorException(PgErrorCodes.FeatureNotSupported,
                    "Named statements/portals are not supported.");
            }

            if (ParameterFormatCodes.Length != Parameters.Count &&
                ParameterFormatCodes.Length != 0 &&
                ParameterFormatCodes.Length != 1)
            {
                throw new PgErrorException(PgErrorCodes.ProtocolViolation,
                    $"Parameter format code amount is {ParameterFormatCodes.Length} when expected " +
                    $"to be 0, 1 or equal to the parameters count {Parameters.Count}.");
            }

            // TODO: Consider moving some of the logic from PgQuery.Bind into here if logical
            transaction.Bind(Parameters, ParameterFormatCodes, ResultColumnFormatCodes);
            await writer.WriteAsync(messageBuilder.BindComplete(), token);
        }
    }
}
