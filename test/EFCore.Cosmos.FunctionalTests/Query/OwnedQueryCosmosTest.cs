// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OwnedQueryCosmosTest : OwnedQueryTestBase<OwnedQueryCosmosTest.OwnedQueryCosmosFixture>
{
    public OwnedQueryCosmosTest(OwnedQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Query_loads_reference_nav_automatically_in_projection(bool async)
    {
        // Fink.Barton is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Query_with_owned_entity_equality_operator(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Query_with_owned_entity_equality_operator(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

        AssertSql();
    }

    [ConditionalTheory]
    public override Task Navigation_rewrite_on_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(c["Orders"]) > 0))
ORDER BY c["Id"]
""");
            });

    [ConditionalTheory]
    public override async Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception =
                await Assert.ThrowsAsync<CosmosException>(() => base.Navigation_rewrite_on_owned_collection_with_composition(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE (ARRAY(
    SELECT VALUE (o["Id"] != 42)
    FROM o IN c["Orders"]
    ORDER BY o["Id"])[0] ?? false)
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
""");
        }
    }

    public override async Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // TODO: #33995
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Navigation_rewrite_on_owned_collection_with_composition_complex(async));

            AssertSql();
        }
    }

    public override Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_reference_projecting_entity(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["Country"]["Name"] = "USA"))
""");
            });

    public override Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_reference_projecting_scalar(a);

                AssertSql(
                    """
SELECT VALUE c["PersonAddress"]["Country"]["Name"]
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["Country"]["Name"] = "USA"))
""");
            });

    public override Task Query_for_base_type_loads_all_owned_navs(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_base_type_loads_all_owned_navs(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Query_for_branch_type_loads_all_owned_navs(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_branch_type_loads_all_owned_navs(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("Branch", "LeafA")
""");
            });

    public override Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_branch_type_loads_all_owned_navs_tracking(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("Branch", "LeafA")
""");
            });

    public override Task Query_for_leaf_type_loads_all_owned_navs(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_leaf_type_loads_all_owned_navs(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] = "LeafA")
""");
            });

    public override async Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Set_throws_for_owned_type(bool async)
    {
        await base.Set_throws_for_owned_type(async);

        AssertSql();
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task
        Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
            bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
                async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Project_multiple_owned_navigations(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Project_multiple_owned_navigations(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    public override async Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery("Planet", "OwnedPerson"));

        AssertSql();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task SelectMany_on_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.SelectMany_on_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE o
FROM root c
JOIN o IN c["Orders"]
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task SelectMany_with_result_selector(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.SelectMany_with_result_selector(a);

                AssertSql(
                    """
SELECT VALUE
{
    "PersonId" : c["Id"],
    "OrderId" : o["Id"]
}
FROM root c
JOIN o IN c["Orders"]
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    // Address.Planet is a non-owned navigation, cross-document join
    public override async Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
    {
        // Address.Planet is a non-owned navigation, cross-document join
        await AssertTranslationFailedWithDetails(
            () => base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Planet), nameof(OwnedPerson)));

        AssertSql();
    }

    // Non-correlated queries not supported by Cosmos
    public override Task Query_with_owned_entity_equality_method(bool async)
        => AssertTranslationFailed(() => base.Query_with_owned_entity_equality_method(async));

    // Non-correlated queries not supported by Cosmos
    public override Task Query_with_owned_entity_equality_object_method(bool async)
        => AssertTranslationFailed(() => base.Query_with_owned_entity_equality_object_method(async));

    public override Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_with_OfType_eagerly_loads_correct_owned_navigations(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Terminator"] = "LeafA"))
""");
            });

    // TODO: Subquery pushdown, #33968
    public override Task Query_when_subquery(bool async)
        => AssertTranslationFailed(() => base.Query_when_subquery(async));

    public override Task Project_owned_reference_navigation_which_owns_additional(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_owned_reference_navigation_which_owns_additional(a);

                // TODO: The following should project out c["PersonAddress"], not c: #34067
                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
""");
            });

    // Issue #34068
    public override Task Project_owned_reference_navigation_which_does_not_own_additional(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Project_owned_reference_navigation_which_does_not_own_additional(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
""");
            });

    public override Task No_ignored_include_warning_when_implicit_load(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.No_ignored_include_warning_when_implicit_load(a);

                AssertSql(
                    """
SELECT VALUE COUNT(1)
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override async Task Client_method_skip_loads_owned_navigations(bool async)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Client_method_skip_loads_owned_navigations(async));

        Assert.Equal(CosmosStrings.OffsetRequiresLimit, exception.Message);
    }

    public override async Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
    {
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Client_method_skip_loads_owned_navigations_variation_2(async));

        Assert.Equal(CosmosStrings.OffsetRequiresLimit, exception.Message);
    }

    public override Task Where_owned_collection_navigation_ToList_Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                // TODO: #34011
                // We override this test because the test data for this class gets saved incorrectly - the Order.Details collection gets
                // persisted as null instead of [] when there are no Details. So we change the Count we check to 1.
                await AssertQuery(
                    a,
                    ss => ss.Set<OwnedPerson>()
                        .OrderBy(p => p.Id)
                        .SelectMany(p => p.Orders)
                        .Select(p => p.Details.ToList())
                        .Where(e => e.Count() == 1),
                    assertOrder: true,
                    elementAsserter: (e, a) => AssertCollection(e, a));

                AssertSql(
                    """
SELECT VALUE o["Details"]
FROM root c
JOIN o IN c["Orders"]
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(o["Details"]) = 1))
ORDER BY c["Id"]
""");
            });

    public override Task Where_collection_navigation_ToArray_Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                // TODO: #34011
                // We override this test because the test data for this class gets saved incorrectly - the Order.Details collection gets
                // persisted as null instead of [] when there are no Details. So we change the Count we check to 1.
                await AssertQuery(
                    a,
                    ss => ss.Set<OwnedPerson>()
                        .OrderBy(p => p.Id)
                        .SelectMany(p => p.Orders)
                        .Select(p => p.Details.AsEnumerable().ToArray())
                        .Where(e => e.Count() == 1),
                    assertOrder: true,
                    elementAsserter: (e, a) => AssertCollection(e, a));

                AssertSql(
                    """
