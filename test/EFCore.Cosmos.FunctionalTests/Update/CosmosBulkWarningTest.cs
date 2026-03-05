// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using static Microsoft.EntityFrameworkCore.Update.CosmosBulkExecutionTest;

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkWarningTest(CosmosBulkWarningTest.ThrowingFixture fixture) : IClassFixture<CosmosBulkWarningTest.ThrowingFixture>
{
    [ConditionalFact]
    public virtual async Task AutoTransactionBehaviorNever_DoesNotThrow()
    {
        using var context = fixture.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

        context.AddRange(Enumerable.Range(0, 100).Select(x => new Customer()));
        await context.SaveChangesAsync();
    }

    [ConditionalFact]
    public virtual async Task AutoTransactionBehaviorWhenNeeded_Throws()
    {
        using var context = fixture.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.WhenNeeded;

        context.AddRange(Enumerable.Range(0, 200).Select(x => new Customer()));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(BulkExecutionWithTransactionalBatchMessage, ex.Message);
    }

    [ConditionalFact]
    public virtual async Task AutoTransactionBehaviorAlways_Throws()
    {
        using var context = fixture.CreateContext();
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;

        context.AddRange(Enumerable.Range(0, 200).Select(x => new Customer()));
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        Assert.Equal(BulkExecutionWithTransactionalBatchMessage, ex.Message);
    }

    private string BulkExecutionWithTransactionalBatchMessage => CoreStrings.WarningAsErrorTemplate(
            CosmosEventId.BulkExecutionWithTransactionalBatch.ToString(),
            CosmosResources.LogBulkExecutionWithTransactionalBatch(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
            "CosmosEventId.BulkExecutionWithTransactionalBatch");

    public class ThrowingFixture : SharedStoreFixtureBase<CosmosBulkExecutionContext>
    {
        protected override string StoreName
            => nameof(CosmosBulkExecutionTest);

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder) => base.AddOptions(builder).UseCosmos(x => x.BulkExecutionEnabled());
    }
}
