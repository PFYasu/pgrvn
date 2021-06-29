using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    class Transaction
    {
        private Parse _parseMessage = null;

        public Transaction()
        {
        }
        
        public ReadOnlyMemory<byte> Parse(Parse message, MessageBuilder messageBuilder)
        {
            _parseMessage = message;

            // TODO: Verify data and return ErrorMessage if needed

            return messageBuilder.ParseComplete();
        }

        public ReadOnlyMemory<byte> Bind(Bind message, MessageBuilder messageBuilder)
        {
            // TODO: Verify data and return ErrorMessage if needed

            return messageBuilder.BindComplete();
        }

        public ReadOnlyMemory<byte> Describe(Describe message, MessageBuilder messageBuilder)
        {

            throw new NotImplementedException();
        }

        public ReadOnlyMemory<byte> Execute(Message message, MessageBuilder messageBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
