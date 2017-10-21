using System;
using Microsoft.Data.Sqlite;

namespace DataChangedSample
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
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT
                );

                INSERT INTO user (name)
                VALUES ('Bryce'),
                        ('Jon');
            ";
            createCommand.ExecuteNonQuery();

            connection.Update +=
                (_, e) => Console.WriteLine($"{e.Event} {e.Table} {e.RowId}");

            var updateCommand = connection.CreateCommand();
            updateCommand.CommandText =
            @"
                UPDATE user
                SET name = 'Brice'
                WHERE name = 'Bryce';

                DELETE FROM user
                WHERE name = 'Jon';

                INSERT INTO user (name)
                VALUES ('Seth');
            ";
            updateCommand.ExecuteNonQuery();
        }
    }
}
