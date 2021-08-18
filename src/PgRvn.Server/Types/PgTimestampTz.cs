using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server.Types
{
    public class PgTimestampTz : PgType
    {
        public static readonly PgTimestampTz Default = new();
        public override int Oid => PgTypeOIDs.TimestampTz;
        public override short Size => 8;
        public override int TypeModifier => -1;

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return Encoding.UTF8.GetBytes(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fffffffzz"));
            }

            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(GetTimestampTz((DateTime)value)));
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return GetDateTimeOffset(Utf8GetString(buffer)); // TODO: Verify it works
            }

            return GetDateTime(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer)));
        }

        private static DateTime GetDateTime(long timestamp)
        {
            return new DateTime(timestamp * PgTimestamp.TicksMultiplier + PgTimestamp.OffsetTicks, DateTimeKind.Utc);
        }

        private static DateTimeOffset GetDateTimeOffset(string datetimeStr)
        {
            var dt = DateTimeOffset.Parse(datetimeStr).ToOffset(TimeSpan.Zero);
            return dt;
        }

        // TODO: Try using PgTimestamp's function instead, but make sure that the conversions don't break anything
        private static long GetTimestampTz(DateTimeOffset timestamp)
        {
            return (timestamp.Ticks - PgTimestamp.OffsetTicks) / PgTimestamp.TicksMultiplier;
        }
    }
}
