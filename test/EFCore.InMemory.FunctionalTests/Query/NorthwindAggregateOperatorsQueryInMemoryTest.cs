// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindAggregateOperatorsQueryInMemoryTest : NorthwindAggregateOperatorsQueryTestBase<
        NorthwindQueryInMemoryFixture<NoopModelCustomizer>>
    {
        public NorthwindAggregateOperatorsQueryInMemoryTest(
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
            using var context = CreateContext();

            Assert.Equal(
                "Sequence contains no elements",
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Average(o => o.OrderID)).ToList()).Message);
        }

        public override void Max_no_data_subquery()
        {
            using var context = CreateContext();

            Assert.Equal(
                "Sequence contains no elements",
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Max(o => o.OrderID)).ToList()).Message);
        }

        public override void Min_no_data_subquery()
        {
            using var context = CreateContext();

            Assert.Equal(
                "Sequence contains no elements",
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Min(o => o.OrderID)).ToList()).Message);
        }

        public override Task Collection_Last_member_access_in_projection_translated(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Collection_Last_member_access_in_projection_translated(async));
        }

        [ConditionalTheory(Skip = "Issue#17386")]
        public override Task Contains_with_local_tuple_array_closure(bool async)
        {
            return base.Contains_with_local_tuple_array_closure(async);
        }

        [ConditionalFact(Skip = "Issue#20023")]
        public override void Contains_over_keyless_entity_throws()
        {
            base.Contains_over_keyless_entity_throws();
        }
    }
}
