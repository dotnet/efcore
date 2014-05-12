// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Northwind;
using Xunit;

namespace Microsoft.Data.FunctionalTests
{
    public abstract class NorthwindQueryTestBase
    {
        [Fact]
        public void Queryable_simple()
        {
            Assert.Equal(91,
                AssertQuery<Customer>(cs => cs));
        }

        [Fact]
        public void Queryable_nested_simple()
        {
            Assert.Equal(91,
                AssertQuery<Customer>(cs =>
                    from c1 in (from c2 in (from c3 in cs select c3) select c2) select c1));
        }

        [Fact]
        public void Take_simple()
        {
            Assert.Equal(10,
                AssertQuery<Customer>(cs => cs.OrderBy(c => c.CustomerID).Take(10)));
        }

        [Fact]
        public void Take_simple_projection()
        {
            Assert.Equal(10,
                AssertQuery<Customer>(cs => cs.OrderBy(c => c.CustomerID).Select(c => c.City).Take(10)));
        }

        [Fact]
        public void Any_simple()
        {
            AssertQuery<Customer>(cs => cs.Any());
        }

        [Fact]
        public virtual async Task Any_simple_async()
        {
            await AssertQueryAsync<Customer>(cs => cs.AnyAsync());
        }

        [Fact]
        public void Select_into()
        {
            AssertQuery<Customer>(cs =>
                from c in cs
                select c.CustomerID
                into id
                where id == "ALFKI"
                select id);
        }

        [Fact]
        public void Take_with_single()
        {
            AssertQuery<Customer>(cs => cs.OrderBy(c => c.CustomerID).Take(1).Single());
        }

        [Fact]
        public void Where_simple()
        {
            AssertQuery<Customer>(cs => cs.Where(c => c.City == "London"));
        }

        [Fact]
        public void Where_primitive()
        {
            AssertQuery<Employee>(es =>
                es.Select(e => e.EmployeeID).Take(9).Where(i => i == 5));
        }

        [Fact]
        public async Task Where_simple_async()
        {
            await AssertQueryAsync<Customer>(cs => cs.Where(c => c.City == "London"));
        }

        [Fact]
        public void Where_true()
        {
            Assert.Equal(91,
                AssertQuery<Customer>(cs => cs.Where(c => true)));
        }

        [Fact]
        public void Where_false()
        {
            Assert.Equal(0,
                AssertQuery<Customer>(cs => cs.Where(c => false)));
        }

//        TODO: Re-write entity ref equality to identity equality.
//
//        [Fact]
//        public void Where_compare_entity_equal()
//        {
//            var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
//
//            Assert.Equal(1,
//                // ReSharper disable once PossibleUnintendedReferenceComparison
//                AssertQuery<Customer>(cs => cs.Where(c => c == alfki)));
//        }
//
//        [Fact]
//        public void Where_compare_entity_not_equal()
//        {
//            var alfki = new Customer { CustomerID = "ALFKI" };
//
//            Assert.Equal(90,
//                // ReSharper disable once PossibleUnintendedReferenceComparison
//                AssertQuery<Customer>(cs => cs.Where(c => c != alfki)));
//
//        [Fact]
//        public void Project_compare_entity_equal()
//        {
//            var alfki = NorthwindData.Customers.Single(c => c.CustomerID == "ALFKI");
//
//            Assert.Equal(1,
//                // ReSharper disable once PossibleUnintendedReferenceComparison
//                AssertQuery<Customer>(cs => cs.Select(c => c == alfki)));
//        }
//
//        [Fact]
//        public void Project_compare_entity_not_equal()
//        {
//            var alfki = new Customer { CustomerID = "ALFKI" };
//
//            Assert.Equal(90,
//                // ReSharper disable once PossibleUnintendedReferenceComparison
//                AssertQuery<Customer>(cs => cs.Select(c => c != alfki)));
//        }

