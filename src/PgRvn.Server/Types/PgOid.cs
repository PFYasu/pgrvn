using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgOid : PgType
    {
        public static readonly PgOid Default = new();
        public override int Oid => PgTypeOIDs.Oid;
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
            throw new NotImplementedException(); // TODO
        }
    }
}
