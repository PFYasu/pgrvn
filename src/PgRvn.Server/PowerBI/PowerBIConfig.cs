using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgRvn.Server.Messages;
using PgRvn.Server.Types;

namespace PgRvn.Server
{
    public static class PowerBIConfig
    {
        // Note: PowerBI Desktop uses and ships with NpgSQL 4.0.10 and does not recommend 4.1 and up. https://docs.microsoft.com/en-us/power-query/connectors/postgresql

        public static readonly string TypesQuery = "\n/*** Load all supported types ***/\nSELECT ns.nspname, a.typname, a.oid, a.typrelid, a.typbasetype,\nCASE WHEN pg_proc.proname='array_recv' THEN 'a' ELSE a.typtype END AS type,\nCASE\n  WHEN pg_proc.proname='array_recv' THEN a.typelem\n  WHEN a.typtype='r' THEN rngsubtype\n  ELSE 0\nEND AS elemoid,\nCASE\n  WHEN pg_proc.proname IN ('array_recv','oidvectorrecv') THEN 3    /* Arrays last */\n  WHEN a.typtype='r' THEN 2                                        /* Ranges before */\n  WHEN a.typtype='d' THEN 1                                        /* Domains before */\n  ELSE 0                                                           /* Base types first */\nEND AS ord\nFROM pg_type AS a\nJOIN pg_namespace AS ns ON (ns.oid = a.typnamespace)\nJOIN pg_proc ON pg_proc.oid = a.typreceive\nLEFT OUTER JOIN pg_class AS cls ON (cls.oid = a.typrelid)\nLEFT OUTER JOIN pg_type AS b ON (b.oid = a.typelem)\nLEFT OUTER JOIN pg_class AS elemcls ON (elemcls.oid = b.typrelid)\nLEFT OUTER JOIN pg_range ON (pg_range.rngtypid = a.oid) \nWHERE\n  a.typtype IN ('b', 'r', 'e', 'd') OR         /* Base, range, enum, domain */\n  (a.typtype = 'c' AND cls.relkind='c') OR /* User-defined free-standing composites (not table composites) by default */\n  (pg_proc.proname='array_recv' AND (\n    b.typtype IN ('b', 'r', 'e', 'd') OR       /* Array of base, range, enum, domain */\n    (b.typtype = 'p' AND b.typname IN ('record', 'void')) OR /* Arrays of special supported pseudo-types */\n    (b.typtype = 'c' AND elemcls.relkind='c')  /* Array of user-defined free-standing composites (not table composites) */\n  )) OR\n  (a.typtype = 'p' AND a.typname IN ('record', 'void'))  /* Some special supported pseudo-types */\nORDER BY ord";
        public static readonly string TableSchemaQuery = "select\n    pkcol.COLUMN_NAME as PK_COLUMN_NAME,\n    fkcol.TABLE_SCHEMA AS FK_TABLE_SCHEMA,\n    fkcol.TABLE_NAME AS FK_TABLE_NAME,\n    fkcol.COLUMN_NAME as FK_COLUMN_NAME,\n    fkcol.ORDINAL_POSITION as ORDINAL,\n    fkcon.CONSTRAINT_SCHEMA || '_' || fkcol.TABLE_NAME";
        public static readonly string TableSchemaSecondaryQuery = "select\n    pkcol.TABLE_SCHEMA AS PK_TABLE_SCHEMA,\n    pkcol.TABLE_NAME AS PK_TABLE_NAME,\n    pkcol.COLUMN_NAME as PK_COLUMN_NAME,\n    fkcol.COLUMN_NAME as FK_COLUMN_NAME,\n    fkcol.ORDINAL_POSITION as ORDINAL,\n    fkcon.CONSTRAINT_SCHEMA ";
        public static readonly string ConstraintsQuery = "select i.CONSTRAINT_SCHEMA || '_' || i.CONSTRAINT_NAME as INDEX_NAME, ii.COLUMN_NAME, ii.ORDINAL_POSITION, case when i.CONSTRAINT_TYPE = 'PRIMARY KEY' then 'Y' else 'N' end as PRIMARY_KEY\nfrom INFORMATION_SCHEMA.table_constraints i inner join INFORMATION_SCHEMA.key_column_usage ii on i.CONSTRAINT_SCHEMA = ii.CONSTRAINT_SCHEMA and i.CONSTRAINT_NAME = ii.CONSTRAINT_NAME and i.TABLE_SCHEMA = ii.TABLE_SCHEMA and i.TABLE_NAME = ii.TABLE_NAME";
        public static readonly string CharacterSetsQuery = "select character_set_name from INFORMATION_SCHEMA.character_sets";

        public static readonly PgTable TypesResponse = CsvToPg.Convert(
            @"types_query.csv",
            new Dictionary<string, PgColumn>
            {
                { "nspname", new PgColumn("nspname", 0, PgName.Default, PgFormat.Text) },
                { "typname", new PgColumn("typname", 1, PgName.Default, PgFormat.Text) },
                { "oid", new PgColumn("oid", 2, PgOid.Default, PgFormat.Text) },
                { "typrelid", new PgColumn("typrelid", 3, PgOid.Default, PgFormat.Text) },
                { "typbasetype", new PgColumn("typbasetype", 4, PgOid.Default, PgFormat.Text) },
                { "type", new PgColumn("type", 5, PgChar.Default, PgFormat.Text, 1) },
                { "elemoid", new PgColumn("elemoid", 6, PgOid.Default, PgFormat.Text) },
                { "ord", new PgColumn("ord", 7, PgInt4.Default, PgFormat.Text) },
            });

        public static readonly PgTable TableSchemaResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("pk_column_name", 0, PgName.Default, PgFormat.Binary),
                new PgColumn("fk_table_schema", 1, PgName.Default, PgFormat.Binary),
                new PgColumn("fk_table_name", 2, PgName.Default, PgFormat.Binary),
                new PgColumn("fk_column_name", 3, PgName.Default, PgFormat.Binary),
                new PgColumn("ordinal", 4, PgInt4.Default, PgFormat.Binary),
                new PgColumn("fk_name", 5, PgName.Default, PgFormat.Binary),
            }
        };

        public static readonly PgTable TableSchemaSecondaryResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("pk_table_schema", 0, PgName.Default, PgFormat.Binary),
                new PgColumn("pk_table_name", 1, PgName.Default, PgFormat.Binary),
                new PgColumn("pk_column_name", 2, PgName.Default, PgFormat.Binary),
                new PgColumn("fk_column_name", 3, PgName.Default, PgFormat.Binary),
                new PgColumn("ordinal", 4, PgInt4.Default, PgFormat.Binary),
                new PgColumn("fk_name", 5, PgName.Default, PgFormat.Binary),
            }
        };

        public static readonly PgTable ConstraintsResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("index_name", 0, PgText.Default, PgFormat.Binary),
                new PgColumn("column_name", 1, PgName.Default, PgFormat.Binary),
                new PgColumn("ordinal_position", 2, PgInt4.Default, PgFormat.Binary),
                new PgColumn("primary_key", 3, PgText.Default, PgFormat.Binary),
            }
        };

        public static readonly PgTable CharacterSetsResponse = new()
        {
            Columns = new List<PgColumn>
            {
                new PgColumn("character_set_name", 0, PgName.Default, PgFormat.Text),
            },
            Data = new List<PgDataRow>
            {
                new()
                {
                    ColumnData = new ReadOnlyMemory<byte>?[]
                    {
                        PgName.Default.ToBytes("UTF8", PgFormat.Text)
                    }
                },
            }
        };
    }
}