SELECT VALUE o
FROM root c
JOIN o IN c["Orders"]
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(o["Details"]) = 1))
ORDER BY c["Id"]
""");
            });

    public override Task Where_collection_navigation_AsEnumerable_Count(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                // TODO: #34011
                // We override this test because the test data for this class gets saved incorrectly - the Order.Details collection gets
                // persisted as null instead of [] when there are no Details. So we change the Count we check to 1.
                await AssertQuery(
                    a,
                    ss => ss.Set<OwnedPerson>()
                        .OrderBy(p => p.Id)
                        .SelectMany(p => p.Orders)
                        .Select(p => p.Details.AsEnumerable())
                        .Where(e => e.Count() == 1),
                    assertOrder: true,
                    elementAsserter: (e, a) => AssertCollection(e, a));

                AssertSql(
                    """
SELECT VALUE o
FROM root c
JOIN o IN c["Orders"]
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(o["Details"]) = 1))
ORDER BY c["Id"]
""");
            });

    public override Task Where_collection_navigation_ToList_Count_member(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                // TODO: #34011
                // We override this test because the test data for this class gets saved incorrectly - the Order.Details collection gets
                // persisted as null instead of [] when there are no Details. So we change the Count we check to 1.
                await AssertQuery(
                    a,
                    ss => ss.Set<OwnedPerson>()
                        .OrderBy(p => p.Id)
                        .SelectMany(p => p.Orders)
                        .Select(p => p.Details.ToList())
                        .Where(e => e.Count == 1),
                    assertOrder: true,
                    elementAsserter: (e, a) => AssertCollection(e, a));

                AssertSql(
                    """
SELECT VALUE o["Details"]
FROM root c
JOIN o IN c["Orders"]
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(o["Details"]) = 1))
ORDER BY c["Id"]
""");
            });

    public override Task Where_collection_navigation_ToArray_Length_member(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                // TODO: #34011
                // We override this test because the test data for this class gets saved incorrectly - the Order.Details collection gets persisted
                // as null instead of [].
                await AssertQuery(
                    a,
                    ss => ss.Set<OwnedPerson>()
                        .OrderBy(p => p.Id)
                        .SelectMany(p => p.Orders)
                        .Select(p => p.Details.AsEnumerable().ToArray())
                        .Where(e => e.Length == 1),
                    assertOrder: true,
                    elementAsserter: (e, a) => AssertCollection(e, a));

                AssertSql(
                    """
