// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkEndToEndTestNoBatching(NonSharedFixture fixture) : EndToEndCosmosTest(fixture), IClassFixture<NonSharedFixture>
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
