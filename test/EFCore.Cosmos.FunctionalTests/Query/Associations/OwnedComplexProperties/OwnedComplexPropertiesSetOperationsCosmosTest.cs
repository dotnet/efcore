// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesSetOperationsCosmosTest : OwnedComplexPropertiesSetOperationsTestBase<OwnedComplexPropertiesCosmosFixture>
{
    public OwnedComplexPropertiesSetOperationsCosmosTest(OwnedComplexPropertiesCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Over_associate_collections()
    {
        await base.Over_associate_collections();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_CONCAT(ARRAY(
    SELECT VALUE a
    FROM a IN c["AssociateCollection"]
    WHERE (a["Int"] = 8)), ARRAY(
    SELECT VALUE a0
    FROM a0 IN c["AssociateCollection"]
    WHERE (a0["String"] = "foo")))) = 4)
""");
    }

    public override Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Over_associate_collection_projected(queryTrackingBehavior)); // #37918

    // https://github.com/Azure/azure-cosmos-db-emulator-docker/issues/330 (Aggregates over subqueries return null result set)
    public override async Task Over_assocate_collection_Select_nested_with_aggregates_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        CosmosTestEnvironment.SkipOnLinuxEmulator();

        await base.Over_assocate_collection_Select_nested_with_aggregates_projected(queryTrackingBehavior);

        AssertSql(
            """
SELECT VALUE (
    SELECT VALUE SUM((
        SELECT VALUE SUM(n["Int"])
        FROM n IN a1["NestedCollection"]))
    FROM a1 IN (SELECT VALUE ARRAY_CONCAT(ARRAY(
        SELECT VALUE a
        FROM a IN c["AssociateCollection"]
        WHERE (a["Int"] = 8)), ARRAY(
        SELECT VALUE a0
        FROM a0 IN c["AssociateCollection"]
        WHERE (a0["String"] = "foo")))))
FROM root c
""");
    }

    public override async Task Over_nested_associate_collection()
    {
        await base.Over_nested_associate_collection();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(ARRAY_CONCAT(ARRAY(
    SELECT VALUE n
    FROM n IN c["RequiredAssociate"]["NestedCollection"]
    WHERE (n["Int"] = 8)), ARRAY(
    SELECT VALUE n0
    FROM n0 IN c["RequiredAssociate"]["NestedCollection"]
    WHERE (n0["String"] = "foo")))) = 4)
""");
    }

    public override Task Over_different_collection_properties()
        => AssertTranslationFailed(base.Over_different_collection_properties);

    public override async Task Union_over_associate_collections()
    {
        await base.Union_over_associate_collections();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(SetUnion(ARRAY(
    SELECT VALUE a
    FROM a IN c["AssociateCollection"]
    WHERE (a["Int"] = 8)), ARRAY(
    SELECT VALUE a0
    FROM a0 IN c["AssociateCollection"]
    WHERE (a0["String"] = "foo")))) = 2)
""");
    }

    // Set operations aren't supported on Cosmos
    public override Task Union_associates_then_project_nested_associate()
        => AssertTranslationFailed(base.Union_associates_then_project_nested_associate);

    // Set operations aren't supported on Cosmos
    public override Task Union_associates_then_filter_on_nested_associate()
        => AssertTranslationFailed(base.Union_associates_then_filter_on_nested_associate);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
