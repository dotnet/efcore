// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public class TPCInheritanceQuerySqlServerTest : TPCInheritanceQueryTestBase<TPCInheritanceQuerySqlServerFixture>
{
    public TPCInheritanceQuerySqlServerTest(TPCInheritanceQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Byte_enum_value_constant_used_in_projection(bool async)
    {
        await base.Byte_enum_value_constant_used_in_projection(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_filter_all_animals(bool async)
    {
        await base.Can_filter_all_animals(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_include_animals(bool async)
    {
        await base.Can_include_animals(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_include_prey(bool async)
    {
        await base.Can_include_prey(async);

        AssertSql();
    }

    [ConditionalFact(Skip = "Issue#3170")]
    public override void Can_insert_update_delete()
    {
        base.Can_insert_update_delete();

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_query_all_animals(bool async)
    {
        await base.Can_query_all_animals(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_query_all_birds(bool async)
    {
        await base.Can_query_all_birds(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_query_all_plants(bool async)
    {
        await base.Can_query_all_plants(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_query_all_types_when_shared_column(bool async)
    {
        await base.Can_query_all_types_when_shared_column(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_query_just_kiwis(bool async)
    {
        await base.Can_query_just_kiwis(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_query_just_roses(bool async)
    {
        await base.Can_query_just_roses(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_query_when_shared_column(bool async)
    {
        await base.Can_query_when_shared_column(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_backwards_is_animal(bool async)
    {
        await base.Can_use_backwards_is_animal(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_backwards_of_type_animal(bool async)
    {
        await base.Can_use_backwards_of_type_animal(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_is_kiwi(bool async)
    {
        await base.Can_use_is_kiwi(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_is_kiwi_with_cast(bool async)
    {
        await base.Can_use_is_kiwi_with_cast(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_is_kiwi_in_projection(bool async)
    {
        await base.Can_use_is_kiwi_in_projection(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_is_kiwi_with_other_predicate(bool async)
    {
        await base.Can_use_is_kiwi_with_other_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_animal(bool async)
    {
        await base.Can_use_of_type_animal(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_bird(bool async)
    {
        await base.Can_use_of_type_bird(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_bird_first(bool async)
    {
        await base.Can_use_of_type_bird_first(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_bird_predicate(bool async)
    {
        await base.Can_use_of_type_bird_predicate(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_bird_with_projection(bool async)
    {
        await base.Can_use_of_type_bird_with_projection(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_kiwi(bool async)
    {
        await base.Can_use_of_type_kiwi(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_kiwi_where_north_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_north_on_derived_property(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_kiwi_where_south_on_derived_property(bool async)
    {
        await base.Can_use_of_type_kiwi_where_south_on_derived_property(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Can_use_of_type_rose(bool async)
    {
        await base.Can_use_of_type_rose(async);

        AssertSql();
    }

    [ConditionalFact(Skip = "Issue#3170")]
    public override void Member_access_on_intermediate_type_works()
    {
        base.Member_access_on_intermediate_type_works();

        AssertSql();
    }

    public override async Task OfType_Union_OfType(bool async)
    {
        await base.OfType_Union_OfType(async);

        AssertSql();
    }

    public override async Task OfType_Union_subquery(bool async)
    {
        await base.OfType_Union_subquery(async);

        AssertSql();
    }

    [ConditionalFact(Skip = "Issue#3170")]
    public override void Setting_foreign_key_to_a_different_type_throws()
    {
        base.Setting_foreign_key_to_a_different_type_throws();

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Subquery_OfType(bool async)
    {
        await base.Subquery_OfType(async);

        AssertSql();
    }

    public override async Task Union_entity_equality(bool async)
    {
        await base.Union_entity_equality(async);

        AssertSql();
    }

    public override async Task Union_siblings_with_duplicate_property_in_subquery(bool async)
    {
        await base.Union_siblings_with_duplicate_property_in_subquery(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Selecting_only_base_properties_on_base_type(bool async)
    {
        await base.Selecting_only_base_properties_on_base_type(async);

        AssertSql();
    }

    [ConditionalTheory(Skip = "Issue#3170")]
    public override async Task Selecting_only_base_properties_on_derived_type(bool async)
    {
        await base.Selecting_only_base_properties_on_derived_type(async);

        AssertSql();
    }

    public override async Task Can_query_all_animal_views(bool async)
    {
        await base.Can_query_all_animal_views(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_derived_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_derived_type2(bool async)
    {
        await base.Discriminator_used_when_projection_over_derived_type2(async);

        AssertSql();
    }

    public override async Task Discriminator_used_when_projection_over_of_type(bool async)
    {
        await base.Discriminator_used_when_projection_over_of_type(async);

        AssertSql();
    }

    public override async Task Discriminator_with_cast_in_shadow_property(bool async)
    {
        await base.Discriminator_with_cast_in_shadow_property(async);

        AssertSql();
    }

    public override void Using_from_sql_throws()
    {
        base.Using_from_sql_throws();

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
