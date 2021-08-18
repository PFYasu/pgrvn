using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgUnknown : PgType
    {
        public static readonly PgUnknown Default = new();
        public override int Oid => PgTypeOIDs.Unknown;
        public override short Size => -1;
        public override int TypeModifier => -1;

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            return Utf8GetBytes(value);
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            return buffer;
        }
    }
}
