// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteHistoryRepositoryTest
    {
        private static string EOL => Environment.NewLine;

        [Fact]
        public void GetCreateScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateScript();

            Assert.Equal(
                "CREATE TABLE \"__MigrationHistory\" (" + EOL +
                "    \"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK_HistoryRow\" PRIMARY KEY," + EOL +
                "    \"ProductVersion\" TEXT" + EOL +
                ");" + EOL,
                sql);
        }

        [Fact]
        public void GetCreateIfNotExistsScript_works()
        {
            var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

            Assert.Equal(
                "CREATE TABLE IF NOT EXISTS \"__MigrationHistory\" (" + EOL +
                "    \"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK_HistoryRow\" PRIMARY KEY," + EOL +
                "    \"ProductVersion\" TEXT" + EOL +
                ");" + EOL,
                sql);
        }

        [Fact]
        public void GetDeleteScript_works()
        {
            var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

            Assert.Equal(
                "DELETE FROM \"__MigrationHistory\"" + EOL +
                "WHERE \"MigrationId\" = 'Migration1';",
                sql);
        }

        [Fact]
        public void GetInsertScript_works()
        {
            var sql = CreateHistoryRepository().GetInsertScript(
                new HistoryRow("Migration1", "7.0.0"));

            Assert.Equal(
                "INSERT INTO \"__MigrationHistory\" (\"MigrationId\", \"ProductVersion\")" + EOL +
                "VALUES ('Migration1', '7.0.0');",
                sql);
        }

        [Fact]
        public void GetBeginIfNotExistsScript_works()
        {
            var repository = CreateHistoryRepository();
            var ex = Assert.Throws<NotSupportedException>(() => repository.GetBeginIfNotExistsScript("Migration1"));

            Assert.Equal(Strings.MigrationScriptGenerationNotSupported, ex.Message);
        }

        [Fact]
        public void GetBeginIfExistsScript_works()
        {
            var repository = CreateHistoryRepository();
            var ex = Assert.Throws<NotSupportedException>(() => repository.GetBeginIfExistsScript("Migration1"));

            Assert.Equal(Strings.MigrationScriptGenerationNotSupported, ex.Message);
        }

        [Fact]
        public void GetEndIfScript_works()
        {
            var repository = CreateHistoryRepository();
            var ex = Assert.Throws<NotSupportedException>(() => repository.GetEndIfScript());

            Assert.Equal(Strings.MigrationScriptGenerationNotSupported, ex.Message);
        }

        private static IHistoryRepository CreateHistoryRepository()
        {
            var annotationsProvider = new SqliteMetadataExtensionProvider();
            var updateSqlGenerator = new SqliteUpdateSqlGenerator();

            return new SqliteHistoryRepository(
                Mock.Of<IRelationalDatabaseCreator>(),
                Mock.Of<ISqlStatementExecutor>(),
                Mock.Of<IRelationalConnection>(),
                new MigrationModelFactory(),
                new DbContextOptions<DbContext>(
                    new Dictionary<Type, IDbContextOptionsExtension>
                    {
                        { typeof(SqliteOptionsExtension), new SqliteOptionsExtension() }
                    }),
                new ModelDiffer(
                    annotationsProvider,
                    new SqliteMigrationAnnotationProvider()),
                new SqliteMigrationSqlGenerator(
                    updateSqlGenerator,
                    new SqliteTypeMapper(),
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
