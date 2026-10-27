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

    #region ROLLUP, CUBE and GROUPING SETS

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Rollup_two_keys_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .GroupBy(o => EF.Functions.Rollup(new { o.CustomerID, o.EmployeeID }))
                .Select(g => new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() }),
            ss =>
            {
                var orders = ss.Set<Order>().Where(o => o.OrderID < 10300);
                return orders
                    .GroupBy(o => new { o.CustomerID, o.EmployeeID })
                    .Select(g => new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() })
                    .Concat(
                        orders.GroupBy(o => o.CustomerID)
                            .Select(g => new { CustomerID = g.Key, EmployeeID = (uint?)null, Count = g.Count() }))
                    .Concat(
                        orders.GroupBy(o => 1)
                            .Select(g => new { CustomerID = (string?)null, EmployeeID = (uint?)null, Count = g.Count() }));
            },
            elementSorter: e => (e.CustomerID, e.EmployeeID, e.Count));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Rollup_Count_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => EF.Functions.Rollup(new { c.Country, c.City }))
                .Select(g => new
                {
                    g.Key.Country,
                    g.Key.City,
                    Titles = g.Select(c => c.ContactTitle).Distinct().Count()
                }),
            ss =>
            {
                var customers = ss.Set<Customer>();
                return customers
                    .GroupBy(c => new { c.Country, c.City })
                    .Select(g => new
                    {
                        g.Key.Country,
                        g.Key.City,
                        Titles = g.Select(c => c.ContactTitle).Distinct().Count()
                    })
                    .Concat(
                        customers.GroupBy(c => c.Country)
                            .Select(g => new
                            {
                                Country = g.Key,
                                City = (string?)null,
                                Titles = g.Select(c => c.ContactTitle).Distinct().Count()
                            }))
                    .Concat(
                        customers.GroupBy(c => 1)
                            .Select(g => new
                            {
                                Country = (string?)null,
                                City = (string?)null,
                                Titles = g.Select(c => c.ContactTitle).Distinct().Count()
                            }));
            },
            elementSorter: e => (e.Country, e.City, e.Titles));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Cube_two_keys_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .GroupBy(o => EF.Functions.Cube(new { o.CustomerID, o.EmployeeID }))
                .Select(g => new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() }),
            ss =>
            {
                var orders = ss.Set<Order>().Where(o => o.OrderID < 10300);
                return orders
                    .GroupBy(o => new { o.CustomerID, o.EmployeeID })
                    .Select(g => new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() })
                    .Concat(
                        orders.GroupBy(o => o.CustomerID)
                            .Select(g => new { CustomerID = g.Key, EmployeeID = (uint?)null, Count = g.Count() }))
                    .Concat(
                        orders.GroupBy(o => o.EmployeeID)
                            .Select(g => new { CustomerID = (string?)null, EmployeeID = g.Key, Count = g.Count() }))
                    .Concat(
                        orders.GroupBy(o => 1)
                            .Select(g => new { CustomerID = (string?)null, EmployeeID = (uint?)null, Count = g.Count() }));
            },
            elementSorter: e => (e.CustomerID, e.EmployeeID, e.Count));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_GroupingSets_with_empty_set(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .GroupBy(o => EF.Functions.GroupingSets(
                    new { o.CustomerID, o.EmployeeID },
                    new object?[] { o.CustomerID },
                    new object?[] { o.EmployeeID },
                    new object?[0]))
                .Select(g => new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() }),
            ss =>
            {
                var orders = ss.Set<Order>().Where(o => o.OrderID < 10300);
                return orders.GroupBy(o => o.CustomerID)
                    .Select(g => new { CustomerID = g.Key, EmployeeID = (uint?)null, Count = g.Count() })
                    .Concat(
                        orders.GroupBy(o => o.EmployeeID)
                            .Select(g => new { CustomerID = (string?)null, EmployeeID = g.Key, Count = g.Count() }))
                    .Concat(
                        orders.GroupBy(o => 1)
                            .Select(g => new { CustomerID = (string?)null, EmployeeID = (uint?)null, Count = g.Count() }));
            },
            elementSorter: e => (e.CustomerID, e.EmployeeID, e.Count));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Rollup_with_Grouping_function(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .GroupBy(o => EF.Functions.Rollup(new { o.CustomerID, o.EmployeeID }))
                .Select(g => new
                {
                    g.Key.CustomerID,
                    g.Key.EmployeeID,
                    CustomerGrouping = EF.Functions.Grouping(g.Key.CustomerID),
                    EmployeeGrouping = EF.Functions.Grouping(g.Key.EmployeeID),
                    Count = g.Count()
                }),
            ss =>
            {
                var orders = ss.Set<Order>().Where(o => o.OrderID < 10300);
                return orders
                    .GroupBy(o => new { o.CustomerID, o.EmployeeID })
                    .Select(g => new
                    {
                        g.Key.CustomerID,
                        g.Key.EmployeeID,
                        CustomerGrouping = 0,
                        EmployeeGrouping = 0,
                        Count = g.Count()
                    })
                    .Concat(
                        orders.GroupBy(o => o.CustomerID)
                            .Select(g => new
                            {
                                CustomerID = g.Key,
                                EmployeeID = (uint?)null,
                                CustomerGrouping = 0,
                                EmployeeGrouping = 1,
                                Count = g.Count()
                            }))
                    .Concat(
                        orders.GroupBy(o => 1)
                            .Select(g => new
                            {
                                CustomerID = (string?)null,
                                EmployeeID = (uint?)null,
                                CustomerGrouping = 1,
                                EmployeeGrouping = 1,
                                Count = g.Count()
                            }));
            },
            elementSorter: e => (e.CustomerID, e.EmployeeID, e.Count));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Rollup_with_Having(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .GroupBy(o => EF.Functions.Rollup(new { o.CustomerID, o.EmployeeID }))
                .Where(g => g.Count() > 1)
                .Select(g => new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() }),
            ss =>
            {
                var orders = ss.Set<Order>().Where(o => o.OrderID < 10300);
                return orders
                    .GroupBy(o => new { o.CustomerID, o.EmployeeID })
                    .Select(g => new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() })
                    .Concat(
                        orders.GroupBy(o => o.CustomerID)
                            .Select(g => new { CustomerID = g.Key, EmployeeID = (uint?)null, Count = g.Count() }))
                    .Concat(
                        orders.GroupBy(o => 1)
                            .Select(g => new { CustomerID = (string?)null, EmployeeID = (uint?)null, Count = g.Count() }))
                    .Where(e => e.Count > 1);
            },
            elementSorter: e => (e.CustomerID, e.EmployeeID, e.Count));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Rollup_computed_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .GroupBy(o => EF.Functions.Rollup(new { Year = (int?)o.OrderDate!.Value.Year, o.EmployeeID }))
                .Select(g => new { g.Key.Year, g.Key.EmployeeID, Count = g.Count() }),
            ss =>
            {
                var orders = ss.Set<Order>().Where(o => o.OrderID < 10300);
                return orders
                    .GroupBy(o => new { Year = (int?)o.OrderDate!.Value.Year, o.EmployeeID })
                    .Select(g => new { g.Key.Year, g.Key.EmployeeID, Count = g.Count() })
                    .Concat(
                        orders.GroupBy(o => (int?)o.OrderDate!.Value.Year)
                            .Select(g => new { Year = g.Key, EmployeeID = (uint?)null, Count = g.Count() }))
                    .Concat(
                        orders.GroupBy(o => 1)
                            .Select(g => new { Year = (int?)null, EmployeeID = (uint?)null, Count = g.Count() }));
            },
            elementSorter: e => (e.Year, e.EmployeeID, e.Count));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_Rollup_non_nullable_key_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders
            .GroupBy(o => EF.Functions.Rollup(o.OrderID))
            .Select(g => g.Count());

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            async () => _ = async ? await query.ToListAsync() : query.ToList())).Message;

        Assert.Equal(RelationalStrings.GroupingSetsKeyMustBeNullable("Key", "int"), message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_Rollup_final_GroupBy_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders
            .GroupBy(o => EF.Functions.Rollup(o.CustomerID));

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            async () => _ = async ? await query.ToListAsync() : query.ToList())).Message;

        Assert.Equal(RelationalStrings.GroupingSetsFinalGroupByNotSupported, message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_Rollup_grouping_element_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders
            .GroupBy(o => EF.Functions.Rollup(o.CustomerID))
            .Select(g => new { g.Key, Ids = g.Where(o => o.OrderID > 10300).Select(o => o.OrderID).ToList() });

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            async () => _ = async ? await query.ToListAsync() : query.ToList())).Message;

        Assert.Equal(RelationalStrings.GroupingSetsGroupingElementNotSupported, message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_GroupingSets_element_not_in_key_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders
            .GroupBy(o => EF.Functions.GroupingSets(new { o.CustomerID }, new object?[] { o.ShipCity }))
            .Select(g => g.Count());

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            async () => _ = async ? await query.ToListAsync() : query.ToList())).Message;

        Assert.StartsWith(RelationalStrings.GroupingSetsElementNotInKey("o.ShipCity")[..30], message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_GroupingSets_without_sets_throws(bool async)
    {
        using var context = CreateContext();
        var query = context.Orders
            .GroupBy(o => EF.Functions.GroupingSets(new { o.CustomerID }))
            .Select(g => g.Count());

        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            async () => _ = async ? await query.ToListAsync() : query.ToList())).Message;

        Assert.Equal(RelationalStrings.GroupingSetsEmpty, message);
    }

    #endregion
}
