using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;

namespace PgRvn.Server.Types
{
    public class PgInt2 : PgType
    {
        public static readonly PgInt2 Default = new();
        public override int Oid => PgTypeOIDs.Int2;
        public override short Size => sizeof(short);
        public override int TypeModifier { get; }

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return Utf8GetBytes(value);
            }

            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)value));
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return short.Parse(Utf8GetString(buffer));
            }

            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer));
        }
    }
}
