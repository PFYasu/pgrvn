using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgJson : IPgType
    {
        public static readonly PgJson Default = new();
        public int Oid => PgTypeOIDs.Json;
        public short Size => -1;
    }
}
