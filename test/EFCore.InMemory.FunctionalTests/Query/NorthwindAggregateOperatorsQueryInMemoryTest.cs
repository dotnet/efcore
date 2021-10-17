// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

        public override async Task Average_on_nav_subquery_in_projection(bool async)
        {
            Assert.Equal(
                "Sequence contains no elements",
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => base.Average_on_nav_subquery_in_projection(async))).Message);
        }

        public override Task Collection_Last_member_access_in_projection_translated(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Collection_Last_member_access_in_projection_translated(async));
        }
    }
}
