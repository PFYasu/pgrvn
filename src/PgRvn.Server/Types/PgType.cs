using PgRvn.Server.Exceptions;
using PgRvn.Server.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public abstract class PgType
    {
        public abstract int Oid { get; }
        public abstract short Size { get; }
        public abstract int TypeModifier { get; }

        public abstract ReadOnlyMemory<byte> ToBytes(object value, PgFormat formatCode);
        public abstract object FromBytes(byte[] buffer, PgFormat formatCode);

        public static PgType Parse(int dataType) // todo: int typeModifier
        {
            return dataType switch
            {
                PgTypeOIDs.Bool => PgBool.Default,
                PgTypeOIDs.Bytea => PgBytea.Default,
                PgTypeOIDs.Char => PgChar.Default,
                PgTypeOIDs.Float8 => PgFloat8.Default,
                PgTypeOIDs.Int2 => PgInt2.Default,
                PgTypeOIDs.Int4 => PgInt4.Default,
                PgTypeOIDs.Int8 => PgInt8.Default,
                PgTypeOIDs.Interval => PgInterval.Default,
                PgTypeOIDs.Json => PgJson.Default,
                PgTypeOIDs.Name => PgName.Default,
                PgTypeOIDs.Oid => PgOid.Default,
                PgTypeOIDs.Text => PgText.Default,
                PgTypeOIDs.Timestamp => PgTimestamp.Default,
                PgTypeOIDs.TimestampTz => PgTimestampTz.Default,
                PgTypeOIDs.Varchar => PgVarchar.Default,
                _ => PgUnknown.Default
            };
        }

        public static PgType Parse(string dataType)
        {
            // TODO: Make sure these are the actual possible strings of each type
            return dataType switch
            {
                "bool" => PgBool.Default,
                "bytea" => PgBytea.Default,
                "char" => PgChar.Default,
                "float8" => PgFloat8.Default,
                "int2" => PgInt2.Default,
                "int4" => PgInt4.Default,
                "int8" => PgInt8.Default,
                "interval" => PgInterval.Default,
                "json" => PgJson.Default,
                "name" => PgName.Default,
                "oid" => PgOid.Default,
                "text" => PgText.Default,
                "timestamp" => PgTimestamp.Default,
                "timestamptz" => PgTimestampTz.Default,
                "varchar" => PgVarchar.Default,
                _ => throw new PgErrorException(PgErrorCodes.AmbiguousParameter,
                                "Couldn't determine parameter type, try explicitly providing it in your query " +
                                "(e.g. from Orders where Freight = $1::double)")
            };
        }
        
        protected byte[] Utf8GetBytes(object value)
        {
            return Encoding.UTF8.GetBytes(value.ToString() ?? string.Empty);
        }

        protected string Utf8GetString(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
