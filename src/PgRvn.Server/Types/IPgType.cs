using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public interface IPgType
    {
        public int Oid { get; }
        public short Size { get; }
    }
}
