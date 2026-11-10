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

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().GroupBy(c => c.City).SelectMany(g => g));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_GroupBy_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID)
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_GroupBy_SelectMany_shadow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().OrderBy(e => e.EmployeeID)
                .GroupBy(e => e.EmployeeID)
                .SelectMany(g => g)
                .Select(g => EF.Property<string>(g, "Title")));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderByDescending(o => o.OrderDate).ThenBy(o => o.OrderID).Take(2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_parameter(bool async)
    {
        var count = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(count)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(1).Take(2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(3).Skip(1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_Take_without_OrderBy_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.Take(1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_Select(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(2).Select(o => new { o.OrderID, o.OrderDate })),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_Select_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.Select(o => new { g.Key, o.OrderID })),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_result_selector(bool async)
        => AssertQuery(
            async,
            ss => from g in ss.Set<Order>().GroupBy(o => o.CustomerID)
                  from o in g
                  select new { g.Key, o.OrderID },
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_result_selector(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => new { o.CustomerID, o.EmployeeID })
                .SelectMany(
                    g => g.OrderBy(o => o.OrderID).Take(1),
                    (g, o) => new { g.Key.CustomerID, g.Key.EmployeeID, o.OrderID }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_nullable_key_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>()
                .GroupBy(e => e.ReportsTo)
                .SelectMany(g => g.OrderBy(e => e.EmployeeID).Take(1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_computed_key_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.OrderDate!.Value.Year)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_OrderBy_key_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .OrderBy(g => g.Key)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_composed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(2))
                .Where(o => o.OrderID > 10500)
                .OrderBy(o => o.OrderID)
                .Take(5)
                .Select(o => o.OrderID),
            assertOrder: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1))
                .Select(o => new { o.OrderID, o.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_Where_Sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10400)
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1))
                .Select(o => o.OrderID));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_Take_parameters(bool async)
    {
        var skip = 1;
        var take = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(skip).Take(take)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_computed_key_SelectMany_OrderBy_Take_Select_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.OrderDate!.Value.Year)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(2).Select(o => new { g.Key, o.OrderID })),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_composite_key_member_SelectMany_OrderBy_Take_Select(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => new { o.CustomerID, o.EmployeeID })
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1).Select(o => new { g.Key.EmployeeID, o.OrderID })),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_navigation_key_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.Customer!.City)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_element_selector_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID, o => o.OrderID)
                .SelectMany(g => g));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_element_selector_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID, o => new { o.OrderID, o.EmployeeID })
                .SelectMany(g => g.OrderBy(e => e.OrderID).Take(1)),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_ToList(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.ToList()));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_Include(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1))
                .Include(o => o.Customer),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer!)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_Include_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10400)
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1))
                .Include(o => o.OrderDetails),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.OrderDetails)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_GroupBy_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Include(o => o.Customer)
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1)),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer!)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_GroupBy_SelectMany(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(o => new ProjectedType { Order = o.OrderID, Customer = o.CustomerID })
                .GroupBy(p => p.Customer)
                .SelectMany(g => g),
            elementSorter: g => g.Order);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_result_selector_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g, (g, o) => new { g.Key, o })
                .Select(x => new { x.Key, x.o.OrderID, x.o.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_result_selector_collection_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(1), (g, o) => new { g.Key, o })
                .Select(x => new { x.o.OrderID, x.o.OrderDetails.Count }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Select_anonymous_GroupBy_SelectMany_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(o => new { o, o.CustomerID })
                .GroupBy(x => x.CustomerID)
                .SelectMany(g => g)
                .Select(x => new { x.o.OrderID, x.o.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_anonymous_element_selector_SelectMany_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID, o => new { o })
                .SelectMany(g => g)
                .Select(x => new { x.o.OrderID, x.o.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_result_selector_dto_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g, (g, o) => new OrderHolder { Key = g.Key, Order = o })
                .Select(x => new { x.Key, x.Order.OrderID, x.Order.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_nested_anonymous_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(2).Select(o => new { a = new { o } }))
                .Select(x => new { x.a.o.OrderID, x.a.o.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_GroupBy_SelectMany_result_selector_dto(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Include(o => o.OrderDetails)
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g, (g, o) => new OrderHolder { Key = g.Key, Order = o }),
            elementSorter: e => e.Order.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertInclude(e.Order, a.Order, new ExpectedInclude<Order>(o => o.OrderDetails));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_GroupBy_dto_element_selector_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10300)
                .Include(o => o.OrderDetails)
                .GroupBy(o => o.CustomerID, o => new OrderHolder { Key = o.CustomerID, Order = o })
                .SelectMany(g => g.OrderBy(x => x.Order.OrderID).Take(1)),
            elementSorter: e => e.Order.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Key, a.Key);
                AssertInclude(e.Order, a.Order, new ExpectedInclude<Order>(o => o.OrderDetails));
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_constant_Take_int_MaxValue_parameter(bool async)
    {
        var take = int.MaxValue;

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(1).Take(take)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_OrderBy_key_SelectMany_OrderByDescending_Skip_Take_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .OrderByDescending(g => g.Key)
                .SelectMany(g => g.OrderBy(o => o.OrderDate).ThenByDescending(o => o.OrderID).Skip(1).Take(2))
                .Take(15)
                .Select(o => o.OrderID),
            assertOrder: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_in_predicate_subquery_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(async, OrdersInFirstOrderOfEachCustomer));

    private static IQueryable<Order> OrdersInFirstOrderOfEachCustomer(ISetSource ss)
        => ss.Set<Order>().Where(o => ss.Set<Order>()
            .GroupBy(x => x.CustomerID)
            .SelectMany(g => g.OrderBy(x => x.OrderID).Take(1))
            .Select(x => x.OrderID)
            .Contains(o.OrderID));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_ToList_result_selector(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.ToList(), (g, o) => new { g.Key, o.OrderID }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_zero(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(0)),
            assertEmpty: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Take_Skip_past_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(2).Skip(3)),
            assertEmpty: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_Take_int_MaxValue(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(1).Take(int.MaxValue)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_negative_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(-1).Take(2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_Take_int_MaxValue_parameters(bool async)
    {
        var skip = 1;
        var take = int.MaxValue;

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(skip).Take(take)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_negative_Take_parameters(bool async)
    {
        var skip = -1;
        var take = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(skip).Take(take)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_Skip_constant_Take_parameter(bool async)
    {
        var take = 2;

        return AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Skip(1).Take(take)));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_ThenBy_ThenBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.EmployeeID).ThenByDescending(o => o.OrderDate).ThenBy(o => o.OrderID).Take(2)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_OrderBy_key_SelectMany_OrderByDescending(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .OrderBy(g => g.Key)
                .SelectMany(g => g.OrderByDescending(o => o.OrderID))
                .Select(o => o.OrderID),
            assertOrder: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_OrderBy_key_SelectMany_OrderByDescending_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .OrderBy(g => g.Key)
                .SelectMany(g => g.OrderByDescending(o => o.OrderID).Take(2))
                .Select(o => o.OrderID),
            assertOrder: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_Take_count_of_group_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(g.Count() - 1))));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_Take_key_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.OrderID % 3)
                .SelectMany(g => g.OrderBy(o => o.OrderID).Take(g.Key))));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_group_aggregate_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.OrderBy(o => g.Count()).ThenBy(o => o.OrderID).Take(1))));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Correlated_GroupBy_SelectMany_OrderBy_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("A"))
                .SelectMany(c => c.Orders.GroupBy(o => o.EmployeeID).SelectMany(g => g.OrderBy(o => o.OrderID).Take(1))));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_Where_over_grouping_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.Where(o => o.OrderID > 10500).Take(1))));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_OrderBy_over_Take_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.Take(2).OrderBy(o => o.OrderID))));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_collection_in_element_selector_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .SelectMany(g => g.Select(o => new { o.OrderID, o.OrderDetails }))));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Where_SelectMany_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_Take_SelectMany_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .GroupBy(o => o.CustomerID)
                .OrderBy(g => g.Key)
                .Take(2)
                .SelectMany(g => g)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task GroupBy_SelectMany_in_subquery_fails(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.GroupBy(o => o.EmployeeID).SelectMany(g => g).ToList()),
            elementSorter: e => e.Count,
            elementAsserter: (e, a) => AssertCollection(e, a)));

    protected class OrderHolder
    {
        public string? Key { get; set; }
        public Order Order { get; set; } = null!;
    }
}
