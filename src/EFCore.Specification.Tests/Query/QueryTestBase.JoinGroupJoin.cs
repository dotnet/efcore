// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract partial class QueryTestBase<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
        public virtual void Join_customers_orders_projection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID });
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_entities()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c, o });
        }

        [ConditionalFact]
        public virtual void Join_select_many()
        {
            AssertQuery<Customer, Order, Employee>((cs, os, es) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                from e in es
                select new { c, o, e });
        }

        [ConditionalFact]
        public virtual void Client_Join_select_many()
        {
            AssertQuery<Employee>(es =>
                    from e1 in es.Take(2)
                    join e2 in es.Take(2) on e1.EmployeeID equals GetEmployeeID(e2)
                    from e3 in es.Skip(6).Take(2)
                    select new { e1, e2, e3 },
                entryCount: 4);
        }

        private int GetEmployeeID(Employee employee)
        {
            return employee.EmployeeID;
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_select()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID }
                into p
                select p);
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_with_subquery()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                (from o2 in os orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_with_subquery_with_take()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                (from o2 in os orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                (from o2 in os orderby o2.OrderID select new { o2 }) on c.CustomerID equals o1.o2.CustomerID
                where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                select new { o1, o1.o2, Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate") });
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_with_subquery_anonymous_property_method_with_take()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                (from o2 in os orderby o2.OrderID select new { o2 }).Take(5) on c.CustomerID equals o1.o2.CustomerID
                where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                select new { o1, o1.o2, Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate") });
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_with_subquery_predicate()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                (from o2 in os where o2.OrderID > 0 orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [ConditionalFact]
        public virtual void Join_customers_orders_with_subquery_predicate_with_take()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in
                (from o2 in os where o2.OrderID > 0 orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID });
        }

        [ConditionalFact]
        public virtual void Join_composite_key()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on new { a = c.CustomerID, b = c.CustomerID }
                equals new { a = o.CustomerID, b = o.CustomerID }
                select new { c, o });
        }

        [ConditionalFact]
        public virtual void Join_complex_condition()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs.Where(c => c.CustomerID == "ALFKI")
                join o in os.Where(o => o.OrderID < 10250) on true equals true
                select c.CustomerID);
        }

        [ConditionalFact]
        public virtual void Join_client_new_expression()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on new Foo { Bar = c.CustomerID } equals new Foo { Bar = o.CustomerID }
                select new { c, o });
        }

        [ConditionalFact]
        public virtual void Join_local_collection_int_closure_is_cached_correctly()
        {
            var ids = new[] { 1, 2 };

            AssertQuery<Employee>(es =>
                from e in es
                join id in ids on e.EmployeeID equals id
                select e.EmployeeID);

            ids = new[] { 3 };

            AssertQuery<Employee>(es =>
                from e in es
                join id in ids on e.EmployeeID equals id
                select e.EmployeeID);
        }

        [ConditionalFact]
        public virtual void Join_local_string_closure_is_cached_correctly()
        {
            var ids = "12";

            AssertQuery<Employee>(es =>
                from e in es
                join id in ids on e.EmployeeID equals id
                select e.EmployeeID);

            ids = "3";

            AssertQuery<Employee>(es =>
                from e in es
                join id in ids on e.EmployeeID equals id
                select e.EmployeeID);
        }

        [ConditionalFact]
        public virtual void Join_local_bytes_closure_is_cached_correctly()
        {
            var ids = new byte[] { 1, 2 };

            AssertQuery<Employee>(es =>
                from e in es
                join id in ids on e.EmployeeID equals id
                select e.EmployeeID);

            ids = new byte[] { 3 };

            AssertQuery<Employee>(es =>
                from e in es
                join id in ids on e.EmployeeID equals id
                select e.EmployeeID);
        }

        [ConditionalFact]
        public virtual void Join_same_collection_multiple()
        {
            AssertQuery<Customer, Customer, Customer>((cs1, cs2, cs3) =>
                cs1.Join(cs2, o => o.CustomerID, i => i.CustomerID, (c1, c2) => new { c1, c2 }).Join(cs3, o => o.c1.CustomerID, i => i.CustomerID, (c12, c3) => c3));
        }

        [ConditionalFact]
        public virtual void Join_same_collection_force_alias_uniquefication()
        {
            AssertQuery<Order, Order>((os1, os2) =>
                os1.Join(os2, o => o.CustomerID, i => i.CustomerID, (_, o) => new { _, o }));
        }

        [ConditionalFact]
        public virtual void GroupJoin_customers_orders()
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

        [ConditionalFact]
        public virtual void GroupJoin_customers_orders_count()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                select new { cust = c, ords = orders.Count() });
        }

        [ConditionalFact]
        public virtual void GroupJoin_customers_orders_count_preserves_ordering()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    from c in cs.Where(c => c.CustomerID != "VAFFE").OrderBy(c => c.City).Take(5)
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select new { cust = c, ords = orders.Count() },
                assertOrder: true);
        }

        [ConditionalFact]
        public virtual void GroupJoin_customers_employees_shadow()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                (from c in cs
                 join e in es on c.City equals e.City into employees
                 select employees)
                .SelectMany(emps => emps)
                .Select(e =>
                    new
                    {
                        Title = EF.Property<string>(e, "Title"),
                        Id = e.EmployeeID
                    }));
        }

        [ConditionalFact]
        public virtual void GroupJoin_customers_employees_subquery_shadow()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                (from c in cs
                 join e in es.OrderBy(e => e.City) on c.City equals e.City into employees
                 select employees)
                .SelectMany(emps => emps)
                .Select(e =>
                    new
                    {
                        Title = EF.Property<string>(e, "Title"),
                        Id = e.EmployeeID
                    }));
        }

        [ConditionalFact]
        public virtual void GroupJoin_customers_employees_subquery_shadow_take()
        {
            AssertQuery<Customer, Employee>((cs, es) =>
                (from c in cs
                 join e in es.OrderBy(e => e.City).Take(5) on c.City equals e.City into employees
                 select employees)
                .SelectMany(emps => emps)
                .Select(e =>
                    new
                    {
                        Title = EF.Property<string>(e, "Title"),
                        Id = e.EmployeeID
                    }));
        }

        [ConditionalFact]
        public virtual void GroupJoin_simple()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);
        }

        [ConditionalFact]
        public virtual void GroupJoin_simple2()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select c);
        }

        [ConditionalFact]
        public virtual void GroupJoin_simple3()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select new { o.OrderID });
        }

        [ConditionalFact]
        public virtual void GroupJoin_tracking_groups()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select orders,
                entryCount: 830,
                asserter:
                (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_tracking_groups2()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select new { c, orders },
                entryCount: 921,
                asserter:
                (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_simple_ordering()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs.OrderBy(c => c.City)
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);
        }

        [ConditionalFact]
        public virtual void GroupJoin_simple_subquery()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os.OrderBy(o => o.OrderID).Take(4) on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);
        }

        [ConditionalFact]
        public virtual void GroupJoin_projection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select new { c, o });
        }

        [ConditionalFact]
        public virtual void GroupJoin_outer_projection()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, o) => new { c.City, o }),
                asserter: (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_outer_projection2()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, g) => new { c.City, g = g.Select(o => o.CustomerID) }),
                asserter: (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_outer_projection3()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, g) => new { g = g.Select(o => o.CustomerID) }),
                asserter: (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_outer_projection4()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, g) => g.Select(o => o.CustomerID)),
                asserter: (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_outer_projection_reverse()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    os.GroupJoin(cs, o => o.CustomerID, c => c.CustomerID, (o, c) => new { o.CustomerID, c }),
                asserter: (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_outer_projection_reverse2()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    os.GroupJoin(cs, o => o.CustomerID, c => c.CustomerID, (o, g) => new { o.CustomerID, g = g.Select(c => c.City) }),
                asserter: (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_subquery_projection_outer_mixed()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                    from c in cs
                    from o0 in os.Take(1)
                    join o1 in os on c.CustomerID equals o1.CustomerID into orders
                    from o2 in orders
                    select new { A = c.CustomerID, B = o0.CustomerID, C = o2.CustomerID },
                asserter:
                (l2oResults, efResults) => { Assert.Equal(l2oResults.Count, efResults.Count); });
        }

        [ConditionalFact]
        public virtual void GroupJoin_DefaultIfEmpty()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select new { c, o });
        }

        [ConditionalFact]
        public virtual void GroupJoin_DefaultIfEmpty_multiple()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o1 in os on c.CustomerID equals o1.CustomerID into orders1
                from o1 in orders1.DefaultIfEmpty()
                join o2 in os on c.CustomerID equals o2.CustomerID into orders2
                from o2 in orders2.DefaultIfEmpty()
                select new { c, o1, o2 });
        }

        [ConditionalFact]
        public virtual void GroupJoin_DefaultIfEmpty2()
        {
            AssertQuery<Employee, Order>((es, os) =>
                from e in es
                join o in os on e.EmployeeID equals o.EmployeeID into orders
                from o in orders.DefaultIfEmpty()
                select new { e, o });
        }

        [ConditionalFact]
        public virtual void GroupJoin_DefaultIfEmpty3()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs.OrderBy(c => c.CustomerID).Take(1)
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select o);
        }

        [ConditionalFact]
        public virtual void GroupJoin_Where()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                where o.CustomerID == "ALFKI"
                select o);
        }

        [ConditionalFact]
        public virtual void GroupJoin_Where_OrderBy()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders
                where o.CustomerID == "ALFKI" || c.CustomerID == "ANATR"
                orderby c.City
                select o);
        }

        [ConditionalFact]
        public virtual void GroupJoin_DefaultIfEmpty_Where()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                where o != null && o.CustomerID == "ALFKI"
                select o);
        }

        [ConditionalFact]
        public virtual void Join_GroupJoin_DefaultIfEmpty_Where()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID
                join o2 in os on c.CustomerID equals o2.CustomerID into orders
                from o3 in orders.DefaultIfEmpty()
                where o3 != null && o3.CustomerID == "ALFKI"
                select o3);
        }

        [ConditionalFact]
        public virtual void GroupJoin_DefaultIfEmpty_Project()
        {
            AssertQuery<Customer, Order>((cs, os) =>
                from c in cs
                join o in os on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select o != null ? (object)o.OrderID : null);
        }

        [ConditionalFact]
        public virtual void GroupJoin_with_different_outer_elements_with_same_key()
        {
            AssertQuery<Order, Customer>((os, cs) =>
                os.GroupJoin(cs,
                    o => o.CustomerID,
                    c => c.CustomerID,
                    (o, cg) => new
                    {
                        o.OrderID,
                        Name = cg.Select(c => c.ContactName).FirstOrDefault()
                    }));
        }

        [ConditionalFact]
        public virtual void GroupJoin_with_different_outer_elements_with_same_key_with_predicate()
        {
            AssertQuery<Order, Customer>((os, cs) =>
                os.Where(o => o.OrderID > 11500).GroupJoin(cs,
                    o => o.CustomerID,
                    c => c.CustomerID,
                    (o, cg) => new
                    {
                        o.OrderID,
                        Name = cg.Select(c => c.ContactName).FirstOrDefault()
                    }));
        }

        [ConditionalFact]
        public virtual void GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity()
        {
            AssertQuery<OrderDetail, Customer>((ods, cs) =>
                ods.Select(od => od.Order).GroupJoin(cs,
                    o => o.CustomerID,
                    c => c.CustomerID,
                    (o, cg) => new
                    {
                        o.OrderID,
                        Name = cg.Select(c => c.ContactName).FirstOrDefault()
                    }));
        }

        [ConditionalFact]
        public virtual void GroupJoin_SelectMany_subquery_with_filter()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into lo
                            from o in lo.Where(x => x.OrderID > 5)
                            select new { c.ContactName, o.OrderID });
        }

        [ConditionalFact]
        public virtual void GroupJoin_SelectMany_subquery_with_filter_orderby()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into lo
                            from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate)
                            select new { c.ContactName, o.OrderID });
        }

        [ConditionalFact]
        public virtual void GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into lo
                            from o in lo.Where(x => x.OrderID > 5).DefaultIfEmpty()
                            select new { c.ContactName, o });
        }

        [ConditionalFact]
        public virtual void GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into lo
                            from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate).DefaultIfEmpty()
                            select new { c.ContactName, o });
        }
    }
}
