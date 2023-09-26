// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindKeylessEntitiesQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindKeylessEntitiesQueryTestBase(TFixture fixture)
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
    public virtual Task KeylessEntity_simple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerQuery>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_where_simple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerQuery>().Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_by_database_view(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<ProductView>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Auto_initialized_view_set(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerQuery>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_with_nav_defining_query(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerQueryWithQueryFilter>().Where(cq => cq.OrderCount > 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_with_defining_query(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderQuery>().Where(ov => ov.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_with_defining_query_and_correlated_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderQuery>().Where(ov => ov.CustomerID == "ALFKI").Select(ov => ov.Customer)
                .OrderBy(c => c.CustomerID)
                .Select(cv => cv.Orders.Where(cc => true).ToList()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_with_mixed_tracking(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  from o in ss.Set<OrderQuery>().Where(ov => ov.CustomerID == c.CustomerID)
                  select new { c, o },
            e => e.c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_with_included_nav(bool async)
        => AssertQuery(
            async,
            ss => from ov in ss.Set<OrderQuery>().Include(ov => ov.Customer)
                  where ov.CustomerID == "ALFKI"
                  select ov,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<OrderQuery>(ov => ov.Customer)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_with_included_navs_multi_level(bool async)
        => AssertQuery(
            async,
            ss => from ov in ss.Set<OrderQuery>().Include(ov => ov.Customer.Orders)
                  where ov.CustomerID == "ALFKI"
                  select ov,
            elementAsserter: (e, a) => AssertInclude(
                e, a,
                new ExpectedInclude<OrderQuery>(ov => ov.Customer),
                new ExpectedInclude<Customer>(c => c.Orders, "Customer")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_select_where_navigation(bool async)
        => AssertQuery(
            async,
            ss => from ov in ss.Set<OrderQuery>()
                  where ov.Customer.City == "Seattle"
                  select ov);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_select_where_navigation_multi_level(bool async)
        => AssertQuery(
            async,
            ss => from ov in ss.Set<OrderQuery>()
                  where ov.Customer.Orders.Any()
                  select ov);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task KeylessEntity_groupby(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerQuery>()
                .GroupBy(cv => cv.City)
                .Select(
                    g => new
                    {
                        g.Key,
                        Count = g.Count(),
                        Sum = g.Sum(e => e.Address.Length)
                    }),
            elementSorter: e => (e.Key, e.Count, e.Sum));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_mapped_to_view_on_right_side_of_join(bool async)
        => AssertQuery(
            async,
            ss => from o in ss.Set<Order>()
                  join pv in ss.Set<ProductView>() on o.CustomerID equals pv.CategoryName into grouping
                  from pv in grouping.DefaultIfEmpty()
                  select new { Order = o, ProductView = pv },
            elementSorter: e => (e.Order.OrderID, e.ProductView?.ProductID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_correlated_with_keyless_entity_in_predicate_works(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<CustomerQuery>()
                .Where(cq => ss.Set<Customer>().Where(c => c.City == cq.City).Any())
                .Select(pv => new { pv.City, pv.ContactName })
                .OrderBy(x => x.ContactName)
                .Take(2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_over_keyless_entity(bool async)
        => AssertCount(
            async,
            ss => ss.Set<CustomerQuery>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_over_keyless_entity_with_pushdown(bool async)
        => AssertCount(
            async,
            ss => ss.Set<CustomerQuery>().OrderBy(x => x.ContactTitle).Take(10));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_over_keyless_entity_with_pushdown_empty_projection(bool async)
        => AssertCount(
            async,
            ss => ss.Set<CustomerQuery>().Take(10));
}
