// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class MiscellaneousOperatorTranslationsSqlServerTest : MiscellaneousOperatorTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public MiscellaneousOperatorTranslationsSqlServerTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Conditional()
    {
        await base.Conditional();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 8) ? c["String"] : "Foo") = "Seattle")
""");
    }

    public override async Task Conditional_simplifiable_equality()
    {
        await base.Conditional_simplifiable_equality();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] > 1)
""");
    }

    public override async Task Conditional_simplifiable_inequality()
    {
        await base.Conditional_simplifiable_inequality();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["Int"] > 1)
""");
    }

    public override async Task Conditional_uncoalesce_with_equality_left()
    {
        await base.Conditional_uncoalesce_with_equality_left();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 9) ? null : c["Int"]) > 1)
""");
    }

    public override async Task Conditional_uncoalesce_with_equality_right()
    {
        await base.Conditional_uncoalesce_with_equality_right();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((9 = c["Int"]) ? null : c["Int"]) > 1)
""");
    }

    public override async Task Conditional_uncoalesce_with_inequality_left()
    {
        await base.Conditional_uncoalesce_with_inequality_left();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] != 9) ? c["Int"] : null) > 1)
""");
    }

    public override async Task Conditional_uncoalesce_with_inequality_right()
    {
        await base.Conditional_uncoalesce_with_inequality_right();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((9 != c["Int"]) ? c["Int"] : null) > 1)
""");
    }

    public override async Task Conditional_uncoalesce_with_string()
    {
        await base.Conditional_uncoalesce_with_string();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["String"] = "Seattle") ? null : c["String"]) = "London")
""");
    }

    public override async Task Coalesce()
    {
        await base.Coalesce();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (((c["String"] != null) ? c["String"] : "Unknown") = "Seattle")
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
