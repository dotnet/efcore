// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.InMemory.FunctionalTests
{
    public class UpdateTests
    {
        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public async Task Can_add_update_delete_end_to_end()
        {
            var inMemoryDataStore = new InMemoryDataStore();

            var entityConfiguration
                = new EntityConfiguration
                    {
                        DataStore = inMemoryDataStore,
                        Model = CreateModel()
                    };

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = entityConfiguration.CreateContext())
            {
                context.Add(customer);

                await context.SaveChangesAsync();
            }

            Assert.Equal(1, inMemoryDataStore.Objects.Count);
            Assert.Equal(new object[] { 42, "Theon" }, inMemoryDataStore.Objects[customer]);

            using (var context = entityConfiguration.CreateContext())
            {
                customer.Name = "Theon Greyjoy";
                context.Update(customer);

                await context.SaveChangesAsync();
            }

            Assert.Equal(1, inMemoryDataStore.Objects.Count);
            Assert.Equal(new object[] { 42, "Theon Greyjoy" }, inMemoryDataStore.Objects[customer]);

            using (var context = entityConfiguration.CreateContext())
            {
                context.Delete(customer);

                await context.SaveChangesAsync();
            }

            Assert.Equal(0, inMemoryDataStore.Objects.Count);
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
