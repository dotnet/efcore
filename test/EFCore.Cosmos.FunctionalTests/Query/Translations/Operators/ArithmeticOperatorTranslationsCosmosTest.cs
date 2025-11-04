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

    public override async Task Add()
    {
        await base.Add();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] + 2) = 10)
""");
    }

    public override async Task Subtract()
    {
        await base.Subtract();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] - 3) = 5)
""");
    }

    public override async Task Multiply()
    {
        await base.Multiply();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] * 2) = 16)
""");
    }

    public override async Task Modulo()
    {
        await base.Modulo();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] % 3) = 2)
""");
    }

    public override async Task Minus()
    {
        await base.Minus();

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
