// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsWeakQueryTestBase<TFixture> : ComplexNavigationsQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsWeakQueryFixtureBase, new()
    {
        protected ComplexNavigationsWeakQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "issue #8248")]
        public override Task Required_navigation_on_a_subquery_with_First_in_projection(bool isAsync)
        {
            return base.Required_navigation_on_a_subquery_with_First_in_projection(isAsync);
        }

        [ConditionalTheory(Skip = "issue #8526")]
        public override Task Select_subquery_with_client_eval_and_navigation1(bool isAsync)
        {
            return base.Select_subquery_with_client_eval_and_navigation1(isAsync);
        }

        [ConditionalTheory(Skip = "issue #8526")]
        public override Task Select_subquery_with_client_eval_and_navigation2(bool isAsync)
        {
            return base.Select_subquery_with_client_eval_and_navigation2(isAsync);
        }

        // Naked instances not supported
        public override Task Entity_equality_empty(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Key_equality_two_conditions_on_same_navigation(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Level4_Include(bool isAsync)
        {
            // Due to level 4 being weak, other tests using l4 as root could cause same query as this one to run
            // generating different SQL
            return Task.CompletedTask;
        }

        //#12583
        public override Task Include_with_groupjoin_skip_and_take(bool isAsync)
        {
            return Task.CompletedTask;
        }

        // Self-ref not supported
        public override Task Join_navigation_self_ref(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Multiple_complex_includes_self_ref(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_multiple_properties(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Join_condition_optimizations_applied_correctly_when_anonymous_type_with_single_property(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Navigation_filter_navigation_grouping_ordering_by_group_key(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Include_after_multiple_SelectMany_and_reference_navigation(bool isAsync)
        {
            return Task.CompletedTask;
        }

        public override Task Include_after_SelectMany_and_multiple_reference_navigations(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "issue #13560")]
        public override Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool isAsync)
        {
            return base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(isAsync);
        }

        [ConditionalTheory(Skip = "issue #13560")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(
            bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(isAsync);
        }

        [ConditionalTheory(Skip = "issue #13560")]
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(
            bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(isAsync);
        }

        [ConditionalTheory(Skip = "issue #13560")]
        public override void SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany()
        {
            base.SelectMany_with_nested_navigations_and_additional_joins_outside_of_SelectMany();
        }

        [ConditionalTheory(Skip = "issue #13560")]
        public override Task
            Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool isAsync)
        {
            return base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(
                isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include8(bool isAsync)
        {
            return base.Include8(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include9(bool isAsync)
        {
            return base.Include9(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_complex(bool isAsync)
        {
            return base.Include_collection_with_multiple_orderbys_complex(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_complex_repeated(bool isAsync)
        {
            return base.Include_collection_with_multiple_orderbys_complex_repeated(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_member(bool isAsync)
        {
            return base.Include_collection_with_multiple_orderbys_member(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_methodcall(bool isAsync)
        {
            return base.Include_collection_with_multiple_orderbys_methodcall(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#16752")]
        public override Task Include_collection_with_multiple_orderbys_property(bool isAsync)
        {
            return base.Include_collection_with_multiple_orderbys_property(isAsync);
        }

        // Cannot create DbSet for Level2
        public override void Join_with_navigations_in_the_result_selector2()
        {
        }

        public override void Member_pushdown_chain_3_levels_deep()
        {
        }

        public override void Member_pushdown_with_collection_navigation_in_the_middle()
        {
        }
    }
}
