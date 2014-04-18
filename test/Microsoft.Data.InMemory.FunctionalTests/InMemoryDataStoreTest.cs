// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.DependencyInjection.Advanced;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Xunit;

namespace Microsoft.Data.InMemory.FunctionalTests
{
    public class InMemoryDataStoreTest
    {
        [Fact]
        public async Task Can_add_update_delete_end_to_end()
        {
            var model = CreateModel();

            var configuration = new EntityConfigurationBuilder()
                .WithServices(s => s.AddInMemoryStore().UseLoggerFactory(TestFileLogger.Factory))
                .UseModel(model)
                .UseInMemoryStore(persist: true)
                .BuildConfiguration();

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new EntityContext(configuration))
            {
                context.Add(customer);

                await context.SaveChangesAsync();

                customer.Name = "Changed!";
            }

            using (var context = new EntityContext(configuration))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
            }

            using (var context = new EntityContext(configuration))
            {
                customer.Name = "Theon Greyjoy";
                context.Update(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new EntityContext(configuration))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            }

            using (var context = new EntityContext(configuration))
            {
                context.Delete(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new EntityContext(configuration))
            {
                Assert.Equal(0, context.Set<Customer>().Count());
            }
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

        [Fact]
        public async Task Can_share_instance_between_contexts_with_sugar_experience()
        {
            using (var db = new SimpleContext())
            {
                db.Artists.Add(new SimpleContext.Artist { Name = "John Doe" });
                await db.SaveChangesAsync();
            }

            using (var db = new SimpleContext())
            {
                var data = db.Artists.ToList();
                Assert.Equal(1, data.Count);
                Assert.Equal("John Doe", data[0].Name);
            }
        }

        private class SimpleContext : EntityContext
        {
            public EntitySet<Artist> Artists { get; set; }

            protected override void OnConfiguring(EntityConfigurationBuilder builder)
            {
                builder.WithServices(s => s.AddInMemoryStore())
                    .UseInMemoryStore(persist: true);
            }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<Artist>().Key(a => a.ArtistId);
            }

            public class Artist : ArtistBase<string>
            {
            }

            public class ArtistBase<TKey>
            {
                public TKey ArtistId { get; set; }
                public string Name { get; set; }
            }
        }
    }
}
