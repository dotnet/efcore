// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class ConcurrencyDetectorDisabledCosmosTest : ConcurrencyDetectorDisabledTestBase<
    ConcurrencyDetectorDisabledCosmosTest.ConcurrencyDetectorCosmosFixture>
{
    public ConcurrencyDetectorDisabledCosmosTest(ConcurrencyDetectorCosmosFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory(Skip = "Issue #17246")]
    public override Task Any(bool async)
        => base.Any(async);

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
