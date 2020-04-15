// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQuerySqliteTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQuerySqliteFixture>
    {
        public ComplexNavigationsQuerySqliteTest(ComplexNavigationsQuerySqliteFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool async)
        {
            return base.SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(async);
        }

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task Project_collection_navigation_nested_with_take(bool async)
        {
            return base.Project_collection_navigation_nested_with_take(async);
        }

        [ConditionalTheory(Skip = "Issue #17230")]
        public override Task Include_inside_subquery(bool async)
        {
            return base.Include_inside_subquery(async);
        }

        // Sqlite does not support cross/outer apply
        public override Task SelectMany_with_outside_reference_to_joined_table_correctly_translated_to_apply(bool async) => null;
        public override Task Nested_SelectMany_correlated_with_join_table_correctly_translated_to_apply(bool async) => null;
        public override void Filtered_include_Skip_without_OrderBy() { }
        public override void Filtered_include_Take_without_OrderBy() { }
        public override Task Filtered_include_after_different_filtered_include_same_level(bool async) => null;
        public override Task Filtered_include_after_different_filtered_include_different_level(bool async) => null;
        public override Task Filtered_include_after_reference_navigation(bool async) => null;
        public override Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async) => null;
        public override Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async) => null;
        public override Task Filtered_include_basic_OrderBy_Take(bool async) => null;
        public override Task Filtered_include_basic_OrderBy_Skip(bool async) => null;
        public override Task Filtered_include_basic_OrderBy_Skip_Take(bool async) => null;
        public override void Filtered_include_context_accessed_inside_filter() { }
        public override void Filtered_include_context_accessed_inside_filter_correlated() { }
        public override Task Filtered_include_on_ThenInclude(bool async) => null;
        public override void Filtered_include_outer_parameter_used_inside_filter() { }
        public override void Filtered_include_variable_used_inside_filter() { }
        public override void Filtered_include_is_considered_loaded() { }
        public override Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async) => null;
        public override Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async) => null;
        public override Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async) => null;
        public override Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async) => null;
        public override Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async) => null;
        public override Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async) => null;
    }
}
