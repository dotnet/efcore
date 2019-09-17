// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Newtonsoft.Json.Linq;
using Xunit;

// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class EndToEndCosmosTest : IClassFixture<EndToEndCosmosTest.CosmosFixture>
    {
        private const string DatabaseName = "CosmosEndToEndTest";

        protected CosmosFixture Fixture { get; }

        public EndToEndCosmosTest(CosmosFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_end_to_end()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new CustomerContext(options))
            {
                context.Database.EnsureCreated();

                context.Add(customer);

                context.SaveChanges();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);

                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                context.Remove(customerFromStore);

                context.SaveChanges();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(context.Set<Customer>().ToList());
            }
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_end_to_end_async()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new CustomerContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);

                customerFromStore.Name = "Theon Greyjoy";

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                context.Remove(customerFromStore);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(await context.Set<Customer>().ToListAsync());
            }
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_detached_entity_end_to_end_async()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon" };
            string storeId = null;
            using (var context = new CustomerContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var entry = context.Add(customer);

                await context.SaveChangesAsync();

                context.Add(customer);

                storeId = entry.Property<string>("id").CurrentValue;
            }

            Assert.NotNull(storeId);

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
            }

            using (var context = new CustomerContext(options))
            {
                customer.Name = "Theon Greyjoy";

                var entry = context.Entry(customer);
                entry.Property<string>("id").CurrentValue = storeId;

                entry.State = EntityState.Modified;

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            }

            using (var context = new CustomerContext(options))
            {
                var entry = context.Entry(customer);
                entry.Property<string>("id").CurrentValue = storeId;
                entry.State = EntityState.Deleted;

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(context.Set<Customer>().ToList());
            }
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int PartitionKey { get; set; }
        }

        private class CustomerContext : DbContext
        {
            public CustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>();
            }
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_end_to_end_with_partition_key()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon", PartitionKey = 1 };

            using (var context = new PartitionKeyContext(options))
            {
                context.Database.EnsureCreated();

                context.Add(customer);

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(1, customerFromStore.PartitionKey);

                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                Assert.Equal(1, customerFromStore.PartitionKey);

                context.Remove(customerFromStore);

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContext(options))
            {
                Assert.Empty(context.Set<Customer>().ToList());
            }
        }

        private class PartitionKeyContext : DbContext
        {
            public PartitionKeyContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>().HasPartitionKey(c => c.PartitionKey);
                modelBuilder.Entity<Customer>().Property(c => c.PartitionKey).HasConversion<string>();
            }
        }

        [ConditionalFact]
        public async Task Can_use_detached_entities_without_discriminators()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                context.Database.EnsureCreated();

                context.Add(customer);

                context.SaveChanges();
            }

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                context.Add(customer).State = EntityState.Modified;

                customer.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().AsNoTracking().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                context.Add(customer).State = EntityState.Deleted;

                context.SaveChanges();
            }

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                Assert.Empty(context.Set<Customer>().ToList());
            }
        }

        private class NoDiscriminatorCustomerContext : CustomerContext
        {
            public NoDiscriminatorCustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>().HasNoDiscriminator();
            }
        }

        [ConditionalFact]
        public async Task Can_update_unmapped_properties()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new ExtraCustomerContext(options))
            {
                context.Database.EnsureCreated();

                var entry = context.Add(customer);
                entry.Property<string>("EMail").CurrentValue = "theon.g@winterfell.com";

                context.SaveChanges();
            }

            using (var context = new ExtraCustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);

                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new ExtraCustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                var entry = context.Entry(customerFromStore);
                Assert.Equal("theon.g@winterfell.com", entry.Property<string>("EMail").CurrentValue);

                var json = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.Equal("theon.g@winterfell.com", json["e-mail"]);

                context.Remove(customerFromStore);

                context.SaveChanges();
            }

            using (var context = new ExtraCustomerContext(options))
            {
                Assert.Empty(context.Set<Customer>().ToList());
            }
        }

        private class ExtraCustomerContext : CustomerContext
        {
            public ExtraCustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasDefaultContainer(nameof(CustomerContext));
                modelBuilder.Entity<Customer>().Property<string>("EMail").ToJsonProperty("e-mail");
            }
        }

        [ConditionalFact]
        public async Task Can_use_non_persisted_properties()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new UnmappedCustomerContext(options))
            {
                context.Database.EnsureCreated();

                context.Add(customer);

                context.SaveChanges();
                Assert.Equal("Theon", customer.Name);
            }

            using (var context = new UnmappedCustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Null(customerFromStore.Name);

                customerFromStore.Name = "Theon Greyjoy";

                Assert.Equal(0, context.SaveChanges());
            }
        }

        private class UnmappedCustomerContext : CustomerContext
        {
            public UnmappedCustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>().Property(c => c.Name).ToJsonProperty("");
            }
        }

        [ConditionalFact]
        public async Task Using_a_conflicting_incompatible_id_throws()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            using (var context = new ConflictingIncompatibleIdContext(options))
            {
                await Assert.ThrowsAnyAsync<Exception>(
                    async () =>
                    {
                        await context.Database.EnsureCreatedAsync();

                        context.Add(new ConflictingIncompatibleId { id = 42 });

                        await context.SaveChangesAsync();
                    });
            }
        }

        private class ConflictingIncompatibleId
        {
            // ReSharper disable once InconsistentNaming
            public int id { get; set; }
            public string Name { get; set; }
        }

        public class ConflictingIncompatibleIdContext : DbContext
        {
            public ConflictingIncompatibleIdContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ConflictingIncompatibleId>();
            }
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_end_to_end_with_conflicting_id()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);
            var options = Fixture.CreateOptions(testDatabase);

            var entity = new ConflictingId { id = "42", Name = "Theon" };

            using (var context = new ConflictingIdContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(entity);

                await context.SaveChangesAsync();
            }

            using (var context = new ConflictingIdContext(options))
            {
                var entityFromStore = context.Set<ConflictingId>().Single();

                Assert.Equal("42", entityFromStore.id);
                Assert.Equal("Theon", entityFromStore.Name);
            }

            using (var context = new ConflictingIdContext(options))
            {
                entity.Name = "Theon Greyjoy";

                context.Update(entity);

                await context.SaveChangesAsync();
            }

            using (var context = new ConflictingIdContext(options))
            {
                var entityFromStore = context.Set<ConflictingId>().Single();

                Assert.Equal("42", entityFromStore.id);
                Assert.Equal("Theon Greyjoy", entityFromStore.Name);
            }

            using (var context = new ConflictingIdContext(options))
            {
                context.Remove(entity);

                await context.SaveChangesAsync();
            }

            using (var context = new ConflictingIdContext(options))
            {
                Assert.Empty(context.Set<ConflictingId>().ToList());
            }
        }

        private class ConflictingId
        {
            public string id { get; set; }
            public string Name { get; set; }
        }

        public class ConflictingIdContext : DbContext
        {
            public ConflictingIdContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ConflictingId>();
            }
        }

        [ConditionalFact]
        public async Task Can_have_non_string_property_named_Discriminator()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);

            using (var context = new NonStringDiscriminatorContext(Fixture.CreateOptions(testDatabase)))
            {
                context.Database.EnsureCreated();

                context.Add(new NonStringDiscriminator { Id = 1 });
                await context.SaveChangesAsync();

                Assert.NotNull(await context.Set<NonStringDiscriminator>().FirstOrDefaultAsync());
            }
        }

        private class NonStringDiscriminator
        {
            public int Id { get; set; }
            public EntityType Discriminator { get; set; }
        }

        private enum EntityType
        {
            Base,
            Derived
        }

        public class NonStringDiscriminatorContext : DbContext
        {
            public NonStringDiscriminatorContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<NonStringDiscriminator>();
            }
        }

        public class CosmosFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;
        }
    }
}
