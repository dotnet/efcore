// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Framework.Logging;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteHistoryRepositoryTest
    {
        public static string EOL => Environment.NewLine;

        [Fact]
        public void Create_generates_command()
        {
            var hp = CreateSqliteHistoryRepo();

            var columnDef = "    MigrationId TEXT PRIMARY KEY," + EOL +
                            "    ContextKey TEXT NOT NULL," + EOL +
                            "    ProductVersion TEXT NOT NULL" + EOL +
                            ");";

            Assert.Equal("CREATE TABLE \"__migrationHistory\" (" + EOL + columnDef,
                hp.Create(ifNotExists: false));

            Assert.Equal("CREATE TABLE IF NOT EXISTS \"__migrationHistory\" (" + EOL + columnDef,
                hp.Create(ifNotExists: true));
        }

        [Fact]
        public void Scripting_not_supported()
        {
            var hp = CreateSqliteHistoryRepo();
            var methods = new Action[]
            {
                () => hp.BeginIfExists("any"),
                () => hp.BeginIfNotExists("any"),
                () => hp.EndIf()
            };
            foreach (var method in methods)
            {
                var ex = Assert.Throws<NotSupportedException>(method);
                Assert.Equal(Strings.MigrationScriptGenerationNotSupported, ex.Message);
            }
        }

        [Fact]
        public void GetDeleteOperation_deletes_row()
        {
            var hp = CreateSqliteHistoryRepo();
            var expected = new SqlOperation
            {
                Sql = "DELETE FROM \"__migrationHistory\" WHERE \"MigrationId\" = 'exodus';"
            };
            var actual = hp.GetDeleteOperation("exodus") as SqlOperation;
            Assert.Equal(expected.IsDestructiveChange, actual.IsDestructiveChange);
            Assert.Equal(expected.Sql, actual.Sql);
            Assert.Equal(expected.SuppressTransaction, actual.SuppressTransaction);
        }

        [Fact]
        public void GetInsertOperation_deletes_row()
        {
            var hp = CreateSqliteHistoryRepo();
            var typename = typeof(TestContext).FullName;
            var expected = new SqlOperation
            {
                Sql = $"INSERT INTO \"__migrationHistory\" (\"MigrationId\", \"ContextKey\", \"ProductVersion\") VALUES ('m5', '{typename}', '7');"
            };
            var historyRow = new HistoryRow("m5", "7");
            var actual = hp.GetInsertOperation(historyRow) as SqlOperation;
            Assert.Equal(expected.IsDestructiveChange, actual.IsDestructiveChange);
            Assert.Equal(expected.Sql, actual.Sql);
            Assert.Equal(expected.SuppressTransaction, actual.SuppressTransaction);
        }

        [Fact]
        public void Exists_finds_existing_table()
        {
            var connection = SqliteTestConnection.CreateScratch();
            connection.Open();
            using (var cmd = connection.DbConnection.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE __migrationHistory (column1);";
                connection.Open();
                cmd.ExecuteNonQuery();
            }

            var hp = new SqliteHistoryRepository(connection, new TestContext(), new SqliteUpdateSqlGenerator());

            Assert.True(hp.Exists());
        }

        [Fact]
        public void Exists_no_table()
        {
            var connection = SqliteTestConnection.CreateScratch();

            var hp = new SqliteHistoryRepository(connection, new TestContext(), new SqliteUpdateSqlGenerator());

            Assert.False(hp.Exists());
        }

        [Fact]
        public void GetAppliedMigrations_finds_migrations()
        {
            var testConnection = SqliteTestConnection.CreateScratch();
            testConnection.Open();
            using (var command = testConnection.DbConnection.CreateCommand())
            {
                command.CommandText = CreateSqliteHistoryRepo().Create(true);
                command.ExecuteNonQuery();
            }
            using (var command = testConnection.DbConnection.CreateCommand())
            {
                command.CommandText = "INSERT INTO __migrationHistory VALUES ('different_context','SomeFakeContext','1');";
                command.ExecuteNonQuery();
            }

            var row = new HistoryRow("Mig1", "7");
            using (var command = testConnection.DbConnection.CreateCommand())
            {
                var operation = CreateSqliteHistoryRepo()
                    .GetInsertOperation(row) as SqlOperation;
                command.CommandText = operation?.Sql;
                command.ExecuteNonQuery();
            }

            var hp = new SqliteHistoryRepository(testConnection, new TestContext(), new SqliteUpdateSqlGenerator());

            Assert.Collection(hp.GetAppliedMigrations(), p =>
                {
                    Assert.Equal(row.MigrationId, p.MigrationId);
                    Assert.Equal(row.ProductVersion, p.ProductVersion);
                });
            testConnection.Close();
        }

        public class SqliteTestConnection : SqliteDatabaseConnection
        {
            public SqliteTestConnection(IDbContextOptions options)
                : base(options, new LoggerFactory())
            {
            }

            private static int _scratchCount;

            private string _fileName;

            public static SqliteTestConnection CreateScratch()
            {
                var options = new DbContextOptionsBuilder();
                string name;
                do
                {
                    name = "scratch-" + Interlocked.Increment(ref _scratchCount) + ".db";
                }
                while (File.Exists(name));
                options.UseSqlite("Data Source=" + name);
                var connection = new SqliteTestConnection(options.Options) { _fileName = name };
                return connection;
            }

            public override void Dispose()
            {
                base.Dispose();
                if (_fileName != null)
                {
                    File.Delete(_fileName);
                }
            }

            public static SqliteTestConnection InMemory()
            {
                var options = new DbContextOptionsBuilder();
                options.UseSqlite("Data Source=:memory:");
                return new SqliteTestConnection(options.Options);
            }
        }

        private static SqliteHistoryRepository CreateSqliteHistoryRepo() => new SqliteHistoryRepository(SqliteTestConnection.InMemory(), new TestContext(), new SqliteUpdateSqlGenerator());

        private class TestContext : DbContext
        {
        }
    }
}
