using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Sparrow.Json;

namespace PgRvn.Server
{
    class PgTypeConverter
    {
        public delegate byte[] ToBytesDelegate(object obj);
        
        public static readonly Dictionary<(int, PgFormat), ToBytesDelegate> ToBytes = new()
        {
            [(PgTypeOIDs.Bool, PgFormat.Text)] = (obj) => (bool)obj ? ToUtf8("t") : ToUtf8("f"), // TODO confirm this works
            [(PgTypeOIDs.Bool, PgFormat.Binary)] = (obj) => (bool)obj ? PgConfig.TrueBuffer : PgConfig.FalseBuffer,

            [(PgTypeOIDs.Text, PgFormat.Text)] = ToUtf8,
            [(PgTypeOIDs.Text, PgFormat.Binary)] = ToUtf8,

            [(PgTypeOIDs.Json, PgFormat.Text)] = ToUtf8,
            [(PgTypeOIDs.Json, PgFormat.Binary)] = ToUtf8,

            [(PgTypeOIDs.Int8, PgFormat.Text)] = ToUtf8,
            [(PgTypeOIDs.Int8, PgFormat.Binary)] = (obj) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)obj)),

            [(PgTypeOIDs.Float8, PgFormat.Text)] = ToUtf8,
            [(PgTypeOIDs.Float8, PgFormat.Binary)] = (obj) => BitConverter.GetBytes((double)obj).Reverse().ToArray(),

            [(PgTypeOIDs.Text, PgFormat.Text)] = ToUtf8,
            [(PgTypeOIDs.Text, PgFormat.Binary)] = ToUtf8,

            [(PgTypeOIDs.Json, PgFormat.Text)] = ToUtf8,
            [(PgTypeOIDs.Json, PgFormat.Binary)] = ToUtf8,
        };

        private static byte[] ToUtf8(object obj)
        {
            return Encoding.UTF8.GetBytes(obj.ToString());
        }
    }
}
