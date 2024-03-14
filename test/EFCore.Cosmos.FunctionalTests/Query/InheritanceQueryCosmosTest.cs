// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class InheritanceQueryCosmosTest : InheritanceQueryTestBase<InheritanceQueryCosmosFixture>
{
    public InheritanceQueryCosmosTest(InheritanceQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        //TestLoggerFactory.TestOutputHelper = testOutputHelper;
    }

    public override Task Can_query_when_shared_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_when_shared_column(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = 1)
OFFSET 0 LIMIT 2
""",
                    //
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = 2)
OFFSET 0 LIMIT 2
""",
                    //
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = 3)
OFFSET 0 LIMIT 2
""");
            });

    public override Task Can_query_all_types_when_shared_column(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_all_types_when_shared_column(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN (0, 1, 2, 3)
""");
            });

    public override Task Can_use_of_type_animal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_animal(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
ORDER BY c["Species"]
""");
            });

    public override Task Can_use_is_kiwi(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_is_kiwi(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
            });

    public override Task Can_use_is_kiwi_with_cast(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_is_kiwi_with_cast(a);

                AssertSql(
                    """
SELECT VALUE {"Value" : ((c["Discriminator"] = "Kiwi") ? c["FoundOn"] : 0)}
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
            });

    public override Task Can_use_backwards_is_animal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_backwards_is_animal(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
            });

    public override Task Can_use_is_kiwi_with_other_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_is_kiwi_with_other_predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND ((c["Discriminator"] = "Kiwi") AND (c["CountryId"] = 1)))
""");
            });

    public override Task Can_use_is_kiwi_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_is_kiwi_in_projection(a);

                AssertSql(
                    """
SELECT VALUE {"c" : (c["Discriminator"] = "Kiwi")}
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
            });

    public override Task Can_use_of_type_bird(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_bird(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
""");
            });

    public override Task Can_use_of_type_bird_predicate(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_bird_predicate(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["CountryId"] = 1)) AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
""");
            });

    public override Task Can_use_of_type_bird_with_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_bird_with_projection(a);

                AssertSql(
                    """
SELECT c["EagleId"]
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
""");
            });

    public override Task Can_use_of_type_bird_first(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_bird_first(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND c["Discriminator"] IN ("Eagle", "Kiwi"))
ORDER BY c["Species"]
OFFSET 0 LIMIT 1
""");
            });

    public override Task Can_use_of_type_kiwi(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_kiwi(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
            });

    public override Task Can_use_backwards_of_type_animal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_backwards_of_type_animal(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
            });

    public override Task Can_use_of_type_rose(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_rose(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Daisy", "Rose") AND (c["Discriminator"] = "Rose"))
""");
            });

    public override Task Can_query_all_animals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_all_animals(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
ORDER BY c["Species"]
""");
            });

    [ConditionalTheory(Skip = "Issue#17246 Views are not supported")]
    public override async Task Can_query_all_animal_views(bool async)
    {
        await base.Can_query_all_animal_views(async);

        AssertSql(" ");
    }

    public override Task Can_query_all_plants(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_all_plants(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("Daisy", "Rose")
ORDER BY c["Species"]
""");
            });

    public override Task Can_filter_all_animals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_filter_all_animals(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Name"] = "Great spotted kiwi"))
ORDER BY c["Species"]
""");
            });

    public override Task Can_query_all_birds(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_all_birds(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
ORDER BY c["Species"]
""");
            });

    public override Task Can_query_just_kiwis(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_just_kiwis(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
OFFSET 0 LIMIT 2
""");
            });

    public override Task Can_query_just_roses(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_query_just_roses(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Rose")
OFFSET 0 LIMIT 2
""");
            });

    [ConditionalTheory(Skip = "Issue#17246 Non-embedded Include")]
    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql(" ");
    }

    [ConditionalTheory(Skip = "Issue#17246 Non-embedded Include")]
    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql(" ");
    }

    public override Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_kiwi_where_south_on_derived_property(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["FoundOn"] = 1))
