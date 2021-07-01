using System;
using System.Collections.Generic;
using System.Data;
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

            var connString = "Host=127.0.0.1;Port=5433;User Id=postgres;Password=123456;Database=BookStore;ServerCompatibilityMode=NoTypeLoading;Timeout=600";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            //Select(conn, "from Employees");
            //Select(conn, "from Employees select LastName, FirstName");
            //Select(conn, "from index 'Orders/Totals'"); // map index
            //Select(conn, "from index 'Orders/ByCompany'"); // map/reduce index
            //Select(conn, "from index 'Orders/ByCompany' order by Count as long desc select Company, Count"); // map/reduce index
            //Select(conn, "from Orders select Company, OrderedAt, Freight"); // map index projection
            //Select(conn, "from index 'Orders/Totals' select Company, OrderedAt, Freight"); // map index projection
            //Select(conn, "from Employees as e select { FullName: e.FirstName + ' ' + e.LastName } "); // projection via js
            Select(conn, "from Employees where Address.City = @city", new Dictionary<string, object>
            {
                ["city"] = "Seattle"
            }); // with args
            //Select(conn, "from Orders include Employee"); // with include


            // out of scope for now: graph queries


        }
    }
}
