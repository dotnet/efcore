// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindGroupByQueryRelationalTestBase<TFixture>(TFixture fixture) : NorthwindGroupByQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    // A projection into a type whose members do not map onto its constructor arguments cannot be folded back into the
    // aggregate selector, so the GroupBy aggregate lift is abandoned and the query fails to translate exactly as it
    // does without it.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_element_selector_projecting_constructor_bound_type_with_aggregate(bool async)
        => AssertTranslationFailed(() => AssertQuery(async, ConstructorBoundElementSelectorQuery));

    private static IQueryable<string?> ConstructorBoundElementSelectorQuery(ISetSource ss)
        => ss.Set<Order>()
            .GroupBy(o => o.EmployeeID, o => new OrderRegion(o.Customer!.Region, o.OrderID))
            .Select(g => g.Max(x => x.Region));

    private class OrderRegion(string? region, int orderID)
    {
        public string? Region { get; } = region;

        public int OrderID { get; } = orderID;
    }
}
