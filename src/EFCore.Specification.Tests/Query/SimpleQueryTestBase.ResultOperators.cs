// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ReplaceWithSingleCallToCount
// ReSharper disable ReplaceWithSingleCallToFirstOrDefault
// ReSharper disable ReplaceWithSingleCallToFirst
// ReSharper disable AccessToModifiedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
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
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                return obj.GetType() == GetType()
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

        [ConditionalFact]
        public virtual void Sum_with_no_arg()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Sum());
        }

        [ConditionalFact]
        public virtual void Sum_with_no_data_nullable()
        {
            AssertSingleResult<Order>(os => os.Where(o => o.OrderID < 0).Select(o => (int?)o.OrderID).Sum());
        }

        [ConditionalFact]
        public virtual void Sum_with_binary_expression()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID * 2).Sum());
        }

        [ConditionalFact]
        public virtual void Sum_with_no_arg_empty()
        {
            AssertSingleResult<Order>(os => os.Where(o => o.OrderID == 42).Select(o => o.OrderID).Sum());
        }

        [ConditionalFact]
        public virtual void Sum_with_arg()
        {
            AssertSingleResult<Order>(os => os.Sum(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Sum_with_arg_expression()
        {
            AssertSingleResult<Order>(os => os.Sum(o => o.OrderID + o.OrderID));
        }

        [ConditionalFact]
        public virtual void Sum_with_division_on_decimal()
        {
            AssertSingleResult<OrderDetail>(
                ods => ods.Sum(od => od.Quantity / 2.09m),
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalFact]
        public virtual void Sum_with_division_on_decimal_no_significant_digits()
        {
            AssertSingleResult<OrderDetail>(
                ods => ods.Sum(od => od.Quantity / 2m),
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalFact]
        public virtual void Sum_with_coalesce()
        {
            AssertSingleResult<Product>(ps => ps.Where(p => p.ProductID < 40).Sum(p => p.UnitPrice ?? 0));
        }

        [ConditionalFact]
        public virtual void Sum_over_subquery_is_client_eval()
        {
            AssertSingleResult<Customer>(cs => cs.Sum(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void Sum_on_float_column()
        {
            AssertSingleResult<OrderDetail>(ods => ods.Where(od => od.ProductID == 1).Sum(od => od.Discount));
        }

        [ConditionalFact]
        public virtual void Sum_on_float_column_in_subquery()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10300).Select(o => new { o.OrderID, Sum = o.OrderDetails.Sum(od => od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalFact]
        public virtual void Average_with_no_arg()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Average());
        }

        [ConditionalFact]
        public virtual void Average_with_binary_expression()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID * 2).Average());
        }

        [ConditionalFact]
        public virtual void Average_with_arg()
        {
            AssertSingleResult<Order>(os => os.Average(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Average_with_arg_expression()
        {
            AssertSingleResult<Order>(os => os.Average(o => o.OrderID + o.OrderID));
        }

        [ConditionalFact]
        public virtual void Average_with_division_on_decimal()
        {
            AssertSingleResult<OrderDetail>(
                ods => ods.Average(od => od.Quantity / 2.09m),
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalFact]
        public virtual void Average_with_division_on_decimal_no_significant_digits()
        {
            AssertSingleResult<OrderDetail>(
                ods => ods.Average(od => od.Quantity / 2m),
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalFact]
        public virtual void Average_with_coalesce()
        {
            AssertSingleResult<Product>(
                ps => ps.Where(p => p.ProductID < 40).Average(p => p.UnitPrice ?? 0),
                asserter: (e, a) => Assert.InRange((decimal)e - (decimal)a, -0.1m, 0.1m));
        }

        [ConditionalFact]
        public virtual void Average_over_subquery_is_client_eval()
        {
            AssertSingleResult<Customer>(cs => cs.Average(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void Average_on_float_column()
        {
            AssertSingleResult<OrderDetail>(ods => ods.Where(od => od.ProductID == 1).Average(od => od.Discount));
        }

        [ConditionalFact]
        public virtual void Average_on_float_column_in_subquery()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10300).Select(o => new { o.OrderID, Sum = o.OrderDetails.Average(od => od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalFact]
        public virtual void Average_on_float_column_in_subquery_with_cast()
        {
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10300)
                    .Select(o => new { o.OrderID, Sum = o.OrderDetails.Average(od => (float?)od.Discount) }),
                e => e.OrderID);
        }

        [ConditionalFact]
        public virtual void Min_with_no_arg()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Min());
        }

        [ConditionalFact]
        public virtual void Min_with_arg()
        {
            AssertSingleResult<Order>(os => os.Min(o => o.OrderID));
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

        [ConditionalFact]
        public virtual void Min_with_coalesce()
        {
            AssertSingleResult<Product>(ps => ps.Where(p => p.ProductID < 40).Min(p => p.UnitPrice ?? 0));
        }

        [ConditionalFact]
        public virtual void Min_over_subquery_is_client_eval()
        {
            AssertSingleResult<Customer>(cs => cs.Min(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void Max_with_no_arg()
        {
            AssertSingleResult<Order>(os => os.Select(o => o.OrderID).Max());
        }

        [ConditionalFact]
        public virtual void Max_with_arg()
        {
            AssertSingleResult<Order>(os => os.Max(o => o.OrderID));
        }

        [ConditionalFact]
        public virtual void Max_with_coalesce()
        {
            AssertSingleResult<Product>(ps => ps.Where(p => p.ProductID < 40).Max(p => p.UnitPrice ?? 0));
        }

        [ConditionalFact]
        public virtual void Max_over_subquery_is_client_eval()
        {
            AssertSingleResult<Customer>(cs => cs.Max(c => c.Orders.Sum(o => o.OrderID)));
        }

        [ConditionalFact]
        public virtual void Count_with_no_predicate()
        {
            AssertSingleResult<Order>(os => os.Count());
        }

        [ConditionalFact]
        public virtual void Count_with_predicate()
        {
            AssertSingleResult<Order>(os => os.Count(o => o.CustomerID == "ALFKI"));
        }

        [ConditionalFact]
        public virtual void Count_with_order_by()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.CustomerID).Count());
        }

        [ConditionalFact]
        public virtual void Where_OrderBy_Count()
        {
            AssertSingleResult<Order>(os => os.Where(o => o.CustomerID == "ALFKI").OrderBy(o => o.OrderID).Count());
        }

        [ConditionalFact]
        public virtual void OrderBy_Where_Count()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.CustomerID == "ALFKI").Count());
        }

        [ConditionalFact]
        public virtual void OrderBy_Count_with_predicate()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Count(o => o.CustomerID == "ALFKI"));
        }

        [ConditionalFact]
        public virtual void OrderBy_Where_Count_with_predicate()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => o.OrderID > 10).Count(o => o.CustomerID != "ALFKI"));
        }

        [ConditionalFact]
        public virtual void Where_OrderBy_Count_client_eval()
        {
            AssertSingleResult<Order>(os => os.Where(o => ClientEvalPredicate(o)).OrderBy(o => ClientEvalSelectorStateless()).Count());
        }

        [ConditionalFact]
        public virtual void Where_OrderBy_Count_client_eval_mixed()
        {
            AssertSingleResult<Order>(os => os.Where(o => o.OrderID > 10).OrderBy(o => ClientEvalPredicate(o)).Count());
        }

        [ConditionalFact]
        public virtual void OrderBy_Where_Count_client_eval()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)).Count());
        }

        [ConditionalFact]
        public virtual void OrderBy_Where_Count_client_eval_mixed()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)).Count());
        }

        [ConditionalFact]
        public virtual void OrderBy_Count_with_predicate_client_eval()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Count(o => ClientEvalPredicate(o)));
        }

        [ConditionalFact]
        public virtual void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Count(o => ClientEvalPredicate(o)));
        }

        [ConditionalFact]
        public virtual void OrderBy_Where_Count_with_predicate_client_eval()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => ClientEvalSelectorStateless()).Where(o => ClientEvalPredicate(o)).Count(o => ClientEvalPredicate(o)));
        }

        [ConditionalFact]
        public virtual void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            AssertSingleResult<Order>(os => os.OrderBy(o => o.OrderID).Where(o => ClientEvalPredicate(o)).Count(o => o.CustomerID != "ALFKI"));
        }

        [ConditionalFact]
        public virtual void OrderBy_client_Take()
        {
            AssertQuery<Employee>(es => es.OrderBy(o => ClientEvalSelectorStateless()).Take(10), entryCount: 9);
        }

        public static bool ClientEvalPredicateStateless() => true;

        protected static bool ClientEvalPredicate(Order order) => order.OrderID > 10000;

        private static int ClientEvalSelectorStateless() => 42;