SELECT VALUE o
FROM root c
JOIN o IN c["Orders"]
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(o["Details"]) = 1))
ORDER BY c["Id"]
""");
            });

    // TODO: GroupBy, #17313
    public override Task GroupBy_with_multiple_aggregates_on_owned_navigation_properties(bool async)
        => AssertTranslationFailed(() => base.GroupBy_with_multiple_aggregates_on_owned_navigation_properties(async));

    public override Task Can_query_on_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_on_indexer_properties(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Name"] = "Mona Cy"))
""");
            });

    public override Task Can_query_on_owned_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_on_owned_indexer_properties(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["ZipCode"] = 38654))
""");
            });

    public override Task Can_query_on_indexer_property_when_property_name_from_closure(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_on_indexer_property_when_property_name_from_closure(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Name"] = "Mona Cy"))
""");
            });

    public override Task Can_project_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_indexer_properties(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Can_project_owned_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_owned_indexer_properties(a);

                AssertSql(
                    """
SELECT VALUE c["PersonAddress"]["AddressLine"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Can_project_indexer_properties_converted(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_indexer_properties_converted(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override async Task Can_OrderBy_indexer_properties(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Can_OrderBy_indexer_properties(async));

            Assert.Contains(
                "The order by query does not have a corresponding composite index that it can be served from.",
                exception.Message);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Name"], c["Id"]
""");
        }
    }

    public override async Task Can_OrderBy_indexer_properties_converted(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Can_OrderBy_indexer_properties_converted(async));

            Assert.Contains(
                "The order by query does not have a corresponding composite index that it can be served from.",
                exception.Message);

            AssertSql(
                """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Name"], c["Id"]
""");
        }
    }

    public override async Task Can_OrderBy_owned_indexer_properties(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Can_OrderBy_owned_indexer_properties(async));

            Assert.Contains(
                "The order by query does not have a corresponding composite index that it can be served from.",
                exception.Message);

            AssertSql(
                """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["PersonAddress"]["ZipCode"], c["Id"]
""");
        }
    }

    public override async Task Can_OrderBy_owned_indexer_properties_converted(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Can_OrderBy_owned_indexer_properties_converted(async));

            Assert.Contains(
                "The order by query does not have a corresponding composite index that it can be served from.",
                exception.Message);

            AssertSql(
                """
SELECT VALUE c["Name"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["PersonAddress"]["ZipCode"], c["Id"]
""");
        }
    }

    // TODO: GroupBy, #17313
    public override Task Can_group_by_indexer_property(bool async)
        => AssertTranslationFailed(() => base.Can_group_by_indexer_property(async));

    // TODO: GroupBy, #17313
    public override Task Can_group_by_converted_indexer_property(bool async)
        => AssertTranslationFailed(() => base.Can_group_by_converted_indexer_property(async));

    // TODO: GroupBy, #17313
    public override Task Can_group_by_owned_indexer_property(bool async)
        => AssertTranslationFailed(() => base.Can_group_by_owned_indexer_property(async));

    // TODO: GroupBy, #17313
    public override Task Can_group_by_converted_owned_indexer_property(bool async)
        => AssertTranslationFailed(() => base.Can_group_by_converted_owned_indexer_property(async));

    // Uncorrelated JOINS aren't supported by Cosmos
    public override Task Can_join_on_indexer_property_on_query(bool async)
        => AssertTranslationFailed(() => base.Can_group_by_converted_owned_indexer_property(async));

    public override Task Projecting_indexer_property_ignores_include(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Projecting_indexer_property_ignores_include(a);

                AssertSql(
                    """
SELECT VALUE c["PersonAddress"]["ZipCode"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Projecting_indexer_property_ignores_include_converted(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Projecting_indexer_property_ignores_include_converted(a);

                AssertSql(
                    """
SELECT VALUE c["PersonAddress"]["ZipCode"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Indexer_property_is_pushdown_into_subquery(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Indexer_property_is_pushdown_into_subquery(async),
            CosmosStrings.NonCorrelatedSubqueriesNotSupported);

    public override Task Can_query_indexer_property_on_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_indexer_property_on_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c["Name"]
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND ((
    SELECT VALUE COUNT(1)
    FROM o IN c["Orders"]
    WHERE (DateTimePart("yyyy", o["OrderDate"]) = 2018)) = 1))
""");
            });

    public override async Task NoTracking_Include_with_cycles_throws(bool async)
    {
        await base.NoTracking_Include_with_cycles_throws(async);

        AssertSql();
    }

    // Order.Client is a non-owned navigation, cross-document join
    public override Task NoTracking_Include_with_cycles_does_not_throw_when_performing_identity_resolution(
        bool async,
        bool useAsTracking)
        => AssertTranslationFailedWithDetails(
            () => base.NoTracking_Include_with_cycles_does_not_throw_when_performing_identity_resolution(async, useAsTracking),
            CosmosStrings.CrossDocumentJoinNotSupported);

    public override async Task Trying_to_access_non_existent_indexer_property_throws_meaningful_exception(bool async)
    {
        await base.Trying_to_access_non_existent_indexer_property_throws_meaningful_exception(async);

        AssertSql();
    }

    public override async Task Ordering_by_identifying_projection(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.Ordering_by_identifying_projection(async));

            Assert.Contains(
                "The order by query does not have a corresponding composite index that it can be served from.",
                exception.Message);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["PersonAddress"]["PlaceType"], c["Id"]
""");
        }
    }

    public override Task Query_on_collection_entry_works_for_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_on_collection_entry_works_for_owned_collection(a);

                AssertSql(
                    """ReadItem(None, 1)""",
                    //
                    """
@__p_0='1'

SELECT VALUE o
FROM root c
JOIN o IN c["Orders"]
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (o["ClientId"] = @__p_0))
""");
            });

    public override async Task Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(
        bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(Barton), nameof(Fink)));

        AssertSql();
    }

    public override async Task Left_join_on_entity_with_owned_navigations(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Left_join_on_entity_with_owned_navigations(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(OwnedPerson), nameof(Planet)));

        AssertSql();
    }

    public override async Task Left_join_on_entity_with_owned_navigations_complex(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Left_join_on_entity_with_owned_navigations_complex(async),
            CosmosStrings.MultipleRootEntityTypesReferencedInQuery(nameof(OwnedPerson), nameof(Planet)));

        AssertSql();
    }

    // TODO: GroupBy, #17313
    public override Task GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(bool async)
        => AssertTranslationFailed(() => base.GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(async));

    public override Task Filter_on_indexer_using_closure(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Filter_on_indexer_using_closure(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["ZipCode"] = 38654))
