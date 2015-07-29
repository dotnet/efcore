// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [Fact]
        public void GetCreateScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateScript();

            Assert.Equal(
                "CREATE TABLE [__MigrationHistory] (" + EOL +
                "    [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "    [ProductVersion] nvarchar(32)," + EOL +
                "    CONSTRAINT [PK_HistoryRow] PRIMARY KEY ([MigrationId])" + EOL +
                ");" + EOL,
                sql);
        }

        [Fact]
        public void GetCreateIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

            Assert.Equal(
                "IF OBJECT_ID(N'__MigrationHistory') IS NULL" + EOL +
                "    CREATE TABLE [__MigrationHistory] (" + EOL +
                "        [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "        [ProductVersion] nvarchar(32)," + EOL +
                "        CONSTRAINT [PK_HistoryRow] PRIMARY KEY ([MigrationId])" + EOL +
                "    );" + EOL,
                sql);
        }

        [Fact]
        public void GetDeleteScript_works()
        {
            var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

            Assert.Equal(
                "DELETE FROM [__MigrationHistory]" + EOL +
                "WHERE [MigrationId] = N'Migration1';",
                sql);
        }

        [Fact]
        public void GetInsertScript_works()
        {
            var sql = CreateHistoryRepository().GetInsertScript(
                new HistoryRow("Migration1", "7.0.0"));

            Assert.Equal(
                "INSERT INTO [__MigrationHistory] ([MigrationId], [ProductVersion])" + EOL +
                "VALUES (N'Migration1', N'7.0.0');",
                sql);
        }

        [Fact]
        public void GetBeginIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfNotExistsScript("Migration1");

            Assert.Equal(
                "IF NOT EXISTS(SELECT * FROM [__MigrationHistory] WHERE [MigrationId] = N'Migration1')" + EOL +
                "BEGIN",
                sql);
        }

        [Fact]
        public void GetBeginIfExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfExistsScript("Migration1");

            Assert.Equal(
                "IF EXISTS(SELECT * FROM [__MigrationHistory] WHERE [MigrationId] = N'Migration1')" + EOL +
                "BEGIN",
                sql);
        }

        [Fact]
        public void GetEndIfScript_works()
        {
            var sql = CreateHistoryRepository().GetEndIfScript();

            Assert.Equal("END", sql);
        }

        private static IHistoryRepository CreateHistoryRepository()
        {
            var annotationsProvider = new SqlServerMetadataExtensionProvider();
            var updateSqlGenerator = new SqlServerUpdateSqlGenerator();

            return new SqlServerHistoryRepository(
                Mock.Of<IRelationalDatabaseCreator>(),
                Mock.Of<ISqlStatementExecutor>(),
                Mock.Of<ISqlServerConnection>(),
                new MigrationModelFactory(),
                new DbContextOptions<DbContext>(
                    new Dictionary<Type, IDbContextOptionsExtension>
                    {
                        { typeof(SqlServerOptionsExtension), new SqlServerOptionsExtension() }
                    }),
                new ModelDiffer(
                    annotationsProvider,
                    new SqlServerMigrationAnnotationProvider()),
                new SqlServerMigrationSqlGenerator(
                    updateSqlGenerator,
                    new SqlServerTypeMapper(),
                    annotationsProvider),
                annotationsProvider,
                updateSqlGenerator,
                Mock.Of<IServiceProvider>());
        }

        private class Context : DbContext
        {
        }
    }
}
