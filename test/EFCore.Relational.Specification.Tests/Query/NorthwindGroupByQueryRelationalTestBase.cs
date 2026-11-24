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
}
