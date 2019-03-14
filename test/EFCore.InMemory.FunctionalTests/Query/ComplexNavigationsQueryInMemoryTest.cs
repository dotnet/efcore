// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQueryInMemoryTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryInMemoryFixture>
    {
        public ComplexNavigationsQueryInMemoryTest(ComplexNavigationsQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "issue #4311")]
        public override Task Nested_group_join_with_take(bool IsAsync)
        {
            return base.Nested_group_join_with_take(IsAsync);
        }

        [ConditionalTheory(Skip = "issue #9591")]
        public override Task Multi_include_with_groupby_in_subquery(bool IsAsync)
        {
            return base.Multi_include_with_groupby_in_subquery(IsAsync);
        }

        [ConditionalTheory(Skip = "issue #13561")]
        public override Task
            Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool isAsync)
        {
            return base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(
                isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool isAsync)
        {
            return base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Key_equality_using_property_method_nested(bool isAsync)
        {
            return base.Key_equality_using_property_method_nested(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Key_equality_using_property_method_required(bool isAsync)
        {
            return base.Key_equality_using_property_method_required(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Key_equality_when_sentinel_ef_property(bool isAsync)
        {
            return base.Key_equality_when_sentinel_ef_property(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Navigation_inside_method_call_translated_to_join(bool isAsync)
        {
            return base.Navigation_inside_method_call_translated_to_join(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Entity_equality_empty(bool isAsync)
        {
            return base.Entity_equality_empty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_inside_property_method_translated_to_join(bool isAsync)
        {
            return base.Optional_navigation_inside_property_method_translated_to_join(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Navigation_key_access_optional_comparison(bool isAsync)
        {
            return base.Navigation_key_access_optional_comparison(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_inside_method_call_translated_to_join(bool isAsync)
        {
            return base.Optional_navigation_inside_method_call_translated_to_join(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(bool isAsync)
        {
            return base.Optional_navigation_inside_method_call_translated_to_join_keeps_original_nullability(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Key_equality_two_conditions_on_same_navigation(bool isAsync)
        {
            return base.Key_equality_two_conditions_on_same_navigation(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(bool isAsync)
        {
            return base.Method_call_on_optional_navigation_translates_to_null_conditional_properly_for_arguments(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join1(bool isAsync)
        {
            return base.Optional_navigation_propagates_nullability_to_manually_created_left_join1(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Key_equality_using_property_method_and_member_expression1(bool isAsync)
        {
            return base.Key_equality_using_property_method_and_member_expression1(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Key_equality_using_property_method_and_member_expression2(bool isAsync)
        {
            return base.Key_equality_using_property_method_and_member_expression2(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Complex_navigations_with_predicate_projected_into_anonymous_type(bool isAsync)
        {
            return base.Complex_navigations_with_predicate_projected_into_anonymous_type(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_inside_nested_method_call_translated_to_join(bool isAsync)
        {
            return base.Optional_navigation_inside_nested_method_call_translated_to_join(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Join_navigation_in_outer_selector_translated_to_extra_join_nested2(bool isAsync)
        {
            return base.Join_navigation_in_outer_selector_translated_to_extra_join_nested2(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Join_navigation_in_outer_selector_translated_to_extra_join(bool isAsync)
        {
            return base.Join_navigation_in_outer_selector_translated_to_extra_join(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Join_navigation_in_outer_selector_translated_to_extra_join_nested(bool isAsync)
        {
            return base.Join_navigation_in_outer_selector_translated_to_extra_join_nested(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(bool isAsync)
        {
            return base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(bool isAsync)
        {
            return base.Optional_navigation_inside_nested_method_call_translated_to_join_keeps_original_nullability_also_for_arguments(isAsync);
        }
    }
}