        [Fact]
        public void Where_compare_constructed_equal()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => new { x = c.City } == new { x = "London" }));
        }

        [Fact]
        public void Where_compare_constructed_multi_value_equal()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => new { x = c.City, y = c.Country } == new { x = "London", y = "UK" }));
        }

        [Fact]
        public void Where_compare_constructed_multi_value_not_equal()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => new { x = c.City, y = c.Country } != new { x = "London", y = "UK" }));
        }

        [Fact]
        public void Where_compare_constructed()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => new { x = c.City } == new { x = "London" }));
        }

        [Fact]
        public void Select_scalar()
        {
            AssertQuery<Customer>(cs => cs.Select(c => c.City));
        }

        [Fact]
        public void Select_anonymous_one()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { c.City }));
        }

        [Fact]
        public void Select_anonymous_two()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { c.City, c.Phone }));
        }

        [Fact]
        public void Select_anonymous_three()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { c.City, c.Phone, c.Country }));
        }

        [Fact]
        public void Select_customer_table()
        {
            AssertQuery<Customer>(cs => cs);
        }

        [Fact]
        public void Select_customer_identity()
        {
            AssertQuery<Customer>(cs => cs.Select(c => c));
        }

        [Fact]
        public void Select_anonymous_with_object()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { c.City, c }));
        }

        [Fact]
        public void Select_anonymous_nested()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { c.City, Country = new { c.Country } }));
        }

        [Fact]
        public void Select_anonymous_empty()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { }));
        }

        [Fact]
        public void Select_anonymous_literal()
        {
            AssertQuery<Customer>(cs => cs.Select(c => new { X = 10 }));
        }

        [Fact]
        public void Select_constant_int()
        {
            AssertQuery<Customer>(cs => cs.Select(c => 0));
        }

        [Fact]
        public void Select_constant_null_string()
        {
            AssertQuery<Customer>(cs => cs.Select(c => (string)null));
        }

        [Fact]
        public void Select_local()
        {
            // ReSharper disable once ConvertToConstant.Local
            var x = 10;

            AssertQuery<Customer>(cs => cs.Select(c => x));
        }

        [Fact]
        public void Select_scalar_primitive()
        {
            Assert.Equal(9,
                AssertQuery<Employee>(es => es.Select(e => e.EmployeeID)));
        }

        [Fact]
        public void Select_scalar_primitive_after_take()
        {
            Assert.Equal(9,
                AssertQuery<Employee>(es => es.Take(9).Select(e => e.EmployeeID)));
        }

        [Fact]
        public void Select_nested_collection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                where c.City == "London"
                orderby c.CustomerID
                select os
                    .Where(o => o.CustomerID == c.CustomerID
                                && o.OrderDate.Value.Year == 1997)
                    .Select(o => o.OrderID)
                    .OrderBy(o => o),
                assertOrder: true);
        }

        // TODO: Re-linq parser
