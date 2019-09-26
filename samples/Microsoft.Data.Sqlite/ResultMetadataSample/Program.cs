using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace ResultMetadataSample
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
                CREATE TABLE post (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    title TEXT NOT NULL UNIQUE,
                    body TEXT
                );
            ";
            createCommand.ExecuteNonQuery();

            var queryCommand = connection.CreateCommand();
            queryCommand.CommandText =
            @"
                SELECT id AS post_id,
                       title,
                       body,
                       random() AS random
                FROM post;
            ";

            var reader = queryCommand.ExecuteReader();

            var schemaTable = reader.GetSchemaTable();
            foreach (DataRow column in schemaTable.Rows)
            {
                if ((bool)column[SchemaTableColumn.IsExpression])
                {
                    Console.Write("(expression) ");
                }
                else
                {
                    Console.Write($"{column[SchemaTableColumn.BaseColumnName]} ");
                }

                if ((bool)column[SchemaTableColumn.IsAliased])
                {
                    Console.Write($"AS {column[SchemaTableColumn.ColumnName]} ");
                }

                Console.Write($"{column["DataTypeName"]} ");

                if (column[SchemaTableColumn.AllowDBNull] as bool? == false)
                {
                    Console.Write("NOT NULL ");
                }

                if (column[SchemaTableColumn.IsKey] as bool? == true)
                {
                    Console.Write("PRIMARY KEY ");
                }

                if (column[SchemaTableOptionalColumn.IsAutoIncrement] as bool? == true)
                {
                    Console.Write("AUTOINCREMENT ");
                }

                if (column[SchemaTableColumn.IsUnique] as bool? == true)
                {
                    Console.Write("UNIQUE ");
                }

                Console.WriteLine();
            }
        }
    }
}
