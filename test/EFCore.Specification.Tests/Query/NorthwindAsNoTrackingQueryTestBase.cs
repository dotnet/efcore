// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class NorthwindAsNoTrackingQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase<NoopModelCustomizer>, new()
    {
        protected NorthwindAsNoTrackingQueryTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Entity_not_added_to_state_manager(bool useParam)
        {
            using var context = CreateContext();
            var customers = useParam
                ? context.Set<Customer>().AsTracking(QueryTrackingBehavior.NoTracking).ToList()
                : context.Set<Customer>().AsNoTracking().ToList();

            Assert.Equal(91, customers.Count);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public virtual void Applied_to_body_clause()
        {
            using var context = CreateContext();
            var customers
                = (from c in context.Set<Customer>()
                   join o in context.Set<Order>().AsNoTracking()
                       on c.CustomerID equals o.CustomerID
                   where c.CustomerID == "ALFKI"
                   select o)
                .ToList();

            Assert.Equal(6, customers.Count);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public virtual void Applied_to_multiple_body_clauses()
        {
            using var context = CreateContext();
            var customers
                = (from c in context.Set<Customer>().AsNoTracking()
                   from o in context.Set<Order>().AsNoTracking()
                   where c.CustomerID == o.CustomerID
                   select new { c, o })
                .ToList();

            Assert.Equal(830, customers.Count);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public virtual void Applied_to_body_clause_with_projection()
        {
            using var context = CreateContext();
            var customers
                = (from c in context.Set<Customer>()
                   join o in context.Set<Order>().AsNoTracking()
                       on c.CustomerID equals o.CustomerID
                   where c.CustomerID == "ALFKI"
                   select new
                   {
                       c.CustomerID,
                       c,
                       ocid = o.CustomerID,
                       o
                   })
                .ToList();

            Assert.Equal(6, customers.Count);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public virtual void Applied_to_projection()
        {
            using var context = CreateContext();
            var customers
                = (from c in context.Set<Customer>()
                   join o in context.Set<Order>().AsNoTracking()
                       on c.CustomerID equals o.CustomerID
                   where c.CustomerID == "ALFKI"
                   select new { c, o })
                .AsNoTracking()
                .ToList();

            Assert.Equal(6, customers.Count);
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [ConditionalFact]
        public virtual void Can_get_current_values()
        {
            using var db = CreateContext();
            var customer = db.Customers.First();

            customer.CompanyName = "foo";

            var dbCustomer = db.Customers.AsNoTracking().First();

            Assert.NotEqual(customer.CompanyName, dbCustomer.CompanyName);
        }

        [ConditionalFact]
        public virtual void Include_reference_and_collection()
        {
            using var context = CreateContext();
            var orders
                = context.Set<Order>()
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                    .AsNoTracking()
                    .ToList();

            Assert.Equal(830, orders.Count);
        }

        [ConditionalFact]
        public virtual void Applied_after_navigation_expansion()
        {
            using var context = CreateContext();
            var orders = context.Set<Order>().Where(o => o.Customer.City != "London").AsNoTracking().ToList();

            Assert.Equal(784, orders.Count);
        }

        [ConditionalFact]
        public virtual void Where_simple_shadow()
        {
            using var context = CreateContext();
            var employees
                = context.Set<Employee>()
                    .Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                    .AsNoTracking()
                    .ToList();

            Assert.Equal(6, employees.Count);
        }

        [ConditionalFact]
        public virtual void Query_fast_path_when_ctor_binding()
        {
            using var context = CreateContext();
            var employees
                = context.Set<Customer>()
                    .AsNoTracking()
                    .ToList();

            Assert.Equal(91, employees.Count);
        }

        [ConditionalFact]
        public virtual async Task Query_fast_path_when_ctor_binding_async()
        {
            using var context = CreateContext();
            var employees
                = await context.Set<Customer>()
                    .AsNoTracking()
                    .ToListAsync();

            Assert.Equal(91, employees.Count);
        }

        [ConditionalFact]
        public virtual void SelectMany_simple()
        {
            using var context = CreateContext();
            var results
                = (from e in context.Set<Employee>()
                   from c in context.Set<Customer>()
                   select new { c, e })
                .AsNoTracking()
                .ToList();

            Assert.Equal(819, results.Count);
        }

        protected NorthwindContext CreateContext()
            => Fixture.CreateContext();
    }
}
