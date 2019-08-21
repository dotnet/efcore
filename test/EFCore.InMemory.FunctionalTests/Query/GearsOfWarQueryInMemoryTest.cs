// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryInMemoryTest : GearsOfWarQueryTestBase<GearsOfWarQueryInMemoryFixture>
    {
        public GearsOfWarQueryInMemoryTest(GearsOfWarQueryInMemoryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        [ConditionalTheory(Skip = "issue #16963")] // groupby
        public override Task GroupBy_Property_Include_Aggregate_with_anonymous_selector(bool isAsync)
        {
            return base.GroupBy_Property_Include_Aggregate_with_anonymous_selector(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] // groupby
        public override Task GroupBy_Property_Include_Select_Count(bool isAsync)
        {
            return base.GroupBy_Property_Include_Select_Count(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] // groupby
        public override Task GroupBy_Property_Include_Select_LongCount(bool isAsync)
        {
            return base.GroupBy_Property_Include_Select_LongCount(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] // groupby
        public override Task GroupBy_Property_Include_Select_Max(bool isAsync)
        {
            return base.GroupBy_Property_Include_Select_Max(isAsync);
        }

        [ConditionalTheory(Skip = "issue #16963")] // groupby
        public override Task GroupBy_Property_Include_Select_Min(bool isAsync)
        {
            return base.GroupBy_Property_Include_Select_Min(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool isAsync)
        {
            return base.Correlated_collection_order_by_constant_null_of_non_mapped_type(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Client_side_equality_with_parameter_works_with_optional_navigations(bool isAsync)
        {
            return base.Client_side_equality_with_parameter_works_with_optional_navigations(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Where_coalesce_with_anonymous_types(bool isAsync)
        {
            return base.Where_coalesce_with_anonymous_types(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Where_conditional_with_anonymous_type(bool isAsync)
        {
            return base.Where_conditional_with_anonymous_type(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task GetValueOrDefault_on_DateTimeOffset(bool isAsync)
        {
            return base.GetValueOrDefault_on_DateTimeOffset(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task Correlated_collection_with_complex_OrderBy(bool isAsync)
        {
            return base.Correlated_collection_with_complex_OrderBy(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17453")]
        public override Task Correlated_collection_with_very_complex_order_by(bool isAsync)
        {
            return base.Correlated_collection_with_very_complex_order_by(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17463")]
        public override Task Include_collection_OrderBy_aggregate(bool isAsync)
        {
            return base.Include_collection_OrderBy_aggregate(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17463")]
        public override Task Include_collection_with_complex_OrderBy3(bool isAsync)
        {
            return base.Include_collection_with_complex_OrderBy3(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17463")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();
        }

        [ConditionalTheory(Skip = "issue #17463")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();
        }

        [ConditionalTheory(Skip = "issue #16963")] //length
        public override Task Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool isAsync)
        {
            return base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(isAsync);
        }
    }
}
