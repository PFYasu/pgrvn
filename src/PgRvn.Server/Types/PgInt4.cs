using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgInt4 : IPgType
    {
        public static readonly PgInt4 Default = new();
        public int Oid => PgTypeOIDs.Int4;
        public short Size => sizeof(int);
    }
}
