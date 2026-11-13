using System.Net;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public class CosmosTransactionalBatchTest(CosmosTransactionalBatchTest.CosmosFixture fixture)
    : IClassFixture<CosmosTransactionalBatchTest.CosmosFixture>, IAsyncLifetime
{
    private const string DatabaseName = nameof(CosmosTransactionalBatchTest);

    protected CosmosFixture Fixture { get; } = fixture;

    [Fact]
    public virtual async Task
        SaveChanges_fails_for_duplicate_key_in_same_partition_prevents_other_inserts_in_same_partition_even_if_staged_before_add()
    {
        using (var arrangeContext = Fixture.CreateContext())
        {
            arrangeContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
            await arrangeContext.SaveChangesAsync();
        }

        using var context = Fixture.CreateContext();

        context.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        var updateException =
            await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Equal(1, updateException.Entries.Count);
        Assert.IsAssignableFrom<Customer>(updateException.Entries.First().Entity);

        using var assertContext = Fixture.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(1, customersCount);
    }

    [Fact]
    public virtual async Task SaveChanges_transaction_behavior_always_succeeds_for_100_entities_in_same_partition()
    {
        using var context = Fixture.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.Customers.AddRange(Enumerable.Range(0, 100).Select(value => new Customer { Id = value.ToString(), PartitionKey = "1" }));

        await context.SaveChangesAsync();

        using var assertContext = Fixture.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(100, customersCount);
    }

    public async ValueTask InitializeAsync()
    {
        using var context = Fixture.CreateContext();
        context.RemoveRange(
            await context.Set<Customer>().Select(customer => new Customer { Id = customer.Id, PartitionKey = customer.PartitionKey })
                .ToListAsync());
        await context.SaveChangesAsync();
    }

    public ValueTask DisposeAsync()
        => ValueTask.CompletedTask;

    public class CosmosFixture : SharedStoreFixtureBase<TransactionalBatchContext>
    {
        protected override string StoreName
            => DatabaseName;

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;
    }

    public class TransactionalBatchContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Customer>(entity =>
            {
                entity.HasKey(customer => customer.Id);
                entity.Property(customer => customer.ETag).IsETagConcurrency();
                entity.OwnsMany(customer => customer.Children);
                entity.HasPartitionKey(customer => customer.PartitionKey);
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
}