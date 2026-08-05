// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class StoreGeneratedFixupSqlServerTest(StoreGeneratedFixupSqlServerTest.StoreGeneratedFixupSqlServerFixture fixture)
    : StoreGeneratedFixupRelationalTestBase<
        StoreGeneratedFixupSqlServerTest.StoreGeneratedFixupSqlServerFixture>(fixture)
{
    [ConditionalFact]
    public Task Temp_values_are_replaced_on_save()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entry = context.Add(new TestTemp());

                Assert.True(entry.Property(e => e.Id).IsTemporary);
                Assert.False(entry.Property(e => e.NotId).IsTemporary);

                var tempValue = entry.Property(e => e.Id).CurrentValue;

                await context.SaveChangesAsync();

                Assert.False(entry.Property(e => e.Id).IsTemporary);
                Assert.NotEqual(tempValue, entry.Property(e => e.Id).CurrentValue);
            });

    protected override void MarkIdsTemporary(DbContext context, object dependent, object principal)
    {
        var entry = context.Entry(dependent);
        entry.Property("Id1").IsTemporary = true;
        entry.Property("Id2").IsTemporary = true;

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsForeignKey())
            {
                property.IsTemporary = true;
            }
        }

        entry = context.Entry(principal);
        entry.Property("Id1").IsTemporary = true;
        entry.Property("Id2").IsTemporary = true;
    }

    protected override void MarkIdsTemporary(DbContext context, object game, object level, object item)
    {
        var entry = context.Entry(game);
        entry.Property("Id").IsTemporary = true;

        entry = context.Entry(item);
        entry.Property("Id").IsTemporary = true;
    }

    protected override bool EnforcesFKs
        => true;

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class StoreGeneratedFixupSqlServerFixture : StoreGeneratedFixupRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Child>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ParentPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ChildPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ParentDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ChildDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ParentNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ChildNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<CategoryDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ProductDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<CategoryPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ProductPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<CategoryNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ProductNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Category>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Product>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Item>(b => b.Property(e => e.Id).ValueGeneratedOnAdd());

            modelBuilder.Entity<Game>(b => b.Property(e => e.Id).ValueGeneratedOnAdd().HasDefaultValueSql("newid()"));
        }
    }
}
