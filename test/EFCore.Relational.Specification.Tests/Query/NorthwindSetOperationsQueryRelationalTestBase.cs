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
}
