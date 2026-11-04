// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

public abstract partial class MigrationsInfrastructureTestBase<TFixture>
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual async Task Completed_migration_models_can_be_collected(bool async, bool down)
    {
        using var context = CreateRetentionContext();
        context.Database.EnsureDeleted();
        GiveMeSomeTime(context);

        if (down)
        {
            context.State.VerifyCollection = false;
            context.Database.Migrate();
            context.State.Reset();
        }

        if (async)
        {
            await context.Database.MigrateAsync(down ? Migration.InitialDatabase : null);
        }
        else
        {
            context.Database.Migrate(down ? Migration.InitialDatabase : null);
        }

        Assert.Equal(down ? 4 : 5, context.State.Models.Count);
        Assert.Equal(down ? 0 : 5, context.GetService<IHistoryRepository>().GetAppliedMigrations().Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual void Completed_migration_models_can_be_collected_while_generating_script(bool down)
    {
        using var context = CreateRetentionContext();
        var script = context.GetService<IMigrator>().GenerateScript(
            down ? "00000000000005_Retention5" : null,
            down ? Migration.InitialDatabase : null);

        Assert.Equal(down ? 4 : 5, context.State.Models.Count);
        Assert.Contains("00000000000001_Retention1", script);
        Assert.Contains("00000000000005_Retention5", script);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Target_model_cache_survives_migration_execution(bool async)
    {
        using var context = CreateRetentionContext();
        context.State.VerifyCollection = false;
        context.State.RetainFirstMigration = true;
        context.Database.EnsureDeleted();
        GiveMeSomeTime(context);

        if (async)
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            context.Database.Migrate();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.NotNull(context.State.Models[0].Target);
        Assert.Same(context.State.Models[0].Target, context.State.RetainedMigration!.TargetModel);
        Assert.Equal(5, context.State.Models.Count);
    }

    private MigrationRetentionContext CreateRetentionContext()
        => new(
            Fixture.TestStore.AddProviderOptions(new DbContextOptionsBuilder())
                .ReplaceService<IMigrationsAssembly, MigrationRetentionContext.RetentionMigrationsAssembly>()
                .Options);
}

internal sealed class MigrationRetentionContext(DbContextOptions options) : DbContext(options)
{
    public RetentionState State { get; } = new();

    public sealed class RetentionState
    {
        public List<WeakReference> Models { get; } = [];
        public bool VerifyCollection { get; set; } = true;
        public bool RetainFirstMigration { get; set; }
        public Migration? RetainedMigration { get; set; }

        public void Reset()
        {
            Models.Clear();
            VerifyCollection = true;
        }

        public void ModelBuilding(ModelBuilder builder)
        {
            if (VerifyCollection && Models.Count >= 3)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Assert.False(Models[0].IsAlive);
            }

            Models.Add(new WeakReference(builder.Model));
            builder.Entity("RetentionEntity").Property<int>("Id");
        }
    }

    public sealed class RetentionMigrationsAssembly : MigrationsAssembly
    {
        private readonly RetentionState _state;

        public RetentionMigrationsAssembly(
            ICurrentDbContext currentContext,
            IDbContextOptions options,
            IMigrationsIdGenerator idGenerator,
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
            : base(currentContext, options, idGenerator, logger)
            => _state = ((MigrationRetentionContext)currentContext.Context).State;

        public override Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
        {
            var migration = (RetentionMigration)base.CreateMigration(migrationClass, activeProvider);
            migration.State = _state;
            if (_state.RetainFirstMigration && migration is Retention1)
            {
                _state.RetainedMigration = migration;
            }

            return migration;
        }
    }

    private abstract class RetentionMigration : Migration
    {
        public RetentionState State { get; set; } = null!;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }

        protected override void BuildTargetModel(ModelBuilder modelBuilder)
            => State.ModelBuilding(modelBuilder);
    }

    [DbContext(typeof(MigrationRetentionContext)), Migration("00000000000001_Retention1")]
    private sealed class Retention1 : RetentionMigration;

    [DbContext(typeof(MigrationRetentionContext)), Migration("00000000000002_Retention2")]
    private sealed class Retention2 : RetentionMigration;

    [DbContext(typeof(MigrationRetentionContext)), Migration("00000000000003_Retention3")]
    private sealed class Retention3 : RetentionMigration;

    [DbContext(typeof(MigrationRetentionContext)), Migration("00000000000004_Retention4")]
    private sealed class Retention4 : RetentionMigration;

    [DbContext(typeof(MigrationRetentionContext)), Migration("00000000000005_Retention5")]
    private sealed class Retention5 : RetentionMigration;
}
