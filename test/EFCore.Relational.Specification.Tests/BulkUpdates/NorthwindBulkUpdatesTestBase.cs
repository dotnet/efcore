// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class NorthwindBulkUpdatesTestBase<TFixture> : BulkUpdatesTestBase<TFixture>
    where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
{
    protected NorthwindBulkUpdatesTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_delete(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_delete_parameter(bool async)
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
    public virtual Task Where_delete_OrderBy(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID),
            rowsAffectedCount: 140);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_delete_OrderBy_Skip(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Skip(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_delete_OrderBy_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Take(100),
            rowsAffectedCount: 100);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_delete_OrderBy_Skip_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).OrderBy(e => e.OrderID).Skip(100).Take(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_delete_Skip(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100),
            rowsAffectedCount: 40);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_delete_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Take(100),
            rowsAffectedCount: 100);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_delete_Skip_Take(bool async)
        => AssertDelete(
            async,
            ss => ss.Set<OrderDetail>().Where(e => e.OrderID < 10300).Skip(100).Take(100),
            rowsAffectedCount: 40);

    protected virtual void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
