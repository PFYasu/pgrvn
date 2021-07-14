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
                    ColumnIndex = 0,
                    TypeObjectId = PgTypeOIDs.Name,
                    DataTypeSize = -1,
                    FormatCode = PgFormat.Text
                },
                new()
                {
                    Name = "oid",
                    ColumnIndex = 1,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typnamespace",
                    ColumnIndex = 2,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typname",
                    ColumnIndex = 3,
                    TypeObjectId = PgTypeOIDs.Name,
                    DataTypeSize = -1,
                    FormatCode = PgFormat.Text
                },
                new()
                {
                    Name = "typtype",
                    ColumnIndex = 4,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typrelid",
                    ColumnIndex = 5,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "typnotnull",
                    ColumnIndex = 6,
                    TypeObjectId = PgTypeOIDs.Bool,
                    DataTypeSize = sizeof(bool),
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "relkind",
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
                    ColumnIndex = 9,
                    TypeObjectId = PgTypeOIDs.Name,
                    DataTypeSize = -1,
                    FormatCode = PgFormat.Text
                },
                new()
                {
                    Name = "elemrelkind",
                    ColumnIndex = 10,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "elemtyptype",
                    ColumnIndex = 11,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Binary
                },
                new()
                {
                    Name = "ord",
                    ColumnIndex = 12,
                    TypeObjectId = PgTypeOIDs.Int4,
                    DataTypeSize = sizeof(int),
                    FormatCode = PgFormat.Binary
                }
            },
            Data = new List<PgDataRow>
            {
                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
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

        // public static readonly PgTable NpgsqlInitialQueryResponse = new()
        // {
        //     Columns = new List<PgColumn>
        //     {
        //         new()
        //         {
        //             Name = "nspname",
        //             TypeObjectId = PgTypeOIDs.Name,
        //             FormatCode = PgFormat.Text
        //         },
        //         new()
        //         {
        //             Name = "oid",
        //             TypeObjectId = PgTypeOIDs.Oid,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "typnamespace",
        //             TypeObjectId = PgTypeOIDs.Oid,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "typname",
        //             TypeObjectId = PgTypeOIDs.Name,
        //             TypeModifier = -1,
        //             FormatCode = PgFormat.Text
        //         },
        //         new()
        //         {
        //             Name = "typtype",
        //             TypeObjectId = PgTypeOIDs.Char,
        //             TypeModifier = 1,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "typrelid",
        //             TypeObjectId = PgTypeOIDs.Oid,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "typnotnull",
        //             TypeObjectId = PgTypeOIDs.Bool,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "relkind",
        //             TypeObjectId = PgTypeOIDs.Char,
        //             TypeModifier = 1,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "elemtypoid",
        //             TypeObjectId = PgTypeOIDs.Oid,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "elemtypname",
        //             TypeObjectId = PgTypeOIDs.Name,
        //             FormatCode = PgFormat.Text
        //         },
        //         new()
        //         {
        //             Name = "elemrelkind",
        //             TypeObjectId = PgTypeOIDs.Char,
        //             TypeModifier = 1,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "elemtyptype",
        //             TypeObjectId = PgTypeOIDs.Char,
        //             TypeModifier = 1,
        //             FormatCode = PgFormat.Binary
        //         },
        //         new()
        //         {
        //             Name = "elemtyptype",
        //             TypeObjectId = PgTypeOIDs.Int4,
        //             FormatCode = PgFormat.Binary
        //         }
        //     },
        //     Data = new List<PgDataRow>
        //     {
        //         new()
        //         {
        //             ColumnData = new ReadOnlyMemory<byte>?[]
        //             {
        //                 Encoding.UTF8.GetBytes("pg_catalog"),
        //                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2970)),
        //                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
        //                 Encoding.UTF8.GetBytes("txid_snapshot"),
        //                 new[] { (byte)'b' },
        //                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
        //                 FalseBuffer,
        //                 null,
        //                 null,
        //                 null,
        //                 null,
        //                 null,
        //                 BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
        //             }
        //         }
        //     }
        // };
    }
}