""");
            });

    public override Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Can_use_of_type_kiwi_where_north_on_derived_property(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["FoundOn"] = 0))
""");
            });

    public override Task Discriminator_used_when_projection_over_derived_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Discriminator_used_when_projection_over_derived_type(a);

                AssertSql(
                    """
SELECT c["FoundOn"]
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
            });

    public override Task Discriminator_used_when_projection_over_derived_type2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Discriminator_used_when_projection_over_derived_type2(a);

                AssertSql(
                    """
SELECT c["IsFlightless"], c["Discriminator"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
            });

    public override Task Discriminator_with_cast_in_shadow_property(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Discriminator_with_cast_in_shadow_property(a);

                AssertSql(
                    """
SELECT VALUE {"Predator" : c["Name"]}
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND ("Kiwi" = c["Discriminator"]))
""");
            });

    public override Task Discriminator_used_when_projection_over_of_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Discriminator_used_when_projection_over_of_type(a);

                AssertSql(
                    """
SELECT c["FoundOn"]
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
            });

    [ConditionalFact(Skip = "Issue#17246 Transations not supported")]
    public override async Task Can_insert_update_delete()
    {
        await base.Can_insert_update_delete();

        AssertSql(" ");
    }

    public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
    {
        await base.Union_siblings_with_duplicate_property_in_subquery(async);

        AssertSql(" ");
    }

    public override async Task OfType_Union_subquery(bool async)
    {
        await base.OfType_Union_subquery(async);

        AssertSql(" ");
    }

    public override async Task OfType_Union_OfType(bool async)
    {
        await base.OfType_Union_OfType(async);

        AssertSql(" ");
    }

    public override Task Subquery_OfType(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Subquery_OfType(a);

                AssertSql(
                    """
@__p_0='5'

SELECT DISTINCT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
ORDER BY c["Species"]
OFFSET 0 LIMIT @__p_0
""");
            });

    public override async Task Union_entity_equality(bool async)
    {
        await base.Union_entity_equality(async);

        AssertSql(" ");
    }

    public override async Task Setting_foreign_key_to_a_different_type_throws()
    {
        await base.Setting_foreign_key_to_a_different_type_throws();

        AssertSql(
            """
SELECT c
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
OFFSET 0 LIMIT 2
""");
    }

    public override Task Byte_enum_value_constant_used_in_projection(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Byte_enum_value_constant_used_in_projection(a);

                AssertSql(
                    """
SELECT VALUE {"c" : (c["IsFlightless"] ? 0 : 1)}
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
""");
            });

    public override async Task Member_access_on_intermediate_type_works()
    {
        await base.Member_access_on_intermediate_type_works();

        AssertSql(
            """
SELECT c["Name"]
FROM root c
WHERE (c["Discriminator"] = "Kiwi")
ORDER BY c["Name"]
""");
    }

    [ConditionalTheory(Skip = "Issue#17246 subquery usage")]
    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql(" ");
    }

    public override Task Selecting_only_base_properties_on_base_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Selecting_only_base_properties_on_base_type(a);

                AssertSql(
                    """
SELECT c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
            });

    public override Task Selecting_only_base_properties_on_derived_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Selecting_only_base_properties_on_derived_type(a);

                AssertSql(
                    """
SELECT c["Name"]
FROM root c
WHERE c["Discriminator"] IN ("Eagle", "Kiwi")
""");
            });

    public override Task GetType_in_hierarchy_in_abstract_base_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_in_hierarchy_in_abstract_base_type(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND false)
""");
            });

    public override Task GetType_in_hierarchy_in_intermediate_type(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_in_hierarchy_in_intermediate_type(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND false)
""");
            });

    public override Task GetType_in_hierarchy_in_leaf_type_with_sibling(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_in_hierarchy_in_leaf_type_with_sibling(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Eagle"))
""");
            });

    public override Task GetType_in_hierarchy_in_leaf_type_with_sibling2(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_in_hierarchy_in_leaf_type_with_sibling2(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
            });

    public override Task GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_reverse(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi"))
""");
            });

    public override Task GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.GetType_in_hierarchy_in_leaf_type_with_sibling2_not_equal(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE (c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] != "Kiwi"))
""");
            });

    public override Task Using_is_operator_on_multiple_type_with_no_result(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Using_is_operator_on_multiple_type_with_no_result(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["Discriminator"] = "Eagle"))
""");
            });

    public override Task Using_is_operator_with_of_type_on_multiple_type_with_no_result(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Using_is_operator_with_of_type_on_multiple_type_with_no_result(a);

                AssertSql(
                    """
SELECT c
FROM root c
WHERE ((c["Discriminator"] IN ("Eagle", "Kiwi") AND (c["Discriminator"] = "Kiwi")) AND (c["Discriminator"] = "Eagle"))
""");
            });

    protected override bool EnforcesFkConstraints
        => false;

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
