using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;

namespace PgRvn.Server.Types
{
    public class PgChar : PgType
    {
        public static readonly PgChar Default = new();
        public override int Oid => PgTypeOIDs.Char;
        public override short Size => sizeof(byte);
        public override int TypeModifier => -1;

        public override ReadOnlyMemory<byte> ToBytes(object value, PgFormat formatCode)
        {
            return Utf8GetBytes(value); // TODO: Verify this works
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            return Utf8GetString(buffer); // TODO: Verify this works
        }

        public override object FromString(string value)
        {
            return value;
        }
    }
}
