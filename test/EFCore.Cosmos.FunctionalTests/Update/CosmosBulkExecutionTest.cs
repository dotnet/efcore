// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkExecutionTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string StoreName => nameof(CosmosBulkExecutionTest);

    protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual async Task DoesNotBatchSingleBatchableWrite()
    {
        var contextFactory = await InitializeAsync<CosmosBulkExecutionContext>(onConfiguring: (cfg) => cfg.UseCosmos(c => c.BulkExecutionEnabled()).ConfigureWarnings(x => x.Ignore(CosmosEventId.BulkExecutionWithTransactionalBatch)));
        using var context = contextFactory.CreateContext();

        context.Add(new Customer() { PartitionKey = "4" });
        context.AddRange(Enumerable.Range(0, 3).Select(x => new Customer()));
        context.AddRange(Enumerable.Range(0, 3).Select(x => new Customer() { PartitionKey = "2"}));
        context.Add(new Customer() { PartitionKey = "3" });

        ListLoggerFactory.Log.Clear();

        await context.SaveChangesAsync();
        Assert.Equal(CosmosEventId.ExecutedCreateItem, ListLoggerFactory.Log[0].Id);
        Assert.Equal(CosmosEventId.ExecutedCreateItem, ListLoggerFactory.Log[1].Id);
        Assert.Equal(CosmosEventId.ExecutedTransactionalBatch, ListLoggerFactory.Log[2].Id);
        Assert.Equal(CosmosEventId.ExecutedTransactionalBatch, ListLoggerFactory.Log[3].Id);
    }

    [ConditionalFact]
    public async Task SessionEnabled_Throws()
    {
        var contextFactory = await InitializeAsync<CosmosBulkExecutionContext>(onConfiguring: (cfg) => cfg.UseCosmos(c => c.BulkExecutionEnabled().SessionTokenManagementMode(Cosmos.Infrastructure.SessionTokenManagementMode.SemiAutomatic)));
        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context.Database.UseSessionToken("0:-1#1");
        context.Add(new Customer());
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("Consistency, Session, Properties, and Triggers are not allowed when AllowBulkExecution is set to true.", inner.Message);
    }

    [ConditionalFact]
    public async Task Trigger_Throws()
    {
        var contextFactory = await InitializeAsync<CosmosBulkExecutionContext>(onModelCreating: (b) => b.Entity<Customer>().HasTrigger(StoreName, Azure.Cosmos.Scripts.TriggerType.Post, Azure.Cosmos.Scripts.TriggerOperation.Create), onConfiguring: (cfg) => cfg.UseCosmos(c => c.BulkExecutionEnabled()));
        using var context = contextFactory.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context.Add(new Customer());
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("Consistency, Session, Properties, and Triggers are not allowed when AllowBulkExecution is set to true.", inner.Message);
    }

    public class CosmosBulkExecutionContext : DbContext
    {
        public CosmosBulkExecutionContext(DbContextOptions options) : base(options)
        {
        }

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

    public class CosmosFixture : SharedStoreFixtureBase<CosmosBulkExecutionContext>
    {
        protected override string StoreName
            => nameof(CosmosBulkExecutionTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder) => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled()).ConfigureWarnings(x => x.Ignore(CosmosEventId.BulkExecutionWithTransactionalBatch));
    }
}
