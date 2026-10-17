// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class LogicalOperatorTranslationsCosmosTest : LogicalOperatorTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public LogicalOperatorTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task And()
    {
        await base.And();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 8) AND (c["String"] = "Seattle"))
""");
    }

    public override async Task And_with_bool_property()
    {
        await base.And_with_bool_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Bool"] AND (c["String"] = "Seattle"))
""");
    }

    public override async Task Or()
    {
        await base.Or();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 999) OR (c["String"] = "Seattle"))
""");
    }

    public override async Task Or_with_bool_property()
    {
        await base.Or_with_bool_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Bool"] OR (c["String"] = "Seattle"))
""");
    }

    public override async Task Not()
    {
        await base.Not();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE NOT((c["Int"] = 999))
""");
    }

    public override async Task Not_with_bool_property()
    {
        await base.Not_with_bool_property();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE NOT(c["Bool"])
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
