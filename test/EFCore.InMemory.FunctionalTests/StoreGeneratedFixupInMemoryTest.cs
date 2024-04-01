// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class StoreGeneratedFixupInMemoryTest(StoreGeneratedFixupInMemoryTest.StoreGeneratedFixupInMemoryFixture fixture) : StoreGeneratedFixupTestBase<
    StoreGeneratedFixupInMemoryTest.StoreGeneratedFixupInMemoryFixture>(fixture)
{
    public override void Temporary_value_equals_database_generated_value()
    {
        // In-memory doesn't use real store-generated values.
    }

    [ConditionalFact]
    public void InMemory_database_does_not_use_temp_values()
    {
        using var context = CreateContext();
        var entry = context.Add(new TestTemp());

        Assert.False(entry.Property(e => e.Id).IsTemporary);
        Assert.False(entry.Property(e => e.NotId).IsTemporary);

        var tempValue = entry.Property(e => e.Id).CurrentValue;

        context.SaveChanges();

        Assert.False(entry.Property(e => e.Id).IsTemporary);
        Assert.Equal(tempValue, entry.Property(e => e.Id).CurrentValue);
    }

    protected override async Task ExecuteWithStrategyInTransactionAsync(Func<DbContext, Task> testOperation)
    {
        await base.ExecuteWithStrategyInTransactionAsync(testOperation);
        await Fixture.ReseedAsync();
    }

    protected override bool EnforcesFKs
        => false;

    public class StoreGeneratedFixupInMemoryFixture : StoreGeneratedFixupFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<Child>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ParentPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ChildPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ParentDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ChildDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ParentNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ChildNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<CategoryDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ProductDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<CategoryPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ProductPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<CategoryNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<ProductNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedNever();
                    b.Property(e => e.Id2).ValueGeneratedNever();
                });

            modelBuilder.Entity<Item>(b => b.Property(e => e.Id).ValueGeneratedNever());

            modelBuilder.Entity<Game>(b => b.Property(e => e.Id).ValueGeneratedNever());
        }
    }
}
