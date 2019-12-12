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
        public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool async)
        {
            return base.Complex_query_with_optional_navigations_and_client_side_evaluation(async);
        }

        [ConditionalTheory(Skip = "Issue#17539")]
        public override Task Join_navigations_in_inner_selector_translated_without_collision(bool async)
        {
            return base.Join_navigations_in_inner_selector_translated_without_collision(async);
        }

        [ConditionalTheory(Skip = "Issue#17539")]
        public override Task Join_with_navigations_in_the_result_selector1(bool async)
        {
            return base.Join_with_navigations_in_the_result_selector1(async);
        }

        [ConditionalTheory(Skip = "Issue#17539")]
        public override Task Where_nav_prop_reference_optional1_via_DefaultIfEmpty(bool async)
        {
            return base.Where_nav_prop_reference_optional1_via_DefaultIfEmpty(async);
        }

        [ConditionalTheory(Skip = "Issue#17539")]
        public override Task Optional_navigation_propagates_nullability_to_manually_created_left_join2(bool async)
        {
            return base.Optional_navigation_propagates_nullability_to_manually_created_left_join2(async);
        }

        [ConditionalTheory(Skip = "issue #17620")]
        public override Task Lift_projection_mapping_when_pushing_down_subquery(bool async)
        {
            return base.Lift_projection_mapping_when_pushing_down_subquery(async);
        }

        [ConditionalTheory(Skip = "issue #18912")]
        public override Task OrderBy_collection_count_ThenBy_reference_navigation(bool async)
        {
            return base.OrderBy_collection_count_ThenBy_reference_navigation(async);
        }
    }
}
