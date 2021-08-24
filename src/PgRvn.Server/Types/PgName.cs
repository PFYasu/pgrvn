using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;

namespace PgRvn.Server.Types
{
    public class PgName : PgType
    {
        public static readonly PgName Default = new();
        public override int Oid => PgTypeOIDs.Name;
        public override short Size => 64;
        public override int TypeModifier => -1;

        public override ReadOnlyMemory<byte> ToBytes(object value, PgFormat formatCode)
        {
            return Utf8GetBytes(value);
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            return Utf8GetString(buffer);
        }

        public override object FromString(string value)
        {
            return value;
        }
    }
}
