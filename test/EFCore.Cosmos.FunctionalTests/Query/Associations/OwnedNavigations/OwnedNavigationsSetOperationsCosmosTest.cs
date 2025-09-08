// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsSetOperationsCosmosTest : OwnedNavigationsSetOperationsTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsSetOperationsCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task On_related()
    {
        await base.On_related();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_CONCAT(ARRAY(
    SELECT VALUE r
    FROM r IN c["RelatedCollection"]
    WHERE (r["Int"] = 8)), ARRAY(
    SELECT VALUE r0
    FROM r0 IN c["RelatedCollection"]
    WHERE (r0["String"] = "foo")))) = 4)
""");
    }

    public override Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.On_related_projected(queryTrackingBehavior));

    public override Task On_related_Select_nested_with_aggregates(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.On_related_projected(queryTrackingBehavior));

    public override async Task On_nested()
    {
        await base.On_nested();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_CONCAT(ARRAY(
    SELECT VALUE n
    FROM n IN c["RequiredRelated"]["NestedCollection"]
    WHERE (n["Int"] = 8)), ARRAY(
    SELECT VALUE n0
    FROM n0 IN c["RequiredRelated"]["NestedCollection"]
    WHERE (n0["String"] = "foo")))) = 4)
""");
    }

    public override Task Over_different_collection_properties()
        => AssertTranslationFailed(base.Over_different_collection_properties);

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
