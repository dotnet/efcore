// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
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

        public override Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
        {
            return AssertTranslationFailedWithDetails(() => base.Client_member_and_unsupported_string_Equals_in_the_same_query(async),
                CoreStrings.QueryUnableToTranslateMember(nameof(Gear.IsMarcus), nameof(Gear)));
        }

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

        [ConditionalTheory(Skip = "issue #17620")]
        public override Task Select_subquery_projecting_single_constant_inside_anonymous(bool async)
            => base.Select_subquery_projecting_single_constant_inside_anonymous(async);

        [ConditionalTheory(Skip = "issue #19683")]
        public override Task Group_by_on_StartsWith_with_null_parameter_as_argument(bool async)
            => base.Group_by_on_StartsWith_with_null_parameter_as_argument(async);

        [ConditionalTheory(Skip = "issue #17537")]
        public override Task SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(bool async)
            => base.SelectMany_predicate_with_non_equality_comparison_with_Take_doesnt_convert_to_join(async);

        [ConditionalTheory(Skip = "issue #19584")]
        public override Task Cast_to_derived_followed_by_include_and_FirstOrDefault(bool async)
            => base.Cast_to_derived_followed_by_include_and_FirstOrDefault(async);
    }
}
