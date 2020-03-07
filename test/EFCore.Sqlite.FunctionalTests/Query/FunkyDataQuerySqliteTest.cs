// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

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
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

            protected override QueryAsserter<FunkyDataContext> CreateQueryAsserter(
                Dictionary<Type, object> entitySorters,
                Dictionary<Type, object> entityAsserters)
                => new RelationalQueryAsserter<FunkyDataContext>(
                    CreateContext,
                    new FunkyDataData(),
                    entitySorters,
                    entityAsserters,
                    CanExecuteQueryString);
        }
    }
}
