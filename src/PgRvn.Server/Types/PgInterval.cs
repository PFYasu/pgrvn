using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgInterval : IPgType
    {
        public static readonly PgInterval Default = new();
        public int Oid => PgTypeOIDs.Interval;
        public short Size => 16;
    }
}
