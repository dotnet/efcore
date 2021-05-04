// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

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
