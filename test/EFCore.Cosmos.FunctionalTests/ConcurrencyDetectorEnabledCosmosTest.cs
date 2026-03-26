// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConcurrencyDetectorEnabledCosmosTest(ConcurrencyDetectorEnabledCosmosTest.ConcurrencyDetectorCosmosFixture fixture)
    : ConcurrencyDetectorEnabledTestBase<
        ConcurrencyDetectorEnabledCosmosTest.ConcurrencyDetectorCosmosFixture>(fixture)
{
    public override async Task Find(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Find(async);
        }
    }

    public override async Task Count(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Count(async);
        }
    }

    public override async Task First(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.First(async);
        }
    }

    public override async Task Last(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Last(async);
        }
    }

    public override async Task Single(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.Single(async);
        }
    }

    [ConditionalTheory(Skip = "Issue #17246")]
    public override Task Any(bool async)
        => base.Any(async);

    public override async Task ToList(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            await base.ToList(async);
        }
    }

    public class ConcurrencyDetectorCosmosFixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));
    }
}
