// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class CosmosBulkConcurrencyTest(CosmosBulkConcurrencyTest.BulkConcurrencyFixture fixture) : CosmosConcurrencyTest(fixture), IClassFixture<CosmosBulkConcurrencyTest.BulkConcurrencyFixture>
{
    public class BulkConcurrencyFixture : CosmosConcurrencyTest.CosmosFixture
    {
        protected override string StoreName => nameof(CosmosBulkConcurrencyTest);

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
        var context = base.CreateContext(options);
        context.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
        return context;
    }
}
