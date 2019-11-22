// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        public class CustomerDeets
        {
            public string Id { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is null)
                {
                    return false;
                }

                return ReferenceEquals(this, obj)
                    ? true
                    : obj.GetType() == GetType()
                    && string.Equals(Id, ((CustomerDeets)obj).Id);
            }

            public override int GetHashCode() => Id != null ? Id.GetHashCode() : 0;
        }

        [ConditionalFact]
        public virtual void Select_All()
        {
            using (var context = CreateContext())
            {
                Assert.False(
                    context
                        .Set<Order>()
                        .Select(
                            o => new ProjectedType { Order = o.OrderID, Customer = o.CustomerID })
                        .All(p => p.Customer == "ALFKI")
                );
            }
        }

        private class ProjectedType
        {
            public int Order { get; set; }
            public string Customer { get; set; }

            private bool Equals(ProjectedType other) => Equals(Order, other.Order);

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

            public override int GetHashCode() => Order.GetHashCode();
        }

        [ConditionalFact(Skip = "Issue #17068")]
        public virtual void GroupBy_tracking_after_dispose()
        {
            List<IGrouping<string, Order>> groups;

            using (var context = CreateContext())
            {
                groups = context.Orders.GroupBy(o => o.CustomerID).ToList();
            }

            groups[0].First();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_arg(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_data_cast_to_nullable(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID < 0).Select(o => (int?)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_data_nullable(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Product>(),
                selector: o => o.SupplierID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_binary_expression(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderID * 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_arg_empty(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID == 42).Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_arg(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_arg_expression(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID + o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_division_on_decimal(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2.09m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_coalesce(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_min_subquery_is_client_eval(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Min(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_on_float_column(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<OrderDetail>().Where(od => od.ProductID == 1),
                selector: od => od.Discount);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_on_float_column_in_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300).Select(
                    o => new { o.OrderID, Sum = o.OrderDetails.Sum(od => od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_no_arg(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_binary_expression(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderID * 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_arg(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_arg_expression(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID + o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_division_on_decimal(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2.09m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<OrderDetail>(),
                selector: od => od.Quantity / 2m,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_coalesce(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0,
                asserter: (e, a) => Assert.InRange(e - a, -0.1m, 0.1m));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Average(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_max_subquery_is_client_eval(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<OrderDetail>().Where(od => od.ProductID == 1),
                selector: od => od.Discount);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column_in_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300).Select(
                    o => new { o.OrderID, Sum = o.OrderDetails.Average(od => od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column_in_subquery_with_cast(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.OrderID < 10300)
                    .Select(o => new { o.OrderID, Sum = o.OrderDetails.Average(od => (float?)od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_no_arg(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_arg(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalFact]
        public virtual void Min_no_data()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Orders.Where(o => o.OrderID == -1).Min(o => o.OrderID));
            }
        }

        [ConditionalFact]
        public virtual void Min_no_data_nullable()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.Products.Where(o => o.SupplierID == -1).Min(o => o.SupplierID));
            }
        }

        [ConditionalFact]
        public virtual void Min_no_data_cast_to_nullable()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.Orders.Where(o => o.OrderID == -1).Min(o => (int?)o.OrderID));
            }
        }

        [ConditionalFact]
        public virtual void Min_no_data_subquery()
        {
            using (var context = CreateContext())
            {
                // Verify that it does not throw
                context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Min(o => o.OrderID)).ToList();
            }
        }

        [ConditionalFact]
        public virtual void Max_no_data()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Orders.Where(o => o.OrderID == -1).Max(o => o.OrderID));
            }
        }

        [ConditionalFact]
        public virtual void Max_no_data_nullable()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.Products.Where(o => o.SupplierID == -1).Max(o => o.SupplierID));
            }
        }

        [ConditionalFact]
        public virtual void Max_no_data_cast_to_nullable()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.Orders.Where(o => o.OrderID == -1).Max(o => (int?)o.OrderID));
            }
        }

        [ConditionalFact]
        public virtual void Max_no_data_subquery()
        {
            using (var context = CreateContext())
            {
                // Verify that it does not throw
                context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Max(o => o.OrderID)).ToList();
            }
        }

        [ConditionalFact]
        public virtual void Average_no_data()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Orders.Where(o => o.OrderID == -1).Average(o => o.OrderID));
            }
        }

        [ConditionalFact]
        public virtual void Average_no_data_nullable()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.Products.Where(o => o.SupplierID == -1).Average(o => o.SupplierID));
            }
        }

        [ConditionalFact]
        public virtual void Average_no_data_cast_to_nullable()
        {
            using (var context = CreateContext())
            {
                Assert.Null(context.Orders.Where(o => o.OrderID == -1).Average(o => (int?)o.OrderID));
            }
        }

        [ConditionalFact]
        public virtual void Average_no_data_subquery()
        {
            using (var context = CreateContext())
            {
                // Verify that it does not throw
                context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Average(o => o.OrderID)).ToList();
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_coalesce(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Min(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_max_subquery_is_client_eval(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_no_arg(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_arg(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Order>(),
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_coalesce(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Product>().Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Customer>(),
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_sum_subquery_is_client_eval(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_no_predicate(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Order>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_predicate(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Order>(),
                predicate: o => o.CustomerID == "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_order_by(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Order>().OrderBy(o => o.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_OrderBy_Count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Order>().Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID),
                predicate: o => o.CustomerID == "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => o.OrderID > 10),
                predicate: o => o.CustomerID != "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_OrderBy_Count_client_eval(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    isAsync,
                    ss => ss.Set<Order>().Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless())));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_client_eval(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    isAsync,
                    ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_client_eval_mixed(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    isAsync,
                    ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate_client_eval(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    isAsync,
                    ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()),
                    predicate: o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    isAsync,
                    ss => ss.Set<Order>().OrderBy(o => o.OrderID),
                    predicate: o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate_client_eval(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    isAsync,
                    ss => ss.Set<Order>().OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)),
                    predicate: o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertCount(
                    isAsync,
                    ss => ss.Set<Order>().OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)),
                    predicate: o => o.CustomerID != "ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_client_Take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().OrderBy(o => ClientEvalSelectorStateless()).Take(10),
                entryCount: 9);
        }

        public static bool ClientEvalPredicateStateless() => true;

        protected static bool ClientEvalPredicate(Order order) => order.OrderID > 10000;

        private static int ClientEvalSelectorStateless() => 42;

        protected internal uint ClientEvalSelector(Order order) => order.EmployeeID % 10 ?? 0;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Distinct(),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Scalar(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Select(c => c.City).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Distinct(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Select(c => c.Country).Distinct().OrderBy(c => c),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID),
                ss => ss.Set<Customer>().Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID),
                ss => ss.Set<Customer>().Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID, StringComparer.Ordinal),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Customer>().Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Select_Distinct_Count(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Customer>().Select(c => c.City).Select(c => c).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Single_Throws(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                async () => await AssertSingle(isAsync, ss => ss.Set<Customer>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Single_Predicate(bool isAsync)
        {
            return AssertSingle(
                isAsync,
                ss => ss.Set<Customer>(),
                predicate: c => c.CustomerID == "ALFKI",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Single(bool isAsync)
        {
            return AssertSingle(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SingleOrDefault_Throws(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await AssertSingleOrDefault(isAsync, ss => ss.Set<Customer>()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SingleOrDefault_Predicate(bool isAsync)
        {
            return AssertSingle(
                isAsync,
                ss => ss.Set<Customer>(),
                predicate: c => c.CustomerID == "ALFKI",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_SingleOrDefault(bool isAsync)
        {
            return AssertSingleOrDefault(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First(bool isAsync)
        {
            return AssertFirst(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First_Predicate(bool isAsync)
        {
            return AssertFirst(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_First(bool isAsync)
        {
            return AssertFirst(
                isAsync,
                // ReSharper disable once ReplaceWithSingleCallToFirst
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault(bool isAsync)
        {
            return AssertFirstOrDefault(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_Predicate(bool isAsync)
        {
            return AssertFirstOrDefault(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_FirstOrDefault(bool isAsync)
        {
            return AssertFirstOrDefault(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").FirstOrDefault().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory(Skip = "Issue#16314")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_collection_navigation_with_FirstOrDefault_chained(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                    c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID).FirstOrDefault()),
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                    c => Maybe(
                        Maybe(
                            c.Orders.OrderBy(o => o.OrderID).FirstOrDefault(),
                            () => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails),
                        () => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID)
                            .FirstOrDefault())));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_collection_navigation_with_FirstOrDefault_chained_projecting_scalar(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                    c => (int?)c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID).FirstOrDefault()
                        .ProductID),
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Select(
                    c => MaybeScalar<int>(
                        Maybe(
                            c.Orders.OrderBy(o => o.OrderID).FirstOrDefault(),
                            () => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails),
                        () => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().OrderDetails.OrderBy(od => od.ProductID).FirstOrDefault()
                            .ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First_inside_subquery_gets_client_evaluated(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").First().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last(bool isAsync)
        {
            return AssertLast(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last_when_no_order_by(bool isAsync)
        {
            return AssertTranslationFailed(
                () => AssertLast(
                    isAsync,
                    ss => ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI"),
                    entryCount: 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last_Predicate(bool isAsync)
        {
            return AssertLast(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Last(bool isAsync)
        {
            return AssertLast(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault(bool isAsync)
        {
            return AssertLastOrDefault(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault_Predicate(bool isAsync)
        {
            return AssertLastOrDefault(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_LastOrDefault(bool isAsync)
        {
            return AssertLastOrDefault(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ss.Set<Order>().Select(o => o.CustomerID).Contains(c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_array_closure(bool isAsync)
        {
            var ids = new[] { "ABCDE", "ALFKI" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)), entryCount: 1);

            ids = new[] { "ABCDE" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_subquery_and_local_array_closure(bool isAsync)
        {
            var ids = new[] { "London", "Buenos Aires" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 9);

            ids = new[] { "London" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => ss.Set<Customer>().Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_uint_array_closure(bool isAsync)
        {
            var ids = new uint[] { 0, 1 };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

            ids = new uint[] { 0 };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_nullable_uint_array_closure(bool isAsync)
        {
            var ids = new uint?[] { 0, 1 };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

            ids = new uint?[] { 0 };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Employee>().Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_array_inline(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_closure(bool isAsync)
        {
            var ids = new List<string> { "ABCDE", "ALFKI" };
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_object_list_closure(bool isAsync)
        {
            var ids = new List<object> { "ABCDE", "ALFKI" };
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(EF.Property<object>(c, nameof(Customer.CustomerID)))), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_closure_all_null(bool isAsync)
        {
            var ids = new List<string> { null, null };
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_inline(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new List<string> { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_list_inline_closure_mix(bool isAsync)
        {
            var id = "ALFKI";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)), entryCount: 1);

            id = "ANATR";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool isAsync)
        {
            var id = "ALFKI";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new List<Customer> { new Customer { CustomerID = "ABCDE" }, new Customer { CustomerID = id } }
                        .Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);

            id = "ANATR";

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new List<Customer> { new Customer { CustomerID = "ABCDE" }, new Customer { CustomerID = id } }
                        .Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_non_primitive_list_closure_mix(bool isAsync)
        {
            var ids = new List<Customer> { new Customer { CustomerID = "ABCDE" }, new Customer { CustomerID = "ALFKI" } };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => ids.Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_false(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => !ids.Contains(c.CustomerID)), entryCount: 90);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_and(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") && ids.Contains(c.CustomerID)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_or(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") || !ids.Contains(c.CustomerID)),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) && (c.CustomerID != "ALFKI" && c.CustomerID != "ABCDE")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_sql_injection(bool isAsync)
        {
            string[] ids = { "ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_empty_closure(bool isAsync)
        {
            var ids = Array.Empty<string>();

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_empty_inline(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => !(new List<string>().Contains(c.CustomerID))), entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_top_level(bool isAsync)
        {
            return AssertSingleResult(
                isAsync,
                syncQuery: ss => ss.Set<Customer>().Select(c => c.CustomerID).Contains("ALFKI"),
                asyncQuery: ss => ss.Set<Customer>().Select(c => c.CustomerID).ContainsAsync("ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_tuple_array_closure(bool isAsync)
        {
            var ids = new[] { Tuple.Create(1, 2), Tuple.Create(10248, 11) };

            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<OrderDetail>().Where(o => ids.Contains(new Tuple<int, int>(o.OrderID, o.ProductID))),
                    entryCount: 1));
        }

        [ConditionalTheory(Skip = "Issue #15937")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_anonymous_type_array_closure(bool isAsync)
        {
            var ids = new[] { new { Id1 = 1, Id2 = 2 }, new { Id1 = 10248, Id2 = 11 } };

            return AssertTranslationFailed(
                () => AssertQuery(
                    isAsync,
                    ss => ss.Set<OrderDetail>().Where(o => ids.Contains(new { Id1 = o.OrderID, Id2 = o.ProductID })),
                    entryCount: 1));
        }

        //protected string RemoveNewLines(string message)
        //    => message.Replace("\n", "").Replace("\r", "");

        [ConditionalFact]
        public virtual void OfType_Select()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    "Reims",
                    context.Set<Order>()
                        .OfType<Order>()
                        .OrderBy(o => o.OrderID)
                        .Select(o => o.Customer.City)
                        .First());
            }
        }

        [ConditionalFact]
        public virtual void OfType_Select_OfType_Select()
        {
            using (var context = CreateContext())
            {
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
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool isAsync)
        {
            return AssertAverage(
                isAsync,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            return AssertMax(
                isAsync,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            return AssertMin(
                isAsync,
                ss => ss.Set<Order>()
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_Last_gives_correct_result(bool isAsync)
        {
            return AssertLast(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(20),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_Last_gives_correct_result(bool isAsync)
        {
            return AssertLast(
                isAsync,
                ss => ss.Set<Customer>().OrderBy(c => c.CustomerID).Skip(20),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_over_entityType_should_rewrite_to_identity_equality()
        {
            using (var context = CreateContext())
            {
                var query
                    = context.Orders.Where(o => o.CustomerID == "VINET")
                        .Contains(context.Orders.Single(o => o.OrderID == 10248));

                Assert.True(query);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task List_Contains_over_entityType_should_rewrite_to_identity_equality(bool isAsync)
        {
            var someOrder = new Order { OrderID = 10248 };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.Orders.Contains(someOrder)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task List_Contains_with_constant_list(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(
                    c => new List<Customer> { new Customer { CustomerID = "ALFKI" }, new Customer { CustomerID = "ANATR" } }.Contains(c)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task List_Contains_with_parameter_list(bool isAsync)
        {
            var customers = new List<Customer> { new Customer { CustomerID = "ALFKI" }, new Customer { CustomerID = "ANATR" } };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => customers.Contains(c)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_parameter_list_value_type_id(bool isAsync)
        {
            var orders = new List<Order> { new Order { OrderID = 10248 }, new Order { OrderID = 10249 } };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(o => orders.Contains(o)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_constant_list_value_type_id(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().Where(
                    o => new List<Order> { new Order { OrderID = 10248 }, new Order { OrderID = 10249 } }.Contains(o)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task HashSet_Contains_with_parameter(bool isAsync)
        {
            var ids = new HashSet<string> { "ALFKI" };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ImmutableHashSet_Contains_with_parameter(bool isAsync)
        {
            var ids = ImmutableHashSet<string>.Empty.Add("ALFKI");

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Contains(c.CustomerID)),
                entryCount: 1);
        }

        private static readonly IEnumerable<string> _customers = new string[] { "ALFKI", "WRONG" };

        [ConditionalTheory(Skip = "Issue#18658")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Array_cast_to_IEnumerable_Contains_with_constant(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => _customers.Contains(c.CustomerID)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_over_keyless_entity_throws()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.CustomerQueries.Contains(new CustomerView()));
            }
        }

        [ConditionalFact]
        public virtual void Contains_over_entityType_with_null_should_rewrite_to_identity_equality()
        {
            using (var context = CreateContext())
            {
                var query
                    = context.Orders.Where(o => o.CustomerID == "VINET")
                        .Contains(null);

                Assert.False(query);
            }
        }

        [ConditionalFact]
        public virtual void Contains_over_entityType_should_materialize_when_composite()
        {
            using (var context = CreateContext())
            {
                Assert.Equal(
                    "Cannot translate a Contains() operator on entity 'OrderDetail' because it has a composite key.",
                    Assert.Throws<InvalidOperationException>(
                        () => context.OrderDetails.Where(o => o.ProductID == 42)
                            .Contains(context.OrderDetails.First(o => o.OrderID == 10248 && o.ProductID == 42))).Message);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task String_FirstOrDefault_in_projection_does_client_eval(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Customer>().Select(c => c.CustomerID.FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_constant_Sum(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Employee>(),
                selector: e => 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_any_equals_operator(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Any(li => li == c.CustomerID)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_any_equals(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => new[] { "ABCDE", "ALFKI", "ANATR" }.Any(li => li.Equals(c.CustomerID))),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_any_equals_static(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.Any(li => Equals(li, c.CustomerID))),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_subquery_where_any(bool isAsync)
        {
            var ids = new[] { "ABCDE", "ALFKI", "ANATR" };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.Any(li => li == c.CustomerID)),
                entryCount: 1);

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.Any(li => c.CustomerID == li)),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_all_not_equals_operator(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.All(li => li != c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_all_not_equals(bool isAsync)
        {
            return AssertQuery(
                isAsync,
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
        public virtual Task Where_subquery_all_not_equals_static(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => ids.All(li => !Equals(li, c.CustomerID))),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_subquery_where_all(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.All(li => li != c.CustomerID)),
                entryCount: 4);

            await AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.City == "México D.F.").Where(c => ids.All(li => c.CustomerID != li)),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_to_same_Type_Count_works(bool isAsync)
        {
            return AssertCount(
                isAsync,
                ss => ss.Set<Customer>().Cast<Customer>());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_before_aggregate_is_preserved(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Customer>().Select(c => c.Orders.Select(o => (double?)o.OrderID).Average()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enumerable_min_is_mapped_to_Queryable_1(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Customer>().Select(c => c.Orders.Min(o => (double?)o.OrderID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Enumerable_min_is_mapped_to_Queryable_2(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => ss.Set<Customer>().Select(c => c.Orders.Select(o => (double?)o.OrderID).Min()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task DefaultIfEmpty_selects_only_required_columns(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Product>().Select(p => new { p.ProductID, p.ProductName }).DefaultIfEmpty().Select(p => p.ProductName));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_Last_member_access_in_projection_translated(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(c => c.Orders.OrderByDescending(o => o.OrderID).Last().CustomerID == c.CustomerID),
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(
                        c => Maybe(
                                c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault(),
                                () => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().CustomerID)
                            == c.CustomerID),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_LastOrDefault_member_access_in_projection_translated(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(c => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().CustomerID == c.CustomerID),
                ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                    .Where(
                        c => Maybe(
                                c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault(),
                                () => c.Orders.OrderByDescending(o => o.OrderID).LastOrDefault().CustomerID)
                            == c.CustomerID),
                entryCount: 7);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_explicit_cast_over_column(bool isAsync)
        {
            return AssertSum(
                isAsync,
                ss => ss.Set<Order>(),
                o => (long?)o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Count_on_projection_with_client_eval(bool isAsync)
        {
            await AssertCount(
                isAsync,
                ss => ss.Set<Order>().Select(o => o.OrderID.ToString("000000")));

            await AssertCount(
                isAsync,
                ss => ss.Set<Order>().Select(o => new { Id = o.OrderID.ToString("000000") }));

            await AssertCount(
                isAsync,
                ss => ss.Set<Order>().Select(o => new { Id = CodeFormat(o.OrderID) }));
        }

        private static string CodeFormat(int str)
        {
            return str.ToString();
        }
    }
}
