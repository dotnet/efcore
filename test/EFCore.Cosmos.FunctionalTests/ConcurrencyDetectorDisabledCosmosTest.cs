// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConcurrencyDetectorDisabledCosmosTest(ConcurrencyDetectorDisabledCosmosTest.ConcurrencyDetectorCosmosFixture fixture)
    : ConcurrencyDetectorDisabledTestBase<
        ConcurrencyDetectorDisabledCosmosTest.ConcurrencyDetectorCosmosFixture>(fixture)
{
    [ConditionalTheory(Skip = "Issue #17246")]
    public override Task Any(bool async)
        => base.Any(async);

    public override Task SaveChanges(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.SaveChanges(a));

    public override Task Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Count(a));

    public override Task Find(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Find(a));

    public override Task First(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.First(a));

    public override Task Last(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Last(a));

    public override Task Single(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Single(a));

    public override Task ToList(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.ToList(a));

    public class ConcurrencyDetectorCosmosFixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.EnableThreadSafetyChecks(enableChecks: false);
    }
}
