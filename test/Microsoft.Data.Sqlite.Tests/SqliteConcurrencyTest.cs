// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.IO;
using Microsoft.Data.Sqlite.Interop;
using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class SqliteConcurrencyTest : IDisposable
    {
        private const int SQLITE_BUSY = 5;
        private const int SQLITE_LOCKED = 6;

        public SqliteConcurrencyTest()
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS a (b);
INSERT INTO a VALUES (1);
INSERT INTO a VALUES (2);";
                command.ExecuteNonQuery();
            }
        }

        [Fact]
        public void It_throws_sqlite_locked()
        {
            using (var connection = CreateConnection())
            {
                var selectCommand = connection.CreateCommand();
                connection.Open();
                selectCommand.CommandText = "SELECT * FROM a;";

                var insertCommand = connection.CreateCommand();
                insertCommand.CommandText = "DROP TABLE a;";

                using (var reader = selectCommand.ExecuteReader())
                {
                    reader.Read();
                    var ex = Assert.Throws<SqliteException>(() => insertCommand.ExecuteNonQuery());

                    Assert.Equal(SQLITE_LOCKED, ex.SqliteErrorCode);
                    var message = NativeMethods.sqlite3_errstr(SQLITE_LOCKED);
                    Assert.Equal(Strings.FormatSqliteNativeError(SQLITE_LOCKED, message), ex.Message);
                }

                insertCommand.ExecuteNonQuery();
                Assert.Throws<SqliteException>(() => insertCommand.ExecuteNonQuery());
            }
        }

        [Fact]
        public void It_throws_sqlite_busy()
        {
            using (var connection = CreateConnection())
            {
                var selectCommand = connection.CreateCommand();
                connection.Open();
                selectCommand.CommandText = "SELECT * FROM a;";
                using (var connection2 = CreateConnection())
                {
                    var insertCommand = connection2.CreateCommand();
                    connection2.Open();
                    insertCommand.CommandText = "DROP TABLE a;";
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        reader.Read();
                        var ex = Assert.Throws<SqliteException>(() => insertCommand.ExecuteNonQuery());

                        Assert.Equal(SQLITE_BUSY, ex.SqliteErrorCode);
                        var message = NativeMethods.sqlite3_errstr(SQLITE_BUSY);
                        Assert.Equal(Strings.FormatSqliteNativeError(SQLITE_BUSY, message), ex.Message);
                    }

                    insertCommand.ExecuteNonQuery();
                    Assert.Throws<SqliteException>(() => insertCommand.ExecuteNonQuery());
                }
            }
        }

        private const string FileName = "./concurrency.db";

        private DbConnection CreateConnection() => new SqliteConnection("Data Source=" + FileName);

        public void Dispose()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }
    }
}