""");
            });

    public override Task Filter_on_indexer_using_function_argument(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Filter_on_indexer_using_function_argument(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["ZipCode"] = 38654))
""");
            });

    // Non-correlated queries not supported by Cosmos
    public override Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool async)
        => AssertTranslationFailed(() => base.Preserve_includes_when_applying_skip_take_after_anonymous_type_select(async));

    public override Task Can_project_owned_indexer_properties_converted(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_owned_indexer_properties_converted(a);

                AssertSql(
                    """
SELECT VALUE c["PersonAddress"]["AddressLine"]
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Can_query_owner_with_different_owned_types_having_same_property_name_in_hierarchy(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_owner_with_different_owned_types_having_same_property_name_in_hierarchy(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("HeliumBalloon", "HydrogenBalloon")
""");
            });

    public override Task Client_method_skip_take_loads_owned_navigations_variation_2(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Client_method_skip_take_loads_owned_navigations_variation_2(a);

                AssertSql(
                    """
@__p_0='1'
@__p_1='2'

SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
OFFSET @__p_0 LIMIT @__p_1
""");
            });

    public override Task Client_method_skip_take_loads_owned_navigations(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Client_method_skip_take_loads_owned_navigations(a);

                AssertSql(
                    """
@__p_0='1'
@__p_1='2'

SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
OFFSET @__p_0 LIMIT @__p_1
""");
            });

    public override async Task Non_nullable_property_through_optional_navigation(bool async)
    {
        // Sync always throws before getting to exception being tested.
        if (async)
        {
            await CosmosTestHelpers.Instance.NoSyncTest(
                async, async a =>
                {
                    await Assert.ThrowsAsync<NullReferenceException>(
                        () => AssertQuery(
                            async,
                            ss => ss.Set<Barton>().Select(e => new { e.Throned.Value })));

                    AssertSql(
                        """
SELECT VALUE c["Throned"]["Value"]
FROM root c
WHERE (c["Terminator"] = "Barton")
""");
                });
        }
    }

    public override Task Owned_entity_without_owner_does_not_throw_for_identity_resolution(bool async, bool useAsTracking)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Owned_entity_without_owner_does_not_throw_for_identity_resolution(a, useAsTracking);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Simple_query_entity_with_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Simple_query_entity_with_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] = "Star")
