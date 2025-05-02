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

    public override Task Equal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Equal(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Int"] = 8)
""");
            });

    public override Task NotEqual(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.NotEqual(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Int"] != 8)
""");
            });

    public override Task GreaterThan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GreaterThan(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Int"] > 8)
""");
            });

    public override Task GreaterThanOrEqual(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GreaterThanOrEqual(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Int"] >= 8)
""");
            });

    public override Task LessThan(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.LessThan(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Int"] < 8)
""");
            });

    public override Task LessThanOrEqual(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.LessThanOrEqual(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Int"] <= 8)
""");
            });

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
