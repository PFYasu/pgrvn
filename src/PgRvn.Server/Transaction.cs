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
        
        public ReadOnlyMemory<byte> Parse(Message message, MessageBuilder messageBuilder)
        {
            _parseMessage = (Parse)message;

            // TODO: Verify parse data and return ErrorMessage if needed

            return messageBuilder.ParseComplete();
        }

        public void Bind(Message message, MessageBuilder messageBuilder)
        {
            throw new NotImplementedException();
        }

        public void Describe(Message message, MessageBuilder messageBuilder)
        {
            throw new NotImplementedException();
        }

        public void Execute(Message message, MessageBuilder messageBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
