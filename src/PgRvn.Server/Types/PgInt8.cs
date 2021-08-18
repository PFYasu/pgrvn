using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgInt8 : PgType
    {
        public static readonly PgInt8 Default = new();
        public override int Oid => PgTypeOIDs.Int8;
        public override short Size => sizeof(long);
        public override int TypeModifier => -1;

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return Utf8GetBytes(value);
            }

            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)value));
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return long.Parse(Utf8GetString(buffer));
            }

            return IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer));
        }
    }
}
