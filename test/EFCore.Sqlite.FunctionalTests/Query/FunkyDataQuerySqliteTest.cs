// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FunkyDataQuerySqliteTest : FunkyDataQueryTestBase<FunkyDataQuerySqliteTest.FunkyDataQuerySqliteFixture>
    {
        public FunkyDataQuerySqliteTest(FunkyDataQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        public class FunkyDataQuerySqliteFixture : FunkyDataQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
