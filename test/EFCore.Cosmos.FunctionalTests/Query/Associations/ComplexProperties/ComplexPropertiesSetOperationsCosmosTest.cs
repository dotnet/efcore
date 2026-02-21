// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public class ComplexPropertiesSetOperationsCosmosTest
    : ComplexPropertiesSetOperationsTestBase<ComplexPropertiesCosmosFixture>
{
    public ComplexPropertiesSetOperationsCosmosTest(ComplexPropertiesCosmosFixture fixture, ITestOutputHelper outputHelper) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(outputHelper);
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
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Over_associate_collection_projected(queryTrackingBehavior));

    public override Task Over_assocate_collection_Select_nested_with_aggregates_projected(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Over_assocate_collection_Select_nested_with_aggregates_projected(queryTrackingBehavior));

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

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
