using System;
using Microsoft.Data.Sqlite;

using static SQLitePCL.raw;

namespace InteropSample
{
    class Program
    {
        static void Main()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                // Get the underlying sqlite3 object
                var db = connection.Handle;
                sqlite3_trace(
                    db,
                    (_, statement) => Console.WriteLine(statement),
                    null);

                // You could also use db.ptr to access the native sqlite3 struct to, for example,
                // invoke a [DllImport] method.

                var command = connection.CreateCommand();
                command.CommandText = "SELECT $value";
                command.Parameters.AddWithValue("$value", "Trace me!");

                using (var reader = command.ExecuteReader())
                {
                    // Get the underlying sqlite3_stmt object
                    var stmt = reader.Handle;
                    var steps = sqlite3_stmt_status(
                        stmt,
                        SQLITE_STMTSTATUS_VM_STEP,
                        resetFlg: 0);
                    Console.WriteLine($"VM operations: {steps}");

                    // Likewise, use stmt.ptr to access the native sqlite3_stmt struct.
                }
            }
        }
    }
}
