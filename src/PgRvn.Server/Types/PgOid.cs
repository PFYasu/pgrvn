using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgOid : IPgType
    {
        public static readonly PgOid Default = new();
        public int Oid => PgTypeOIDs.Oid;
        public short Size => sizeof(int);
    }
}