""");
            });

    public override Task Throw_for_owned_entities_without_owner_in_tracking_query(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Throw_for_owned_entities_without_owner_in_tracking_query(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Unmapped_property_projection_loads_owned_navigations(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Unmapped_property_projection_loads_owned_navigations(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Id"] = 1))
""");
            });

    public override Task Client_method_take_loads_owned_navigations(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Client_method_take_loads_owned_navigations(a);

                AssertSql(
                    """
@__p_0='2'

SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
OFFSET 0 LIMIT @__p_0
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Client_method_take_loads_owned_navigations_variation_2(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Client_method_take_loads_owned_navigations_variation_2(a);

                AssertSql(
                    """
@__p_0='2'

SELECT VALUE c
FROM root c
WHERE c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
OFFSET 0 LIMIT @__p_0
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Count_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Count_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(c["Orders"]) = 2))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Any_without_predicate_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Any_without_predicate_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(c["Orders"]) > 0))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Any_with_predicate_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Any_with_predicate_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND EXISTS (
    SELECT 1
    FROM o IN c["Orders"]
    WHERE (o["Id"] = -30)))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Contains_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Contains_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND EXISTS (
    SELECT 1
    FROM o IN c["Orders"]
    WHERE (o["Id"] = -30)))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task ElementAt_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.ElementAt_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Orders"][1]["Id"] = -11))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task ElementAtOrDefault_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.ElementAtOrDefault_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND ((c["Orders"][10] ?? null)["Id"] = -11))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task OrderBy_ElementAt_over_owned_collection(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            var exception = await Assert.ThrowsAsync<CosmosException>(() => base.OrderBy_ElementAt_over_owned_collection(async));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY(
    SELECT VALUE o["Id"]
    FROM o IN c["Orders"]
    ORDER BY o["Id"])[1] = -10))
""");
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Skip_Take_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Skip_Take_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(ARRAY_SLICE(c["Orders"], 1, 1)) = 1))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task FirstOrDefault_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.FirstOrDefault_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (DateTimePart("yyyy", (ARRAY(
    SELECT VALUE o["OrderDate"]
    FROM o IN c["Orders"]
    WHERE (o["Id"] > -20))[0] ?? "0001-01-01T00:00:00")) = 2018))
""");
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Distinct_over_owned_collection(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // TODO: Subquery pushdown, #33968
            await AssertTranslationFailed(() => base.Distinct_over_owned_collection(async));

            AssertSql();
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Union_over_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Union_over_owned_collection(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (c["Terminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (ARRAY_LENGTH(SetUnion(ARRAY(
    SELECT VALUE o
    FROM o IN c["Orders"]
    WHERE (o["Id"] = -10)), ARRAY(
    SELECT VALUE o0
    FROM o0 IN c["Orders"]
    WHERE (o0["Id"] = -11)))) = 2))
