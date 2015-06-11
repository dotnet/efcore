// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Sqlite;
using Moq;
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
                    () => hp.EndIf(),
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
            var connection = new SqliteConnection("Data Source=:memory:");
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE __migrationHistory (column1);";
                connection.Open();
                cmd.ExecuteNonQuery();
            }
            var mockConnection = new Mock<IRelationalConnection>();
            mockConnection.SetupGet(p => p.DbConnection).Returns(connection);

            var hp = new SqliteHistoryRepository(mockConnection.Object, new TestContext(), new SqliteSqlGenerator());

            Assert.True(hp.Exists());
        }

        [Fact]
        public void Exists_no_table()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            var mockConnection = new Mock<IRelationalConnection>();
            mockConnection.SetupGet(p => p.DbConnection).Returns(connection);

            var hp = new SqliteHistoryRepository(mockConnection.Object, new TestContext(), new SqliteSqlGenerator());

            Assert.False(hp.Exists());
        }

        [Fact]
        public void GetAppliedMigrations_finds_migrations()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            var mockConnection = new Mock<IRelationalConnection>();
            mockConnection.SetupGet(p => p.DbConnection).Returns(connection);
            using (var command = connection.CreateCommand())
            {
                command.CommandText = CreateSqliteHistoryRepo().Create(true);
                connection.Open();
                command.ExecuteNonQuery();
            }
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO __migrationHistory VALUES ('different_context','SomeFakeContext','1');";
                command.ExecuteNonQuery();
            }


            var row = new HistoryRow("Mig1", "7");
            using (var command = connection.CreateCommand())
            {
                var operation = CreateSqliteHistoryRepo()
                    .GetInsertOperation(row) as SqlOperation;
                command.CommandText = operation?.Sql;
                command.ExecuteNonQuery();
            }


            var hp = new SqliteHistoryRepository(mockConnection.Object, new TestContext(), new SqliteSqlGenerator());

            Assert.Collection(hp.GetAppliedMigrations(), p =>
                {
                    Assert.Equal(row.MigrationId, p.MigrationId);
                    Assert.Equal(row.ProductVersion, p.ProductVersion);
                });

        }

        private static SqliteHistoryRepository CreateSqliteHistoryRepo() => new SqliteHistoryRepository(Mock.Of<IRelationalConnection>(), new TestContext(), new SqliteSqlGenerator());

        private class TestContext : DbContext
        {
        }
    }
}
