using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;

namespace PgRvn.Server.Types
{
    public class PgTimestamp : PgType
    {
        public static readonly PgTimestamp Default = new();
        
        public override int Oid => PgTypeOIDs.Timestamp;
        public override short Size => 8;
        public override int TypeModifier => -1;

        public const long OffsetTicks = 630822816000000000L;
        public const int TicksMultiplier = 10;

        public override byte[] ToBytes(object value, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return Encoding.UTF8.GetBytes(((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.fffffff"));
            }

            var timestamp = GetTimestamp((DateTime) value);
            return BitConverter.GetBytes(IPAddress.HostToNetworkOrder(timestamp));
        }

        public override object FromBytes(byte[] buffer, PgFormat formatCode)
        {
            if (formatCode == PgFormat.Text)
            {
                return GetDateTime(Utf8GetString(buffer)); // TODO: Verify it works
            }

            return GetDateTime(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer)));
        }

        private static DateTime GetDateTime(long timestamp)
        {
            return new DateTime(timestamp * PgTimestamp.TicksMultiplier + PgTimestamp.OffsetTicks);
        }

        private static DateTime GetDateTime(string datetimeStr)
        {
            return DateTime.Parse(datetimeStr);
        }

        private static long GetTimestamp(DateTime timestamp)
        {
            return (timestamp.Ticks - OffsetTicks) / TicksMultiplier;
        }
    }
}
