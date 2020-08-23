// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class FunkyDataQuerySqliteTest : FunkyDataQueryTestBase<FunkyDataQuerySqliteTest.FunkyDataQuerySqliteFixture>
    {
        public FunkyDataQuerySqliteTest(FunkyDataQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(FunkyDataQuerySqliteFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);

        public class FunkyDataQuerySqliteFixture : FunkyDataQueryFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }
}