//        [Fact]
//        public void Select_nested_ordered_enumerable_collection()
//        {
//            AssertQuery<Customer>(cs =>
//                cs.Select(c => cs.AsEnumerable().OrderBy(c2 => c2.CustomerID)),
//                assertOrder: true);
//        }

        [Fact]
        public void Select_nested_collection_in_anonymous_type()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                where c.CustomerID == "ALFKI"
                select new
                    {
                        CustomerId = c.CustomerID,
                        OrderIds
                            = os.Where(o => o.CustomerID == c.CustomerID
                                            && o.OrderDate.Value.Year == 1997)
                                .Select(o => o.OrderID)
                                .OrderBy(o => o),
                        Customer = c
                    },
                asserter:
                    (l2oResults, efResults) =>
                        {
                            dynamic l2oResult = l2oResults.Single();
                            dynamic efResult = efResults.Single();

                            Assert.Equal(l2oResult.CustomerId, efResult.CustomerId);
                            Assert.Equal(l2oResult.OrderIds, efResult.OrderIds);
                            Assert.Equal(l2oResult.Customer, efResult.Customer);
                        });
        }

        [Fact]
        public void Select_subquery_recursive_trivial()
        {
            AssertQuery<Employee>(es =>
                from e1 in es
                select (from e2 in es
                    select (from e3 in es
                        orderby e3.EmployeeID
                        select e3)),
                assertOrder: true);
        }

        // TODO: [Fact] See #153
        public virtual void Where_subquery_on_collection()
        {
            AssertQuery<Product, OrderDetail>((pr, od) =>
                from p in pr
                where p.OrderDetails.Contains(od.FirstOrDefault(orderDetail => orderDetail.Discount == 0.1))
                select p,
                assertOrder: false);
        }

        [Fact]
        public void Where_subquery_recursive_trivial()
        {
            AssertQuery<Employee>(es =>
                from e1 in es
                where (from e2 in es
                    where (from e3 in es
                        orderby e3.EmployeeID
                        select e3).Any()
                    select e2).Any()
                orderby e1.EmployeeID
                select e1,
                assertOrder: true);
        }

        [Fact]
        public void Select_nested_collection_deep()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                where c.City == "London"
                orderby c.CustomerID
                select (from o1 in os
                    where o1.CustomerID == c.CustomerID
                          && o1.OrderDate.Value.Year == 1997
                    orderby o1.OrderID
                    select (from o2 in os
                        where o1.CustomerID == c.CustomerID
                        orderby o2.OrderID
                        select o1.OrderID)),
                assertOrder: true);
        }

        [Fact]
        public void OrderBy_scalar_primitive()
        {
            AssertQuery<Employee>(es =>
                es.Select(e => e.EmployeeID).OrderBy(i => i),
                assertOrder: true);
        }

        [Fact]
        public void SelectMany_simple()
        {
            AssertQuery<Employee>(
                es => from e1 in es
                    from e2 in es
                    from e3 in es
                    select new { e1, e2 });
        }

        [Fact]
        public void SelectMany_nested_simple()
        {
            AssertQuery<Customer>(cs =>
                from c in cs
                from c1 in (from c2 in (from c3 in cs select c3) select c2)
                orderby c1.CustomerID
                select c1,
                assertOrder: true);
        }

        [Fact]
        public void SelectMany_correlated_simple()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                from c in cs
                from e in es
                where c.City == e.City
                orderby c.CustomerID, e.EmployeeID
                select new { c, e },
                assertOrder: true);
        }

        [Fact]
        public void SelectMany_correlated_subquery_simple()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es.Where(e => e.City == c.City)
                    orderby c.CustomerID, e.EmployeeID
                    select new { c, e },
                assertOrder: true);
        }

        [Fact]
        public void SelectMany_correlated_subquery_hard()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c1 in (from c2 in cs.Take(91) select c2.City).Distinct()
                    from e1 in (from e2 in es where c1 == e2.City select new { e2.City, c1 }).Take(9)
                    from e2 in (from e3 in es where e1.City == e3.City select c1).Take(9)
                    select new { c1, e1 });
        }

        [Fact]
        public void SelectMany_cartesian_product_with_ordering()
        {
            AssertQuery<Customer, Employee>(
                (cs, es) =>
                    from c in cs
                    from e in es
                    where c.City == e.City
                    orderby e.City ascending, c.CustomerID descending
                    select new { c, e.City },
                assertOrder: true);
        }

        [Fact]
        public void SelectMany_primitive()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from i in es.Select(e2 => e2.EmployeeID)
                    select i);
        }

        [Fact]
        public void SelectMany_primitive_select_subquery()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    from i in es.Select(e2 => e2.EmployeeID)
                    select es.Any());
        }

        [Fact]
        public void Join_customers_orders()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID });
        }

        [Fact]
        public void Join_customers_orders_select()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID }
                into p
                select p);
        }

        [Fact]
        public void Join_customers_orders_with_subquery()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                    (from o2 in os orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [Fact]
        public void Join_multi_key()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on new { a = c.CustomerID, b = c.CustomerID }
                    equals new { a = o.CustomerID, b = o.CustomerID }
                select new { c, o });
        }

        [Fact]
        public void GroupJoin_into_customers_orders()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID into orders
                select new { customer = c, orders = orders.ToList() },
                asserter: (l2oItems, efItems) =>
                    {
                        foreach (var pair in 
                            from dynamic l2oItem in l2oItems
                            join dynamic efItem in efItems on l2oItem.customer equals efItem.customer
                            select new { l2oItem, efItem })
                        {
                            Assert.Equal(pair.l2oItem.orders, pair.efItem.orders);
                        }
                    });
        }

        [Fact]
        public void Join_into_customers_orders_count()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into ords
                select new { cust = c, ords = ords.Count() });
        }

        [Fact]
        public void Join_into_default_if_empty()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o1 in orders.DefaultIfEmpty()
                select new { c, o1 });
        }

        [Fact]
        public void SelectMany_customer_orders()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                from o in os
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID });
        }

        // TODO: Composite keys, slow..

