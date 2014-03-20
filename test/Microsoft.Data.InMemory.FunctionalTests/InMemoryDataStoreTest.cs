// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Xunit;

namespace Microsoft.Data.InMemory.FunctionalTests
{
    public class InMemoryDataStoreTest
    {
        [Fact]
        public async Task Can_add_update_delete_end_to_end()
        {
            var inMemoryDataStore = new InMemoryDataStore();
            var model = CreateModel();

            var configuration = new EntityConfigurationBuilder()
                .UseModel(model)
                .UseDataStore(inMemoryDataStore)
                .BuildConfiguration();

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new EntityContext(configuration))
            {
                context.Add(customer);

                await context.SaveChangesAsync();

                customer.Name = "Changed!";
            }

            var customerFromStore = await inMemoryDataStore.Read(typeof(Customer), model).SingleAsync();

            Assert.Equal(new object[] { 42, "Theon" }, customerFromStore);

            using (var context = new EntityContext(configuration))
            {
                customer.Name = "Theon Greyjoy";
                context.Update(customer);

                await context.SaveChangesAsync();
            }

            customerFromStore = await inMemoryDataStore.Read(typeof(Customer), model).SingleAsync();

            Assert.Equal(new object[] { 42, "Theon Greyjoy" }, customerFromStore);

            using (var context = new EntityContext(configuration))
            {
                context.Delete(customer);

                await context.SaveChangesAsync();
            }

            Assert.Equal(0, await inMemoryDataStore.Read(typeof(Customer), model).CountAsync());
        }

        private class Customer
        {
            // ReSharper disable once UnusedMember.Local
            private Customer(object[] values)
            {
                Id = (int)values[0];
                Name = (string)values[1];
            }

            public Customer()
            {
            }

            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static Model CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(c => c.Id)
                .Properties(ps => ps.Property(c => c.Name));

            return model;
        }
    }
}
