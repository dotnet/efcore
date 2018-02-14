// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedQuerySqliteTest : OwnedQueryTestBase<OwnedQuerySqliteTest.OwnedQuerySqliteFixture>
    {
        public OwnedQuerySqliteTest(OwnedQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public class OwnedQuerySqliteFixture : OwnedQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        }
    }
}
