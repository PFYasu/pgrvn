using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgVarchar : PgType
    {
        public static readonly PgVarchar Default = new();
        public override int Oid => PgTypeOIDs.Varchar;
        public override short Size => -1;
        public override int TypeModifier => -1;

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            var str = value.ToString() ?? string.Empty;
            if (TypeModifier != -1 && str.Length > TypeModifier)
            {
                throw new PgErrorException(PgErrorCodes.StringDataRightTruncation,
                    $"Value too long ({str.Length}) for type character varying({TypeModifier})");
            }

            return Utf8GetBytes(value);
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            var str = Utf8GetString(buffer);
            if (TypeModifier != -1 && str.Length > TypeModifier)
            {
                throw new PgErrorException(PgErrorCodes.StringDataRightTruncation, 
                    $"Converted value too long ({str.Length}) for type character varying({TypeModifier})");
            }

            return str;
        }
    }
}
