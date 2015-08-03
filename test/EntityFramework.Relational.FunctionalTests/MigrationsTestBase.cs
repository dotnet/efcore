// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.History;
using Microsoft.Data.Entity.Migrations.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class MigrationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : MigrationsFixtureBase, new()
    {
        private readonly TFixture _fixture;

        public MigrationsTestBase(TFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void Can_apply_all_migrations()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();

                db.Database.ApplyMigrations();

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
                    x => Assert.Equal("00000000000002_Migration2", x.MigrationId));
            }
        }

        [Fact]
        public void Can_apply_one_migration()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();

                var migrator = db.GetService().GetRequiredService<IMigrator>();
                migrator.ApplyMigrations("Migration1");

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
            }
        }

        [Fact]
        public void Can_revert_all_migrations()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();
                db.Database.ApplyMigrations();

                var migrator = db.GetService().GetRequiredService<IMigrator>();
                migrator.ApplyMigrations(Migrator.InitialDatabase);

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Empty(history.GetAppliedMigrations());
            }
        }

        [Fact]
        public void Can_revert_one_migrations()
        {
            using (var db = _fixture.CreateContext())
            {
                db.Database.EnsureDeleted();
                db.Database.ApplyMigrations();

                var migrator = db.GetService().GetRequiredService<IMigrator>();
                migrator.ApplyMigrations("Migration1");

                var history = db.GetService().GetRequiredService<IHistoryRepository>();
                Assert.Collection(
                    history.GetAppliedMigrations(),
                    x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
            }
        }

        /// <remarks>
        ///     Creating databases and executing DDL is slow. This oddly-structured test allows us to get the most ammount of
        ///     coverage using the least ammount of database operations.
        /// </remarks>
        [Fact]
        public virtual async Task Can_execute_operations()
        {
            using (var db = _fixture.CreateContext())
            {
                await db.Database.EnsureDeletedAsync();
                await db.Database.EnsureCreatedAsync();

                var services = db.GetService();
                var connection = db.Database.GetDbConnection();

                await db.Database.OpenConnectionAsync();

                await ExecuteAsync(services, BuildFirstMigration);
                await AssertFirstMigrationAsync(connection);
                await ExecuteAsync(services, BuildSecondMigration);
                await AssertSecondMigrationAsync(connection);
            }
        }

        protected virtual async Task ExecuteAsync(IServiceProvider services, Action<MigrationBuilder> buildMigration)
        {
            var generator = services.GetRequiredService<IMigrationSqlGenerator>();
            var connection = services.GetRequiredService<IRelationalConnection>();
            var executor = services.GetRequiredService<ISqlStatementExecutor>();

            var migrationBuilder = new MigrationBuilder();
            buildMigration(migrationBuilder);
            var operations = migrationBuilder.Operations.ToList();

            var batches = generator.Generate(operations, model: null);

            using (var transaction = await connection.BeginTransactionAsync())
            {
                await executor.ExecuteNonQueryAsync(connection, transaction.DbTransaction, batches);
                transaction.Commit();
            }
        }

        protected virtual void BuildFirstMigration(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CreatedTable",
                columns: x => new
                {
                    Id = x.Column<int>(nullable: false),
                    ColumnWithDefaultToDrop = x.Column<int>(nullable: true, defaultValue: 0),
                    ColumnWithDefaultToAlter = x.Column<int>(nullable: true, defaultValue: 1)
                },
                constraints: x =>
                {
                    x.PrimaryKey(
                        name: "PK_CreatedTable",
                        columns: t => t.Id);
                });
        }

        protected virtual Task AssertFirstMigrationAsync(DbConnection connection)
        {
            AssertFirstMigration(connection);

            return Task.FromResult(0);
        }

        protected virtual void AssertFirstMigration(DbConnection connection)
        {
        }

        protected virtual void BuildSecondMigration(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColumnWithDefaultToDrop",
                table: "CreatedTable");
            migrationBuilder.AlterColumn<int>(
                name: "ColumnWithDefaultToAlter",
                table: "CreatedTable",
                nullable: true);
        }

        protected virtual Task AssertSecondMigrationAsync(DbConnection connection)
        {
            AssertSecondMigration(connection);

            return Task.FromResult(0);
        }

        protected virtual void AssertSecondMigration(DbConnection connection)
        {
        }
    }
}
