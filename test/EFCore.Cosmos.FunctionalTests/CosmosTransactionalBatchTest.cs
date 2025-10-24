﻿// Licensed to the .NET Foundation under one or more agreements.
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
    public virtual async Task SaveChanges_succeeds_for_101_entities_in_same_partition()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();

        context.Customers.AddRange(Enumerable.Range(0, 101).Select(x => new Customer { Id = x.ToString(), PartitionKey = "1" }));

        await context.SaveChangesAsync();

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(101, customersCount);
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

    [ConditionalFact]
    public virtual async Task SaveChanges_three_1mb_entries_succeeds()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", Name = new string('x', 1_000_000), PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "2", Name = new string('x', 1_000_000), PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "3", Name = new string('x', 1_000_000), PartitionKey = "1" });

        await context.SaveChangesAsync();

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(3, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_entity_too_large_throws()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", Name = new string('x', 50_000_000), PartitionKey = "1" });

        var exception = await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.NotNull(exception.InnerException);
        var cosmosException = Assert.IsAssignableFrom<CosmosException>(exception.InnerException);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, cosmosException.StatusCode);

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(0, customersCount);
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task SaveChanges_exactly_2_mib_does_not_split_and_one_byte_over_splits(bool oneByteOver)
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();
        
        using var context = contextFactory.CreateContext();

        var customer1 = new Customer { Id = new string('x', 1023), PartitionKey = new string('x', 1023) };
        var customer2 = new Customer { Id = new string('y', 1023), PartitionKey = new string('x', 1023) };

        context.Customers.Add(customer1);
        context.Customers.Add(customer2);

        await context.SaveChangesAsync();
        ListLoggerFactory.Clear();

        customer1.Name = new string('x', 1044994);
        customer2.Name = new string('x', 1044994);

        if (oneByteOver)
        {
            customer1.Name += 'x';
        }

        await context.SaveChangesAsync();
        using var assertContext = contextFactory.CreateContext();
        Assert.Equal(2, (await context.Customers.ToListAsync()).Count);

        if (oneByteOver)
        {
            Assert.Equal(2, ListLoggerFactory.Log.Count(x => x.Id == CosmosEventId.ExecutedTransactionalBatch));
        }
        else
        {
            Assert.Equal(1, ListLoggerFactory.Log.Count(x => x.Id == CosmosEventId.ExecutedTransactionalBatch));
        }
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_too_large_entry_after_smaller_throws_after_saving_smaller()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();

        context.Customers.Add(new Customer { Id = "1", Name = new string('x', 1_000_000), PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "2", Name = new string('x', 50_000_000), PartitionKey = "1" });

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(1, customersCount);
        Assert.Equal("1", (await assertContext.Customers.FirstAsync()).Id);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transaction_behaviour_always_payload_exactly_2_mib()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.Customers.Add(new Customer { Id = "1", Name = new string('x', 1048291), PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "2", Name = new string('x', 1048291), PartitionKey = "1" });

        await context.SaveChangesAsync();

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(2, customersCount);
    }

    [ConditionalFact]
    public virtual async Task SaveChanges_transaction_behaviour_always_payload_larger_than_cosmos_limit_throws()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.Customers.Add(new Customer { Id = "1", Name = new string('x', 50_000_000 / 2), PartitionKey = "1" });
        context.Customers.Add(new Customer { Id = "2", Name = new string('x', 50_000_000 / 2), PartitionKey = "1" });

        var exception = await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
        Assert.NotNull(exception.InnerException);
        var cosmosException = Assert.IsAssignableFrom<CosmosException>(exception.InnerException);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, cosmosException.StatusCode);

        using var assertContext = contextFactory.CreateContext();
        var customersCount = await assertContext.Customers.CountAsync();
        Assert.Equal(0, customersCount);
    }

    private const int nameLengthToExceed2MiBWithSpecialCharIdOnUpdate = 1046358;

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task SaveChanges_update_id_contains_special_chars_which_makes_request_larger_than_2_mib_splits_into_2_batches(bool isIdSpecialChar)
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();

        var id1 = isIdSpecialChar ? new string('€', 341) : new string('x', 341);
        var id2 = isIdSpecialChar ? new string('Ω', 341) : new string('y', 341);

        var customer1 = new Customer { Id = id1, PartitionKey = new string('€', 341) };
        var customer2 = new Customer { Id = id2, PartitionKey = new string('€', 341) };

        context.Customers.Add(customer1);
        context.Customers.Add(customer2);

        await context.SaveChangesAsync();
        ListLoggerFactory.Clear();

        customer1.Name = new string('x', nameLengthToExceed2MiBWithSpecialCharIdOnUpdate);
        customer2.Name = new string('x', nameLengthToExceed2MiBWithSpecialCharIdOnUpdate);

        await context.SaveChangesAsync();
        using var assertContext = contextFactory.CreateContext();
        Assert.Equal(2, (await context.Customers.ToListAsync()).Count);

        // The id being a special character should make the difference whether this fits in 1 batch.
        if (isIdSpecialChar)
        {
            Assert.Equal(2, ListLoggerFactory.Log.Count(x => x.Id == CosmosEventId.ExecutedTransactionalBatch));
        }
        else
        {
            Assert.Equal(1, ListLoggerFactory.Log.Count(x => x.Id == CosmosEventId.ExecutedTransactionalBatch));
        }
    }

    [ConditionalTheory, InlineData(true), InlineData(false)]
    public virtual async Task SaveChanges_create_id_contains_special_chars_which_would_make_request_larger_than_2_mib_on_update_does_not_split_into_2_batches_for_create(bool isIdSpecialChar)
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        using var context = contextFactory.CreateContext();

        var id1 = isIdSpecialChar ? new string('€', 341) : new string('x', 341);
        var id2 = isIdSpecialChar ? new string('Ω', 341) : new string('y', 341);

        var customer1 = new Customer { Id = id1, Name = new string('x', nameLengthToExceed2MiBWithSpecialCharIdOnUpdate), PartitionKey = new string('€', 341) };
        var customer2 = new Customer { Id = id2, Name = new string('x', nameLengthToExceed2MiBWithSpecialCharIdOnUpdate), PartitionKey = new string('€', 341) };

        context.Customers.Add(customer1);
        context.Customers.Add(customer2);

        await context.SaveChangesAsync();
        using var assertContext = contextFactory.CreateContext();
        Assert.Equal(2, (await context.Customers.ToListAsync()).Count);

        // The id being a special character should not make the difference whether this fits in 1 batch, as id is duplicated in the payload on create.
        Assert.Equal(1, ListLoggerFactory.Log.Count(x => x.Id == CosmosEventId.ExecutedTransactionalBatch));
    }

    [ConditionalFact]
    public async Task SaveChanges_transaction_behaviour_never_does_not_use_transactions()
    {
        var contextFactory = await InitializeAsync<TransactionalBatchContext>();

        TransactionalBatchContext CreateContext()
        {
            var context = contextFactory.CreateContext();
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            return context;
        }

        var customers = new Customer[] { new Customer { Id = "42", Name = "Theon", PartitionKey = "1" }, new Customer { Id = "43", Name = "Rob", PartitionKey = "1" } };

        using (var context = CreateContext())
        {
            ListLoggerFactory.Clear();

            context.AddRange(customers);

            await context.SaveChangesAsync();
            
            var logEntries = ListLoggerFactory.Log.Where(e => e.Id == CosmosEventId.ExecutedCreateItem).ToList();
            Assert.Equal(2, logEntries.Count);
            foreach (var logEntry in logEntries)
            {
                Assert.Equal(LogLevel.Information, logEntry.Level);
                Assert.Contains("CreateItem", logEntry.Message);
            }
        }

        using (var context = CreateContext())
        {
            ListLoggerFactory.Clear();
            var customerFromStore1 = await context.Set<Customer>().FirstAsync(x => x.Id == "42");
            var customerFromStore2 = await context.Set<Customer>().LastAsync(x => x.Id == "43");

            customerFromStore1.Name += " Greyjoy";
            customerFromStore2.Name += " Stark";

            await context.SaveChangesAsync();

            var logEntries = ListLoggerFactory.Log.Where(e => e.Id == CosmosEventId.ExecutedReplaceItem).ToList();
            Assert.Equal(2, logEntries.Count);
            foreach (var logEntry in logEntries)
            {
                Assert.Equal(LogLevel.Information, logEntry.Level);
                Assert.Contains("ReplaceItem", logEntry.Message);
            }
        }

        using (var context = CreateContext())
        {
            ListLoggerFactory.Clear();
            var customerFromStore1 = await context.Set<Customer>().FirstAsync(x => x.Id == "42");
            var customerFromStore2 = await context.Set<Customer>().LastAsync(x => x.Id == "43");

            Assert.Equal("42", customerFromStore1.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore1.Name);

            Assert.Equal("43", customerFromStore2.Id);
            Assert.Equal("Rob Stark", customerFromStore2.Name);

            context.Remove(customerFromStore1);
            context.Remove(customerFromStore2);

            await context.SaveChangesAsync();

            var logEntries = ListLoggerFactory.Log.Where(e => e.Id == CosmosEventId.ExecutedDeleteItem).ToList();
            Assert.Equal(2, logEntries.Count);
            foreach (var logEntry in logEntries)
            {
                Assert.Equal(LogLevel.Information, logEntry.Level);
                Assert.Contains("DeleteItem", logEntry.Message);
            }
        }

        using (var context = CreateContext())
        {
            ListLoggerFactory.Clear();
            Assert.Empty(await context.Set<Customer>().ToListAsync());
        }
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
