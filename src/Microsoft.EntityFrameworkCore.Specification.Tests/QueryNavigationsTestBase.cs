// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ReplaceWithSingleCallToFirstOrDefault
// ReSharper disable ReplaceWithSingleCallToAny
// ReSharper disable ReplaceWithSingleCallToFirst
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable UseCollectionCountProperty
// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleUnintendedReferenceComparison

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class QueryNavigationsTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
        public virtual void Select_Where_Navigation()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.Customer.City == "Seattle"
                      select o,
                entryCount: 14);
        }

        [ConditionalFact]
        public virtual async Task Select_Where_Navigation_Async()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o in context.Set<Order>()
                             where o.Customer.City == "Seattle"
                             select o).ToListAsync();

                Assert.Equal(14, orders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>().Where(o => o.OrderID < 10300)
                       from o2 in context.Set<Order>().Where(o => o.OrderID < 10400)
                       where o1.Customer.City == o2.Customer.City
                       select new { o1, o2 }).ToList();

                Assert.Equal(223, orders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>().Where(o => o.OrderID < 10300)
                       from o2 in context.Set<Order>().Where(o => o.OrderID < 10400)
                       where o1.Customer.City == o2.Customer.City
                       select new { o1.CustomerID, C2 = o2.CustomerID }).ToList();

                Assert.Equal(223, orders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Client()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.Customer.IsLondon
                      select o,
                entryCount: 46);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Deep()
        {
            AssertQuery<OrderDetail>(
                ods => (from od in ods
                        where od.Order.Customer.City == "Seattle"
                        orderby od.OrderID, od.ProductID
                        select od).Take(1),
                asserter: (l2oItems, efItems) =>
                {
                    var matchingPairs =
                        from dynamic l2oItem in l2oItems
                        join dynamic efItem in efItems on new { l2oItem.OrderID, l2oItem.ProductID } equals new { efItem.OrderID, efItem.ProductID }
                        select new { l2oItem, efItem };

                    Assert.Equal(matchingPairs.Count(), l2oItems.Count);
                },
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Take_Select_Navigation()
        {
            AssertQuery<Customer>(
                cs => cs.Take(2)
                    .Select(c => c.Orders.FirstOrDefault()));
        }


        [ConditionalFact]
        public virtual void Select_collection_FirstOrDefault_project_single_column1()
        {
            AssertQuery<Customer>(
                cs => cs.Take(2).Select(c => c.Orders.FirstOrDefault().CustomerID));
        }

        [ConditionalFact]
        public virtual void Select_collection_FirstOrDefault_project_single_column2()
        {
            AssertQuery<Customer>(
                cs => cs.Take(2).Select(c => c.Orders.Select(o => o.CustomerID).FirstOrDefault()));
        }

        [ConditionalFact]
        public virtual void Select_collection_FirstOrDefault_project_anonymous_type()
        {
            AssertQuery<Customer>(
                cs => cs.Take(2).Select(c => c.Orders.Select(o => new { o.CustomerID, o.OrderID }).FirstOrDefault()));
        }

        [ConditionalFact]
        public virtual void Select_collection_FirstOrDefault_project_entity()
        {
            AssertQuery<Customer>(
                cs => cs.Take(2).Select(c => c.Orders.FirstOrDefault()));
        }

        [ConditionalFact]
        public virtual void Skip_Select_Navigation()
        {
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID)
                    .Skip(20)
                    .Select(c => c.Orders
                        .OrderBy(o => o.OrderID)
                        .FirstOrDefault()));
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null()
        {
            AssertQuery<Employee>(
                es => from e in es
                      where e.Manager == null
                      select e,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null_Reverse()
        {
            AssertQuery<Employee>(
                es => from e in es
                      where null == e.Manager
                      select e,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Null_Deep()
        {
            AssertQuery<Employee>(
                es => from e in es
                      where e.Manager.Manager == null
                      select e,
                es => from e in es
                      where (e.Manager != null ? e.Manager.Manager : null) == null
                      select e,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Equals_Navigation()
        {
            using (var context = CreateContext())
            {
                var orders
                    = (from o1 in context.Set<Order>()
                       from o2 in context.Set<Order>()
                       where o1.Customer == o2.Customer
                       select new { o1, o2 }).ToList();

                Assert.Equal(10712, orders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Included()
        {
            Func<IQueryable<Order>, IQueryable<Order>> queryFunc =
                os => from o in os.Include(o => o.Customer)
                      where o.Customer.City == "Seattle"
                      select o;

            using (var context = CreateContext())
            {
                var result = queryFunc(context.Orders).ToList();

                Assert.Equal(14, result.Count);
                Assert.True(result.All(o => o.Customer != null));
            }

            ClearLog();

            AssertQuery(
                queryFunc,
                os => from o in os
                      where o.Customer.City == "Seattle"
                      select o,
                entryCount: 15);
        }

        [ConditionalFact]
        public virtual void Select_count_plus_sum()
        {
            AssertQuery<Order>(os => os.Select(o => new
            {
                Total = o.OrderDetails.Sum(od => od.Quantity) + o.OrderDetails.Count()
            }));
        }

        [ConditionalFact]
        public virtual void Singleton_Navigation_With_Member_Access()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.Customer.City == "Seattle"
                      where o.Customer.Phone != "555 555 5555"
                      select new { B = o.Customer.City });
        }

        [ConditionalFact]
        public virtual void Select_Singleton_Navigation_With_Member_Access()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.Customer.City == "Seattle"
                      where o.Customer.Phone != "555 555 5555"
                      select new { A = o.Customer, B = o.Customer.City },
                      entryCount: 1);
        }

        [ConditionalFact]
        public virtual async Task Select_Singleton_Navigation_With_Member_Access_Async()
        {
            using (var context = CreateContext())
            {
                var orders
                    = await (from o in context.Set<Order>()
                             where o.Customer.City == "Seattle"
                             where o.Customer.Phone != "555 555 5555"
                             select new { A = o.Customer, B = o.Customer.City }).ToListAsync();

                Assert.Equal(14, orders.Count);
                Assert.True(orders.All(o => (o.A != null) && (o.B != null)));
            }
        }

        [ConditionalFact]
        public virtual void Select_Where_Navigation_Multiple_Access()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.Customer.City == "Seattle"
                            && o.Customer.Phone != "555 555 5555"
                      select o,
                entryCount: 14);
        }

        [ConditionalFact]
        public virtual void Select_Navigation()
        {
            AssertQuery<Order>(
                os => from o in os
                      select o.Customer,
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual void Select_Navigations()
        {
            AssertQuery<Order>(
                os => from o in os
                      select new { A = o.Customer, B = o.Customer },
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual void Select_Navigations_Where_Navigations()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.Customer.City == "Seattle"
                      where o.Customer.Phone != "555 555 5555"
                      select new { A = o.Customer, B = o.Customer },
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Select_collection_navigation_simple()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      select new { c.CustomerID, c.Orders },
                asserter: (l2oItems, efItems) =>
                {
                    foreach (var pair in
                        from dynamic l2oItem in l2oItems
                        join dynamic efItem in efItems on l2oItem.CustomerID equals efItem.CustomerID
                        select new { l2oItem, efItem })
                    {
                        Assert.Equal(pair.l2oItem.Orders, pair.efItem.Orders);
                    }
                });
        }

        [ConditionalFact]
        public virtual void Select_collection_navigation_multi_part()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.CustomerID == "ALFKI"
                      select new { o.OrderID, o.Customer.Orders },
                asserter: (l2oItems, efItems) =>
                {
                    foreach (var pair in
                        from dynamic l2oItem in l2oItems
                        join dynamic efItem in efItems on l2oItem.OrderID equals efItem.OrderID
                        select new { l2oItem, efItem })
                    {
                        Assert.Equal(pair.l2oItem.Orders, pair.efItem.Orders);
                    }
                });
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_any()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { Any = c.Orders.Any() },
                cs => from c in cs
                      select new { Any = (c.Orders ?? new List<Order>()).Any() });
        }


        [ConditionalFact]
        public virtual void Collection_select_nav_prop_predicate()
        {
            AssertQuery<Customer, bool>(
                cs => cs.Select(c => c.Orders.Count > 0),
                cs => cs.Select(c => (c.Orders ?? new List<Order>()).Count > 0));
        }

        [ConditionalFact]
        public virtual void Collection_where_nav_prop_any()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.Orders.Any()
                      select c,

                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).Any()
                      select c,
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual void Collection_where_nav_prop_any_predicate()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.Orders.Any(o => o.OrderID > 0)
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).Any(o => o.OrderID > 0)
                      select c,
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_all()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { All = c.Orders.All(o => o.CustomerID == "ALFKI") },
                cs => from c in cs
                      select new { All = (c.Orders ?? new List<Order>()).All(o => o.CustomerID == "ALFKI") });
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_all_client()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { All = c.Orders.All(o => o.ShipCity == "London") },
                cs => from c in cs
                      select new { All = (c.Orders ?? new List<Order>()).All(o => o.ShipCity == "London") });
        }

        [ConditionalFact]
        public virtual void Collection_where_nav_prop_all()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.Orders.All(o => o.CustomerID == "ALFKI")
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).All(o => o.CustomerID == "ALFKI")
                      select c,
                entryCount: 3);
        }

        [ConditionalFact]
        public virtual void Collection_where_nav_prop_all_client()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       where c.Orders.All(o => o.ShipCity == "London")
                       select c).ToList();

                Assert.Equal(2, customers.Count);
            }
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_count()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { c.Orders.Count },
                cs => from c in cs
                      select new { (c.Orders ?? new List<Order>()).Count });
        }

        [ConditionalFact]
        public virtual void Collection_where_nav_prop_count()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.Orders.Count() > 5
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).Count() > 5
                      select c,
                entryCount: 63);
        }

        [ConditionalFact]
        public virtual void Collection_where_nav_prop_count_reverse()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where 5 < c.Orders.Count()
                      select c,
                cs => from c in cs
                      where 5 < (c.Orders ?? new List<Order>()).Count()
                      select c,
                entryCount: 63);
        }

        [ConditionalFact]
        public virtual void Collection_orderby_nav_prop_count()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      orderby c.Orders.Count()
                      select c,
                cs => from c in cs
                      orderby (c.Orders ?? new List<Order>()).Count()
                      select c,
                entryCount: 91);
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_long_count()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { C = c.Orders.LongCount() },
                cs => from c in cs
                      select new { C = (c.Orders ?? new List<Order>()).LongCount() });
        }

        [ConditionalFact]
        public virtual void Select_multiple_complex_projections()
        {
            AssertQuery<Order>(
                os => from o in os
                      where o.CustomerID.StartsWith("A")
                      select new
                      {
                          collection1 = o.OrderDetails.Count(),
                          scalar1 = o.OrderDate,
                          any = o.OrderDetails.Select(od => od.UnitPrice).Any(up => up > 10),
                          conditional = o.CustomerID == "ALFKI" ? "50" : "10",
                          scalar2 = (int?)o.OrderID,
                          all = o.OrderDetails.All(od => od.OrderID == 42),
                          collection2 = o.OrderDetails.LongCount(),
                      });
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_sum()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { Sum = c.Orders.Sum(o => o.OrderID) },
                cs => from c in cs
                      select new { Sum = (c.Orders ?? new List<Order>()).Sum(o => o.OrderID) });
        }

        [ConditionalFact]
        public virtual void Collection_where_nav_prop_sum()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      where c.Orders.Sum(o => o.OrderID) > 1000
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).Sum(o => o.OrderID) > 1000
                      select c,
                entryCount: 89);
        }

        [ConditionalFact]
        public virtual async Task Collection_where_nav_prop_sum_async()
        {
            using (var context = CreateContext())
            {
                var customers
                    = await (from c in context.Set<Customer>()
                             where c.Orders.Sum(o => o.OrderID) > 1000
                             select c).ToListAsync();

                Assert.Equal(89, customers.Count);
            }
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_first_or_default()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { First = c.Orders.FirstOrDefault() },
                cs => from c in cs
                      select new { First = (c.Orders ?? new List<Order>()).FirstOrDefault() });
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_first_or_default_then_nav_prop()
        {
            var orderIds = new[] { 10643, 10692, 10702, 10835, 10952, 11011 };

            AssertQuery<Customer>(
                cs => from c in cs.Where(e => e.CustomerID.StartsWith("A"))
                      select new { c.Orders.Where(e => orderIds.Contains(e.OrderID)).FirstOrDefault().Customer },
                cs => from c in cs.Where(e => e.CustomerID.StartsWith("A"))
                      select new { Customer = c.Orders != null && c.Orders.Where(e => orderIds.Contains(e.OrderID)).Any() 
                        ? c.Orders.Where(e => orderIds.Contains(e.OrderID)).First().Customer 
                        : null });
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_first_or_default_then_nav_prop_nested()
        {
            AssertQuery<Customer, Order, string>(
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(c => os.FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City));
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_single_or_default_then_nav_prop_nested()
        {
            AssertQuery<Customer, Order, string>(
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(c => os.SingleOrDefault(o => o.OrderID == 10643).Customer.City));
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method()
        {
            AssertQuery<Customer, Order, string>(
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(c => EF.Property<string>(
                        EF.Property<Customer>(
                            os.FirstOrDefault(oo => oo.CustomerID == "ALFKI"),
                            "Customer"), 
                        "City")),
                (cs, os) => cs.Where(c => c.CustomerID.StartsWith("A"))
                    .Select(c => os.FirstOrDefault(o => o.CustomerID == "ALFKI").Customer != null 
                        ? os.FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City
                        : null)
                );
        }

        [ConditionalFact]
        public virtual void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby()
        {
            AssertQuery<Customer, Order, string>(
                // ReSharper disable once StringStartsWithIsCultureSpecific
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(c => os.OrderBy(o => o.CustomerID).FirstOrDefault(o =>o.CustomerID == "ALFKI").Customer.City));
        }

        [ConditionalFact]
        public virtual void Navigation_fk_based_inside_contains()
        {
            AssertQuery<Order>(
                os => from o in os
                      where new[] { "ALFKI" }.Contains(o.Customer.CustomerID)
                      select o,
                entryCount: 6);
        }

        [ConditionalFact]
        public virtual void Navigation_inside_contains()
        {
            AssertQuery<Order>(
                os => from o in os
                      where new[] { "Novigrad", "Seattle" }.Contains(o.Customer.City)
                      select o,
                entryCount: 14);
        }

        [ConditionalFact]
        public virtual void Navigation_inside_contains_nested()
        {
            AssertQuery<OrderDetail, OrderDetail>(
                ods => from od in ods
                       where new[] { "Novigrad", "Seattle" }.Contains(od.Order.Customer.City)
                       select od,
                asserter: (l2oItems, efItems) =>
                {
                    var l2oIds = l2oItems.Select(i => new { i.OrderID, i.ProductID });
                    var efIds = efItems.Select(i => new { i.OrderID, i.ProductID });

                    foreach (var efId in efIds)
                    {
                        Assert.True(l2oIds.Contains(efId));
                    }

                    foreach (var l2oId in l2oIds)
                    {
                        Assert.True(efIds.Contains(l2oId));
                    }
                },
                entryCount: 40);
        }

        [ConditionalFact]
        public virtual void Navigation_from_join_clause_inside_contains()
        {
            AssertQuery<OrderDetail, Order, OrderDetail>(
                (ods, os) => from od in ods
                             join o in os on od.OrderID equals o.OrderID
                             where new[] { "USA", "Redania" }.Contains(o.Customer.Country)
                             select od,
                asserter: (l2oItems, efItems) =>
                {
                    var l2oIds = l2oItems.Select(i => new { i.OrderID, i.ProductID });
                    var efIds = efItems.Select(i => new { i.OrderID, i.ProductID });

                    foreach (var efId in efIds)
                    {
                        Assert.True(l2oIds.Contains(efId));
                    }

                    foreach (var l2oId in l2oIds)
                    {
                        Assert.True(efIds.Contains(l2oId));
                    }
                },
                entryCount: 352);
        }

        [ConditionalFact]
        public virtual void Where_subquery_on_navigation()
        {
            AssertQuery<Product, OrderDetail, Product>(
                (ps, ods) => from p in ps
                             where p.OrderDetails.Contains(ods.FirstOrDefault(orderDetail => orderDetail.Quantity == 1))
                             select p,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_subquery_on_navigation2()
        {
            AssertQuery<Product, OrderDetail, Product>(
                (ps, ods) => from p in ps
                             where p.OrderDetails.Contains(ods.OrderByDescending(o => o.OrderID).ThenBy(o => o.ProductID).FirstOrDefault())
                             select p,
                entryCount: 1);
        }

        [ConditionalFact]
        public virtual void Where_subquery_on_navigation_client_eval()
        {
            AssertQuery<Customer, Order, Customer>(
                (cs, os) => from c in cs
                            where c.Orders.Select(o => o.OrderID)
                                .Contains(
                                    os.OrderByDescending(o => ClientMethod(o.OrderID)).Select(o => o.OrderID).FirstOrDefault())
                            select c,
                entryCount: 1);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private int ClientMethod(int argument) => argument;

        [ConditionalFact]
        public virtual void Navigation_in_subquery_referencing_outer_query()
        {
            using (var context = CreateContext())
            {
                var query = from o in context.Orders
                                // ReSharper disable once UseMethodAny.0
                            where (from od in context.OrderDetails
                                   where o.Customer.Country == od.Order.Customer.Country
                                   select od).Count() > 0
                            select o;

                var result = query.ToList();

                Assert.Equal(830, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void GroupBy_on_nav_prop()
        {
            AssertQuery<Order, IGrouping<string, Order>>(
                os => from o in os
                      group o by o.Customer.City into og
                      select og,
            asserter: (l2oItems, efItems) =>
            {
                foreach (var pair in
                    from l2oItem in l2oItems
                    join efItem in efItems on l2oItem.Key equals efItem.Key
                    select new { l2oItem, efItem })
                {
                    Assert.Equal(
                        pair.l2oItem.Select(i => i.OrderID).OrderBy(i => i),
                        pair.efItem.Select(i => i.OrderID).OrderBy(i => i));
                }
            });
        }

        [ConditionalFact]
        public virtual void Where_nav_prop_group_by()
        {
            AssertQuery<OrderDetail, IGrouping<short, OrderDetail>>(
                ods => from od in ods
                      where od.Order.CustomerID == "ALFKI"
                      group od by od.Quantity,
                asserter: (l2oItems, efItems) =>
                    {
                        foreach (var pair in
                            from l2oItem in l2oItems
                            join efItem in efItems on l2oItem.Key equals efItem.Key
                            select new { l2oItem, efItem })
                        {
                            Assert.Equal(
                                pair.l2oItem.Select(i => i.OrderID).OrderBy(i => i),
                                pair.efItem.Select(i => i.OrderID).OrderBy(i => i));
                        }
                    });
        }

        [ConditionalFact]
        public virtual void Let_group_by_nav_prop()
        {
            AssertQuery<OrderDetail, IGrouping<string, OrderDetail>>(
                ods => from od in ods
                       let customer = od.Order.CustomerID
                       group od by customer into odg
                       select odg,
                asserter: (l2oItems, efItems) =>
                {
                    foreach (var pair in
                        from l2oItem in l2oItems
                        join efItem in efItems on l2oItem.Key equals efItem.Key
                        select new { l2oItem, efItem })
                    {
                        Assert.Equal(
                            pair.l2oItem.Select(i => i.OrderID).OrderBy(i => i),
                            pair.efItem.Select(i => i.OrderID).OrderBy(i => i));
                    }
                });
        }

        protected QueryNavigationsTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class 
            => AssertQuery(query, query, assertOrder, entryCount, asserter);

        protected void AssertQuery<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<TResult>, IList<TResult>> asserter = null)
            where TItem1 : class
            where TItem2 : class 
            => AssertQuery(query, query, assertOrder, entryCount, asserter);

        protected void AssertQuery<TItem, TResult>(
            Func<IQueryable<TItem>, IQueryable<TResult>> query,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<TResult>, IList<TResult>> asserter = null)
            where TItem : class 
            => AssertQuery(query, query, assertOrder, entryCount, asserter);

        protected void AssertQuery<TItem>(
            Func<IQueryable<TItem>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem>, IQueryable<object>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<object>, IList<object>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    efQuery(context.Set<TItem>()).ToArray(),
                    assertOrder,
                    asserter);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }

        protected void AssertQuery<TItem, TResult>(
            Func<IQueryable<TItem>, IQueryable<TResult>> efQuery,
            Func<IQueryable<TItem>, IQueryable<TResult>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<TResult>, IList<TResult>> asserter = null)
            where TItem : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem>()).ToArray(),
                    efQuery(context.Set<TItem>()).ToArray(),
                    assertOrder,
                    asserter);
            }
        }

        protected void AssertQuery<TItem1, TItem2, TResult>(
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<TItem2>, IQueryable<TResult>> l2oQuery,
            bool assertOrder = false,
            int entryCount = 0,
            Action<IList<TResult>, IList<TResult>> asserter = null)
            where TItem1 : class
            where TItem2 : class
        {
            using (var context = CreateContext())
            {
                TestHelpers.AssertResults(
                    l2oQuery(NorthwindData.Set<TItem1>(), NorthwindData.Set<TItem2>()).ToArray(),
                    efQuery(context.Set<TItem1>(), context.Set<TItem2>()).ToArray(),
                    assertOrder,
                    asserter);

                Assert.Equal(entryCount, context.ChangeTracker.Entries().Count());
            }
        }
    }
}
