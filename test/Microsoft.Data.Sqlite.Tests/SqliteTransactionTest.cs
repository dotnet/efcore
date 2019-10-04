// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class SqliteTransactionTest
    {
        [Fact]
        public void Ctor_sets_read_uncommitted()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:;Cache=Shared"))
            {
                connection.Open();

                using (connection.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    Assert.Equal(1L, connection.ExecuteScalar<long>("PRAGMA read_uncommitted;"));
                }
            }
        }

        [Fact]
        public void Ctor_unsets_read_uncommitted_when_serializable()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    Assert.Equal(0L, connection.ExecuteScalar<long>("PRAGMA read_uncommitted;"));
                }
            }
        }

        [Theory]
        [InlineData(IsolationLevel.Chaos)]
        [InlineData(IsolationLevel.Snapshot)]
        public void Ctor_throws_when_invalid_isolation_level(IsolationLevel isolationLevel)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var ex = Assert.Throws<ArgumentException>(() => connection.BeginTransaction(isolationLevel));

                Assert.Equal(Resources.InvalidIsolationLevel(isolationLevel), ex.Message);
            }
        }

        [Fact]
        public void ReadUncommitted_allows_dirty_reads()
        {
            const string connectionString = "Data Source=read-uncommitted;Mode=Memory;Cache=Shared";

            using (var connection1 = new SqliteConnection(connectionString))
            using (var connection2 = new SqliteConnection(connectionString))
            {
                connection1.Open();
                connection2.Open();

                connection1.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

                using (connection1.BeginTransaction())
                using (connection2.BeginTransaction(IsolationLevel.ReadUncommitted))
                {
                    connection1.ExecuteNonQuery("UPDATE Data SET Value = 1;");

                    var value = connection2.ExecuteScalar<long>("SELECT * FROM Data;");

                    Assert.Equal(1, value);
                }
            }
        }

        [Fact]
        public void Serialized_disallows_dirty_reads()
        {
            const string connectionString = "Data Source=serialized;Mode=Memory;Cache=Shared";

            using (var connection1 = new SqliteConnection(connectionString))
            using (var connection2 = new SqliteConnection(connectionString))
            {
                connection1.Open();
                connection2.Open();

                connection1.ExecuteNonQuery("CREATE TABLE Data (Value); INSERT INTO Data VALUES (0);");

                using (connection1.BeginTransaction())
                {
                    connection1.ExecuteNonQuery("UPDATE Data SET Value = 1;");

                    connection2.DefaultTimeout = 0;

                    var ex = Assert.Throws<SqliteException>(
                        () =>
                        {
                            using (connection2.BeginTransaction(IsolationLevel.Serializable))
                            {
                                connection2.ExecuteScalar<long>("SELECT * FROM Data;");
                            }
                        });

                    Assert.Equal(raw.SQLITE_LOCKED, ex.SqliteErrorCode);
                    Assert.Equal(raw.SQLITE_LOCKED_SHAREDCACHE, ex.SqliteExtendedErrorCode);
                }
            }
        }

        [Fact]
        public void IsolationLevel_throws_when_completed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();
                transaction.Dispose();

                var ex = Assert.Throws<InvalidOperationException>(() => transaction.IsolationLevel);

                Assert.Equal(Resources.TransactionCompleted, ex.Message);
            }
        }

        [Fact]
        public void IsolationLevel_is_inferred_when_unspecified()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:;Cache=Shared"))
            {
                connection.Open();
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 1;");

                using (var transaction = connection.BeginTransaction())
                {
                    Assert.Equal(IsolationLevel.ReadUncommitted, transaction.IsolationLevel);
                }
            }
        }

        [Theory]
        [InlineData(IsolationLevel.ReadUncommitted)]
        [InlineData(IsolationLevel.ReadCommitted)]
        [InlineData(IsolationLevel.RepeatableRead)]
        public void IsolationLevel_is_increased_when_unsupported(IsolationLevel isolationLevel)
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction(isolationLevel))
                {
                    Assert.Equal(IsolationLevel.Serializable, transaction.IsolationLevel);
                }
            }
        }

        [Fact]
        public void Commit_throws_when_completed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();
                transaction.Dispose();

                var ex = Assert.Throws<InvalidOperationException>(() => transaction.Commit());

                Assert.Equal(Resources.TransactionCompleted, ex.Message);
            }
        }

        [Fact]
        public void Commit_throws_when_completed_externally()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    connection.ExecuteNonQuery("ROLLBACK;");

                    var ex = Assert.Throws<InvalidOperationException>(() => transaction.Commit());

                    Assert.Equal(Resources.TransactionCompleted, ex.Message);
                }
            }
        }

        [Fact]
        public void Commit_throws_when_connection_closed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    connection.Close();

                    var ex = Assert.Throws<InvalidOperationException>(() => transaction.Commit());

                    Assert.Equal(Resources.TransactionCompleted, ex.Message);
                }
            }
        }

        [Fact]
        public void Commit_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                CreateTestTable(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    connection.ExecuteNonQuery("INSERT INTO TestTable VALUES (1);");

                    transaction.Commit();

                    Assert.Null(connection.Transaction);
                    Assert.Null(transaction.Connection);
                }

                Assert.Equal(1L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));
            }
        }

        [Fact]
        public void Rollback_noops_once_when_completed_externally()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    connection.ExecuteNonQuery("ROLLBACK;");

                    transaction.Rollback();
                    var ex = Assert.Throws<InvalidOperationException>(() => transaction.Rollback());

                    Assert.Equal(Resources.TransactionCompleted, ex.Message);
                }
            }
        }

        [Fact]
        public void Rollback_throws_when_completed()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();
                transaction.Dispose();

                var ex = Assert.Throws<InvalidOperationException>(() => transaction.Rollback());

                Assert.Equal(Resources.TransactionCompleted, ex.Message);
            }
        }

        [Fact]
        public void Rollback_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                CreateTestTable(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    connection.ExecuteNonQuery("INSERT INTO TestTable VALUES (1);");

                    transaction.Rollback();

                    Assert.Null(connection.Transaction);
                    Assert.Null(transaction.Connection);
                }

                Assert.Equal(0L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));
            }
        }

        [Fact]
        public void Dispose_works()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();
                CreateTestTable(connection);

                using (var transaction = connection.BeginTransaction())
                {
                    connection.ExecuteNonQuery("INSERT INTO TestTable VALUES (1);");

                    transaction.Dispose();

                    Assert.Null(connection.Transaction);
                    Assert.Null(transaction.Connection);
                }

                Assert.Equal(0L, connection.ExecuteScalar<long>("SELECT COUNT(*) FROM TestTable;"));
            }
        }

        [Fact]
        public void Dispose_can_be_called_more_than_once()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                var transaction = connection.BeginTransaction();

                transaction.Dispose();
                transaction.Dispose();
            }
        }

        private static void CreateTestTable(SqliteConnection connection)
        {
            connection.ExecuteNonQuery(
                @"
                CREATE TABLE TestTable (
                    TestColumn INTEGER
                )");
        }
    }
}
