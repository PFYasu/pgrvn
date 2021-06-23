using System;
using Npgsql;
using NpgsqlTypes;

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
            var connString = "Host=127.0.0.1;Port=5432;Username=postgres;Password=123456;Database=BookStore";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            SelectOne(conn);
        }
    }
}
