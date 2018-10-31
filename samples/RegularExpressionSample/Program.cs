using System;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace RegularExpressionSample
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
                CREATE TABLE user (
                    name TEXT,
                    bio TEXT
                );

                INSERT INTO user
                VALUES ('Arthur', 'I put two spaces after my full stops.  Always.'),
                       ('Brice', 'I used to double-space after periods. I stopped.');
            ";
            createCommand.ExecuteNonQuery();

            // Registering this function enables the REGEXP operator
            connection.CreateFunction(
                "regexp",
                (string pattern, string input)
                    => Regex.IsMatch(input, pattern));

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText =
            @"
                SELECT count()
                FROM user
                WHERE bio REGEXP '\w\. {2,}\w'
            ";
            var count = (long)queryCommand.ExecuteScalar();

            Console.WriteLine($"Double-spacers: {count}");
        }
    }
}
