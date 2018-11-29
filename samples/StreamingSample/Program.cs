using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace StreamingSample
{
    class Program
    {
        static async Task Main()
        {
            var connection = new SqliteConnection("Data Source=StreamingSample.db");
            connection.Open();

            var createCommand = connection.CreateCommand();
            createCommand.CommandText =
            @"
                CREATE TABLE data (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    value BLOB
                )
            ";
            createCommand.ExecuteNonQuery();

            // You can reduce memory usage while reading and writing large objects by streaming
            // the data into and out of the database. This can be especially useful when
            // parsing or transforming the data

            using (var inputStream = File.OpenRead("input.txt"))
            {
                // Start by inserting a row as normal. Use the zeroblob() function to allocate
                // space in the database for the large object. The last_insert_rowid() function
                // provides a convenient way to get its rowid
                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText =
                @"
                    INSERT INTO data(value)
                    VALUES (zeroblob($length));

                    SELECT last_insert_rowid();
                ";
                insertCommand.Parameters.AddWithValue("$length", inputStream.Length);
                var rowid = (long)insertCommand.ExecuteScalar();

                // After inserting the row, open a stream to write the large object
                using (var writeStream = new SqliteBlob(connection, "data", "value", rowid))
                {
                    Console.WriteLine("Writing the large object...");

                    // NB: Although SQLite doesn't support async, other types of streams do
                    await inputStream.CopyToAsync(writeStream);
                }
            }

            using (var outputStream = Console.OpenStandardOutput())
            {
                // To stream the large object, you must select the rowid or one of its aliases as
                // show here in addition to the large object's column. If you don't, the entire
                // object will be loaded into memory
                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText =
                @"
                    SELECT id, value
                    FROM data
                    LIMIT 1
                ";
                using (var reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        using (var readStream = reader.GetStream(1))
                        {
                            Console.WriteLine("Reading the large object...");
                            await readStream.CopyToAsync(outputStream);
                        }
                    }
                }
            }

            // Clean up
            connection.Close();
            File.Delete("StreamingSample.db");
        }
    }
}