//        [Fact]
//        public void Multiple_joins_with_join_conditions_in_where()
//        {
//            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
//                from c in cs
//                from o in os.OrderBy(o1 => o1.OrderID).Take(10)
//                from od in ods
//                where o.CustomerID == c.CustomerID
//                    && o.OrderID == od.OrderID
//                where c.CustomerID == "ALFKI"
//                select od.ProductID,
//                assertOrder: true);
//        }
//        [Fact]
//
//        public void TestMultipleJoinsWithMissingJoinCondition()
//        {
//            AssertQuery<Customer, Order, OrderDetail>((cs, os, ods) =>
//                from c in cs
//                from o in os
//                from od in ods
//                where o.CustomerID == c.CustomerID
//                where c.CustomerID == "ALFKI"
//                select od.ProductID
//                );
//        }

        [Fact]
        public void OrderBy()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderBy(c => c.CustomerID),
                assertOrder: true);
        }

        [Fact]
        public void OrderBy_ThenBy_predicate()
        {
            AssertQuery<Customer>(cs =>
                cs.Where(c => c.City == "London")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID),
                assertOrder: true);
        }

        [Fact]
        public void OrderBy_correlated_subquery_lol()
        {
            AssertQuery<Customer>(cs =>
                from c in cs
                orderby cs.Any(c2 => c2.CustomerID == c.CustomerID)
                select c);
        }

        [Fact]
        public void OrderBy_Select()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderBy(c => c.CustomerID)
                    .Select(c => c.ContactName),
                assertOrder: true);
        }

        [Fact]
        public void OrderBy_multiple()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderBy(c => c.CustomerID)
                    // ReSharper disable once MultipleOrderBy
                    .OrderBy(c => c.Country)
                    .Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public void OrderBy_ThenBy()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderBy(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public void OrderByDescending()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderByDescending(c => c.CustomerID).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public void OrderByDescending_ThenBy()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderByDescending(c => c.CustomerID).ThenBy(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public void OrderByDescending_ThenByDescending()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderByDescending(c => c.CustomerID).ThenByDescending(c => c.Country).Select(c => c.City),
                assertOrder: true);
        }

        [Fact]
        public void OrderBy_Join()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs.OrderBy(c => c.CustomerID)
                join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID
                select new { c.CustomerID, o.OrderID },
                assertOrder: true);
        }

        [Fact]
        public void OrderBy_SelectMany()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs.OrderBy(c => c.CustomerID)
                from o in os.OrderBy(o => o.OrderID)
                where c.CustomerID == o.CustomerID
                select new { c.ContactName, o.OrderID },
                assertOrder: true);
        }

        // TODO: Need to figure out how to do this 
