// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if WINDOWS_UWP

using System.IO;
using Windows.Storage;
using Xunit;

namespace Microsoft.Data.Sqlite.Tests
{
    public class SqliteTransactionTest
    {
        [Fact]
        public void Transactions_do_not_throw()
        {
            var path = Path.Combine(ApplicationData.Current.LocalFolder.Path, "test.db");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var connection = new SqliteConnection("Filename=" + path))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                DROP TABLE IF EXISTS table1;
                CREATE TABLE table1(Id INTEGER PRIMARY KEY, Value INT);
                INSERT INTO table1 (Value) VALUES ('transaction test');

                BEGIN; -- <<< throws if temp dir not set correctly
                INSERT INTO table1 (Value) VALUES ('value 2');

                CREATE TABLE temp_table2 ( Id INT, Value TEXT);
                INSERT INTO temp_table2 SELECT * FROM table1;";
                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT count(*) from temp_table2;";
                    Assert.Equal(2L, command.ExecuteScalar());

                    command.CommandText = "ROLLBACK;";
                    command.ExecuteNonQuery();

                    command.CommandText = "SELECT count(*) FROM table1;";
                    Assert.Equal(1L, command.ExecuteScalar());
                }
            }
        }
    }
}

#endif
