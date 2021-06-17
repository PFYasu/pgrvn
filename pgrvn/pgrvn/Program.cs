using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;

namespace pgrvn
{
    class Program
    {
        static void Main(string[] args)
        {
            var connString = "Host=127.0.0.1;Username=postgres;Password=123456;Database=BookStore";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            // Insert some data
            using (var cmd = new NpgsqlCommand("INSERT INTO \"Customers\" (\"First Name\") VALUES (@p)", conn))
            {
                cmd.Parameters.AddWithValue("p", NpgsqlDbType.Name, "Hello world");
                cmd.ExecuteNonQuery();
            }

            // Retrieve all rows
            using (var cmd = new NpgsqlCommand("SELECT \"First Name\" FROM \"Customers\"", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetString(0));
                }
            }
        }
    }
}
