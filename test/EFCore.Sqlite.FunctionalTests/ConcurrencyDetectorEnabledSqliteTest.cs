// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ConcurrencyDetectorEnabledSqliteTest : ConcurrencyDetectorEnabledRelationalTestBase<
        ConcurrencyDetectorEnabledSqliteTest.ConcurrencyDetectorSqlServerFixture>
    {
        public ConcurrencyDetectorEnabledSqliteTest(ConcurrencyDetectorSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class ConcurrencyDetectorSqlServerFixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
