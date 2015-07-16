// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class ChangeTrackingTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual void Entity_reverts_when_state_set_to_unchanged()
        {
            using (var context = CreateContext())
            {
                var customer = context.Customers.First();
                var firstTrackedEntity = context.ChangeTracker.Entries<Customer>().Single();
                var originalPhoneNumber = customer.Phone;

                customer.Phone = "425-882-8080";

                context.ChangeTracker.DetectChanges();

                Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(EntityState.Modified, firstTrackedEntity.State);
                Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).CurrentValue);

                firstTrackedEntity.State = EntityState.Unchanged;

                Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(originalPhoneNumber, firstTrackedEntity.Property(c => c.Phone).CurrentValue);
                Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
            }
        }

        [Fact]
        public virtual void Multiple_entities_can_revert()
        {
            using (var context = CreateContext())
            {
                var customerPostalCodes = context.Customers.Select(c => c.PostalCode).ToList();
                var customerRegion = context.Customers.Select(c => c.Region).ToList();

                foreach (var customer in context.Customers)
                {
                    customer.PostalCode = "98052";
                    customer.Region = "'Murica";
                }

                Assert.Equal(91, context.ChangeTracker.Entries().Count());
                Assert.Equal("98052", context.Customers.First().PostalCode);
                Assert.Equal("'Murica", context.Customers.First().Region);

                foreach (var entityEntry in context.ChangeTracker.Entries())
                {
                    entityEntry.State = EntityState.Unchanged;
                }

                var newCustomerPostalCodes = context.Customers.Select(c => c.PostalCode);
                var newCustomerRegion = context.Customers.Select(c => c.Region);

                Assert.Equal(customerPostalCodes, newCustomerPostalCodes);
                Assert.Equal(customerRegion, newCustomerRegion);
            }
        }

        [Fact]
        public virtual void Entity_does_not_revert_when_attached_on_DbContext()
        {
            using (var context = CreateContext())
            {
                var customer = context.Customers.First();
                var firstTrackedEntity = context.ChangeTracker.Entries<Customer>().Single();
                var originalPhoneNumber = customer.Phone;
                Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
                Assert.NotEqual(customer.Phone, "425-882-8080");

                var property = firstTrackedEntity.Property(c => c.Phone);
                Assert.NotEqual("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);

                customer.Phone = "425-882-8080";
                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, firstTrackedEntity.State);

                context.Attach(customer);

                Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
                Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).CurrentValue);
                Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);
            }
        }

        [Fact]
        public virtual void Entity_does_not_revert_when_attached_on_DbSet()
        {
            using (var context = CreateContext())
            {
                var customer = context.Customers.First();
                var firstTrackedEntity = context.ChangeTracker.Entries<Customer>().Single();
                Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
                Assert.NotEqual(customer.Phone, "425-882-8080");

                Assert.NotEqual("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);

                customer.Phone = "425-882-8080";
                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, firstTrackedEntity.State);

                context.Customers.Attach(customer);

                Assert.Equal(customer.CustomerID, firstTrackedEntity.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(EntityState.Unchanged, firstTrackedEntity.State);
                Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).CurrentValue);
                Assert.Equal("425-882-8080", firstTrackedEntity.Property(c => c.Phone).OriginalValue);
            }
        }

        [Fact]
        public virtual void Entity_range_does_not_revert_when_attached_dbContext()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers.OrderBy(c => c.CustomerID).Take(2);

                var customer0 = customers.First();
                var customer1 = customers.Skip(1).First();

                var trackedEntity0 = context.ChangeTracker.Entries<Customer>().First();
                var trackedEntity1 = context.ChangeTracker.Entries<Customer>().Skip(1).First();
                var originalPhoneNumber0 = customer0.Phone;
                var originalPhoneNumber1 = customer1.Phone;
                Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
                Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
                Assert.NotEqual(customer0.Phone, "425-882-8080");
                Assert.NotEqual(customer1.Phone, "425-882-8080");

                var property0 = trackedEntity0.Property(c => c.Phone);
                var property1 = trackedEntity1.Property(c => c.Phone);
                Assert.NotEqual("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
                Assert.NotEqual("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);

                customer0.Phone = "425-882-8080";
                customer1.Phone = "425-882-8080";
                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, trackedEntity0.State);
                Assert.Equal(EntityState.Modified, trackedEntity1.State);

                context.AttachRange(customers);

                Assert.Equal(customer0.CustomerID, trackedEntity0.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(customer1.CustomerID, trackedEntity1.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
                Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
                Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).CurrentValue);
                Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).CurrentValue);
                Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
                Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);
            }
        }

        [Fact]
        public virtual void Entity_range_does_not_revert_when_attached_dbSet()
        {
            using (var context = CreateContext())
            {
                var customers = context.Customers.OrderBy(c => c.CustomerID).Take(2);

                var customer0 = customers.First();
                var customer1 = customers.Skip(1).First();

                var trackedEntity0 = context.ChangeTracker.Entries<Customer>().First();
                var trackedEntity1 = context.ChangeTracker.Entries<Customer>().Skip(1).First();
                var originalPhoneNumber0 = customer0.Phone;
                var originalPhoneNumber1 = customer1.Phone;
                Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
                Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
                Assert.NotEqual(customer0.Phone, "425-882-8080");
                Assert.NotEqual(customer1.Phone, "425-882-8080");

                var property0 = trackedEntity0.Property(c => c.Phone);
                var property1 = trackedEntity1.Property(c => c.Phone);
                Assert.NotEqual("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
                Assert.NotEqual("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);

                customer0.Phone = "425-882-8080";
                customer1.Phone = "425-882-8080";
                context.ChangeTracker.DetectChanges();
                Assert.Equal(EntityState.Modified, trackedEntity0.State);
                Assert.Equal(EntityState.Modified, trackedEntity1.State);

                context.Customers.AttachRange(customers);

                Assert.Equal(customer0.CustomerID, trackedEntity0.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(customer1.CustomerID, trackedEntity1.Property(c => c.CustomerID).CurrentValue);
                Assert.Equal(EntityState.Unchanged, trackedEntity0.State);
                Assert.Equal(EntityState.Unchanged, trackedEntity1.State);
                Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).CurrentValue);
                Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).CurrentValue);
                Assert.Equal("425-882-8080", trackedEntity0.Property(c => c.Phone).OriginalValue);
                Assert.Equal("425-882-8080", trackedEntity1.Property(c => c.Phone).OriginalValue);
            }
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected ChangeTrackingTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }
    }
}
