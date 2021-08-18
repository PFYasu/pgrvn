using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgFloat8 : IPgType
    {
        public static readonly PgFloat8 Default = new();
        public int Oid => PgTypeOIDs.Float8;
        public short Size => sizeof(double);
    }
}
