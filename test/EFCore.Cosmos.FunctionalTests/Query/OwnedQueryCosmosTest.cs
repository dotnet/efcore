// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OwnedQueryCosmosTest : OwnedQueryTestBase<OwnedQueryCosmosTest.OwnedQueryCosmosFixture>
{
    public OwnedQueryCosmosTest(OwnedQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    [ConditionalTheory(Skip = "Issue#17246")]
    public override Task Query_loads_reference_nav_automatically_in_projection(bool async)
        => base.Query_loads_reference_nav_automatically_in_projection(async);

    [ConditionalTheory(Skip = "SelectMany #17246")]
    public override Task Query_with_owned_entity_equality_operator(bool async)
        => base.Query_with_owned_entity_equality_operator(async);

    [ConditionalTheory(Skip = "Count #16146")]
    public override Task Navigation_rewrite_on_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_collection(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""LeafB"") OR ((c[""Discriminator""] = ""LeafA"") OR ((c[""Discriminator""] = ""Branch"") OR (c[""Discriminator""] = ""OwnedPerson""))))
""");
            });

    [ConditionalTheory(Skip = "Issue#16926")]
    public override Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_collection_with_composition(a);

                AssertSql(" ");
            });

    [ConditionalTheory(Skip = "Issue#16926")]
    public override Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_collection_with_composition_complex(a);

                AssertSql(" ");
            });

    public override Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_reference_projecting_entity(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["Country"]["Name"] = "USA"))
""");
            });

    public override Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Navigation_rewrite_on_owned_reference_projecting_scalar(a);

                AssertSql(
                    """
SELECT c["PersonAddress"]["Country"]["Name"]
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["Country"]["Name"] = "USA"))
""");
            });

    public override Task Query_for_base_type_loads_all_owned_navs(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_base_type_loads_all_owned_navs(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Query_for_branch_type_loads_all_owned_navs(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_branch_type_loads_all_owned_navs(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("Branch", "LeafA")
""");
            });

    public override Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_branch_type_loads_all_owned_navs_tracking(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("Branch", "LeafA")
""");
            });

    public override Task Query_for_leaf_type_loads_all_owned_navs(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_for_leaf_type_loads_all_owned_navs(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "LeafA")
""");
            });

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
        => base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_count(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task
        Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool async)
        => base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
            async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Project_multiple_owned_navigations(bool async)
        => base.Project_multiple_owned_navigations(async);

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
        => base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(async);

    [ConditionalTheory(Skip = "SelectMany #17246")]
    public override Task SelectMany_on_owned_collection(bool async)
        => base.SelectMany_on_owned_collection(async);

    [ConditionalTheory(Skip = "SelectMany #17246")]
    public override Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
        => base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(async);

    [ConditionalTheory(Skip = "SelectMany #17246")]
    public override Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
        => base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(async);

    [ConditionalTheory(Skip = "SelectMany #17246")]
    public override Task Query_with_owned_entity_equality_method(bool async)
        => base.Query_with_owned_entity_equality_method(async);

    [ConditionalTheory(Skip = "SelectMany #17246")]
    public override Task Query_with_owned_entity_equality_object_method(bool async)
        => base.Query_with_owned_entity_equality_object_method(async);

    public override Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Query_with_OfType_eagerly_loads_correct_owned_navigations(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Discriminator"] = "LeafA"))
""");
            });

    [ConditionalTheory(Skip = "Distinct ordering #16156")]
    public override Task Query_when_subquery(bool async)
        => base.Query_when_subquery(async);

    [ConditionalTheory(Skip = "Count #16146")]
    public override Task No_ignored_include_warning_when_implicit_load(bool async)
        => base.No_ignored_include_warning_when_implicit_load(async);

    [ConditionalTheory(Skip = "Skip withouth Take #18923")]
    public override Task Client_method_skip_loads_owned_navigations(bool async)
        => base.Client_method_skip_loads_owned_navigations(async);

    [ConditionalTheory(Skip = "Skip withouth Take #18923")]
    public override Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
        => base.Client_method_skip_loads_owned_navigations_variation_2(async);

    [ConditionalTheory(Skip = "Composition over embedded collection #16926")]
    public override Task Where_owned_collection_navigation_ToList_Count(bool async)
        => base.Where_owned_collection_navigation_ToList_Count(async);

    [ConditionalTheory(Skip = "Composition over embedded collection #16926")]
    public override Task Where_collection_navigation_ToArray_Count(bool async)
        => base.Where_collection_navigation_ToArray_Count(async);

    [ConditionalTheory(Skip = "Composition over embedded collection #16926")]
    public override Task Where_collection_navigation_AsEnumerable_Count(bool async)
        => base.Where_collection_navigation_AsEnumerable_Count(async);

    [ConditionalTheory(Skip = "Composition over embedded collection #16926")]
    public override Task Where_collection_navigation_ToList_Count_member(bool async)
        => base.Where_collection_navigation_ToList_Count_member(async);

    [ConditionalTheory(Skip = "Composition over embedded collection #16926")]
    public override Task Where_collection_navigation_ToArray_Length_member(bool async)
        => base.Where_collection_navigation_ToArray_Length_member(async);

    [ConditionalTheory(Skip = "Issue #16146")]
    public override Task GroupBy_with_multiple_aggregates_on_owned_navigation_properties(bool async)
        => base.GroupBy_with_multiple_aggregates_on_owned_navigation_properties(async);

    public override Task Can_query_on_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_on_indexer_properties(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Name"] = "Mona Cy"))
""");
            });

    public override Task Can_query_on_owned_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_on_owned_indexer_properties(a);

                AssertSql(
                    """
SELECT c["Name"]
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["ZipCode"] = 38654))
""");
            });

    public override Task Can_query_on_indexer_property_when_property_name_from_closure(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_on_indexer_property_when_property_name_from_closure(a);

                AssertSql(
                    """
