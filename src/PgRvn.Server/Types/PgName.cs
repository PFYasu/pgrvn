using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgName : PgType
    {
        public static readonly PgName Default = new();
        public override int Oid => PgTypeOIDs.Name;
        public override short Size => 64;
        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            return Utf8GetBytes(value);
        }
    }
}
