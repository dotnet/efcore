// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Xunit;

namespace Microsoft.Data.InMemory.FunctionalTests
{
    public class ShadowStateUpdateTest
    {
        [Fact]
        public async Task Can_add_update_delete_end_to_end_using_only_shadow_state()
        {
            var model = new Model();

            var customerType = new EntityType("Customer")
                {
                    Key = new[] { new Property("Id", typeof(int), hasClrProperty: false) }
                };

            customerType.AddProperty(new Property("Name", typeof(string), hasClrProperty: false));

            model.AddEntityType(customerType);

            var inMemoryDataStore = new InMemoryDataStore();

            var entityConfiguration = new EntityConfiguration
                {
                    DataStore = inMemoryDataStore,
                    Model = model
                };

            using (var context = entityConfiguration.CreateContext())
            {
                // TODO: Better API for shadow state access
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry.SetPropertyValue(customerType.GetProperty("Id"), 42);
                customerEntry.SetPropertyValue(customerType.GetProperty("Name"), "Daenerys");

                await customerEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);

                await context.SaveChangesAsync();

                customerEntry.SetPropertyValue(customerType.GetProperty("Name"), "Changed!");
            }

            var customerFromStore = await inMemoryDataStore.Read(customerType).SingleAsync();

            Assert.Equal(new object[] { 42, "Daenerys" }, customerFromStore);

            using (var context = entityConfiguration.CreateContext())
            {
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry.SetPropertyValue(customerType.GetProperty("Id"), 42);
                customerEntry.SetPropertyValue(customerType.GetProperty("Name"), "Daenerys Targaryen");

                await customerEntry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None);

                await context.SaveChangesAsync();
            }

            customerFromStore = await inMemoryDataStore.Read(customerType).SingleAsync();

            Assert.Equal(new object[] { 42, "Daenerys Targaryen" }, customerFromStore);

            using (var context = entityConfiguration.CreateContext())
            {
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry.SetPropertyValue(customerType.GetProperty("Id"), 42);

                await customerEntry.SetEntityStateAsync(EntityState.Deleted, CancellationToken.None);

                await context.SaveChangesAsync();
            }

            Assert.Equal(0, await inMemoryDataStore.Read(customerType).CountAsync());
        }

        [Fact]
        public async Task Can_add_update_delete_end_to_end_using_partial_shadow_state()
        {
            var model = new Model();

            var customerType = new EntityType(typeof(Customer))
                {
                    Key = new[] { new Property("Id", typeof(int), hasClrProperty: true) }
                };

            customerType.AddProperty(new Property("Name", typeof(string), hasClrProperty: false));

            model.AddEntityType(customerType);

            var inMemoryDataStore = new InMemoryDataStore();

            var entityConfiguration = new EntityConfiguration
                {
                    DataStore = inMemoryDataStore,
                    Model = model
                };

            var customer = new Customer { Id = 42 };

            using (var context = entityConfiguration.CreateContext())
            {
                context.Add(customer);

                // TODO: Better API for shadow state access
                var customerEntry = context.ChangeTracker.Entry(customer).StateEntry;
                customerEntry.SetPropertyValue(customerType.GetProperty("Name"), "Daenerys");

                await context.SaveChangesAsync();

                customerEntry.SetPropertyValue(customerType.GetProperty("Name"), "Changed!");
            }

            var customerFromStore = await inMemoryDataStore.Read(typeof(Customer), model).SingleAsync();

            Assert.Equal(new object[] { 42, "Daenerys" }, customerFromStore);

            using (var context = entityConfiguration.CreateContext())
            {
                var customerEntry = context.ChangeTracker.Entry(customer).StateEntry;
                customerEntry.SetPropertyValue(customerType.GetProperty("Name"), "Daenerys Targaryen");

                context.Update(customer);

                await context.SaveChangesAsync();
            }

            customerFromStore = await inMemoryDataStore.Read(typeof(Customer), model).SingleAsync();

            Assert.Equal(new object[] { 42, "Daenerys Targaryen" }, customerFromStore);

            using (var context = entityConfiguration.CreateContext())
            {
                context.Delete(customer);

                await context.SaveChangesAsync();
            }

            Assert.Equal(0, await inMemoryDataStore.Read(typeof(Customer), model).CountAsync());
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
