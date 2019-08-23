// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices.WindowsRuntime;
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

        // InMemory can throw server side exception
        public override void Average_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Average_no_data_subquery());
        }

        public override void Max_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Max_no_data_subquery());
        }

        public override void Min_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Min_no_data_subquery());
        }

        public override Task Where_query_composition_entity_equality_one_element_Single(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_one_element_Single(isAsync));
        }

        public override Task Where_query_composition_entity_equality_one_element_First(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_one_element_First(isAsync));
        }

        public override Task Where_query_composition_entity_equality_no_elements_Single(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_no_elements_Single(isAsync));
        }

        public override Task Where_query_composition_entity_equality_no_elements_First(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_no_elements_First(isAsync));
        }

        public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(isAsync));
        }

        public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_multiple_elements_Single(isAsync));
        }

        // Sending client code to server
        [ConditionalFact(Skip = "Issue#17050")]
        public override void Client_code_using_instance_in_anonymous_type()
        {
            base.Client_code_using_instance_in_anonymous_type();
        }

        [ConditionalFact(Skip = "Issue#17050")]
        public override void Client_code_using_instance_in_static_method()
        {
            base.Client_code_using_instance_in_static_method();
        }

        [ConditionalFact(Skip = "Issue#17050")]
        public override void Client_code_using_instance_method_throws()
        {
            base.Client_code_using_instance_method_throws();
        }

        #region Set Operations
        public override Task Concat(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Concat_nested(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Concat_non_entity(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Except(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Except_nested(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Except_non_entity(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Except_simple_followed_by_projecting_constant(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Intersect(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Intersect_nested(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Intersect_non_entity(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_Include(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_Intersect(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_nested(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override void Union_non_entity(bool isAsync)
        {
        }

        public override Task Union_OrderBy_Skip_Take(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_over_different_projection_types(bool isAsync, string leftType, string rightType)
        {
            return Task.CompletedTask;
        }

        public override Task Union_Select(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_Skip_Take_OrderBy_ThenBy_Where(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_Take_Union_Take(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_Union(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_Where(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Union_with_anonymous_type_projection(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Include_Union(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Client_eval_Union_FirstOrDefault(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task GroupBy_Select_Union(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override void Include_Union_different_includes_throws()
        {
        }

        public override void Include_Union_only_on_one_side_throws()
        {
        }

        public override Task Select_Union(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Select_Union_different_fields_in_anonymous_with_subquery(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Select_Union_unrelated(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SubSelect_Union(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Select_Except_reference_projection(bool isAsync)
        {
            return Task.CompletedTask;
        }

        #endregion

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Contains_with_local_tuple_array_closure(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Last_when_no_order_by(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task OrderBy_multiple_queries(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_1()
        {
            base.Random_next_is_not_funcletized_1();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_2()
        {
            base.Random_next_is_not_funcletized_2();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_3()
        {
            base.Random_next_is_not_funcletized_3();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_4()
        {
            base.Random_next_is_not_funcletized_4();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_5()
        {
            base.Random_next_is_not_funcletized_5();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_6()
        {
            base.Random_next_is_not_funcletized_6();
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Where_bool_client_side_negated(bool isAsync)
        {
            return base.Where_bool_client_side_negated(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Projection_when_arithmetic_mixed_subqueries(bool isAsync)
        {
            return base.Projection_when_arithmetic_mixed_subqueries(isAsync);
        }

        #region DefaultIfEmpty

        public override Task DefaultIfEmpty_in_subquery(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task DefaultIfEmpty_in_subquery_nested(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task DefaultIfEmpty_in_subquery_not_correlated(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override void DefaultIfEmpty_without_group_join()
        {
        }

        public override Task Default_if_empty_top_level(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Default_if_empty_top_level_followed_by_projecting_constant(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Default_if_empty_top_level_positive(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Default_if_empty_top_level_projection(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Join_with_default_if_empty_on_both_sources(bool isAsync)
        {
            return Task.CompletedTask;
        }
        #endregion

        #region SelectMany

        public override Task Multiple_select_many_with_predicate(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_Joined(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_Joined_DefaultIfEmpty(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_Joined_DefaultIfEmpty2(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_Joined_Take(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_correlated_subquery_simple(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_correlated_with_outer_1(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_correlated_with_outer_2(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_correlated_with_outer_3(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_correlated_with_outer_4(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_without_result_selector_collection_navigation_composed(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task SelectMany_without_result_selector_naked_collection_navigation(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Select_DTO_with_member_init_distinct_in_subquery_translated_to_server_2(bool isAsync)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region NullableError

        public override Task Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault_followed_by_projecting_length(bool isAsync)
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
