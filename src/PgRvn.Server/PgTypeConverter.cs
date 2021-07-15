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
        public delegate object FromBytesDelegate(byte[] buffer);

        public static readonly Dictionary<(int, PgFormat), ToBytesDelegate> ToBytes = new()
        {
            [(PgTypeOIDs.Bool, PgFormat.Text)] = (obj) => (bool)obj ? Utf8GetBytes("t") : Utf8GetBytes("f"), // TODO confirm this works
            [(PgTypeOIDs.Bool, PgFormat.Binary)] = (obj) => (bool)obj ? PgConfig.TrueBuffer : PgConfig.FalseBuffer,

            [(PgTypeOIDs.Text, PgFormat.Text)] = Utf8GetBytes,
            [(PgTypeOIDs.Text, PgFormat.Binary)] = Utf8GetBytes,

            [(PgTypeOIDs.Json, PgFormat.Text)] = Utf8GetBytes,
            [(PgTypeOIDs.Json, PgFormat.Binary)] = Utf8GetBytes,

            [(PgTypeOIDs.Int2, PgFormat.Text)] = Utf8GetBytes,
            [(PgTypeOIDs.Int2, PgFormat.Binary)] = (obj) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)obj)),

            [(PgTypeOIDs.Int4, PgFormat.Text)] = Utf8GetBytes,
            [(PgTypeOIDs.Int4, PgFormat.Binary)] = (obj) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)obj)),

            [(PgTypeOIDs.Int8, PgFormat.Text)] = Utf8GetBytes,
            [(PgTypeOIDs.Int8, PgFormat.Binary)] = (obj) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder((long)obj)),

            [(PgTypeOIDs.Float8, PgFormat.Text)] = Utf8GetBytes,
            [(PgTypeOIDs.Float8, PgFormat.Binary)] = (obj) => BitConverter.GetBytes((double)obj).Reverse().ToArray(),
        };

        private static byte[] Utf8GetBytes(object obj)
        {
            return Encoding.UTF8.GetBytes(obj.ToString());
        }

        private static string Utf8GetString(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }

        public static readonly Dictionary<(int, PgFormat), FromBytesDelegate> FromBytes = new()
        {
            [(PgTypeOIDs.Bool, PgFormat.Text)] = (buffer) => Utf8GetString(buffer).Equals("t"),
            [(PgTypeOIDs.Bool, PgFormat.Binary)] = (buffer) => buffer.Equals(PgConfig.TrueBuffer),

            [(PgTypeOIDs.Text, PgFormat.Text)] = Utf8GetString,
            [(PgTypeOIDs.Text, PgFormat.Binary)] = Utf8GetString,

            [(PgTypeOIDs.Json, PgFormat.Text)] = Utf8GetString,
            [(PgTypeOIDs.Json, PgFormat.Binary)] = Utf8GetString,

            [(PgTypeOIDs.Int2, PgFormat.Text)] = (buffer) => short.Parse(Utf8GetString(buffer)),
            [(PgTypeOIDs.Int2, PgFormat.Binary)] = (buffer) => IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer)),

            [(PgTypeOIDs.Int4, PgFormat.Text)] = (buffer) => int.Parse(Utf8GetString(buffer)),
            [(PgTypeOIDs.Int4, PgFormat.Binary)] = (buffer) => IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer)),

            [(PgTypeOIDs.Int8, PgFormat.Text)] = (buffer) => long.Parse(Utf8GetString(buffer)),
            [(PgTypeOIDs.Int8, PgFormat.Binary)] = (buffer) => IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer)),

            [(PgTypeOIDs.Float4, PgFormat.Text)] = (buffer) => float.Parse(Utf8GetString(buffer)),
            [(PgTypeOIDs.Float4, PgFormat.Binary)] = (buffer) => BitConverter.ToSingle(buffer.Reverse().ToArray()),

            [(PgTypeOIDs.Float8, PgFormat.Text)] = (buffer) => double.Parse(Utf8GetString(buffer)),
            [(PgTypeOIDs.Float8, PgFormat.Binary)] = (buffer) => BitConverter.ToDouble(buffer.Reverse().ToArray()),
        };
    }
}
