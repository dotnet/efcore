// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable UnusedVariable
// ReSharper disable ConvertToExpressionBodyWhenPossible
// ReSharper disable InconsistentNaming
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ReplaceWithSingleCallToFirstOrDefault
// ReSharper disable ReplaceWithSingleCallToAny
// ReSharper disable ReplaceWithSingleCallToFirst
// ReSharper disable StringStartsWithIsCultureSpecific
// ReSharper disable UseCollectionCountProperty
// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleUnintendedReferenceComparison

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class QueryNavigationsTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected QueryNavigationsTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_with_nav_projected_in_subquery_when_client_eval(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed(
                    "Join<Customer, TransparentIdentifier<Order, Customer>, string, TransparentIdentifier<Customer, TransparentIdentifier<Order, Customer>>>(    outer: DbSet<Customer>,     inner: LeftJoin<Order, Customer, string, TransparentIdentifier<Order, Customer>>(        outer: DbSet<Order>,         inner: DbSet<Customer>,         outerKeySelector: (o) => Property<string>(o, \"CustomerID\"),         innerKeySelector: (c0) => Property<string>(c0, \"CustomerID\"),         resultSelector: (o, i) => new TransparentIdentifier<Order, Customer>(            Outer = o,             Inner = i        )),     outerKeySelector: (c) => c.CustomerID,     innerKeySelector: (o) => ClientProjection<Order>(        t: o.Outer,         _: o.Inner).CustomerID,     resultSelector: (c, o) => new TransparentIdentifier<Customer, TransparentIdentifier<Order, Customer>>(        Outer = c,         Inner = o    ))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer, Order, OrderDetail>(
                            isAsync,
                            (cs, os, ods) => (from c in cs
                                              join o in os.Select(o => ClientProjection(o, o.Customer)) on c.CustomerID equals o.CustomerID
                                              join od in ods.Select(od => ClientProjection(od, od.Product)) on o.OrderID equals od.OrderID
                                              select c),
                            entryCount: 89))).Message));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_nav_projected_in_subquery_when_client_eval(bool isAsync)
        {
            return AssertQuery<Customer, Order, OrderDetail>(
                isAsync,
                (cs, os, ods) => (from c in cs
                                  join o in os.Select(o => ClientProjection(o, o.Customer)) on c.CustomerID equals o.CustomerID into
                                      grouping
                                  from o in grouping
                                  join od in ods.Select(od => ClientProjection(od, od.Product)) on o.OrderID equals od.OrderID into
                                      grouping2
                                  from od in grouping2
                                  select c),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_with_nav_in_predicate_in_subquery_when_client_eval(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("Where<TransparentIdentifier<Order, Customer>>(    source: LeftJoin<Order, Customer, string, TransparentIdentifier<Order, Customer>>(        outer: DbSet<Order>,         inner: DbSet<Customer>,         outerKeySelector: (o) => Property<string>(o, \"CustomerID\"),         innerKeySelector: (c0) => Property<string>(c0, \"CustomerID\"),         resultSelector: (o, i) => new TransparentIdentifier<Order, Customer>(            Outer = o,             Inner = i        )),     predicate: (o) => ClientPredicate<Order>(        t: o.Outer,         _: o.Inner))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer, Order, OrderDetail>(
                            isAsync,
                            (cs, os, ods) => (from c in cs
                                              join o in os.Where(o => ClientPredicate(o, o.Customer)) on c.CustomerID equals o.CustomerID
                                              join od in ods.Where(od => ClientPredicate(od, od.Product)) on o.OrderID equals od.OrderID
                                              select c),
                            entryCount: 89))).Message));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_nav_in_predicate_in_subquery_when_client_eval(bool isAsync)
        {
            return AssertQuery<Customer, Order, OrderDetail>(
                isAsync,
                (cs, os, ods) => (from c in cs
                                  join o in os.Where(o => ClientPredicate(o, o.Customer)) on c.CustomerID equals o.CustomerID into grouping
                                  from o in grouping
                                  join od in ods.Where(od => ClientPredicate(od, od.Product)) on o.OrderID equals od.OrderID into grouping2
                                  from od in grouping2
                                  select c),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_with_nav_in_orderby_in_subquery_when_client_eval(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("OrderBy<TransparentIdentifier<Order, Customer>, int>(    source: LeftJoin<Order, Customer, string, TransparentIdentifier<Order, Customer>>(        outer: DbSet<Order>,         inner: DbSet<Customer>,         outerKeySelector: (o) => Property<string>(o, \"CustomerID\"),         innerKeySelector: (c0) => Property<string>(c0, \"CustomerID\"),         resultSelector: (o, i) => new TransparentIdentifier<Order, Customer>(            Outer = o,             Inner = i        )),     keySelector: (o) => ClientOrderBy<Order>(        t: o.Outer,         _: o.Inner))"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery<Customer, Order, OrderDetail>(
                        isAsync,
                        (cs, os, ods) => (from c in cs
                                          join o in os.OrderBy(o => ClientOrderBy(o, o.Customer)) on c.CustomerID equals o.CustomerID
                                          join od in ods.OrderBy(od => ClientOrderBy(od, od.Product)) on o.OrderID equals od.OrderID
                                          select c),
                        entryCount: 89))).Message));
        }

        private string RemoveNewLines(string message)
            => message.Replace("\n", "").Replace("\r", "");

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_nav_in_orderby_in_subquery_when_client_eval(bool isAsync)
        {
            return AssertQuery<Customer, Order, OrderDetail>(
                isAsync,
                (cs, os, ods) => (from c in cs
                                  join o in os.OrderBy(o => ClientOrderBy(o, o.Customer)) on c.CustomerID equals o.CustomerID into grouping
                                  from o in grouping
                                  join od in ods.OrderBy(od => ClientOrderBy(od, od.Product)) on o.OrderID equals od.OrderID into grouping2
                                  from od in grouping2
                                  select c),
                entryCount: 89);
        }

        private static readonly Random _randomGenerator = new Random();
        private static T ClientProjection<T>(T t, object _) => t;
        private static bool ClientPredicate<T>(T t, object _) => true;
        private static int ClientOrderBy<T>(T t, object _) => _randomGenerator.Next(0, 20);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where o.Customer.City == "Seattle"
                      select o,
                entryCount: 14);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Contains(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where o.Customer.City.Contains("Sea")
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o1 in os.Where(o => o.OrderID < 10300)
                      from o2 in os.Where(o => o.OrderID < 10400)
                      where o1.Customer.City == o2.Customer.City
                      select new
                      {
                          o1, o2
                      },
                elementSorter: e => e.o1.OrderID + " " + e.o2.OrderID,
                elementAsserter: (e, a) => Assert.Equal(e.o1.OrderID, a.o1.OrderID),
                entryCount: 107);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o1 in os.Where(o => o.OrderID < 10300)
                      from o2 in os.Where(o => o.OrderID < 10400)
                      where o1.Customer.City == o2.Customer.City
                      select new
                      {
                          o1.CustomerID,
                          C2 = o2.CustomerID
                      },
                elementSorter: e => e.CustomerID + " " + e.C2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Select_Where_Navigation_Client(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed(
                    "Where<TransparentIdentifier<Order, Customer>>(    source: LeftJoin<Order, Customer, string, TransparentIdentifier<Order, Customer>>(        outer: DbSet<Order>,         inner: DbSet<Customer>,         outerKeySelector: (o) => Property<string>(o, \"CustomerID\"),         innerKeySelector: (c) => Property<string>(c, \"CustomerID\"),         resultSelector: (o, i) => new TransparentIdentifier<Order, Customer>(            Outer = o,             Inner = i        )),     predicate: (o) => o.Inner.IsLondon)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Order>(
                            isAsync,
                            os => from o in os
                                  where o.Customer.IsLondon
                                  select o,
                            entryCount: 46))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Deep(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods => (from od in ods
                        where od.Order.Customer.City == "Seattle"
                        orderby od.OrderID, od.ProductID
                        select od).Take(1),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Take_Select_Navigation(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(2)
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_FirstOrDefault_project_single_column1(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(2).Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault().CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_FirstOrDefault_project_single_column2(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(2)
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).Select(o => o.CustomerID).FirstOrDefault()));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_FirstOrDefault_project_anonymous_type(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(2).Select(
                    c => c.Orders.OrderBy(o => o.OrderID).Select(
                        o => new
                        {
                            o.CustomerID,
                            o.OrderID
                        }).FirstOrDefault()),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_FirstOrDefault_project_entity(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID).Take(2).Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()),
                entryCount: 2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Skip_Select_Navigation(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => cs.OrderBy(c => c.CustomerID)
                    .Skip(20)
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()),
                entryCount: 69,
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Null(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => from e in es
                      where e.Manager == null
                      select e,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Null_Reverse(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => from e in es
                      where null == e.Manager
                      select e,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Null_Deep(bool isAsync)
        {
            return AssertQuery<Employee>(
                isAsync,
                es => from e in es
                      where e.Manager.Manager == null
                      select e,
                es => from e in es
                      where Maybe(e.Manager, () => e.Manager.Manager) == null
                      select e,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Equals_Navigation(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o1 in os
                      from o2 in os
                      where o1.CustomerID.StartsWith("A")
                      where o2.CustomerID.StartsWith("A")
                      where o1.Customer == o2.Customer
                      select new
                      {
                          o1,
                          o2
                      },
                elementSorter: e => e.o1.OrderID + " " + e.o2.OrderID,
                entryCount: 30);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Included(bool isAsync)
        {
            return AssertIncludeQuery<Order>(
                isAsync,
                os => from o in os.Include(o => o.Customer)
                      where o.Customer.City == "Seattle"
                      select o,
                new List<IExpectedInclude>
                {
                    new ExpectedInclude<Order>(o => o.Customer, "Customer")
                },
                entryCount: 15);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Include_with_multiple_optional_navigations(bool isAsync)
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<OrderDetail>(od => od.Order, "Order"),
                new ExpectedInclude<Order>(o => o.Customer, "Customer", "Order")
            };

            return AssertIncludeQuery<OrderDetail>(
                isAsync,
                ods => ods
                    .Include(od => od.Order.Customer)
                    .Where(od => od.Order.Customer.City == "London"),
                expectedIncludes,
                entryCount: 164);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_count_plus_sum(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Select(
                    o => new
                    {
                        Total = o.OrderDetails.Sum(od => od.Quantity) + o.OrderDetails.Count()
                    }),
                elementSorter: e => e.Total);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where o.Customer.City == "Seattle"
                      where o.Customer.Phone != "555 555 5555"
                      select new
                      {
                          B = o.Customer.City
                      },
                elementSorter: e => e.B);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Singleton_Navigation_With_Member_Access(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where o.Customer.City == "Seattle"
                      where o.Customer.Phone != "555 555 5555"
                      select new
                      {
                          A = o.Customer,
                          B = o.Customer.City
                      },
                elementSorter: e => e.A + " " + e.B,
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
                             select new
                             {
                                 A = o.Customer,
                                 B = o.Customer.City
                             }).ToListAsync();

                Assert.Equal(14, orders.Count);
                Assert.True(orders.All(o => (o.A != null) && (o.B != null)));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Where_Navigation_Multiple_Access(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where o.Customer.City == "Seattle"
                            && o.Customer.Phone != "555 555 5555"
                      select o,
                entryCount: 14);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Navigation(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      select o.Customer,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Navigations(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      select new
                      {
                          A = o.Customer,
                          B = o.Customer
                      },
                elementSorter: e => e.A.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.A.CustomerID, a.A.CustomerID);
                    Assert.Equal(e.B.CustomerID, a.B.CustomerID);
                },
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_Navigations_Where_Navigations(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where o.Customer.City == "Seattle"
                      where o.Customer.Phone != "555 555 5555"
                      select new
                      {
                          A = o.Customer,
                          B = o.Customer
                      },
                elementSorter: e => e.A.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.A.CustomerID, a.A.CustomerID);
                    Assert.Equal(e.B.CustomerID, a.B.CustomerID);
                },
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_navigation_simple(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      orderby c.CustomerID
                      select new
                      {
                          c.CustomerID,
                          c.Orders
                      },
                elementSorter: e => e.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    CollectionAsserter<Order>(o => o.OrderID, (ee, aa) => Assert.Equal(ee.OrderID, aa.OrderID))(e.Orders, a.Orders);
                },
                entryCount: 30);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_navigation_simple2(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      orderby c.CustomerID
                      select new
                      {
                          c.CustomerID,
                          c.Orders.Count
                      },
                elementSorter: e => e.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    Assert.Equal(e.Count, a.Count);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_navigation_simple_followed_by_ordering_by_scalar(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => (from c in cs
                       where c.CustomerID.StartsWith("A")
                       orderby c.CustomerID
                       select new
                       {
                           c.CustomerID,
                           c.Orders
                       }).OrderBy(e => e.CustomerID),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    CollectionAsserter<Order>(o => o.OrderID, (ee, aa) => Assert.Equal(ee.OrderID, aa.OrderID))(e.Orders, a.Orders);
                },
                assertOrder: true,
                entryCount: 30);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_navigation_multi_part(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where o.CustomerID == "ALFKI"
                      select new
                      {
                          o.OrderID,
                          o.Customer.Orders
                      },
                elementSorter: e => e.OrderID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.OrderID, a.OrderID);
                    CollectionAsserter<Order>(o => o.OrderID, (ee, aa) => Assert.Equal(ee.OrderID, aa.OrderID))(e.Orders, a.Orders);
                },
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_collection_navigation_multi_part2(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods =>
                    from od in ods
                    orderby od.OrderID, od.ProductID
                    where od.Order.CustomerID == "ALFKI" || od.Order.CustomerID == "ANTON"
                    select new
                    {
                        od.Order.Customer.Orders
                    },
                assertOrder: true,
                elementAsserter: (e, a) => CollectionAsserter<Order>(ee => ee.OrderID, (ee, aa) => Assert.Equal(ee.OrderID, aa.OrderID)),
                entryCount: 13);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_any(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      select new
                      {
                          Any = c.Orders.Any()
                      },
                cs => from c in cs
                      select new
                      {
                          Any = (c.Orders ?? new List<Order>()).Any()
                      },
                elementSorter: e => e.Any);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_predicate(bool isAsync)
        {
            return AssertQueryScalar<Customer>(
                isAsync,
                cs => cs.Select(c => c.Orders.Count > 0),
                cs => cs.Select(c => (c.Orders ?? new List<Order>()).Count > 0));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_where_nav_prop_any(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.Orders.Any()
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).Any()
                      select c,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_where_nav_prop_any_predicate(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.Orders.Any(o => o.OrderID > 0)
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).Any(o => o.OrderID > 0)
                      select c,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_all(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      select new
                      {
                          All = c.Orders.All(o => o.CustomerID == "ALFKI")
                      },
                cs => from c in cs
                      select new
                      {
                          All = (c.Orders ?? new List<Order>()).All(o => o.CustomerID == "ALFKI")
                      },
                elementSorter: e => e.All);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Collection_select_nav_prop_all_client(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed("All<Order>(    source: Where<Order>(        source: DbSet<Order>,         predicate: (o0) => Property<string>(EntityShaperExpression:             EntityType: Customer            ValueBufferExpression:                 ProjectionBindingExpression: EmptyProjectionMember            IsNullable: False        , \"CustomerID\") == Property<string>(o0, \"CustomerID\")),     predicate: (o0) => o0.ShipCity == \"London\")"),
                RemoveNewLines((await Assert.ThrowsAsync<InvalidOperationException>(
                    () => AssertQuery<Customer>(
                        isAsync,
                        cs => from c in cs
                              orderby c.CustomerID
                              select new
                              {
                                  All = c.Orders.All(o => o.ShipCity == "London")
                              },
                        cs => from c in cs
                              orderby c.CustomerID
                              select new
                              {
                                  All = (c.Orders ?? new List<Order>()).All(o => false)
                              },
                        assertOrder: true))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_where_nav_prop_all(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
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
                Assert.Equal(
                    CoreStrings.TranslationFailed(
                        "All<Order>(    source: Where<Order>(        source: DbSet<Order>,         predicate: (o) => Property<string>(EntityShaperExpression:             EntityType: Customer            ValueBufferExpression:                 ProjectionBindingExpression: EmptyProjectionMember            IsNullable: False        , \"CustomerID\") == Property<string>(o, \"CustomerID\")),     predicate: (o) => o.ShipCity == \"London\")"),
                    RemoveNewLines(
                        Assert.Throws<InvalidOperationException>(
                            () => (from c in context.Set<Customer>()
                                   orderby c.CustomerID
                                   where c.Orders.All(o => o.ShipCity == "London")
                                   select c).ToList()).Message));
            }
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_count(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      select new
                      {
                          c.Orders.Count
                      },
                cs => from c in cs
                      select new
                      {
                          (c.Orders ?? new List<Order>()).Count
                      },
                elementSorter: e => e.Count);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_where_nav_prop_count(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.Orders.Count() > 5
                      select c,
                cs => from c in cs
                      where (c.Orders ?? new List<Order>()).Count() > 5
                      select c,
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_where_nav_prop_count_reverse(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where 5 < c.Orders.Count()
                      select c,
                cs => from c in cs
                      where 5 < (c.Orders ?? new List<Order>()).Count()
                      select c,
                entryCount: 63);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_orderby_nav_prop_count(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      orderby c.Orders.Count(), c.CustomerID
                      select c,
                cs => from c in cs
                      orderby (c.Orders ?? new List<Order>()).Count(), c.CustomerID
                      select c,
                assertOrder: true,
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_long_count(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      select new
                      {
                          C = c.Orders.LongCount()
                      },
                cs => from c in cs
                      select new
                      {
                          C = (c.Orders ?? new List<Order>()).LongCount()
                      },
                elementSorter: e => e.C);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Select_multiple_complex_projections(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
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
                          collection2 = o.OrderDetails.LongCount()
                      },
                elementSorter: e => e.scalar2);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_sum(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      select new
                      {
                          Sum = c.Orders.Sum(o => o.OrderID)
                      },
                cs => from c in cs
                      select new
                      {
                          Sum = (c.Orders ?? new List<Order>()).Sum(o => o.OrderID)
                      },
                elementSorter: e => e.Sum);
        }

        // issue #12657
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_sum_plus_one(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      select new
                      {
                          Sum = c.Orders.Sum(o => o.OrderID) + 1
                      },
                cs => from c in cs
                      select new
                      {
                          Sum = (c.Orders ?? new List<Order>()).Sum(o => o.OrderID) + 1
                      },
                elementSorter: e => e.Sum);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_where_nav_prop_sum(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
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

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_first_or_default(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      orderby c.CustomerID
                      select new
                      {
                          First = c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()
                      },
                cs => from c in cs
                      orderby c.CustomerID
                      select new
                      {
                          First = (c.Orders ?? new List<Order>()).FirstOrDefault()
                      },
                assertOrder: true,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop(bool isAsync)
        {
            var orderIds = new[] { 10643, 10692, 10702, 10835, 10952, 11011 };

            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs.Where(e => e.CustomerID.StartsWith("A"))
                      orderby c.CustomerID
                      select new
                      {
                          c.Orders.Where(e => orderIds.Contains(e.OrderID)).FirstOrDefault().Customer
                      },
                cs => from c in cs.Where(e => e.CustomerID.StartsWith("A"))
                      orderby c.CustomerID
                      select new
                      {
#pragma warning disable RCS1146 // Use conditional access.
                          Customer = c.Orders != null && c.Orders.Where(e => orderIds.Contains(e.OrderID)).Any()
#pragma warning restore RCS1146 // Use conditional access.
                              ? c.Orders.Where(e => orderIds.Contains(e.OrderID)).First().Customer
                              : null
                      },
                elementSorter: e => e.Customer?.CustomerID,
                elementAsserter: (e, a) => Assert.Equal(e.Customer?.CustomerID, a.Customer?.CustomerID),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop_nested(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(c => os.FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_single_or_default_then_nav_prop_nested(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(c => os.SingleOrDefault(o => o.OrderID == 10643).Customer.City));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(
                        c => EF.Property<string>(
                            EF.Property<Customer>(
                                os.FirstOrDefault(oo => oo.CustomerID == "ALFKI"),
                                "Customer"),
                            "City")),
                (cs, os) => cs.Where(c => c.CustomerID.StartsWith("A"))
                    .Select(
                        c => os.FirstOrDefault(o => o.CustomerID == "ALFKI").Customer != null
                            ? os.FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City
                            : null)
            );
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                // ReSharper disable once StringStartsWithIsCultureSpecific
                (cs, os) => cs.Where(e => e.CustomerID.StartsWith("A"))
                    .Select(c => os.OrderBy(o => o.CustomerID).FirstOrDefault(o => o.CustomerID == "ALFKI").Customer.City));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_fk_based_inside_contains(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where new[] { "ALFKI" }.Contains(o.Customer.CustomerID)
                      select o,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_contains(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      where new[] { "Novigrad", "Seattle" }.Contains(o.Customer.City)
                      select o,
                entryCount: 14);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_inside_contains_nested(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods => from od in ods
                       where new[] { "Novigrad", "Seattle" }.Contains(od.Order.Customer.City)
                       select od,
                entryCount: 40);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_from_join_clause_inside_contains(bool isAsync)
        {
            return AssertQuery<OrderDetail, Order>(
                isAsync,
                (ods, os) => from od in ods
                             join o in os on od.OrderID equals o.OrderID
                             where new[] { "USA", "Redania" }.Contains(o.Customer.Country)
                             select od,
                entryCount: 352);
        }

        [ConditionalTheory(Skip = "issue #15260")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_on_navigation(bool isAsync)
        {
            return AssertQuery<Product, OrderDetail>(
                isAsync,
                (ps, ods) => from p in ps
                             where p.OrderDetails.Contains(
                                 ods.OrderByDescending(o => o.OrderID).ThenBy(o => o.ProductID)
                                     .FirstOrDefault(orderDetail => orderDetail.Quantity == 1))
                             select p,
                entryCount: 1);
        }

        [ConditionalTheory(Skip = "issue #15260")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_subquery_on_navigation2(bool isAsync)
        {
            return AssertQuery<Product, OrderDetail>(
                isAsync,
                (ps, ods) => from p in ps
                             where p.OrderDetails.Contains(ods.OrderByDescending(o => o.OrderID).ThenBy(o => o.ProductID).FirstOrDefault())
                             select p,
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Where_subquery_on_navigation_client_eval(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed(
                    "OrderByDescending<Order, int>(    source: DbSet<Order>,     keySelector: (o) => ClientMethod(o.OrderID))"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer, Order>(
                            isAsync,
                            (cs, os) => from c in cs
                                        orderby c.CustomerID
                                        where c.Orders.Select(o => o.OrderID)
                                            .Contains(
                                                os.OrderByDescending(o => ClientMethod(o.OrderID)).Select(o => o.OrderID).FirstOrDefault())
                                        select c,
                            entryCount: 1))).Message));
        }

        // ReSharper disable once MemberCanBeMadeStatic.Local
        private static int ClientMethod(int argument) => argument;

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
                            where o.OrderID == 10643 || o.OrderID == 10692
                            select o;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_in_subquery_referencing_outer_query_with_client_side_result_operator_and_count()
        {
            using (var context = CreateContext())
            {
                var query = from o in context.Orders
                            where o.OrderID == 10643 || o.OrderID == 10692
                            // ReSharper disable once UseMethodAny.0
                            where (from od in context.OrderDetails
                                   where o.Customer.Country == od.Order.Customer.Country
                                   select od).Distinct().Count() > 0
                            select o;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupBy_on_nav_prop(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => from o in os
                      group o by o.Customer.City
                      into og
                      select og,
                elementSorter: GroupingSorter<string, Order>(),
                elementAsserter: GroupingAsserter<string, Order>(o => o.OrderID, (e, a) => Assert.Equal(e.OrderID, a.OrderID)),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Where_nav_prop_group_by(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods => from od in ods
                       where od.Order.CustomerID == "ALFKI"
                       group od by od.Quantity,
                elementSorter: GroupingSorter<short, OrderDetail>(),
                elementAsserter: GroupingAsserter<short, OrderDetail>(
                    e => e.OrderID + " " + e.ProductID,
                    (e, a) =>
                    {
                        Assert.Equal(e.OrderID, a.OrderID);
                        Assert.Equal(e.ProductID, a.ProductID);
                    }),
                entryCount: 12);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Let_group_by_nav_prop(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods => from od in ods
                       let customer = od.Order.CustomerID
                       group od by customer
                       into odg
                       select odg,
                elementSorter: GroupingSorter<string, OrderDetail>(),
                elementAsserter: GroupingAsserter<string, OrderDetail>(
                    e => e.OrderID + " " + e.ProductID,
                    (e, a) =>
                    {
                        Assert.Equal(e.OrderID, a.OrderID);
                        Assert.Equal(e.ProductID, a.ProductID);
                    }),
                entryCount: 2155);
        }

        // issue #12816
        //[ConditionalTheory]
        //[MemberData(nameof(IsAsyncData))]
        public virtual Task Select_anonymous_type_order_by_field_group_by_same_field(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods => ods
                    .Select(od => new { od, customer = od.Order.CustomerID })
                    .OrderBy(e => e.customer)
                    .GroupBy(e => e.customer, elementSelector: e => e.od)
                    .Select(e => e));
        }

        [ConditionalTheory(Skip = "issue #6061")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Project_first_or_default_on_empty_collection_of_value_types_returns_proper_default(bool isAsync)
        {
            Assert.Equal(
                CoreStrings.TranslationFailed(
                    "(o) => ClientMethod(o.OrderID)"),
                RemoveNewLines(
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () => AssertQuery<Customer>(
                            isAsync,
                            cs => from c in cs
                                  where c.CustomerID.Equals("FISSA")
                                  select new
                                  {
                                      c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault()
                                  },
                            cs => from c in cs
                                  select new
                                  {
                                      c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault()
                                  },
                            elementSorter: e => e.CustomerID))).Message));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_scalar_value_subquery_is_properly_inlined(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      select new
                      {
                          c.CustomerID,
                          OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => (int?)o.OrderID).FirstOrDefault()
                      },
                cs => from c in cs
                      select new
                      {
                          c.CustomerID,
                          OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => (int?)o.OrderID).FirstOrDefault()
                      },
                elementSorter: e => e.CustomerID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_entity_value_subquery_works(bool isAsync)
        {
            return AssertQuery<Customer>(
                isAsync,
                cs => from c in cs
                      where c.CustomerID.StartsWith("A")
                      orderby c.CustomerID
                      select new
                      {
                          c.CustomerID,
                          Order = c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()
                      },
                elementSorter: e => e.CustomerID,
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Project_single_scalar_value_subquery_in_query_with_optional_navigation_works(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => (from o in os
                       orderby o.OrderID
                       select new
                       {
                           o.OrderID,
                           OrderDetail = o.OrderDetails.OrderBy(od => od.OrderID).ThenBy(od => od.ProductID).Select(od => od.OrderID)
                               .FirstOrDefault(),
                           o.Customer.City
                       }).Take(3),
                elementSorter: e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_and_LOJ_gets_flattened(bool isAsync)
        {
            return AssertQuery<Customer, Order, OrderDetail>(
                isAsync,
                (cs, os, ods) => (from c in cs
                                  join subquery in
                                      (
                                          from od in ods
                                          join o in os on od.OrderID equals 10260
                                          join c2 in cs on o.CustomerID equals c2.CustomerID
                                          select c2
                                      )
                                      on c.CustomerID equals subquery.CustomerID
                                      into result
                                  from subquery in result.DefaultIfEmpty()
                                  select c),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_complex_subquery_and_LOJ_gets_flattened2(bool isAsync)
        {
            return AssertQuery<Customer, Order, OrderDetail>(
                isAsync,
                (cs, os, ods) => (from c in cs
                                  join subquery in
                                      (
                                          from od in ods
                                          join o in os on od.OrderID equals 10260
                                          join c2 in cs on o.CustomerID equals c2.CustomerID
                                          select c2
                                      )
                                      on c.CustomerID equals subquery.CustomerID
                                      into result
                                  from subquery in result.DefaultIfEmpty()
                                  select c.CustomerID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_with_collection_with_nullable_type_key(bool isAsync)
        {
            return AssertQuery<Order>(
                isAsync,
                os => os.Where(o => o.Customer.Orders.Count(oo => oo.OrderID > 10260) > 30),
                entryCount: 31);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_groupjoin_with_orderby_key_descending(bool isAsync)
        {
            return AssertQueryScalar<Customer, Order>(
                isAsync,
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into grouping
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID descending
                    select grouping.Count());
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_projection_on_groupjoin_qsre(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID == "ALFKI"
                            select new
                            {
                                c,
                                G = grouping.Select(o => o.OrderDetails).ToList()
                            },
                elementSorter: e => e.c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.c.CustomerID, a.c.CustomerID);
                    CollectionAsserter<OrderDetail>(
                        ee => ee.OrderID + " " + ee.ProductID,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.OrderID, aa.OrderID);
                            Assert.Equal(ee.ProductID, aa.ProductID);
                        });
                },
                entryCount: 1);
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID == "ALFKI"
                            orderby c.CustomerID
                            select grouping.Select(o => o.OrderDetails).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    var expected = ((IEnumerable<IEnumerable<OrderDetail>>)e).SelectMany(i => i).ToList();
                    var actual = ((IEnumerable<IEnumerable<OrderDetail>>)e).SelectMany(i => i).ToList();

                    Assert.Equal(expected, actual);
                });
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Navigation_projection_on_groupjoin_qsre_with_empty_grouping(bool isAsync)
        {
            var anatrsOrders = new[] { 10308, 10625, 10759, 10926 };

            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => from c in cs
                            join o in os.Where(oo => !anatrsOrders.Contains(oo.OrderID)) on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID.StartsWith("A")
                            select new
                            {
                                c,
                                G = grouping.Select(o => o.OrderDetails).ToList()
                            },
                elementSorter: e => e.c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.c.CustomerID, a.c.CustomerID);

                    var expected = ((IEnumerable<IEnumerable<OrderDetail>>)e.G).SelectMany(i => i).ToList();
                    var actual = ((IEnumerable<IEnumerable<OrderDetail>>)e.G).SelectMany(i => i).ToList();

                    Assert.Equal(expected, actual);
                },
                entryCount: 4);
        }

        [ConditionalFact(Skip = "Issue#17068")]
        public virtual void Include_on_inner_projecting_groupjoin()
        {
            using (var ctx = CreateContext())
            {
                var query = from c in ctx.Customers
                            join o in ctx.Orders.Include(oo => oo.OrderDetails) on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID == "ALFKI"
                            select grouping.ToList();

                var result = query.ToList();
                Assert.Equal(1, result.Count);
                foreach (var order in result[0])
                {
                    Assert.True(order.OrderDetails.Count > 0);
                }
            }
        }

        [ConditionalFact(Skip = "Issue#17068")]
        public virtual void Include_on_inner_projecting_groupjoin_complex()
        {
            using (var ctx = CreateContext())
            {
                var query = from c in ctx.Customers
                            join o in ctx.Orders.Include(oo => oo.OrderDetails).ThenInclude(od => od.Product) on c.CustomerID equals o
                                .CustomerID into grouping
                            where c.CustomerID == "ALFKI"
                            select grouping.ToList();

                var result = query.ToList();
                Assert.Equal(1, result.Count);
                foreach (var order in result[0])
                {
                    Assert.True(order.OrderDetails.Count > 0);
                    foreach (var detail in order.OrderDetails)
                    {
                        Assert.NotNull(detail.Product);
                    }
                }
            }
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Group_join_doesnt_get_bound_directly_to_group_join_qsre(bool isAsync)
        {
            return AssertQuery<Customer, Order>(
                isAsync,
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID.StartsWith("A")
                            select new
                            {
                                G = grouping.Count()
                            },
                elementSorter: e => e.G);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Multiple_include_with_multiple_optional_navigations(bool isAsync)
        {
            return AssertQuery<OrderDetail>(
                isAsync,
                ods => ods
                    .Include(od => od.Order.Customer)
                    .Include(od => od.Product)
                    .Where(od => od.Order.Customer.City == "London"),
                entryCount: 221);
        }

        private class OrderDTO
        {
            public Customer Customer { get; set; }
        }
    }
}
