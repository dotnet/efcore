// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindSetOperationsQueryRelationalTestBase<TFixture>(TFixture fixture)
    : NorthwindSetOperationsQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    public override async Task Collection_projection_after_set_operation_fails_if_distinct(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Collection_projection_after_set_operation_fails_if_distinct(async))).Message;

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, message);
    }

    public override async Task Collection_projection_before_set_operation_fails(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(()
            => base.Collection_projection_before_set_operation_fails(async))).Message;

        Assert.Equal(RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation, message);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Set_operations_over_null_checked_to_one_non_entity_subquery(bool async)
    {
        await AssertQuery(
            async,
            ss =>
            {
                var withOrders =
                    from c in ss.Set<Customer>()
                    let latest = c.Orders
                        .OrderByDescending(o => o.OrderDate)
                        .ThenByDescending(o => o.OrderID)
                        .Select(o => new { o.OrderID, o.OrderDate })
                        .FirstOrDefault()
                    select new
                    {
                        c.CustomerID,
                        c.City,
                        LatestOrderID = latest != null ? latest.OrderID : 0,
                        LatestOrderDate = latest != null ? latest.OrderDate : null
                    };

                var neverOrdered = ss.Set<Customer>()
                    .Where(c => !c.Orders.Any())
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.City,
                        LatestOrderID = 0,
                        LatestOrderDate = (DateTime?)null
                    });

                return withOrders.Concat(neverOrdered);
            },
            elementSorter: e => (e.CustomerID, e.LatestOrderID));

        await AssertQuery(
            async,
            ss =>
            {
                var withOrders =
                    from c in ss.Set<Customer>()
                    let latest = c.Orders
                        .OrderByDescending(o => o.OrderDate)
                        .ThenByDescending(o => o.OrderID)
                        .Select(o => new { o.OrderID, o.OrderDate })
                        .FirstOrDefault()
                    select new
                    {
                        c.CustomerID,
                        c.City,
                        LatestOrderID = latest == null ? 0 : latest.OrderID,
                        LatestOrderDate = latest == null ? null : latest.OrderDate
                    };

                var neverOrdered = ss.Set<Customer>()
                    .Where(c => !c.Orders.Any())
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.City,
                        LatestOrderID = 0,
                        LatestOrderDate = (DateTime?)null
                    });

                return neverOrdered.Union(withOrders);
            },
            elementSorter: e => (e.CustomerID, e.LatestOrderID));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_anonymous_member_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(o => new { o })
                .Distinct()
                .Select(x => new { x.o.OrderID, x.o.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_anonymous_member_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10260).Select(o => new { o })
                .Union(ss.Set<Order>().Where(o => o.OrderID > 11070).Select(o => new { o }))
                .Select(x => new { x.o.OrderID, x.o.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_dto_member_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10260).Select(o => new OrderHolder { Order = o })
                .Union(ss.Set<Order>().Where(o => o.OrderID > 11070).Select(o => new OrderHolder { Order = o }))
                .Select(x => new { x.Order.OrderID, x.Order.Customer!.City }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_optional_entity_in_second_operand_required_navigation(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250).Select(od => new { od })
                .Concat(
                    from o in ss.Set<Order>().Where(o => o.OrderID < 10252)
                    join od in ss.Set<OrderDetail>().Where(od => od.Quantity > 1000) on o.OrderID equals od.OrderID into ods
                    from od in ods.DefaultIfEmpty()
                    select new { od })
                .Select(x => x.od == null ? -1 : x.od.Order.OrderID));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Concat_optional_entity_in_second_operand_required_navigation_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                .Concat(
                    from o in ss.Set<Order>().Where(o => o.OrderID < 10252)
                    join od in ss.Set<OrderDetail>().Where(od => od.Quantity > 1000) on o.OrderID equals od.OrderID into ods
                    from od in ods.DefaultIfEmpty()
                    select od)
                .Select(od => od == null ? -1 : od.Order.OrderID));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Union_dto_with_members_in_different_order(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10250).Select(o => new OrderHolder { Order = o, Key = o.CustomerID })
                .Union(ss.Set<Order>().Where(o => o.OrderID > 11075).Select(o => new OrderHolder { Key = o.CustomerID, Order = o }))
                .Select(x => x.Key),
            elementSorter: e => e);

    protected class OrderHolder
    {
        public Order Order { get; set; } = null!;
        public string? Key { get; set; }

        public override bool Equals(object? obj)
            => obj is OrderHolder other && other.Order.OrderID == Order.OrderID && other.Key == Key;

        public override int GetHashCode()
            => HashCode.Combine(Order.OrderID, Key);
    }
}
