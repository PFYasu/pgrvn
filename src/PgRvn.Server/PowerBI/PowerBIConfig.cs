using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgRvn.Server
{
    public static class PowerBIConfig
    {
        public static readonly string TypesQuery = "\n/*** Load all supported types ***/\nSELECT ns.nspname, a.typname, a.oid, a.typrelid, a.typbasetype,\nCASE WHEN pg_proc.proname='array_recv' THEN 'a' ELSE a.typtype END AS type,\nCASE\n  WHEN pg_proc.proname='array_recv' THEN a.typelem\n  WHEN a.typtype='r' THEN rngsubtype\n  ELSE 0\nEND AS elemoid,\nCASE\n  WHEN pg_proc.proname IN ('array_recv','oidvectorrecv') THEN 3    /* Arrays last */\n  WHEN a.typtype='r' THEN 2                                        /* Ranges before */\n  WHEN a.typtype='d' THEN 1                                        /* Domains before */\n  ELSE 0                                                           /* Base types first */\nEND AS ord\nFROM pg_type AS a\nJOIN pg_namespace AS ns ON (ns.oid = a.typnamespace)\nJOIN pg_proc ON pg_proc.oid = a.typreceive\nLEFT OUTER JOIN pg_class AS cls ON (cls.oid = a.typrelid)\nLEFT OUTER JOIN pg_type AS b ON (b.oid = a.typelem)\nLEFT OUTER JOIN pg_class AS elemcls ON (elemcls.oid = b.typrelid)\nLEFT OUTER JOIN pg_range ON (pg_range.rngtypid = a.oid) \nWHERE\n  a.typtype IN ('b', 'r', 'e', 'd') OR         /* Base, range, enum, domain */\n  (a.typtype = 'c' AND cls.relkind='c') OR /* User-defined free-standing composites (not table composites) by default */\n  (pg_proc.proname='array_recv' AND (\n    b.typtype IN ('b', 'r', 'e', 'd') OR       /* Array of base, range, enum, domain */\n    (b.typtype = 'p' AND b.typname IN ('record', 'void')) OR /* Arrays of special supported pseudo-types */\n    (b.typtype = 'c' AND elemcls.relkind='c')  /* Array of user-defined free-standing composites (not table composites) */\n  )) OR\n  (a.typtype = 'p' AND a.typname IN ('record', 'void'))  /* Some special supported pseudo-types */\nORDER BY ord";
        public static readonly string CompositeTypesQuery = "/*** Load field definitions for (free-standing) composite types ***/\nSELECT typ.oid, att.attname, att.atttypid\nFROM pg_type AS typ\nJOIN pg_namespace AS ns ON (ns.oid = typ.typnamespace)\nJOIN pg_class AS cls ON (cls.oid = typ.typrelid)\nJOIN pg_attribute AS att ON (att.attrelid = typ.typrelid)\nWHERE\n  (typ.typtype = 'c' AND cls.relkind='c') AND\n  attnum > 0 AND     /* Don't load system attributes */\n  NOT attisdropped\nORDER BY typ.oid, att.attnum";
        public static readonly string EnumTypesQuery = "/*** Load enum fields ***/\nSELECT pg_type.oid, enumlabel\nFROM pg_enum\nJOIN pg_type ON pg_type.oid=enumtypid\nORDER BY oid, enumsortorder";
        public static readonly string TableSchemaQuery = "select\r\n    pkcol.COLUMN_NAME as PK_COLUMN_NAME,\r\n    fkcol.TABLE_SCHEMA AS FK_TABLE_SCHEMA,\r\n    fkcol.TABLE_NAME AS FK_TABLE_NAME,\r\n    fkcol.COLUMN_NAME as FK_COLUMN_NAME,\r\n    fkcol.ORDINAL_POSITION as ORDINAL,\r\n    fkcon.CONSTRAINT_SCHEMA || '_' || fkcol.TABLE_NAME";
        public static readonly string TableSchemaSecondaryQuery = "select\r\n    pkcol.TABLE_SCHEMA AS PK_TABLE_SCHEMA,\r\n    pkcol.TABLE_NAME AS PK_TABLE_NAME,\r\n    pkcol.COLUMN_NAME as PK_COLUMN_NAME,\r\n    fkcol.COLUMN_NAME as FK_COLUMN_NAME,\r\n    fkcol.ORDINAL_POSITION as ORDINAL,\r\n    fkcon.CONSTRAINT_SCHEMA ";
        public static readonly string ConstraintsQuery = "select i.CONSTRAINT_SCHEMA || '_' || i.CONSTRAINT_NAME as INDEX_NAME, ii.COLUMN_NAME, ii.ORDINAL_POSITION, case when i.CONSTRAINT_TYPE = 'PRIMARY KEY' then 'Y' else 'N' end as PRIMARY_KEY\r\nfrom INFORMATION_SCHEMA.table_constraints i inner join INFORMATION_SCHEMA.key_column_usage ii on i.CONSTRAINT_SCHEMA = ii.CONSTRAINT_SCHEMA and i.CONSTRAINT_NAME = ii.CONSTRAINT_NAME and i.TABLE_SCHEMA = ii.TABLE_SCHEMA and i.TABLE_NAME = ii.TABLE_NAME";
        public static readonly string VersionQuery = "select version()";

        public static readonly PgTable TypesResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("nspname", 0, PgTypeOIDs.Name, -1, PgFormat.Text),
                new PgColumn("typname", 1, PgTypeOIDs.Name, -1, PgFormat.Text),
                new PgColumn("oid", 2, PgTypeOIDs.Oid, sizeof(int), PgFormat.Text),
                new PgColumn("typrelid", 3, PgTypeOIDs.Oid, sizeof(int), PgFormat.Text),
                new PgColumn("typbasetype", 4, PgTypeOIDs.Oid, sizeof(int), PgFormat.Text),
                new PgColumn("type", 5, PgTypeOIDs.Char, sizeof(byte), PgFormat.Text, 1),
                new PgColumn("elemoid", 6, PgTypeOIDs.Oid, sizeof(int), PgFormat.Text),
                new PgColumn("ord", 7, PgTypeOIDs.Int4, sizeof(int), PgFormat.Text),
            },

            Data = new List<PgDataRow>
            {
                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("bool"),
                        Encoding.UTF8.GetBytes("16"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regproc"),
                        Encoding.UTF8.GetBytes("24"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("text"),
                        Encoding.UTF8.GetBytes("25"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("oid"),
                        Encoding.UTF8.GetBytes("26"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("tid"),
                        Encoding.UTF8.GetBytes("27"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("xid"),
                        Encoding.UTF8.GetBytes("28"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("cid"),
                        Encoding.UTF8.GetBytes("29"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("bytea"),
                        Encoding.UTF8.GetBytes("17"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("json"),
                        Encoding.UTF8.GetBytes("114"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("xml"),
                        Encoding.UTF8.GetBytes("142"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("pg_node_tree"),
                        Encoding.UTF8.GetBytes("194"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("pg_ndistinct"),
                        Encoding.UTF8.GetBytes("3361"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("pg_dependencies"),
                        Encoding.UTF8.GetBytes("3402"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("pg_mcv_list"),
                        Encoding.UTF8.GetBytes("5017"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("xid8"),
                        Encoding.UTF8.GetBytes("5069"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("point"),
                        Encoding.UTF8.GetBytes("600"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("lseg"),
                        Encoding.UTF8.GetBytes("601"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("path"),
                        Encoding.UTF8.GetBytes("602"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("box"),
                        Encoding.UTF8.GetBytes("603"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("polygon"),
                        Encoding.UTF8.GetBytes("604"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("line"),
                        Encoding.UTF8.GetBytes("628"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("float4"),
                        Encoding.UTF8.GetBytes("700"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("float8"),
                        Encoding.UTF8.GetBytes("701"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("circle"),
                        Encoding.UTF8.GetBytes("718"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("money"),
                        Encoding.UTF8.GetBytes("790"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("macaddr"),
                        Encoding.UTF8.GetBytes("829"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("inet"),
                        Encoding.UTF8.GetBytes("869"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("cidr"),
                        Encoding.UTF8.GetBytes("650"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("macaddr8"),
                        Encoding.UTF8.GetBytes("774"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("bpchar"),
                        Encoding.UTF8.GetBytes("1042"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("varchar"),
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("date"),
                        Encoding.UTF8.GetBytes("1082"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("time"),
                        Encoding.UTF8.GetBytes("1083"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("timestamp"),
                        Encoding.UTF8.GetBytes("1114"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("timestamptz"),
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("interval"),
                        Encoding.UTF8.GetBytes("1186"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("timetz"),
                        Encoding.UTF8.GetBytes("1266"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("bit"),
                        Encoding.UTF8.GetBytes("1560"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("varbit"),
                        Encoding.UTF8.GetBytes("1562"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("numeric"),
                        Encoding.UTF8.GetBytes("1700"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("refcursor"),
                        Encoding.UTF8.GetBytes("1790"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regprocedure"),
                        Encoding.UTF8.GetBytes("2202"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regoper"),
                        Encoding.UTF8.GetBytes("2203"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regoperator"),
                        Encoding.UTF8.GetBytes("2204"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regclass"),
                        Encoding.UTF8.GetBytes("2205"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regcollation"),
                        Encoding.UTF8.GetBytes("4191"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regtype"),
                        Encoding.UTF8.GetBytes("2206"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regrole"),
                        Encoding.UTF8.GetBytes("4096"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regnamespace"),
                        Encoding.UTF8.GetBytes("4089"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("uuid"),
                        Encoding.UTF8.GetBytes("2950"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("pg_lsn"),
                        Encoding.UTF8.GetBytes("3220"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("tsvector"),
                        Encoding.UTF8.GetBytes("3614"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("tsquery"),
                        Encoding.UTF8.GetBytes("3615"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regconfig"),
                        Encoding.UTF8.GetBytes("3734"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("regdictionary"),
                        Encoding.UTF8.GetBytes("3769"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("jsonb"),
                        Encoding.UTF8.GetBytes("3802"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("jsonpath"),
                        Encoding.UTF8.GetBytes("4072"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("txid_snapshot"),
                        Encoding.UTF8.GetBytes("2970"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("pg_snapshot"),
                        Encoding.UTF8.GetBytes("5038"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("record"),
                        Encoding.UTF8.GetBytes("2249"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("p"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("char"),
                        Encoding.UTF8.GetBytes("18"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("void"),
                        Encoding.UTF8.GetBytes("2278"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("p"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("name"),
                        Encoding.UTF8.GetBytes("19"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("int8"),
                        Encoding.UTF8.GetBytes("20"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("int2"),
                        Encoding.UTF8.GetBytes("21"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("int2vector"),
                        Encoding.UTF8.GetBytes("22"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("int4"),
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("cardinal_number"),
                        Encoding.UTF8.GetBytes("13171"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("yes_or_no"),
                        Encoding.UTF8.GetBytes("13183"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("character_data"),
                        Encoding.UTF8.GetBytes("13174"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("time_stamp"),
                        Encoding.UTF8.GetBytes("13181"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("sql_identifier"),
                        Encoding.UTF8.GetBytes("13176"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("19"),
                        Encoding.UTF8.GetBytes("d"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("1"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("tsrange"),
                        Encoding.UTF8.GetBytes("3908"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("1114"),
                        Encoding.UTF8.GetBytes("2"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("tstzrange"),
                        Encoding.UTF8.GetBytes("3910"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("2"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("daterange"),
                        Encoding.UTF8.GetBytes("3912"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("1082"),
                        Encoding.UTF8.GetBytes("2"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("int8range"),
                        Encoding.UTF8.GetBytes("3926"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("20"),
                        Encoding.UTF8.GetBytes("2"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("int4range"),
                        Encoding.UTF8.GetBytes("3904"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("2"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("numrange"),
                        Encoding.UTF8.GetBytes("3906"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("r"),
                        Encoding.UTF8.GetBytes("1700"),
                        Encoding.UTF8.GetBytes("2"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("_yes_or_no"),
                        Encoding.UTF8.GetBytes("13182"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("13183"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("public"),
                        Encoding.UTF8.GetBytes("_Customers"),
                        Encoding.UTF8.GetBytes("16403"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("16404"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("oidvector"),
                        Encoding.UTF8.GetBytes("30"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("b"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_record"),
                        Encoding.UTF8.GetBytes("2287"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2249"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_bool"),
                        Encoding.UTF8.GetBytes("1000"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("16"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_bytea"),
                        Encoding.UTF8.GetBytes("1001"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("17"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_char"),
                        Encoding.UTF8.GetBytes("1002"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("18"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_name"),
                        Encoding.UTF8.GetBytes("1003"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("19"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_int8"),
                        Encoding.UTF8.GetBytes("1016"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("20"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_int2"),
                        Encoding.UTF8.GetBytes("1005"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("21"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_int2vector"),
                        Encoding.UTF8.GetBytes("1006"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("22"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_int4"),
                        Encoding.UTF8.GetBytes("1007"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("23"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regproc"),
                        Encoding.UTF8.GetBytes("1008"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("24"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_text"),
                        Encoding.UTF8.GetBytes("1009"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("25"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_oid"),
                        Encoding.UTF8.GetBytes("1028"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("26"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_tid"),
                        Encoding.UTF8.GetBytes("1010"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("27"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_xid"),
                        Encoding.UTF8.GetBytes("1011"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("28"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_cid"),
                        Encoding.UTF8.GetBytes("1012"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("29"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_oidvector"),
                        Encoding.UTF8.GetBytes("1013"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("30"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_json"),
                        Encoding.UTF8.GetBytes("199"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("114"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_xml"),
                        Encoding.UTF8.GetBytes("143"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("142"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_xid8"),
                        Encoding.UTF8.GetBytes("271"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("5069"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_point"),
                        Encoding.UTF8.GetBytes("1017"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("600"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_lseg"),
                        Encoding.UTF8.GetBytes("1018"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("601"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_path"),
                        Encoding.UTF8.GetBytes("1019"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("602"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_box"),
                        Encoding.UTF8.GetBytes("1020"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("603"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_polygon"),
                        Encoding.UTF8.GetBytes("1027"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("604"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_line"),
                        Encoding.UTF8.GetBytes("629"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("628"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_float4"),
                        Encoding.UTF8.GetBytes("1021"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("700"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_float8"),
                        Encoding.UTF8.GetBytes("1022"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("701"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_circle"),
                        Encoding.UTF8.GetBytes("719"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("718"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_money"),
                        Encoding.UTF8.GetBytes("791"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("790"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_macaddr"),
                        Encoding.UTF8.GetBytes("1040"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("829"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_inet"),
                        Encoding.UTF8.GetBytes("1041"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("869"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_cidr"),
                        Encoding.UTF8.GetBytes("651"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("650"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_macaddr8"),
                        Encoding.UTF8.GetBytes("775"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("774"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_aclitem"),
                        Encoding.UTF8.GetBytes("1034"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1033"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_bpchar"),
                        Encoding.UTF8.GetBytes("1014"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1042"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_varchar"),
                        Encoding.UTF8.GetBytes("1015"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1043"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_date"),
                        Encoding.UTF8.GetBytes("1182"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1082"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_time"),
                        Encoding.UTF8.GetBytes("1183"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1083"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_timestamp"),
                        Encoding.UTF8.GetBytes("1115"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1114"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_timestamptz"),
                        Encoding.UTF8.GetBytes("1185"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1184"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_interval"),
                        Encoding.UTF8.GetBytes("1187"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1186"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_timetz"),
                        Encoding.UTF8.GetBytes("1270"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1266"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_bit"),
                        Encoding.UTF8.GetBytes("1561"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1560"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_varbit"),
                        Encoding.UTF8.GetBytes("1563"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1562"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_numeric"),
                        Encoding.UTF8.GetBytes("1231"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1700"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_refcursor"),
                        Encoding.UTF8.GetBytes("2201"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("1790"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regprocedure"),
                        Encoding.UTF8.GetBytes("2207"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2202"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regoper"),
                        Encoding.UTF8.GetBytes("2208"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2203"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regoperator"),
                        Encoding.UTF8.GetBytes("2209"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2204"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regclass"),
                        Encoding.UTF8.GetBytes("2210"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2205"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regcollation"),
                        Encoding.UTF8.GetBytes("4192"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("4191"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regtype"),
                        Encoding.UTF8.GetBytes("2211"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2206"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regrole"),
                        Encoding.UTF8.GetBytes("4097"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("4096"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regnamespace"),
                        Encoding.UTF8.GetBytes("4090"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("4089"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_uuid"),
                        Encoding.UTF8.GetBytes("2951"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2950"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_pg_lsn"),
                        Encoding.UTF8.GetBytes("3221"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3220"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_tsvector"),
                        Encoding.UTF8.GetBytes("3643"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3614"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_gtsvector"),
                        Encoding.UTF8.GetBytes("3644"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3642"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_tsquery"),
                        Encoding.UTF8.GetBytes("3645"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3615"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regconfig"),
                        Encoding.UTF8.GetBytes("3735"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3734"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_regdictionary"),
                        Encoding.UTF8.GetBytes("3770"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3769"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_jsonb"),
                        Encoding.UTF8.GetBytes("3807"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3802"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_jsonpath"),
                        Encoding.UTF8.GetBytes("4073"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("4072"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_txid_snapshot"),
                        Encoding.UTF8.GetBytes("2949"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2970"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_pg_snapshot"),
                        Encoding.UTF8.GetBytes("5039"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("5038"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_int4range"),
                        Encoding.UTF8.GetBytes("3905"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3904"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_numrange"),
                        Encoding.UTF8.GetBytes("3907"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3906"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_tsrange"),
                        Encoding.UTF8.GetBytes("3909"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3908"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_tstzrange"),
                        Encoding.UTF8.GetBytes("3911"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3910"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_daterange"),
                        Encoding.UTF8.GetBytes("3913"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3912"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_int8range"),
                        Encoding.UTF8.GetBytes("3927"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("3926"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("pg_catalog"),
                        Encoding.UTF8.GetBytes("_cstring"),
                        Encoding.UTF8.GetBytes("1263"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("2275"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("_cardinal_number"),
                        Encoding.UTF8.GetBytes("13170"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("13171"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("_character_data"),
                        Encoding.UTF8.GetBytes("13173"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("13174"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("_sql_identifier"),
                        Encoding.UTF8.GetBytes("13175"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("13176"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },

                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.UTF8.GetBytes("information_schema"),
                        Encoding.UTF8.GetBytes("_time_stamp"),
                        Encoding.UTF8.GetBytes("13180"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("0"),
                        Encoding.UTF8.GetBytes("a"),
                        Encoding.UTF8.GetBytes("13181"),
                        Encoding.UTF8.GetBytes("3"),
                    }
                },
            }
        };

        public static readonly PgTable CompositeTypesResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("oid", 0, PgTypeOIDs.Oid, sizeof(int), PgFormat.Text),
                new PgColumn("attname", 1, PgTypeOIDs.Name, -1, PgFormat.Text),
                new PgColumn("atttypid", 2, PgTypeOIDs.Oid, sizeof(int), PgFormat.Text),
            }
        };

        public static readonly PgTable EnumTypesResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("oid", 0, PgTypeOIDs.Oid, sizeof(int), PgFormat.Text),
                new PgColumn("enumlabel", 1, PgTypeOIDs.Name, -1, PgFormat.Text),
            }
        };

        public static readonly PgTable TableSchemaResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("pk_column_name", 0, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("fk_table_schema", 1, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("fk_table_name", 2, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("fk_column_name", 3, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("ordinal", 4, PgTypeOIDs.Int4, sizeof(int), PgFormat.Binary),
                new PgColumn("fk_name", 5, PgTypeOIDs.Name, -1, PgFormat.Binary),
            }
        };

        public static readonly PgTable TableSchemaSecondaryResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("pk_table_schema", 0, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("pk_table_name", 1, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("pk_column_name", 2, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("fk_column_name", 3, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("ordinal", 4, PgTypeOIDs.Int4, sizeof(int), PgFormat.Binary),
                new PgColumn("fk_name", 5, PgTypeOIDs.Name, -1, PgFormat.Binary),
            }
        };

        public static readonly PgTable ConstraintsResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("index_name", 0, PgTypeOIDs.Text, -1, PgFormat.Binary),
                new PgColumn("column_name", 1, PgTypeOIDs.Name, 64, PgFormat.Binary),
                new PgColumn("ordinal_position", 2, PgTypeOIDs.Int4, sizeof(int), PgFormat.Binary),
                new PgColumn("primary_key", 3, PgTypeOIDs.Text, -1, PgFormat.Binary),
            }
        };

        public static readonly PgTable VersionResponse = new()
        {
            Columns = new List<PgColumn>()
            {
                new PgColumn("version", 0, PgTypeOIDs.Text, -1, PgFormat.Text),
            },
            Data = new List<PgDataRow>
            {
                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        Encoding.ASCII.GetBytes("PostgreSQL 13.3, compiled by Visual C++ build 1914, 64-bit")
                    }
                }
            }
        };
    }
}
