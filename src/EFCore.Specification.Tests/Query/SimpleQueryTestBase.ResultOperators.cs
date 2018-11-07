// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ReplaceWithSingleCallToCount
// ReSharper disable ReplaceWithSingleCallToFirstOrDefault
// ReSharper disable ReplaceWithSingleCallToFirst
// ReSharper disable AccessToModifiedClosure
// ReSharper disable InconsistentNaming

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_with_custom_projection(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CompanyName.StartsWith("A"))
                    .Union(cs.Where(c => c.CompanyName.StartsWith("B")))
                    .Select(
                        c => new CustomerDeets
                        {
                            Id = c.CustomerID
                        }));
        }

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
                            o => new ProjectedType
                            {
                                Order = o.OrderID,
                                Customer = o.CustomerID
                            })
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

        [ConditionalFact]
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
            return AssertSum<Order>(
                isAsync,
                os => os.Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_data_nullable(bool isAsync)
        {
            return AssertSum<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 0).Select(o => (int?)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_binary_expression(bool isAsync)
        {
            return AssertSum<Order>(
                isAsync,
                os => os.Select(o => o.OrderID * 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_no_arg_empty(bool isAsync)
        {
            return AssertSum<Order>(
                isAsync,
                os => os.Where(o => o.OrderID == 42).Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_arg(bool isAsync)
        {
            return AssertSum<Order, Order>(
                isAsync,
                os => os,
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_arg_expression(bool isAsync)
        {
            return AssertSum<Order, Order>(
                isAsync,
                os => os,
                selector: o => o.OrderID + o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_division_on_decimal(bool isAsync)
        {
            return AssertSum<OrderDetail, OrderDetail>(
                isAsync,
                ods => ods,
                selector: od => od.Quantity / 2.09m,
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            return AssertSum<OrderDetail, OrderDetail>(
                isAsync,
                ods => ods,
                selector: od => od.Quantity / 2m,
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_with_coalesce(bool isAsync)
        {
            return AssertSum<Product, Product>(
                isAsync,
                ps => ps.Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertSum<Customer, Customer>(
                isAsync,
                cs => cs,
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertSum<Customer, Customer>(
                isAsync,
                cs => cs,
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));
        }


        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_over_min_subquery_is_client_eval(bool isAsync)
        {
            return AssertSum<Customer, Customer>(
                isAsync,
                cs => cs,
                selector: c => c.Orders.Sum(o => 5 + o.OrderDetails.Min(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_on_float_column(bool isAsync)
        {
            return AssertSum<OrderDetail, OrderDetail>(
                isAsync,
                ods => ods.Where(od => od.ProductID == 1),
                selector: od => od.Discount);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Sum_on_float_column_in_subquery(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10300).Select(
                    o => new
                    {
                        o.OrderID,
                        Sum = o.OrderDetails.Sum(od => od.Discount)
                    }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_no_arg(bool isAsync)
        {
             return AssertAverage <Order>(
                isAsync,
                os => os.Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_binary_expression(bool isAsync)
        {
            return AssertAverage<Order>(
                isAsync,
                os => os.Select(o => o.OrderID * 2));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_arg(bool isAsync)
        {
            return AssertAverage<Order, Order>(
                isAsync,
                os => os,
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_arg_expression(bool isAsync)
        {
            return AssertAverage<Order, Order>(
                isAsync,
                os => os,
                selector: o => o.OrderID + o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_division_on_decimal(bool isAsync)
        {
            return AssertAverage<OrderDetail, OrderDetail>(
                isAsync,
                ods => ods,
                selector: od => od.Quantity / 2.09m,
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_division_on_decimal_no_significant_digits(bool isAsync)
        {
            return AssertAverage<OrderDetail, OrderDetail>(
                isAsync,
                ods => ods,
                selector: od => od.Quantity / 2m,
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_coalesce(bool isAsync)
        {
            return AssertAverage<Product, Product>(
                isAsync,
                ps => ps.Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0,
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertAverage<Customer, Customer>(
                isAsync,
                cs => cs,
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertAverage<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Average(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_over_max_subquery_is_client_eval(bool isAsync)
        {
            return AssertAverage<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(3),
                selector: c => (decimal)c.Orders.Average(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column(bool isAsync)
        {
            return AssertAverage<OrderDetail, OrderDetail>(
                isAsync,
                ods => ods.Where(od => od.ProductID == 1),
                selector: od => od.Discount);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column_in_subquery(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10300).Select(
                    o => new
                    {
                        o.OrderID,
                        Sum = o.OrderDetails.Average(od => od.Discount)
                    }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_on_float_column_in_subquery_with_cast(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.OrderID < 10300)
                    .Select(
                        o => new
                        {
                            o.OrderID,
                            Sum = o.OrderDetails.Average(od => (float?)od.Discount)
                        }),
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_no_arg(bool isAsync)
        {
            return AssertMin<Order>(
                isAsync,
                os => os.Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_arg(bool isAsync)
        {
            return AssertMin<Order, Order>(
                isAsync,
                os => os,
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
        public virtual void Min_no_data_subquery()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () =>
                        context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Min(o => o.OrderID)).ToList());
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
        public virtual void Max_no_data_subquery()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () =>
                        context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Max(o => o.OrderID)).ToList());
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
        public virtual void Average_no_data_subquery()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(
                    () =>
                        context.Customers.Select(c => c.Orders.Where(o => o.OrderID == -1).Average(o => o.OrderID)).ToList());
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_coalesce(bool isAsync)
        {
            return AssertMin<Product, Product>(
                isAsync,
                ps => ps.Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertMin<Customer, Customer>(
                isAsync,
                cs => cs,
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertMin<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Min(od => od.ProductID)));
        }
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_over_max_subquery_is_client_eval(bool isAsync)
        {
            return AssertMin<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(3),
                selector: c => c.Orders.Min(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_no_arg(bool isAsync)
        {
            return AssertMax<Order>(
                isAsync,
                os => os.Select(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_arg(bool isAsync)
        {
            return AssertMax<Order, Order>(
                isAsync,
                os => os,
                selector: o => o.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_coalesce(bool isAsync)
        {
            return AssertMax<Product, Product>(
                isAsync,
                ps => ps.Where(p => p.ProductID < 40),
                selector: p => p.UnitPrice ?? 0);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_subquery_is_client_eval(bool isAsync)
        {
            return AssertMax<Customer, Customer>(
                isAsync,
                cs => cs,
                selector: c => c.Orders.Sum(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_nested_subquery_is_client_eval(bool isAsync)
        {
            return AssertMax<Customer, Customer>(
               isAsync,
               cs => cs.OrderBy(c => c.CustomerID).Take(3),
               selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Max(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_over_sum_subquery_is_client_eval(bool isAsync)
        {
            return AssertMax<Customer, Customer>(
               isAsync,
               cs => cs.OrderBy(c => c.CustomerID).Take(3),
               selector: c => c.Orders.Max(o => 5 + o.OrderDetails.Sum(od => od.ProductID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_no_predicate(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_predicate(bool isAsync)
        {
            return AssertCount<Order, Order>(
                isAsync,
                os => os,
                predicate: o => o.CustomerID == "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count_with_order_by(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.OrderBy(o => o.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_OrderBy_Count(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate(bool isAsync)
        {
            return AssertCount<Order, Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID),
                predicate: o => o.CustomerID == "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate(bool isAsync)
        {
            return AssertCount<Order, Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Where(o => o.OrderID > 10),
                predicate: o => o.CustomerID != "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_OrderBy_Count_client_eval(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_OrderBy_Count_client_eval_mixed(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.Where(o => o.OrderID > 10).OrderBy(o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_client_eval(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_client_eval_mixed(bool isAsync)
        {
            return AssertCount<Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate_client_eval(bool isAsync)
        {
            return AssertCount<Order, Order>(
                isAsync,
                os => os.OrderBy(o => ClientEvalSelectorStateless()),
                predicate: o => ClientEvalPredicate(o));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            return AssertCount<Order, Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID),
                predicate: o => ClientEvalPredicate(o));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate_client_eval(bool isAsync)
        {
            return AssertCount<Order, Order>(
                isAsync,
                os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)),
                predicate: o => ClientEvalPredicate(o));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool isAsync)
        {
            return AssertCount<Order, Order>(
                isAsync,
                os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)),
                predicate: o => o.CustomerID != "ALFKI");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_client_Take(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => es.OrderBy(o => ClientEvalSelectorStateless()).Take(10), entryCount: 9);
        }

        public static bool ClientEvalPredicateStateless() => true;

        protected static bool ClientEvalPredicate(Order order) => order.OrderID > 10000;

        private static int ClientEvalSelectorStateless() => 42;

        protected internal uint ClientEvalSelector(Order order) => order.EmployeeID % 10 ?? 0;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Distinct(),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Scalar(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(c => c.City).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Distinct(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(c => c.Country).Distinct().OrderBy(c => c),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy2(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Distinct().OrderBy(c => c.CustomerID),
                cs => cs.Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_OrderBy3(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).Distinct().OrderBy(a => a.CustomerID),
                cs => cs.Select(
                    c => new
                    {
                        c.CustomerID
                    }).Distinct().OrderBy(a => a.CustomerID, StringComparer.Ordinal),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distinct_Count(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Select_Distinct_Count(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.Select(c => c.City).Select(c => c).Distinct());
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Single_Throws(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                async () => await AssertSingle<Customer>(isAsync, cs => cs));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Single_Predicate(bool isAsync)
        {
            return AssertSingle<Customer, Customer>(
                isAsync,
                cs => cs,
                predicate: c => c.CustomerID == "ALFKI",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Single(bool isAsync)
        {
            return AssertSingle<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SingleOrDefault_Throws(bool isAsync)
        {
            return Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                    await AssertSingleOrDefault<Customer>(isAsync, cs => cs));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SingleOrDefault_Predicate(bool isAsync)
        {
            return AssertSingle<Customer, Customer>(
                isAsync,
                cs => cs,
                predicate: c => c.CustomerID == "ALFKI",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_SingleOrDefault(bool isAsync)
        {
            return AssertSingleOrDefault<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First(bool isAsync)
        {
            return AssertFirst<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First_Predicate(bool isAsync)
        {
            return AssertFirst<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_First(bool isAsync)
        {
            return AssertFirst<Customer>(
                isAsync,
                // ReSharper disable once ReplaceWithSingleCallToFirst
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault(bool isAsync)
        {
            return AssertFirstOrDefault<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_Predicate(bool isAsync)
        {
            return AssertFirstOrDefault<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_FirstOrDefault(bool isAsync)
        {
            return AssertFirstOrDefault<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task FirstOrDefault_inside_subquery_gets_server_evaluated(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").FirstOrDefault().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First_inside_subquery_gets_client_evaluated(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").First().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last(bool isAsync)
        {
            return AssertLast<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last_when_no_order_by(bool isAsync)
        {
            return AssertLast<Customer>(
                isAsync,
                cs => cs.Where(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last_Predicate(bool isAsync)
        {
            return AssertLast<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_Last(bool isAsync)
        {
            return AssertLast<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault(bool isAsync)
        {
            return AssertLastOrDefault<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LastOrDefault_Predicate(bool isAsync)
        {
            return AssertLastOrDefault<Customer, Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName),
                predicate: c => c.City == "London",
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_LastOrDefault(bool isAsync)
        {
            return AssertLastOrDefault<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_subquery(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) =>
                    cs.Where(c => os.Select(o => o.CustomerID).Contains(c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_array_closure(bool isAsync)
        {
            var ids = new[] { "ABCDE", "ALFKI" };

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);

            ids = new[] { "ABCDE" };

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_subquery_and_local_array_closure(bool isAsync)
        {
            var ids = new[] { "London", "Buenos Aires" };

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(
                    c =>
                        cs.Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 9);

            ids = new[] { "London" };

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(
                    c =>
                        cs.Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_int_array_closure(bool isAsync)
        {
            var ids = new uint[] { 0, 1 };

            await AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

            ids = new uint[] { 0 };

            await AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_nullable_int_array_closure(bool isAsync)
        {
            var ids = new uint?[] { 0, 1 };

            await AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

            ids = new uint?[] { 0 };

            await AssertQuery<Employee>(
                isAsync,
                es => es.Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_array_inline(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_closure(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI"
            };
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_closure_all_null(bool isAsync)
        {
            var ids = new List<string>
            {
                null,
                null
            };
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_list_inline(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs =>
                    cs.Where(
                        c => new List<string>
                        {
                            "ABCDE",
                            "ALFKI"
                        }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_list_inline_closure_mix(bool isAsync)
        {
            var id = "ALFKI";

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(
                    c => new List<string>
                    {
                        "ABCDE",
                        id
                    }.Contains(c.CustomerID)), entryCount: 1);

            id = "ANATR";

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(
                    c => new List<string>
                    {
                        "ABCDE",
                        id
                    }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_non_primitive_list_inline_closure_mix(bool isAsync)
        {
            var id = "ALFKI";

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(
                    c => new List<Customer>
                    {
                        new Customer{ CustomerID = "ABCDE" },
                        new Customer{ CustomerID = id }
                    }.Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);

            id = "ANATR";

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(
                    c => new List<Customer>
                    {
                        new Customer{ CustomerID = "ABCDE" },
                        new Customer{ CustomerID = id }
                    }.Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_non_primitive_list_closure_mix(bool isAsync)
        {
            var ids = new List<Customer>
                    {
                        new Customer{ CustomerID = "ABCDE" },
                        new Customer{ CustomerID = "ALFKI" }
                    };

            await AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(
                    c => ids.Select(i => i.CustomerID).Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_false(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => !ids.Contains(c.CustomerID)), entryCount: 90);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_and(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") && ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_or(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins1(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") || !ids.Contains(c.CustomerID)), entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_complex_predicate_not_matching_ins2(bool isAsync)
        {
            string[] ids = { "ABCDE", "ALFKI" };

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID) && (c.CustomerID != "ALFKI" && c.CustomerID != "ABCDE")));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_sql_injection(bool isAsync)
        {
            string[] ids = { "ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --" };

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")), entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_empty_closure(bool isAsync)
        {
            var ids = Array.Empty<string>();

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_with_local_collection_empty_inline(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => !(new List<string>().Contains(c.CustomerID))), entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains_top_level(bool isAsync)
        {
            return AssertSingleResult<Customer>(
                isAsync,
                syncQuery: cs => cs.Select(c => c.CustomerID).Contains("ALFKI"),
                asyncQuery: cs => cs.Select(c => c.CustomerID).ContainsAsync("ALFKI"));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_tuple_array_closure(bool isAsync)
        {
            var ids = new[] { Tuple.Create(1, 2), Tuple.Create(10248, 11) };

            await AssertQuery<OrderDetail>(
                isAsync,
                od => od.Where(o => ids.Contains(new Tuple<int, int>(o.OrderID, o.ProductID))), entryCount: 1);

            ids = new[] { Tuple.Create(1, 2) };

            await AssertQuery<OrderDetail>(
                isAsync,
                od => od.Where(o => ids.Contains(new Tuple<int, int>(o.OrderID, o.ProductID))));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Contains_with_local_anonymous_type_array_closure(bool isAsync)
        {
            var ids = new[]
            {
                new
                {
                    Id1 = 1,
                    Id2 = 2
                },
                new
                {
                    Id1 = 10248,
                    Id2 = 11
                }
            };

            await AssertQuery<OrderDetail>(
                isAsync,
                od => od.Where(
                    o => ids.Contains(
                        new
                        {
                            Id1 = o.OrderID,
                            Id2 = o.ProductID
                        })), entryCount: 1);

            ids = new[]
            {
                new
                {
                    Id1 = 1,
                    Id2 = 2
                }
            };

            await AssertQuery<OrderDetail>(
                isAsync,
                od => od.Where(
                    o => ids.Contains(
                        new
                        {
                            Id1 = o.OrderID,
                            Id2 = o.ProductID
                        })));
        }

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

        [ConditionalFact]
        public virtual void Concat_dbset()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Concat(context.Set<Customer>())
                    .ToList();

                Assert.Equal(96, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Concat_simple()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Concat(
                        context.Set<Customer>()
                            .Where(s => s.ContactTitle == "Owner"))
                    .ToList();

                Assert.Equal(22, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Concat_nested(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.City == "México D.F.")
                    .Concat(cs.Where(s => s.City == "Berlin"))
                    .Concat(cs.Where(e => e.City == "London")),
                entryCount: 12);
        }

        [ConditionalFact]
        public virtual void Concat_non_entity()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Select(c => c.CustomerID)
                    .Concat(
                        context.Set<Customer>()
                            .Where(s => s.ContactTitle == "Owner")
                            .Select(c => c.CustomerID))
                    .ToList();

                Assert.Equal(22, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_dbset(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(s => s.ContactTitle == "Owner").Except(cs));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Except(cs.Where(c => c.City == "México D.F.")),
                entryCount: 14);
        }

        // issue #12568
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Except_simple_followed_by_projecting_constant(bool isAsync)
        {
            return AssertQueryScalar<Customer>(
                isAsync,
                cs => cs.Except(cs).Select(e => 1));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Except_nested(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Except(cs.Where(s => s.City == "México D.F."))
                    .Except(cs.Where(e => e.City == "Seattle")),
                entryCount: 13);
        }

        [ConditionalFact]
        public virtual void Except_non_entity()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Where(s => s.ContactTitle == "Owner")
                    .Select(c => c.CustomerID)
                    .Except(
                        context.Set<Customer>()
                            .Where(c => c.City == "México D.F.")
                            .Select(c => c.CustomerID))
                    .ToList();

                Assert.Equal(14, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect_dbset(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.City == "México D.F.").Intersect(cs),
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.City == "México D.F.")
                    .Intersect(cs.Where(s => s.ContactTitle == "Owner")),
                entryCount: 3);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersect_nested(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.City == "México D.F.")
                    .Intersect(cs.Where(s => s.ContactTitle == "Owner"))
                    .Intersect(cs.Where(e => e.Fax != null)),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Intersect_non_entity()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Where(c => c.City == "México D.F.")
                    .Select(c => c.CustomerID)
                    .Intersect(
                        context.Set<Customer>()
                            .Where(s => s.ContactTitle == "Owner")
                            .Select(c => c.CustomerID))
                    .ToList();

                Assert.Equal(3, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_dbset(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(s => s.ContactTitle == "Owner").Union(cs),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F.")),
                entryCount: 19);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_nested(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(s => s.City == "México D.F."))
                    .Union(cs.Where(e => e.City == "London")),
                entryCount: 25);
        }

        [ConditionalFact]
        public virtual void Union_non_entity()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .Where(s => s.ContactTitle == "Owner")
                    .Select(c => c.CustomerID)
                    .Union(
                        context.Set<Customer>()
                            .Where(c => c.City == "México D.F.")
                            .Select(c => c.CustomerID))
                    .ToList();

                Assert.Equal(19, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast(bool isAsync)
        {
            return AssertAverage<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Max_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            return AssertMax<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Min_with_non_matching_types_in_projection_introduces_explicit_cast(bool isAsync)
        {
            return AssertMin<Order>(
                isAsync,
                os => os
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .Select(o => (long)o.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Take_Last_gives_correct_result(bool isAsync)
        {
            return AssertLast<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(20),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OrderBy_Skip_Last_gives_correct_result(bool isAsync)
        {
            return AssertLast<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Skip(20),
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

        [ConditionalFact]
        public virtual void Contains_over_entityType_should_materialize_when_composite()
        {
            using (var context = CreateContext())
            {
                var query
                    = context.OrderDetails.Where(o => o.ProductID == 42)
                        .Contains(context.OrderDetails.First(o => o.OrderID == 10248 && o.ProductID == 42));

                Assert.True(query);
            }
        }

        [ConditionalFact]
        public virtual void Paging_operation_on_string_doesnt_issue_warning()
        {
            using (var context = CreateContext())
            {
                var query = context.Customers.Select(c => c.CustomerID.FirstOrDefault()).ToList();
                Assert.Equal(91, query.Count);
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_constant_Sum(bool isAsync)
        {
            return AssertSum<Employee, Employee>(
                isAsync,
                es => es,
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

            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Any(li => li == c.CustomerID)),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_any_equals(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Any(li => li.Equals(c.CustomerID))),
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
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.Any(li => Equals(li, c.CustomerID))),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_where_any(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.City == "México D.F.").Where(c => ids.Any(li => li == c.CustomerID)),
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
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.All(li => li != c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_all_not_equals(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.All(li => !li.Equals(c.CustomerID))),
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
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => ids.All(li => !Equals(li, c.CustomerID))),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_where_all(bool isAsync)
        {
            var ids = new List<string>
            {
                "ABCDE",
                "ALFKI",
                "ANATR"
            };
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.Where(c => c.City == "México D.F.").Where(c => ids.All(li => li != c.CustomerID)),
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Cast_to_same_Type_Count_works(bool isAsync)
        {
            return AssertCount<Customer>(
                isAsync,
                cs => cs.Cast<Customer>());
        }
    }
}
