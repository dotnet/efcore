using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace EncryptionSample
{
    class Program
    {
        static void Main()
        {
            const string connectionString = "Data Source=EncryptionSample.db";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Notice which packages are referenced by this project:
                // - Microsoft.Data.Sqlite.Core
                // - SQLitePCLRaw.bundle_sqlcipher

                // Immediately after opening the connection, send PRAGMA key to use encryption
                var keyCommand = connection.CreateCommand();
                keyCommand.CommandText =
                @"
                    PRAGMA key = 'password';
                ";
                keyCommand.ExecuteNonQuery();

                var createCommand = connection.CreateCommand();
                createCommand.CommandText =
                @"
                    CREATE TABLE data (
                        value TEXT
                    );

                    INSERT INTO data
                    VALUES ('Hello, encryption!');
                ";
                createCommand.ExecuteNonQuery();
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                Console.Write("Password (it's 'password'): ");
                var password = Console.ReadLine();

                // Sanitize the user input using the quote() function
                var quoteCommand = connection.CreateCommand();
                quoteCommand.CommandText =
                @"
                    SELECT quote($value)
                ";
                quoteCommand.Parameters.AddWithValue("$value", password);
                var quotedPassword = (string)quoteCommand.ExecuteScalar();

                // PRAGMA statements can't be parameterized. We're forced to concatenate the
                // escaped user input
                var keyCommand = connection.CreateCommand();
                keyCommand.CommandText =
                $@"
                    PRAGMA key = {quotedPassword}
                ";
                keyCommand.ExecuteScalar();

                try
                {
                    var queryCommand = connection.CreateCommand();
                    queryCommand.CommandText =
                    @"
                        SELECT *
                        FROM data
                    ";
                    var data = (string)queryCommand.ExecuteScalar();
                    Console.WriteLine(data);
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == SQLitePCL.raw.SQLITE_NOTADB)
                {
                    Console.WriteLine("Access denied.");
                }
            }

            // Clean up
            File.Delete("EncryptionSample.db");
        }
    }
}