""");
            });

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    public class OwnedQueryCosmosFixture : OwnedQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(
                builder.ConfigureWarnings(
                    w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.HasDefaultContainer("OwnedQueryTest");

            modelBuilder.Entity<OwnedPerson>(
                eb =>
                {
                    eb.ToContainer("OwnedPeople");
                    eb.IndexerProperty<string>("Name");
                    eb.HasData(
                        new
                        {
                            Id = 1,
                            id = Guid.NewGuid().ToString(),
                            Name = "Mona Cy"
                        });

                    eb.OwnsOne(
                        p => p.PersonAddress, ab =>
                        {
                            ab.IndexerProperty<string>("AddressLine");
                            ab.IndexerProperty(typeof(int), "ZipCode");
                            ab.HasData(
                                new
                                {
                                    OwnedPersonId = 1,
                                    PlaceType = "Land",
                                    AddressLine = "804 S. Lakeshore Road",
                                    ZipCode = 38654
                                },
                                new
                                {
                                    OwnedPersonId = 2,
                                    PlaceType = "Land",
                                    AddressLine = "7 Church Dr.",
                                    ZipCode = 28655
                                },
                                new
                                {
                                    OwnedPersonId = 3,
                                    PlaceType = "Land",
                                    AddressLine = "72 Hickory Rd.",
                                    ZipCode = 07728
                                },
                                new
                                {
                                    OwnedPersonId = 4,
                                    PlaceType = "Land",
                                    AddressLine = "28 Strawberry St.",
                                    ZipCode = 19053
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 1,
                                            PlanetId = 1,
                                            Name = "USA"
                                        },
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 2,
                                            PlanetId = 1,
                                            Name = "USA"
                                        },
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 3,
                                            PlanetId = 1,
                                            Name = "USA"
                                        },
                                        new
                                        {
                                            OwnedAddressOwnedPersonId = 4,
                                            PlanetId = 1,
                                            Name = "USA"
                                        });

                                    cb.HasOne(cc => cc.Planet).WithMany().HasForeignKey(ee => ee.PlanetId)
                                        .OnDelete(DeleteBehavior.Restrict);
                                });
                        });

                    eb.OwnsMany(
                        p => p.Orders, ob =>
                        {
                            ob.HasKey(o => o.Id);
                            ob.IndexerProperty<DateTime>("OrderDate");
                            ob.HasData(
                                new
                                {
                                    Id = -10,
                                    ClientId = 1,
                                    OrderDate = Convert.ToDateTime("2018-07-11 10:01:41")
                                },
                                new
                                {
                                    Id = -11,
                                    ClientId = 1,
                                    OrderDate = Convert.ToDateTime("2015-03-03 04:37:59")
                                },
                                new
                                {
                                    Id = -20,
                                    ClientId = 2,
                                    OrderDate = Convert.ToDateTime("2015-05-25 20:35:48")
                                },
                                new
                                {
                                    Id = -30,
                                    ClientId = 3,
                                    OrderDate = Convert.ToDateTime("2014-11-10 04:32:42")
                                },
                                new
                                {
                                    Id = -40,
                                    ClientId = 4,
                                    OrderDate = Convert.ToDateTime("2016-04-25 19:23:56")
                                }
                            );

                            ob.OwnsMany(
                                e => e.Details, odb =>
                                {
                                    odb.HasData(
                                        new
                                        {
                                            Id = -100,
                                            OrderId = -10,
                                            OrderClientId = 1,
                                            Detail = "Discounted Order"
                                        },
                                        new
                                        {
                                            Id = -101,
                                            OrderId = -10,
                                            OrderClientId = 1,
                                            Detail = "Full Price Order"
                                        },
                                        new
                                        {
                                            Id = -200,
                                            OrderId = -20,
                                            OrderClientId = 2,
                                            Detail = "Internal Order"
                                        },
                                        new
                                        {
                                            Id = -300,
                                            OrderId = -30,
                                            OrderClientId = 3,
                                            Detail = "Bulk Order"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<Branch>(
                eb =>
                {
                    eb.HasData(
                        new
                        {
                            Id = 2,
                            id = Guid.NewGuid().ToString(),
                            Name = "Antigonus Mitul"
                        });

                    eb.OwnsOne(
                        p => p.BranchAddress, ab =>
                        {
                            ab.IndexerProperty<string>("BranchName").IsRequired();
                            ab.HasData(
                                new
                                {
                                    BranchId = 2,
                                    PlaceType = "Land",
                                    BranchName = "BranchA"
                                },
                                new
                                {
                                    BranchId = 3,
                                    PlaceType = "Land",
                                    BranchName = "BranchB"
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressBranchId = 2,
                                            PlanetId = 1,
                                            Name = "Canada"
                                        },
                                        new
                                        {
                                            OwnedAddressBranchId = 3,
                                            PlanetId = 1,
                                            Name = "Canada"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<LeafA>(
                eb =>
                {
                    eb.HasData(
                        new
                        {
                            Id = 3,
                            id = Guid.NewGuid().ToString(),
                            Name = "Madalena Morana"
                        });

                    eb.OwnsOne(
                        p => p.LeafAAddress, ab =>
                        {
                            ab.IndexerProperty<int>("LeafType");
                            ab.HasData(
                                new
                                {
                                    LeafAId = 3,
                                    PlaceType = "Land",
                                    LeafType = 1
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressLeafAId = 3,
                                            PlanetId = 1,
                                            Name = "Mexico"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<LeafB>(
                eb =>
                {
                    eb.HasData(
                        new
                        {
                            Id = 4,
                            id = Guid.NewGuid().ToString(),
                            Name = "Vanda Waldemar"
                        });

                    eb.OwnsOne(
                        p => p.LeafBAddress, ab =>
                        {
                            ab.IndexerProperty<string>("LeafBType").IsRequired();
                            ab.HasData(
                                new
                                {
                                    LeafBId = 4,
                                    PlaceType = "Land",
                                    LeafBType = "Green"
                                });

                            ab.OwnsOne(
                                a => a.Country, cb =>
                                {
                                    cb.HasData(
                                        new
                                        {
                                            OwnedAddressLeafBId = 4,
                                            PlanetId = 1,
                                            Name = "Panama"
                                        });
                                });
                        });
                });

            modelBuilder.Entity<Planet>(
                pb =>
                {
                    pb.ToContainer("Planets");
                    pb.HasDiscriminatorInJsonId();
                    pb.HasData(
                        new
                        {
                            Id = 1,
                            id = Guid.NewGuid().ToString(),
                            StarId = 1
                        });
                });

            modelBuilder.Entity<Moon>(
                mb =>
                {
                    mb.ToContainer("Planets");
                    mb.HasDiscriminatorInJsonId();
                    mb.HasData(
                        new
                        {
                            Id = 1,
                            id = Guid.NewGuid().ToString(),
                            PlanetId = 1,
                            Diameter = 3474
                        });
                });

            modelBuilder.Entity<Star>(
                sb =>
                {
                    sb.ToContainer("Planets");
                    sb.HasDiscriminatorInJsonId();
                    sb.HasData(
                        new
                        {
                            Id = 1,
                            id = Guid.NewGuid().ToString(),
                            Name = "Sol"
                        });

                    sb.OwnsMany(
                        s => s.Composition, ob =>
                        {
                            ob.HasKey(o => o.Id);
                            ob.HasData(
                                new
                                {
                                    Id = "H",
                                    Name = "Hydrogen",
                                    StarId = 1
                                },
                                new
                                {
                                    Id = "He",
                                    Name = "Helium",
                                    StarId = 1
                                });
                        });
                });

            modelBuilder.Entity<Barton>(
                b =>
                {
                    b.ToContainer("Bartons");
                    b.HasDiscriminatorInJsonId();
                    b.OwnsOne(
                        e => e.Throned, b => b.HasData(
                            new
                            {
                                BartonId = 1,
                                Property = "Property",
                                Value = 42
                            }));
                    b.HasData(
                        new Barton { Id = 1, Simple = "Simple" },
                        new Barton { Id = 2, Simple = "Not" });
                });

            modelBuilder.Entity<Fink>(
                b =>
                {
                    b.ToContainer("Bartons");
                    b.HasDiscriminatorInJsonId();
                    b.HasData(
                        new { Id = 1, BartonId = 1 });
                });

            modelBuilder
                .Entity<Balloon>()
                .ToContainer("Balloons");

            modelBuilder
                .Entity<HydrogenBalloon>()
                .OwnsOne(e => e.Gas);

            modelBuilder
                .Entity<HeliumBalloon>()
                .OwnsOne(e => e.Gas);

            modelBuilder.HasEmbeddedDiscriminatorName("Terminator");
        }
    }
}
