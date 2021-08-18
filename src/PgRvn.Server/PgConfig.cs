using System;
using System.Collections.Generic;
using System.Text;
using PgRvn.Server.Types;

namespace PgRvn.Server
{
    public static class PgConfig
    {
        // TODO: Customize these
        // TODO: Support changing these after startup by the user
        public static readonly Dictionary<string, string> ParameterStatusList = new(StringComparer.OrdinalIgnoreCase)
        {
            ["client_encoding"] = "UTF8",
            ["server_encoding"] = "UTF8", // Cannot be modified after startup
            ["server_version"] = "13.3", // Cannot be modified after startup
            ["application_name"] = "",
            ["DataStyle"] = "ISO, DMY",
            ["integer_datetimes"] = "on", // Cannot be modified after startup
            ["IntervalStyle"] = "postgres",
            ["is_superuser"] = "on",
            ["session_authorization"] = "postgres",
            ["standard_conforming_strings"] = "on",
            ["TimeZone"] = "UTC",
        };

        public static readonly PgTable NpgsqlInitialQueryResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("nspname", 0, PgName.Default, PgFormat.Text),
                new PgColumn("oid", 1, PgOid.Default, PgFormat.Text),
                new PgColumn("typnamespace", 2, PgOid.Default, PgFormat.Text),
                new PgColumn("typname", 3, PgName.Default, PgFormat.Text),
                new PgColumn("typtype", 4, PgChar.Default, PgFormat.Text, 1),
                new PgColumn("typrelid", 5, PgOid.Default, PgFormat.Text),
                new PgColumn("typnotnull", 6, PgBool.Default, PgFormat.Text),
                new PgColumn("relkind", 7, PgChar.Default, PgFormat.Text, 1),
                new PgColumn("elemtypoid", 8, PgOid.Default, PgFormat.Text),
                new PgColumn("elemtypname", 9, PgName.Default, PgFormat.Text),
                new PgColumn("elemrelkind", 10, PgChar.Default, PgFormat.Text, 1),
                new PgColumn("elemtyptype", 11, PgChar.Default, PgFormat.Text, 1),
                new PgColumn("ord", 12, PgInt4.Default, PgFormat.Text),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("int4"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("24"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regproc"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("25"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("text"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("26"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("oid"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("27"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("tid"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("28"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("xid"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("29"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("cid"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("30"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("oidvector"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("114"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("json"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("142"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("xml"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("194"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("pg_node_tree"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3361"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("pg_ndistinct"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3402"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("pg_dependencies"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("5017"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("pg_mcv_list"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("5069"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("xid8"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("600"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("point"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("601"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("lseg"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("602"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("path"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("603"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("box"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("604"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("polygon"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("628"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("line"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("700"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("float4"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("701"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("float8"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("718"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("circle"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("790"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("money"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("829"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("macaddr"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("869"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("inet"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("650"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("cidr"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("774"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("macaddr8"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1033"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("aclitem"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1042"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("bpchar"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("varchar"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1082"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("date"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1083"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("time"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1114"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("timestamp"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("timestamptz"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1186"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("interval"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1266"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("timetz"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1560"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("bit"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1562"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("varbit"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1700"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("numeric"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1790"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("refcursor"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2202"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regprocedure"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2203"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regoper"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2204"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regoperator"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2205"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regclass"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4191"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regcollation"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2206"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regtype"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4096"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regrole"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4089"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regnamespace"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2950"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("uuid"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3220"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("pg_lsn"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3614"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("tsvector"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3642"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("gtsvector"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3615"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("tsquery"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3734"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regconfig"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3769"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("regdictionary"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3802"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("jsonb"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4072"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("jsonpath"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("22"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("int2vector"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("5038"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("pg_snapshot"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2249"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("record"),
                        Encoding.UTF8.GetBytes("p"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        null,
                        null,
                        null,
                        null,
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("17"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("bytea"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("16"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("bool"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2278"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("void"),
                        Encoding.UTF8.GetBytes("p"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        null,
                        null,
                        null,
                        null,
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("18"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("char"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("19"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("name"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("20"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("int8"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("21"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("int2"),
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
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3926"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("int8range"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("20"),
                        Encoding.UTF8.GetBytes("int8"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3904"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("int4range"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("int4"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3906"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("numrange"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1700"),
                        Encoding.UTF8.GetBytes("numeric"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3908"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("tsrange"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1114"),
                        Encoding.UTF8.GetBytes("timestamp"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3910"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("tstzrange"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("timestamptz"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3912"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("daterange"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1082"),
                        Encoding.UTF8.GetBytes("date"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13183"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("yes_or_no"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("varchar"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13181"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("time_stamp"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("timestamptz"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13174"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("character_data"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("varchar"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13171"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("cardinal_number"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("int4"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13176"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("sql_identifier"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("19"),
                        Encoding.UTF8.GetBytes("name"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13182"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("_yes_or_no"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("13183"),
                        Encoding.UTF8.GetBytes("yes_or_no"),
                        null,
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2287"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_record"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2249"),
                        Encoding.UTF8.GetBytes("record"),
                        null,
                        Encoding.UTF8.GetBytes("p"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1000"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_bool"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("16"),
                        Encoding.UTF8.GetBytes("bool"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1001"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_bytea"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("17"),
                        Encoding.UTF8.GetBytes("bytea"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1002"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_char"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("18"),
                        Encoding.UTF8.GetBytes("char"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1003"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_name"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("19"),
                        Encoding.UTF8.GetBytes("name"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1016"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_int8"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("20"),
                        Encoding.UTF8.GetBytes("int8"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1005"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_int2"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("21"),
                        Encoding.UTF8.GetBytes("int2"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1006"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_int2vector"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("22"),
                        Encoding.UTF8.GetBytes("int2vector"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1007"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_int4"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("int4"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1008"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regproc"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("24"),
                        Encoding.UTF8.GetBytes("regproc"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1009"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_text"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("25"),
                        Encoding.UTF8.GetBytes("text"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1028"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_oid"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("26"),
                        Encoding.UTF8.GetBytes("oid"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1010"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_tid"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("27"),
                        Encoding.UTF8.GetBytes("tid"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1011"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_xid"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("28"),
                        Encoding.UTF8.GetBytes("xid"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1012"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_cid"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("29"),
                        Encoding.UTF8.GetBytes("cid"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1013"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_oidvector"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("30"),
                        Encoding.UTF8.GetBytes("oidvector"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("199"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_json"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("114"),
                        Encoding.UTF8.GetBytes("json"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("143"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_xml"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("142"),
                        Encoding.UTF8.GetBytes("xml"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("271"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_xid8"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("5069"),
                        Encoding.UTF8.GetBytes("xid8"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1017"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_point"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("600"),
                        Encoding.UTF8.GetBytes("point"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1018"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_lseg"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("601"),
                        Encoding.UTF8.GetBytes("lseg"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1019"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_path"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("602"),
                        Encoding.UTF8.GetBytes("path"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1020"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_box"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("603"),
                        Encoding.UTF8.GetBytes("box"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1027"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_polygon"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("604"),
                        Encoding.UTF8.GetBytes("polygon"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("629"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_line"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("628"),
                        Encoding.UTF8.GetBytes("line"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1021"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_float4"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("700"),
                        Encoding.UTF8.GetBytes("float4"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1022"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_float8"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("701"),
                        Encoding.UTF8.GetBytes("float8"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("719"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_circle"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("718"),
                        Encoding.UTF8.GetBytes("circle"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("791"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_money"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("790"),
                        Encoding.UTF8.GetBytes("money"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1040"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_macaddr"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("829"),
                        Encoding.UTF8.GetBytes("macaddr"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1041"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_inet"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("869"),
                        Encoding.UTF8.GetBytes("inet"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("651"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_cidr"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("650"),
                        Encoding.UTF8.GetBytes("cidr"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("775"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_macaddr8"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("774"),
                        Encoding.UTF8.GetBytes("macaddr8"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1034"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_aclitem"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1033"),
                        Encoding.UTF8.GetBytes("aclitem"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1014"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_bpchar"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1042"),
                        Encoding.UTF8.GetBytes("bpchar"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1015"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_varchar"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("varchar"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1182"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_date"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1082"),
                        Encoding.UTF8.GetBytes("date"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1183"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_time"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1083"),
                        Encoding.UTF8.GetBytes("time"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1115"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_timestamp"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1114"),
                        Encoding.UTF8.GetBytes("timestamp"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1185"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_timestamptz"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("timestamptz"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1187"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_interval"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1186"),
                        Encoding.UTF8.GetBytes("interval"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1270"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_timetz"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1266"),
                        Encoding.UTF8.GetBytes("timetz"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1561"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_bit"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1560"),
                        Encoding.UTF8.GetBytes("bit"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1563"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_varbit"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1562"),
                        Encoding.UTF8.GetBytes("varbit"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("1231"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_numeric"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1700"),
                        Encoding.UTF8.GetBytes("numeric"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2201"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_refcursor"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("1790"),
                        Encoding.UTF8.GetBytes("refcursor"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2207"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regprocedure"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2202"),
                        Encoding.UTF8.GetBytes("regprocedure"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2208"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regoper"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2203"),
                        Encoding.UTF8.GetBytes("regoper"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2209"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regoperator"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2204"),
                        Encoding.UTF8.GetBytes("regoperator"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2210"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regclass"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2205"),
                        Encoding.UTF8.GetBytes("regclass"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4192"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regcollation"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("4191"),
                        Encoding.UTF8.GetBytes("regcollation"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2211"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regtype"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2206"),
                        Encoding.UTF8.GetBytes("regtype"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4097"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regrole"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("4096"),
                        Encoding.UTF8.GetBytes("regrole"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4090"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regnamespace"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("4089"),
                        Encoding.UTF8.GetBytes("regnamespace"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2951"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_uuid"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2950"),
                        Encoding.UTF8.GetBytes("uuid"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3221"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_pg_lsn"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3220"),
                        Encoding.UTF8.GetBytes("pg_lsn"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3643"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_tsvector"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3614"),
                        Encoding.UTF8.GetBytes("tsvector"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3644"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_gtsvector"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3642"),
                        Encoding.UTF8.GetBytes("gtsvector"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3645"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_tsquery"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3615"),
                        Encoding.UTF8.GetBytes("tsquery"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3735"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regconfig"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3734"),
                        Encoding.UTF8.GetBytes("regconfig"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3770"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_regdictionary"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3769"),
                        Encoding.UTF8.GetBytes("regdictionary"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3807"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_jsonb"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3802"),
                        Encoding.UTF8.GetBytes("jsonb"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("4073"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_jsonpath"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("4072"),
                        Encoding.UTF8.GetBytes("jsonpath"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("2949"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_txid_snapshot"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("2970"),
                        Encoding.UTF8.GetBytes("txid_snapshot"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("5039"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_pg_snapshot"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("5038"),
                        Encoding.UTF8.GetBytes("pg_snapshot"),
                        null,
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3905"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_int4range"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3904"),
                        Encoding.UTF8.GetBytes("int4range"),
                        null,
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3907"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_numrange"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3906"),
                        Encoding.UTF8.GetBytes("numrange"),
                        null,
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3909"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_tsrange"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3908"),
                        Encoding.UTF8.GetBytes("tsrange"),
                        null,
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3911"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_tstzrange"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3910"),
                        Encoding.UTF8.GetBytes("tstzrange"),
                        null,
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3913"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_daterange"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3912"),
                        Encoding.UTF8.GetBytes("daterange"),
                        null,
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("3927"),
                        Encoding.UTF8.GetBytes("11"),
                        Encoding.UTF8.GetBytes("_int8range"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("3926"),
                        Encoding.UTF8.GetBytes("int8range"),
                        null,
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13170"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("_cardinal_number"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("13171"),
                        Encoding.UTF8.GetBytes("cardinal_number"),
                        null,
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13173"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("_character_data"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("13174"),
                        Encoding.UTF8.GetBytes("character_data"),
                        null,
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13175"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("_sql_identifier"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("13176"),
                        Encoding.UTF8.GetBytes("sql_identifier"),
                        null,
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("13180"),
                        Encoding.UTF8.GetBytes("13158"),
                        Encoding.UTF8.GetBytes("_time_stamp"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("f"),
                        null,
                        Encoding.UTF8.GetBytes("13181"),
                        Encoding.UTF8.GetBytes("time_stamp"),
                        null,
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("4"),
                    }
                },
            }
        };
    }
}
