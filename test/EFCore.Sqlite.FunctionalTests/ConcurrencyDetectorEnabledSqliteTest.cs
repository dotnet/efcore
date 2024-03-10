// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ConcurrencyDetectorEnabledSqliteTest(ConcurrencyDetectorEnabledSqliteTest.ConcurrencyDetectorSqlServerFixture fixture) : ConcurrencyDetectorEnabledRelationalTestBase<
    ConcurrencyDetectorEnabledSqliteTest.ConcurrencyDetectorSqlServerFixture>(fixture)
{
    public class ConcurrencyDetectorSqlServerFixture : ConcurrencyDetectorFixtureBase, ITestSqlLoggerFactory
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
