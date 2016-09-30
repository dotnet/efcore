// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable AccessToDisposedClosure
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class AsNoTrackingTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [ConditionalFact]
        public virtual void State_manager_not_loaded()
        {
            StateManagerProxy.IsInitialized = false;

            using (var context = new NorthwindContext(
                Fixture.BuildOptions(
                    new ServiceCollection()
                        .AddScoped<IStateManager, StateManagerProxy>())))
            {
                context.Set<Customer>().AsNoTracking().ToList();
                Assert.False(StateManagerProxy.IsInitialized);

                context.GetService<IStateManager>();
                Assert.True(StateManagerProxy.IsInitialized);
            }
        }

        private class StateManagerProxy : StateManager
        {
            public static bool IsInitialized { get; set; }

            public StateManagerProxy(
                IInternalEntityEntryFactory factory,
                IInternalEntityEntrySubscriber subscriber,
                IInternalEntityEntryNotifier notifier,
                IValueGenerationManager valueGeneration,
                IModel model,
                IDatabase database,
                IConcurrencyDetector concurrencyDetector,
                ICurrentDbContext currentContext)
                : base(factory, subscriber, notifier, valueGeneration, model, database, concurrencyDetector, currentContext)
            {
                IsInitialized = true;
            }
        }

        [ConditionalFact]
        public virtual void Entity_not_added_to_state_manager()
        {
            using (var context = CreateContext())
            {
                var customers = context.Set<Customer>().AsNoTracking().ToList();

                Assert.Equal(91, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Applied_to_body_clause()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                       on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select o)
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Applied_to_multiple_body_clauses()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>().AsNoTracking()
                       from o in context.Set<Order>().AsNoTracking()
                       where c.CustomerID == o.CustomerID
                       select new { c, o })
                        .ToList();

                Assert.Equal(830, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Applied_to_body_clause_with_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                       on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select new { c.CustomerID, c, ocid = o.CustomerID, o })
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Applied_to_projection()
        {
            using (var context = CreateContext())
            {
                var customers
                    = (from c in context.Set<Customer>()
                       join o in context.Set<Order>().AsNoTracking()
                       on c.CustomerID equals o.CustomerID
                       where c.CustomerID == "ALFKI"
                       select new { c, o })
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(6, customers.Count);
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [ConditionalFact]
        public virtual void Can_get_current_values()
        {
            using (var db = CreateContext())
            {
                var customer = db.Customers.First();

                customer.CompanyName = "foo";

                var dbCustomer = db.Customers.AsNoTracking().First();

                Assert.NotEqual(customer.CompanyName, dbCustomer.CompanyName);
            }
        }

        [ConditionalFact]
        public virtual void Include_reference_and_collection()
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<Order>()
                        .Include(o => o.Customer)
                        .Include(o => o.OrderDetails)
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(830, orders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_simple_shadow()
        {
            using (var context = CreateContext())
            {
                var employees
                    = context.Set<Employee>()
                        .Where(e => EF.Property<string>(e, "Title") == "Sales Representative")
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(6, employees.Count);
            }
        }

        [ConditionalFact]
        public virtual void SelectMany_simple()
        {
            using (var context = CreateContext())
            {
                var results
                    = (from e in context.Set<Employee>()
                       from c in context.Set<Customer>()
                       select new { c, e })
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(819, results.Count);
            }
        }

        protected NorthwindContext CreateContext() => Fixture.CreateContext();

        protected AsNoTrackingTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}
