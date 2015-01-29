// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class ShadowStateUpdateTest : IClassFixture<InMemoryFixture>
    {
        [Fact]
        public async Task Can_add_update_delete_end_to_end_using_only_shadow_state()
        {
            var model = new Model();

            var customerType = model.AddEntityType("Customer");
            customerType.GetOrSetPrimaryKey(customerType.AddProperty("Id", typeof(int), shadowProperty: true));
            customerType.GetOrAddProperty("Name", typeof(string), shadowProperty: true);

            var options = new DbContextOptions()
                .UseModel(model);
            options.UseInMemoryStore();

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                // TODO: Better API for shadow state access
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry[customerType.GetProperty("Id")] = 42;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys";

                customerEntry.SetEntityState(EntityState.Added);

                await context.SaveChangesAsync();

                customerEntry[customerType.GetProperty("Name")] = "Changed!";
            }

            // TODO: Fix this when we can query shadow entities
            // var customerFromStore = await inMemoryDataStore.Read(customerType).SingleAsync();
            //
            // Assert.Equal(new object[] { 42, "Daenerys" }, customerFromStore);

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry[customerType.GetProperty("Id")] = 42;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys Targaryen";

                customerEntry.SetEntityState(EntityState.Modified);

                await context.SaveChangesAsync();
            }

            // TODO: Fix this when we can query shadow entities
            // customerFromStore = await inMemoryDataStore.Read(customerType).SingleAsync();
            // 
            // Assert.Equal(new object[] { 42, "Daenerys Targaryen" }, customerFromStore);

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry[customerType.GetProperty("Id")] = 42;

                customerEntry.SetEntityState(EntityState.Deleted);

                await context.SaveChangesAsync();
            }

            // TODO: Fix this when we can query shadow entities
            // Assert.Equal(0, await inMemoryDataStore.Read(customerType).CountAsync());
        }

        [Fact]
        public async Task Can_add_update_delete_end_to_end_using_partial_shadow_state()
        {
            var model = new Model();

            var customerType = model.AddEntityType(typeof(Customer));
            customerType.GetOrSetPrimaryKey(customerType.GetOrAddProperty("Id", typeof(int)));
            customerType.GetOrAddProperty("Name", typeof(string), shadowProperty: true);

            var options = new DbContextOptions()
                .UseModel(model);
            options.UseInMemoryStore();

            var customer = new Customer { Id = 42 };

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                context.Add(customer);

                // TODO: Better API for shadow state access
                var customerEntry = context.Entry(customer).StateEntry;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys";

                await context.SaveChangesAsync();

                customerEntry[customerType.GetProperty("Name")] = "Changed!";
            }

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal(
                    "Daenerys",
                    (string)context.Entry(customerFromStore).Property("Name").CurrentValue);
            }

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                var customerEntry = context.Entry(customer).StateEntry;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys Targaryen";

                context.Update(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal(
                    "Daenerys Targaryen",
                    (string)context.Entry(customerFromStore).Property("Name").CurrentValue);
            }

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                context.Remove(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new DbContext(_fixture.ServiceProvider, options))
            {
                Assert.Equal(0, context.Set<Customer>().Count());
            }
        }

        private readonly InMemoryFixture _fixture;

        public ShadowStateUpdateTest(InMemoryFixture fixture)
        {
            _fixture = fixture;
        }

        private class Customer
        {
            private Customer(object[] values)
            {
                Id = (int)values[0];
            }

            public Customer()
            {
            }

            public int Id { get; set; }
        }
    }
}
