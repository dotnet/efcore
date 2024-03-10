// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class GraphUpdatesSqliteTestBase<TFixture> : GraphUpdatesTestBase<TFixture>
    where TFixture : GraphUpdatesSqliteTestBase<TFixture>.GraphUpdatesSqliteFixtureBase, new()
{
    protected GraphUpdatesSqliteTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
    public override Task Update_principal_with_shadow_key_owned_collection_throws(bool async)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
    public override Task Delete_principal_with_shadow_key_owned_collection_throws(bool async)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
    public override Task Clearing_shadow_key_owned_collection_throws(bool async, bool useUpdate, bool addNew)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
    public override Task Update_principal_with_CLR_key_owned_collection(bool async)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
    public override Task Delete_principal_with_CLR_key_owned_collection(bool async)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "Default owned collection pattern does not work with SQLite due to composite key.")]
    public override Task Clearing_CLR_key_owned_collection(bool async, bool useUpdate, bool addNew)
        => Task.CompletedTask;

    protected override IQueryable<Root> ModifyQueryRoot(IQueryable<Root> query)
        => query.AsSplitQuery();

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public abstract class GraphUpdatesSqliteFixtureBase : GraphUpdatesFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder.ConfigureWarnings(b => b.Ignore(SqliteEventId.CompositeKeyWithValueGeneration)));

        public override bool AutoDetectChanges
            => false;

        public override PoolableDbContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.AutoDetectChangesEnabled = AutoDetectChanges;

            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<OwnerRoot>(
                b =>
                {
                    b.OwnsMany(
                        e => e.OptionalChildren, b =>
                        {
                            b.HasKey("Id");
                            b.OwnsMany(
                                e => e.Children, b =>
                                {
                                    b.HasKey("Id");
                                });
                        });
                    b.OwnsMany(
                        e => e.RequiredChildren, b =>
                        {
                            b.HasKey("Id");
                            b.OwnsMany(
                                e => e.Children, b =>
                                {
                                    b.HasKey("Id");
                                });
                        });
                });

            modelBuilder.Entity<AccessState>(
                b =>
                {
                    b.Property(e => e.AccessStateId).ValueGeneratedNever();
                    b.HasData(new AccessState { AccessStateId = 1 });
                });

            modelBuilder.Entity<Cruiser>(
                b =>
                {
                    b.Property(e => e.IdUserState).HasDefaultValue(1);
                    b.HasOne(e => e.UserState).WithMany(e => e.Users).HasForeignKey(e => e.IdUserState);
                });

            modelBuilder.Entity<AccessStateWithSentinel>(
                b =>
                {
                    b.Property(e => e.AccessStateWithSentinelId).ValueGeneratedNever();
                    b.HasData(new AccessStateWithSentinel { AccessStateWithSentinelId = 1 });
                });

            modelBuilder.Entity<CruiserWithSentinel>(
                b =>
                {
                    b.Property(e => e.IdUserState).HasDefaultValue(1).HasSentinel(667);
                    b.HasOne(e => e.UserState).WithMany(e => e.Users).HasForeignKey(e => e.IdUserState);
                });

            modelBuilder.Entity<SomethingOfCategoryA>().Property<int>("CategoryId").HasDefaultValue(1);
            modelBuilder.Entity<SomethingOfCategoryB>().Property(e => e.CategoryId).HasDefaultValue(2);

            modelBuilder.Entity<CompositeKeyWith<int>>(
                b =>
                {
                    b.Property(e => e.PrimaryGroup).HasDefaultValue(1).HasSentinel(1);
                });

            modelBuilder.Entity<CompositeKeyWith<bool>>(
                b =>
                {
                    b.Property(e => e.PrimaryGroup).HasDefaultValue(true);
                });

            modelBuilder.Entity<CompositeKeyWith<bool?>>(
                b =>
                {
                    b.Property(e => e.PrimaryGroup).HasDefaultValue(true);
                });
        }
    }
}
