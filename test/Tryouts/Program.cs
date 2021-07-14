using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Threading;
using Npgsql;
using NpgsqlTypes;
using PgRvn.Server;

namespace Tryouts
{
    class Program
    {
        static void InsertData(NpgsqlConnection conn)
        {
            using (var cmd = new NpgsqlCommand("INSERT INTO \"Customers\" (\"First Name\") VALUES (@p, @x)", conn))
            {
                cmd.Parameters.AddWithValue("p", NpgsqlDbType.Name, "Hello world");
                cmd.ExecuteNonQuery();
            }
        }

        static void Select(NpgsqlConnection conn, string query, Dictionary<string,object> namedArgs = null)
        {
            using var cmd = new NpgsqlCommand(query, conn);
            Console.WriteLine(query);
            if (namedArgs != null)
            {
                foreach (var (key, val) in namedArgs)
                {
                    Console.WriteLine($"\t{key} = {val}");
                    cmd.Parameters.AddWithValue(key, val);
                }
            }
            
            using var reader = cmd.ExecuteReader();

            var dt = new DataTable();
            dt.Load(reader);

            dt.Print();
        }

        static void SelectMulti(NpgsqlConnection conn, string query, Dictionary<string, object> namedArgs = null)
        {
            using var cmd = new NpgsqlCommand(query, conn);
            Console.WriteLine(query);
            if (namedArgs != null)
            {
                foreach (var (key, val) in namedArgs)
                {
                    Console.WriteLine($"\t{key} = {val}");
                    cmd.Parameters.AddWithValue(key, val);
                }
            }

            using var reader = cmd.ExecuteReader();

            var dt1 = new DataTable();
            var dt2 = new DataTable();

            var ds = new DataSet();
            ds.Tables.Add(dt1);
            ds.Tables.Add(dt2);

            ds.Load(reader, LoadOption.OverwriteChanges, dt1, dt2);

            dt1.Print();
            dt2.Print();
        }

        static void InitODBC()
        {
            //var connectionString = "Driver={PostgreSQL Unicode};Server=127.0.0.1;Port=5432;Database=BookStore;Uid=postgres;Pwd=123456;";
            var connectionString = "Driver={PostgreSQL Unicode};Server=127.0.0.1;Port=5433;Database=Northwind;Uid=postgres;Pwd=123456;";
            var cnDb = new OdbcConnection(connectionString);
            cnDb.Open();

            // Create a dataset
            var dsDB = new DataSet();
            var adDB = new OdbcDataAdapter();
            var cbDB = new OdbcCommandBuilder(adDB);
            adDB.SelectCommand = new OdbcCommand("select 1; select 2", cnDb);
            adDB.Fill(dsDB);

            // Display the record count
            Console.WriteLine($"Table contains {dsDB.Tables[0].Rows.Count} rows.\n");
        }

