// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsWeakQueryTestBase<TFixture> : ComplexNavigationsQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsWeakQueryFixtureBase, new()
    {
        protected ComplexNavigationsWeakQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "issue #8248")]
        public override Task Required_navigation_on_a_subquery_with_First_in_projection()
        {
            return base.Required_navigation_on_a_subquery_with_First_in_projection();
        }

        [ConditionalFact(Skip = "issue #8526")]
        public override Task Select_subquery_with_client_eval_and_navigation1()
        {
            return base.Select_subquery_with_client_eval_and_navigation1();
        }

        [ConditionalFact(Skip = "issue #8526")]
        public override Task Select_subquery_with_client_eval_and_navigation2()
        {
            return base.Select_subquery_with_client_eval_and_navigation2();
        }

        // Naked instances not supported
        public override Task Entity_equality_empty()
        {
            return Task.CompletedTask;
        }

        public override Task Key_equality_two_conditions_on_same_navigation()
        {
            return Task.CompletedTask;
        }

        public override Task Level4_Include()
        {
            // Due to level 4 being owned, other tests using l4 as root could cause same query as this one to run
            // generating different SQL
            return Task.CompletedTask;
        }

        #region #8172 - One-to-many not supported yet

        public override Task Multiple_SelectMany_with_string_based_Include()
        {
            return Task.CompletedTask;
        }

        public override Task Where_navigation_property_to_collection_of_original_entity_type()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_Include1()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_Include2()
        {
            return Task.CompletedTask;
        }

        public override Task Navigations_compared_to_each_other1()
        {
            return Task.CompletedTask;
        }

        public override Task Navigations_compared_to_each_other2()
        {
            return Task.CompletedTask;
        }

        public override Task Navigations_compared_to_each_other3()
        {
            return Task.CompletedTask;
        }

        public override Task Navigations_compared_to_each_other4()
        {
            return Task.CompletedTask;
        }

        public override Task Navigations_compared_to_each_other5()
        {
            return Task.CompletedTask;
        }

        public override Task Navigation_with_same_navigation_compared_to_null()
        {
            return Task.CompletedTask;
        }

        public override Task Multi_level_navigation_compared_to_null()
        {
            return Task.CompletedTask;
        }

        public override Task Multi_level_navigation_with_same_navigation_compared_to_null()
        {
            return Task.CompletedTask;
        }

        public override Task Multi_level_include_correct_PK_is_chosen_as_the_join_predicate_for_queries_that_join_same_table_multiple_times()
        {
            return Task.CompletedTask;
        }

        public override Task Required_navigation_with_Include_ThenInclude()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_nested_navigation_property_required()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_include_with_multiple_optional_navigations()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_SelectMany_calls()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_complex_includes()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_string_based_Include2()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty()
        {
            return Task.CompletedTask;
        }

        public override Task Contains_with_subquery_optional_navigation_and_constant_item()
        {
            return Task.CompletedTask;
        }

        public override Task Include_with_groupjoin_skip_and_take()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_navigation_property_and_projection()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_nested_navigation_property_optional_and_projection()
        {
            return Task.CompletedTask;
        }

        public override Task Multi_level_include_one_to_many_optional_and_one_to_many_optional_produces_valid_sql()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_navigation_property_with_another_navigation_in_subquery()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_string_based_Include1()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_navigation_property()
        {
            return Task.CompletedTask;
        }

        public override Task Complex_multi_include_with_order_by_and_paging()
        {
            return Task.CompletedTask;
        }

        public override Task Select_nav_prop_collection_one_to_many_required()
        {
            return Task.CompletedTask;
        }

        public override void Data_reader_is_closed_correct_number_of_times_for_include_queries_on_optional_navigations()
        {
        }

        public override Task SelectMany_where_with_subquery()
        {
            return Task.CompletedTask;
        }

        public override Task Required_navigation_with_Include()
        {
            return Task.CompletedTask;
        }

        public override Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key()
        {
            return Task.CompletedTask;
        }

        public override Task Complex_query_with_optional_navigations_and_client_side_evaluation()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_navigation_property_and_filter_before()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_complex_include_select()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_navigation_and_Distinct()
        {
            return Task.CompletedTask;
        }

        public override Task Optional_navigation_with_Include_ThenInclude()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_Include_ThenInclude()
        {
            return Task.CompletedTask;
        }

        public override Task Include_nested_with_optional_navigation()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_optional_navigation_with_Include()
        {
            return Task.CompletedTask;
        }

        public override Task Where_navigation_property_to_collection()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_SelectMany_with_Include()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_optional_navigation_with_string_based_Include()
        {
            return Task.CompletedTask;
        }

        public override Task Where_navigation_property_to_collection2()
        {
            return Task.CompletedTask;
        }

        public override Task Where_on_multilevel_reference_in_subquery_with_outer_projection()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_navigation_property_and_filter_after()
        {
            return Task.CompletedTask;
        }

        public override Task Complex_multi_include_with_order_by_and_paging_joins_on_correct_key2()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty()
        {
            return Task.CompletedTask;
        }

        public override Task Comparing_collection_navigation_on_optional_reference_to_null()
        {
            return Task.CompletedTask;
        }

        // Self-ref not supported
        public override Task Join_navigation_translated_to_subquery_self_ref()
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_complex_includes_self_ref()
        {
            return Task.CompletedTask;
        }

        public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties()
        {
            return Task.CompletedTask;
        }

        public override void Multi_level_include_reads_key_values_from_data_reader_rather_than_incorrect_reader_deep_into_the_stack()
        {
        }

        public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property()
        {
            return Task.CompletedTask;
        }

        public override Task Navigation_filter_navigation_grouping_ordering_by_group_key()
        {
            return Task.CompletedTask;
        }

        public override Task Manually_created_left_join_propagates_nullability_to_navigations()
        {
            return Task.CompletedTask;
        }

        public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join1()
        {
            return Task.CompletedTask;
        }

        public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join2()
        {
            return Task.CompletedTask;
        }

        public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened()
        {
            return Task.CompletedTask;
        }

        public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened2()
        {
            return Task.CompletedTask;
        }

        public override Task GroupJoin_with_complex_subquery_with_joins_does_not_get_flattened3()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_navigation()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_navigation_nested()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_navigation_using_ef_property()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_navigation_nested_anonymous()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_navigation_count()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_navigation_composed()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_and_root_entity()
        {
            return Task.CompletedTask;
        }

        public override Task Project_collection_and_include()
        {
            return Task.CompletedTask;
        }

        public override Task Project_navigation_and_collection()
        {
            return Task.CompletedTask;
        }

        public override Task Select_optional_navigation_property_string_concat()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_multiple_orderbys_member()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_multiple_orderbys_property()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_multiple_orderbys_methodcall()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_multiple_orderbys_complex()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_multiple_orderbys_complex_repeated()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_groupby_in_subquery()
        {
            return Task.CompletedTask;
        }

        public override Task Multi_include_with_groupby_in_subquery()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_groupby_in_subquery_and_filter_before_groupby()
        {
            return Task.CompletedTask;
        }

        public override Task Include_collection_with_groupby_in_subquery_and_filter_after_groupby()
        {
            return Task.CompletedTask;
        }

        public override Task Include_reference_collection_order_by_reference_navigation()
        {
            return Task.CompletedTask;
        }

        public override Task Optional_navigation_with_order_by_and_Include()
        {
            return Task.CompletedTask;
        }

        public override Task Optional_navigation_with_Include_and_order()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_order_by_and_Include()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_with_Include_and_order_by()
        {
            return Task.CompletedTask;
        }

        public override Task Include_after_SelectMany_and_reference_navigation()
        {
            return Task.CompletedTask;
        }

        public override Task Include_after_multiple_SelectMany_and_reference_navigation()
        {
            return Task.CompletedTask;
        }

        public override Task Include_after_SelectMany_and_multiple_reference_navigations()
        {
            return Task.CompletedTask;
        }

        public override Task Include_after_SelectMany_and_reference_navigation_with_another_SelectMany_with_Distinct()
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_subquery_with_custom_projection()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
