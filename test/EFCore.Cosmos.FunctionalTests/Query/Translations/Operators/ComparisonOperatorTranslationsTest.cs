// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class ComparisonOperatorTranslationsCosmosTest : ComparisonOperatorTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public ComparisonOperatorTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Equal()
    {
        await base.Equal();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] = 8)
""");
    }

    public override async Task NotEqual()
    {
        await base.NotEqual();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] != 8)
""");
    }

    public override async Task GreaterThan()
    {
        await base.GreaterThan();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] > 8)
""");
    }

    public override async Task GreaterThanOrEqual()
    {
        await base.GreaterThanOrEqual();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] >= 8)
""");
    }

    public override async Task LessThan()
    {
        await base.LessThan();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] < 8)
""");
    }

    public override async Task LessThanOrEqual()
    {
        await base.LessThanOrEqual();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] <= 8)
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
