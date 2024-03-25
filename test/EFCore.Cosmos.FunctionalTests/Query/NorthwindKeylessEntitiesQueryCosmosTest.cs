// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindKeylessEntitiesQueryCosmosTest : NorthwindKeylessEntitiesQueryTestBase<
    NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindKeylessEntitiesQueryCosmosTest(
        NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override Task KeylessEntity_simple(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.KeylessEntity_simple(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task KeylessEntity_where_simple(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.KeylessEntity_where_simple(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["City"] = "London"))
""");
            });

    public override async Task KeylessEntity_by_database_view(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Views are not supported.
            await Assert.ThrowsAsync<EqualException>(
                () => base.KeylessEntity_by_database_view(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "ProductView")
""");
        }
    }

    public override async Task Entity_mapped_to_view_on_right_side_of_join(bool async)
    {
        await AssertTranslationFailed(() => base.Entity_mapped_to_view_on_right_side_of_join(async));

        AssertSql();
    }

    public override async Task KeylessEntity_with_nav_defining_query(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Defining queries are not supported.
            await Assert.ThrowsAsync<EqualException>(
                () => base.KeylessEntity_with_nav_defining_query(async));

            AssertSql(
                """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["OrderCount"] > 0))
""");
        }
    }

    public override async Task KeylessEntity_with_mixed_tracking(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.KeylessEntity_with_mixed_tracking(async));

        AssertSql();
    }

    public override async Task KeylessEntity_with_included_nav(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.KeylessEntity_with_included_nav(async));

        AssertSql();
    }

    public override Task KeylessEntity_with_defining_query(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.KeylessEntity_with_defining_query(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (c["CustomerID"] = "ALFKI"))
""");
            });

    public override async Task KeylessEntity_with_defining_query_and_correlated_collection(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.KeylessEntity_with_defining_query_and_correlated_collection(async));

        AssertSql();
    }

    public override async Task KeylessEntity_select_where_navigation(bool async)
    {
        // Left join translation. Issue #17314.
        await AssertTranslationFailed(() => base.KeylessEntity_select_where_navigation(async));

        AssertSql();
    }

    public override async Task KeylessEntity_select_where_navigation_multi_level(bool async)
    {
        // Left join translation. Issue #17314.
        await AssertTranslationFailed(() => base.KeylessEntity_select_where_navigation_multi_level(async));

        AssertSql();
    }

    public override async Task KeylessEntity_with_included_navs_multi_level(bool async)
    {
        // Left join translation. Issue #17314.
        await AssertTranslationFailed(() => base.KeylessEntity_with_included_navs_multi_level(async));

        AssertSql();
    }

    public override async Task KeylessEntity_groupby(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.KeylessEntity_groupby(async));

        AssertSql();
    }

    public override async Task Collection_correlated_with_keyless_entity_in_predicate_works(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Collection_correlated_with_keyless_entity_in_predicate_works(async));

        AssertSql();
    }

    public override Task Auto_initialized_view_set(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Auto_initialized_view_set(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Count_over_keyless_entity(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Count_over_keyless_entity(a);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE (c["Discriminator"] = "Customer")
""");
            });

    public override Task Count_over_keyless_entity_with_pushdown(bool async)
        // Cosmos client evaluation. Issue #17246.
        => AssertTranslationFailed(() => base.Count_over_keyless_entity_with_pushdown(async));

    public override Task Count_over_keyless_entity_with_pushdown_empty_projection(bool async)
        // Cosmos client evaluation. Issue #17246.
        => AssertTranslationFailed(() => base.Count_over_keyless_entity_with_pushdown_empty_projection(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
