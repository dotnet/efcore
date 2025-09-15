// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsCollectionCosmosTest : OwnedNavigationsCollectionTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsCollectionCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["RelatedCollection"]) = 2)
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM r IN c["RelatedCollection"]
    WHERE (r["Int"] != 8)) = 2)
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        // 'ORDER BY' is not supported in subqueries.
        await Assert.ThrowsAsync<CosmosException>(() => base.OrderBy_ElementAt());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (ARRAY(
    SELECT VALUE r["Int"]
    FROM r IN c["RelatedCollection"]
    ORDER BY r["Id"])[0] = 8)
""");
    }

    #region Distinct

    public override Task Distinct()
        => AssertTranslationFailed(base.Distinct);

    public override Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAnyAsync<Exception>(() => base.Distinct_projected(queryTrackingBehavior));

    public override Task Distinct_over_projected_nested_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Distinct_over_projected_nested_collection);

    public override Task Distinct_over_projected_filtered_nested_collection()
        => Assert.ThrowsAsync<InvalidOperationException>(base.Distinct_over_projected_nested_collection);

    #endregion Distinct

    #region Index

    public override async Task Index_constant()
    {
        await base.Index_constant();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][0]["Int"] = 8)
""");
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql(
            """
@i='0'

SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][@i]["Int"] = 8)
""");
    }

    public override async Task Index_column()
    {
        // The specified query includes 'member indexer' which is currently not supported
        await Assert.ThrowsAsync<CosmosException>(() => base.Index_column());

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][(c["Id"] - 1)]["Int"] = 8)
""");
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][9999]["Int"] = 8)
""");
    }

    #endregion Index

    #region GroupBy

    [ConditionalFact]
    public override Task GroupBy()
        => AssertTranslationFailed(base.GroupBy);

    #endregion GroupBy

    public override async Task Select_within_Select_within_Select_with_aggregates()
    {
        await base.Select_within_Select_within_Select_with_aggregates();

        AssertSql(
            """
SELECT VALUE (
    SELECT VALUE SUM((
        SELECT VALUE MAX(n["Int"])
        FROM n IN r["NestedCollection"]))
    FROM r IN c["RelatedCollection"])
FROM root c
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
