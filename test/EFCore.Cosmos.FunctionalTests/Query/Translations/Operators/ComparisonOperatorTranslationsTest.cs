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

    public override async Task Equal(bool async)
    {
        await base.Equal(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] = 8)
""");
    }

    public override async Task NotEqual(bool async)
    {
        await base.NotEqual(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] != 8)
""");
    }

    public override async Task GreaterThan(bool async)
    {
        await base.GreaterThan(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] > 8)
""");
    }

    public override async Task GreaterThanOrEqual(bool async)
    {
        await base.GreaterThanOrEqual(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] >= 8)
""");
    }

    public override async Task LessThan(bool async)
    {
        await base.LessThan(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] < 8)
""");
    }

    public override async Task LessThanOrEqual(bool async)
    {
        await base.LessThanOrEqual(async);

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
