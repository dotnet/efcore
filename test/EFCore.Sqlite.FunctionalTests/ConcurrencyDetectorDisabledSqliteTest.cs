// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ConcurrencyDetectorDisabledSqliteTest(ConcurrencyDetectorDisabledSqliteTest.ConcurrencyDetectorSqlServerFixture fixture) : ConcurrencyDetectorDisabledRelationalTestBase<
    ConcurrencyDetectorDisabledSqliteTest.ConcurrencyDetectorSqlServerFixture>(fixture)
{
    public class ConcurrencyDetectorSqlServerFixture : ConcurrencyDetectorFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.EnableThreadSafetyChecks(enableChecks: false);
    }
}
