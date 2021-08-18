using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgTimestamp : IPgType
    {
        public static readonly PgTimestamp Default = new();
        public int Oid => PgTypeOIDs.Timestamp;
        public short Size => 8;
    }
}
