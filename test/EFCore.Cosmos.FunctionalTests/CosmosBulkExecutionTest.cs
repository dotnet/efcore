// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore;

public class CosmosBulkExecutionTest(CosmosBulkExecutionTest.CosmosFixture fixture) : IClassFixture<CosmosBulkExecutionTest.CosmosFixture>
{
    public CosmosFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public virtual async Task DoesNotBatchSingleBatchableWrite()
    {
        using var context = Fixture.CreateContext();

        context.Add(new Customer() { PartitionKey = "4" });
        context.AddRange(Enumerable.Range(0, 3).Select(x => new Customer()));
        context.AddRange(Enumerable.Range(0, 3).Select(x => new Customer() { PartitionKey = "2"}));
        context.Add(new Customer() { PartitionKey = "3" });

        Fixture.ListLoggerFactory.Log.Clear();

        await context.SaveChangesAsync();
        Assert.Equal(CosmosEventId.ExecutedCreateItem, Fixture.ListLoggerFactory.Log[0].Id);
        Assert.Equal(CosmosEventId.ExecutedCreateItem, Fixture.ListLoggerFactory.Log[1].Id);
        Assert.Equal(CosmosEventId.ExecutedTransactionalBatch, Fixture.ListLoggerFactory.Log[2].Id);
        Assert.Equal(CosmosEventId.ExecutedTransactionalBatch, Fixture.ListLoggerFactory.Log[3].Id);
    }

    public class EndToEndTest(NonSharedFixture fixture) : EndToEndCosmosTest(fixture), IClassFixture<NonSharedFixture>
    {
        protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled()).ConfigureWarnings(x => x.Ignore(CosmosEventId.BulkExecutionWithTransactionalBatch));
    }

    public class EndToEndTestNever(NonSharedFixture fixture) : EndToEndCosmosTest(fixture), IClassFixture<NonSharedFixture>
    {
        protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled());

        protected override TContext CreateContext<TContext>(ContextFactory<TContext> factory, bool transactionalBatch)
        {
            var context = base.CreateContext(factory, transactionalBatch);
            if (!transactionalBatch)
            {
                context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            }
            else
            {
                throw Xunit.Sdk.SkipException.ForSkip("Only AutoTransactionBehavior.Never is tested.");
            }
            return context;
        }
    }

    public class ConcurrencyTest(ConcurrencyTest.ConcurrencyFicture fixture) : CosmosConcurrencyTest(fixture), IClassFixture<ConcurrencyTest.ConcurrencyFicture>
    {
        public class ConcurrencyFicture : CosmosConcurrencyTest.CosmosFixture
        {
            public override ConcurrencyContext CreateContext()
            {
                var context = base.CreateContext();
                context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
                return context;
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled());
        }

        protected override ConcurrencyContext CreateContext(DbContextOptions options)   
        {
            var context = base.CreateContext();
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            return context;
        }
    }

    public class WarningTest(ThrowingFixture fixture) : IClassFixture<ThrowingFixture>
    {
        [ConditionalFact]
        public virtual async Task AutoTransactionBehaviorNever_DoesNotThrow()
        {
            using var context = fixture.CreateContext();
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

            context.AddRange(Enumerable.Range(0, 200).Select(x => new Customer()));
            await context.SaveChangesAsync();
        }

        [ConditionalFact]
        public virtual async Task AutoTransactionBehaviorWhenNeeded_Throws()
        {
            using var context = fixture.CreateContext();
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;

            context.AddRange(Enumerable.Range(0, 200).Select(x => new Customer()));
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal(Message, inner.Message);
        }

        [ConditionalFact]
        public virtual async Task AutoTransactionBehaviorAlways_Throws()
        {
            using var context = fixture.CreateContext();
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

            context.AddRange(Enumerable.Range(0, 200).Select(x => new Customer()));
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal(Message, inner.Message);
        }

        private string Message => CoreStrings.WarningAsErrorTemplate(
                CosmosEventId.BulkExecutionWithTransactionalBatch.ToString(),
                CosmosResources.LogBulkExecutionWithTransactionalBatch(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
                "CosmosEventId.BulkExecutionWithTransactionalBatch");
    }

    public class UnsupportedThrowsTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
    {
        protected override string StoreName => nameof(UnsupportedThrowsTest);

        protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

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
            var contextFactory = await InitializeAsync<CosmosBulkExecutionContext>(onModelCreating: (b) => b.Entity<Customer>().HasTrigger(StoreName, Azure.Cosmos.Scripts.TriggerType.Post, Azure.Cosmos.Scripts.TriggerOperation.Create),onConfiguring: (cfg) => cfg.UseCosmos(c => c.BulkExecutionEnabled()));
            using var context = contextFactory.CreateContext();
            context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            context.Add(new Customer());
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var inner = Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Contains("Consistency, Session, Properties, and Triggers are not allowed when AllowBulkExecution is set to true.", inner.Message);
        }
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

    public class ThrowingFixture : SharedStoreFixtureBase<CosmosBulkExecutionContext>
    {
        protected override string StoreName
            => nameof(CosmosBulkExecutionTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder) => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled());
    }

    public class CosmosFixture : ThrowingFixture
    {
        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder) => base.AddOptions(builder).ConfigureWarnings(x => x.Ignore(CosmosEventId.BulkExecutionWithTransactionalBatch));
    }
}
