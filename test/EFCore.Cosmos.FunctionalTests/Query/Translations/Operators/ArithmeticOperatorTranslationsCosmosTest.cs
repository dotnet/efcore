// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class ArithmeticOperatorTranslationsCosmosTest : ArithmeticOperatorTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public ArithmeticOperatorTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Add(bool async)
    {
        await base.Add(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] + 2) = 10)
""");
    }

    public override async Task Subtract(bool async)
    {
        await base.Subtract(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] - 3) = 5)
""");
    }

    public override async Task Multiply(bool async)
    {
        await base.Multiply(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] * 2) = 16)
""");
    }

    public override async Task Modulo(bool async)
    {
        await base.Modulo(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] % 3) = 2)
""");
    }

    public override async Task Minus(bool async)
    {
        await base.Minus(async);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (-(c["Int"]) = -8)
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
