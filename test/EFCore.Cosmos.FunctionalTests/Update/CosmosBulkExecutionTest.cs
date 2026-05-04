// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkExecutionTest(CosmosBulkExecutionTest.BulkFixture fixture)
    : IClassFixture<CosmosBulkExecutionTest.BulkFixture>
{
    [ConditionalFact]
    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292 (Transactional batch limits not enforced)
    [CosmosCondition(CosmosCondition.IsNotLinuxEmulator)]
    public virtual async Task DoesNotBatchSingleBatchableWrite()
    {
        using var context = fixture.CreateContext();

        context.Add(new Customer { PartitionKey = "4" });
        context.AddRange(Enumerable.Range(0, 3).Select(x => new Customer()));
        context.AddRange(Enumerable.Range(0, 3).Select(x => new Customer { PartitionKey = "2" }));
        context.Add(new Customer { PartitionKey = "3" });

        fixture.ListLoggerFactory.Clear();

        await context.SaveChangesAsync();
        Assert.Equal(CosmosEventId.ExecutedCreateItem, fixture.ListLoggerFactory.Log[0].Id);
        Assert.Equal(CosmosEventId.ExecutedCreateItem, fixture.ListLoggerFactory.Log[1].Id);
        Assert.Equal(CosmosEventId.ExecutedTransactionalBatch, fixture.ListLoggerFactory.Log[2].Id);
        Assert.Equal(CosmosEventId.ExecutedTransactionalBatch, fixture.ListLoggerFactory.Log[3].Id);
    }

    [ConditionalFact]
    public virtual async Task AutoTransactionBehaviorNever_DoesNotThrow()
    {
        using var context = fixture.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        context.AddRange(Enumerable.Range(0, 100).Select(x => new Customer()));
        await context.SaveChangesAsync();
    }

    public class CosmosBulkExecutionContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Customer> Customers { get; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>().HasPartitionKey(x => x.PartitionKey);
        }
    }

    public class Customer
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string PartitionKey { get; set; } = "1";
    }

    public class BulkFixture : SharedStoreFixtureBase<CosmosBulkExecutionContext>
    {
        protected override string StoreName
            => nameof(CosmosBulkExecutionTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override bool UsePooling
            => false;

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Database.Command.Name;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder)
                .UseCosmos(x => x.BulkExecutionAllowed())
                .ConfigureWarnings(x => x.Ignore(CosmosEventId.BulkExecutionWithTransactionalBatch));
    }
}
