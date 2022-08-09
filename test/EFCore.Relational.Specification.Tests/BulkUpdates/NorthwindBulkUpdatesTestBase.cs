// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class NorthwindBulkUpdatesTestBase<TFixture> : BulkUpdatesTestBase<TFixture>
    where TFixture : NorthwindBulkUpdatesFixture<NoopModelCustomizer>, new()
{
    protected NorthwindBulkUpdatesTestBase(TFixture fixture)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_Where_parameter(bool async)
    {
        int? quantity = 1;

        await AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.Quantity == quantity),
            rowsAffectedCount: 17);

        quantity = null;

        await AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.Quantity == quantity),
            rowsAffectedCount: 0);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy_Skip(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Skip(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Take(100),
            rowsAffectedCount: 100);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_OrderBy_Skip_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Skip(100).Take(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Skip(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Take(100),
            rowsAffectedCount: 100);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Skip_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100).Take(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_predicate_with_group_by_aggregate(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(e => e.OrderID < ss.Set<Order>()
                                        .GroupBy(o => o.CustomerID)
                                        .Where(g => g.Count() > 11)
                                        .Select(g => g.First()).First().OrderID),
            rowsAffectedCount: 284);

    [ConditionalTheory(Skip = "Issue#28524")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_predicate_with_group_by_aggregate_2(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>()
                .Where(e => ss.Set<Order>()
                            .GroupBy(o => o.CustomerID)
                            .Where(g => g.Count() > 9)
                            .Select(g => g.First()).Contains(e.Order)),
            rowsAffectedCount: 40);

    [ConditionalTheory(Skip = "Issue#28525")]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_GroupBy_Where_Select(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>()
                    .GroupBy(od => od.OrderID)
                    .Where(g => g.Count() > 5)
                    .Select(g => g.First()),
            rowsAffectedCount: 284);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Skip_Take_Skip_Take_causing_subquery(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100).Take(100).Skip(20).Take(5),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_Distinct(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Distinct(),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_SelectMany(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Order>().Where(e => e.OrderID < 10250).SelectMany(e => e.OrderDetails),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_SelectMany_subquery(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<Order>().Where(e => e.OrderID < 10250).SelectMany(e => e.OrderDetails.Where(i => i.ProductID > 0)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_using_navigation(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.Order.OrderDate.Value.Year == 2000),
            rowsAffectedCount: 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Where_using_navigation_2(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.Order.Customer.CustomerID.StartsWith("F")),
            rowsAffectedCount: 164);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Union(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                    .Union(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Concat(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                    .Concat(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Intersect(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                    .Intersect(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_Except(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                    .Except(ss.Set<OrderDetail>().Where(od => od.OrderID > 11250)),
            rowsAffectedCount: 5);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_non_entity_projection(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnNonEntityType("ExecuteDelete"),
            () => AssertDelete(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250).Select(e => e.ProductID),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_non_entity_projection_2(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnNonEntityType("ExecuteDelete"),
            () => AssertDelete(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                .Select(e => new OrderDetail { OrderID = e.OrderID, ProductID = e.ProductID }),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_non_entity_projection_3(bool async)
        => AssertTranslationFailed(
            RelationalStrings.ExecuteOperationOnNonEntityType("ExecuteDelete"),
            () => AssertDelete(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.OrderID < 10250)
                .Select(e => new { OrderDetail = e, ProductID = e.ProductID }),
                rowsAffectedCount: 0));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_FromSql_converted_to_subquery(bool async)
    {
        if (async)
        {
            await TestHelpers.ExecuteWithStrategyInTransactionAsync(
                () => Fixture.CreateContext(),
                (DatabaseFacade facade, IDbContextTransaction transaction) => Fixture.UseTransaction(facade, transaction),
                async context => await context.Set<OrderDetail>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [OrderID], [ProductID], [UnitPrice], [Quantity], [Discount]
FROM [Order Details]
WHERE [OrderID] < 10300"))
                    .ExecuteDeleteAsync());
        }
        else
        {
            TestHelpers.ExecuteWithStrategyInTransaction(
                () => Fixture.CreateContext(),
                (DatabaseFacade facade, IDbContextTransaction transaction) => Fixture.UseTransaction(facade, transaction),
                context => context.Set<OrderDetail>().FromSqlRaw(
                    NormalizeDelimitersInRawString(
                        @"SELECT [OrderID], [ProductID], [UnitPrice], [Quantity], [Discount]
FROM [Order Details]
WHERE [OrderID] < 10300"))
                    .ExecuteDelete());
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_join(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>()
                  join o in ss.Set<Order>().Where(o => o.OrderID < 10300).OrderBy(e => e.OrderID).Skip(0).Take(100)
                    on od.OrderID equals o.OrderID
                  select od,
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_left_join(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  join o in ss.Set<Order>().Where(o => o.OrderID < 10300).OrderBy(e => e.OrderID).Skip(0).Take(100)
                    on od.OrderID equals o.OrderID into grouping
                  from o in grouping.DefaultIfEmpty()
                  select od,
            rowsAffectedCount: 74);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_cross_join(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  from o in ss.Set<Order>().Where(o => o.OrderID < 10300).OrderBy(e => e.OrderID).Skip(0).Take(100)
                  select od,
            rowsAffectedCount: 74);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_cross_apply(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  from o in ss.Set<Order>().Where(o => o.OrderID < od.OrderID).OrderBy(e => e.OrderID).Skip(0).Take(100)
                  select od,
            rowsAffectedCount: 71);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Delete_with_outer_apply(bool async)
        => AssertDelete(
            async,
            ss => from od in ss.Set<OrderDetail>().Where(e => e.OrderID < 10276)
                  from o in ss.Set<Order>().Where(o => o.OrderID < od.OrderID).OrderBy(e => e.OrderID).Skip(0).Take(100).DefaultIfEmpty()
                  select od,
            rowsAffectedCount: 74);

    protected string NormalizeDelimitersInRawString(string sql)
        => Fixture.TestStore.NormalizeDelimitersInRawString(sql);

    protected virtual void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
