using System;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace BulkInsertSample
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
                CREATE TABLE data (
                    value INTEGER
                )
            ";
            createCommand.ExecuteNonQuery();

            // There is no special API for inserting data in bulk. For the best performance,
            // follow this pattern.

            Console.WriteLine("Inserting 150,000 rows...");
            var stopwatch = Stopwatch.StartNew();

            // Always use a transaction
            using (var transaction = connection.BeginTransaction())
            {
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText =
                @"
                    INSERT INTO data
                    VALUES ($value)
                ";

                // Re-use the same parameterized SqliteCommand
                var valueParameter = insertCommand.CreateParameter();
                valueParameter.ParameterName = "$value";
                insertCommand.Parameters.Add(valueParameter);

                // No need to call Prepare() since it's done lazily during the first execution.
                //insertCommand.Prepare();

                var random = new Random();

                for (int i = 0; i < 150_000; i++)
                {
                    valueParameter.Value = random.Next();
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            Console.WriteLine($"Done. (took {stopwatch.ElapsedMilliseconds} ms)");
        }
    }
}
