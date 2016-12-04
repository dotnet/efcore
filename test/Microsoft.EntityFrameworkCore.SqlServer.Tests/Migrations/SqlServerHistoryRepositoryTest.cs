// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Tests.Migrations
{
    public class SqlServerHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [Fact]
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

        [Fact]
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

        [Fact]
        public void GetCreateIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

            Assert.Equal(
                "IF OBJECT_ID(N'__EFMigrationsHistory') IS NULL" + EOL +
                "BEGIN" + EOL +
                "    CREATE TABLE [__EFMigrationsHistory] (" + EOL +
                "        [MigrationId] nvarchar(150) NOT NULL," + EOL +
                "        [ProductVersion] nvarchar(32) NOT NULL," + EOL +
                "        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])" + EOL +
                "    );" + EOL +
                "END;" + EOL,
                sql);
        }

        [Fact]
        public void GetCreateIfNotExistsScript_works_with_schema()
        {
            var sql = CreateHistoryRepository("my").GetCreateIfNotExistsScript();

            Assert.Equal(
                "IF OBJECT_ID(N'my.__EFMigrationsHistory') IS NULL" + EOL +
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

        [Fact]
        public void GetDeleteScript_works()
        {
            var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

            Assert.Equal(
                "DELETE FROM [__EFMigrationsHistory]" + EOL +
                "WHERE [MigrationId] = N'Migration1';" + EOL,
                sql);
        }

        [Fact]
        public void GetInsertScript_works()
        {
            var sql = CreateHistoryRepository().GetInsertScript(
                new HistoryRow("Migration1", "7.0.0"));

            Assert.Equal(
                "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])" + EOL +
                "VALUES (N'Migration1', N'7.0.0');" + EOL,
                sql);
        }

        [Fact]
        public void GetBeginIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfNotExistsScript("Migration1");

            Assert.Equal(
                "IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'Migration1')" + EOL +
                "BEGIN",
                sql);
        }

        [Fact]
        public void GetBeginIfExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetBeginIfExistsScript("Migration1");

            Assert.Equal(
                "IF EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'Migration1')" + EOL +
                "BEGIN",
                sql);
        }

        [Fact]
        public void GetEndIfScript_works()
        {
            var sql = CreateHistoryRepository().GetEndIfScript();

            Assert.Equal("END;" + EOL, sql);
        }

        private static IHistoryRepository CreateHistoryRepository(string schema = null)
        {
            var annotationsProvider = new SqlServerAnnotationProvider();
            var sqlGenerator = new SqlServerSqlGenerationHelper();
            var typeMapper = new SqlServerTypeMapper();

            var commandBuilderFactory = new RelationalCommandBuilderFactory(
                new FakeSensitiveDataLogger<RelationalCommandBuilderFactory>(),
                new DiagnosticListener("Fake"),
                typeMapper);

            return new SqlServerHistoryRepository(
                Mock.Of<IRelationalDatabaseCreator>(),
                Mock.Of<IRawSqlCommandBuilder>(),
                Mock.Of<ISqlServerConnection>(),
                new DbContextOptions<DbContext>(
                    new Dictionary<Type, IDbContextOptionsExtension>
                    {
                        {
                            typeof(SqlServerOptionsExtension),
                            new SqlServerOptionsExtension { MigrationsHistoryTableSchema = schema }
                        }
                    }),
                new MigrationsModelDiffer(
                    new SqlServerTypeMapper(),
                    annotationsProvider,
                    new SqlServerMigrationsAnnotationProvider()),
                new SqlServerMigrationsSqlGenerator(
                    commandBuilderFactory,
                    new SqlServerSqlGenerationHelper(),
                    typeMapper,
                    annotationsProvider,
                    new SqlServerMigrationsAnnotationProvider()),
                annotationsProvider,
                sqlGenerator);
        }

        private class Context : DbContext
        {
        }
    }
}
