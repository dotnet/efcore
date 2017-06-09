// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    public class SqliteHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [Fact]
        public void GetCreateScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateScript();

            Assert.Equal(
                "CREATE TABLE \"__EFMigrationsHistory\" (" + EOL +
                "    \"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY," + EOL +
                "    \"ProductVersion\" TEXT NOT NULL" + EOL +
                ");" + EOL,
                sql);
        }

        [Fact]
        public void GetCreateIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

            Assert.Equal(
                "CREATE TABLE IF NOT EXISTS \"__EFMigrationsHistory\" (" + EOL +
                "    \"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY," + EOL +
                "    \"ProductVersion\" TEXT NOT NULL" + EOL +
                ");" + EOL,
                sql);
        }

        [Fact]
        public void GetDeleteScript_works()
        {
            var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

            Assert.Equal(
                "DELETE FROM \"__EFMigrationsHistory\"" + EOL +
                "WHERE \"MigrationId\" = 'Migration1';" + EOL,
                sql);
        }

        [Fact]
        public void GetInsertScript_works()
        {
            var sql = CreateHistoryRepository().GetInsertScript(
                new HistoryRow("Migration1", "7.0.0"));

            Assert.Equal(
                "INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\")" + EOL +
                "VALUES ('Migration1', '7.0.0');" + EOL,
                sql);
        }

        [Fact]
        public void GetBeginIfNotExistsScript_works()
        {
            var repository = CreateHistoryRepository();
            var ex = Assert.Throws<NotSupportedException>(() => repository.GetBeginIfNotExistsScript("Migration1"));

            Assert.Equal(SqliteStrings.MigrationScriptGenerationNotSupported, ex.Message);
        }

        [Fact]
        public void GetBeginIfExistsScript_works()
        {
            var repository = CreateHistoryRepository();
            var ex = Assert.Throws<NotSupportedException>(() => repository.GetBeginIfExistsScript("Migration1"));

            Assert.Equal(SqliteStrings.MigrationScriptGenerationNotSupported, ex.Message);
        }

        [Fact]
        public void GetEndIfScript_works()
        {
            var repository = CreateHistoryRepository();
            var ex = Assert.Throws<NotSupportedException>(() => repository.GetEndIfScript());

            Assert.Equal(SqliteStrings.MigrationScriptGenerationNotSupported, ex.Message);
        }

        private static IHistoryRepository CreateHistoryRepository()
        {
            var sqlGenerator = new SqliteSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());
            var typeMapper = new SqliteTypeMapper(new RelationalTypeMapperDependencies());

            return new SqliteHistoryRepository(
                new HistoryRepositoryDependencies(
                    Mock.Of<IRelationalDatabaseCreator>(),
                    Mock.Of<IRawSqlCommandBuilder>(),
                    Mock.Of<IRelationalConnection>(),
                    new DbContextOptions<DbContext>(
                        new Dictionary<Type, IDbContextOptionsExtension>
                        {
                            { typeof(SqliteOptionsExtension), new SqliteOptionsExtension() }
                        }),
                    new MigrationsModelDiffer(
                        typeMapper,
                        new SqliteMigrationsAnnotationProvider(new MigrationsAnnotationProviderDependencies()),
                        new FakeStateManager(),
                        RelationalTestHelpers.Instance.CreateCommandBatchPreparer()),
                    new SqliteMigrationsSqlGenerator(
                        new MigrationsSqlGeneratorDependencies(
                            new RelationalCommandBuilderFactory(
                                new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                                typeMapper),
                            new FakeSqlGenerator(
                                new UpdateSqlGeneratorDependencies(
                                    new RelationalSqlGenerationHelper(
                                        new RelationalSqlGenerationHelperDependencies()),
                                    typeMapper)),
                            new SqliteSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                            typeMapper)),
                    sqlGenerator));
        }

        private class Context : DbContext
        {
        }
    }
}
