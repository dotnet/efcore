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

        public override Task Where_query_composition_entity_equality_one_element_Single(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_one_element_Single(isAsync));
        }

        public override Task Where_query_composition_entity_equality_one_element_First(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_one_element_First(isAsync));
        }

        public override Task Where_query_composition_entity_equality_no_elements_Single(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_no_elements_Single(isAsync));
        }

        public override Task Where_query_composition_entity_equality_no_elements_First(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_no_elements_First(isAsync));
        }

        public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(isAsync));
        }

        public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(() => base.Where_query_composition_entity_equality_multiple_elements_Single(isAsync));
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
        public override Task Contains_with_local_tuple_array_closure(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Last_when_no_order_by(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task OrderBy_multiple_queries(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_1()
        {
            base.Random_next_is_not_funcletized_1();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_2()
        {
            base.Random_next_is_not_funcletized_2();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_3()
        {
            base.Random_next_is_not_funcletized_3();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_4()
        {
            base.Random_next_is_not_funcletized_4();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_5()
        {
            base.Random_next_is_not_funcletized_5();
        }

        [ConditionalFact(Skip = "Issue#17386")]
        public override void Random_next_is_not_funcletized_6()
        {
            base.Random_next_is_not_funcletized_6();
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool isAsync)
        {
            return Task.CompletedTask;
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Where_bool_client_side_negated(bool isAsync)
        {
            return base.Where_bool_client_side_negated(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Projection_when_arithmetic_mixed_subqueries(bool isAsync)
        {
            return base.Projection_when_arithmetic_mixed_subqueries(isAsync);
        }

        [ConditionalTheory(Skip = "Issue#17536")]
        public override Task SelectMany_correlated_with_outer_3(bool isAsync)
        {
            return base.SelectMany_correlated_with_outer_3(isAsync);
        }

        [ConditionalTheory(Skip = "Issue #17531")]
        public override Task DefaultIfEmpty_in_subquery_nested(bool isAsync)
        {
            return base.DefaultIfEmpty_in_subquery_nested(isAsync);
        }

        [ConditionalTheory(Skip = "issue #17386")]
        public override Task Where_equals_on_null_nullable_int_types(bool isAsync)
        {
            return base.Where_equals_on_null_nullable_int_types(isAsync);
        }
    }
}
