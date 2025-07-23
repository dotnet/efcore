// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedNavigationsCollectionCosmosTest : OwnedNavigationsCollectionTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsCollectionCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Count(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (ARRAY_LENGTH(c["RelatedCollection"]) = 2)
""");
            });

    public override Task Where(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Where(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((
    SELECT VALUE COUNT(1)
    FROM r IN c["RelatedCollection"]
    WHERE (r["Int"] != 8)) = 2)
""");
            });

    public override async Task OrderBy_ElementAt(bool async)
    {
        if (async)
        {
            // 'ORDER BY' is not supported in subqueries.
            await Assert.ThrowsAsync<CosmosException>(() => base.OrderBy_ElementAt(async));

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
    }

    public override Task Index_constant(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Index_constant(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][0]["Int"] = 8)
""");
            });

    public override Task Index_parameter(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Index_parameter(a);

                AssertSql(
                    """
@i=?

SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][@i]["Int"] = 8)
""");
            });

    public override async Task Index_column(bool async)
    {
        if (async)
        {
            // The specified query includes 'member indexer' which is currently not supported
            await Assert.ThrowsAsync<CosmosException>(() => base.Index_column(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][(c["Id"] - 1)]["Int"] = 8)
""");
        }
    }

    public override async Task Index_out_of_bounds(bool async)
    {
        if (async)
        {
            await base.Index_out_of_bounds(async);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["RelatedCollection"][9999]["Int"] = 8)
""");
        }
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
