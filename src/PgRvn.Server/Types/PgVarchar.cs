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
        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            throw new NotImplementedException("Converting Varchar to bytes is not implemented."); // TODO
        }
    }
}
