// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class MigrationsInfrastructureTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : MigrationsInfrastructureFixtureBase, new()
    {
        protected TFixture Fixture { get; }

        protected MigrationsInfrastructureTestBase(TFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestStore.CloseConnection();
        }

        protected string Sql { get; private set; }

        protected string ActiveProvider { get; private set; }

        // Database deletion can happen as async file operation and SQLClient
        // doesn't account for this, so give some time for it to happen on slow C.I. machines
        protected virtual void GiveMeSomeTime(DbContext db)
        {
            var stillExists = true;
            for (var i = 0; stillExists && i < 10; i++)
            {
                try
                {
                    Thread.Sleep(500);

                    stillExists = db.GetService<IRelationalDatabaseCreator>().Exists();
                }
                catch
                {
                }
            }
        }

        protected virtual async Task GiveMeSomeTimeAsync(DbContext db)
        {
            var stillExists = true;
            for (var i = 0; stillExists && i < 10; i++)
            {
                try
                {
                    await Task.Delay(500);

                    stillExists = await db.GetService<IRelationalDatabaseCreator>().ExistsAsync();
                }
                catch
                {
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_apply_all_migrations()
        {
            using var db = Fixture.CreateContext();
            db.Database.EnsureDeleted();

            GiveMeSomeTime(db);

            db.Database.Migrate();

            var history = db.GetService<IHistoryRepository>();
            Assert.Collection(
                history.GetAppliedMigrations(),
                x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
                x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
                x => Assert.Equal("00000000000003_Migration3", x.MigrationId));
        }

        [ConditionalFact]
        public virtual void Can_apply_one_migration()
        {
            using var db = Fixture.CreateContext();
            db.Database.EnsureDeleted();

            GiveMeSomeTime(db);

            var migrator = db.GetService<IMigrator>();
            migrator.Migrate("Migration1");

            var history = db.GetService<IHistoryRepository>();
            Assert.Collection(
                history.GetAppliedMigrations(),
                x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
        }

        [ConditionalFact]
        public virtual void Can_revert_all_migrations()
        {
            using var db = Fixture.CreateContext();
            db.Database.EnsureDeleted();

            GiveMeSomeTime(db);

            db.Database.Migrate();

            var migrator = db.GetService<IMigrator>();
            migrator.Migrate(Migration.InitialDatabase);

            var history = db.GetService<IHistoryRepository>();
            Assert.Empty(history.GetAppliedMigrations());
        }

        [ConditionalFact]
        public virtual void Can_revert_one_migrations()
        {
            using var db = Fixture.CreateContext();
            db.Database.EnsureDeleted();

            GiveMeSomeTime(db);

            db.Database.Migrate();

            var migrator = db.GetService<IMigrator>();
            migrator.Migrate("Migration1");

            var history = db.GetService<IHistoryRepository>();
            Assert.Collection(
                history.GetAppliedMigrations(),
                x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
        }

        [ConditionalFact]
        public virtual async Task Can_apply_all_migrations_async()
        {
            using var db = Fixture.CreateContext();
            await db.Database.EnsureDeletedAsync();

            await GiveMeSomeTimeAsync(db);

            await db.Database.MigrateAsync();

            var history = db.GetService<IHistoryRepository>();
            Assert.Collection(
                await history.GetAppliedMigrationsAsync(),
                x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
                x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
                x => Assert.Equal("00000000000003_Migration3", x.MigrationId));
        }

        [ConditionalFact]
        public virtual void Can_generate_no_migration_script()
        {
            using var db = Fixture.CreateEmptyContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(migrator.GenerateScript());
        }

        [ConditionalFact]
        public virtual void Can_generate_migration_from_initial_database_to_initial()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(migrator.GenerateScript(fromMigration: Migration.InitialDatabase, toMigration: Migration.InitialDatabase));
        }

        [ConditionalFact]
        public virtual void Can_generate_up_scripts()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(migrator.GenerateScript());
        }

        [ConditionalFact]
        public virtual void Can_generate_up_scripts_noTransactions()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(migrator.GenerateScript(options: MigrationsSqlGenerationOptions.NoTransactions));
        }

        [ConditionalFact]
        public virtual void Can_generate_one_up_script()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(migrator.GenerateScript(fromMigration: "00000000000001_Migration1", toMigration: "00000000000002_Migration2"));
        }

        [ConditionalFact]
        public virtual void Can_generate_up_script_using_names()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(migrator.GenerateScript(fromMigration: "Migration1", toMigration: "Migration2"));
        }

        [ConditionalFact]
        public virtual void Can_generate_idempotent_up_scripts()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(migrator.GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent));
        }

        [ConditionalFact]
        public virtual void Can_generate_idempotent_up_scripts_noTransactions()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(
                migrator.GenerateScript(
                    options: MigrationsSqlGenerationOptions.Idempotent
                    | MigrationsSqlGenerationOptions.NoTransactions));
        }

        [ConditionalFact]
        public virtual void Can_generate_down_scripts()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(
                migrator.GenerateScript(
                    fromMigration: "Migration2",
                    toMigration: Migration.InitialDatabase));
        }

        [ConditionalFact]
        public virtual void Can_generate_one_down_script()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(
                migrator.GenerateScript(
                    fromMigration: "00000000000002_Migration2",
                    toMigration: "00000000000001_Migration1"));
        }

        [ConditionalFact]
        public virtual void Can_generate_down_script_using_names()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(
                migrator.GenerateScript(
                    fromMigration: "Migration2",
                    toMigration: "Migration1"));
        }

        [ConditionalFact]
        public virtual void Can_generate_idempotent_down_scripts()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();

            SetSql(
                migrator.GenerateScript(
                    fromMigration: "Migration2",
                    toMigration: Migration.InitialDatabase,
                    MigrationsSqlGenerationOptions.Idempotent));
        }

        [ConditionalFact]
        public virtual void Can_get_active_provider()
        {
            using var db = Fixture.CreateContext();
            var migrator = db.GetService<IMigrator>();
            MigrationsInfrastructureFixtureBase.ActiveProvider = null;

            migrator.GenerateScript(toMigration: "Migration1");

            ActiveProvider = MigrationsInfrastructureFixtureBase.ActiveProvider;
        }

        [ConditionalFact]
        public abstract void Can_diff_against_2_2_model();

        [ConditionalFact]
        public abstract void Can_diff_against_3_0_ASP_NET_Identity_model();

        [ConditionalFact]
        public abstract void Can_diff_against_2_2_ASP_NET_Identity_model();

        [ConditionalFact]
        public abstract void Can_diff_against_2_1_ASP_NET_Identity_model();

        protected virtual void DiffSnapshot(ModelSnapshot snapshot, DbContext context)
        {
            var dependencies = context.GetService<ProviderConventionSetBuilderDependencies>();
            var relationalDependencies = context.GetService<RelationalConventionSetBuilderDependencies>();
            var typeMappingConvention = new TypeMappingConvention(dependencies);
            typeMappingConvention.ProcessModelFinalizing(((IConventionModel)snapshot.Model).Builder, null);

            var relationalModelConvention = new RelationalModelConvention(dependencies, relationalDependencies);
            var sourceModel = relationalModelConvention.ProcessModelFinalized(snapshot.Model);

            var modelDiffer = context.GetService<IMigrationsModelDiffer>();
            var operations = modelDiffer.GetDifferences(
                ((IMutableModel)sourceModel).FinalizeModel().GetRelationalModel(),
                context.Model.GetRelationalModel());

            Assert.Equal(0, operations.Count);
        }

        private void SetSql(string value)
            => Sql = value.Replace(ProductInfo.GetVersion(), "7.0.0-test");
    }

    public abstract class
        MigrationsInfrastructureFixtureBase : SharedStoreFixtureBase<MigrationsInfrastructureFixtureBase.MigrationsContext>
    {
        public static string ActiveProvider { get; set; }

        public new RelationalTestStore TestStore
            => (RelationalTestStore)base.TestStore;

        protected override string StoreName { get; } = "MigrationsTest";

        public EmptyMigrationsContext CreateEmptyContext()
            => new EmptyMigrationsContext(
                TestStore.AddProviderOptions(
                        new DbContextOptionsBuilder())
                    .UseInternalServiceProvider(
                        TestStoreFactory.AddProviderServices(
                                new ServiceCollection())
                            .BuildServiceProvider())
                    .Options);

        public new virtual MigrationsContext CreateContext()
            => base.CreateContext();

        public class EmptyMigrationsContext : DbContext
        {
            public EmptyMigrationsContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        public class MigrationsContext : PoolableDbContext
        {
            public MigrationsContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Foo> Foos { get; set; }
        }

        public class Foo
        {
            public int Id { get; set; }
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000001_Migration1")]
        private class Migration1 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                MigrationsInfrastructureFixtureBase.ActiveProvider = migrationBuilder.ActiveProvider;

                migrationBuilder
                    .CreateTable(
                        name: "Table1",
                        columns: x => new { Id = x.Column<int>(), Foo = x.Column<int>() })
                    .PrimaryKey(
                        name: "PK_Table1",
                        columns: x => x.Id);
            }

            protected override void Down(MigrationBuilder migrationBuilder)
                => migrationBuilder.DropTable("Table1");
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000002_Migration2")]
        private class Migration2 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
                => migrationBuilder.RenameColumn(
                    name: "Foo",
                    table: "Table1",
                    newName: "Bar");

            protected override void Down(MigrationBuilder migrationBuilder)
                => migrationBuilder.RenameColumn(
                    name: "Bar",
                    table: "Table1",
                    newName: "Foo");
        }

        [DbContext(typeof(MigrationsContext))]
        [Migration("00000000000003_Migration3")]
        private class Migration3 : Migration
        {
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
                {
                    migrationBuilder.Sql("CREATE DATABASE TransactionSuppressed;", suppressTransaction: true);
                    migrationBuilder.Sql("DROP DATABASE TransactionSuppressed;", suppressTransaction: true);
                }
            }

            protected override void Down(MigrationBuilder migrationBuilder)
            {
            }
        }
    }
}
