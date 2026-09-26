// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindGroupByQueryRelationalTestBase<TFixture>(TFixture fixture) : NorthwindGroupByQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    // The result selector of a lifted grouping projection runs on the client, but isn't a child of the shaper, so VisitShapedQuery's
    // verification doesn't reach it; ApplyGroupByResultSelector verifies it separately. Without that, the captured instance would be
    // rooted in the compiled query cache instead of being reported.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_selecting_grouping_element_list_with_captured_instance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().GroupBy(o => o.CustomerID)
                .Select(g => new { Orders = g.Select(e => e.OrderID).ToList(), Instance = this }));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Final_GroupBy_nullable_value_type_key_with_split_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F") || c.CustomerID.StartsWith("P"))
                .Include(c => c.Orders)
                .AsSplitQuery()
                .GroupBy(c => c.Orders.Max(o => o.EmployeeID)),
            elementSorter: e => e.Key,
            elementAsserter: (e, a) => AssertGrouping(
                e, a,
                elementSorter: c => c.CustomerID,
                elementAsserter: (ee, aa) => AssertInclude(ee, aa, new ExpectedInclude<Customer>(c => c.Orders))));
}
