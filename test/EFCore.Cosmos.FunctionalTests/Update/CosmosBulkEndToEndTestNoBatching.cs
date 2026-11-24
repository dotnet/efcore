// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkEndToEndTestNoBatching(NonSharedFixture fixture) : EndToEndCosmosTest(fixture), IClassFixture<NonSharedFixture>
{
    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => base.AddNonSharedOptions(builder).UseCosmos(x => x.BulkExecutionAllowed());

    protected override TContext CreateContext<TContext>(ContextFactory<TContext> factory, bool transactionalBatch)
    {
        var context = base.CreateContext(factory, transactionalBatch);
        context.Database.AutoTransactionBehavior = !transactionalBatch
            ? AutoTransactionBehavior.Never
            : throw SkipException.ForSkip("Only AutoTransactionBehavior.Never is tested.");
        return context;
    }
}
