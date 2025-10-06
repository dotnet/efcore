// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public class CosmosTransactionalBatchTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName
        => "CosmosTransactionalBatchTest";

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual async Task SaveChanges_fails_for_duplicate_key_in_same_partition_prevents_other_inserts_in_same_partition_even_if_staged_before_add()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using (var arrangeContext = contextFactory.CreateContext())
        {
            arrangeContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
            await arrangeContext.SaveChangesAsync();
        }

        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });

        var updateException =
            await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());

        Assert.Equal(1, updateException.Entries.Count);
        Assert.IsAssignableFrom<Customer>(updateException.Entries.First().Entity);

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(1, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_fails_for_duplicate_key_in_same_partition_writes_only_partition_staged_before_error()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using (var arrangeContext = contextFactory.CreateContext())
        {
            arrangeContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
            arrangeContext.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
            await arrangeContext.SaveChangesAsync();
        }

        using var context = contextFactory.CreateContext();

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

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        // The batch from the first partition found should have executed (partition 2, ids: 4,5)
        // No other batches after (and including) batch for partition 1 should have executed
        Assert.Equal(4, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transactionbehavior_never_fails_for_duplicate_key_in_same_partition_writes_all_staged_before_error()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using (var arrangeContext = contextFactory.CreateContext())
        {
            arrangeContext.Customers.Add(new Customer { Id = "1", PartitionKey = "1" });
            arrangeContext.Customers.Add(new Customer { Id = "2", PartitionKey = "1" });
            await arrangeContext.SaveChangesAsync();
        }

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

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

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        // The batch from the first partition found should have executed (partition 2, ids: 4,5)
        // No other batches after (and including) batch for partition 1 should have executed
        Assert.Equal(4, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transactionbehavior_always_fails_for_multiple_partitionkeys()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.Customers.Add(new Customer { Id = "4", PartitionKey = "2" });
        context.Customers.Add(new Customer { Id = "3", PartitionKey = "1" });

        var exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysAtomicity, exception.Message);

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(0, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transactionbehavior_always_fails_for_101_entities_in_same_partition()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.Customers.AddRange(Enumerable.Range(0, 101).Select(x => new Customer { Id = x.ToString(), PartitionKey = "1" }));

        var exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysAtomicity, exception.Message);

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(0, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transactionbehavior_always_succeeds_for_100_entities_in_same_partition()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.Customers.AddRange(Enumerable.Range(0, 100).Select(x => new Customer { Id = x.ToString(), PartitionKey = "1" }));

        await context.SaveChangesAsync();

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(100, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transactionbehavior_always_fails_for_multiple_entities_with_triggers()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.CustomersWithTrigger.Add(new CustomerWithTrigger { Id = "4", PartitionKey = "2" });
        context.CustomersWithTrigger.Add(new CustomerWithTrigger { Id = "3", PartitionKey = "1" });

        var exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysTriggerAtomicity, exception.Message);

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.CustomersWithTrigger.CountAsync();
        Assert.Equal(0, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transactionbehavior_always_succeeds_for_single_entity_with_trigger()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();

        var cosmosClient = context.Database.GetCosmosClient();
        var databaseId = context.Database.GetCosmosDatabaseId();
        var database = cosmosClient.GetDatabase(databaseId);

        // Get the container name from the Product entity type metadata
        var productEntityType = context.Model.FindEntityType(typeof(CustomerWithTrigger));
        var containerName = productEntityType!.GetContainer()!;
        var container = database.GetContainer(containerName);

        var preInsertTriggerDefinition = new TriggerProperties
        {
            Id = "trigger",
            TriggerType = TriggerType.Pre,
            TriggerOperation = TriggerOperation.All,
            Body = @"function trigger() {}"
        };

        try
        {
            await container.Scripts.CreateTriggerAsync(preInsertTriggerDefinition);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            // Trigger already exists, replace it
            await container.Scripts.ReplaceTriggerAsync(preInsertTriggerDefinition);
        }

        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.CustomersWithTrigger.Add(new CustomerWithTrigger { Id = "4", PartitionKey = "2" });

        await context.SaveChangesAsync();

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.CustomersWithTrigger.CountAsync();
        Assert.Equal(1, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transactionbehavior_always_fails_for_single_entity_with_trigger_and_entity_without_trigger()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.Customers.Add(new Customer { Id = "4", PartitionKey = "2" });
        context.CustomersWithTrigger.Add(new CustomerWithTrigger { Id = "4", PartitionKey = "2" });

        var exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(CosmosStrings.SaveChangesAutoTransactionBehaviorAlwaysTriggerAtomicity, exception.Message);

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.CustomersWithTrigger.CountAsync();
        Assert.Equal(0, customersCount);
    }

    public class TransactionalBatchContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Customer> Customers { get; set; } = null!;

        public DbSet<CustomerWithTrigger> CustomersWithTrigger { get; set; } = null!;


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

            builder.Entity<CustomerWithTrigger>(
                b =>
                {
                    b.HasKey(c => c.Id);
                    b.Property(c => c.ETag).IsETagConcurrency();
                    b.HasPartitionKey(c => c.PartitionKey);
                    b.HasTrigger("trigger", Azure.Cosmos.Scripts.TriggerType.Pre, Azure.Cosmos.Scripts.TriggerOperation.All);
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

    public class CustomerWithTrigger
    {
        public string? Id { get; set; }

        public string? ETag { get; set; }

        public string? PartitionKey { get; set; }
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
