// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedNavigationsMiscellaneousCosmosTest : OwnedNavigationsMiscellaneousTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsMiscellaneousCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Where_related_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_related_property(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["RequiredRelated"]["Int"] = 8)
""");
            });

    public override Task Where_optional_related_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_optional_related_property(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["OptionalRelated"]["Int"] = 9)
""");
            });

    public override Task Where_nested_related_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where_nested_related_property(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["RequiredRelated"]["RequiredNested"]["Int"] = 50)
""");
            });

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
