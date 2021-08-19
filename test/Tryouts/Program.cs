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

        static void Select(NpgsqlConnection conn, string query, Dictionary<string, (NpgsqlDbType, object)> namedArgs = null)
        {
            using var cmd = new NpgsqlCommand(query, conn);
            Console.WriteLine(query);
            if (namedArgs != null)
            {
                foreach (var (key, val) in namedArgs)
                {
                    Console.WriteLine($"\t{key} = {val}");
                    cmd.Parameters.AddWithValue(key, val.Item1, val.Item2);
                }
            }
            
            using var reader = cmd.ExecuteReader();

            var dt = new DataTable();
            dt.Load(reader);

            dt.Print();
        }

        static void SelectMulti(NpgsqlConnection conn, string query, Dictionary<string, (NpgsqlDbType, object)> namedArgs = null)
        {
            using var cmd = new NpgsqlCommand(query, conn);
            Console.WriteLine(query);
            if (namedArgs != null)
            {
                foreach (var (key, val) in namedArgs)
                {
                    Console.WriteLine($"\t{key} = {val}");
                    cmd.Parameters.AddWithValue(key, val.Item1, val.Item2);
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
            var conn = new OdbcConnection(connectionString);
            conn.Open();

            // SELECT * FROM Customers WHERE field = $1

            var cmd = conn.CreateCommand();
            cmd.CommandText = "from Employees select FirstName limit 2";
            //cmd.CommandText = "from Products where Discontinued = ?";
            //cmd.CommandText = "from Orders where OrderedAtUtc = ?";
            //cmd.CommandText = "from index 'Stocks_ByTradeVolume' as f where f.Company = 'Alfreds Futterkiste' select { adv: new Date(f.Date).toJSON() }";
            // from Products where Discontinued = $1::int8
            // Parse - ParameterType - 25

            // from Products where Discontinued = $1
            // Parse - ParameterType - 0

            //cmd.Parameters.Add("@BigInt", OdbcType.BigInt).Value = long.MaxValue;
            //cmd.Parameters.Add("@Binary", OdbcType.Binary).Value = new byte[] { 0x42, 0x43 };
            //cmd.Parameters.Add("@Bit", OdbcType.Bit).Value = false;
            //cmd.Parameters.Add("@Char", OdbcType.Char).Value = 'a';
            //cmd.Parameters.Add("@Date", OdbcType.Date).Value = new DateTime(2000, 1, 1);
            //cmd.Parameters.Add("@DateTime", OdbcType.DateTime).Value = DateTime.Parse("1998-05-05T01:02:03.0405060Z"); //.0405060Z
            //cmd.Parameters.Add("@Decimal", OdbcType.Decimal).Value = (decimal)15.3;
            //cmd.Parameters.Add("@Double", OdbcType.Double).Value = (double)123.123123;
            //cmd.Parameters.Add("@Image", OdbcType.Image).Value = new byte[] { 0x1, 0x2, 0x3, 0x4 };
            //cmd.Parameters.Add("@Int", OdbcType.Int).Value = 5;
            //cmd.Parameters.Add("@NChar", OdbcType.NChar).Value = "Hello World";
            //cmd.Parameters.Add("@NText", OdbcType.NText).Value = "Hello World";
            //cmd.Parameters.Add("@Numeric", OdbcType.Numeric).Value = (decimal)15.3;
            //cmd.Parameters.Add("@NVarChar", OdbcType.NVarChar).Value = "Hello World";
            //cmd.Parameters.Add("@Real", OdbcType.Real).Value = (float)13.37;
            //cmd.Parameters.Add("@SmallDateTime", OdbcType.SmallDateTime).Value = new DateTime(2000, 3, 3);
            //cmd.Parameters.Add("@SmallInt", OdbcType.SmallInt).Value = (short)2;
            //cmd.Parameters.Add("@Text", OdbcType.Text).Value = "Hello World";
            //cmd.Parameters.Add("@Time", OdbcType.Time).Value = new TimeSpan(1, 1, 1, 1);
            //cmd.Parameters.Add("@Timestamp", OdbcType.Timestamp).Value = new byte[] { 0x42, 0x43 };
            //cmd.Parameters.Add("@TinyInt", OdbcType.TinyInt).Value = 0x1;
            //cmd.Parameters.Add("@UniqueIdentifier", OdbcType.UniqueIdentifier).Value = new Guid(new byte[] { 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 });
            //cmd.Parameters.Add("@VarBinary", OdbcType.VarBinary).Value = new byte[] { 0x1, 0x2 };
            //cmd.Parameters.Add("@VarChar", OdbcType.VarChar).Value = "Hello World";

            using var reader = cmd.ExecuteReader();

            var dt1 = new DataTable();
            var dt2 = new DataTable();

            var ds = new DataSet();
            ds.Tables.Add(dt1);
            ds.Tables.Add(dt2);

            ds.EnforceConstraints = false;

            ds.Load(reader, LoadOption.OverwriteChanges, dt1, dt2);

            dt1.Print();
            dt2.Print();
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


            InitODBC();

            //Console.ReadLine();

            //var connString = "Host=127.0.0.1;Port=5432;User Id=postgres;Password=123456;Database=BookStore;Timeout=600";
            //var connString = "Host=127.0.0.1;Port=5433;User Id=postgres;Password=123456;Database=Northwind;Timeout=1000;"; // ServerCompatibilityMode=NoTypeLoading
            //using var conn = new NpgsqlConnection(connString); conn.Open();
            //Select(conn, "from Orders"); 

            //var dto = DateTime.Parse("1998-05-05T01:02:03.0405060Z");
            ////var dto = new TimeSpan(1, 2, 3, 4);
            //Select(conn, "from Orders where OrderedAtUtc = @param1", new Dictionary<string, (NpgsqlDbType, object)>
            //{
            //    ["param1"] = (NpgsqlDbType.TimestampTz, dto)
            //});

            //Select(conn, "SELECT * FROM \"Customers\"");

            // Select(conn, "from Orders as o where id() = 'orders/829-A' update { o.Freight = \"13.31\"}");
            // Select(conn, "select version()"); 
            // SelectMulti(conn, "from Employees select FirstName");
            // SelectMulti(conn, "select @test1; select @test2", new Dictionary<string, object>
            // {
            //     ["test1"] = 1,
            //     ["test2"] = 2
            // });
            // Select(conn, "from @empty");
            // Select(conn, "from Employees select LastName, FirstName");
            // Select(conn, "from index 'Orders/Totals'"); // map index
            // Select(conn, "from index 'Orders/ByCompany'"); // map/reduce index
            // Select(conn, "from index 'Orders/ByCompany' order by Count as long desc select Company, Count"); // map/reduce index
            // Select(conn, "from Orders select Company, OrderedAt, Freight"); // map index projection
            // Select(conn, "from index 'Orders/Totals' select Company, OrderedAt, Freight"); // map index projection
            // Select(conn, "from Employees as e select { FullName: e.FirstName + ' ' + e.LastName } "); // projection via js
            // Select(conn, "from Orders include Employee limit 3 "); // with include
            //Select(conn, "thisisbad"); // invalid query
            // Select(conn, "from x"); // Empty results


            // out of scope for now: graph queries


        }
    }
}
