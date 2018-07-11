// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeOneToOneSqliteTest : IncludeOneToOneTestBase<IncludeOneToOneSqliteTest.OneToOneQuerySqliteFixture>
    {
        public IncludeOneToOneSqliteTest(OneToOneQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public class OneToOneQuerySqliteFixture : OneToOneQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        }
    }
}
