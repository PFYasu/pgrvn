using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgText : IPgType
    {
        public static readonly PgText Default = new();
        public int Oid => PgTypeOIDs.Text;
        public short Size => -1;
    }
}
