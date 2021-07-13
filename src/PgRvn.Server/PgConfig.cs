using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public static class PgConfig
    {
        public static readonly Dictionary<string, string> ParameterStatusList = new()
        {
            ["client_encoding"] = "UTF8",
            ["server_encoding"] = "UTF8",
            ["server_version"] = "13.3",
            ["application_name"] = "",
            ["DataStyle"] = "ISO, DMY",
            ["integer_datetimes"] = "on",
            ["IntervalStyle"] = "postgres",
            ["is_superuser"] = "on", // TODO
            ["session_authorization"] = "postgres",
            ["standard_conforming_strings"] = "on",
            ["TimeZone"] = "Asia/Jerusalem", // TODO
        };

        public static byte[] TrueBuffer = { 1 }, FalseBuffer = { 0 };

        public static readonly PgTable NpgsqlInitialQueryResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new()
                {
                    Name = "nspname",
                    TableObjectId = 0,
                    ColumnIndex = 0,
                    TypeObjectId = PgTypeOIDs.Name,
                    DataTypeSize = -1,
                    TypeModifier = -1,
                    FormatCode = PgFormat.Text
                },
                new()
                {
                    Name = "oid",
                    TableObjectId = 0,
                    ColumnIndex = 1,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    TypeModifier = -1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typnamespace",
                    TableObjectId = 0,
                    ColumnIndex = 2,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    TypeModifier = -1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typname",
                    TableObjectId = 0,
                    ColumnIndex = 3,
                    TypeObjectId = PgTypeOIDs.Name,
                    DataTypeSize = -1,
                    TypeModifier = -1,
                    FormatCode = PgFormat.Text
                },
                new()
                {
                    Name = "typtype",
                    TableObjectId = 0,
                    ColumnIndex = 4,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typrelid",
                    TableObjectId = 0,
                    ColumnIndex = 5,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    TypeModifier = -1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typnotnull",
                    TableObjectId = 0,
                    ColumnIndex = 6,
                    TypeObjectId = PgTypeOIDs.Bool,
                    DataTypeSize = sizeof(bool),
                    TypeModifier = -1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "relkind",
                    TableObjectId = 0,
                    ColumnIndex = 7,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "elemtypoid",
                    TableObjectId = 0,
                    ColumnIndex = 8,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    TypeModifier = -1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "elemtypname",
                    TableObjectId = 0,
                    ColumnIndex = 9,
                    TypeObjectId = PgTypeOIDs.Name,
                    DataTypeSize = -1,
                    TypeModifier = -1,
                    FormatCode = PgFormat.Text
                },
                new()
                {
                    Name = "elemrelkind",
                    TableObjectId = 0,
                    ColumnIndex = 10,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "elemtyptype",
                    TableObjectId = 0,
                    ColumnIndex = 11,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "elemtyptype",
                    TableObjectId = 0,
                    ColumnIndex = 12,
                    TypeObjectId = PgTypeOIDs.Int4,
                    DataTypeSize = sizeof(int),
                    TypeModifier = -1,
                    FormatCode = PgFormat.Binary
                }
            },
            Data = new List<PgDataRow>
            {
                new()
                {
                    ColumnData = new List<Memory<byte>>
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2970)),
                        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
                        Encoding.UTF8.GetBytes("txid_snapshot"),
                        new[] { (byte)'b' },
                        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
                        FalseBuffer,
                        null,
                        null,
                        null,
                        null,
                        null,
                        BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
                    }
                }
            }
        };
    }
}
