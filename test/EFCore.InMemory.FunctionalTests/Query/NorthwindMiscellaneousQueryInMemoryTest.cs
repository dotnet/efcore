// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindMiscellaneousQueryInMemoryTest : NorthwindMiscellaneousQueryTestBase<NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindMiscellaneousQueryInMemoryTest(
            NorthwindQueryInMemoryFixture<NoopModelCustomizer> fixture,
#pragma warning disable IDE0060 // Remove unused parameter
            ITestOutputHelper testOutputHelper)
#pragma warning restore IDE0060 // Remove unused parameter
            : base(fixture)
        {
            //TestLoggerFactory.TestOutputHelper = testOutputHelper;
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

        [ConditionalTheory]
        public override Task DefaultIfEmpty_in_subquery_nested(bool async)
        {
            return base.DefaultIfEmpty_in_subquery_nested(async);
        }
    }
}
