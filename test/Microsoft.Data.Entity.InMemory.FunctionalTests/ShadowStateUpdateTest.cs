// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class ShadowStateUpdateTest
    {
        [Fact]
        public async Task Can_add_update_delete_end_to_end_using_only_shadow_state()
        {
            var model = new Model();

            var customerType = new EntityType("Customer");
            customerType.SetKey(customerType.AddProperty("Id", typeof(int), shadowProperty: true, concurrencyToken: false));
            customerType.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: false);

            model.AddEntityType(customerType);

            var configuration = new DbContextOptions()
                .UseModel(model)
                .UseInMemoryStore()
                .BuildConfiguration();

            using (var context = new DbContext(configuration))
            {
                // TODO: Better API for shadow state access
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry[customerType.GetProperty("Id")] = 42;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys";

                await customerEntry.SetEntityStateAsync(EntityState.Added, CancellationToken.None);

                await context.SaveChangesAsync();

                customerEntry[customerType.GetProperty("Name")] = "Changed!";
            }

            // TODO: Fix this when we can query shadow entities
            // var customerFromStore = await inMemoryDataStore.Read(customerType).SingleAsync();
            //
            // Assert.Equal(new object[] { 42, "Daenerys" }, customerFromStore);

            using (var context = new DbContext(configuration))
            {
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry[customerType.GetProperty("Id")] = 42;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys Targaryen";

                await customerEntry.SetEntityStateAsync(EntityState.Modified, CancellationToken.None);

                await context.SaveChangesAsync();
            }

            // TODO: Fix this when we can query shadow entities
            // customerFromStore = await inMemoryDataStore.Read(customerType).SingleAsync();
            // 
            // Assert.Equal(new object[] { 42, "Daenerys Targaryen" }, customerFromStore);

            using (var context = new DbContext(configuration))
            {
                var customerEntry = context.ChangeTracker.StateManager.CreateNewEntry(customerType);
                customerEntry[customerType.GetProperty("Id")] = 42;

                await customerEntry.SetEntityStateAsync(EntityState.Deleted, CancellationToken.None);

                await context.SaveChangesAsync();
            }

            // TODO: Fix this when we can query shadow entities
            // Assert.Equal(0, await inMemoryDataStore.Read(customerType).CountAsync());
        }

        [Fact]
        public async Task Can_add_update_delete_end_to_end_using_partial_shadow_state()
        {
            var model = new Model();

            var customerType = new EntityType(typeof(Customer));
            customerType.SetKey(customerType.AddProperty("Id", typeof(int), shadowProperty: false, concurrencyToken: false));
            customerType.AddProperty("Name", typeof(string), shadowProperty: true, concurrencyToken: false);

            model.AddEntityType(customerType);

            var configuration = new DbContextOptions()
                .UseModel(model)
                .UseInMemoryStore()
                .BuildConfiguration();

            var customer = new Customer { Id = 42 };

            using (var context = new DbContext(configuration))
            {
                context.Add(customer);

                // TODO: Better API for shadow state access
                var customerEntry = context.ChangeTracker.Entry(customer).StateEntry;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys";

                await context.SaveChangesAsync();

                customerEntry[customerType.GetProperty("Name")] = "Changed!";
            }

            using (var context = new DbContext(configuration))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal(
                    "Daenerys",
                    (string)context.ChangeTracker.Entry(customerFromStore).Property("Name").CurrentValue);
            }

            using (var context = new DbContext(configuration))
            {
                var customerEntry = context.ChangeTracker.Entry(customer).StateEntry;
                customerEntry[customerType.GetProperty("Name")] = "Daenerys Targaryen";

                context.Update(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new DbContext(configuration))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal(
                    "Daenerys Targaryen",
                    (string)context.ChangeTracker.Entry(customerFromStore).Property("Name").CurrentValue);
            }

            using (var context = new DbContext(configuration))
            {
                context.Delete(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new DbContext(configuration))
            {
                Assert.Equal(0, context.Set<Customer>().Count());
            }
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
