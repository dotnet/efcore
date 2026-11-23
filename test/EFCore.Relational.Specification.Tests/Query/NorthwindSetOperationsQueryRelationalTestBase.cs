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
    public virtual Task Append(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.City == "Berlin" || c.City == "Madrid")
                .Select(c => c.CustomerID)
                .Append("ZZZZZ"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Prepend(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.City == "Berlin" || c.City == "Madrid")
                .Select(c => c.CustomerID)
                .Prepend("AAAAA"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Append_and_Prepend_followed_by_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.City == "Berlin" || c.City == "Madrid")
                .Select(c => c.CustomerID)
                .Append("ZZZZZ")
                .Prepend("AAAAA")
                .OrderByDescending(id => id),
            assertOrder: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Append_followed_by_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.City == "Berlin")
                .Select(c => c.CustomerID)
                .Append("ZZZZZ"));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Append_on_collection_navigation_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("F"))
                .Select(c => new { c.CustomerID, MaxOrderId = c.Orders.Select(o => o.OrderID).Append(0).Max() }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Append_parameter_on_collection_navigation_in_predicate(bool async)
    {
        var orderId = 10300;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Select(o => o.OrderID).Append(orderId).Max() == orderId));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Prepend_column_on_collection_navigation_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderID < 10260)
                .Select(o => new { o.OrderID, MaxId = o.OrderDetails.Select(od => od.ProductID).Prepend(o.OrderID).Max() }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Append_entity(bool async)
        => AssertTranslationFailed(() => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "Berlin").Append(new Customer { CustomerID = "ZZZZZ" })));
}
