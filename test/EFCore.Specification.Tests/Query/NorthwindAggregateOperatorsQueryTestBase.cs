// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
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

        [ConditionalFact]
        public virtual void Select_All()
        {
            using var context = CreateContext();
            Assert.False(
                context
                    .Set<Order>()
                    .Select(
                        o => new ProjectedType { Order = o.OrderID, Customer = o.CustomerID })
                    .All(p => p.Customer == "ALFKI")
            );
        }

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
        {
            return AssertSum(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_data_cast_to_nullable(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 0).Select(o => (int?)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_data_nullable(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Product>(),
                selector: o => o.SupplierID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_binary_expression(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderID * 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_arg_empty(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID == 42).Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_arg(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_arg_expression(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID + o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_division_on_decimal(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2.09m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_division_on_decimal_no_significant_digits(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_coalesce(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_subquery_is_client_eval(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_nested_subquery_is_client_eval(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_min_subquery_is_client_eval(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Min(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_on_float_column(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.ProductID == 1),
                selector: od => od.Discount);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_on_float_column_in_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300).Select(
                    o => new { o.OrderID, Sum = o.OrderDetails.Sum(od => od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_no_arg(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_binary_expression(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderID * 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_arg(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_arg_expression(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID + o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_division_on_decimal(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2.09m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_division_on_decimal_no_significant_digits(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_coalesce(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_subquery_is_client_eval(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_nested_subquery_is_client_eval(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Average(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_max_subquery_is_client_eval(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<OrderDetail>().Where(od => od.ProductID == 1),
                selector: od => od.Discount);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column_in_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300).Select(
                    o => new { o.OrderID, Sum = o.OrderDetails.Average(od => od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column_in_subquery_with_cast(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(o => new { o.OrderID, Sum = o.OrderDetails.Average(od => (float?)od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_no_arg(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_arg(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalFact]
        public virtual void Min_no_data()
        {
            using var context = CreateContext();
            Assert.Throws<InvalidOperationException>(() => context.Orders.Where(o => o.OrderID == -1).Min(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Min_no_data_nullable()
        {
            using var context = CreateContext();
            Assert.Null(context.Products.Where(o => o.SupplierID == -1).Min(o => o.SupplierID));
        }

        [ConditionalFact]
        public virtual void Min_no_data_cast_to_nullable()
        {
            using var context = CreateContext();
            Assert.Null(context.Orders.Where(o => o.OrderID == -1).Min(o => (int?)o.OrderID));
        }

        [ConditionalFact]
        public virtual void Min_no_data_subquery()
        {
            using var context = CreateContext();

            Assert.Equal(
                "Nullable object must have a value.",
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Min(o => o.OrderID)).ToList()).Message);
        }

        [ConditionalFact]
        public virtual void Max_no_data()
        {
            using var context = CreateContext();
            Assert.Throws<InvalidOperationException>(() => context.Orders.Where(o => o.OrderID == -1).Max(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Max_no_data_nullable()
        {
            using var context = CreateContext();
            Assert.Null(context.Products.Where(o => o.SupplierID == -1).Max(o => o.SupplierID));
        }

        [ConditionalFact]
        public virtual void Max_no_data_cast_to_nullable()
        {
            using var context = CreateContext();
            Assert.Null(context.Orders.Where(o => o.OrderID == -1).Max(o => (int?)o.OrderID));
        }

        [ConditionalFact]
        public virtual void Max_no_data_subquery()
        {
            using var context = CreateContext();

            Assert.Equal(
                "Nullable object must have a value.",
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Max(o => o.OrderID)).ToList()).Message);
        }

        [ConditionalFact]
        public virtual void Average_no_data()
        {
            using var context = CreateContext();
            Assert.Throws<InvalidOperationException>(() => context.Orders.Where(o => o.OrderID == -1).Average(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Average_no_data_nullable()
        {
            using var context = CreateContext();
            Assert.Null(context.Products.Where(o => o.SupplierID == -1).Average(o => o.SupplierID));
        }

        [ConditionalFact]
        public virtual void Average_no_data_cast_to_nullable()
        {
            using var context = CreateContext();
            Assert.Null(context.Orders.Where(o => o.OrderID == -1).Average(o => (int?)o.OrderID));
        }

        [ConditionalFact]
        public virtual void Average_no_data_subquery()
        {
            using var context = CreateContext();

            Assert.Equal(
                "Nullable object must have a value.",
                Assert.Throws<InvalidOperationException>(
                    () => context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Average(o => o.OrderID)).ToList()).Message);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_coalesce(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_subquery_is_client_eval(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_nested_subquery_is_client_eval(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Min(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_max_subquery_is_client_eval(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_no_arg(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_arg(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_coalesce(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_subquery_is_client_eval(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_nested_subquery_is_client_eval(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_sum_subquery_is_client_eval(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_no_predicate(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_predicate(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>(),
                predicate: o => o.CustomerID == "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_order_by(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_OrderBy_Count(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID),
                predicate: o => o.CustomerID == "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.OrderID > 10),
                predicate: o => o.CustomerID != "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_OrderBy_Count_client_eval(bool async)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    async,
                    ss => ss.Set<Order>().Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless())));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_client_eval(bool async)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    async,
                    ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_client_eval_mixed(bool async)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    async,
                    ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate_client_eval(bool async)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    async,
                    ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()),
                    predicate: o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate_client_eval_mixed(bool async)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    async,
                    ss => ss.Set<Order>().OrderBy(o => o.OrderID),
                    predicate: o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate_client_eval(bool async)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    async,
                    ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)),
                    predicate: o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool async)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    async,
                    ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)),
                    predicate: o => o.CustomerID != "ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_client_Take(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Employee>().OrderBy(o => ClientEvalSelectorStateless()).Take(10),
                entryCount: 9);
        }

        protected static bool ClientEvalPredicate(Order order)
            => order.OrderID > 10000;

        private static int ClientEvalSelectorStateless()
            => 42;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Distinct(),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Scalar(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.City).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Distinct(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => c.Country).Distinct().OrderBy(c => c),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID),
                ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy3(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID),
                ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID, StringComparer.Ordinal),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Count(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Customer>().Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Select_Distinct_Count(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Customer>().Select(c => c.City).Select(c => c).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Single_Throws(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                async () => await AssertSingle(async, ss => ss.Set<Customer>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Single_Predicate(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Customer>(),
                predicate: c => c.CustomerID == "ALFKI",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Single(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SingleOrDefault_Throws(bool async)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await AssertSingleOrDefault(async, ss => ss.Set<Customer>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SingleOrDefault_Predicate(bool async)
        {
            return AssertSingle(
                async,
                ss => ss.Set<Customer>(),
                predicate: c => c.CustomerID == "ALFKI",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_SingleOrDefault(bool async)
        {
            return AssertSingleOrDefault(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First(bool async)
        {
            return AssertFirst(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First_Predicate(bool async)
        {
            return AssertFirst(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_First(bool async)
        {
            return AssertFirst(
                async,
                // ReSharper disable once ReplaceWithSingleCallToFirst
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_Predicate(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_FirstOrDefault(bool async)
        {
            return AssertFirstOrDefault(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").FirstOrDefault().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID).Select(
                    c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID).FirstOrDefault()),
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.CustomerID).Select(
                    c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()
                        .Maybe(x => x.OrderDetails)
                        .Maybe(xx => xx.OrderBy(od => od.ProductID).FirstOrDefault())),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).OrderBy(c => c.CustomerID).Select(
                    c => (int?)c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID).FirstOrDefault()
                        .ProductID),
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).OrderBy(c => c.CustomerID).Select(
                    c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()
                        .Maybe(x => x.OrderDetails)
                        .MaybeScalar(x => x.OrderBy(od => od.ProductID).FirstOrDefault().ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First_inside_subquery_gets_client_evaluated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").First().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last_when_no_order_by(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault_when_no_order_by(bool async)
        {
            return AssertLastOrDefault(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last_Predicate(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Last(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault(bool async)
        {
            return AssertLastOrDefault(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault_Predicate(bool async)
        {
            return AssertLastOrDefault(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_LastOrDefault(bool async)
        {
            return AssertLastOrDefault(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ss.Set<Order>().Select(o => o.CustomerID).Contains(c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_array_closure(bool async)
        {
            var ids = new[] { "ABCDE", "ALFKI" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)), entryCount: 1);

            ids = new[] { "ABCDE" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_subquery_and_local_array_closure(bool async)
        {
            var ids = new[] { "London", "Buenos Aires" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 9);

            ids = new[] { "London" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_uint_array_closure(bool async)
        {
            var ids = new uint[] { 0, 1 };

            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

            ids = new uint[] { 0 };

            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_nullable_uint_array_closure(bool async)
        {
            var ids = new uint?[] { 0, 1 };

            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

            ids = new uint?[] { 0 };

            await AssertQuery(
                async,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_array_inline(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_closure(bool async)
        {
            var ids = new List<string> { "ABCDE", "ALFKI" };
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_object_list_closure(bool async)
        {
            var ids = new List<object> { "ABCDE", "ALFKI" };
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(EF.Property<object>(c, nameof(Customer.CustomerID)))), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_closure_all_null(bool async)
        {
            var ids = new List<string> { null, null };
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_inline(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => new List<string> { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_list_inline_closure_mix(bool async)
        {
            var id = "ALFKI";

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)), entryCount: 1);

            id = "ANATR";

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool async)
        {
            var id = "ALFKI";

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => new List<Customer> { new Customer { CustomerID = "ABCDE" }, new Customer { CustomerID = id } }
                        .Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);

            id = "ANATR";

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => new List<Customer> { new Customer { CustomerID = "ABCDE" }, new Customer { CustomerID = id } }
                        .Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_non_primitive_list_closure_mix(bool async)
        {
            var ids = new List<Customer> { new Customer { CustomerID = "ABCDE" }, new Customer { CustomerID = "ALFKI" } };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => ids.Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_false(bool async)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => !ids.Contains(c.CustomerID)), entryCount: 90);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_and(bool async)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") && ids.Contains(c.CustomerID)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_or(bool async)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool async)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") || !ids.Contains(c.CustomerID)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool async)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) && (c.CustomerID != "ALFKI" && c.CustomerID != "ABCDE")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_sql_injection(bool async)
        {
            string[] ids = { "ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --" };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_empty_closure(bool async)
        {
            var ids = Array.Empty<string>();

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_empty_inline(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => !(new List<string>().Contains(c.CustomerID))), entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_top_level(bool async)
        {
            return AssertSingleResult(
                async,
                syncQuery: ss => ss.Set<Customer>().Select(c => c.CustomerID).Contains("ALFKI"),
                asyncQuery: ss => ss.Set<Customer>().Select(c => c.CustomerID).ContainsAsync("ALFKI", default));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_tuple_array_closure(bool async)
        {
            var ids = new[] { Tuple.Create(1, 2), Tuple.Create(10248, 11) };

            return AssertTranslationFailed(
                () => AssertQuery(
                    async,
                    ss => ss.Set<OrderDetail>().Where(o => ids.Contains(new Tuple<int, int>(o.OrderID, o.ProductID))),
                    entryCount: 1));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_anonymous_type_array_closure(bool async)
        {
            var ids = new[] { new { Id1 = 1, Id2 = 2 }, new { Id1 = 10248, Id2 = 11 } };

            return AssertTranslationFailed(
                () => AssertQuery(
                    async,
                    ss => ss.Set<OrderDetail>().Where(o => ids.Contains(new { Id1 = o.OrderID, Id2 = o.ProductID })),
                    entryCount: 1));
        }

        //protected string RemoveNewLines(string message)
        //    => message.Replace("\n", "").Replace("\r", "");

        [ConditionalFact]
        public virtual void OfType_Select()
        {
            using var context = CreateContext();
            Assert.Equal(
                "Reims",
                context.Set<Order>()
                    .OfType<Order>()
                    .OrderBy(o => o.OrderID)
                    .Select(o => o.Customer.City)
                    .First());
        }

        [ConditionalFact]
        public virtual void OfType_Select_OfType_Select()
        {
            using var context = CreateContext();
            Assert.Equal(
                "Reims",
                context.Set<Order>()
                    .OfType<Order>()
                    .Select(o => o)
                    .OfType<Order>()
                    .OrderBy(o => o.OrderID)
                    .Select(o => o.Customer.City)
                    .First());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool async)
        {
            return AssertAverage(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
        {
            return AssertMax(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool async)
        {
            return AssertMin(
                async,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_Last_gives_correct_result(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(20),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_Last_gives_correct_result(bool async)
        {
            return AssertLast(
                async,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(20),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_over_entityType_should_rewrite_to_identity_equality()
        {
            using var context = CreateContext();
            var query
                = context.Orders.Where(o => o.CustomerID == "VINET")
                    .Contains(context.Orders.Single(o => o.OrderID == 10248));

            Assert.True(query);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task List_Contains_over_entityType_should_rewrite_to_identity_equality(bool async)
        {
            var someOrder = new Order { OrderID = 10248 };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.Orders.Contains(someOrder)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task List_Contains_with_constant_list(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => new List<Customer> { new Customer { CustomerID = "ALFKI" }, new Customer { CustomerID = "ANATR" } }.Contains(c)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task List_Contains_with_parameter_list(bool async)
        {
            var customers = new List<Customer> { new Customer { CustomerID = "ALFKI" }, new Customer { CustomerID = "ANATR" } };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => customers.Contains(c)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_parameter_list_value_type_id(bool async)
        {
            var orders = new List<Order> { new Order { OrderID = 10248 }, new Order { OrderID = 10249 } };

            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => orders.Contains(o)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_constant_list_value_type_id(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(
                    o => new List<Order> { new Order { OrderID = 10248 }, new Order { OrderID = 10249 } }.Contains(o)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task HashSet_Contains_with_parameter(bool async)
        {
            var ids = new HashSet<string> { "ALFKI" };

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ImmutableHashSet_Contains_with_parameter(bool async)
        {
            var ids = ImmutableHashSet<string>.Empty.Add("ALFKI");

            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
                entryCount: 1);
        }

        private static readonly IEnumerable<string> _customers = new[] { "ALFKI", "WRONG" };

        [ConditionalTheory(Skip = "Issue#18658")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Array_cast_to_IEnumerable_Contains_with_constant(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => _customers.Contains(c.CustomerID)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_over_keyless_entity_throws()
        {
            using var context = CreateContext();
            Assert.Throws<InvalidOperationException>(() => context.CustomerQueries.Contains(new CustomerQuery()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_entityType_with_null_should_rewrite_to_false(bool async)
        {
            return AssertSingleResult(
                async,
                ss => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Contains(null),
                ss => ss.Set<Order>().Where(o => o.CustomerID == "VINET").ContainsAsync(null, default));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Contains(null)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_scalar_with_null_should_rewrite_to_identity_equality_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(
                    o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.CustomerID).Contains(null)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_negated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(
                    o => !ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.CustomerID).Contains(null)),
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_entityType_with_null_should_rewrite_to_identity_equality_subquery_complex(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Order>().Where(
                    o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.CustomerID)
                            .Contains(null)
                        == ss.Set<Order>().Where(o => o.CustomerID != "VINET").Select(o => o.CustomerID)
                            .Contains(null)),
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_nullable_scalar_with_null_in_subquery_translated_correctly(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(
                    o => ss.Set<Order>().Where(o => o.CustomerID == "VINET").Select(o => o.CustomerID).Contains(null)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_non_nullable_scalar_with_null_in_subquery_simplifies_to_false(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Order>().Select(
                    o => ss.Set<Customer>().Where(o => o.CustomerID != "VINET").Select(o => o.CustomerID).Contains(null)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_entityType_should_materialize_when_composite(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(o => o.ProductID == 42 && ss.Set<OrderDetail>().Contains(o)),
                entryCount: 30);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_over_entityType_should_materialize_when_composite2(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<OrderDetail>().Where(o => o.ProductID == 42 && ss.Set<OrderDetail>().Where(x => x.OrderID > 42).Contains(o)),
                entryCount: 30);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_FirstOrDefault_in_projection_does_not_do_client_eval(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.CustomerID.FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_constant_Sum(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Employee>(),
                selector: e => 1);
        }

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
                ss => ss.Set<Customer>().Where(c => ids.Any(li => li == c.CustomerID)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_any_equals(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => new[] { "ABCDE", "ALFKI", "ANATR" }.Any(li => li.Equals(c.CustomerID))),
                entryCount: 2);
        }

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
                ss => ss.Set<Customer>().Where(c => ids.Any(li => Equals(li, c.CustomerID))),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_subquery_where_any(bool async)
        {
            var ids = new[] { "ABCDE", "ALFKI", "ANATR" };

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.Any(li => li == c.CustomerID)),
                entryCount: 1);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.Any(li => c.CustomerID == li)),
                entryCount: 1);
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
                ss => ss.Set<Customer>().Where(c => ids.All(li => li != c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_all_not_equals(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(
                    c => new List<string>
                    {
                        "ABCDE",
                        "ALFKI",
                        "ANATR"
                    }.All(li => !li.Equals(c.CustomerID))),
                entryCount: 89);
        }

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
                ss => ss.Set<Customer>().Where(c => ids.All(li => !Equals(li, c.CustomerID))),
                entryCount: 89);
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
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.All(li => li != c.CustomerID)),
                entryCount: 4);

            await AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.All(li => c.CustomerID != li)),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_to_same_Type_Count_works(bool async)
        {
            return AssertCount(
                async,
                ss => ss.Set<Customer>().Cast<Customer>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_before_aggregate_is_preserved(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.Select(o => (double?)o.OrderID).Average()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enumerable_min_is_mapped_to_Queryable_1(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.Min(o => (double?)o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enumerable_min_is_mapped_to_Queryable_2(bool async)
        {
            return AssertQueryScalar(
                async,
                ss => ss.Set<Customer>().Select(c => c.Orders.Select(o => (double?)o.OrderID).Min()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DefaultIfEmpty_selects_only_required_columns(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Product>().Select(p => new { p.ProductID, p.ProductName }).DefaultIfEmpty().Select(p => p.ProductName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_Last_member_access_in_projection_translated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(c => c.Orders.OrderByDescending(o => o.OrderID).Last().CustomerID == c.CustomerID),
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(c => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().Maybe(x => x.CustomerID) == c.CustomerID),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_LastOrDefault_member_access_in_projection_translated(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(c => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().CustomerID == c.CustomerID),
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(c => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().Maybe(x => x.CustomerID) == c.CustomerID),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_explicit_cast_over_column(bool async)
        {
            return AssertSum(
                async,
                ss => ss.Set<Order>(),
                o => (long?)o.OrderID);
        }

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
        {
            return AssertTranslationFailedWithDetails(
                () => AssertAverage(
                    async,
                    ss => ss.Set<Order>(),
                    selector: c => c.ShipVia),
                CoreStrings.QueryUnableToTranslateMember(nameof(Order.ShipVia), nameof(Order)));
        }

        private static string CodeFormat(int str)
            => str.ToString();

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_empty_returns_zero(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 42),
                o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_default_returns_default(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 10248),
                o => o.OrderID - 10248);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_default_returns_default(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 10248),
                o => o.OrderID - 10248);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_default_returns_default(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 10248),
                o => o.OrderID - 10248);
        }

        [ConditionalTheory(Skip = "Issue#20637")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_after_default_if_empty_does_not_throw(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 10243).Select(o => o.OrderID).DefaultIfEmpty());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_after_default_if_empty_does_not_throw(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 10243).Select(o => o.OrderID).DefaultIfEmpty());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_after_default_if_empty_does_not_throw(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 10243).Select(o => o.OrderID).DefaultIfEmpty());
        }
    }
}
