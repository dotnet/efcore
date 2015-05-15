// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations.History;
using Microsoft.Data.Entity.Relational.Migrations.Operations;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Migrations
{
    public class SqlServerHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [Fact]
        public void GetCreateOperation_works_when_ifNotExists_false()
        {
            var sql = CreateHistoryRepository().Create(ifNotExists: false);

            Assert.Equal(
                "CREATE TABLE [dbo].[__MigrationHistory] (" + EOL +
                "    [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "    [ContextKey] nvarchar(300) NOT NULL," + EOL +
                "    [ProductVersion] nvarchar(32) NOT NULL," + EOL +
                "    CONSTRAINT [PK_MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])" + EOL +
                ");",
                sql);
        }

        [Fact]
        public void GetCreateOperation_works_when_ifNotExists_true()
        {
            var sql = CreateHistoryRepository().Create(ifNotExists: true);

            Assert.Equal(
                "IF NOT EXISTS(SELECT * FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_SCHEMA] = N'dbo' AND [TABLE_NAME] = '__MigrationHistory' AND [TABLE_TYPE] = 'BASE TABLE')" + EOL +
                "    CREATE TABLE [dbo].[__MigrationHistory] (" + EOL +
                "        [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "        [ContextKey] nvarchar(300) NOT NULL," + EOL +
                "        [ProductVersion] nvarchar(32) NOT NULL," + EOL +
                "        CONSTRAINT [PK_MigrationHistory] PRIMARY KEY ([MigrationId], [ContextKey])" + EOL +
                "    );",
                sql);
        }

        [Fact]
        public void GetDeleteOperation_works()
        {
            var sqlOperation = (SqlOperation)CreateHistoryRepository().GetDeleteOperation("Migration1");

            Assert.Equal(
                "DELETE FROM [dbo].[__MigrationHistory]" + EOL +
                "WHERE [MigrationId] = 'Migration1' AND [ContextKey] = '" + typeof(Context).FullName + "';" + EOL,
                sqlOperation.Sql);
        }

        [Fact]
        public void GetInsertOperation_works()
        {
            var sqlOperation = (SqlOperation)CreateHistoryRepository().GetInsertOperation(
                new HistoryRow("Migration1", "7.0.0"));

            Assert.Equal(
                "INSERT INTO [dbo].[__MigrationHistory] ([MigrationId], [ContextKey], [ProductVersion])" + EOL +
                "VALUES ('Migration1', '" + typeof(Context).FullName + "', '7.0.0');" + EOL,
                sqlOperation.Sql);
        }

        [Fact]
        public void BeginIfNotExists_works()
        {
            var sql = CreateHistoryRepository().BeginIfNotExists("Migration1");

            Assert.Equal(
                "IF NOT EXISTS(SELECT * FROM [dbo].[__MigrationHistory] WHERE [MigrationId] = 'Migration1' AND [ContextKey] = '" + typeof(Context).FullName + "')" + EOL +
                "BEGIN",
                sql);
        }

        [Fact]
        public void BeginIfExists_works()
        {
            var sql = CreateHistoryRepository().BeginIfExists("Migration1");

            Assert.Equal(
                "IF EXISTS(SELECT * FROM [dbo].[__MigrationHistory] WHERE [MigrationId] = 'Migration1' AND [ContextKey] = '" + typeof(Context).FullName + "')" + EOL +
                "BEGIN",
                sql);
        }

        [Fact]
        public void EndIf_works()
        {
            var sql = CreateHistoryRepository().EndIf();

            Assert.Equal("END", sql);
        }

        private static IHistoryRepository CreateHistoryRepository()
        {
            return new SqlServerHistoryRepository(
                Mock.Of<ISqlServerConnection>(),
                Mock.Of<IRelationalDataStoreCreator>(),
                new Context(),
                new SqlServerSqlGenerator());
        }

        private class Context : DbContext
        {
        }
    }
}
