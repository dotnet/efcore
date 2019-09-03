// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQueryInMemoryTest : SimpleQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public SimpleQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalFact(Skip = "See issue#13857")]
        public override void Auto_initialized_view_set()
        {
        }

        [ConditionalTheory(Skip = "See issue#13857")]
        public override Task KeylessEntity_simple(bool isAsync) => null;

        [ConditionalTheory(Skip = "See issue#13857")]
        public override Task KeylessEntity_where_simple(bool isAsync) => null;

        [ConditionalFact(Skip = "See issue#13857")]
        public override void KeylessEntity_by_database_view()
        {
        }

        // InMemory can mimic throw behavior for subquery
        public override void Average_no_data_subquery()
        {
        }

        public override void Min_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Min_no_data_subquery());
        }

        public override void Max_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Max_no_data_subquery());
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Concat(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Concat_nested(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Concat_non_entity(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Except(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Except_nested(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Except_non_entity(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Except_simple_followed_by_projecting_constant(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Intersect(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Intersect_nested(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Intersect_non_entity(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_Intersect(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_Include(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_nested(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_OrderBy_Skip_Take(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_Select(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_Skip_Take_OrderBy_ThenBy_Where(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_Take_Union_Take(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_Union(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_Where(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_with_anonymous_type_projection(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Client_eval_Union_FirstOrDefault(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Include_Union(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_Union(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_Union_different_fields_in_anonymous_with_subquery(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_Union_unrelated(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SubSelect_Union(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_Except_reference_projection(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task GroupBy_Select_Union(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Union_over_different_projection_types(bool isAsync, string leftType, string rightType) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Include_with_orderby_skip_preserves_ordering(bool isAsync) => null;

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Select_nested_collection_multi_level()
        {
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task DefaultIfEmpty_in_subquery(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task DefaultIfEmpty_in_subquery_nested(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task DefaultIfEmpty_in_subquery_not_correlated(bool isAsync) => null;

        [ConditionalFact(Skip = "Issue #16963")]
        public override void DefaultIfEmpty_without_group_join()
        {
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Default_if_empty_top_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Default_if_empty_top_level_followed_by_projecting_constant(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Default_if_empty_top_level_positive(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Default_if_empty_top_level_projection(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Join_with_default_if_empty_on_both_sources(bool isAsync) => null;

        [ConditionalFact(Skip = "Issue #16963")]
        public override void OfType_Select()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void OfType_Select_OfType_Select()
        {
        }

        [ConditionalTheory(Skip = "Issue#16575")]
        public override Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(
            bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task KeylessEntity_with_included_nav(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task KeylessEntity_with_included_navs_multi_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue#17050")]
        public override void Client_code_using_instance_in_static_method()
        {
        }

        [ConditionalTheory(Skip = "Issue#17050")]
        public override void Client_code_using_instance_method_throws()
        {
        }

        [ConditionalTheory(Skip = "Issue#17050")]
        public override void Client_code_using_instance_in_anonymous_type()
        {
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Multiple_select_many_with_predicate(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_correlated_subquery_simple(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_Joined(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_Joined_DefaultIfEmpty(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_Joined_DefaultIfEmpty2(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_Joined_Take(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_without_result_selector_naked_collection_navigation(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_without_result_selector_collection_navigation_composed(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_correlated_with_outer_1(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_correlated_with_outer_2(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_correlated_with_outer_3(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Contains_with_local_tuple_array_closure(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_Count_with_predicate_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_Count_with_predicate_client_eval_mixed(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_Where_Count_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_Where_Count_client_eval_mixed(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_Where_Count_with_predicate_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_OrderBy_Count_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_correlated_with_outer_4(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task FirstOrDefault_over_empty_collection_of_value_type_returns_correct_results(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task All_client(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Client_OrderBy_GroupBy_Group_ordering_works(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task First_client_predicate(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_client_mixed(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task OrderBy_multiple_queries(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Projection_when_arithmetic_mixed_subqueries(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Queryable_reprojection(bool isAsync) => null;

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Random_next_is_not_funcletized_1()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Random_next_is_not_funcletized_2()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Random_next_is_not_funcletized_3()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Random_next_is_not_funcletized_4()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Random_next_is_not_funcletized_5()
        {
        }

        [ConditionalFact(Skip = "Issue #16963")]
        public override void Random_next_is_not_funcletized_6()
        {
        }

        [ConditionalTheory(Skip = "Issue#16963")]
        public override Task Projection_when_client_evald_subquery(bool isAsync)
        {
            return base.Projection_when_client_evald_subquery(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task SelectMany_after_client_method(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_bool_client_side_negated(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_client(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_client_and_server_non_top_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_client_and_server_top_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_client_deep_inside_predicate_and_server_top_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_client_or_server_top_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition3(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition4(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition5(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition6(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_subquery_correlated_client_eval(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task All_client_and_server_top_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task All_client_or_server_top_level(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool isAsync) => null;

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition_entity_equality_one_element_Single(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_one_element_Single(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition_entity_equality_one_element_First(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_one_element_First(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition_entity_equality_no_elements_Single(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_no_elements_Single(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition_entity_equality_no_elements_First(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_no_elements_First(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool isAsync)
        {
            return base.Where_query_composition_entity_equality_multiple_elements_Single(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Last_when_no_order_by(bool isAsync)
        {
            return base.Last_when_no_order_by(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #16963")]
        public override Task Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(bool isAsync)
        {
            return base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2(isAsync);
        }
    }
}
