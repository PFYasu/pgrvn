using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Messages
{
    public interface IInitialMessage
    {
    }

    public class StartupMessage : IInitialMessage
    {
        public ProtocolVersion ProtocolVersion;
        public Dictionary<string, string> ClientOptions;
    }

    public class Cancel : IInitialMessage
    {
        public int ProcessId;
        public int SessionId;
    }

    public class SSLRequest : IInitialMessage
    {
    }
}
