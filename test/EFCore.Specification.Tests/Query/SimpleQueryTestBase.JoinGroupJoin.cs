// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class SimpleQueryTestBase<TFixture>
    {
        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    select new { c.ContactName, o.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_entities(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    select new { c, o },
                e => (e.c.CustomerID, e.o.OrderID),
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_entities_same_entity_twice(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    select new { A = c, B = c },
                e => (e.A.CustomerID, e.B.CustomerID),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_select_many(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                      from e in ss.Set<Employee>()
                      select new
                      {
                          c,
                          o,
                          e
                      },
                e => (e.c.CustomerID, e.o.OrderID, e.e.EmployeeID),
                entryCount: 928);
        }

        [ConditionalTheory(Skip = "Issue #17328")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Client_Join_select_many(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from e1 in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(2)
                      join e2 in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Take(2) on e1.EmployeeID equals GetEmployeeID(e2)
                      from e3 in ss.Set<Employee>().OrderBy(e => e.EmployeeID).Skip(6).Take(2)
                      select new
                      {
                          e1,
                          e2,
                          e3
                      },
                e => (e.e1.EmployeeID, e.e2.EmployeeID, e.e3.EmployeeID),
                entryCount: 4);
        }

        private static uint GetEmployeeID(Employee employee) => employee.EmployeeID;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_select(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    select new { c.ContactName, o.OrderID }
                    into p
                    select p,
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_with_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o1 in
                        (from o2 in ss.Set<Order>() orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                    where o1.CustomerID == "ALFKI"
                    select new { c.ContactName, o1.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_with_subquery_with_take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o1 in
                        (from o2 in ss.Set<Order>() orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1.CustomerID
                    where o1.CustomerID == "ALFKI"
                    select new { c.ContactName, o1.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_with_subquery_anonymous_property_method(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o1 in
                        (from o2 in ss.Set<Order>()
                         orderby o2.OrderID
                         select new { o2 }) on c.CustomerID equals o1.o2.CustomerID
                    where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                    select new
                    {
                        o1,
                        o1.o2,
                        Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate")
                    },
                e => e.o1.o2.OrderID,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_with_subquery_anonymous_property_method_with_take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o1 in
                        (from o2 in ss.Set<Order>()
                         orderby o2.OrderID
                         select new { o2 }).Take(5) on c.CustomerID equals o1.o2.CustomerID
                    where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                    select new
                    {
                        o1,
                        o1.o2,
                        Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate")
                    },
                e => e.o1.o2.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_with_subquery_predicate(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o1 in
                        (from o2 in ss.Set<Order>() where o2.OrderID > 0 orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                    where o1.CustomerID == "ALFKI"
                    select new { c.ContactName, o1.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_with_subquery_predicate_with_take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o1 in
                        (from o2 in ss.Set<Order>() where o2.OrderID > 0 orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1
                            .CustomerID
                    where o1.CustomerID == "ALFKI"
                    select new { c.ContactName, o1.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_composite_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on new { a = c.CustomerID, b = c.CustomerID }
                        equals new { a = o.CustomerID, b = o.CustomerID }
                    select new { c, o },
                e => e.o.OrderID,
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_complex_condition(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                    join o in ss.Set<Order>().Where(o => o.OrderID < 10250) on true equals true
                    select c.CustomerID);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_client_new_expression(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                    select new { c, o },
                e => e.c.CustomerID);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_local_collection_int_closure_is_cached_correctly(bool isAsync)
        {
            var ids = new uint[] { 1, 2 };

            await AssertQueryScalar(
                isAsync,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);

            ids = new uint[] { 3 };

            await AssertQueryScalar(
                isAsync,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_local_string_closure_is_cached_correctly(bool isAsync)
        {
            var ids = "12";
            await AssertQueryScalar(
                isAsync,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);

            ids = "3";
            await AssertQueryScalar(
                isAsync,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_local_bytes_closure_is_cached_correctly(bool isAsync)
        {
            var ids = new byte[] { 1, 2 };
            await AssertQueryScalar(
                isAsync,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);

            ids = new byte[] { 3 };
            await AssertQueryScalar(
                isAsync,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_same_collection_multiple(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Join(
                    ss.Set<Customer>(), o => o.CustomerID, i => i.CustomerID, (c1, c2) => new { c1, c2 }).Join(
                    ss.Set<Customer>(), o => o.c1.CustomerID, i => i.CustomerID, (c12, c3) => c3),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_same_collection_force_alias_uniquefication(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<Order>().Join(
                        ss.Set<Order>(), o => o.CustomerID, i => i.CustomerID, (_, o) => new { _, o }),
                e => (e._.OrderID, e.o.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_customers_orders(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>().OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID into orders
                    select new { customer = c, orders = orders.ToList() },
                e => e.customer.CustomerID,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.customer, a.customer);
                    AssertCollection(e.orders, a.orders);
                },
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_customers_orders_count(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    select new { cust = c, ords = orders.Count() },
                e => e.cust.CustomerID,
                entryCount: 91);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_customers_orders_count_preserves_ordering(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>().Where(c => c.CustomerID != "VAFFE" && c.CustomerID != "DRACD").OrderBy(c => c.City).Take(5)
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    select new { cust = c, ords = orders.Count() },
                assertOrder: true,
                entryCount: 5);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_customers_employees_shadow(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    (from c in ss.Set<Customer>()
                     join e in ss.Set<Employee>() on c.City equals e.City into employees
                     select employees)
                    .SelectMany(emps => emps)
                    .Select(
                        e =>
                            new { Title = EF.Property<string>(e, "Title"), Id = e.EmployeeID }),
                e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_customers_employees_subquery_shadow(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    (from c in ss.Set<Customer>()
                     join e in ss.Set<Employee>().OrderBy(e => e.City) on c.City equals e.City into employees
                     select employees)
                    .SelectMany(emps => emps)
                    .Select(
                        e =>
                            new { Title = EF.Property<string>(e, "Title"), Id = e.EmployeeID }),
                e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_customers_employees_subquery_shadow_take(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    (from c in ss.Set<Customer>()
                     join e in ss.Set<Employee>().OrderBy(e => e.City).Take(5) on c.City equals e.City into employees
                     select employees)
                    .SelectMany(emps => emps)
                    .Select(
                        e =>
                            new { Title = EF.Property<string>(e, "Title"), Id = e.EmployeeID }),
                e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select o,
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select c,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select new { o.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_tracking_groups(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    select orders,
                elementSorter: e => e.Count(),
                elementAsserter: (e, a) => AssertCollection(e, a),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_tracking_groups2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    select new { c, orders },
                elementSorter: e => e.c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    AssertEqual(e.c, a.c);
                    AssertCollection(e.orders, a.orders);
                },
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple_ordering(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>().OrderBy(c => c.City)
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select o,
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple_subquery(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(4) on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select o,
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select new { c, o },
                e => (e.c.CustomerID, e.o.OrderID),
                entryCount: 919);
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_outer_projection(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().GroupJoin(
                    ss.Set<Order>(), c => c.CustomerID, o => o.CustomerID, (c, o) => new { c.City, o }),
                elementSorter: e => (e.City, e.o.Count()),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.City, a.City);
                    AssertCollection(e.o, a.o);
                },
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_outer_projection2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().GroupJoin(
                    ss.Set<Order>(), c => c.CustomerID, o => o.CustomerID, (c, g) => new { c.City, g = g.Select(o => o.CustomerID) }),
                elementSorter: e => (e.City, e.g.Count()),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.City, a.City);
                    AssertCollection(e.g, a.g);
                });
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_outer_projection3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().GroupJoin(
                    ss.Set<Order>(), c => c.CustomerID, o => o.CustomerID, (c, g) => new { g = g.Select(o => o.CustomerID) }),
                elementSorter: e => e.g.Count(),
                elementAsserter: (e, a) => AssertCollection(e.g, a.g));
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_outer_projection4(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().GroupJoin(
                    ss.Set<Order>(), c => c.CustomerID, o => o.CustomerID, (c, g) => g.Select(o => o.CustomerID)),
                elementSorter: e => e.Count(),
                elementAsserter: (e, a) => AssertCollection(e, a));
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_outer_projection_reverse(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().GroupJoin(
                    ss.Set<Customer>(), o => o.CustomerID, c => c.CustomerID, (o, c) => new { o.CustomerID, c }),
                e => e.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    AssertCollection(e.c, a.c);
                },
                entryCount: 89);
        }

        [ConditionalTheory(Skip = "Issue#17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_outer_projection_reverse2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => ss.Set<Order>().GroupJoin(
                    ss.Set<Customer>(), o => o.CustomerID, c => c.CustomerID, (o, g) => new { o.CustomerID, g = g.Select(c => c.City) }),
                elementSorter: e => e.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.CustomerID, a.CustomerID);
                    AssertCollection(e.g, a.g);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_subquery_projection_outer_mixed(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    from o0 in ss.Set<Order>().OrderBy(o => o.OrderID).Take(1)
                    join o1 in ss.Set<Order>() on c.CustomerID equals o1.CustomerID into orders
                    from o2 in orders
                    select new
                    {
                        A = c.CustomerID,
                        B = o0.CustomerID,
                        C = o2.CustomerID
                    },
                e => (e.A, e.B, e.C));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
                    select new { c, o },
                e => (e.c.CustomerID, e.o?.OrderID),
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_DefaultIfEmpty_multiple(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o1 in ss.Set<Order>() on c.CustomerID equals o1.CustomerID into orders1
                    from o1 in orders1.DefaultIfEmpty()
                    join o2 in ss.Set<Order>() on c.CustomerID equals o2.CustomerID into orders2
                    from o2 in orders2.DefaultIfEmpty()
                    select new
                    {
                        c,
                        o1,
                        o2
                    },
                e => (e.c.CustomerID, e.o1?.OrderID, e.o2?.OrderID),
                entryCount: 921);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_DefaultIfEmpty2(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from e in ss.Set<Employee>()
                    join o in ss.Set<Order>() on e.EmployeeID equals o.EmployeeID into orders
                    from o in orders.DefaultIfEmpty()
                    select new { e, o },
                e => (e.e.EmployeeID, e.o?.OrderID),
                entryCount: 839);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_DefaultIfEmpty3(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(1)
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
                    select o,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_Where(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    where o.CustomerID == "ALFKI"
                    select o,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_Where_OrderBy(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    where o.CustomerID == "ALFKI" || c.CustomerID == "ANATR"
                    orderby c.City
                    select o,
                entryCount: 10);
        }

        [ConditionalTheory(Skip = "Issue#15638")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_DefaultIfEmpty_Where(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
#pragma warning disable RCS1146 // Use conditional access.
                    where o != null && o.CustomerID == "ALFKI"
#pragma warning restore RCS1146 // Use conditional access.
                    select o,
                entryCount: 6);
        }

        [ConditionalTheory(Skip = "Issue#15638")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_GroupJoin_DefaultIfEmpty_Where(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    join o2 in ss.Set<Order>() on c.CustomerID equals o2.CustomerID into orders
                    from o3 in orders.DefaultIfEmpty()
#pragma warning disable RCS1146 // Use conditional access.
                    where o3 != null && o3.CustomerID == "ALFKI"
#pragma warning restore RCS1146 // Use conditional access.
                    select o3,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_DefaultIfEmpty_Project(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
                    select o != null ? (object)o.OrderID : null);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_different_outer_elements_with_same_key(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<Order>().GroupJoin(
                        ss.Set<Customer>(),
                        o => o.CustomerID,
                        c => c.CustomerID,
                        (o, cg) => new { o.OrderID, Name = cg.Select(c => c.ContactName).FirstOrDefault() }),
                e => (e.OrderID, e.Name));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_different_outer_elements_with_same_key_with_predicate(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<Order>().Where(o => o.OrderID > 11500).GroupJoin(
                        ss.Set<Customer>(),
                        o => o.CustomerID,
                        c => c.CustomerID,
                        (o, cg) => new { o.OrderID, Name = cg.Select(c => c.ContactName).FirstOrDefault() }),
                e => (e.OrderID, e.Name));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    ss.Set<OrderDetail>().Select(od => od.Order).GroupJoin(
                        ss.Set<Customer>(),
                        o => o.CustomerID,
                        c => c.CustomerID,
                        (o, cg) => new { o.OrderID, Name = cg.Select(c => c.ContactName).FirstOrDefault() }),
                e => (e.OrderID, e.Name));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5)
                    select new { c.ContactName, o.OrderID },
                e => (e.ContactName, e.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate)
                    select new { c.ContactName, o.OrderID },
                e => (e.ContactName, e.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5).DefaultIfEmpty()
                    select new { c.ContactName, o },
                e => (e.ContactName, e.o?.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate).DefaultIfEmpty()
                    select new { c.ContactName, o },
                e => (e.ContactName, e.o?.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_order_by_key_descending1(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                      where c.CustomerID.StartsWith("A")
                      orderby c.CustomerID descending
                      select grouping.Count(),
                assertOrder: true);
        }

        [ConditionalTheory(Skip = "Issue #17068")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_with_order_by_key_descending2(bool isAsync)
        {
            return AssertQueryScalar(
                isAsync,
                ss => from c in ss.Set<Customer>()
                      orderby c.CustomerID descending
                      join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into grouping
                      where c.CustomerID.StartsWith("A")
                      select grouping.Count(),
                assertOrder: true);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_Subquery_with_Take_Then_SelectMany_Where(bool isAsync)
        {
            return AssertQuery(
                isAsync,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(100) on c.CustomerID equals o.CustomerID into lo
                      from o in lo.Where(x => x.CustomerID.StartsWith("A"))
                      select new { c.CustomerID, o.OrderID });
        }
    }
}
