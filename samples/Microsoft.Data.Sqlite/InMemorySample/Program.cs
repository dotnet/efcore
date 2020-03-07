using System;
using Microsoft.Data.Sqlite;

namespace InMemorySample
{
    class Program
    {
        static void Main()
        {
            // Using a name and a shared cache allows multiple connections to access the same
            // in-memory database
            const string connectionString = "Data Source=InMemorySample;Mode=Memory;Cache=Shared";

            // The in-memory database only persists while a connection is open to it. To manage
            // its lifetime, keep one open connection around for as long as you need it.
            var masterConnection = new SqliteConnection(connectionString);
            masterConnection.Open();

            var createCommand = masterConnection.CreateCommand();
            createCommand.CommandText =
            @"
                CREATE TABLE data (
                    value TEXT
                )
            ";
            createCommand.ExecuteNonQuery();

            using (var firstConnection = new SqliteConnection(connectionString))
            {
                firstConnection.Open();

                var updateCommand = firstConnection.CreateCommand();
                updateCommand.CommandText =
                @"
                    INSERT INTO data
                    VALUES ('Hello, memory!')
                ";
                updateCommand.ExecuteNonQuery();
            }

            using (var secondConnection = new SqliteConnection(connectionString))
            {
                secondConnection.Open();
                var queryCommand = secondConnection.CreateCommand();
                queryCommand.CommandText =
                @"
                    SELECT *
                    FROM data
                ";
                var value = (string)queryCommand.ExecuteScalar();
                Console.WriteLine(value);
            }

            // After all the connections are closed, the database is deleted.
            masterConnection.Close();
        }
    }
}