SELECT c["Name"]
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Name"] = "Mona Cy"))
""");
            });

    public override Task Can_project_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_indexer_properties(a);

                AssertSql(
                    """
SELECT c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Can_project_owned_indexer_properties(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_owned_indexer_properties(a);

                AssertSql(
                    """
SELECT c["PersonAddress"]["AddressLine"]
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Can_project_indexer_properties_converted(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_indexer_properties_converted(a);

                AssertSql(
                    """
SELECT c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    [ConditionalTheory(Skip = "OrderBy requires composite index #17246")]
    public override async Task Can_OrderBy_indexer_properties(bool async)
    {
        await base.Can_OrderBy_indexer_properties(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "OrderBy requires composite index #17246")]
    public override async Task Can_OrderBy_indexer_properties_converted(bool async)
    {
        await base.Can_OrderBy_indexer_properties_converted(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "OrderBy requires composite index #17246")]
    public override async Task Can_OrderBy_owned_indexer_properties(bool async)
    {
        await base.Can_OrderBy_owned_indexer_properties(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "OrderBy requires composite index #17246")]
    public override async Task Can_OrderBy_owened_indexer_properties_converted(bool async)
    {
        await base.Can_OrderBy_owened_indexer_properties_converted(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "GroupBy #17246")]
    public override async Task Can_group_by_indexer_property(bool isAsync)
    {
        await base.Can_group_by_indexer_property(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "GroupBy #17246")]
    public override async Task Can_group_by_converted_indexer_property(bool isAsync)
    {
        await base.Can_group_by_converted_indexer_property(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "GroupBy #17246")]
    public override async Task Can_group_by_owned_indexer_property(bool isAsync)
    {
        await base.Can_group_by_owned_indexer_property(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "GroupBy #17246")]
    public override async Task Can_group_by_converted_owned_indexer_property(bool isAsync)
    {
        await base.Can_group_by_converted_owned_indexer_property(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "Join #17246")]
    public override async Task Can_join_on_indexer_property_on_query(bool async)
    {
        await base.Can_join_on_indexer_property_on_query(async);

        AssertSql(" ");
    }

    public override Task Projecting_indexer_property_ignores_include(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Projecting_indexer_property_ignores_include(a);

                AssertSql(
                    """
SELECT VALUE {"Nation" : c["PersonAddress"]["ZipCode"]}
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Projecting_indexer_property_ignores_include_converted(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Projecting_indexer_property_ignores_include_converted(a);

                AssertSql(
                    """
SELECT VALUE {"Nation" : c["PersonAddress"]["ZipCode"]}
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    [ConditionalTheory(Skip = "Subquery #17246")]
    public override async Task Indexer_property_is_pushdown_into_subquery(bool isAsync)
    {
        await base.Indexer_property_is_pushdown_into_subquery(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "Composition over owned collection #17246")]
    public override async Task Can_query_indexer_property_on_owned_collection(bool isAsync)
    {
        await base.Can_query_indexer_property_on_owned_collection(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "No SelectMany, No Ability to Include navigation back to owner #17246")]
    public override Task NoTracking_Include_with_cycles_does_not_throw_when_performing_identity_resolution(
        bool async,
        bool useAsTracking)
        => base.NoTracking_Include_with_cycles_does_not_throw_when_performing_identity_resolution(async, useAsTracking);

    [ConditionalTheory(Skip = "No Composite index to process custom ordering #17246")]
    public override async Task Ordering_by_identifying_projection(bool async)
    {
        await base.Ordering_by_identifying_projection(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "Composition over owned collection #17246")]
    public override async Task Query_on_collection_entry_works_for_owned_collection(bool isAsync)
    {
        await base.Query_on_collection_entry_works_for_owned_collection(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "issue #17246")]
    public override async Task Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(
        bool isAsync)
    {
        await base.Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(isAsync);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override async Task Left_join_on_entity_with_owned_navigations(bool async)
    {
        await base.Left_join_on_entity_with_owned_navigations(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "LeftJoin #17314")]
    public override async Task Left_join_on_entity_with_owned_navigations_complex(bool async)
    {
        await base.Left_join_on_entity_with_owned_navigations_complex(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "GroupBy #17314")]
    public override async Task GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(bool async)
    {
        await base.GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(async);

        AssertSql();
    }

    public override Task Filter_on_indexer_using_closure(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Filter_on_indexer_using_closure(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["ZipCode"] = 38654))
""");
            });

    public override Task Filter_on_indexer_using_function_argument(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Filter_on_indexer_using_function_argument(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["PersonAddress"]["ZipCode"] = 38654))
""");
            });

    public override Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool async)
        => AssertTranslationFailed(() => base.Preserve_includes_when_applying_skip_take_after_anonymous_type_select(async));

    public override Task Can_project_owned_indexer_properties_converted(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_project_owned_indexer_properties_converted(a);

                AssertSql(
                    """
SELECT c["PersonAddress"]["AddressLine"]
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Can_query_owner_with_different_owned_types_having_same_property_name_in_hierarchy(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_owner_with_different_owned_types_having_same_property_name_in_hierarchy(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("HeliumBalloon", "HydrogenBalloon")
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

SELECT c
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
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

SELECT c
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
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
                    await base.Non_nullable_property_through_optional_navigation(a);

                    AssertSql(
                        """
SELECT c["Throned"]["Value"]
FROM root c
WHERE (c["Discriminator"] = "Barton")
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
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Simple_query_entity_with_owned_collection(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Simple_query_entity_with_owned_collection(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Star")
""");
            });

    public override Task Throw_for_owned_entities_without_owner_in_tracking_query(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Throw_for_owned_entities_without_owner_in_tracking_query(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
""");
            });

    public override Task Unmapped_property_projection_loads_owned_navigations(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(
            async, async a =>
            {
                await base.Unmapped_property_projection_loads_owned_navigations(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA") AND (c["Id"] = 1))
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

SELECT c
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
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

SELECT c
FROM root c
WHERE c["Discriminator"] IN ("OwnedPerson", "Branch", "LeafB", "LeafA")
ORDER BY c["Id"]
OFFSET 0 LIMIT @__p_0
""");
            });

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

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<OwnedPerson>(
                eb =>
                {
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

            modelBuilder.Entity<Fink>().HasData(
                new { Id = 1, BartonId = 1 });

            modelBuilder.Entity<Balloon>();
            modelBuilder.Entity<HydrogenBalloon>().OwnsOne(e => e.Gas);
            modelBuilder.Entity<HeliumBalloon>().OwnsOne(e => e.Gas);
        }
    }
}
