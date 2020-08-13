using System;
using Microsoft.Data.Sqlite;

namespace CollationSample
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
                CREATE TABLE greek_letter (
                    value TEXT
                );

                INSERT INTO greek_letter
                VALUES ('Λ'),
                       ('λ');
            ";
            createCommand.ExecuteNonQuery();

            // Without this, the query returns one since the built-in NOCASE collation only
            // handles ASCII characters (A-Z)
            connection.CreateCollation("NOCASE", (x, y) => string.Compare(x, y, ignoreCase: true));

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText =
            @"
                SELECT count()
                FROM greek_letter
                WHERE value = 'λ' COLLATE NOCASE
            ";
            var count = (long)queryCommand.ExecuteScalar();

            Console.WriteLine($"Results: {count}");
        }
    }
}
