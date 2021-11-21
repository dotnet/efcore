// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query;

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
