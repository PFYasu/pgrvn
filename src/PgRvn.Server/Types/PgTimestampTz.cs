using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgTimestampTz : IPgType
    {
        public static readonly PgTimestampTz Default = new();
        public int Oid => PgTypeOIDs.TimestampTz;
        public short Size => 8;
    }
}
