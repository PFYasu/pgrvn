using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgBool : IPgType
    {
        public static readonly PgBool Default = new();
        public int Oid => PgTypeOIDs.Bool;
        public short Size => sizeof(byte);
    }
}
