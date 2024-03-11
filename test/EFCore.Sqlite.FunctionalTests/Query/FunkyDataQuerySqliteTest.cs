// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class FunkyDataQuerySqliteTest : FunkyDataQueryTestBase<FunkyDataQuerySqliteTest.FunkyDataQuerySqliteFixture>
{
    public FunkyDataQuerySqliteTest(FunkyDataQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task String_Contains_and_StartsWith_with_same_parameter(bool async)
    {
        await base.String_Contains_and_StartsWith_with_same_parameter(async);

        AssertSql(
            """
@__s_0='B' (Size = 1)
@__s_0_startswith='B%' (Size = 2)

SELECT "f"."Id", "f"."FirstName", "f"."LastName", "f"."NullableBool"
FROM "FunkyCustomers" AS "f"
WHERE ("f"."FirstName" IS NOT NULL AND instr("f"."FirstName", @__s_0) > 0) OR "f"."LastName" LIKE @__s_0_startswith ESCAPE '\'
""");
    }

    protected override QueryAsserter CreateQueryAsserter(FunkyDataQuerySqliteFixture fixture)
        => new RelationalQueryAsserter(
            fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class FunkyDataQuerySqliteFixture : FunkyDataQueryFixtureBase, ITestSqlLoggerFactory
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;
    }
}
