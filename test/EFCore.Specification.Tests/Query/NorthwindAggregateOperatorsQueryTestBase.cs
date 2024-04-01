// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindAggregateOperatorsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindAggregateOperatorsQueryTestBase(TFixture fixture)
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
    public virtual Task Select_All(bool async)
        => AssertAll(
            async,
            ss => ss.Set<Order>()
                .Select(o => new ProjectedType { Order = o.OrderID, Customer = o.CustomerID }),
            predicate: p => p.Customer == "ALFKI");

    private class ProjectedType
    {
        public int Order { get; set; }
        public string Customer { get; set; }

        private bool Equals(ProjectedType other)
            => Equals(Order, other.Order);

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                ? true
                : obj.GetType() == GetType()
                && Equals((ProjectedType)obj);
        }

        public override int GetHashCode()
            => Order.GetHashCode();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_no_arg(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_no_data_cast_to_nullable(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 0).Select(o => (int?)o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_no_data_nullable(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Product>(),
            selector: o => o.SupplierID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_binary_expression(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID * 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_no_arg_empty(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 42).Select(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_arg(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>(),
            selector: o => o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_arg_expression(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>(),
            selector: o => o.OrderID + o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_division_on_decimal(bool async)
        => AssertSum(
            async,
            ss => ss.Set<OrderDetail>(),
            selector: od => od.Quantity / 2.09m,
            asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_division_on_decimal_no_significant_digits(bool async)
        => AssertSum(
            async,
            ss => ss.Set<OrderDetail>(),
            selector: od => od.Quantity / 2m,
            asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_with_coalesce(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Product>().Where(p => p.ProductID < 40),
            selector: p => p.UnitPrice ?? 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_subquery_is_client_eval(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Customer>(),
            selector: c => c.Orders.Sum(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_nested_subquery_is_client_eval(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Customer>(),
            selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_min_subquery_is_client_eval(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Customer>(),
            selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Min(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_on_float_column(bool async)
        => AssertSum(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.ProductID == 1),
            selector: od => od.Discount);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_on_float_column_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10300).Select(
                o => new { o.OrderID, Sum = o.OrderDetails.Sum(od => od.Discount) }),
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_no_arg(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_binary_expression(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID * 2));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_arg(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>(),
            selector: o => o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_arg_expression(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>(),
            selector: o => o.OrderID + o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_division_on_decimal(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<OrderDetail>(),
            selector: od => od.Quantity / 2.09m,
            asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_division_on_decimal_no_significant_digits(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<OrderDetail>(),
            selector: od => od.Quantity / 2m,
            asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_coalesce(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Product>().Where(p => p.ProductID < 40),
            selector: p => p.UnitPrice ?? 0,
            asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_over_subquery_is_client_eval(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Customer>(),
            selector: c => c.Orders.Sum(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_over_nested_subquery_is_client_eval(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Average(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_over_max_subquery_is_client_eval(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_on_float_column(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<OrderDetail>().Where(od => od.ProductID == 1),
            selector: od => od.Discount);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_on_float_column_in_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10300).Select(
                o => new { o.OrderID, Sum = o.OrderDetails.Average(od => od.Discount) }),
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_on_float_column_in_subquery_with_cast(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                .Select(o => new { o.OrderID, Sum = o.OrderDetails.Average(od => (float?)od.Discount) }),
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_with_no_arg(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_with_arg(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>(),
            selector: o => o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_no_data(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertMin(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID == -1),
                selector: o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_no_data_nullable(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Product>().Where(o => o.SupplierID == -1),
            selector: o => o.SupplierID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_no_data_cast_to_nullable(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == -1),
            selector: o => (int?)o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_no_data_subquery(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.Where(o => o.OrderID == -1).Min(o => o.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_no_data(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertMax(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID == -1),
                selector: o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_no_data_nullable(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Product>().Where(o => o.SupplierID == -1),
            selector: o => o.SupplierID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_no_data_cast_to_nullable(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == -1),
            selector: o => (int?)o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_no_data_subquery(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.Where(o => o.OrderID == -1).Max(o => o.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_no_data(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            () => AssertAverage(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID == -1),
                selector: o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_no_data_nullable(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Product>().Where(o => o.SupplierID == -1),
            selector: o => o.SupplierID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_no_data_cast_to_nullable(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == -1),
            selector: o => (int?)o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_no_data_subquery(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.Where(o => o.OrderID == -1).Average(o => o.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_with_coalesce(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Product>().Where(p => p.ProductID < 40),
            selector: p => p.UnitPrice ?? 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_over_subquery_is_client_eval(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Customer>(),
            selector: c => c.Orders.Sum(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_over_nested_subquery_is_client_eval(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Min(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_over_max_subquery_is_client_eval(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Max(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_with_no_arg(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_with_arg(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>(),
            selector: o => o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_with_coalesce(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Product>().Where(p => p.ProductID < 40),
            selector: p => p.UnitPrice ?? 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_over_subquery_is_client_eval(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Customer>(),
            selector: c => c.Orders.Sum(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_over_nested_subquery_is_client_eval(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Max(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_over_sum_subquery_is_client_eval(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
            selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_with_no_predicate(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_with_predicate(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>(),
            predicate: o => o.CustomerID == "ALFKI");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_with_order_by(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_OrderBy_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Where_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Count_with_predicate(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID),
            predicate: o => o.CustomerID == "ALFKI");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Where_Count_with_predicate(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.OrderID > 10),
            predicate: o => o.CustomerID != "ALFKI");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_OrderBy_Count_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertCount(
                async,
                ss => ss.Set<Order>().Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless())));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Where_Count_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Where_Count_client_eval_mixed(bool async)
        => AssertTranslationFailed(
            () => AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Count_with_predicate_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()),
                predicate: o => ClientEvalPredicate(o)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Count_with_predicate_client_eval_mixed(bool async)
        => AssertTranslationFailed(
            () => AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID),
                predicate: o => ClientEvalPredicate(o)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Where_Count_with_predicate_client_eval(bool async)
        => AssertTranslationFailed(
            () => AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)),
                predicate: o => ClientEvalPredicate(o)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool async)
        => AssertTranslationFailed(
            () => AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)),
                predicate: o => o.CustomerID != "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_client_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>().OrderBy(o => ClientEvalSelectorStateless()).Take(10));

    protected static bool ClientEvalPredicate(Order order)
        => order.OrderID > 10000;

    private static int ClientEvalSelectorStateless()
        => 42;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_Scalar(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.City).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Distinct(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_OrderBy(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.Country).Distinct().OrderBy(c => c),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_OrderBy2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID),
            ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_OrderBy3(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID),
            ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID, StringComparer.Ordinal),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_Select_Distinct_Count(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().Select(c => c.City).Select(c => c).Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Single_Throws(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            async () => await AssertSingle(async, ss => ss.Set<Customer>()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Single_Predicate(bool async)
        => AssertSingle(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => c.CustomerID == "ALFKI");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Single(bool async)
        => AssertSingle(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SingleOrDefault_Throws(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await AssertSingleOrDefault(async, ss => ss.Set<Customer>()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SingleOrDefault_Predicate(bool async)
        => AssertSingle(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => c.CustomerID == "ALFKI");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_SingleOrDefault(bool async)
        => AssertSingleOrDefault(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First_Predicate(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
            predicate: c => c.City == "London");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_First(bool async)
        => AssertFirst(
            async,
            // ReSharper disable once ReplaceWithSingleCallToFirst
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_Predicate(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
            predicate: c => c.City == "London");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_FirstOrDefault(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").FirstOrDefault().CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID).Select(
                c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID).FirstOrDefault()),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID).Select(
                c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()
                    .Maybe(x => x.OrderDetails)
                    .Maybe(xx => xx.OrderBy(od => od.ProductID).FirstOrDefault())));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).OrderBy(c => c.CustomerID).Select(
                c => (int?)c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID).FirstOrDefault()
                    .ProductID),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).OrderBy(c => c.CustomerID).Select(
                c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()
                    .Maybe(x => x.OrderDetails)
                    .MaybeScalar(x => x.OrderBy(od => od.ProductID).FirstOrDefault().ProductID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task First_inside_subquery_gets_client_evaluated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").First().CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Last(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Last_when_no_order_by(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LastOrDefault_when_no_order_by(bool async)
        => AssertLastOrDefault(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Last_Predicate(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
            predicate: c => c.City == "London");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_Last(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LastOrDefault(bool async)
        => AssertLastOrDefault(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LastOrDefault_Predicate(bool async)
        => AssertLastOrDefault(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
            predicate: c => c.City == "London");

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_LastOrDefault(bool async)
        => AssertLastOrDefault(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ss.Set<Order>().Select(o => o.CustomerID).Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_array_closure(bool async)
    {
        var ids = new[] { "ABCDE", "ALFKI" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));

        ids = ["ABCDE"];

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_subquery_and_local_array_closure(bool async)
    {
        var ids = new[] { "London", "Buenos Aires" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)));

        ids = ["London"];

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_uint_array_closure(bool async)
    {
        var ids = new uint[] { 0, 1 };

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)));

        ids = [0];

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_nullable_uint_array_closure(bool async)
    {
        var ids = new uint?[] { 0, 1 };

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)));

        ids = [0];

        await AssertQuery(
            async,
            ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_array_inline(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_list_closure(bool async)
    {
        var ids = new List<string> { "ABCDE", "ALFKI" };
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_object_list_closure(bool async)
    {
        var ids = new List<object> { "ABCDE", "ALFKI" };
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(EF.Property<object>(c, nameof(Customer.CustomerID)))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_list_closure_all_null(bool async)
    {
        var ids = new List<string> { null, null };
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_list_inline(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<string> { "ABCDE", "ALFKI" }.Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_list_inline_closure_mix(bool async)
    {
        var id = "ALFKI";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)));

        id = "ANATR";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_enumerable_closure(bool async)
    {
        var ids = new[] { "ABCDE", "ALFKI" }.Where(e => e != null);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));

        ids = new[] { "ABCDE" }.Where(e => e != null);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_object_enumerable_closure(bool async)
    {
        var ids = new List<object> { "ABCDE", "ALFKI" }.Where(e => e != null);
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(EF.Property<object>(c, nameof(Customer.CustomerID)))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_enumerable_closure_all_null(bool async)
    {
        var ids = new List<string> { null, null }.Where(e => e != null);
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_enumerable_inline(bool async)
        => await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<string> { "ABCDE", "ALFKI" }.Where(e => e != null).Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_enumerable_inline_closure_mix(bool async)
    {
        var id = "ALFKI";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Where(e => e != null).Contains(c.CustomerID)));

        id = "ANATR";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Where(e => e != null).Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_ordered_enumerable_closure(bool async)
    {
        var ids = new[] { "ABCDE", "ALFKI" }.Order();

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));

        ids = new[] { "ABCDE" }.Order();

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_object_ordered_enumerable_closure(bool async)
    {
        var ids = new List<object> { "ABCDE", "ALFKI" }.Order();
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(EF.Property<object>(c, nameof(Customer.CustomerID)))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_ordered_enumerable_closure_all_null(bool async)
    {
        var ids = new List<string> { null, null }.Order();
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_ordered_enumerable_inline(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<string> { "ABCDE", "ALFKI" }.Order().Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_ordered_enumerable_inline_closure_mix(bool async)
    {
        var id = "ALFKI";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Order().Contains(c.CustomerID)));

        id = "ANATR";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Order().Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_read_only_collection_closure(bool async)
    {
        var ids = new[] { "ABCDE", "ALFKI" }.AsReadOnly();

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));

        ids = new[] { "ABCDE" }.AsReadOnly();

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_object_read_only_collection_closure(bool async)
    {
        var ids = new List<object> { "ABCDE", "ALFKI" }.AsReadOnly();
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(EF.Property<object>(c, nameof(Customer.CustomerID)))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_ordered_read_only_collection_all_null(bool async)
    {
        var ids = new List<string> { null, null }.AsReadOnly();
        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_read_only_collection_inline(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<string> { "ABCDE", "ALFKI" }.AsReadOnly().Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_read_only_collection_inline_closure_mix(bool async)
    {
        var id = "ALFKI";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.AsReadOnly().Contains(c.CustomerID)));

        id = "ANATR";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.AsReadOnly().Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool async)
    {
        var id = "ALFKI";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<Customer> { new() { CustomerID = "ABCDE" }, new() { CustomerID = id } }
                    .Select(i => i.CustomerID).Contains(c.CustomerID)));

        id = "ANATR";

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<Customer> { new() { CustomerID = "ABCDE" }, new() { CustomerID = id } }
                    .Select(i => i.CustomerID).Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_non_primitive_list_closure_mix(bool async)
    {
        var ids = new List<Customer> { new() { CustomerID = "ABCDE" }, new() { CustomerID = "ALFKI" } };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => ids.Select(i => i.CustomerID).Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_false(bool async)
    {
        string[] ids = ["ABCDE", "ALFKI"];

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => !ids.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_complex_predicate_and(bool async)
    {
        string[] ids = ["ABCDE", "ALFKI"];

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") && ids.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_complex_predicate_or(bool async)
    {
        string[] ids = ["ABCDE", "ALFKI"];

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool async)
    {
        string[] ids = ["ABCDE", "ALFKI"];

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") || !ids.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool async)
    {
        string[] ids = ["ABCDE", "ALFKI"];

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) && (c.CustomerID != "ALFKI" && c.CustomerID != "ABCDE")),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_sql_injection(bool async)
    {
        string[] ids = ["ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --"];

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_empty_closure(bool async)
    {
        string[] ids = [];

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
            assertEmpty: true);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_collection_empty_inline(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => !(new List<string>().Contains(c.CustomerID))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_top_level(bool async)
        => AssertSingleResult(
            async,
            syncQuery: ss => ss.Set<Customer>().Select(c => c.CustomerID).Contains("ALFKI"),
            asyncQuery: ss => ss.Set<Customer>().Select(c => c.CustomerID).ContainsAsync("ALFKI", default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_tuple_array_closure(bool async)
    {
        var ids = new[] { Tuple.Create(1, 2), Tuple.Create(10248, 11) };

        return AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(o => ids.Contains(new Tuple<int, int>(o.OrderID, o.ProductID))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_local_anonymous_type_array_closure(bool async)
    {
        var ids = new[] { new { Id1 = 1, Id2 = 2 }, new { Id1 = 10248, Id2 = 11 } };

        return AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(o => ids.Contains(new { Id1 = o.OrderID, Id2 = o.ProductID })));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfType_Select(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Order>()
                .OfType<Order>()
                .OrderBy(o => o.OrderID)
                .Select(o => o.Customer.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OfType_Select_OfType_Select(bool async)
        => AssertFirst(
            async,
            ss => ss.Set<Order>()
                .OfType<Order>()
                .Select(o => o)
                .OfType<Order>()
                .OrderBy(o => o.OrderID)
                .Select(o => o.Customer.City));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID.StartsWith("A"))
                .OrderBy(o => o.OrderID)
                .Select(o => (long)o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID.StartsWith("A"))
                .OrderBy(o => o.OrderID)
                .Select(o => (long)o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID.StartsWith("A"))
                .Select(o => (long)o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Take_Last_gives_correct_result(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(20));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OrderBy_Skip_Last_gives_correct_result(bool async)
        => AssertLast(
            async,
            ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(20));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
        => AssertSingleResult(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "VINET")
                .Contains(ss.Set<Order>().Single(o => o.OrderID == 10248)),
            ss => ss.Set<Order>()
                .Where(o => o.CustomerID == "VINET")
                .ContainsAsync(ss.Set<Order>().Single(o => o.OrderID == 10248), default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task List_Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
    {
        var someOrder = new Order { OrderID = 10248 };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.Orders.Contains(someOrder)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task List_Contains_with_constant_list(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<Customer> { new() { CustomerID = "ALFKI" }, new() { CustomerID = "ANATR" } }.Contains(c)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task List_Contains_with_parameter_list(bool async)
    {
        var customers = new List<Customer> { new() { CustomerID = "ALFKI" }, new() { CustomerID = "ANATR" } };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => customers.Contains(c)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_parameter_list_value_type_id(bool async)
    {
        var orders = new List<Order> { new() { OrderID = 10248 }, new() { OrderID = 10249 } };

        return AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => orders.Contains(o)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_with_constant_list_value_type_id(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => new List<Order> { new() { OrderID = 10248 }, new() { OrderID = 10249 } }.Contains(o)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IImmutableSet_Contains_with_parameter(bool async)
    {
        IImmutableSet<string> ids = ImmutableHashSet<string>.Empty.Add("ALFKI");

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IReadOnlySet_Contains_with_parameter(bool async)
    {
        IReadOnlySet<string> ids = new HashSet<string> { "ALFKI" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task HashSet_Contains_with_parameter(bool async)
    {
        var ids = new HashSet<string> { "ALFKI" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ImmutableHashSet_Contains_with_parameter(bool async)
    {
        var ids = ImmutableHashSet<string>.Empty.Add("ALFKI");

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
    }

    private static readonly IEnumerable<string> _customers = new[] { "ALFKI", "WRONG" };

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Array_cast_to_IEnumerable_Contains_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => _customers.Contains(c.CustomerID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_keyless_entity_throws(bool async)
        => AssertSingleResult(
            async,
            ss => ss.Set<CustomerQuery>().Contains(ss.Set<CustomerQuery>().First()),
            ss => ss.Set<CustomerQuery>().ContainsAsync(ss.Set<CustomerQuery>().First(), default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_with_null_should_rewrite_to_false(bool async)
        => AssertSingleResult(
            async,
            ss => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Contains(null),
            ss => ss.Set<Order>().Where(o => o.CustomerID == "VINET").ContainsAsync(null, default));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Contains(null)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_with_null_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ss.Set<Order>().Where(o => o.CustomerID == "VINET")
                    .Contains(null)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.EmployeeID).Contains(null)),
            assertEmpty: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => !ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.EmployeeID).Contains(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(
                o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.EmployeeID)
                        .Contains(null)
                    == ss.Set<Order>().Where(o => o.CustomerID != "VINET").Select(o => o.EmployeeID)
                        .Contains(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(
                o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.EmployeeID).Contains(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(
                o => ss.Set<Customer>().Where(o => o.CustomerID != "VINET").Select(o => o.CustomerID).Contains(null)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_should_materialize_when_composite(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(o => o.ProductID == 42 && ss.Set<OrderDetail>().Contains(o)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_over_entityType_should_materialize_when_composite2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<OrderDetail>().Where(o => o.ProductID == 42 && ss.Set<OrderDetail>().Where(x => x.OrderID > 42).Contains(o)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task String_FirstOrDefault_in_projection_does_not_do_client_eval(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.CustomerID.FirstOrDefault()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_constant_Sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Employee>(),
            selector: e => 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_any_equals_operator(bool async)
    {
        var ids = new List<string>
        {
            "ABCDE",
            "ALFKI",
            "ANATR"
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Any(li => li == c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_any_equals(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new[] { "ABCDE", "ALFKI", "ANATR" }.Any(li => li.Equals(c.CustomerID))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_any_equals_static(bool async)
    {
        var ids = new List<string>
        {
            "ABCDE",
            "ALFKI",
            "ANATR"
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.Any(li => Equals(li, c.CustomerID))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_subquery_where_any(bool async)
    {
        var ids = new[] { "ABCDE", "ALFKI", "ANATR" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.Any(li => li == c.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.Any(li => c.CustomerID == li)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_all_not_equals_operator(bool async)
    {
        var ids = new List<string>
        {
            "ABCDE",
            "ALFKI",
            "ANATR"
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.All(li => li != c.CustomerID)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_all_not_equals(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new List<string>
                {
                    "ABCDE",
                    "ALFKI",
                    "ANATR"
                }.All(li => !li.Equals(c.CustomerID))));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_subquery_all_not_equals_static(bool async)
    {
        var ids = new List<string>
        {
            "ABCDE",
            "ALFKI",
            "ANATR"
        };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => ids.All(li => !Equals(li, c.CustomerID))));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Where_subquery_where_all(bool async)
    {
        var ids = new List<string>
        {
            "ABCDE",
            "ALFKI",
            "ANATR"
        };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.All(li => li != c.CustomerID)));

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.All(li => c.CustomerID != li)));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_to_same_Type_Count_works(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Customer>().Cast<Customer>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Cast_before_aggregate_is_preserved(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.Select(o => (double?)o.OrderID).Average()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enumerable_min_is_mapped_to_Queryable_1(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.Min(o => (double?)o.OrderID)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Enumerable_min_is_mapped_to_Queryable_2(bool async)
        => AssertQueryScalar(
            async,
            ss => ss.Set<Customer>().Select(c => c.Orders.Select(o => (double?)o.OrderID).Min()));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task DefaultIfEmpty_selects_only_required_columns(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Product>().Select(p => new { p.ProductID, p.ProductName }).DefaultIfEmpty().Select(p => p.ProductName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_Last_member_access_in_projection_translated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Where(c => c.Orders.OrderByDescending(o => o.OrderID).Last().CustomerID == c.CustomerID),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Where(c => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().Maybe(x => x.CustomerID) == c.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Collection_LastOrDefault_member_access_in_projection_translated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Where(c => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().CustomerID == c.CustomerID),
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .Where(c => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().Maybe(x => x.CustomerID) == c.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_explicit_cast_over_column(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>(),
            o => (long?)o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Count_on_projection_with_client_eval(bool async)
    {
        await AssertCount(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderID.ToString("000000")));

        await AssertCount(
            async,
            ss => ss.Set<Order>().Select(o => new { Id = o.OrderID.ToString("000000") }));

        await AssertCount(
            async,
            ss => ss.Set<Order>().Select(o => new { Id = CodeFormat(o.OrderID) }));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_with_unmapped_property_access_throws_meaningful_exception(bool async)
        => AssertTranslationFailedWithDetails(
            () => AssertAverage(
                async,
                ss => ss.Set<Order>(),
                selector: c => c.ShipVia),
            CoreStrings.QueryUnableToTranslateMember(nameof(Order.ShipVia), nameof(Order)));

    private static string CodeFormat(int str)
        => str.ToString();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Sum_over_empty_returns_zero(bool async)
        => AssertSum(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 42),
            o => o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_over_default_returns_default(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 10248),
            o => o.OrderID - 10248);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_over_default_returns_default(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 10248),
            o => o.OrderID - 10248);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_over_default_returns_default(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 10248),
            o => o.OrderID - 10248);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_after_default_if_empty_does_not_throw(bool async)
        => AssertAverage(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 10243).Select(o => o.OrderID).DefaultIfEmpty());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Max_after_default_if_empty_does_not_throw(bool async)
        => AssertMax(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 10243).Select(o => o.OrderID).DefaultIfEmpty());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Min_after_default_if_empty_does_not_throw(bool async)
        => AssertMin(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderID == 10243).Select(o => o.OrderID).DefaultIfEmpty());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Average_on_nav_subquery_in_projection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => new { Ave = (double?)c.Orders.Average(o => o.OrderID) }),
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .Select(c => new { Ave = c.Orders != null && c.Orders.Count() > 0 ? (double?)c.Orders.Average(o => o.OrderID) : null }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Ave.HasValue, a.Ave.HasValue);
                if (e.Ave.HasValue)
                {
                    Assert.InRange(e.Ave.Value - a.Ave.Value, -0.1D, 0.1D);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task All_true(bool async)
        => AssertAll(
            async,
            ss => ss.Set<Customer>(),
            predicate: x => true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Count_after_client_projection(bool async)
        => AssertCount(
            async,
            ss => ss.Set<Order>()
                // ReSharper disable once ConvertTypeCheckToNullCheck
                .Select(o => new { o.OrderID, Customer = o.Customer is Customer ? new { o.Customer.ContactName } : null })
                .Take(1));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Not_Any_false(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => !c.Orders.Any(o => false)).Select(c => c.CustomerID));

    [ConditionalTheory] // #32374
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_inside_aggregate_function_with_GroupBy(bool async)
    {
        var cities = new[] { "London", "Berlin" };

        return AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .GroupBy(c => c.Country)
                .Select(g => g.Count(c => cities.Contains(c.City))));
    }

    [ConditionalTheory] // #32374
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_inside_Average_without_GroupBy(bool async)
    {
        var cities = new[] { "London", "Berlin" };

        return AssertAverage(
            async,
            ss => ss.Set<Customer>(),
            selector: c => cities.Contains(c.City) ? 1.0 : 0.0);
    }

    [ConditionalTheory] // #32374
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_inside_Sum_without_GroupBy(bool async)
    {
        var cities = new[] { "London", "Berlin" };

        return AssertSum(
            async,
            ss => ss.Set<Customer>(),
            selector: c => cities.Contains(c.City) ? 1 : 0);
    }

    [ConditionalTheory] // #32374
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_inside_Count_without_GroupBy(bool async)
    {
        var cities = new[] { "London", "Berlin" };

        return AssertCount(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => cities.Contains(c.City));
    }

    [ConditionalTheory] // #32374
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_inside_LongCount_without_GroupBy(bool async)
    {
        var cities = new[] { "London", "Berlin" };

        return AssertLongCount(
            async,
            ss => ss.Set<Customer>(),
            predicate: c => cities.Contains(c.City));
    }

    [ConditionalTheory] // #32374
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_inside_Max_without_GroupBy(bool async)
    {
        var cities = new[] { "London", "Berlin" };

        return AssertMax(
            async,
            ss => ss.Set<Customer>(),
            selector: c => cities.Contains(c.City) ? 1 : 0);
    }

    [ConditionalTheory] // #32374
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains_inside_Min_without_GroupBy(bool async)
    {
        var cities = new[] { "London", "Berlin" };

        return AssertMin(
            async,
            ss => ss.Set<Customer>(),
            selector: c => cities.Contains(c.City) ? 1 : 0);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Return_type_of_singular_operator_is_preserved(bool async)
    {
        await AssertFirst<CustomerIdDto>(
            async,
            ss => ss.Set<Customer>()
                .Where(x => x.CustomerID == "ALFKI")
                .Select(x => new CustomerIdAndCityDto { CustomerId = x.CustomerID, City = x.City }),
            asserter: (e, a) => Assert.Equal(e.CustomerId, a.CustomerId));

        await AssertFirstOrDefault<CustomerIdDto>(
            async,
            ss => ss.Set<Customer>()
                .Where(x => x.CustomerID == "ALFKI")
                .Select(x => new CustomerIdAndCityDto { CustomerId = x.CustomerID, City = x.City }),
            asserter: (e, a) => Assert.Equal(e.CustomerId, a.CustomerId));

        await AssertSingle<CustomerIdDto>(
            async,
            ss => ss.Set<Customer>()
                .Where(x => x.CustomerID == "ALFKI")
                .Select(x => new CustomerIdAndCityDto { CustomerId = x.CustomerID, City = x.City }),
            asserter: (e, a) => Assert.Equal(e.CustomerId, a.CustomerId));

        await AssertSingleOrDefault<CustomerIdDto>(
            async,
            ss => ss.Set<Customer>()
                .Where(x => x.CustomerID == "ALFKI")
                .Select(x => new CustomerIdAndCityDto { CustomerId = x.CustomerID, City = x.City }),
            asserter: (e, a) => Assert.Equal(e.CustomerId, a.CustomerId));

        await AssertLast<CustomerIdDto>(
            async,
            ss => ss.Set<Customer>()
                .Where(x => x.CustomerID.StartsWith("A"))
                .OrderBy(x => x.CustomerID)
                .Select(x => new CustomerIdAndCityDto { CustomerId = x.CustomerID, City = x.City }),
            asserter: (e, a) => Assert.Equal(e.CustomerId, a.CustomerId));

        await AssertLastOrDefault<CustomerIdDto>(
            async,
            ss => ss.Set<Customer>()
                .Where(x => x.CustomerID.StartsWith("A"))
                .OrderBy(x => x.CustomerID)
                .Select(x => new CustomerIdAndCityDto { CustomerId = x.CustomerID, City = x.City }),
            asserter: (e, a) => Assert.Equal(e.CustomerId, a.CustomerId));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Type_casting_inside_sum(bool async)
        => AssertSum(
            async,
            ss => ss.Set<OrderDetail>(),
            x => (decimal)x.Discount);

    private class CustomerIdDto
    {
        public string CustomerId { get; set; }
    }

    private class CustomerIdAndCityDto : CustomerIdDto
    {
        public string City { get; set; }
    }
}
