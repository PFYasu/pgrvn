using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;

namespace PgRvn.Server.Types
{
    public class PgInt4 : PgType
    {
        public static readonly PgInt4 Default = new();
        public override int Oid => PgTypeOIDs.Int4;
        public override short Size => sizeof(int);
        public override int TypeModifier => -1;

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return Utf8GetBytes(value);
            }

            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)value));
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return int.Parse(Utf8GetString(buffer));
            }

            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer));
        }
    }
}
