// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;
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
        public void Can_add_update_delete_end_to_end()
        {
            var options = Fixture.CreateOptions();

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
            var options = Fixture.CreateOptions();

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
            var options = Fixture.CreateOptions();

            var customer = new Customer { Id = 42, Name = "Theon" };
            string storeId = null;
            using (var context = new CustomerContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var entry = context.Add(customer);

                await context.SaveChangesAsync();

                context.Add(customer);

                storeId = entry.Property<string>(StoreKeyConvention.DefaultIdPropertyName).CurrentValue;
            }

            Assert.Equal("Customer|42", storeId);

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
            }

            using (var context = new CustomerContext(options))
            {
                customer.Name = "Theon Greyjoy";

                var entry = context.Entry(customer);
                entry.Property<string>(StoreKeyConvention.DefaultIdPropertyName).CurrentValue = storeId;

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
                entry.Property<string>(StoreKeyConvention.DefaultIdPropertyName).CurrentValue = storeId;
                entry.State = EntityState.Deleted;

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(await context.Set<Customer>().ToListAsync());
            }
        }

        [ConditionalFact]
        public void Can_add_update_untracked_properties()
        {
            var options = Fixture.CreateOptions();

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new CustomerContext(options))
            {
                context.Database.EnsureCreated();

                var entry = context.Add(customer);

                context.SaveChanges();

                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.NotNull(document);
                Assert.Equal("Theon", document["Name"]);

                context.Remove(customer);

                context.SaveChanges();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(context.Set<Customer>().ToList());

                var entry = context.Add(customer);

                entry.Property<JObject>("__jObject").CurrentValue = new JObject { ["key1"] = "value1" };

                context.SaveChanges();

                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.NotNull(document);
                Assert.Equal("Theon", document["Name"]);
                Assert.Equal("value1", document["key1"]);

                document["key2"] = "value2";
                entry.State = EntityState.Modified;
                context.SaveChanges();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);

                var entry = context.Entry(customerFromStore);
                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.Equal("value1", document["key1"]);
                Assert.Equal("value2", document["key2"]);

                document["key1"] = "value1.1";
                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                var entry = context.Entry(customerFromStore);
                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.Equal("value1.1", document["key1"]);
                Assert.Equal("value2", document["key2"]);

                context.Remove(customerFromStore);

                context.SaveChanges();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(context.Set<Customer>().ToList());
            }
        }

        [ConditionalFact]
        public async Task Can_add_update_untracked_properties_async()
        {
            var options = Fixture.CreateOptions();

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new CustomerContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                var entry = context.Add(customer);

                await context.SaveChangesAsync();

                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.NotNull(document);
                Assert.Equal("Theon", document["Name"]);

                context.Remove(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(await context.Set<Customer>().ToListAsync());

                var entry = context.Add(customer);

                entry.Property<JObject>("__jObject").CurrentValue = new JObject { ["key1"] = "value1" };

                await context.SaveChangesAsync();

                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.NotNull(document);
                Assert.Equal("Theon", document["Name"]);
                Assert.Equal("value1", document["key1"]);

                document["key2"] = "value2";
                entry.State = EntityState.Modified;
                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);

                var entry = context.Entry(customerFromStore);
                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.Equal("value1", document["key1"]);
                Assert.Equal("value2", document["key2"]);

                document["key1"] = "value1.1";
                customerFromStore.Name = "Theon Greyjoy";

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                var entry = context.Entry(customerFromStore);
                var document = entry.Property<JObject>("__jObject").CurrentValue;
                Assert.Equal("value1.1", document["key1"]);
                Assert.Equal("value2", document["key2"]);

                context.Remove(customerFromStore);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContext(options))
            {
                Assert.Empty(await context.Set<Customer>().ToListAsync());
            }
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_end_to_end_with_Guid_async()
        {
            var options = Fixture.CreateOptions();

            var customer = new CustomerGuid
            {
                Id = Guid.NewGuid(),
                Name = "Theon",
                PartitionKey = 42
            };

            using (var context = new CustomerContextGuid(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContextGuid(options))
            {
                var customerFromStore = await context.Set<CustomerGuid>().SingleAsync();

                Assert.Equal(customer.Id, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);

                customerFromStore.Name = "Theon Greyjoy";

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContextGuid(options))
            {
                var customerFromStore = await context.Set<CustomerGuid>().SingleAsync();

                Assert.Equal(customer.Id, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                context.Remove(customerFromStore);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContextGuid(options))
            {
                Assert.Empty(await context.Set<CustomerGuid>().ToListAsync());
            }
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int PartitionKey { get; set; }
        }

        private class CustomerWithResourceId
        {
            public string id { get; set; }
            public string Name { get; set; }
            public int PartitionKey { get; set; }
        }

        private class CustomerGuid
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int PartitionKey { get; set; }
        }

        private class CustomerNoPartitionKey
        {
            public int Id { get; set; }
            public string Name { get; set; }
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

        private class CustomerContextGuid : DbContext
        {
            public CustomerContextGuid(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CustomerGuid>(
                    cb =>
                    {
                        cb.Property(c => c.Id).HasConversion<string>().ToJsonProperty("id");
                        cb.Property(c => c.PartitionKey).HasConversion<string>().ToJsonProperty("pk");
                        cb.HasPartitionKey(c => c.PartitionKey);
                    });
            }
        }

        [ConditionalFact]
        public async Task Can_read_with_find_with_resource_id_async()
        {
            var options = Fixture.CreateOptions();
            const int pk1 = 1;
            const int pk2 = 2;

            var customer = new CustomerWithResourceId
            {
                id = "42",
                Name = "Theon",
                PartitionKey = pk1
            };

            await using (var context = new PartitionKeyContextWithResourceId(options))
            {
                await context.Database.EnsureCreatedAsync();

                Assert.Null(
                    context.Model.FindEntityType(typeof(CustomerWithResourceId))
                        .FindProperty(StoreKeyConvention.DefaultIdPropertyName));

                context.Add(customer);
                context.Add(
                    new CustomerWithResourceId
                    {
                        id = "42",
                        Name = "Theon Twin",
                        PartitionKey = pk2
                    });

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextWithResourceId(options))
            {
                var customerFromStore = await context.Set<CustomerWithResourceId>()
                    .FindAsync(pk1, "42");

                Assert.Equal("42", customerFromStore.id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
                AssertSql(context, @"ReadItem(1, 42)");

                customerFromStore.Name = "Theon Greyjoy";

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextWithResourceId(options))
            {
                var customerFromStore = await context.Set<CustomerWithResourceId>()
                    .WithPartitionKey(partitionKey: pk1.ToString())
                    .FirstAsync();

                Assert.Equal("42", customerFromStore.id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
            }
        }

        [ConditionalFact]
        public void Can_read_with_find_with_resource_id()
        {
            var options = Fixture.CreateOptions();
            const int pk1 = 1;
            const int pk2 = 2;

            var customer = new CustomerWithResourceId
            {
                id = "42",
                Name = "Theon",
                PartitionKey = pk1
            };

            using (var context = new PartitionKeyContextWithResourceId(options))
            {
                context.Database.EnsureCreated();

                context.Add(customer);
                context.Add(
                    new CustomerWithResourceId
                    {
                        id = "42",
                        Name = "Theon Twin",
                        PartitionKey = pk2
                    });

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContextWithResourceId(options))
            {
                var customerFromStore = context.Set<CustomerWithResourceId>()
                    .Find(pk1, "42");

                Assert.Equal("42", customerFromStore.id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
                AssertSql(context, @"ReadItem(1, 42)");

                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContextWithResourceId(options))
            {
                var customerFromStore = context.Set<CustomerWithResourceId>()
                    .WithPartitionKey(partitionKey: pk1.ToString())
                    .First();

                Assert.Equal("42", customerFromStore.id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
            }
        }

        [ConditionalFact]
        public void Find_with_empty_resource_id_throws()
        {
            var options = Fixture.CreateOptions();
            using (var context = new PartitionKeyContextWithResourceId(options))
            {
                context.Database.EnsureCreated();

                Assert.Equal(
                    CosmosStrings.InvalidResourceId,
                    Assert.Throws<InvalidOperationException>(() => context.Set<CustomerWithResourceId>().Find(1, "")).Message);
            }
        }

        [ConditionalFact]
        public async Task Can_read_with_find_with_partition_key_and_value_generator_async()
        {
            var options = Fixture.CreateOptions();
            const int pk1 = 1;
            const int pk2 = 2;

            var customer = new Customer
            {
                Id = 42,
                Name = "Theon",
                PartitionKey = pk1
            };

            await using (var context = new PartitionKeyContextCustomValueGenerator(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);
                context.Add(
                    new Customer
                    {
                        Id = 42,
                        Name = "Theon Twin",
                        PartitionKey = pk2
                    });

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextCustomValueGenerator(options))
            {
                var customerFromStore = await context.Set<Customer>()
                    .FindAsync(pk1, 42);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);

                customerFromStore.Name = "Theon Greyjoy";

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextCustomValueGenerator(options))
            {
                var customerFromStore = await context.Set<Customer>()
                    .WithPartitionKey(partitionKey: pk1.ToString())
                    .FirstAsync();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
            }
        }

        [ConditionalFact]
        public void Can_read_with_find_with_partition_key_and_value_generator()
        {
            var options = Fixture.CreateOptions();
            const int pk1 = 1;
            const int pk2 = 2;

            var customer = new Customer
            {
                Id = 42,
                Name = "Theon",
                PartitionKey = pk1
            };

            using (var context = new PartitionKeyContextCustomValueGenerator(options))
            {
                context.Database.EnsureCreated();

                context.Add(customer);
                context.Add(
                    new Customer
                    {
                        Id = 42,
                        Name = "Theon Twin",
                        PartitionKey = pk2
                    });

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContextCustomValueGenerator(options))
            {
                var customerFromStore = context.Set<Customer>()
                    .Find(pk1, 42);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
                AssertSql(context, @"ReadItem(1, Customer-42)");

                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContextCustomValueGenerator(options))
            {
                var customerFromStore = context.Set<Customer>()
                    .WithPartitionKey(partitionKey: pk1.ToString())
                    .First();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
            }
        }

        [ConditionalFact]
        public void Can_read_with_find_with_partition_key_without_value_generator()
        {
            var options = Fixture.CreateOptions();
            const int pk1 = 1;

            var customer = new Customer
            {
                Id = 42,
                Name = "Theon",
                PartitionKey = pk1
            };

            using (var context = new PartitionKeyContextNoValueGenerator(options))
            {
                context.Database.EnsureCreated();

                var customerEntry = context.Entry(customer);
                customerEntry.Property(StoreKeyConvention.DefaultIdPropertyName).CurrentValue = "42";
                customerEntry.State = EntityState.Added;

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContextNoValueGenerator(options))
            {
                var customerFromStore = context.Set<Customer>()
                    .Find(pk1, 42);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
                AssertSql(
                    context, @"@__p_1='42'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""Customer"") AND (c[""Id""] = @__p_1))
OFFSET 0 LIMIT 1");

                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new PartitionKeyContextNoValueGenerator(options))
            {
                var customerFromStore = context.Set<Customer>()
                    .WithPartitionKey(partitionKey: pk1.ToString())
                    .First();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                Assert.Equal(pk1, customerFromStore.PartitionKey);
            }
        }

        [ConditionalFact]
        public async Task Can_read_with_find_with_partition_key_not_part_of_primary_key()
        {
            var options = Fixture.CreateOptions();

            var customer = new Customer
            {
                Id = 42,
                Name = "Theon",
                PartitionKey = 1
            };

            await using (var context = new PartitionKeyContextNonPrimaryKey(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextNonPrimaryKey(options))
            {
                var customerFromStore = context.Set<Customer>().Find(42);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                AssertSql(context, "ReadItem(, Customer|42)");
            }
        }

        [ConditionalFact]
        public async Task Can_read_with_find_without_partition_key()
        {
            var options = Fixture.CreateOptions();

            var customer = new CustomerNoPartitionKey { Id = 42, Name = "Theon" };

            await using (var context = new PartitionKeyContextEntityWithNoPartitionKey(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextEntityWithNoPartitionKey(options))
            {
                var customerFromStore = context.Set<CustomerNoPartitionKey>().Find(42);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                AssertSql(context, @"ReadItem(, CustomerNoPartitionKey|42)");
            }
        }

        [ConditionalFact]
        public async Task Can_read_with_find_with_PK_partition_key()
        {
            var options = Fixture.CreateOptions();

            var customer = new Customer { Id = 42, Name = "Theon" };

            await using (var context = new PartitionKeyContextPrimaryKey(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextPrimaryKey(options))
            {
                var customerFromStore = context.Set<Customer>().Find(42);

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                AssertSql(context, @"ReadItem(42, 42)");
            }
        }

        [ConditionalFact]
        public async Task Can_read_with_find_with_PK_resource_id()
        {
            var options = Fixture.CreateOptions();

            var customer = new CustomerWithResourceId { id = "42", Name = "Theon" };

            await using (var context = new PartitionKeyContextWithPrimaryKeyResourceId(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            await using (var context = new PartitionKeyContextWithPrimaryKeyResourceId(options))
            {
                var customerFromStore = context.Set<CustomerWithResourceId>().Find("42");

                Assert.Equal("42", customerFromStore.id);
                Assert.Equal("Theon", customerFromStore.Name);
                AssertSql(
                    context, @"@__p_0='42'

SELECT c
FROM root c
WHERE ((c[""Discriminator""] = ""CustomerWithResourceId"") AND (c[""id""] = @__p_0))
OFFSET 0 LIMIT 1");
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
                modelBuilder.Entity<Customer>(
                    cb =>
                    {
                        cb.HasPartitionKey(c => c.PartitionKey);
                        cb.Property(c => c.PartitionKey).HasConversion<string>();
                        cb.HasKey(c => new { c.Id, c.PartitionKey });
                    });
            }
        }

        private class PartitionKeyContextEntityWithNoPartitionKey : DbContext
        {
            public PartitionKeyContextEntityWithNoPartitionKey(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CustomerNoPartitionKey>();
            }
        }

        private class PartitionKeyContextCustomValueGenerator : DbContext
        {
            public PartitionKeyContextCustomValueGenerator(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                    cb =>
                    {
                        cb.Property(StoreKeyConvention.DefaultIdPropertyName)
                            .HasValueGeneratorFactory(typeof(CustomPartitionKeyIdValueGeneratorFactory));

                        cb.Property(c => c.PartitionKey).HasConversion<string>();

                        cb.HasPartitionKey(c => c.PartitionKey);
                        cb.HasKey(c => new { c.PartitionKey, c.Id });
                    });
            }
        }

        private class PartitionKeyContextNoValueGenerator : DbContext
        {
            public PartitionKeyContextNoValueGenerator(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                    cb =>
                    {
                        cb.Property(StoreKeyConvention.DefaultIdPropertyName).HasValueGenerator((Type)null);

                        cb.Property(c => c.PartitionKey).HasConversion<string>();

                        cb.HasPartitionKey(c => c.PartitionKey);
                        cb.HasKey(c => new { c.PartitionKey, c.Id });
                    });
            }
        }

        private class PartitionKeyContextNonPrimaryKey : DbContext
        {
            public PartitionKeyContextNonPrimaryKey(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>();
            }
        }

        private class PartitionKeyContextPrimaryKey : DbContext
        {
            public PartitionKeyContextPrimaryKey(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>(
                    cb =>
                    {
                        cb.HasNoDiscriminator();
                        cb.Property(c => c.Id).HasConversion<string>();
                        cb.HasPartitionKey(c => c.Id);
                    });
            }
        }

        private class PartitionKeyContextWithPrimaryKeyResourceId : DbContext
        {
            public PartitionKeyContextWithPrimaryKeyResourceId(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CustomerWithResourceId>(
                    cb =>
                    {
                        cb.HasPartitionKey(c => c.PartitionKey);
                        cb.Property(c => c.PartitionKey).HasConversion<string>();
                        cb.Property(c => c.id).HasConversion<string>();
                        cb.HasKey(c => new { c.id });
                    });
            }
        }

        private class PartitionKeyContextWithResourceId : DbContext
        {
            public PartitionKeyContextWithResourceId(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CustomerWithResourceId>(
                    cb =>
                    {
                        cb.HasPartitionKey(c => c.PartitionKey);
                        cb.Property(c => c.PartitionKey).HasConversion<string>();
                        cb.HasKey(c => new { c.PartitionKey, c.id });
                    });
            }
        }

        [ConditionalFact]
        public async Task Can_use_detached_entities_without_discriminators()
        {
            var options = Fixture.CreateOptions();

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                context.Add(customer).State = EntityState.Modified;

                customer.Name = "Theon Greyjoy";

                await context.SaveChangesAsync();
            }

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().AsNoTracking().Single();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                context.Add(customer).State = EntityState.Deleted;

                await context.SaveChangesAsync();
            }

            using (var context = new NoDiscriminatorCustomerContext(options))
            {
                Assert.Empty(await context.Set<Customer>().ToListAsync());
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
        public void Can_update_unmapped_properties()
        {
            var options = Fixture.CreateOptions();

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
            var options = Fixture.CreateOptions();

            var customer = new Customer { Id = 42, Name = "Theon" };

            using (var context = new UnmappedCustomerContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
                Assert.Equal("Theon", customer.Name);
            }

            using (var context = new UnmappedCustomerContext(options))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal(42, customerFromStore.Id);
                Assert.Null(customerFromStore.Name);

                customerFromStore.Name = "Theon Greyjoy";

                Assert.Equal(0, await context.SaveChangesAsync());
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
        public async Task Add_update_delete_query_throws_if_no_container()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName + "Empty");
            var options = Fixture.CreateOptions(testDatabase);

            var customer = new Customer { Id = 42, Name = "Theon" };
            using (var context = new CustomerContext(options))
            {
                context.Add(customer);

                Assert.StartsWith(
                    "Response status code does not indicate success: NotFound (404); Substatus: 0",
                    (await Assert.ThrowsAsync<CosmosException>(() => context.SaveChangesAsync())).Message);
            }

            using (var context = new CustomerContext(options))
            {
                context.Add(customer).State = EntityState.Modified;

                Assert.StartsWith(
                    "Response status code does not indicate success: NotFound (404); Substatus: 0",
                    (await Assert.ThrowsAsync<CosmosException>(() => context.SaveChangesAsync())).Message);
            }

            using (var context = new CustomerContext(options))
            {
                context.Add(customer).State = EntityState.Deleted;

                Assert.StartsWith(
                    "Response status code does not indicate success: NotFound (404); Substatus: 0",
                    (await Assert.ThrowsAsync<CosmosException>(() => context.SaveChangesAsync())).Message);
            }

            using (var context = new CustomerContext(options))
            {
                Assert.StartsWith(
                    "Response status code does not indicate success: NotFound (404); Substatus: 0",
                    (await Assert.ThrowsAsync<CosmosException>(() => context.Set<Customer>().SingleAsync())).Message);
            }
        }

        [ConditionalFact]
        public async Task Using_a_conflicting_incompatible_id_throws()
        {
            var options = Fixture.CreateOptions();

            using var context = new ConflictingIncompatibleIdContext(options);
            await Assert.ThrowsAnyAsync<Exception>(
                async () =>
                {
                    await context.Database.EnsureCreatedAsync();

                    context.Add(new ConflictingIncompatibleId { id = 42 });

                    await context.SaveChangesAsync();
                });
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
            var options = Fixture.CreateOptions();

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
            using var context = new NonStringDiscriminatorContext(Fixture.CreateOptions());
            context.Database.EnsureCreated();

            context.Add(new NonStringDiscriminator { Id = 1 });
            await context.SaveChangesAsync();

            Assert.NotNull(await context.Set<NonStringDiscriminator>().OrderBy(e => e.Id).FirstOrDefaultAsync());
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

        private void AssertSql(DbContext context, params string[] expected)
        {
            var logger = (TestSqlLoggerFactory)context.GetService<ILoggerFactory>();
            logger.AssertBaseline(expected);
        }

        public class CosmosFixture : ServiceProviderFixtureBase, IAsyncLifetime
        {
            public CosmosFixture()
            {
                TestStore = CosmosTestStore.Create(DatabaseName);
            }

            protected override ITestStoreFactory TestStoreFactory
                => CosmosTestStoreFactory.Instance;

            public virtual CosmosTestStore TestStore { get; }

            public DbContextOptions CreateOptions()
            {
                TestStore.Initialize(null, (Func<DbContext>)null);
                ListLoggerFactory.Clear();
                return CreateOptions(TestStore);
            }

            public Task InitializeAsync()
                => Task.CompletedTask;

            public Task DisposeAsync()
                => TestStore.DisposeAsync();
        }
    }
}
