using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Sparrow.Json;

namespace PgRvn.Server
{
    class PgTypeConverter
    {
        public delegate byte[] ToBytesDelegate(object obj);
        public delegate object FromBytesDelegate(byte[] buffer);

        private const long _pgTimestampOffsetTicks = 630822816000000000L;


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

            [(PgTypeOIDs.Bytea, PgFormat.Text)] = (obj) => (byte[])obj, // TODO: Verify it works
            [(PgTypeOIDs.Bytea, PgFormat.Binary)] = (obj) => (byte[])obj,

            [(PgTypeOIDs.Timestamp, PgFormat.Text)] = (obj) => Encoding.UTF8.GetBytes(((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss.fffffff")), // TODO: Verify it works
            [(PgTypeOIDs.Timestamp, PgFormat.Binary)] = (obj) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder(GetTimestamp((DateTime)obj))),

            [(PgTypeOIDs.TimestampTz, PgFormat.Text)] = (obj) => Encoding.UTF8.GetBytes(((DateTime)obj).ToString("yyyy-MM-dd HH:mm:ss.fffffffzz")), // TODO: Verify it works
            [(PgTypeOIDs.TimestampTz, PgFormat.Binary)] = (obj) => BitConverter.GetBytes(IPAddress.HostToNetworkOrder(GetTimestampTz((DateTime)obj))),

            [(PgTypeOIDs.Interval, PgFormat.Text)] = Utf8GetBytes, // TODO: Verify it works
            [(PgTypeOIDs.Interval, PgFormat.Binary)] = (obj) =>
            {
                var ts = (TimeSpan)obj;
                var arr = new byte[sizeof(long) + sizeof(int) + sizeof(int)];
                
                var ticksBuf = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(((ts.Ticks - ts.Days * TimeSpan.TicksPerDay) / 10)));
                var daysBuf = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(ts.Days));
                var monthsBuf = BitConverter.GetBytes(0);

                ticksBuf.CopyTo(arr, 0);
                daysBuf.CopyTo(arr, sizeof(long));
                monthsBuf.CopyTo(arr, sizeof(long) + sizeof(int));

                return arr;
            },
        };

        private static byte[] Utf8GetBytes(object obj)
        {
            return Encoding.UTF8.GetBytes(obj.ToString());
        }

        private static string Utf8GetString(byte[] buffer)
        {
            return Encoding.UTF8.GetString(buffer);
        }

        private static long GetTimestamp(DateTime timestamp)
        {
            return (timestamp.Ticks - _pgTimestampOffsetTicks) / 10;
        }

        private static DateTime GetTimestamp(string datetimeStr)
        {
            return DateTime.Parse(datetimeStr);
        }

        private static long GetTimestampTz(DateTimeOffset timestamp)
        {
            return (timestamp.Ticks - _pgTimestampOffsetTicks) / 10;
        }

        private static DateTime GetTimestampTz(string datetimeStr)
        {
            var dt = DateTime.Parse(datetimeStr);
            dt.ToUniversalTime(); // TODO: Test that this line doesn't change the DateTime, just the .Kind property
            return dt;
        }

        private static DateTime GetTimestamp(long timestamp)
        {
            return new DateTime(timestamp * 10 + _pgTimestampOffsetTicks);
        }

        private static DateTime GetTimestampTz(long timestamp)
        {
            return new DateTime(timestamp * 10 + _pgTimestampOffsetTicks, DateTimeKind.Utc);
        }

        private static object GetTimeSpan(byte[] buffer)
        {
            var pos = 0;
            var spanView = new ReadOnlySpan<byte>(buffer);
            
            var ticks = MemoryMarshal.AsRef<long>(spanView);
            pos += sizeof(long);

            var day = MemoryMarshal.AsRef<int>(spanView[pos..]);
            pos += sizeof(int);

            var month = MemoryMarshal.AsRef<int>(spanView[pos..]);
            pos += sizeof(int);

            //var ts = new TimeSpan(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer));



            // TODO: Make this work


            return new TimeSpan();
        }

        public static readonly Dictionary<(int, PgFormat), FromBytesDelegate> FromBytes = new()
        {
            // For unknown types
            [(0, PgFormat.Text)] = Utf8GetString,
            [(0, PgFormat.Binary)] = (buffer) => buffer,

            [(PgTypeOIDs.Bit, PgFormat.Text)] = (buffer) => Utf8GetString(buffer).Equals("1"), // TODO: Test if works
            [(PgTypeOIDs.Bit, PgFormat.Binary)] = (buffer) => buffer.Equals(PgConfig.TrueBuffer), // TODO: Test if works

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

            [(PgTypeOIDs.Bytea, PgFormat.Text)] = (buffer) => buffer, // TODO: Verify it works
            [(PgTypeOIDs.Bytea, PgFormat.Binary)] = (buffer) => buffer,

            [(PgTypeOIDs.Timestamp, PgFormat.Text)] = (buffer) => GetTimestamp(Utf8GetString(buffer)), // TODO: Verify it works
            [(PgTypeOIDs.Timestamp, PgFormat.Binary)] = (buffer) => GetTimestamp(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer))),

            [(PgTypeOIDs.TimestampTz, PgFormat.Text)] = (buffer) => GetTimestampTz(Utf8GetString(buffer)), // TODO: Verify it works
            [(PgTypeOIDs.TimestampTz, PgFormat.Binary)] = (buffer) => GetTimestampTz(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer))),

            [(PgTypeOIDs.Interval, PgFormat.Text)] = (buffer) => TimeSpan.Parse(Utf8GetString(buffer)), // TODO: Verify it works
            [(PgTypeOIDs.Interval, PgFormat.Binary)] = GetTimeSpan,
        };
    }
}
