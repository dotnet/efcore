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

    public override Task And(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.And(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 8) AND (c["String"] = "Seattle"))
""");
            });

    public override Task And_with_bool_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.And_with_bool_property(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Bool"] AND (c["String"] = "Seattle"))
""");
            });

    public override Task Or(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Or(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 999) OR (c["String"] = "Seattle"))
""");
            });

    public override Task Or_with_bool_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Or_with_bool_property(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Bool"] OR (c["String"] = "Seattle"))
""");
            });

    public override Task Not(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Not(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE NOT((c["Int"] = 999))
""");
            });

    public override Task Not_with_bool_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Not_with_bool_property(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE NOT(c["Bool"])
""");
            });

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
