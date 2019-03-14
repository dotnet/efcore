// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
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

        [ConditionalTheory(Skip = "issue #12295")]
        public override Task Double_order_by_on_nullable_bool_coming_from_optional_navigation(bool isAsync)
        {
            return base.Double_order_by_on_nullable_bool_coming_from_optional_navigation(isAsync);
        }

        [ConditionalTheory(Skip = "issue #13746")]
        public override Task Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(bool isAsync)
        {
            return base.Null_semantics_is_correctly_applied_for_function_comparisons_that_take_arguments_from_optional_navigation_complex(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Negated_bool_ternary_inside_anonymous_type_in_projection(bool isAsync)
        {
            return base.Negated_bool_ternary_inside_anonymous_type_in_projection(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_type_compensation_works_with_binary_expression(bool isAsync)
        {
            return base.Optional_navigation_type_compensation_works_with_binary_expression(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Complex_predicate_with_AndAlso_and_nullable_bool_property(bool isAsync)
        {
            return base.Complex_predicate_with_AndAlso_and_nullable_bool_property(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_type_compensation_works_with_conditional_expression(bool isAsync)
        {
            return base.Optional_navigation_type_compensation_works_with_conditional_expression(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_type_compensation_works_with_predicate_negated(bool isAsync)
        {
            return base.Optional_navigation_type_compensation_works_with_predicate_negated(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task String_compare_with_null_conditional_argument(bool isAsync)
        {
            return base.String_compare_with_null_conditional_argument(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_type_compensation_works_with_predicate2(bool isAsync)
        {
            return base.Optional_navigation_type_compensation_works_with_predicate2(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Where_required_navigation_on_derived_type(bool isAsync)
        {
            return base.Where_required_navigation_on_derived_type(isAsync);
        }

        [ConditionalTheory(Skip = "issue #15343")]
        public override Task Optional_navigation_type_compensation_works_with_binary_and_expression(bool isAsync)
        {
            return base.Optional_navigation_type_compensation_works_with_binary_and_expression(isAsync);
        }
    }
}
