// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryFilterFuncletizationSqliteTest : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationSqliteTest.QueryFilterFuncletizationSqliteFixture>
    {
        public QueryFilterFuncletizationSqliteTest(
            QueryFilterFuncletizationSqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public class QueryFilterFuncletizationSqliteFixture : QueryFilterFuncletizationRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
        }
    }
}
