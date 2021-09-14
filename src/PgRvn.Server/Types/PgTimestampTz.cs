﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;

namespace PgRvn.Server.Types
{
    public class PgTimestampTz : PgType
    {
        public static readonly PgTimestampTz Default = new();
        public override int Oid => PgTypeOIDs.TimestampTz;
        public override short Size => 8;
        public override int TypeModifier => -1;

        public override ReadOnlyMemory<byte> ToBytes(object value, PgFormat formatCode)
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
                return FromString(Utf8GetString(buffer)); // TODO: Verify it works
            }

            return GetDateTime(IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer)));
        }

        public override object FromString(string value)
        {
            return DateTimeOffset.Parse(value).ToOffset(TimeSpan.Zero);
        }

        private static DateTime GetDateTime(long timestamp)
        {
            return new DateTime(timestamp * PgTimestamp.TicksMultiplier + PgTimestamp.OffsetTicks, DateTimeKind.Utc);
        }

        private static long GetTimestampTz(DateTimeOffset timestamp)
        {
            return (timestamp.Ticks - PgTimestamp.OffsetTicks) / PgTimestamp.TicksMultiplier;
        }
    }
}
