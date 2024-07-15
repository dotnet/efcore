// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

public abstract class MigrationsInfrastructureTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : MigrationsInfrastructureFixtureBase, new()
{
    protected TFixture Fixture { get; }

    protected MigrationsInfrastructureTestBase(TFixture fixture)
    {
        Fixture = fixture;
        Fixture.TestStore.CloseConnection();
        Fixture.TestSqlLoggerFactory.Clear();
    }

    protected string Sql { get; private set; }

    protected string ActiveProvider { get; private set; }

    public static IEnumerable<object[]> IsAsyncData = [[false], [true]];

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

        MigrationsInfrastructureFixtureBase.MigratorPlugin.ResetCounts();
        db.Database.Migrate((c, d) =>
        {
            c.Add(new MigrationsInfrastructureFixtureBase.Foo { Id = 1, Bar = 10, Description = "Test" });
            c.SaveChanges();
        });

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId),
            x => Assert.Equal("00000000000005_Migration5", x.MigrationId),
            x => Assert.Equal("00000000000006_Migration6", x.MigrationId),
            x => Assert.Equal("00000000000007_Migration7", x.MigrationId));

        Assert.NotNull(db.Find<MigrationsInfrastructureFixtureBase.Foo>(1));

        Assert.Equal(1, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratingCallCount);
        Assert.Equal(1, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratedCallCount);
        Assert.Equal(0, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratingAsyncCallCount);
        Assert.Equal(0, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratedAsyncCallCount);
    }

    [ConditionalFact]
    public virtual async Task Can_apply_all_migrations_async()
    {
        using var db = Fixture.CreateContext();
        await db.Database.EnsureDeletedAsync();

        await GiveMeSomeTimeAsync(db);

        MigrationsInfrastructureFixtureBase.MigratorPlugin.ResetCounts();
        await db.Database.MigrateAsync(async (c, d, ct) =>
        {
            c.Add(new MigrationsInfrastructureFixtureBase.Foo { Id = 1, Bar = 10, Description = "Test" });
            await c.SaveChangesAsync(ct);
        });

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            await history.GetAppliedMigrationsAsync(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId),
            x => Assert.Equal("00000000000005_Migration5", x.MigrationId),
            x => Assert.Equal("00000000000006_Migration6", x.MigrationId),
            x => Assert.Equal("00000000000007_Migration7", x.MigrationId));

        Assert.NotNull(await db.FindAsync<MigrationsInfrastructureFixtureBase.Foo>(1));

        Assert.Equal(0, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratingCallCount);
        Assert.Equal(0, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratedCallCount);
        Assert.Equal(1, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratingAsyncCallCount);
        Assert.Equal(1, MigrationsInfrastructureFixtureBase.MigratorPlugin.MigratedAsyncCallCount);
    }

    [ConditionalFact]
    public virtual void Can_apply_range_of_migrations()
    {
        using var db = Fixture.CreateContext();
        db.Database.EnsureDeleted();

        GiveMeSomeTime(db);

        db.Database.Migrate(null, "Migration6");

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId),
            x => Assert.Equal("00000000000005_Migration5", x.MigrationId),
            x => Assert.Equal("00000000000006_Migration6", x.MigrationId));
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

        Assert.Equal(LogLevel.Error,
            Fixture.TestSqlLoggerFactory.Log.Single(l => l.Id == RelationalEventId.PendingModelChangesWarning).Level);
    }

    [ConditionalFact]
    public virtual void Can_revert_all_migrations()
    {
        using var db = Fixture.CreateContext();
        db.Database.EnsureDeleted();

        GiveMeSomeTime(db);

        var migrator = db.GetService<IMigrator>();
        migrator.Migrate("Migration5");
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

        var migrator = db.GetService<IMigrator>();
        migrator.Migrate("Migration5");
        migrator.Migrate("Migration4");

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId));
    }

    [ConditionalFact]
    public virtual void Can_apply_one_migration_in_parallel()
    {
        using var db = Fixture.CreateContext();
        db.Database.EnsureDeleted();
        GiveMeSomeTime(db);
        db.GetService<IRelationalDatabaseCreator>().Create();

        Parallel.For(0, Environment.ProcessorCount, i =>
        {
            using var context = Fixture.CreateContext();
            var migrator = context.GetService<IMigrator>();
            migrator.Migrate("Migration1");
        });

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
    }

    [ConditionalFact]
    public virtual async Task Can_apply_one_migration_in_parallel_async()
    {
        using var db = Fixture.CreateContext();
        await db.Database.EnsureDeletedAsync();
        await GiveMeSomeTimeAsync(db);
        await db.GetService<IRelationalDatabaseCreator>().CreateAsync();

        await Parallel.ForAsync(0, Environment.ProcessorCount, async (i, _) =>
        {
            using var context = Fixture.CreateContext();
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync("Migration1");
        });

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            await history.GetAppliedMigrationsAsync(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
    }

    [ConditionalFact]
    public virtual void Can_apply_second_migration_in_parallel()
    {
        using var db = Fixture.CreateContext();
        db.Database.EnsureDeleted();
        GiveMeSomeTime(db);
        db.GetService<IMigrator>().Migrate("Migration1");

        Parallel.For(0, Environment.ProcessorCount, i =>
        {
            using var context = Fixture.CreateContext();
            var migrator = context.GetService<IMigrator>();
            migrator.Migrate("Migration2");
        });

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId));
    }

    [ConditionalFact]
    public virtual async Task Can_apply_second_migration_in_parallel_async()
    {
        using var db = Fixture.CreateContext();
        await db.Database.EnsureDeletedAsync();
        await GiveMeSomeTimeAsync(db);
        await db.GetService<IMigrator>().MigrateAsync("Migration1");

        await Parallel.ForAsync(0, Environment.ProcessorCount, async (i, _) =>
        {
            using var context = Fixture.CreateContext();
            var migrator = context.GetService<IMigrator>();
            await migrator.MigrateAsync("Migration2");
        });

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            await history.GetAppliedMigrationsAsync(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId));
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
        var sourceModel = context.GetService<IModelRuntimeInitializer>().Initialize(
            snapshot.Model, designTime: true, validationLogger: null);

        var modelDiffer = context.GetService<IMigrationsModelDiffer>();
        var operations = modelDiffer.GetDifferences(
            sourceModel.GetRelationalModel(),
            context.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        Assert.Equal(0, operations.Count);
    }

    private void SetSql(string value)
        => Sql = value.Replace(ProductInfo.GetVersion(), "7.0.0-test");
}

