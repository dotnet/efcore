using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace DirtyReadSample
{
    class Program
    {
        static void Main()
        {
            // The connections must use a shared cache
            const string connectionString = "Data Source=DirtyReadSample;Mode=Memory;Cache=Shared";

            var firstConnection = new SqliteConnection(connectionString);
            firstConnection.Open();

            var createCommand = firstConnection.CreateCommand();
            createCommand.CommandText =
            @"
                CREATE TABLE data (
                    value TEXT
                );

                INSERT INTO data
                VALUES ('clean');
            ";
            createCommand.ExecuteNonQuery();

            using (firstConnection.BeginTransaction())
            {
                var updateCommand = firstConnection.CreateCommand();
                updateCommand.CommandText =
                @"
                    UPDATE data
                    SET value = 'dirty'
                ";
                updateCommand.ExecuteNonQuery();

                var secondConnection = new SqliteConnection(connectionString);
                secondConnection.Open();

                // Without ReadUncommitted, the command will time out since the table is locked
                // while the transaction on the first connection is active
                using (secondConnection.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    var queryCommand = secondConnection.CreateCommand();
                    queryCommand.CommandText =
                    @"
                        SELECT *
                        FROM data
                    ";
                    var value = (string)queryCommand.ExecuteScalar();
                    Console.WriteLine($"Value: {value}");
                }
            }
        }
    }
}
