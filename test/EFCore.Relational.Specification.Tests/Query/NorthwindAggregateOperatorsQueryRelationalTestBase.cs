// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindAggregateOperatorsQueryRelationalTestBase<TFixture>(TFixture fixture)
    : NorthwindAggregateOperatorsQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    public override async Task Last_when_no_order_by(bool async)
        => Assert.Equal(
            RelationalStrings.LastUsedWithoutOrderBy(nameof(Enumerable.Last)),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Last_when_no_order_by(async))).Message);

    public override async Task LastOrDefault_when_no_order_by(bool async)
        => Assert.Equal(
            RelationalStrings.LastUsedWithoutOrderBy(nameof(Enumerable.LastOrDefault)),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.LastOrDefault_when_no_order_by(async))).Message);

    public override async Task Contains_over_keyless_entity_throws(bool async)
        => Assert.Equal(
            CoreStrings.EntityEqualityOnKeylessEntityNotSupported("Equals", nameof(CustomerQuery)),
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Contains_over_keyless_entity_throws(async))).Message);

    public override async Task Min_no_data_subquery(bool async)
        => Assert.Equal(
            "Cannot read the Value property of a Nullable object that has no value. Check HasValue before reading Value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Min_no_data_subquery(async))).Message);

    public override async Task Max_no_data_subquery(bool async)
        => Assert.Equal(
            "Cannot read the Value property of a Nullable object that has no value. Check HasValue before reading Value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Max_no_data_subquery(async))).Message);

    public override async Task MinBy_no_data_subquery_value_type(bool async)
        => Assert.Equal(
            "Cannot read the Value property of a Nullable object that has no value. Check HasValue before reading Value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.MinBy_no_data_subquery_value_type(async))).Message);

    public override async Task MaxBy_no_data_subquery_value_type(bool async)
        => Assert.Equal(
            "Cannot read the Value property of a Nullable object that has no value. Check HasValue before reading Value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.MaxBy_no_data_subquery_value_type(async))).Message);

    public override async Task Average_no_data_subquery(bool async)
        => Assert.Equal(
            "Cannot read the Value property of a Nullable object that has no value. Check HasValue before reading Value.",
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Average_no_data_subquery(async))).Message);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_expression_with_outer_reference(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Select(o => new { o.OrderID, Total = o.OrderDetails.Sum(od => od.ProductID * o.OrderID) }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_members_of_single_result_subquery_with_outer_reference(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  let projected = from o in c.Orders
                                  let d = o.OrderDetails
                                      .OrderBy(od => od.ProductID)
                                      .Select(od => new { od.ProductID, od.OrderID })
                                      .FirstOrDefault()
                                  select new
                                  {
                                      Products = d!.ProductID * c.CustomerID.Length,
                                      Orders = d.OrderID * c.CustomerID.Length
                                  }
                  select new
                  {
                      c.CustomerID,
                      TotalProducts = projected.Sum(x => x.Products),
                      TotalOrders = projected.Sum(x => x.Orders)
                  },
            elementSorter: e => e.CustomerID);
}
