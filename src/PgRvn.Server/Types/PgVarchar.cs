using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgVarchar : IPgType
    {
        public static readonly PgVarchar Default = new();
        public int Oid => PgTypeOIDs.Varchar;
        public short Size => -1;
    }
}
