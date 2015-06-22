// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class DatabaseInMemoryTest
    {
        [Fact]
        public async Task Can_add_update_delete_end_to_end()
        {
            var model = CreateModel();

            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .ServiceCollection()
                .AddInstance(TestFileLogger.Factory)
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder()
                .UseModel(model);
            optionsBuilder.UseInMemoryDatabase(persist: true);

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new DbContext(serviceProvider, optionsBuilder.Options))
            {
                context.Add(customer);

                await context.SaveChangesAsync();

                customer.Name = "Changed!";
            }

            using (var context = new DbContext(serviceProvider, optionsBuilder.Options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
            }

            using (var context = new DbContext(serviceProvider, optionsBuilder.Options))
            {
                customer.Name = "Theon Greyjoy";
                context.Update(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new DbContext(serviceProvider, optionsBuilder.Options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            }

            using (var context = new DbContext(serviceProvider, optionsBuilder.Options))
            {
                context.Remove(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new DbContext(serviceProvider, optionsBuilder.Options))
            {
                Assert.Equal(0, context.Set<Customer>().Count());
            }
        }

        [Fact]
        public void Any_returns_false_for_empty_sets()
        {
            using (var db = new SimpleContext())
            {
                db.Artists.Any();
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
            var modelBuilder = TestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<Customer>();

            return modelBuilder.Model;
        }

        [Fact]
        public async Task Can_share_instance_between_contexts_with_sugar_experience()
        {
            using (var db = new SimpleContext())
            {
                db.Artists.Add(new SimpleContext.Artist { ArtistId = "JDId", Name = "John Doe" });
                await db.SaveChangesAsync();
            }

            using (var db = new SimpleContext())
            {
                var data = db.Artists.ToList();
                Assert.Equal(1, data.Count);
                Assert.Equal("JDId", data[0].ArtistId);
                Assert.Equal("John Doe", data[0].Name);
            }
        }

        private class SimpleContext : DbContext
        {
            public DbSet<Artist> Artists { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Artist>().Key(a => a.ArtistId);
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
