// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqlServerHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [ConditionalFact]
        public void GetCreateScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateScript();

            Assert.Equal(
                "CREATE TABLE [__EFMigrationsHistory] (" + EOL +
                "    [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "    [ProductVersion] nvarchar(32) NOT NULL," + EOL +
                "    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])" + EOL +
                ");" + EOL,
                sql);
        }

        [ConditionalFact]
        public void GetCreateScript_works_with_schema()
        {
            var sql = CreateHistoryRepository("my").GetCreateScript();

            Assert.Equal(
                "IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my];');" + EOL +
                "CREATE TABLE [my].[__EFMigrationsHistory] (" + EOL +
                "    [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "    [ProductVersion] nvarchar(32) NOT NULL," + EOL +
                "    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])" + EOL +
                ");" + EOL,
                sql);
        }

        [ConditionalFact]
        public void GetCreateIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

            Assert.Equal(
                "IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL" + EOL +
                "BEGIN" + EOL +
                "    CREATE TABLE [__EFMigrationsHistory] (" + EOL +
                "        [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "        [ProductVersion] nvarchar(32) NOT NULL," + EOL +
                "        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])" + EOL +
                "    );" + EOL +
                "END;" + EOL,
                sql);
        }

        [ConditionalFact]
        public void GetCreateIfNotExistsScript_works_with_schema()
        {
            var sql = CreateHistoryRepository("my").GetCreateIfNotExistsScript();

            Assert.Equal(
                "IF OBJECT_ID(N'[my].[__EFMigrationsHistory]') IS NULL" + EOL +
                "BEGIN" + EOL +
                "    IF SCHEMA_ID(N'my') IS NULL EXEC(N'CREATE SCHEMA [my];');" + EOL +
                "    CREATE TABLE [my].[__EFMigrationsHistory] (" + EOL +
                "        [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "        [ProductVersion] nvarchar(32) NOT NULL," + EOL +
                "        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])" + EOL +
                "    );" + EOL +
                "END;" + EOL,
                sql);
        }

        [ConditionalFact]
        public void GetDeleteScript_works()
        {
            var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

            Assert.Equal(
                "DELETE FROM [__EFMigrationsHistory]" + EOL +
                "WHERE [MigrationId] = N'Migration1';" + EOL,
                sql);
        }

        [ConditionalFact]
        public void GetInsertScript_works()
        {
            var sql = CreateHistoryRepository().GetInsertScript(
                new HistoryRow("Migration1", "7.0.0"));

            Assert.Equal(
                "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])" + EOL +
                "VALUES (N'Migration1', N'7.0.0');" + EOL,
                sql);
        }

        [ConditionalFact]
        public void GetBeginIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfNotExistsScript("Migration1");

            Assert.Equal(
                "IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'Migration1')" + EOL +
                "BEGIN",
                sql);
        }

        [ConditionalFact]
        public void GetBeginIfExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfExistsScript("Migration1");

            Assert.Equal(
                "IF EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'Migration1')" + EOL +
                "BEGIN",
                sql);
        }

        [ConditionalFact]
        public void GetEndIfScript_works()
        {
            var sql = CreateHistoryRepository().GetEndIfScript();

            Assert.Equal("END;" + EOL, sql);
        }

        private static IHistoryRepository CreateHistoryRepository(string schema = null)
            => new DbContext(
                    new DbContextOptionsBuilder()
                        .UseInternalServiceProvider(SqlServerTestHelpers.Instance.CreateServiceProvider())
                        .UseSqlServer(
                            new SqlConnection("Database=DummyDatabase"),
                            b => b.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema))
                        .Options)
                .GetService<IHistoryRepository>();
    }
}
