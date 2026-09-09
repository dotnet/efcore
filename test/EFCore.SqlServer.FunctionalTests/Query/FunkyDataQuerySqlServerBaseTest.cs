// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class FunkyDataQuerySqlServerBaseTest<TFixture> : FunkyDataQueryTestBase<TFixture>
    where TFixture : FunkyDataQueryTestBase<TFixture>.FunkyDataQueryFixtureBase, ITestSqlLoggerFactory, new()
{
    public FunkyDataQuerySqlServerBaseTest(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
