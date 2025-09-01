// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;
public class CosmosTransactionalBatchTest(CosmosTransactionalBatchTest.CosmosFixture fixture) : IClassFixture<CosmosTransactionalBatchTest.CosmosFixture>
{
    private const string DatabaseName = "CosmosTransactionalBatchTest";
    protected CosmosFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public virtual async Task SaveChanges_fails_for_duplicate_key_in_same_partition_prevents_other_inserts_in_same_partition_even_if_staged_before_add()
    {
        using (var arrangeContext = CreateContext())
        {
            await Fixture.TestStore.CleanAsync(arrangeContext);
            arrangeContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
            await arrangeContext.SaveChangesAsync();
        }

        using var context = CreateContext();

        context.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        var updateException =
            await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Equal(1, updateException.Entries.Count);
        Assert.IsAssignableFrom<Customer>(updateException.Entries.First().Entity);

        using var assertContext = CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(1, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_fails_for_duplicate_key_in_same_partition_writes_only_partition_staged_before_error()
    {
        using (var arrangeContext = CreateContext())
        {
            await Fixture.TestStore.CleanAsync(arrangeContext);
            arrangeContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
            arrangeContext.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
            await arrangeContext.SaveChangesAsync();
        }

        using var context = CreateContext();

        context.Customers.Add(new Customer { Id = "4", PartitionKey = "2" });
        context.Customers.Add(new Customer { Id = "3", PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "5", PartitionKey = "2" });
        context.Customers.Add(new Customer { Id = "6", PartitionKey = "3" });

        var updateException =
            await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Equal(1, updateException.Entries.Count);
        Assert.IsAssignableFrom<Customer>(updateException.Entries.First().Entity);
        Assert.IsAssignableFrom<Customer>(updateException.Entries.Last().Entity);

        using var assertContext = CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        // The batch from the first partition found should have executed (partition 2, ids: 4,5)
        // No other batches after (and including) batch for partition 1 should have executed
        Assert.Equal(4, customersCount);
    }

    protected ConcurrencyContext CreateContext()
        => Fixture.CreateContext();

    public class CosmosFixture : SharedStoreFixtureBase<ConcurrencyContext>
    {
        protected override string StoreName
            => DatabaseName;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }

    public class ConcurrencyContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; } = null!;

        public DbSet<Order> Orders { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Customer>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.ETag).IsETagConcurrency();
                    b.OwnsMany(x => x.Children);
                    b.HasPartitionKey(c => c.PartitionKey);
                });

            builder.Entity<Order>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.HasPartitionKey(c => c.PartitionKey);
                });
        }
    }

    public class Customer
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public string? ETag { get; set; }

        public string? PartitionKey { get; set; }

        public ICollection<DummyChild> Children { get; } = new HashSet<DummyChild>();
    }

    public class DummyChild
    {
        public string? Id { get; init; }
    }

    public class Order
    {
        public string? Id { get; set; }

        public string? PartitionKey { get; set; }
    }
}
