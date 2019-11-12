// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SimpleQueryInMemoryTest : SimpleQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public SimpleQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
        }

        // InMemory can throw server side exception
        public override void Average_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Average_no_data_subquery());
        }

        public override void Max_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Max_no_data_subquery());
        }

        public override void Min_no_data_subquery()
        {
            Assert.Throws<InvalidOperationException>(() => base.Min_no_data_subquery());
        }

        public override Task Where_query_composition_entity_equality_one_element_Single(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Where_query_composition_entity_equality_one_element_Single(async));
        }

        public override Task Where_query_composition_entity_equality_one_element_First(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Where_query_composition_entity_equality_one_element_First(async));
        }

        public override Task Where_query_composition_entity_equality_no_elements_Single(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Where_query_composition_entity_equality_no_elements_Single(async));
        }

        public override Task Where_query_composition_entity_equality_no_elements_First(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Where_query_composition_entity_equality_no_elements_First(async));
        }

        public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async));
        }

        public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Where_query_composition_entity_equality_multiple_elements_Single(async));
        }

        public override Task Collection_Last_member_access_in_projection_translated(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Collection_Last_member_access_in_projection_translated(async));
        }

        // Sending client code to server
        [ConditionalFact(Skip = "Issue#17050")]
        public override void Client_code_using_instance_in_anonymous_type()
        {
            base.Client_code_using_instance_in_anonymous_type();
        }

        [ConditionalFact(Skip = "Issue#17050")]
        public override void Client_code_using_instance_in_static_method()
        {
            base.Client_code_using_instance_in_static_method();
        }

        [ConditionalFact(Skip = "Issue#17050")]
        public override void Client_code_using_instance_method_throws()
        {
            base.Client_code_using_instance_method_throws();
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Contains_with_local_tuple_array_closure(bool async)
        {
            return base.Contains_with_local_tuple_array_closure(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Last_when_no_order_by(bool async)
        {
            return base.Last_when_no_order_by(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task OrderBy_multiple_queries(bool async)
        {
            return base.OrderBy_multiple_queries(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Random_next_is_not_funcletized_1(bool async)
        {
            return base.Random_next_is_not_funcletized_1(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Random_next_is_not_funcletized_2(bool async)
        {
            return base.Random_next_is_not_funcletized_2(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Random_next_is_not_funcletized_3(bool async)
        {
            return base.Random_next_is_not_funcletized_3(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Random_next_is_not_funcletized_4(bool async)
        {
            return base.Random_next_is_not_funcletized_4(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Random_next_is_not_funcletized_5(bool async)
        {
            return base.Random_next_is_not_funcletized_5(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Random_next_is_not_funcletized_6(bool async)
        {
            return base.Random_next_is_not_funcletized_6(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
        {
            return base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Where_bool_client_side_negated(bool async)
        {
            return base.Where_bool_client_side_negated(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Projection_when_arithmetic_mixed_subqueries(bool async)
        {
            return base.Projection_when_arithmetic_mixed_subqueries(async);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Where_equals_method_string_with_ignore_case(bool async)
        {
            return base.Where_equals_method_string_with_ignore_case(async);
        }

        [ConditionalTheory(Skip = "Issue#17536")]
        public override Task SelectMany_correlated_with_outer_3(bool async)
        {
            return base.SelectMany_correlated_with_outer_3(async);
        }

        [ConditionalTheory]
        public override Task DefaultIfEmpty_in_subquery_nested(bool async)
        {
            return base.DefaultIfEmpty_in_subquery_nested(async);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Where_equals_on_null_nullable_int_types(bool async)
        {
            return base.Where_equals_on_null_nullable_int_types(async);
        }

        // Casting int to object to string is invalid for InMemory
        public override Task Like_with_non_string_column_using_double_cast(bool async) => Task.CompletedTask;
    }
}
