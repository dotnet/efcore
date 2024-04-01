// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class NorthwindJoinQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindJoinQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();

    protected virtual void ClearLog()
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_projection(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID },
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_entities(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                select new { c, o },
            e => (e.c.CustomerID, e.o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_entities_same_entity_twice(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                select new { A = c, B = c },
            e => (e.A.CustomerID, e.B.CustomerID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_select_many(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                  from e in ss.Set<Employee>()
                  select new
                  {
                      c,
                      o,
                      e
                  },
            e => (e.c.CustomerID, e.o.OrderID, e.e.EmployeeID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Client_Join_select_many(bool async)
        // Translation failed message. Issue #17328.
        => AssertTranslationFailed(
            () => AssertQuery(
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
                e => (e.e1.EmployeeID, e.e2.EmployeeID, e.e3.EmployeeID)));

    private static uint GetEmployeeID(Employee employee)
        => employee.EmployeeID;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_select(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                select new { c.ContactName, o.OrderID }
                into p
                select p,
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_with_subquery(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o1 in
                    (from o2 in ss.Set<Order>() orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID },
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_with_subquery_with_take(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o1 in
                    (from o2 in ss.Set<Order>() orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "HANAR"
                select new { c.ContactName, o1.OrderID },
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_with_subquery_anonymous_property_method(bool async)
        => AssertQuery(
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
            e => e.o1.o2.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_with_subquery_anonymous_property_method_with_take(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o1 in
                    (from o2 in ss.Set<Order>()
                     orderby o2.OrderID
                     select new { o2 }).Take(5) on c.CustomerID equals o1.o2.CustomerID
                where EF.Property<string>(o1.o2, "CustomerID") == "HANAR"
                select new
                {
                    o1,
                    o1.o2,
                    Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate")
                },
            e => e.o1.o2.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_with_subquery_predicate(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o1 in
                    (from o2 in ss.Set<Order>() where o2.OrderID > 0 orderby o2.OrderID select o2) on c.CustomerID equals o1.CustomerID
                where o1.CustomerID == "ALFKI"
                select new { c.ContactName, o1.OrderID },
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_customers_orders_with_subquery_predicate_with_take(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o1 in
                    (from o2 in ss.Set<Order>() where o2.OrderID > 0 orderby o2.OrderID select o2).Take(5) on c.CustomerID equals o1
                        .CustomerID
                where o1.CustomerID == "HANAR"
                select new { c.ContactName, o1.OrderID },
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_composite_key(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on new { a = c.CustomerID, b = c.CustomerID }
                    equals new { a = o.CustomerID, b = o.CustomerID }
                select new { c, o },
            e => e.o.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_complex_condition(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID == "ALFKI")
                join o in ss.Set<Order>().Where(o => o.OrderID < 10250) on true equals true
                select c.CustomerID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_local_collection_int_closure_is_cached_correctly(bool async)
    {
        var ids = new uint[] { 1, 2 };
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID);

        ids = [3];
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID);
    }

    [ConditionalTheory(Skip = "#30677")]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_local_string_closure_is_cached_correctly(bool async)
    {
        var ids = "12";
        await AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID));

        ids = "3";
        await AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID));
    }

    [ConditionalTheory(Skip = "#30677")]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Join_local_bytes_closure_is_cached_correctly(bool async)
    {
        var ids = new byte[] { 1, 2 };

        await AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID));

        ids = [3];
        await AssertTranslationFailed(
            () => AssertQueryScalar(
                async,
                ss => from e in ss.Set<Employee>()
                      join id in ids on e.EmployeeID equals id
                      select e.EmployeeID));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_same_collection_multiple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Join(
                ss.Set<Customer>(), o => o.CustomerID, i => i.CustomerID, (c1, c2) => new { c1, c2 }).Join(
                ss.Set<Customer>(), o => o.c1.CustomerID, i => i.CustomerID, (c12, c3) => c3));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_same_collection_force_alias_uniquefication(bool async)
        => AssertQuery(
            async,
            ss =>
                ss.Set<Order>().Where(o => o.CustomerID.StartsWith("F")).Join(
                    ss.Set<Order>(), o => o.CustomerID, i => i.CustomerID, (_, o) => new { _, o }),
            e => (e._.OrderID, e.o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_customers_employees_shadow(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 join e in ss.Set<Employee>() on c.City equals e.City into employees
                 select employees)
                .SelectMany(emps => emps)
                .Select(e => new { Title = EF.Property<string>(e, "Title"), Id = e.EmployeeID }),
            e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_customers_employees_subquery_shadow(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 join e in ss.Set<Employee>().OrderBy(e => e.City) on c.City equals e.City into employees
                 select employees)
                .SelectMany(emps => emps)
                .Select(e => new { Title = EF.Property<string>(e, "Title"), Id = e.EmployeeID }),
            e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_customers_employees_subquery_shadow_take(bool async)
        => AssertQuery(
            async,
            ss =>
                (from c in ss.Set<Customer>()
                 join e in ss.Set<Employee>().OrderBy(e => e.City).Take(5) on c.City equals e.City into employees
                 select employees)
                .SelectMany(emps => emps)
                .Select(e => new { Title = EF.Property<string>(e, "Title"), Id = e.EmployeeID }),
            e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_simple(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_simple2(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select c);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_simple3(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select new { o.OrderID },
            e => e.OrderID);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_simple_ordering(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")).OrderBy(c => c.City)
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_simple_subquery(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(4) on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_projection(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders
                select new { c, o },
            e => (e.c.CustomerID, e.o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_subquery_projection_outer_mixed(bool async)
        => AssertQuery(
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_as_final_operator(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                select new { c, orders },
            e => e.c.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c, a.c);
                AssertCollection(e.orders, a.orders);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Unflattened_GroupJoin_composed(bool async)
        => AssertQuery(
            async,
            ss =>
                from i in (from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                           join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                           select new { c, orders })
                where i.c.City == "Lisboa"
                select i,
            e => e.c.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c, a.c);
                AssertCollection(e.orders, a.orders);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Unflattened_GroupJoin_composed_2(bool async)
        => AssertQuery(
            async,
            ss =>
                from i in (from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                           join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                           select new { c, orders })
                join c2 in ss.Set<Customer>().Where(n => n.City == "Lisboa") on i.c.CustomerID equals c2.CustomerID
                select new { i, c2 },
            e => e.i.c.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.c2, a.c2);
                AssertEqual(e.i.c, a.i.c);
                AssertCollection(e.i.orders, a.i.orders);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select new { c, o },
            e => (e.c.CustomerID, e.o?.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_DefaultIfEmpty_multiple(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
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
            e => (e.c.CustomerID, e.o1?.OrderID, e.o2?.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_DefaultIfEmpty2(bool async)
        => AssertQuery(
            async,
            ss =>
                from e in ss.Set<Employee>()
                join o in ss.Set<Order>().Where(o => o.CustomerID.StartsWith("F")) on e.EmployeeID equals o.EmployeeID into orders
                from o in orders.DefaultIfEmpty()
                select new { e, o },
            e => (e.e.EmployeeID, e.o?.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_DefaultIfEmpty3(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(1)
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_Where(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders
                where o.CustomerID == "ALFKI"
                select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_Where_OrderBy(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders
                where o.CustomerID == "ALFKI" || c.CustomerID == "ANATR"
                orderby c.City
                select o);

    [ConditionalTheory]
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
                select o);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Join_GroupJoin_DefaultIfEmpty_Where(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID
                join o2 in ss.Set<Order>() on c.CustomerID equals o2.CustomerID into orders
                from o3 in orders.DefaultIfEmpty()
#pragma warning disable RCS1146 // Use conditional access.
                where o3 != null && o3.CustomerID == "ALFKI"
#pragma warning restore RCS1146 // Use conditional access.
                select o3);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_DefaultIfEmpty_Project(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into orders
                from o in orders.DefaultIfEmpty()
                select o != null ? (object)o.OrderID : null);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_subquery_with_filter(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                from o in lo.Where(x => x.OrderID > 5)
                select new { c.ContactName, o.OrderID },
            e => (e.ContactName, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>()
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate)
                select new { c.ContactName, o.OrderID },
            e => (e.ContactName, e.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                from o in lo.Where(x => x.OrderID > 5).DefaultIfEmpty()
                select new { c.ContactName, o },
            e => (e.ContactName, e.o?.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool async)
        => AssertQuery(
            async,
            ss =>
                from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                join o in ss.Set<Order>() on c.CustomerID equals o.CustomerID into lo
                from o in lo.Where(x => x.OrderID > 5).OrderBy(x => x.OrderDate).DefaultIfEmpty()
                select new { c.ContactName, o },
            e => (e.ContactName, e.o?.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GroupJoin_Subquery_with_Take_Then_SelectMany_Where(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(100) on c.CustomerID equals o.CustomerID into lo
                  from o in lo.Where(x => x.CustomerID.StartsWith("A"))
                  select new { c.CustomerID, o.OrderID });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inner_join_with_tautology_predicate_converts_to_cross_join(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(10)
                  join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(10) on 1 equals 1
                  select new { c.CustomerID, o.OrderID });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Left_join_with_tautology_predicate_doesnt_convert_to_cross_join(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().OrderBy(c => c.CustomerID).Take(10)
                  join o in ss.Set<Order>().OrderBy(o => o.OrderID).Take(10) on c.CustomerID != null equals true into grouping
                  from o in grouping.DefaultIfEmpty()
                  select new { c.CustomerID, o.OrderID });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_client_eval(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .SelectMany(c => c.Orders.Select(o => new { OrderProperty = ClientMethod(o), CustomerProperty = c.ContactName })),
            elementSorter: e => e.OrderProperty);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_client_eval_with_collection_shaper(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .SelectMany(
                    c => c.Orders.Select(
                        o => new
                        {
                            OrderProperty = ClientMethod(o),
                            o.OrderDetails,
                            CustomerProperty = c.ContactName
                        })),
            elementSorter: e => e.OrderProperty,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.OrderProperty, a.OrderProperty);
                AssertEqual(e.CustomerProperty, a.CustomerProperty);
                AssertCollection(e.OrderDetails, a.OrderDetails);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                .SelectMany(
                    c => c.Orders.Select(
                        o => new
                        {
                            OrderProperty = ClientMethod(o),
                            o.OrderDetails,
                            CustomerProperty = c.ContactName
                        }))
                .Select(e => new { e.OrderProperty, e.CustomerProperty }),
            elementSorter: e => e.OrderProperty);

    private static int ClientMethod(Order order)
        => order.OrderID;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_client_eval_with_constructor(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.CustomerID.StartsWith("A"))
                .OrderBy(c => c.CustomerID)
                .Select(
                    c => new CustomerViewModel(
                        c.CustomerID,
                        c.City,
                        c.Orders.SelectMany(
                                o => o.OrderDetails
                                    .Where(od => od.OrderID < 11000)
                                    .Select(od => new OrderDetailViewModel(od.OrderID, od.ProductID)))
                            .ToArray())),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.CustomerID, a.CustomerID);
                Assert.Equal(e.City, a.City);
                Assert.Equal(
                    e.Views.OrderBy(od => od.OrderID).ThenBy(od => od.ProductID),
                    a.Views.OrderBy(od => od.OrderID).ThenBy(od => od.ProductID));
            });

    private class CustomerViewModel(string customerID, string city, OrderDetailViewModel[] views)
    {
        public string CustomerID { get; } = customerID;
        public string City { get; } = city;
        public OrderDetailViewModel[] Views { get; } = views;

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                || obj.GetType() == GetType()
                && Equals((CustomerViewModel)obj);
        }

        private bool Equals(CustomerViewModel customerViewModel)
            => CustomerID == customerViewModel.CustomerID
                && City == customerViewModel.City
                && Views.SequenceEqual(customerViewModel.Views);

        public override int GetHashCode()
            => HashCode.Combine(CustomerID, City);
    }

    private class OrderDetailViewModel(int orderID, int productID)
    {
        public int OrderID { get; } = orderID;
        public int ProductID { get; } = productID;

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return ReferenceEquals(this, obj)
                || obj.GetType() == GetType()
                && Equals((OrderDetailViewModel)obj);
        }

        private bool Equals(OrderDetailViewModel orderDetailViewModel)
            => OrderID == orderDetailViewModel.OrderID
                && ProductID == orderDetailViewModel.ProductID;

        public override int GetHashCode()
            => HashCode.Combine(OrderID, ProductID);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_selecting_outer_entity(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .SelectMany(c => c.Orders.Select(o => c)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_selecting_outer_element(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(e => new { e, Complex = e.CustomerID + e.City })
                .SelectMany(c => c.e.Orders.Select(o => c.Complex)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_with_selecting_outer_entity_column_and_inner_column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.CustomerID)
                .SelectMany(c => c.Orders.OrderBy(o => o.OrderID).Skip(0).Select(o => new { c.City, o.OrderDate })),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_correlated_subquery_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => new { c.CustomerID })
                .SelectMany(
                    c => ss.Set<Customer>()
                        .Where(i => i.CustomerID == c.CustomerID)
                        .OrderBy(i => i.CustomerID + i.City)
                        .Take(2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_SelectMany_correlated_subquery_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => new { c.CustomerID })
                .Distinct()
                .SelectMany(
                    c => ss.Set<Customer>()
                        .Where(i => i.CustomerID == c.CustomerID)
                        .OrderBy(i => i.CustomerID + i.City)
                        .Take(2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distinct_SelectMany_correlated_subquery_take_2(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Distinct()
                .SelectMany(
                    c => ss.Set<Customer>()
                        .Where(i => i.CustomerID == c.CustomerID)
                        .OrderBy(i => i.CustomerID + i.City)
                        .Take(2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_SelectMany_correlated_subquery_take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Select(c => new { c.CustomerID })
                .OrderBy(c => c.CustomerID)
                .Take(2)
                .SelectMany(
                    c => ss.Set<Customer>()
                        .Where(i => i.CustomerID == c.CustomerID)
                        .OrderBy(i => i.CustomerID + i.City)
                        .Take(2)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Take_in_collection_projection_with_FirstOrDefault_on_top_level(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<Customer>()
                .Select(
                    c => new
                    {
                        Orders = c.Orders.OrderBy(e => e.OrderDate).Take(1)
                            .Select(o => new { Title = o.CustomerID == o.Customer.City ? "A" : "B" }).ToList()
                    }),
            asserter: (e, a) => AssertCollection(e.Orders, a.Orders, ordered: true));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Condition_on_entity_with_include(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F"))
                  join o in ss.Set<Order>().Include(o => o.OrderDetails)
                      on c.CustomerID equals o.CustomerID into g
                  from o in g.DefaultIfEmpty()
                  select new { a = o != null ? o.OrderID : -1 });
}
