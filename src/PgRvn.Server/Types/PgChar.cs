using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgChar : IPgType
    {
        public static readonly PgChar Default = new();
        public int Oid => PgTypeOIDs.Char;
        public short Size => sizeof(byte);
    }
}