        static void Main(string[] args)
        {
            try
            {
                var server = new PgRvnServer();
                server.Initialize();

            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled exception occurred");
                Console.WriteLine(e);
                return;
            }

            // var connString = "Host=127.0.0.1;Port=5432;User Id=postgres;Password=123456;Database=BookStore;Timeout=600";
            var connString = "Host=127.0.0.1;Port=5433;User Id=postgres;Password=123456;Database=Northwind;Timeout=1000;"; // ServerCompatibilityMode=NoTypeLoading

            //InitODBC();

            // TODO: Figure out why Npgsql doesn't handle our ErrorResponse messages gracefully
            using var conn = new NpgsqlConnection(connString);
            conn.Open();


            // Select(conn, "select * from users"); 
//            Select(conn, "select version()"); 
//            Select(conn, @"
// SELECT ns.nspname, typ_and_elem_type.*,
//   CASE
//       WHEN typtype IN ('b', 'e', 'p') THEN 0           -- First base types, enums, pseudo-types
//       WHEN typtype = 'r' THEN 1                        -- Ranges after
//       WHEN typtype = 'c' THEN 2                        -- Composites after
//       WHEN typtype = 'd' AND elemtyptype <> 'a' THEN 3 -- Domains over non-arrays after
//       WHEN typtype = 'a' THEN 4                        -- Arrays before
//       WHEN typtype = 'd' AND elemtyptype = 'a' THEN 5  -- Domains over arrays last
//    END AS ord
//FROM (
//    -- Arrays have typtype=b - this subquery identifies them by their typreceive and converts their typtype to a
//    -- We first do this for the type (innerest-most subquery), and then for its element type
//    -- This also returns the array element, range subtype and domain base type as elemtypoid
//    SELECT
//        typ.oid, typ.typnamespace, typ.typname, typ.typtype, typ.typrelid, typ.typnotnull, typ.relkind,
//        elemtyp.oid AS elemtypoid, elemtyp.typname AS elemtypname, elemcls.relkind AS elemrelkind,
//        CASE WHEN elemproc.proname='array_recv' THEN 'a' ELSE elemtyp.typtype END AS elemtyptype
//    FROM (
//        SELECT typ.oid, typnamespace, typname, typrelid, typnotnull, relkind, typelem AS elemoid,
//            CASE WHEN proc.proname='array_recv' THEN 'a' ELSE typ.typtype END AS typtype,
//            CASE
//                WHEN proc.proname='array_recv' THEN typ.typelem
//                WHEN typ.typtype='r' THEN rngsubtype
//                WHEN typ.typtype='d' THEN typ.typbasetype
//            END AS elemtypoid
//        FROM pg_type AS typ
//        LEFT JOIN pg_class AS cls ON (cls.oid = typ.typrelid)
//        LEFT JOIN pg_proc AS proc ON proc.oid = typ.typreceive
//        LEFT JOIN pg_range ON (pg_range.rngtypid = typ.oid)
//    ) AS typ
//    LEFT JOIN pg_type AS elemtyp ON elemtyp.oid = elemtypoid
//    LEFT JOIN pg_class AS elemcls ON (elemcls.oid = elemtyp.typrelid)
//    LEFT JOIN pg_proc AS elemproc ON elemproc.oid = elemtyp.typreceive
//) AS typ_and_elem_type
//JOIN pg_namespace AS ns ON (ns.oid = typnamespace)
//WHERE
//    typtype IN ('b', 'r', 'e', 'd') OR -- Base, range, enum, domain
//    (typtype = 'c' AND relkind='c') OR -- User-defined free-standing composites (not table composites) by default
//    (typtype = 'p' AND typname IN ('record', 'void')) OR -- Some special supported pseudo-types
//    (typtype = 'a' AND (  -- Array of...
//        elemtyptype IN ('b', 'r', 'e', 'd') OR -- Array of base, range, enum, domain
//        (elemtyptype = 'p' AND elemtypname IN ('record', 'void')) OR -- Arrays of special supported pseudo-types
//        (elemtyptype = 'c' AND elemrelkind='c') -- Array of user-defined free-standing composites (not table composites) by default
//    ))
//ORDER BY ord");
            //SelectMulti(conn, "from Employees select FirstName");
            // SelectMulti(conn, "select @test1; select @test2", new Dictionary<string, object>
            // {
            //     ["test1"] = 1,
            //     ["test2"] = 2
            // });
            // Select(conn, "from Employees");
            // Select(conn, "from Employees select LastName, FirstName");
            // Select(conn, "from index 'Orders/Totals'"); // map index
            // Select(conn, "from index 'Orders/ByCompany'"); // map/reduce index
            // Select(conn, "from index 'Orders/ByCompany' order by Count as long desc select Company, Count"); // map/reduce index
            // Select(conn, "from Orders select Company, OrderedAt, Freight"); // map index projection
            // Select(conn, "from index 'Orders/Totals' select Company, OrderedAt, Freight"); // map index projection
            // Select(conn, "from Employees as e select { FullName: e.FirstName + ' ' + e.LastName } "); // projection via js
            //Select(conn, "from Employees where Address.City = @city", new Dictionary<string, object>
            //{
            //    ["city"] = "Seattle"
            //}); // with args
            // Select(conn, "from Orders include Employee limit 3 "); // with include
            //Select(conn, "thisisbad"); // invalid query
            // Select(conn, "from x"); // Empty results


            // out of scope for now: graph queries


        }
    }
}
