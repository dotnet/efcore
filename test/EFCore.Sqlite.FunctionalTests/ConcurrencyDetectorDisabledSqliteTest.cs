// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ConcurrencyDetectorDisabledSqliteTest : ConcurrencyDetectorDisabledRelationalTestBase<
        ConcurrencyDetectorDisabledSqliteTest.ConcurrencyDetectorSqlServerFixture>
    {
        public ConcurrencyDetectorDisabledSqliteTest(ConcurrencyDetectorSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class ConcurrencyDetectorSqlServerFixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => builder.EnableThreadSafetyChecks(enableChecks: false);
        }
    }
}
