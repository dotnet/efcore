// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NorthwindBulkUpdatesInMemoryTest(
    NorthwindBulkUpdatesInMemoryFixture<NoopModelCustomizer> fixture)
    : NorthwindBulkUpdatesTestBase<NorthwindBulkUpdatesInMemoryFixture<NoopModelCustomizer>>(fixture)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override Task Delete_non_entity_projection(bool async)
        => AssertTranslationFailed(() => base.Delete_non_entity_projection(async));

    public override Task Delete_non_entity_projection_2(bool async)
        => AssertTranslationFailed(() => base.Delete_non_entity_projection_2(async));

    public override Task Delete_non_entity_projection_3(bool async)
        => AssertTranslationFailed(() => base.Delete_non_entity_projection_3(async));

    public override Task Update_without_property_to_set_throws(bool async)
        => AssertTranslationFailed(() => base.Update_without_property_to_set_throws(async));

    public override Task Update_with_invalid_lambda_throws(bool async)
        => AssertTranslationFailed(() => base.Update_with_invalid_lambda_throws(async));

    public override Task Update_with_invalid_lambda_in_set_property_throws(bool async)
        => AssertTranslationFailed(() => base.Update_with_invalid_lambda_in_set_property_throws(async));

    public override Task Update_multiple_tables_throws(bool async)
        => AssertTranslationFailed(() => base.Update_multiple_tables_throws(async));

    public override Task Update_unmapped_property_throws(bool async)
        => AssertTranslationFailed(() => base.Update_unmapped_property_throws(async));

    public override Task Delete_Where_TagWith(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_TagWith(async));

    public override Task Delete_Where(bool async)
        => AssertTranslationFailed(() => base.Delete_Where(async));

    public override Task Delete_Where_parameter(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_parameter(async));

    public override Task Delete_Where_OrderBy(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_OrderBy(async));

    public override Task Delete_Where_OrderBy_Skip(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_OrderBy_Skip(async));

    public override Task Delete_Where_OrderBy_Take(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_OrderBy_Take(async));

    public override Task Delete_Where_OrderBy_Skip_Take(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_OrderBy_Skip_Take(async));

    public override Task Delete_Where_Skip(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_Skip(async));

    public override Task Delete_Where_Take(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_Take(async));

    public override Task Delete_Where_Skip_Take(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_Skip_Take(async));

    public override Task Delete_Where_predicate_with_GroupBy_aggregate(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_predicate_with_GroupBy_aggregate(async));

    public override Task Delete_Where_predicate_with_GroupBy_aggregate_2(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_predicate_with_GroupBy_aggregate_2(async));

    public override Task Delete_GroupBy_Where_Select(bool async)
        => AssertTranslationFailed(() => base.Delete_GroupBy_Where_Select(async));

    public override Task Delete_GroupBy_Where_Select_2(bool async)
        => AssertTranslationFailed(() => base.Delete_GroupBy_Where_Select_2(async));

    public override Task Delete_Where_Skip_Take_Skip_Take_causing_subquery(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_Skip_Take_Skip_Take_causing_subquery(async));

    public override Task Delete_Where_Distinct(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_Distinct(async));

    public override Task Delete_SelectMany(bool async)
        => AssertTranslationFailed(() => base.Delete_SelectMany(async));

    public override Task Delete_SelectMany_subquery(bool async)
        => AssertTranslationFailed(() => base.Delete_SelectMany_subquery(async));

    public override Task Delete_Where_using_navigation(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_using_navigation(async));

    public override Task Delete_Where_using_navigation_2(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_using_navigation_2(async));

    public override Task Delete_Union(bool async)
        => AssertTranslationFailed(() => base.Delete_Union(async));

    public override Task Delete_Concat(bool async)
        => AssertTranslationFailed(() => base.Delete_Concat(async));

    public override Task Delete_Intersect(bool async)
        => AssertTranslationFailed(() => base.Delete_Intersect(async));

    public override Task Delete_Except(bool async)
        => AssertTranslationFailed(() => base.Delete_Except(async));

    public override Task Delete_Where_optional_navigation_predicate(bool async)
        => AssertTranslationFailed(() => base.Delete_Where_optional_navigation_predicate(async));

    public override Task Delete_with_join(bool async)
        => AssertTranslationFailed(() => base.Delete_with_join(async));

    public override Task Delete_with_left_join(bool async)
        => AssertTranslationFailed(() => base.Delete_with_left_join(async));

    public override Task Delete_with_cross_join(bool async)
        => AssertTranslationFailed(() => base.Delete_with_cross_join(async));

    public override Task Delete_with_cross_apply(bool async)
        => AssertTranslationFailed(() => base.Delete_with_cross_apply(async));

    public override Task Delete_with_outer_apply(bool async)
        => AssertTranslationFailed(() => base.Delete_with_outer_apply(async));

    public override Task Update_Where_set_constant_TagWith(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_constant_TagWith(async));

    public override Task Update_Where_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_constant(async));

    public override Task Update_Where_parameter_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_parameter_set_constant(async));

    public override Task Update_Where_set_parameter(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_parameter(async));

    public override Task Update_Where_set_parameter_from_closure_array(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_parameter_from_closure_array(async));

    public override Task Update_Where_set_parameter_from_inline_list(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_parameter_from_inline_list(async));

    public override Task Update_Where_set_parameter_from_multilevel_property_access(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_parameter_from_multilevel_property_access(async));

    public override Task Update_Where_Skip_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_Skip_set_constant(async));

    public override Task Update_Where_Take_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_Take_set_constant(async));

    public override Task Update_Where_Skip_Take_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_Skip_Take_set_constant(async));

    public override Task Update_Where_OrderBy_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_OrderBy_set_constant(async));

    public override Task Update_Where_OrderBy_Skip_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_OrderBy_Skip_set_constant(async));

    public override Task Update_Where_OrderBy_Take_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_OrderBy_Take_set_constant(async));

    public override Task Update_Where_OrderBy_Skip_Take_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_OrderBy_Skip_Take_set_constant(async));

    public override Task Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(async));

    public override Task Update_Where_GroupBy_aggregate_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_GroupBy_aggregate_set_constant(async));

    public override Task Update_Where_GroupBy_First_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_GroupBy_First_set_constant(async));

    public override Task Update_Where_GroupBy_First_set_constant_2(bool async)
        => AssertTranslationFailed(() => base.Update_Where_GroupBy_First_set_constant_2(async));

    public override Task Update_Where_GroupBy_First_set_constant_3(bool async)
        // Translation of EF.Property<string> fails before getting to ExecuteUpdate.
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Update_Where_GroupBy_First_set_constant_3(async));

    public override Task Update_Where_Distinct_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_Distinct_set_constant(async));

    public override Task Update_Where_using_navigation_set_null(bool async)
        => AssertTranslationFailed(() => base.Update_Where_using_navigation_set_null(async));

    public override Task Update_Where_using_navigation_2_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_using_navigation_2_set_constant(async));

    public override Task Update_Where_SelectMany_set_null(bool async)
        => AssertTranslationFailed(() => base.Update_Where_SelectMany_set_null(async));

    public override Task Update_Where_set_property_plus_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_property_plus_constant(async));

    public override Task Update_Where_set_property_plus_parameter(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_property_plus_parameter(async));

    public override Task Update_Where_set_property_plus_property(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_property_plus_property(async));

    public override Task Update_Where_set_constant_using_ef_property(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_constant_using_ef_property(async));

    public override Task Update_Where_set_null(bool async)
        => AssertTranslationFailed(() => base.Update_Where_set_null(async));

    public override Task Update_Where_multiple_set(bool async)
        => AssertTranslationFailed(() => base.Update_Where_multiple_set(async));

    public override Task Update_Union_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Union_set_constant(async));

    public override Task Update_Concat_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Concat_set_constant(async));

    public override Task Update_Except_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Except_set_constant(async));

    public override Task Update_Intersect_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_Intersect_set_constant(async));

    public override Task Update_with_join_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_join_set_constant(async));

    public override Task Update_with_left_join_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_left_join_set_constant(async));

    public override Task Update_with_cross_join_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_cross_join_set_constant(async));

    public override Task Update_with_cross_apply_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_cross_apply_set_constant(async));

    public override Task Update_with_outer_apply_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_outer_apply_set_constant(async));

    public override Task Update_with_cross_join_left_join_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_cross_join_left_join_set_constant(async));

    public override Task Update_with_cross_join_cross_apply_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_cross_join_cross_apply_set_constant(async));

    public override Task Update_with_cross_join_outer_apply_set_constant(bool async)
        => AssertTranslationFailed(() => base.Update_with_cross_join_outer_apply_set_constant(async));

    public override Task Update_Where_SelectMany_subquery_set_null(bool async)
        => AssertTranslationFailed(() => base.Update_Where_SelectMany_subquery_set_null(async));

    public override Task Update_Where_Join_set_property_from_joined_single_result_table(bool async)
        => AssertTranslationFailed(() => base.Update_Where_Join_set_property_from_joined_single_result_table(async));

    public override Task Update_Where_Join_set_property_from_joined_table(bool async)
        => AssertTranslationFailed(() => base.Update_Where_Join_set_property_from_joined_table(async));

    public override Task Update_Where_Join_set_property_from_joined_single_result_scalar(bool async)
        => AssertTranslationFailed(() => base.Update_Where_Join_set_property_from_joined_single_result_scalar(async));

    public override Task Update_with_two_inner_joins(bool async)
        => AssertTranslationFailed(() => base.Update_with_two_inner_joins(async));

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);
}