//        [Fact]
//        public void GroupBy_anonymous()
//        {
//            AssertQuery<Customer>(cs =>
//                cs.Select(c => new { c.City, c.CustomerID })
//                    .GroupBy(a => a.City),
//                assertOrder: true);
//        }
//
//        [Fact]
//        public void GroupBy_anonymous_subquery()
//        {
//            AssertQuery<Customer>(cs =>
//                cs.Select(c => new { c.City, c.CustomerID })
//                    .GroupBy(a => from c2 in cs select c2),
//                assertOrder: true);
//        }
//
//        [Fact]
//        public void GroupBy_nested_order_by_enumerable()
//        {
//            AssertQuery<Customer>(cs =>
//                cs.Select(c => new { c.City, c.CustomerID })
//                    .OrderBy(a => a.City)
//                    .GroupBy(a => a.City)
//                    .Select(g => g.OrderBy(a => a.CustomerID)),
//                assertOrder: true);
//        }

        [Fact]
        public void GroupBy_SelectMany()
        {
            AssertQuery<Customer>(cs =>
                cs.GroupBy(c => c.City).SelectMany(g => g));
        }

        [Fact]
        public void GroupBy_Sum()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Sum(o => o.OrderID)));
        }

        [Fact]
        public void GroupBy_Count()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.Count()));
        }

        [Fact]
        public void GroupBy_LongCount()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g => g.LongCount()));
        }

        [Fact]
        public void GroupBy_Sum_Min_Max_Avg()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Select(g =>
                    new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }));
        }

        [Fact]
        public void GroupBy_with_result_selector()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, (k, g) =>
                    new
                        {
                            Sum = g.Sum(o => o.OrderID),
                            Min = g.Min(o => o.OrderID),
                            Max = g.Max(o => o.OrderID),
                            Avg = g.Average(o => o.OrderID)
                        }));
        }

        [Fact]
        public void GroupBy_with_element_selector_sum()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID).Select(g => g.Sum()));
        }

        [Fact]
        public void GroupBy_with_element_selector()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .OrderBy(g => g.Key)
                    .Select(g => g.OrderBy(o => o)),
                assertOrder: true);
        }

        [Fact]
        public void GroupBy_with_element_selector_sum_max()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => o.OrderID)
                    .Select(g => new { Sum = g.Sum(), Max = g.Max() }));
        }

        [Fact]
        public void GroupBy_with_anonymous_element()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID, o => new { o.OrderID })
                    .Select(g => g.Sum(x => x.OrderID)));
        }

        [Fact]
        public void GroupBy_with_two_part_key()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => new { o.CustomerID, o.OrderDate })
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [Fact]
        public void OrderBy_GroupBy()
        {
            AssertQuery<Order>(os =>
                os.OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .Select(g => g.Sum(o => o.OrderID)));
        }

        [Fact]
        public void OrderBy_GroupBy_SelectMany()
        {
            AssertQuery<Order>(os =>
                os.OrderBy(o => o.OrderID)
                    .GroupBy(o => o.CustomerID)
                    .SelectMany(g => g));
        }

        [Fact]
        public void Sum_with_no_arg()
        {
            AssertQuery<Order>(os => os.Select(o => o.OrderID).Sum());
        }

        [Fact]
        public void Sum_with_arg()
        {
            AssertQuery<Order>(os => os.Sum(o => o.OrderID));
        }

        [Fact]
        public void Count_with_no_predicate()
        {
            AssertQuery<Order>(os => os.Count());
        }

        [Fact]
        public async Task Count_with_no_predicate_async()
        {
            await AssertQueryAsync<Order>(os => os.CountAsync());
        }

        [Fact]
        public void Count_with_predicate()
        {
            AssertQuery<Order>(os =>
                os.Count(o => o.CustomerID == "ALFKI"));
        }

        [Fact]
        public void Distinct()
        {
            AssertQuery<Customer>(cs => cs.Distinct());
        }

        [Fact]
        public void Distinct_Scalar()
        {
            AssertQuery<Customer>(cs =>
                cs.Select(c => c.City).Distinct());
        }

        [Fact]
        public void OrderBy_Distinct()
        {
            AssertQuery<Customer>(cs =>
                cs.OrderBy(c => c.CustomerID).Select(c => c.City).Distinct(),
                assertOrder: true);
        }

        [Fact]
        public void Distinct_OrderBy()
        {
            AssertQuery<Customer>(cs =>
                cs.Select(c => c.City).Distinct().OrderBy(c => c),
                assertOrder: true);
        }

        [Fact]
        public void Distinct_GroupBy()
        {
            AssertQuery<Order>(os =>
                os.Distinct()
                    .GroupBy(o => o.CustomerID)
                    .OrderBy(g => g.Key)
                    .Select(g => new { g.Key, c = g.Count() }),
                assertOrder: true);
        }

        [Fact]
        public void GroupBy_Distinct()
        {
            AssertQuery<Order>(os =>
                os.GroupBy(o => o.CustomerID).Distinct().Select(g => g.Key));
        }

        [Fact]
        public void Distinct_Count()
        {
            AssertQuery<Customer>(cs => cs.Distinct().Count());
        }

        [Fact]
        public void Select_Distinct_Count()
        {
            AssertQuery<Customer>(cs =>
                cs.Select(c => c.City).Distinct().Count());
        }

        [Fact]
        public void Select_Select_Distinct_Count()
        {
            AssertQuery<Customer>(cs =>
                cs.Select(c => c.City).Select(c => c).Distinct().Count());
        }

        protected abstract ImmutableDbContextOptions Configuration { get; }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, int> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private async Task<int> AssertQueryAsync<TItem>(
            Func<IQueryable<TItem>, Task<int>> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, bool> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private async Task<int> AssertQueryAsync<TItem>(
            Func<IQueryable<TItem>, Task<bool>> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    new[] { await query(NorthwindData.Set<TItem>()) },
                    new[] { await query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, TItem> query,
            bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    new[] { query(NorthwindData.Set<TItem>()) },
                    new[] { query(context.Set<TItem>()) },
                    assertOrder);
            }
        }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<IQueryable<object>>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private int AssertQuery<TItem1, TItem2>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<object>> query,
            bool assertOrder = false,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()).ToArray(),
                    query(context.Set<TItem1>(), context.Set<TItem2>()).ToArray(),
                    assertOrder,
                    asserter);
            }
        }

        private int AssertQuery<TItem1, TItem2, TItem3>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TItem3>, IQueryable<int>> query,
            bool assertOrder = false)
            where TItem1 : class
            where TItem2 : class
            where TItem3 : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>(), NorthwindData.Set<TItem3>()).ToArray(),
                    query(context.Set<TItem1>(), context.Set<TItem2>(), context.Set<TItem3>()).ToArray(),
                    assertOrder);
            }
        }

        private async Task<int> AssertQueryAsync<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    await ((IAsyncEnumerable<object>)query(context.Set<TItem>())).ToArray(),
                    assertOrder);
            }
        }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<int>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<long>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private int AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<bool>> query, bool assertOrder = false)
            where TItem : class
        {
            using (var context = new DbContext(Configuration))
            {
                return AssertResults(
                    query(NorthwindData.Set<TItem>()).ToArray(),
                    query(context.Set<TItem>()).ToArray(),
                    assertOrder);
            }
        }

        private static int AssertResults<T>(
            IList<T> l2oItems,
            IList<T> efItems,
            bool assertOrder,
            Action<IList<T>, IList<T>> asserter = null)
        {
            Assert.Equal(l2oItems.Count, efItems.Count);

            if (asserter != null)
            {
                asserter(l2oItems, efItems);
            }
            else
            {
                if (assertOrder)
                {
                    Assert.Equal(l2oItems, efItems);
                }
                else
                {
                    foreach (var l2oItem in l2oItems)
                    {
                        Assert.True(
                            efItems.Contains(l2oItem),
                            string.Format(
                                "\r\nL2o item: [{0}] not found in EF results: [{1}]...",
                                l2oItem,
                                string.Join(", ", efItems.Take(10))));
                    }
                }
            }

            return l2oItems.Count;
        }
    }
}
