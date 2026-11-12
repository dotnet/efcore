// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindSelectQueryRelationalTestBase<TFixture>(TFixture fixture) : NorthwindSelectQueryTestBase<TFixture>(fixture)
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
        => AssertTranslationFailed(() => base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(async));

    public override Task Reverse_without_explicit_ordering(bool async)
        => AssertTranslationFailedWithDetails(
            () => base.Reverse_without_explicit_ordering(async), RelationalStrings.MissingOrderingInSelectExpression);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_over_outer_collection_in_nested_select(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A"))
                  select new
                  {
                      c.CustomerID,
                      Orders = (from o in c.Orders
                                let previous = c.Orders
                                    .Where(x => x.OrderID < o.OrderID)
                                    .OrderByDescending(x => x.OrderID)
                                    .FirstOrDefault()
                                select new
                                {
                                    o.OrderID,
                                    PreviousOrderID = previous == null ? (int?)null : previous.OrderID,
                                    PreviousOrderDate = previous == null ? null : previous.OrderDate
                                }).ToList()
                  },
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertCollection(e.Orders, a.Orders, elementSorter: o => o.OrderID);
            });

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_over_grouping_key(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => c.City)
                .Select(g => new
                {
                    g.Key,
                    Count = g.Count(),
                    OrderDate = ss.Set<Order>().Where(o => o.Customer!.City == g.Key)
                        .OrderBy(o => o.OrderID).Select(o => o.OrderDate).FirstOrDefault(),
                    EmployeeID = ss.Set<Order>().Where(o => o.Customer!.City == g.Key)
                        .OrderBy(o => o.OrderID).Select(o => o.EmployeeID).FirstOrDefault()
                }),
            elementSorter: e => e.Key);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_whose_predicate_repeats_a_subquery_lifts_to_single_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                OrderDate = c.Orders
                    .Where(o => !string.IsNullOrEmpty(
                        ss.Set<Employee>().Where(e => e.EmployeeID == o.EmployeeID).Select(e => e.City).FirstOrDefault()))
                    .OrderBy(o => o.OrderID)
                    .Select(o => o.OrderDate)
                    .FirstOrDefault(),
                EmployeeID = c.Orders
                    .Where(o => !string.IsNullOrEmpty(
                        ss.Set<Employee>().Where(e => e.EmployeeID == o.EmployeeID).Select(e => e.City).FirstOrDefault()))
                    .OrderBy(o => o.OrderID)
                    .Select(o => o.EmployeeID)
                    .FirstOrDefault()
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_in_orderings_lifts_to_single_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .OrderBy(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderDate).FirstOrDefault())
                .ThenBy(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.EmployeeID).FirstOrDefault())
                .ThenByDescending(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderDate).FirstOrDefault())
                .ThenBy(c => c.CustomerID)
                .Select(c => new { c.CustomerID, OrderDate = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderDate).FirstOrDefault() }),
            assertOrder: true);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_uncorrelated_single_result_subquery_is_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                OrderDate = ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.OrderDate).FirstOrDefault(),
                EmployeeID = ss.Set<Order>().OrderBy(o => o.OrderID).Select(o => o.EmployeeID).FirstOrDefault()
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Single_result_subqueries_reading_different_rows_are_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                First = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderDate).FirstOrDefault(),
                FirstFiltered = c.Orders.Where(o => o.EmployeeID != 6).OrderBy(o => o.OrderID).Select(o => o.EmployeeID).FirstOrDefault(),
                Second = c.Orders.OrderBy(o => o.OrderID).Skip(1).Select(o => o.EmployeeID).FirstOrDefault()
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Single_result_subqueries_grouping_by_different_keys_are_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                ByEmployee = ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).GroupBy(o => o.EmployeeID)
                    .OrderByDescending(g => g.Max<Order, int>(o => o.OrderID)).Select(g => g.Count()).FirstOrDefault(),
                ByDate = ss.Set<Order>().Where(o => o.CustomerID == c.CustomerID).GroupBy(o => o.OrderDate)
                    .OrderByDescending(g => g.Max<Order, int>(o => o.OrderID)).Select(g => g.Count()).FirstOrDefault()
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Aggregate_subqueries_over_one_collection_are_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                Count = c.Orders.Count(),
                LastOrderID = c.Orders.Max(o => (int?)o.OrderID)
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_inside_exists_is_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                HasMatch = ss.Set<Order>().Any(o
                    => o.OrderDetails.Where(d => d.ProductID > c.CustomerID.Length).OrderBy(d => d.ProductID).Select(d => (int)d.Quantity)
                        .FirstOrDefault()
                    == o.OrderDetails.Where(d => d.ProductID > c.CustomerID.Length).OrderBy(d => d.ProductID).Select(d => d.ProductID)
                        .FirstOrDefault())
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_guarded_by_case_is_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                OrderDate = c.CustomerID != "ALFKI"
                    ? c.Orders.Where(o => o.OrderID / (o.OrderID - 10643) != 0).OrderBy(o => o.OrderID).Select(o => o.OrderDate)
                        .FirstOrDefault()
                    : null,
                EmployeeID = c.CustomerID != "ALFKI"
                    ? c.Orders.Where(o => o.OrderID / (o.OrderID - 10643) != 0).OrderBy(o => o.OrderID).Select(o => o.EmployeeID)
                        .FirstOrDefault()
                    : null
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_guarded_by_and_is_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                HasOrderDate = c.CustomerID != "ALFKI"
                    && c.Orders.Where(o => o.OrderID % 2 == 0).OrderBy(o => o.OrderID).Select(o => o.OrderDate).FirstOrDefault() != null,
                HasEmployeeID = c.CustomerID != "ALFKI"
                    && c.Orders.Where(o => o.OrderID % 2 == 0).OrderBy(o => o.OrderID).Select(o => o.EmployeeID).FirstOrDefault() != null
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_guarded_by_in_is_not_lifted(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10252).Select(o => new
            {
                o.OrderID,
                Matches = new[]
                {
                    10248,
                    o.OrderDetails.Where(d => d.ProductID % 2 == 0).OrderBy(d => d.ProductID).Select(d => d.OrderID).FirstOrDefault(),
                    o.OrderDetails.Where(d => d.ProductID % 2 == 0).OrderBy(d => d.ProductID).Select(d => d.ProductID).FirstOrDefault()
                }.Contains(o.OrderID)
            }),
            elementSorter: e => e.OrderID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_reading_repeated_single_result_subqueries_lifts_both_to_single_joins(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Select(c => new
            {
                c.CustomerID,
                UnitPrice = c.Orders.OrderBy(o => o.OrderID)
                    .Select(o => o.OrderDetails.OrderBy(d => d.ProductID).Select(d => (decimal?)d.UnitPrice).FirstOrDefault())
                    .FirstOrDefault(),
                Quantity = c.Orders.OrderBy(o => o.OrderID)
                    .Select(o => o.OrderDetails.OrderBy(d => d.ProductID).Select(d => (short?)d.Quantity).FirstOrDefault())
                    .FirstOrDefault()
            }),
            elementSorter: e => e.CustomerID);

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Repeated_single_result_subquery_in_split_query_collection_lifts_to_single_join(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).AsSplitQuery().Select(c => new
            {
                c.CustomerID,
                Orders = c.Orders.Select(o => new
                {
                    o.OrderID,
                    UnitPrice = o.OrderDetails.OrderBy(d => d.ProductID).Select(d => (decimal?)d.UnitPrice).FirstOrDefault(),
                    Quantity = o.OrderDetails.OrderBy(d => d.ProductID).Select(d => (short?)d.Quantity).FirstOrDefault()
                }).ToList()
            }),
            elementSorter: e => e.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertCollection(e.Orders, a.Orders, elementSorter: o => o.OrderID);
            });
}
