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

    public override  Task Add(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Add(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] + 2) = 10)
""");
            });

    public override Task Subtract(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Subtract(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] - 3) = 5)
""");
            });

    public override Task Multiply(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Multiply(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] * 2) = 16)
""");
            });

    public override Task Modulo(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Modulo(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] % 3) = 2)
""");
            });

    public override Task Minus(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Minus(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (-(c["Int"]) = -8)
""");
            });

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
