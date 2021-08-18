using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgInt8 : IPgType
    {
        public static readonly PgInt8 Default = new();
        public int Oid => PgTypeOIDs.Int8;
        public short Size => sizeof(long);
    }
}
