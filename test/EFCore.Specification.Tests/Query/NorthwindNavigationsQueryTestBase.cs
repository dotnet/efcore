// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindNavigationsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindNavigationsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_nav_projected_in_subquery_when_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>().Select(o => ClientProjection(o, o.Customer)) on c.CustomerID equals o.CustomerID
                      join od in ss.Set<OrderDetail>().Select(od => ClientProjection(od, od.Product)) on o.OrderID equals od.OrderID
                      select c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_nav_in_predicate_in_subquery_when_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>().Where(o => ClientPredicate(o, o.Customer)) on c.CustomerID equals o.CustomerID
                      join od in ss.Set<OrderDetail>().Where(od => ClientPredicate(od, od.Product)) on o.OrderID equals od.OrderID
                      select c));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_with_nav_in_orderby_in_subquery_when_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>().OrderBy(o => ClientOrderBy(o, o.Customer)) on c.CustomerID equals o.CustomerID
                      join od in ss.Set<OrderDetail>().OrderBy(od => ClientOrderBy(od, od.Product)) on o.OrderID equals od.OrderID
                      select c));

    private static readonly Random _randomGenerator = new();

    private static T ClientProjection<T>(T t, object _)
        => t;

    private static bool ClientPredicate<T>(T t, object _)
        => true;

    private static int ClientOrderBy<T>(T t, object _)
        => _randomGenerator.Next(0, 20);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.Customer.City == "Seattle"
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Contains(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.Customer.City.Contains("Sea")
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool async)
        => AssertQuery(
            async,
            ss => from o1 in ss.Set<Order>().Where(o => o.OrderID < 10300)
                  from o2 in ss.Set<Order>().Where(o => o.OrderID < 10400)
                  where o1.Customer.City == o2.Customer.City
                  select new { o1, o2 },
            elementSorter: e => e.o1.OrderID + " " + e.o2.OrderID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.o1, a.o1);
                AssertEqual(e.o2, a.o2);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool async)
        => AssertQuery(
            async,
            ss => from o1 in ss.Set<Order>().Where(o => o.OrderID < 10300)
                  from o2 in ss.Set<Order>().Where(o => o.OrderID < 10400)
                  where o1.Customer.City == o2.Customer.City
                  select new { o1.CustomerID, C2 = o2.CustomerID },
            elementSorter: e => e.CustomerID + " " + e.C2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Client(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => from o in ss.Set<Order>()
                      where o.Customer.IsLondon
                      select o),
            CoreStrings.QueryUnableToTranslateMember(nameof(Customer.IsLondon), nameof(Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Deep(bool async)
        => AssertQuery(
            async,
            ss => (from od in ss.Set<OrderDetail>()
                   where od.Order.Customer.City == "Seattle"
                   orderby od.OrderID, od.ProductID
                   select od).Take(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_Select_Navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(2)
                .Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_FirstOrDefault_project_single_column1(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(2)
                .Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_FirstOrDefault_project_single_column2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(2)
                .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_FirstOrDefault_project_anonymous_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID).Take(2).Select(
                c => c.Orders.OrderBy(o => o.OrderID).Select(
                    o => new { o.CustomerID, o.OrderID }).FirstOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_FirstOrDefault_project_anonymous_type_client_eval(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID).Take(2).Select(
                c => c.Orders.OrderBy(o => o.OrderID).Select(
                    o => new { o.CustomerID, OrderID = ClientFunction(o.OrderID, 5) }).FirstOrDefault()),
            assertOrder: true);

    private static int ClientFunction(int a, int b)
        => a + b + 1;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_FirstOrDefault_project_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(2).Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Skip_Select_Navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID)
                .Skip(20)
                .Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Null(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>()
                  where e.Manager == null
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Null_Reverse(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>()
                  where null == e.Manager
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Null_Deep(bool async)
        => AssertQuery(
            async,
            ss => from e in ss.Set<Employee>()
                  where e.Manager.Manager == null
                  select e);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Equals_Navigation(bool async)
        => AssertQuery(
            async,
            ss => from o1 in ss.Set<Order>()
                  from o2 in ss.Set<Order>()
                  where o1.CustomerID.StartsWith("A")
                  where o2.CustomerID.StartsWith("A")
                  where o1.Customer == o2.Customer
                  select new { o1, o2 },
            elementSorter: e => e.o1.OrderID + " " + e.o2.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Included(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>().Include(o => o.Customer)
                  where o.Customer.City == "Seattle"
                  select o,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Order>(o => o.Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_with_multiple_optional_navigations(bool async)
    {
        var expectedIncludes = new IExpectedInclude[]
        {
            new ExpectedInclude<OrderDetail>(od => od.Order), new ExpectedInclude<Order>(o => o.Customer, "Order")
        };

        return AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Include(od => od.Order.Customer)
                .Where(od => od.Order.Customer.City == "London"),
            elementAsserter: (e, a) => AssertInclude(e, a, expectedIncludes));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_count_plus_sum(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Select(
                o => new { Total = o.OrderDetails.Sum(od => od.Quantity) + o.OrderDetails.Count() }),
            elementSorter: e => e.Total);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Singleton_Navigation_With_Member_Access(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.Customer.City == "Seattle"
                  where o.Customer.Phone != "555 555 5555"
                  select new { B = o.Customer.City },
            elementSorter: e => e.B);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Singleton_Navigation_With_Member_Access(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.Customer.City == "Seattle"
                  where o.Customer.Phone != "555 555 5555"
                  select new { A = o.Customer, B = o.Customer.City },
            elementSorter: e => e.A + " " + e.B);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Where_Navigation_Multiple_Access(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.Customer.City == "Seattle"
                      && o.Customer.Phone != "555 555 5555"
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Navigation(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  select o.Customer);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Navigations(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  select new { A = o.Customer, B = o.Customer },
            elementSorter: e => e.A.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.A, a.A);
                AssertEqual(e.B, a.B);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Navigations_Where_Navigations(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.Customer.City == "Seattle"
                  where o.Customer.Phone != "555 555 5555"
                  select new { A = o.Customer, B = o.Customer },
            elementSorter: e => e.A.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.A, a.A);
                AssertEqual(e.B, a.B);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_navigation_simple(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.StartsWith("A")
                  orderby c.CustomerID
                  select new { c.CustomerID, c.Orders },
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                AssertCollection(e.Orders, a.Orders);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_navigation_simple2(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.StartsWith("A")
                  orderby c.CustomerID
                  select new { c.CustomerID, c.Orders.Count },
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.Count, a.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_navigation_simple_followed_by_ordering_by_scalar(bool async)
        => AssertQuery(
            async,
            ss => (from c in ss.Set<Customer>()
                   where c.CustomerID.StartsWith("A")
                   orderby c.CustomerID
                   select new { c.CustomerID, c.Orders }).OrderBy(e => e.CustomerID),
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                AssertCollection(e.Orders, a.Orders);
            },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_navigation_multi_part(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.CustomerID == "ALFKI"
                  select new { o.OrderID, o.Customer.Orders },
            elementSorter: e => e.OrderID,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderID, a.OrderID);
                AssertCollection(e.Orders, a.Orders);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_collection_navigation_multi_part2(bool async)
        => AssertQuery(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  orderby od.OrderID, od.ProductID
                  where od.Order.CustomerID == "ALFKI" || od.Order.CustomerID == "ANTON"
                  select new { od.Order.Customer.Orders },
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e.Orders, a.Orders));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_any(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select new { Any = c.Orders.Any() },
            ss => from c in ss.Set<Customer>()
                  select new { Any = (c.Orders ?? new List<Order>()).Any() },
            elementSorter: e => e.Any);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_predicate(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.Count > 0),
            ss => ss.Set<Customer>().Select(c => (c.Orders ?? new List<Order>()).Count > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_where_nav_prop_any(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.Orders.Any()
                  select c,
            ss => from c in ss.Set<Customer>()
                  where (c.Orders ?? new List<Order>()).Any()
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_where_nav_prop_any_predicate(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.Orders.Any(o => o.OrderID > 0)
                  select c,
            ss => from c in ss.Set<Customer>()
                  where (c.Orders ?? new List<Order>()).Any(o => o.OrderID > 0)
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_all(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select new { All = c.Orders.All(o => o.CustomerID == "ALFKI") },
            ss => from c in ss.Set<Customer>()
                  select new { All = (c.Orders ?? new List<Order>()).All(o => o.CustomerID == "ALFKI") },
            elementSorter: e => e.All);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_all_client(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      orderby c.CustomerID
                      select new { All = c.Orders.All(o => o.ShipCity == "London") },
                ss => from c in ss.Set<Customer>()
                      orderby c.CustomerID
                      select new { All = (c.Orders ?? new List<Order>()).All(o => false) },
                assertOrder: true),
            CoreStrings.QueryUnableToTranslateMember(nameof(Order.ShipCity), nameof(Order)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_where_nav_prop_all(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.Orders.All(o => o.CustomerID == "ALFKI")
                  select c,
            ss => from c in ss.Set<Customer>()
                  where (c.Orders ?? new List<Order>()).All(o => o.CustomerID == "ALFKI")
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_where_nav_prop_all_client(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      orderby c.CustomerID
                      where c.Orders.All(o => o.ShipCity == "London")
                      select c),
            CoreStrings.QueryUnableToTranslateMember(nameof(Order.ShipCity), nameof(Order)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_count(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select new { c.Orders.Count },
            ss => from c in ss.Set<Customer>()
                  select new { (c.Orders ?? new List<Order>()).Count },
            elementSorter: e => e.Count);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_where_nav_prop_count(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.Orders.Count() > 5
                  select c,
            ss => from c in ss.Set<Customer>()
                  where (c.Orders ?? new List<Order>()).Count() > 5
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_where_nav_prop_count_reverse(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where 5 < c.Orders.Count()
                  select c,
            ss => from c in ss.Set<Customer>()
                  where 5 < (c.Orders ?? new List<Order>()).Count()
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_orderby_nav_prop_count(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  orderby c.Orders.Count(), c.CustomerID
                  select c,
            ss => from c in ss.Set<Customer>()
                  orderby (c.Orders ?? new List<Order>()).Count(), c.CustomerID
                  select c,
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_long_count(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select new { C = c.Orders.LongCount() },
            ss => from c in ss.Set<Customer>()
                  select new { C = (c.Orders ?? new List<Order>()).LongCount() },
            elementSorter: e => e.C);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_multiple_complex_projections(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.CustomerID.StartsWith("A")
                  select new
                  {
                      collection1 = o.OrderDetails.Count(),
                      scalar1 = o.OrderDate,
                      any = o.OrderDetails.Select(od => od.UnitPrice).Any(up => up > 10),
                      conditional = o.CustomerID == "ALFKI" ? "50" : "10",
                      scalar2 = (int?)o.OrderID,
                      all = o.OrderDetails.All(od => od.OrderID == 42),
                      collection2 = o.OrderDetails.LongCount()
                  },
            elementSorter: e => e.scalar2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_sum(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select new { Sum = c.Orders.Sum(o => o.OrderID) },
            ss => from c in ss.Set<Customer>()
                  select new { Sum = (c.Orders ?? new List<Order>()).Sum(o => o.OrderID) },
            elementSorter: e => e.Sum);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_sum_plus_one(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select new { Sum = c.Orders.Sum(o => o.OrderID) + 1 },
            ss => from c in ss.Set<Customer>()
                  select new { Sum = (c.Orders ?? new List<Order>()).Sum(o => o.OrderID) + 1 },
            elementSorter: e => e.Sum);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_where_nav_prop_sum(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.Orders.Sum(o => o.OrderID) > 1000
                  select c,
            ss => from c in ss.Set<Customer>()
                  where (c.Orders ?? new List<Order>()).Sum(o => o.OrderID) > 1000
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_first_or_default(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  orderby c.CustomerID
                  select new { First = c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() },
            ss => from c in ss.Set<Customer>()
                  orderby c.CustomerID
                  select new { First = (c.Orders ?? new List<Order>()).FirstOrDefault() },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop(bool async)
    {
        var orderIds = new[] { 10643, 10692, 10702, 10835, 10952, 11011 };

        return AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("A"))
                  orderby c.CustomerID
                  select new { c.Orders.Where(e => orderIds.Contains(e.OrderID)).FirstOrDefault().Customer },
            ss => from c in ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("A"))
                  orderby c.CustomerID
                  select new
                  {
#pragma warning disable RCS1146 // Use conditional access.
                      Customer = c.Orders != null && c.Orders.Where(e => orderIds.Contains(e.OrderID)).Any()
#pragma warning restore RCS1146 // Use conditional access.
                          ? c.Orders.Where(e => orderIds.Contains(e.OrderID)).First().Customer
                          : null
                  },
            assertOrder: true,
            elementAsserter: (e, a) => AssertEqual(e.Customer, a.Customer));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("A"))
                .Select(c => ss.Set<Order>().FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_single_or_default_then_nav_prop_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("A"))
                .Select(c => ss.Set<Order>().SingleOrDefault(o => o.OrderID == 10643).Customer.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("A"))
                .Select(
                    c => EF.Property<string>(
                        EF.Property<Customer>(
                            ss.Set<Order>().FirstOrDefault(oo => oo.CustomerID == "ALFKI"),
                            "Customer"),
                        "City")),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                .Select(
                    c => ss.Set<Order>().FirstOrDefault(o => o.CustomerID == "ALFKI").Customer != null
                        ? ss.Set<Order>().FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City
                        : null)
        );

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(e => e.CustomerID.StartsWith("A"))
                .Select(c => ss.Set<Order>().OrderBy(o => o.CustomerID).FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_fk_based_inside_contains(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where new[] { "ALFKI" }.Contains(o.Customer.CustomerID)
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_inside_contains(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where new[] { "Novigrad", "Seattle" }.Contains(o.Customer.City)
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_inside_contains_nested(bool async)
        => AssertQuery(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  where new[] { "Novigrad", "Seattle" }.Contains(od.Order.Customer.City)
                  select od);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_from_join_clause_inside_contains(bool async)
        => AssertQuery(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  join o in ss.Set<Order>() on od.OrderID equals o.OrderID
                  where new[] { "USA", "Redania" }.Contains(o.Customer.Country)
                  select od);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_subquery_on_navigation(bool async)
        // Complex entity equality. Issue #15260.
        => Assert.Equal(
            CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported("Equals", nameof(OrderDetail)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => from p in ss.Set<Product>()
                          where p.OrderDetails.Contains(
                              ss.Set<OrderDetail>().OrderByDescending(o => o.OrderID).ThenBy(o => o.ProductID)
                                  .FirstOrDefault(orderDetail => orderDetail.Quantity == 1))
                          select p))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_subquery_on_navigation2(bool async)
        // Complex entity equality. Issue #15260.
        => Assert.Equal(
            CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported("Equals", nameof(OrderDetail)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => AssertQuery(
                    async,
                    ss => from p in ss.Set<Product>()
                          where p.OrderDetails.Contains(
                              ss.Set<OrderDetail>().OrderByDescending(o => o.OrderID).ThenBy(o => o.ProductID).FirstOrDefault())
                          select p))).Message);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_on_navigation_client_eval(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  orderby c.CustomerID
                  where c.Orders.Select(o => o.OrderID).Contains(
                      ss.Set<Order>().OrderByDescending(o => ClientMethod(o.OrderID)).Select(o => o.OrderID).FirstOrDefault())
                  select c);

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private static int ClientMethod(int argument)
        => argument;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_in_subquery_referencing_outer_query(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  // ReSharper disable once UseMethodAny.0
                  where (from od in ss.Set<OrderDetail>()
                         where o.Customer.Country == od.Order.Customer.Country
                         select od).Count()
                      > 0
                  where o.OrderID == 10643 || o.OrderID == 10692
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_in_subquery_referencing_outer_query_with_client_side_result_operator_and_count(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  where o.OrderID == 10643 || o.OrderID == 10692
                  // ReSharper disable once UseMethodAny.0
                  where (from od in ss.Set<OrderDetail>()
                         where o.Customer.Country == od.Order.Customer.Country
                         select od).Distinct().Count()
                      > 0
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_scalar_value_subquery_is_properly_inlined(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => (int?)o.OrderID).FirstOrDefault() },
            ss => from c in ss.Set<Customer>()
                  select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => (int?)o.OrderID).FirstOrDefault() },
            elementSorter: e => e.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_entity_value_subquery_works(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  where c.CustomerID.StartsWith("A")
                  orderby c.CustomerID
                  select new { c.CustomerID, Order = c.Orders.OrderBy(o => o.OrderID).FirstOrDefault() },
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_scalar_value_subquery_in_query_with_optional_navigation_works(bool async)
        => AssertQuery(
            async,
            ss => (from o in ss.Set<Order>()
                   orderby o.OrderID
                   select new
                   {
                       o.OrderID,
                       OrderDetail = o.OrderDetails.OrderBy(od => od.OrderID).ThenBy(od => od.ProductID).Select(od => od.OrderID)
                           .FirstOrDefault(),
                       o.Customer.City
                   }).Take(3),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_with_complex_subquery_and_LOJ_gets_flattened(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  join subquery in
                      from od in ss.Set<OrderDetail>()
                      join o in ss.Set<Order>() on od.OrderID equals 10260
                      join c2 in ss.Set<Customer>() on o.CustomerID equals c2.CustomerID
                      select c2
                      on c.CustomerID equals subquery.CustomerID into result
                  from subquery in result.DefaultIfEmpty()
                  select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_with_complex_subquery_and_LOJ_gets_flattened2(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  join subquery in
                      from od in ss.Set<OrderDetail>()
                      join o in ss.Set<Order>() on od.OrderID equals 10260
                      join c2 in ss.Set<Customer>() on o.CustomerID equals c2.CustomerID
                      select c2
                      on c.CustomerID equals subquery.CustomerID into result
                  from subquery in result.DefaultIfEmpty()
                  select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Navigation_with_collection_with_nullable_type_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.Customer.Orders.Count(oo => oo.OrderID > 10260) > 30));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_include_with_multiple_optional_navigations(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>()
                .Include(od => od.Order.Customer)
                .Include(od => od.Product)
                .Where(od => od.Order.Customer.City == "London"));

    private class OrderDTO
    {
        public Customer Customer { get; set; }
    }
}
