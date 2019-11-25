using System;
using System.IO;
using Microsoft.Data.Sqlite;

using static SQLitePCL.raw;

namespace EncryptionSample
{
    class Program
    {
        static void Main()
        {
            const string baseConnectionString = "Data Source=EncryptionSample.db";

            // Notice which packages are referenced by this project:
            // - Microsoft.Data.Sqlite.Core
            // - SQLitePCLRaw.bundle_sqlcipher

            // The Password keyword in the connection string specifies the encryption key
            var connectionString = new SqliteConnectionStringBuilder(baseConnectionString)
            {
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = "password"
            }.ToString();

            using (var connection = new SqliteConnection(connectionString))
            {
                // When a new database is created, it will be encrypted using the key
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE data (
                        value TEXT
                    );

                    INSERT INTO data
                    VALUES ('Hello, encryption!');
                ";
                command.ExecuteNonQuery();
            }

            Console.Write("Password (it's 'password'): ");
            var password = Console.ReadLine();

            connectionString = new SqliteConnectionStringBuilder(baseConnectionString)
            {
                Mode = SqliteOpenMode.ReadWrite,
                Password = password
            }.ToString();

            using (var connection = new SqliteConnection(connectionString))
            {
                try
                {
                    // If the key is incorrect, this will throw
                    connection.Open();
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == SQLITE_NOTADB)
                {
                    Console.WriteLine("Access denied.");
                    goto Cleanup;
                }

                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    SELECT *
                    FROM data
                ";
                var data = (string)command.ExecuteScalar();
                Console.WriteLine(data);
            }

            Cleanup:
            File.Delete("EncryptionSample.db");
        }
    }
}