#if Test20
        protected internal int ClientEvalSelector(Order order) => order.EmployeeID % 10 ?? 0;
#else
        protected internal uint ClientEvalSelector(Order order) => order.EmployeeID % 10 ?? 0;
#endif

        [ConditionalFact]
        public virtual void Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct(),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Distinct_Scalar()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.City).Distinct());
        }

        [ConditionalFact]
        public virtual void OrderBy_Distinct()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct());
        }

        [ConditionalFact]
        public virtual void Distinct_OrderBy()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => c.Country).Distinct().OrderBy(c => c),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Distinct_OrderBy2()
        {
            AssertQuery<Customer>(
                cs => cs.Distinct().OrderBy(c => c.CustomerID),
                cs => cs.Distinct().OrderBy(c => c.CustomerID, StringComparer.Ordinal),
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Distinct_OrderBy3()
        {
            AssertQuery<Customer>(
                cs => cs.Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID),
                cs => cs.Select(c => new { c.CustomerID }).Distinct().OrderBy(a => a.CustomerID, StringComparer.Ordinal),
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void Distinct_Count()
        {
            AssertSingleResult<Customer>(
                cs => cs.Distinct().Count());
        }

        [ConditionalFact]
        public virtual void Select_Select_Distinct_Count()
        {
            AssertSingleResult<Customer>(
                cs => cs.Select(c => c.City).Select(c => c).Distinct().Count());
        }

        [ConditionalFact]
        public virtual void Single_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    AssertSingleResult<Customer>(cs => cs.Single()));
        }

        [ConditionalFact]
        public virtual void Single_Predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.Single(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_Single()
        {
            AssertSingleResult<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToSingle
                cs => cs.Where(c => c.CustomerID == "ALFKI").Single(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void SingleOrDefault_Throws()
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    AssertSingleResult<Customer>(
                        cs => cs.SingleOrDefault()));
        }

        [ConditionalFact]
        public virtual void SingleOrDefault_Predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.SingleOrDefault(c => c.CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_SingleOrDefault()
        {
            AssertSingleResult<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToSingleOrDefault
                cs => cs.Where(c => c.CustomerID == "ALFKI").SingleOrDefault(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void First()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).First(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void First_Predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).First(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_First()
        {
            AssertSingleResult<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToFirst
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").First(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void FirstOrDefault()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstOrDefault(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void FirstOrDefault_Predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).FirstOrDefault(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_FirstOrDefault()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").FirstOrDefault(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void FirstOrDefault_inside_subquery_gets_server_evaluated()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").FirstOrDefault().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void First_inside_subquery_gets_client_evaluated()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CustomerID == "ALFKI" && c.Orders.Where(o => o.CustomerID == "ALFKI").First().CustomerID == "ALFKI"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Last()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Last(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Last_when_no_order_by()
        {
            AssertSingleResult<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLast
                cs => cs.Where(c => c.CustomerID == "ALFKI").Last(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Last_Predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).Last(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_Last()
        {
            AssertSingleResult<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLast
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").Last(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void LastOrDefault()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastOrDefault(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void LastOrDefault_Predicate()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.ContactName).LastOrDefault(c => c.City == "London"),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_LastOrDefault()
        {
            AssertSingleResult<Customer>(
                // ReSharper disable once ReplaceWithSingleCallToLastOrDefault
                cs => cs.OrderBy(c => c.ContactName).Where(c => c.City == "London").LastOrDefault(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_subquery()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    cs.Where(c => os.Select(o => o.CustomerID).Contains(c.CustomerID)),
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_array_closure()
        {
            var ids = new[] { "ABCDE", "ALFKI" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);

            ids = new[] { "ABCDE" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalFact]
        public virtual void Contains_with_subquery_and_local_array_closure()
        {
            var ids = new[] { "London", "Buenos Aires" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(
                        c =>
                            cs.Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 9);

            ids = new[] { "London" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(
                        c =>
                            cs.Where(c1 => ids.Contains(c1.City)).Any(e => e.CustomerID == c.CustomerID)),
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_int_array_closure()
        {
#if Test20
            var ids = new int[] { 0, 1 };
#else
            var ids = new uint[] { 0, 1 };
#endif

            AssertQuery<Employee>(
                es =>
                    es.Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

#if Test20
            ids = new int[] { 0 };
#else
            ids = new uint[] { 0 };
#endif

            AssertQuery<Employee>(
                es =>
                    es.Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_nullable_int_array_closure()
        {
#if Test20
            var ids = new int?[] { 0, 1 };
#else
            var ids = new uint?[] { 0, 1 };
#endif

            AssertQuery<Employee>(
                es =>
                    es.Where(e => ids.Contains(e.EmployeeID)), entryCount: 1);

#if Test20
            ids = new int?[] { 0 };
#else
            ids = new uint?[] { 0 };
#endif

            AssertQuery<Employee>(
                es =>
                    es.Where(e => ids.Contains(e.EmployeeID)));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_array_inline()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => new[] { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_list_closure()
        {
            var ids = new List<string> { "ABCDE", "ALFKI" };
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_list_inline()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => new List<string> { "ABCDE", "ALFKI" }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_list_inline_closure_mix()
        {
            var id = "ALFKI";

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)), entryCount: 1);

            id = "ANATR";

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => new List<string> { "ABCDE", id }.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_false()
        {
            string[] ids = { "ABCDE", "ALFKI" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => !ids.Contains(c.CustomerID)), entryCount: 90);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_complex_predicate_and()
        {
            string[] ids = { "ABCDE", "ALFKI" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") && ids.Contains(c.CustomerID)), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_complex_predicate_or()
        {
            string[] ids = { "ABCDE", "ALFKI" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            string[] ids = { "ABCDE", "ALFKI" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE") || !ids.Contains(c.CustomerID)), entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            string[] ids = { "ABCDE", "ALFKI" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID) && (c.CustomerID != "ALFKI" && c.CustomerID != "ABCDE")));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_sql_injection()
        {
            string[] ids = { "ALFKI", "ABC')); GO; DROP TABLE Orders; GO; --" };

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID) || (c.CustomerID == "ALFKI" || c.CustomerID == "ABCDE")), entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_empty_closure()
        {
            var ids = new string[0];

            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => ids.Contains(c.CustomerID)));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_collection_empty_inline()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.Where(c => !(new List<string>().Contains(c.CustomerID))), entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Contains_top_level()
        {
            AssertSingleResult<Customer>(cs => cs.Select(c => c.CustomerID).Contains("ALFKI"));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_tuple_array_closure()
        {
            var ids = new[] { Tuple.Create(1, 2), Tuple.Create(10248, 11) };

            AssertQuery<OrderDetail>(
                od => od.Where(o => ids.Contains(new Tuple<int, int>(o.OrderID, o.ProductID))), entryCount: 1);

            ids = new[] { Tuple.Create(1, 2) };

            AssertQuery<OrderDetail>(
                od => od.Where(o => ids.Contains(new Tuple<int, int>(o.OrderID, o.ProductID))));
        }

        [ConditionalFact]
        public virtual void Contains_with_local_anonymous_type_array_closure()
        {
            var ids = new[] { new { Id1 = 1, Id2 = 2 }, new { Id1 = 10248, Id2 = 11 } };

            AssertQuery<OrderDetail>(
                od => od.Where(o => ids.Contains(new { Id1 = o.OrderID, Id2 = o.ProductID })), entryCount: 1);

            ids = new[] { new { Id1 = 1, Id2 = 2 } };

            AssertQuery<OrderDetail>(
                od => od.Where(o => ids.Contains(new { Id1 = o.OrderID, Id2 = o.ProductID })));
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

        [ConditionalFact]
        public virtual void Concat_nested()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void Except_dbset()
        {
            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner").Except(cs));
        }

        [ConditionalFact]
        public virtual void Except_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Except(cs.Where(c => c.City == "México D.F.")),
                entryCount: 14);
        }

        [ConditionalFact]
        public virtual void Except_nested()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void Intersect_dbset()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "México D.F.").Intersect(cs),
                entryCount: 5);
        }

        [ConditionalFact]
        public virtual void Intersect_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.City == "México D.F.")
                    .Intersect(cs.Where(s => s.ContactTitle == "Owner")),
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Intersect_nested()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void Union_dbset()
        {
            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner").Union(cs),
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Union_simple()
        {
            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F.")),
                entryCount: 19);
        }

        [ConditionalFact]
        public virtual void Union_nested()
        {
            AssertQuery<Customer>(
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

        [ConditionalFact]
        public virtual void Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast()
        {
            AssertSingleResult<Order>(
                os => os
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID).Average());
        }

        [ConditionalFact]
        public virtual void Max_with_non_matching_types_in_projection_introduces_explicit_cast()
        {
            AssertSingleResult<Order>(
                os => os
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .OrderBy(o => o.OrderID)
                    .Select(o => (long)o.OrderID).Max());
        }

        [ConditionalFact]
        public virtual void Min_with_non_matching_types_in_projection_introduces_explicit_cast()
        {
            AssertSingleResult<Order>(
                os => os
                    .Where(o => o.CustomerID.StartsWith("A"))
                    .Select(o => (long)o.OrderID).Min());
        }

        [ConditionalFact]
        public virtual void OrderBy_Take_Last_gives_correct_result()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID)
                    .Take(20)
                    .Last(),
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void OrderBy_Skip_Last_gives_correct_result()
        {
            AssertSingleResult<Customer>(
                cs => cs.OrderBy(c => c.CustomerID)
                    .Skip(20)
                    .Last(),
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

        [ConditionalFact]
        public virtual void Project_constant_Sum()
        {
            AssertSingleResult<Employee>(es => es.Sum(e => 1));
        }
    }
}
