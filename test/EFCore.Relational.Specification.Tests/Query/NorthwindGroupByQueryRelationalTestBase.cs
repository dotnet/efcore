// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindGroupByQueryRelationalTestBase<TFixture>(TFixture fixture) : NorthwindGroupByQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_DefaultIfEmpty_over_GroupBy(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<Product>()
                  from t in ss.Set<OrderDetail>()
                      .Where(od => od.ProductID < 10)
                      .GroupBy(od => od.ProductID, od => od.Quantity)
                      .Select((IGrouping<int, short> g) => new { g.Key, Sum = g.Sum(int (short q) => q) })
                      .Where(t => t.Key == p.ProductID)
                      .DefaultIfEmpty()
                  select new { p.ProductID, Key = (int?)t!.Key, Sum = (int?)t.Sum ?? 0 },
            expectedQuery: ss => from p in ss.Set<Product>()
                                 from t in ss.Set<OrderDetail>()
                                     .Where(od => od.ProductID < 10)
                                     .GroupBy(od => od.ProductID, od => od.Quantity)
                                     .Select((IGrouping<int, short> g) => new { g.Key, Sum = g.Sum(int (short q) => q) })
                                     .Where(t => t.Key == p.ProductID)
                                     .DefaultIfEmpty()
                                 select new { p.ProductID, Key = t == null ? (int?)null : t.Key, Sum = t == null ? 0 : t.Sum },
            elementSorter: e => e.ProductID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_DefaultIfEmpty_over_Distinct(bool async)
        => AssertQuery(
            async,
            ss => from p in ss.Set<Product>()
                  from t in ss.Set<OrderDetail>()
                      .Where(od => od.ProductID < 10)
                      .Select(od => new { Key = od.ProductID, od.Quantity })
                      .Distinct()
                      .Where(t => t.Key == p.ProductID)
                      .DefaultIfEmpty()
                  select new { p.ProductID, Key = (int?)t!.Key, Quantity = (int?)t.Quantity ?? 0 },
            expectedQuery: ss => from p in ss.Set<Product>()
                                 from t in ss.Set<OrderDetail>()
                                     .Where(od => od.ProductID < 10)
                                     .Select(od => new { Key = od.ProductID, od.Quantity })
                                     .Distinct()
                                     .Where(t => t.Key == p.ProductID)
                                     .DefaultIfEmpty()
                                 select new
                                 {
                                     p.ProductID,
                                     Key = t == null ? (int?)null : t.Key,
                                     Quantity = t == null ? 0 : (int)t.Quantity
                                 },
            elementSorter: e => (e.ProductID, e.Quantity));

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

    // A non-deterministic function in the key can evaluate differently inside the aggregate than it did for the GROUP BY, so
    // the key is not recomputed there, and the lift is abandoned. Multiplying by zero keeps the result checkable.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_aggregate_capturing_key_computed_with_Random_through_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => (int)(EF.Functions.Random() * 0))
                .Select(g => new { g.Key, Max = g.Max(o => o.Customer!.Region == null ? 0 : g.Key + 1) }),
            ss => ss.Set<Order>()
                .GroupBy(o => 0)
                .Select(g => new { g.Key, Max = g.Max(o => o.Customer!.Region == null ? 0 : g.Key + 1) }),
            elementSorter: e => e.Key);

    // The same projection behind a cast, which the member access reads through.
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_element_selector_projecting_cast_constructor_bound_type_with_aggregate(bool async)
        => AssertTranslationFailed(() => AssertQuery(async, CastConstructorBoundElementSelectorQuery));

    private static IQueryable<string?> CastConstructorBoundElementSelectorQuery(ISetSource ss)
        => ss.Set<Order>()
            .GroupBy(o => o.EmployeeID, o => (object)new OrderRegion(o.Customer!.Region, o.OrderID))
            .Select(g => g.Max(x => ((OrderRegion)x).Region));

    private class OrderRegion(string? region, int orderID)
    {
        public string? Region { get; } = region;

        public int OrderID { get; } = orderID;
    }
}
