// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkExecutionTest(NonSharedFixture nonSharedFixture, CosmosBulkExecutionTest.BulkFixture fixture)
    : NonSharedModelTestBase(nonSharedFixture), IClassFixture<CosmosBulkExecutionTest.BulkFixture>, IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName => nameof(CosmosBulkExecutionTest);

    protected override ITestStoreFactory NonSharedTestStoreFactory => CosmosTestStoreFactory.Instance;


    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292 (Transactional batch limits not enforced)
    [ConditionalFact(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
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

    [Fact]
    public virtual async Task AutoTransactionBehaviorNever_DoesNotThrow()
    {
        var contextFactory = await InitializeNonSharedTest<CosmosBulkExecutionContext>(
            onConfiguring: cfg => cfg.UseCosmos(x => x.BulkExecutionAllowed()));
        using var context = contextFactory.CreateDbContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        context.AddRange(Enumerable.Range(0, 100).Select(x => new Customer()));
        await context.SaveChangesAsync();
    }


    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292 (Transactional batch limits not enforced)
    [ConditionalFact(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
    public virtual async Task AutoTransactionBehaviorWhenNeeded_Throws()
    {
        var contextFactory = await InitializeNonSharedTest<CosmosBulkExecutionContext>(
            onConfiguring: cfg => cfg.UseCosmos(x => x.BulkExecutionAllowed()));
        using var context = contextFactory.CreateDbContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;

        context.AddRange(Enumerable.Range(0, 200).Select(x => new Customer()));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(BulkExecutionWithTransactionalBatchMessage, ex.Message);
    }


    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/292 (Transactional batch limits not enforced)
    [ConditionalFact(typeof(CosmosTestEnvironment), nameof(CosmosTestEnvironment.IsNotLinuxEmulator))]
    public virtual async Task AutoTransactionBehaviorAlways_Throws()
    {
        var contextFactory = await InitializeNonSharedTest<CosmosBulkExecutionContext>(
            onConfiguring: cfg => cfg.UseCosmos(x => x.BulkExecutionAllowed()));
        using var context = contextFactory.CreateDbContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.AddRange(Enumerable.Range(0, 200).Select(x => new Customer()));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(BulkExecutionWithTransactionalBatchMessage, ex.Message);
    }

    [Fact]
    public async Task SessionEnabled_Throws()
    {
        var contextFactory = await InitializeNonSharedTest<CosmosBulkExecutionContext>(
            onConfiguring: cfg => cfg.UseCosmos(
                c => c.BulkExecutionAllowed()
                    .SessionTokenManagementMode(Cosmos.Infrastructure.SessionTokenManagementMode.SemiAutomatic)));
        using var context = contextFactory.CreateDbContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context.Database.UseSessionToken("0:-1#1");
        context.Add(new Customer());
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains(
            "Consistency, Session, Properties, and Triggers are not allowed when AllowBulkExecution is set to true.",
            inner.Message);
    }

    [Fact]
    public async Task Trigger_Throws()
    {
        var contextFactory = await InitializeNonSharedTest<CosmosBulkExecutionContext>(
            onModelCreating: b => b.Entity<Customer>().HasTrigger(
                NonSharedStoreName, Azure.Cosmos.Scripts.TriggerType.Post, Azure.Cosmos.Scripts.TriggerOperation.Create),
            onConfiguring: cfg => cfg.UseCosmos(c => c.BulkExecutionAllowed()));
        using var context = contextFactory.CreateDbContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        context.Add(new Customer());
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains(
            "Consistency, Session, Properties, and Triggers are not allowed when AllowBulkExecution is set to true.",
            inner.Message);
    }

    private string BulkExecutionWithTransactionalBatchMessage => CoreStrings.WarningAsErrorTemplate(
        CosmosEventId.BulkExecutionWithTransactionalBatch.ToString(),
        CosmosResources.LogBulkExecutionWithTransactionalBatch(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
        "CosmosEventId.BulkExecutionWithTransactionalBatch");

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
