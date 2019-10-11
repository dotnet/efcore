using System;
using Microsoft.Data.Sqlite;

namespace ScalarFunctionSample
{
    class Program
    {
        static void Main()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var createCommand = connection.CreateCommand();
            createCommand.CommandText =
            @"
                CREATE TABLE cylinder (
                    name TEXT,
                    radius REAL,
                    height REAL
                );

                INSERT INTO cylinder
                VALUES ('1x2', 1.0, 2.0),
                       ('2x1', 2.0, 1.0);
            ";
            createCommand.ExecuteNonQuery();

            // SQLite will invoke this managed delegate. Debug it like you would any other code
            // by setting breakpoints, etc.
            connection.CreateFunction(
                "volume",
                (double radius, double height)
                    => Math.PI * Math.Pow(radius, 2) * height);

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText =
            @"
                SELECT name,
                       volume(radius, height) AS volume
                FROM cylinder
                ORDER BY volume DESC
            ";

            var reader = queryCommand.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"{reader["name"]}: {reader["volume"]}");
            }
        }
    }
}
