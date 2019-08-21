// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
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

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(bool isAsync)
        {
            return base.Complex_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_with_other_query_operators_composed_on_top(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.Multiple_SelectMany_with_navigation_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(bool isAsync)
        {
            return base.Multiple_SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_joined_together(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_navigation_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_navigation_filter_paging_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_same_navs(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany2(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany3(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_explicit_DefaultIfEmpty_and_additional_joins_outside_of_SelectMany4(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_nested_navigation_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //DefaultIfEmpty
        public override Task SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_nested_required_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //GroupBy
        public override Task Simple_level1_level2_GroupBy_Count(bool isAsync)
        {
            return base.Simple_level1_level2_GroupBy_Count(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] //GroupBy
        public override Task Simple_level1_level2_GroupBy_Having_Count(bool isAsync)
        {
            return base.Simple_level1_level2_GroupBy_Having_Count(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool isAsync)
        {
            return base.Complex_query_with_optional_navigations_and_client_side_evaluation(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task Project_collection_navigation_nested(bool isAsync)
        {
            return base.Project_collection_navigation_nested(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task Project_collection_navigation_nested_anonymous(bool isAsync)
        {
            return base.Project_collection_navigation_nested_anonymous(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task Project_collection_navigation_using_ef_property(bool isAsync)
        {
            return base.Project_collection_navigation_using_ef_property(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task Project_navigation_and_collection(bool isAsync)
        {
            return base.Project_navigation_and_collection(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task SelectMany_nested_navigation_property_optional_and_projection(bool isAsync)
        {
            return base.SelectMany_nested_navigation_property_optional_and_projection(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task SelectMany_nested_navigation_property_required(bool isAsync)
        {
            return base.SelectMany_nested_navigation_property_required(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17460")]
        public override Task Where_complex_predicate_with_with_nav_prop_and_OrElse4(bool isAsync)
        {
            return base.Where_complex_predicate_with_with_nav_prop_and_OrElse4(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17460")]
        public override Task Join_flattening_bug_4539(bool isAsync)
        {
            return base.Join_flattening_bug_4539(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17463")]
        public override Task Include18_3_3(bool isAsync)
        {
            return base.Include18_3_3(isAsync);
        }

        [ConditionalFact(Skip = "issue #17463")]
        public override void Include19()
        {
            base.Include19();
        }
    }
}
