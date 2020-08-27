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

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool async)
            => base.Correlated_collection_order_by_constant_null_of_non_mapped_type(async);

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Client_side_equality_with_parameter_works_with_optional_navigations(bool async)
            => base.Client_side_equality_with_parameter_works_with_optional_navigations(async);

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Where_coalesce_with_anonymous_types(bool async)
            => base.Where_coalesce_with_anonymous_types(async);

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task GetValueOrDefault_on_DateTimeOffset(bool async)
            => base.GetValueOrDefault_on_DateTimeOffset(async);

        [ConditionalFact(Skip = "issue #17537")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
            => base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();

        [ConditionalFact(Skip = "issue #17537")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
            => base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();

        [ConditionalTheory(Skip = "issue #17540")]
        public override Task
            Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool async)
            => base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(
                async);

        [ConditionalTheory(Skip = "issue #18284")]
        public override Task GroupBy_with_boolean_groupin_key_thru_navigation_access(bool async)
            => GroupBy_with_boolean_groupin_key_thru_navigation_access(async);

        [ConditionalTheory(Skip = "issue #17620")]
        public override Task Select_subquery_projecting_single_constant_inside_anonymous(bool async)
            => base.Select_subquery_projecting_single_constant_inside_anonymous(async);

        [ConditionalTheory(Skip = "issue #19683")]
        public override Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
            => base.Group_by_on_StartsWith_with_null_parameter_as_argument(async);

        [ConditionalTheory(Skip = "issue #18284")]
        public override Task Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(bool async)
            => base.Enum_closure_typed_as_underlying_type_generates_correct_parameter_type(async);

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
            => base.Client_member_and_unsupported_string_Equals_in_the_same_query(async);

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Client_eval_followed_by_set_operation_throws_meaningful_exception(bool async)
            => base.Client_eval_followed_by_set_operation_throws_meaningful_exception(async);

        [ConditionalTheory(Skip = "issue #17537")]
        public override Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
            => base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async);
    }
}
