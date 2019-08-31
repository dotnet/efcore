// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsWeakQueryInMemoryTest :
        ComplexNavigationsWeakQueryTestBase<ComplexNavigationsWeakQueryInMemoryFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public ComplexNavigationsWeakQueryInMemoryTest(
            ComplexNavigationsWeakQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool isAsync)
        {
            return base.Complex_query_with_optional_navigations_and_client_side_evaluation(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17531")]
        public override Task SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(bool isAsync)
        {
            return base.SelectMany_with_nested_navigations_and_explicit_DefaultIfEmpty_followed_by_Select_required_navigation_using_different_navs(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17531")]
        public override Task SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(bool isAsync)
        {
            return base.SelectMany_with_nested_navigation_filter_and_explicit_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17531")]
        public override Task Project_collection_navigation_nested(bool isAsync)
        {
            return base.Project_collection_navigation_nested(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17531")]
        public override Task Project_collection_navigation_nested_anonymous(bool isAsync)
        {
            return base.Project_collection_navigation_nested_anonymous(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17531")]
        public override Task Project_collection_navigation_using_ef_property(bool isAsync)
        {
            return base.Project_collection_navigation_using_ef_property(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17531")]
        public override Task Project_navigation_and_collection(bool isAsync)
        {
            return base.Project_navigation_and_collection(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17531")]
        public override Task SelectMany_nested_navigation_property_optional_and_projection(bool isAsync)
        {
            return base.SelectMany_nested_navigation_property_optional_and_projection(isAsync);
        }

        [ConditionalTheory(Skip = "17539")]
        public override Task Join_navigations_in_inner_selector_translated_without_collision(bool isAsync)
        {
            return base.Join_navigations_in_inner_selector_translated_without_collision(isAsync);
        }

        [ConditionalTheory(Skip = "17539")]
        public override Task Join_with_navigations_in_the_result_selector1(bool isAsync)
        {
            return base.Join_with_navigations_in_the_result_selector1(isAsync);
        }

        [ConditionalTheory(Skip = "17539")]
        public override Task Where_nav_prop_reference_optional1_via_DefaultIfEmpty(bool isAsync)
        {
            return base.Where_nav_prop_reference_optional1_via_DefaultIfEmpty(isAsync);
        }

        [ConditionalTheory(Skip = "17539")]
        public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool isAsync)
        {
            return base.Optional_navigation_propagates_nullability_to_manually_created_left_join2(isAsync);
        }
    }
}
