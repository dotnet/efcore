// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindJoinQueryTestBase<TFixture> : QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindJoinQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_projection(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    select new { c.ContactName, o.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_entities(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    select new { c, o },
                e => (e.c.CustomerID, e.o.OrderID),
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_customers_orders_entities_same_entity_twice(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                    select new { A = c, B = c },
                e => (e.A.CustomerID, e.B.CustomerID),
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_select_many(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Client_Join_select_many(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_customers_orders_select(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_customers_orders_with_subquery(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_customers_orders_with_subquery_with_take(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_customers_orders_with_subquery_anonymous_property_method(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_customers_orders_with_subquery_anonymous_property_method_with_take(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_customers_orders_with_subquery_predicate(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_customers_orders_with_subquery_predicate_with_take(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_composite_key(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_complex_condition(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                    join o in ss.Set<Order>().Where(o => o.OrderID < 10250) on true equals true
                    select c.CustomerID);
        }

        [ConditionalTheory(Skip = "Issue #19016")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_local_collection_int_closure_is_cached_correctly(bool async)
        {
            var ids = new uint[] { 1, 2 };

            await AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);

            ids = new uint[] { 3 };

            await AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);
        }

        [ConditionalTheory(Skip = "Issue #19016")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_local_string_closure_is_cached_correctly(bool async)
        {
            var ids = "12";
            await AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);

            ids = "3";
            await AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);
        }

        [ConditionalTheory(Skip = "Issue #19016")]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Join_local_bytes_closure_is_cached_correctly(bool async)
        {
            var ids = new byte[] { 1, 2 };
            await AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);

            ids = new byte[] { 3 };
            await AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_same_collection_multiple(bool async)
        {
            return AssertQuery(
                async,
                ss => ss.Set<Customer>().Join(
                    ss.Set<Customer>(), o => o.CustomerID, i => i.CustomerID, (c1, c2) => new { c1, c2 }).Join(
                    ss.Set<Customer>(), o => o.c1.CustomerID, i => i.CustomerID, (c12, c3) => c3),
                entryCount: 91);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Join_same_collection_force_alias_uniquefication(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    ss.Set<Order>().Join(
                        ss.Set<Order>(), o => o.CustomerID, i => i.CustomerID, (_, o) => new { _, o }),
                e => (e._.OrderID, e.o.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_customers_employees_shadow(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_customers_employees_subquery_shadow(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_customers_employees_subquery_shadow_take(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_simple(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select o,
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple2(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select c,
                entryCount: 89);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple3(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select new { o.OrderID },
                e => e.OrderID);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple_ordering(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().OrderBy(c => c.City)
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select o,
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_simple_subquery(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(4) on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select o,
                entryCount: 4);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_projection(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders
                    select new { c, o },
                e => (e.c.CustomerID, e.o.OrderID),
                entryCount: 919);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_subquery_projection_outer_mixed(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_DefaultIfEmpty_multiple(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_DefaultIfEmpty2(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_DefaultIfEmpty3(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(1)
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
                    select o,
                entryCount: 6);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_Where(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_Where_OrderBy(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_DefaultIfEmpty_Where(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task Join_GroupJoin_DefaultIfEmpty_Where(bool async)
        {
            return AssertQuery(
                async,
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
        public virtual Task GroupJoin_DefaultIfEmpty_Project(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                    from o in orders.DefaultIfEmpty()
                    select o != null ? (object)o.OrderID : null);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5)
                    select new { c.ContactName, o.OrderID },
                e => (e.ContactName, e.OrderID));
        }

        [ConditionalTheory(Skip = "Issue #19015")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate)
                    select new { c.ContactName, o.OrderID },
                e => (e.ContactName, e.OrderID));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5).DefaultIfEmpty()
                    select new { c.ContactName, o },
                e => (e.ContactName, e.o?.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory(Skip = "Issue #19015")]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool async)
        {
            return AssertQuery(
                async,
                ss =>
                    from c in ss.Set<Customer>()
                    join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                    from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate).DefaultIfEmpty()
                    select new { c.ContactName, o },
                e => (e.ContactName, e.o?.OrderID),
                entryCount: 830);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GroupJoin_Subquery_with_Take_Then_SelectMany_Where(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>()
                      join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(100) on c.CustomerID equals o.CustomerID into lo
                      from o in lo.Where(x => x.CustomerID.StartsWith("A"))
                      select new { c.CustomerID, o.OrderID });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Inner_join_with_tautology_predicate_converts_to_cross_join(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(10)
                      join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(10) on 1 equals 1 
                      select new { c.CustomerID, o.OrderID });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Left_join_with_tautology_predicate_doesnt_convert_to_cross_join(bool async)
        {
            return AssertQuery(
                async,
                ss => from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(10)
                      join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(10) on c.CustomerID != null equals true into grouping
                      from o in grouping.DefaultIfEmpty()
                      select new { c.CustomerID, o.OrderID });
        }
    }
}
