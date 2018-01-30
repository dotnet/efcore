using System;
using Microsoft.Data.Sqlite;

namespace BatchingSample
{
    class Program
    {
        static void Main()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            // SQLite doesn't support batching natively. Since there's no network involved, it
            // wouldn't really help with performance anyway. Batching is implemented in
            // Microsoft.Data.Sqlite as a convenience. For better command performance, see
            // BulkInsertSample.

            var createCommand = connection.CreateCommand();
            createCommand.CommandText =
            @"
                CREATE TABLE blog (
                    id INTEGER PRIMARY KEY,
                    name TEXT
                );

                CREATE TABLE post (
                    id INTEGER PRIMARY KEY,
                    title TEXT,
                    blog_id INTEGER NOT NULL,
                    FOREIGN KEY (blog_id) REFERENCES blog
                );

                INSERT INTO blog
                VALUES (1, 'Brice''s Blog');

                INSERT INTO post
                VALUES (1, 'Hello, World!', 1),
                       (2, 'SQLite on .NET Core', 1);
            ";
            createCommand.ExecuteNonQuery();

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText =
            @"
                SELECT *
                FROM blog;

                SELECT *
                FROM post;
            ";
            var reader = queryCommand.ExecuteReader();

            // Read the first result set
            while (reader.Read())
            {
                Console.WriteLine($"Blog {reader["id"]}: {reader["name"]}");
            }

            // Read the second result set
            reader.NextResult();

            while (reader.Read())
            {
                Console.WriteLine($"Post {reader["id"]} in blog {reader["blog_id"]}: {reader["title"]}");
            }
        }
    }
}
