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
                    FormatCode = PgFormat.Text //PgFormat.Binary
                },
                new()
                {
                    Name = "typnamespace",
                    ColumnIndex = 2,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    FormatCode = PgFormat.Text //PgFormat.Binary
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
                    FormatCode = PgFormat.Text //PgFormat.Binary
                },
                new()
                {
                    Name = "typrelid",
                    ColumnIndex = 5,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    FormatCode = PgFormat.Text //PgFormat.Binary
                },
                new()
                {
                    Name = "typnotnull",
                    ColumnIndex = 6,
                    TypeObjectId = PgTypeOIDs.Bool,
                    DataTypeSize = sizeof(bool),
                    FormatCode = PgFormat.Text //PgFormat.Binary
                },
                new()
                {
                    Name = "relkind",
                    ColumnIndex = 7,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Text //PgFormat.Binary
                },
                new()
                {
                    Name = "elemtypoid",
                    TableObjectId = 0,
                    ColumnIndex = 8,
                    TypeObjectId = PgTypeOIDs.Oid,
                    DataTypeSize = sizeof(int),
                    TypeModifier = -1,
                    FormatCode = PgFormat.Text //PgFormat.Binary
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
                    FormatCode = PgFormat.Text //PgFormat.Binary
                },
                new()
                {
                    Name = "elemtyptype",
                    ColumnIndex = 11,
                    TypeObjectId = PgTypeOIDs.Char,
                    DataTypeSize = sizeof(byte),
                    TypeModifier = 1,
                    FormatCode = PgFormat.Text //PgFormat.Binary
                },
                new()
                {
                    Name = "ord",
                    ColumnIndex = 12,
                    TypeObjectId = PgTypeOIDs.Int4,
                    DataTypeSize = sizeof(int),
                    FormatCode = PgFormat.Text //PgFormat.Binary
                }
            },

            Data = new List<PgDataRow>
            {
                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2970"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("txid_snapshot"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        null,
                        null,
                        null,
                        null,
                        Encoding.UTF8.GetBytes("0"),
                    }
                }
            }

            // Data = new List<PgDataRow>
            // {
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2970)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("txid_snapshot"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)23)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("int4"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)24)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regproc"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)25)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("text"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)26)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("oid"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)27)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("tid"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)28)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("xid"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)29)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("cid"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)30)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("oidvector"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)114)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("json"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)142)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("xml"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)194)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("pg_node_tree"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3361)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("pg_ndistinct"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3402)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("pg_dependencies"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)5017)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("pg_mcv_list"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)5069)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("xid8"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)600)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("point"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)601)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("lseg"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)602)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("path"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)603)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("box"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)604)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("polygon"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)628)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("line"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)700)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("float4"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)701)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("float8"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)718)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("circle"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)790)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("money"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)829)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("macaddr"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)869)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("inet"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)650)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("cidr"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)774)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("macaddr8"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1033)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("aclitem"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1042)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("bpchar"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1043)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("varchar"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1082)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("date"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1083)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("time"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1114)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("timestamp"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1184)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("timestamptz"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1186)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("interval"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1266)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("timetz"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1560)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("bit"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1562)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("varbit"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1700)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("numeric"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1790)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("refcursor"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2202)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regprocedure"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2203)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regoper"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2204)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regoperator"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2205)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regclass"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4191)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regcollation"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2206)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regtype"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4096)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regrole"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4089)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regnamespace"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2950)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("uuid"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3220)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("pg_lsn"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3614)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("tsvector"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3642)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("gtsvector"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3615)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("tsquery"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3734)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regconfig"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3769)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("regdictionary"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3802)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("jsonb"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4072)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("jsonpath"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)22)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("int2vector"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)5038)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("pg_snapshot"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2249)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("record"),
            //             new[] { (byte)'p' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)17)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("bytea"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)16)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("bool"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2278)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("void"),
            //             new[] { (byte)'p' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)18)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("char"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)19)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("name"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)20)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("int8"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)21)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("int2"),
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             null,
            //             null,
            //             null,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3926)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("int8range"),
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)20)),
            //             Encoding.UTF8.GetBytes("int8"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3904)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("int4range"),
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)23)),
            //             Encoding.UTF8.GetBytes("int4"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3906)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("numrange"),
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1700)),
            //             Encoding.UTF8.GetBytes("numeric"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3908)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("tsrange"),
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1114)),
            //             Encoding.UTF8.GetBytes("timestamp"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3910)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("tstzrange"),
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1184)),
            //             Encoding.UTF8.GetBytes("timestamptz"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3912)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("daterange"),
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1082)),
            //             Encoding.UTF8.GetBytes("date"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13183)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("yes_or_no"),
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1043)),
            //             Encoding.UTF8.GetBytes("varchar"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13181)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("time_stamp"),
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1184)),
            //             Encoding.UTF8.GetBytes("timestamptz"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13174)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("character_data"),
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1043)),
            //             Encoding.UTF8.GetBytes("varchar"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13171)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("cardinal_number"),
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)23)),
            //             Encoding.UTF8.GetBytes("int4"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13176)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("sql_identifier"),
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)19)),
            //             Encoding.UTF8.GetBytes("name"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13182)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("_yes_or_no"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13183)),
            //             Encoding.UTF8.GetBytes("yes_or_no"),
            //             null,
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2287)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_record"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2249)),
            //             Encoding.UTF8.GetBytes("record"),
            //             null,
            //             new[] { (byte)'p' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1000)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_bool"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)16)),
            //             Encoding.UTF8.GetBytes("bool"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1001)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_bytea"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)17)),
            //             Encoding.UTF8.GetBytes("bytea"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1002)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_char"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)18)),
            //             Encoding.UTF8.GetBytes("char"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1003)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_name"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)19)),
            //             Encoding.UTF8.GetBytes("name"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1016)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_int8"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)20)),
            //             Encoding.UTF8.GetBytes("int8"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1005)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_int2"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)21)),
            //             Encoding.UTF8.GetBytes("int2"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1006)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_int2vector"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)22)),
            //             Encoding.UTF8.GetBytes("int2vector"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1007)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_int4"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)23)),
            //             Encoding.UTF8.GetBytes("int4"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1008)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regproc"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)24)),
            //             Encoding.UTF8.GetBytes("regproc"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1009)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_text"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)25)),
            //             Encoding.UTF8.GetBytes("text"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1028)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_oid"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)26)),
            //             Encoding.UTF8.GetBytes("oid"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1010)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_tid"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)27)),
            //             Encoding.UTF8.GetBytes("tid"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1011)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_xid"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)28)),
            //             Encoding.UTF8.GetBytes("xid"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1012)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_cid"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)29)),
            //             Encoding.UTF8.GetBytes("cid"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1013)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_oidvector"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)30)),
            //             Encoding.UTF8.GetBytes("oidvector"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)199)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_json"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)114)),
            //             Encoding.UTF8.GetBytes("json"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)143)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_xml"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)142)),
            //             Encoding.UTF8.GetBytes("xml"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)271)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_xid8"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)5069)),
            //             Encoding.UTF8.GetBytes("xid8"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1017)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_point"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)600)),
            //             Encoding.UTF8.GetBytes("point"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1018)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_lseg"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)601)),
            //             Encoding.UTF8.GetBytes("lseg"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1019)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_path"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)602)),
            //             Encoding.UTF8.GetBytes("path"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1020)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_box"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)603)),
            //             Encoding.UTF8.GetBytes("box"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1027)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_polygon"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)604)),
            //             Encoding.UTF8.GetBytes("polygon"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)629)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_line"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)628)),
            //             Encoding.UTF8.GetBytes("line"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1021)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_float4"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)700)),
            //             Encoding.UTF8.GetBytes("float4"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1022)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_float8"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)701)),
            //             Encoding.UTF8.GetBytes("float8"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)719)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_circle"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)718)),
            //             Encoding.UTF8.GetBytes("circle"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)791)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_money"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)790)),
            //             Encoding.UTF8.GetBytes("money"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1040)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_macaddr"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)829)),
            //             Encoding.UTF8.GetBytes("macaddr"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1041)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_inet"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)869)),
            //             Encoding.UTF8.GetBytes("inet"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)651)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_cidr"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)650)),
            //             Encoding.UTF8.GetBytes("cidr"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)775)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_macaddr8"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)774)),
            //             Encoding.UTF8.GetBytes("macaddr8"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1034)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_aclitem"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1033)),
            //             Encoding.UTF8.GetBytes("aclitem"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1014)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_bpchar"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1042)),
            //             Encoding.UTF8.GetBytes("bpchar"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1015)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_varchar"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1043)),
            //             Encoding.UTF8.GetBytes("varchar"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1182)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_date"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1082)),
            //             Encoding.UTF8.GetBytes("date"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1183)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_time"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1083)),
            //             Encoding.UTF8.GetBytes("time"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1115)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_timestamp"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1114)),
            //             Encoding.UTF8.GetBytes("timestamp"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1185)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_timestamptz"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1184)),
            //             Encoding.UTF8.GetBytes("timestamptz"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1187)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_interval"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1186)),
            //             Encoding.UTF8.GetBytes("interval"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1270)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_timetz"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1266)),
            //             Encoding.UTF8.GetBytes("timetz"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1561)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_bit"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1560)),
            //             Encoding.UTF8.GetBytes("bit"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1563)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_varbit"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1562)),
            //             Encoding.UTF8.GetBytes("varbit"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1231)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_numeric"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1700)),
            //             Encoding.UTF8.GetBytes("numeric"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2201)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_refcursor"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)1790)),
            //             Encoding.UTF8.GetBytes("refcursor"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2207)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regprocedure"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2202)),
            //             Encoding.UTF8.GetBytes("regprocedure"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2208)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regoper"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2203)),
            //             Encoding.UTF8.GetBytes("regoper"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2209)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regoperator"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2204)),
            //             Encoding.UTF8.GetBytes("regoperator"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2210)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regclass"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2205)),
            //             Encoding.UTF8.GetBytes("regclass"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4192)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regcollation"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4191)),
            //             Encoding.UTF8.GetBytes("regcollation"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2211)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regtype"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2206)),
            //             Encoding.UTF8.GetBytes("regtype"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4097)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regrole"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4096)),
            //             Encoding.UTF8.GetBytes("regrole"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4090)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regnamespace"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4089)),
            //             Encoding.UTF8.GetBytes("regnamespace"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2951)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_uuid"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2950)),
            //             Encoding.UTF8.GetBytes("uuid"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3221)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_pg_lsn"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3220)),
            //             Encoding.UTF8.GetBytes("pg_lsn"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3643)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_tsvector"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3614)),
            //             Encoding.UTF8.GetBytes("tsvector"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3644)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_gtsvector"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3642)),
            //             Encoding.UTF8.GetBytes("gtsvector"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3645)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_tsquery"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3615)),
            //             Encoding.UTF8.GetBytes("tsquery"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3735)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regconfig"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3734)),
            //             Encoding.UTF8.GetBytes("regconfig"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3770)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_regdictionary"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3769)),
            //             Encoding.UTF8.GetBytes("regdictionary"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3807)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_jsonb"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3802)),
            //             Encoding.UTF8.GetBytes("jsonb"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4073)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_jsonpath"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4072)),
            //             Encoding.UTF8.GetBytes("jsonpath"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2949)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_txid_snapshot"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)2970)),
            //             Encoding.UTF8.GetBytes("txid_snapshot"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)5039)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_pg_snapshot"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)5038)),
            //             Encoding.UTF8.GetBytes("pg_snapshot"),
            //             null,
            //             new[] { (byte)'b' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3905)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_int4range"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3904)),
            //             Encoding.UTF8.GetBytes("int4range"),
            //             null,
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3907)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_numrange"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3906)),
            //             Encoding.UTF8.GetBytes("numrange"),
            //             null,
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3909)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_tsrange"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3908)),
            //             Encoding.UTF8.GetBytes("tsrange"),
            //             null,
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3911)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_tstzrange"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3910)),
            //             Encoding.UTF8.GetBytes("tstzrange"),
            //             null,
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3913)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_daterange"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3912)),
            //             Encoding.UTF8.GetBytes("daterange"),
            //             null,
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("pg_catalog"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3927)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)11)),
            //             Encoding.UTF8.GetBytes("_int8range"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)3926)),
            //             Encoding.UTF8.GetBytes("int8range"),
            //             null,
            //             new[] { (byte)'r' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13170)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("_cardinal_number"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13171)),
            //             Encoding.UTF8.GetBytes("cardinal_number"),
            //             null,
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13173)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("_character_data"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13174)),
            //             Encoding.UTF8.GetBytes("character_data"),
            //             null,
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13175)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("_sql_identifier"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13176)),
            //             Encoding.UTF8.GetBytes("sql_identifier"),
            //             null,
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     },
            //
            //     new()
            //     {
            //         ColumnData = new ReadOnlyMemory<byte>?[]
            //         {
            //             Encoding.UTF8.GetBytes("information_schema"),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13180)),
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13158)),
            //             Encoding.UTF8.GetBytes("_time_stamp"),
            //             new[] { (byte)'a' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)0)),
            //             FalseBuffer,
            //             null,
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)13181)),
            //             Encoding.UTF8.GetBytes("time_stamp"),
            //             null,
            //             new[] { (byte)'d' },
            //             BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)4)),
            //         }
            //     }
            // }
        };
    }
}
