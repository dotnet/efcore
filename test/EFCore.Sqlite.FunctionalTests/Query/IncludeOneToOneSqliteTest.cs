// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class IncludeOneToOneSqliteTest(IncludeOneToOneSqliteTest.OneToOneQuerySqliteFixture fixture) : IncludeOneToOneTestBase<IncludeOneToOneSqliteTest.OneToOneQuerySqliteFixture>(fixture)
{
    public class OneToOneQuerySqliteFixture : OneToOneQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
