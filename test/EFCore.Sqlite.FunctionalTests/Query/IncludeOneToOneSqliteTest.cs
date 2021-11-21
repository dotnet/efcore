// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

public class IncludeOneToOneSqliteTest : IncludeOneToOneTestBase<IncludeOneToOneSqliteTest.OneToOneQuerySqliteFixture>
{
    public IncludeOneToOneSqliteTest(OneToOneQuerySqliteFixture fixture)
        : base(fixture)
    {
    }

    public class OneToOneQuerySqliteFixture : OneToOneQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