public abstract class MigrationsInfrastructureFixtureBase
    : SharedStoreFixtureBase<MigrationsInfrastructureFixtureBase.MigrationsContext>
{
    public static string ActiveProvider { get; set; }

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
    {
        TestStore.UseConnectionString = true;
        return base.AddServices(serviceCollection)
            .AddSingleton<IMigratorPlugin, MigratorPlugin>();
    }

    protected override string StoreName
        => "MigrationsTest";

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public EmptyMigrationsContext CreateEmptyContext()
        => new(
            TestStore.AddProviderOptions(
                    new DbContextOptionsBuilder())
                .UseInternalServiceProvider(
                    TestStoreFactory.AddProviderServices(
                            new ServiceCollection())
                        .BuildServiceProvider(validateScopes: true))
                .Options);

    public class EmptyMigrationsContext(DbContextOptions options) : DbContext(options);

    public class MigrationsContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Foo> Foos { get; set; }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<Foo>(b => b.ToTable("Table1"));
    }

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(e => e
            .Log(RelationalEventId.PendingModelChangesWarning)
            .Log(RelationalEventId.NonTransactionalMigrationOperationWarning)
        );

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Migrations.Name;

    public class Foo
    {
        public int Id { get; set; }
        public int Bar { get; set; }
        public string Description { get; set; }
    }

    public class MigratorPlugin : IMigratorPlugin
    {
        public static int MigratedCallCount { get; private set; }
        public static int MigratedAsyncCallCount { get; private set; }
        public static int MigratingCallCount { get; private set; }
        public static int MigratingAsyncCallCount { get; private set; }

        public static void ResetCounts()
        {
            MigratedCallCount = 0;
            MigratedAsyncCallCount = 0;
            MigratingCallCount = 0;
            MigratingAsyncCallCount = 0;
        }

        public void Migrated(DbContext context, IMigratorData data)
        {
            MigratedCallCount++;
        }

        public Task MigratedAsync(DbContext context, IMigratorData data, CancellationToken cancellationToken)
        {
            MigratedAsyncCallCount++;
            return Task.CompletedTask;
        }

        public void Migrating(DbContext context, IMigratorData data)
            => MigratingCallCount++;

        public Task MigratingAsync(DbContext context, IMigratorData data, CancellationToken cancellationToken = default)
        {
            MigratingAsyncCallCount++;
            return Task.CompletedTask;
        }
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
                    columns: x => new { Id = x.Column<int>(), Foo = x.Column<int>(), Description = x.Column<string>() })
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

    [DbContext(typeof(MigrationsContext))]
    [Migration("00000000000004_Migration4")]
    private class Migration4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql("""
                CREATE PROCEDURE [dbo].[GotoReproduction]
                AS
                BEGIN
                    DECLARE @Counter int;
                    SET @Counter = 1;
                    WHILE @Counter < 10
                    BEGIN
                        SELECT @Counter
                        SET @Counter = @Counter + 1
                        IF @Counter = 4 GOTO Branch_One --Jumps to the first branch.
                        IF @Counter = 5 GOTO Branch_Two --This will never execute.
                    END
                    Branch_One:
                        SELECT 'Jumping To Branch One.'
                        GOTO Branch_Three; --This will prevent Branch_Two from executing.
                    Branch_Two:
                        SELECT 'Jumping To Branch Two.'
                    Branch_Three:
                        SELECT 'Jumping To Branch Three.'
                END;
            """);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }

    [DbContext(typeof(MigrationsContext))]
    [Migration("00000000000005_Migration5")]
    private class Migration5 : Migration
    {
        public const string TestValue = """
            Value With

            Empty Lines
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"INSERT INTO Table1 (Id, Bar, Description) VALUES (-1, 3, '{TestValue}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }

    [DbContext(typeof(MigrationsContext))]
    [Migration("00000000000006_Migration6")]
    private class Migration6 : Migration
    {
        public const string TestValue = """
            GO
            Value With

            Empty Lines
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"INSERT INTO Table1 (Id, Bar, Description) VALUES (-2, 4, '{TestValue}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }

    [DbContext(typeof(MigrationsContext))]
    [Migration("00000000000007_Migration7")]
    private class Migration7 : Migration
    {
        public const string TestValue = """
            GO
            Value With

            GO

            Empty Lines
            GO
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"INSERT INTO Table1 (Id, Bar, Description) VALUES (-3, 5, '{TestValue}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
