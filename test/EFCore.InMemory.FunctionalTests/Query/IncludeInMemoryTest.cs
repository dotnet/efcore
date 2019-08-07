// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeInMemoryTest : IncludeTestBase<IncludeInMemoryFixture>
    {
        public IncludeInMemoryTest(IncludeInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_reference_and_collection_order_by(bool useString)
        {
            base.Include_reference_and_collection_order_by(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_references_then_include_collection(bool useString)
        {
            base.Include_references_then_include_collection(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Then_include_collection_order_by_collection_column(bool useString)
        {
            base.Then_include_collection_order_by_collection_column(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_when_result_operator(bool useString)
        {
            base.Include_when_result_operator(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection(bool useString)
        {
            base.Include_collection(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_then_reference(bool useString)
        {
            base.Include_collection_then_reference(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_with_last(bool useString)
        {
            base.Include_collection_with_last(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_with_last_no_orderby(bool useString)
        {
            base.Include_collection_with_last_no_orderby(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_skip_no_order_by(bool useString)
        {
            base.Include_collection_skip_no_order_by(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_take_no_order_by(bool useString)
        {
            base.Include_collection_take_no_order_by(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_skip_take_no_order_by(bool useString)
        {
            base.Include_collection_skip_take_no_order_by(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_alias_generation(bool useString)
        {
            base.Include_collection_alias_generation(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_and_reference(bool useString)
        {
            base.Include_collection_and_reference(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_as_no_tracking(bool useString)
        {
            base.Include_collection_as_no_tracking(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_as_no_tracking2(bool useString)
        {
            base.Include_collection_as_no_tracking2(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_dependent_already_tracked(bool useString)
        {
            base.Include_collection_dependent_already_tracked(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_dependent_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_additional_from_clause(bool useString)
        {
            base.Include_collection_on_additional_from_clause(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_additional_from_clause_no_tracking(bool useString)
        {
            base.Include_collection_on_additional_from_clause_no_tracking(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_additional_from_clause_with_filter(bool useString)
        {
            base.Include_collection_on_additional_from_clause_with_filter(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_additional_from_clause2(bool useString)
        {
            base.Include_collection_on_additional_from_clause2(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_filter(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_join_clause_with_order_by_and_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_group_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_group_join_clause_with_filter(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_on_inner_group_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_inner_group_join_clause_with_filter(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_when_groupby(bool useString)
        {
            base.Include_collection_when_groupby(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_when_groupby_subquery(bool useString)
        {
            base.Include_collection_when_groupby_subquery(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_order_by_collection_column(bool useString)
        {
            base.Include_collection_order_by_collection_column(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_order_by_key(bool useString)
        {
            base.Include_collection_order_by_key(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_order_by_non_key(bool useString)
        {
            base.Include_collection_order_by_non_key(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_order_by_non_key_with_skip(bool useString)
        {
            base.Include_collection_order_by_non_key_with_skip(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_order_by_non_key_with_first_or_default(bool useString)
        {
            base.Include_collection_order_by_non_key_with_first_or_default(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_order_by_subquery(bool useString)
        {
            base.Include_collection_order_by_subquery(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_principal_already_tracked(bool useString)
        {
            base.Include_collection_principal_already_tracked(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_principal_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_principal_already_tracked_as_no_tracking(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_single_or_default_no_result(bool useString)
        {
            base.Include_collection_single_or_default_no_result(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_when_projection(bool useString)
        {
            base.Include_collection_when_projection(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_with_filter(bool useString)
        {
            base.Include_collection_with_filter(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_with_filter_reordered(bool useString)
        {
            base.Include_collection_with_filter_reordered(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_duplicate_collection(bool useString)
        {
            base.Include_duplicate_collection(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_duplicate_collection_result_operator(bool useString)
        {
            base.Include_duplicate_collection_result_operator(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_duplicate_collection_result_operator2(bool useString)
        {
            base.Include_duplicate_collection_result_operator2(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_with_client_filter(bool useString)
        {
            base.Include_collection_with_client_filter(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_multi_level_reference_and_collection_predicate(bool useString)
        {
            base.Include_multi_level_reference_and_collection_predicate(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_multi_level_collection_and_then_include_reference_predicate(bool useString)
        {
            base.Include_multi_level_collection_and_then_include_reference_predicate(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_multiple_references_and_collection_multi_level(bool useString)
        {
            base.Include_multiple_references_and_collection_multi_level(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_multiple_references_and_collection_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_and_collection_multi_level_reverse(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_reference_and_collection(bool useString)
        {
            base.Include_reference_and_collection(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_force_alias_uniquefication(bool useString)
        {
            base.Include_collection_force_alias_uniquefication(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_references_and_collection_multi_level(bool useString)
        {
            base.Include_references_and_collection_multi_level(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_then_include_collection(bool useString)
        {
            base.Include_collection_then_include_collection(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_then_include_collection_then_include_reference(bool useString)
        {
            base.Include_collection_then_include_collection_then_include_reference(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_then_include_collection_predicate(bool useString)
        {
            base.Include_collection_then_include_collection_predicate(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_references_and_collection_multi_level_predicate(bool useString)
        {
            base.Include_references_and_collection_multi_level_predicate(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_multi_level_reference_then_include_collection_predicate(bool useString)
        {
            base.Include_multi_level_reference_then_include_collection_predicate(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_multiple_references_then_include_collection_multi_level(bool useString)
        {
            base.Include_multiple_references_then_include_collection_multi_level(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_multiple_references_then_include_collection_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_then_include_collection_multi_level_reverse(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_references_then_include_collection_multi_level(bool useString)
        {
            base.Include_references_then_include_collection_multi_level(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_references_then_include_collection_multi_level_predicate(bool useString)
        {
            base.Include_references_then_include_collection_multi_level_predicate(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_with_conditional_order_by(bool useString)
        {
            base.Include_collection_with_conditional_order_by(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_GroupBy_Select(bool useString)
        {
            base.Include_collection_GroupBy_Select(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_Join_GroupBy_Select(bool useString)
        {
            base.Include_collection_Join_GroupBy_Select(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Join_Include_collection_GroupBy_Select(bool useString)
        {
            base.Join_Include_collection_GroupBy_Select(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_GroupJoin_GroupBy_Select(bool useString)
        {
            base.Include_collection_GroupJoin_GroupBy_Select(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void GroupJoin_Include_collection_GroupBy_Select(bool useString)
        {
            base.GroupJoin_Include_collection_GroupBy_Select(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_SelectMany_GroupBy_Select(bool useString)
        {
            base.Include_collection_SelectMany_GroupBy_Select(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void SelectMany_Include_collection_GroupBy_Select(bool useString)
        {
            base.SelectMany_Include_collection_GroupBy_Select(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_distinct_is_server_evaluated(bool useString)
        {
            base.Include_collection_distinct_is_server_evaluated(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_OrderBy_object(bool useString)
        {
            base.Include_collection_OrderBy_object(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_OrderBy_empty_list_contains(bool useString)
        {
            base.Include_collection_OrderBy_empty_list_contains(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_OrderBy_empty_list_does_not_contains(bool useString)
        {
            base.Include_collection_OrderBy_empty_list_does_not_contains(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_OrderBy_list_contains(bool useString)
        {
            base.Include_collection_OrderBy_list_contains(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_collection_OrderBy_list_does_not_contains(bool useString)
        {
            base.Include_collection_OrderBy_list_does_not_contains(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override Task Include_empty_collection_sets_IsLoaded(bool useString, bool async)
        {
            return base.Include_empty_collection_sets_IsLoaded(useString, async);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_closes_reader(bool useString)
        {
            base.Include_closes_reader(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_with_take(bool useString)
        {
            base.Include_with_take(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_with_skip(bool useString)
        {
            base.Include_with_skip(useString);
        }

        [ConditionalTheory(Skip = "issue #16963")]
        public override void Include_list(bool useString)
        {
            base.Include_list(useString);
        }
    }
}
