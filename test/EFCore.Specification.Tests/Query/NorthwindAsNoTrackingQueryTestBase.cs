// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.Northwind;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query;

public abstract class NorthwindAsNoTrackingQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
{
    protected NorthwindAsNoTrackingQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual Task Entity_not_added_to_state_manager(bool useParam, bool async)
        => useParam
            ? AssertQuery(
                async,
                ss => ss.Set<Customer>().AsTracking(QueryTrackingBehavior.NoTracking))
            : AssertQuery(
                async,
                ss => ss.Set<Customer>().AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Applied_to_body_clause(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  join o in ss.Set<Order>().AsNoTracking()
                      on c.CustomerID equals o.CustomerID
                  where c.CustomerID == "ALFKI"
                  select o);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Applied_to_multiple_body_clauses(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>().AsNoTracking()
                  from o in ss.Set<Order>().AsNoTracking()
                  where c.CustomerID == o.CustomerID
                  select new { c, o },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Applied_to_body_clause_with_projection(bool async)
        => AssertQuery(
            async,
            ss => from c in ss.Set<Customer>()
                  join o in ss.Set<Order>().AsNoTracking()
                      on c.CustomerID equals o.CustomerID
                  where c.CustomerID == "ALFKI"
                  select new
                  {
                      c.CustomerID,
                      c,
                      ocid = o.CustomerID,
                      o
                  },
            elementSorter: e => (e.CustomerID, e.o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Applied_to_projection(bool async)
        => AssertQuery(
            async,
            ss => (from c in ss.Set<Customer>()
                   join o in ss.Set<Order>().AsNoTracking()
                       on c.CustomerID equals o.CustomerID
                   where c.CustomerID == "ALFKI"
                   select new { c, o }).AsNoTracking(),
            elementSorter: e => (e.c.CustomerID, e.o.OrderID));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Can_get_current_values(bool async)
    {
        using var db = CreateContext();

        if (async)
        {
            var customer = await db.Customers.FirstAsync();
            customer.CompanyName = "foo";
            var dbCustomer = await db.Customers.AsNoTracking().FirstAsync();
            Assert.NotEqual(customer.CompanyName, dbCustomer.CompanyName);
        }
        else
        {
            var customer = db.Customers.First();
            customer.CompanyName = "foo";
            var dbCustomer = db.Customers.AsNoTracking().First();
            Assert.NotEqual(customer.CompanyName, dbCustomer.CompanyName);
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_reference_and_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Applied_after_navigation_expansion(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.Customer.City != "London").AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Where_simple_shadow(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Employee>()
                .Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                .AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Query_fast_path_when_ctor_binding(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().AsNoTracking());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_simple(bool async)
        => AssertQuery(
            async,
            ss => (from e in ss.Set<Employee>()
                   from c in ss.Set<Customer>()
                   select new { c, e }).AsNoTracking(),
            elementSorter: e => (e.c.CustomerID, e.e.EmployeeID));

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
