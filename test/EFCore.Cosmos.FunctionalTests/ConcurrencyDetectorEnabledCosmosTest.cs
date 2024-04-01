// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConcurrencyDetectorEnabledCosmosTest(ConcurrencyDetectorEnabledCosmosTest.ConcurrencyDetectorCosmosFixture fixture) : ConcurrencyDetectorEnabledTestBase<
    ConcurrencyDetectorEnabledCosmosTest.ConcurrencyDetectorCosmosFixture>(fixture)
{
    [ConditionalTheory(Skip = "Issue #17246")]
    public override Task Any(bool async)
        => base.Any(async);

    public class ConcurrencyDetectorCosmosFixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
