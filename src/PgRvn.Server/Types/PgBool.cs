using PgRvn.Server.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;

namespace PgRvn.Server.Types
{
    public class PgBool : PgType
    {
        public static readonly PgBool Default = new();
        public override int Oid => PgTypeOIDs.Bool;
        public override short Size => sizeof(byte);
        public override int TypeModifier => -1;
        
        public static byte[] TrueBuffer = { 1 }, FalseBuffer = { 0 };

        public override ReadOnlyMemory<byte> ToBytes(object value, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return (bool)value ? Utf8GetBytes("t") : Utf8GetBytes("f");
            }

            return (bool)value ? TrueBuffer : FalseBuffer;
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return FromString(Utf8GetString(buffer));
            }

            return buffer.Equals(TrueBuffer);
        }

        public override object FromString(string value)
        {
            return value.Equals("t");
        }
    }
}
