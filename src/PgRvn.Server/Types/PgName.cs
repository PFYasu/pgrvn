using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgName : IPgType
    {
        public static readonly PgName Default = new();
        public int Oid => PgTypeOIDs.Name;
        public short Size => 64;
    }
}
