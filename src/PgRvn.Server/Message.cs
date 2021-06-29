using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public enum MessageType : byte
    {
        // Received
        Parse = (byte)'P',
        Bind = (byte)'B',
        Describe = (byte)'D',
        Execute = (byte)'E',

        // Sent
        ParameterStatus = (byte)'S',
        BackendKeyData = (byte)'K',
        AuthenticationOk = (byte)'R',
        ReadyForQuery = (byte)'Z',

        ParseComplete = (byte)'1',
        BindComplete = (byte)'2',
    }

    public abstract class Message
    {
        public abstract MessageType Type { get; }
    }

    public class Parse : Message
    {
        public override MessageType Type => MessageType.Parse;
        public string StatementName;
        public string Query;
        public int[] Parameters;
    }

    public class Bind : Message
    {
        public override MessageType Type => MessageType.Bind;
        public string PortalName;
        public string StatementName;
        public short[] ParameterFormatCodes;
        public List<byte[]> Parameters;
        public short[] ResultColumnFormatCodes;
    }

    public class Describe : Message
    {
        public override MessageType Type => MessageType.Describe;
    }

    public class Execute : Message
    {
        public override MessageType Type => MessageType.Execute;
    }
}
