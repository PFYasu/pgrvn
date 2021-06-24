using System;
using Npgsql;
using NpgsqlTypes;
using PgRvn.Server;

namespace Tryouts
{
    class Program
    {
        static void InsertData(NpgsqlConnection conn)
        {
            using (var cmd = new NpgsqlCommand("INSERT INTO \"Customers\" (\"First Name\") VALUES (@p)", conn))
            {
                cmd.Parameters.AddWithValue("p", NpgsqlDbType.Name, "Hello world");
                cmd.ExecuteNonQuery();
            }
        }

        static void SelectOne(NpgsqlConnection conn)
        {
            using (var cmd = new NpgsqlCommand("SELECT 1", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetValue(0));
                }
            }
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

            var connString = "Host=127.0.0.1;Port=5433;User Id=postgres;Password=123456;Database=BookStore";
            //var connString = "Host=127.0.0.1;Port=5432;User Id=postgres;Password=123456;Database=BookStore";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            SelectOne(conn);
        }
    }
}
