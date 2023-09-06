// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Cosmos;

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

            var logEntry = TestSqlLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("CreateItem", logEntry.Message);
        }

        using (var context = new CustomerContext(options))
        {
            TestSqlLoggerFactory.Clear();
            var customerFromStore = context.Set<Customer>().Single();

            var logEntry = TestSqlLoggerFactory.Log.Last();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadNext", logEntry.Message);
            TestSqlLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);

            customerFromStore.Name = "Theon Greyjoy";

            context.SaveChanges();

            logEntry = TestSqlLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReplaceItem", logEntry.Message);
        }

        using (var context = new CustomerContext(options))
        {
            TestSqlLoggerFactory.Clear();
            var customerFromStore = context.Find<Customer>(42);

            var logEntry = TestSqlLoggerFactory.Log.Last();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadItem", logEntry.Message);
            TestSqlLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            context.Remove(customerFromStore);

            context.SaveChanges();

            logEntry = TestSqlLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("DeleteItem", logEntry.Message);
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

            await context.AddAsync(customer);

            await context.SaveChangesAsync();

            var logEntry = TestSqlLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("CreateItem", logEntry.Message);
        }

        using (var context = new CustomerContext(options))
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            var logEntry = TestSqlLoggerFactory.Log.Last();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadNext", logEntry.Message);
            TestSqlLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);

            customerFromStore.Name = "Theon Greyjoy";

            await context.SaveChangesAsync();

            logEntry = TestSqlLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReplaceItem", logEntry.Message);
        }

        using (var context = new CustomerContext(options))
        {
            var customerFromStore = await context.FindAsync<Customer>(42);

            var logEntry = TestSqlLoggerFactory.Log.Last();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadItem", logEntry.Message);
            TestSqlLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();

            logEntry = TestSqlLoggerFactory.Log.Single();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("DeleteItem", logEntry.Message);
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

            var entry = await context.AddAsync(customer);

            await context.SaveChangesAsync();

            await context.AddAsync(customer);

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

            var entry = await context.AddAsync(customer);

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

            var entry = await context.AddAsync(customer);

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

            await context.AddAsync(customer);

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

    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end_with_DateTime_async()
    {
        var options = Fixture.CreateOptions();

        var customer = new CustomerDateTime
        {
            Id = DateTime.MinValue,
            Name = "Theon/\\#\\\\?",
            PartitionKey = 42
        };

        using (var context = new CustomerContextDateTime(options))
        {
            await context.Database.EnsureCreatedAsync();

            var entry = await context.AddAsync(customer);

            Assert.Equal("CustomerDateTime|0001-01-01T00:00:00.0000000|Theon^2F^5C^23^5C^5C^3F", entry.CurrentValues["__id"]);

            await context.SaveChangesAsync();
        }

        using (var context = new CustomerContextDateTime(options))
        {
            var customerFromStore = await context.Set<CustomerDateTime>().SingleAsync();

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal("Theon/\\#\\\\?", customerFromStore.Name);

            customerFromStore.Value = 23;

            await context.SaveChangesAsync();
        }

        using (var context = new CustomerContextDateTime(options))
        {
            var customerFromStore = await context.Set<CustomerDateTime>().SingleAsync();

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal(23, customerFromStore.Value);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();
        }

        using (var context = new CustomerContextDateTime(options))
        {
            Assert.Empty(await context.Set<CustomerDateTime>().ToListAsync());
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

    private class CustomerDateTime
    {
        public DateTime Id { get; set; }
        public string Name { get; set; }
        public int PartitionKey { get; set; }
        public int Value { get; set; }
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
            => modelBuilder.Entity<Customer>();
    }

    private class CustomerContextGuid : DbContext
    {
        public CustomerContextGuid(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerGuid>(
                cb =>
                {
                    cb.Property(c => c.Id).ToJsonProperty("id");
                    cb.Property(c => c.PartitionKey).HasConversion<string>().ToJsonProperty("pk");
                    cb.HasPartitionKey(c => c.PartitionKey);
                });
    }

    private class CustomerContextDateTime : DbContext
    {
        public CustomerContextDateTime(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerDateTime>(
                cb =>
                {
                    cb.Property(c => c.Id);
                    cb.Property(c => c.PartitionKey).HasConversion<string>();
                    cb.HasPartitionKey(c => c.PartitionKey);
                    cb.HasKey(c => new { c.Id, c.Name });
                });
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_with_dateTime_string_end_to_end_async()
    {
        var options = Fixture.CreateOptions();

        var customer = new Customer { Id = 42, Name = "2021-08-23T06:23:40+00:00" };

        using (var context = new CustomerContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        using (var context = new CustomerContext(options))
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            var logEntry = TestSqlLoggerFactory.Log.Last();
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadNext", logEntry.Message);
            TestSqlLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("2021-08-23T06:23:40+00:00", customerFromStore.Name);

            customerFromStore.Name = "2021-08-23T06:23:40+02:00";

            await context.SaveChangesAsync();
        }

        using (var context = new CustomerContext(options))
        {
            var customerFromStore = await context.FindAsync<Customer>(42);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("2021-08-23T06:23:40+02:00", customerFromStore.Name);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();
        }

        using (var context = new CustomerContext(options))
        {
            Assert.Empty(await context.Set<Customer>().ToListAsync());
        }
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_with_collections()
    {
        await Can_add_update_delete_with_collection(
            new List<short> { 1, 2 },
            c =>
            {
                c.Collection.Clear();
                c.Collection.Add(3);
            },
            new List<short> { 3 });

        await Can_add_update_delete_with_collection<IList<byte?>>(
            new List<byte?>(),
            c =>
            {
                c.Collection.Clear();
                c.Collection.Add(3);
                c.Collection.Add(null);
            },
            new List<byte?> { 3, null });

        await Can_add_update_delete_with_collection<IReadOnlyList<string>>(
            new[] { "1", null },
            c =>
            {
                c.Collection = new List<string>
                {
                    "3",
                    "2",
                    "1"
                };
            },
            new List<string>
            {
                "3",
                "2",
                "1"
            });

        await Assert.ThrowsAsync<ArgumentException>( // #31616
            async () =>
            {
                // See #25343
                await Can_add_update_delete_with_collection(
                    new List<EntityType>
                    {
                        EntityType.Base,
                        EntityType.Derived,
                        EntityType.Derived
                    },
                    c =>
                    {
                        c.Collection.Clear();
                        c.Collection.Add(EntityType.Base);
                    },
                    new List<EntityType> { EntityType.Base },
                    modelBuilder => modelBuilder.Entity<CustomerWithCollection<List<EntityType>>>(
                        c =>
                            c.Property(s => s.Collection)
                                .HasConversion(
                                    m => m.Select(v => (int)v).ToList(), p => p.Select(v => (EntityType)v).ToList(),
                                    new ListComparer<EntityType, List<EntityType>>(
                                        ValueComparer.CreateDefault(typeof(EntityType), false), readOnly: false))));
            });

        await Can_add_update_delete_with_collection(
            new[] { 1f, 2 },
            c =>
            {
                c.Collection[0] = 3f;
            },
            new[] { 3f, 2 });

        await Can_add_update_delete_with_collection(
            new decimal?[] { 1, null },
            c =>
            {
                c.Collection[0] = 3;
            },
            new decimal?[] { 3, null });

        await Can_add_update_delete_with_collection(
            new Dictionary<string, int> { { "1", 1 } },
            c =>
            {
                c.Collection["2"] = 3;
            },
            new Dictionary<string, int> { { "1", 1 }, { "2", 3 } });

        await Can_add_update_delete_with_collection<IDictionary<string, long?>>(
            new SortedDictionary<string, long?> { { "2", 2 }, { "1", 1 } },
            c =>
            {
                c.Collection.Clear();
                c.Collection["2"] = null;
            },
            new SortedDictionary<string, long?> { { "2", null } });

        await Can_add_update_delete_with_collection<IReadOnlyDictionary<string, short?>>(
            ImmutableDictionary<string, short?>.Empty
                .Add("2", 2).Add("1", 1),
            c =>
            {
                c.Collection = ImmutableDictionary<string, short?>.Empty.Add("1", 1).Add("2", null);
            },
            new Dictionary<string, short?> { { "1", 1 }, { "2", null } });
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_with_nested_collections()
    {
        await Can_add_update_delete_with_collection(
            new List<List<short>> { new() { 1, 2 } },
            c =>
            {
                c.Collection.Clear();
                c.Collection.Add(new List<short> { 3 });
            },
            new List<List<short>> { new() { 3 } });
        await Can_add_update_delete_with_collection<IList<byte?[]>>(
            new List<byte?[]>(),
            c =>
            {
                c.Collection.Add(new byte?[] { 3, null });
                c.Collection.Add(null);
            },
            new List<byte?[]> { new byte?[] { 3, null }, null });
        await Can_add_update_delete_with_collection<IReadOnlyList<Dictionary<string, string>>>(
            new Dictionary<string, string>[] { new() { { "1", null } } },
            c =>
            {
                var dictionary = c.Collection[0]["3"] = "2";
            },
            new List<Dictionary<string, string>> { new() { { "1", null }, { "3", "2" } } });

        await Can_add_update_delete_with_collection(
            new List<float>[] { new() { 1f }, new() { 2 } },
            c =>
            {
                c.Collection[1][0] = 3f;
            },
            new List<float>[] { new() { 1f }, new() { 3f } });

        await Can_add_update_delete_with_collection(
            new[] { new decimal?[] { 1, null } },
            c =>
            {
                c.Collection[0][1] = 3;
            },
            new[] { new decimal?[] { 1, 3 } });

        await Can_add_update_delete_with_collection(
            new Dictionary<string, List<int>> { { "1", new List<int> { 1 } } },
            c =>
            {
                c.Collection["2"] = new List<int> { 3 };
            },
            new Dictionary<string, List<int>> { { "1", new List<int> { 1 } }, { "2", new List<int> { 3 } } });

        await Can_add_update_delete_with_collection<IDictionary<string, long?[]>>(
            new SortedDictionary<string, long?[]> { { "2", new long?[] { 2 } }, { "1", new long?[] { 1 } } },
            c =>
            {
                c.Collection.Clear();
                c.Collection["2"] = null;
            },
            new SortedDictionary<string, long?[]> { { "2", null } });

        await Can_add_update_delete_with_collection<IReadOnlyDictionary<string, Dictionary<string, short?>>>(
            ImmutableDictionary<string, Dictionary<string, short?>>.Empty
                .Add("2", new Dictionary<string, short?> { { "value", 2 } })
                .Add("1", new Dictionary<string, short?> { { "value", 1 } }),
            c =>
            {
                c.Collection = ImmutableDictionary<string, Dictionary<string, short?>>.Empty
                    .Add("1", new Dictionary<string, short?> { { "value", 1 } }).Add("2", null);
            },
            new Dictionary<string, Dictionary<string, short?>>
            {
                { "1", new Dictionary<string, short?> { { "value", 1 } } }, { "2", null }
            });
    }

    private async Task Can_add_update_delete_with_collection<TCollection>(
        TCollection initialValue,
        Action<CustomerWithCollection<TCollection>> modify,
        TCollection modifiedValue,
        Action<ModelBuilder> onModelBuilder = null)
        where TCollection : class
    {
        var options = Fixture.CreateOptions();

        var customer = new CustomerWithCollection<TCollection>
        {
            Id = 42,
            Name = "Theon",
            Collection = initialValue
        };

        using (var context = new CollectionCustomerContext<TCollection>(options, onModelBuilder))
        {
            await context.Database.EnsureCreatedAsync();

            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        using (var context = new CollectionCustomerContext<TCollection>(options))
        {
            var customerFromStore = await context.Customers.SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal(initialValue, customerFromStore.Collection);

            modify(customerFromStore);

            await context.SaveChangesAsync();
        }

        using (var context = new CollectionCustomerContext<TCollection>(options))
        {
            var customerFromStore = await context.Customers.SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal(modifiedValue, customerFromStore.Collection);

            customerFromStore.Collection = null;

            await context.SaveChangesAsync();
        }

        using (var context = new CollectionCustomerContext<TCollection>(options))
        {
            var customerFromStore = await context.Customers.SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Null(customerFromStore.Collection);
        }
    }

    private class CustomerWithCollection<TCollection>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TCollection Collection { get; set; }
    }

    private class CollectionCustomerContext<TCollection> : DbContext
    {
        private readonly Action<ModelBuilder> _onModelBuilder;

        public DbSet<CustomerWithCollection<TCollection>> Customers { get; set; }

        public CollectionCustomerContext(DbContextOptions dbContextOptions, Action<ModelBuilder> onModelBuilder = null)
            : base(dbContextOptions)
        {
            _onModelBuilder = onModelBuilder;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => _onModelBuilder?.Invoke(modelBuilder);
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

            await context.AddAsync(customer);
            await context.AddAsync(
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

            await context.AddAsync(customer);
            await context.AddAsync(
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
                context,
"""
@__p_1='42'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "Customer") AND (c["Id"] = @__p_1))
OFFSET 0 LIMIT 1
""");

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

            await context.AddAsync(customer);

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

            await context.AddAsync(customer);

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

        var customer = new CustomerGuid { Id = Guid.NewGuid(), Name = "Theon" };

        await using (var context = new PartitionKeyContextPrimaryKey(options))
        {
            await context.Database.EnsureCreatedAsync();

            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        await using (var context = new PartitionKeyContextPrimaryKey(options))
        {
            var customerFromStore = context.Set<CustomerGuid>().Find(customer.Id);

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            AssertSql(context, @$"ReadItem({customer.Id}, {customer.Id})");
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

            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        await using (var context = new PartitionKeyContextWithPrimaryKeyResourceId(options))
        {
            var customerFromStore = context.Set<CustomerWithResourceId>().Find("42");

            Assert.Equal("42", customerFromStore.id);
            Assert.Equal("Theon", customerFromStore.Name);
            AssertSql(
                context,
"""
@__p_0='42'

SELECT c
FROM root c
WHERE ((c["Discriminator"] = "CustomerWithResourceId") AND (c["id"] = @__p_0))
OFFSET 0 LIMIT 1
""");
        }
    }

    private class PartitionKeyContext : DbContext
    {
        public PartitionKeyContext(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.HasPartitionKey(c => c.PartitionKey);
                    cb.Property(c => c.PartitionKey).HasConversion<string>();
                    cb.HasKey(c => new { c.Id, c.PartitionKey });
                });
    }

    private class PartitionKeyContextEntityWithNoPartitionKey : DbContext
    {
        public PartitionKeyContextEntityWithNoPartitionKey(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerNoPartitionKey>();
    }

    private class PartitionKeyContextCustomValueGenerator : DbContext
    {
        public PartitionKeyContextCustomValueGenerator(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.Property(StoreKeyConvention.DefaultIdPropertyName)
                        .HasValueGeneratorFactory(typeof(CustomPartitionKeyIdValueGeneratorFactory));

                    cb.Property(c => c.PartitionKey).HasConversion<string>();

                    cb.HasPartitionKey(c => c.PartitionKey);
                    cb.HasKey(c => new { c.PartitionKey, c.Id });
                });
    }

    private class PartitionKeyContextNoValueGenerator : DbContext
    {
        public PartitionKeyContextNoValueGenerator(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.Property(StoreKeyConvention.DefaultIdPropertyName).HasValueGenerator((Type)null);

                    cb.Property(c => c.PartitionKey).HasConversion<string>();

                    cb.HasPartitionKey(c => c.PartitionKey);
                    cb.HasKey(c => new { c.PartitionKey, c.Id });
                });
    }

    private class PartitionKeyContextNonPrimaryKey : DbContext
    {
        public PartitionKeyContextNonPrimaryKey(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>();
    }

    private class PartitionKeyContextPrimaryKey : DbContext
    {
        public PartitionKeyContextPrimaryKey(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerGuid>(
                cb =>
                {
                    cb.Property(c => c.Id).ToJsonProperty("id");
                    cb.HasPartitionKey(c => c.Id);
                });
    }

    private class PartitionKeyContextWithPrimaryKeyResourceId : DbContext
    {
        public PartitionKeyContextWithPrimaryKeyResourceId(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerWithResourceId>(
                cb =>
                {
                    cb.HasPartitionKey(c => c.PartitionKey);
                    cb.Property(c => c.PartitionKey).HasConversion<string>();
                    cb.Property(c => c.id).HasConversion<string>();
                    cb.HasKey(c => new { c.id });
                });
    }

    private class PartitionKeyContextWithResourceId : DbContext
    {
        public PartitionKeyContextWithResourceId(DbContextOptions dbContextOptions)
            : base(dbContextOptions)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerWithResourceId>(
                cb =>
                {
                    cb.HasPartitionKey(c => c.PartitionKey);
                    cb.Property(c => c.PartitionKey).HasConversion<string>();
                    cb.HasKey(c => new { c.PartitionKey, c.id });
                });
    }

    [ConditionalFact]
    public async Task Can_use_detached_entities_without_discriminators()
    {
        var options = Fixture.CreateOptions();

        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = new NoDiscriminatorCustomerContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        using (var context = new NoDiscriminatorCustomerContext(options))
        {
            (await context.AddAsync(customer)).State = EntityState.Modified;

            customer.Name = "Theon Greyjoy";

            await context.SaveChangesAsync();
        }

        using (var context = new NoDiscriminatorCustomerContext(options))
        {
            var customerFromStore = context.Set<Customer>().AsNoTracking().Single();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            (await context.AddAsync(customer)).State = EntityState.Deleted;

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
            => modelBuilder.Entity<Customer>().HasNoDiscriminator();
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

            await context.AddAsync(customer);

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
            => modelBuilder.Entity<Customer>().Property(c => c.Name).ToJsonProperty("");
    }

    [ConditionalFact]
    public async Task Add_update_delete_query_throws_if_no_container()
    {
        await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName + "Empty");
        var options = Fixture.CreateOptions(testDatabase);

        var customer = new Customer { Id = 42, Name = "Theon" };
        using (var context = new CustomerContext(options))
        {
            await context.AddAsync(customer);

            Assert.StartsWith(
                "Response status code does not indicate success: NotFound (404); Substatus: 0",
                (await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())).InnerException!.Message);
        }

        using (var context = new CustomerContext(options))
        {
            (await context.AddAsync(customer)).State = EntityState.Modified;

            Assert.StartsWith(
                "Response status code does not indicate success: NotFound (404); Substatus: 0",
                (await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())).InnerException!.Message);
        }

        using (var context = new CustomerContext(options))
        {
            (await context.AddAsync(customer)).State = EntityState.Deleted;

            Assert.StartsWith(
                "Response status code does not indicate success: NotFound (404); Substatus: 0",
                (await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())).InnerException!.Message);
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

                await context.AddAsync(new ConflictingIncompatibleId { id = 42 });

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
            => modelBuilder.Entity<ConflictingIncompatibleId>();
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end_with_conflicting_id()
    {
        var options = Fixture.CreateOptions();

        var entity = new ConflictingId { id = "42", Name = "Theon" };

        using (var context = new ConflictingIdContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            await context.AddAsync(entity);

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
            => modelBuilder.Entity<ConflictingId>();
    }

    [ConditionalFact]
    public async Task Can_have_non_string_property_named_Discriminator()
    {
        using var context = new NonStringDiscriminatorContext(Fixture.CreateOptions());
        context.Database.EnsureCreated();

        var entry = await context.AddAsync(new NonStringDiscriminator { Id = 1 });
        await context.SaveChangesAsync();

        var document = entry.Property<JObject>("__jObject").CurrentValue;
        Assert.NotNull(document);
        Assert.Equal("0", document["Discriminator"]);

        Assert.NotNull(await context.Set<NonStringDiscriminator>()
            .Where(e => e.Discriminator == EntityType.Base).OrderBy(e => e.Id).FirstOrDefaultAsync());

        AssertSql(
            context,
"""
SELECT c
FROM root c
WHERE (c["Discriminator"] = 0)
ORDER BY c["Id"]
OFFSET 0 LIMIT 1
""");
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
            => modelBuilder.Entity<NonStringDiscriminator>();
    }

    private void AssertSql(DbContext context, params string[] expected)
    {
        var logger = (TestSqlLoggerFactory)context.GetService<ILoggerFactory>();
        logger.AssertBaseline(expected);
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)Fixture.ListLoggerFactory;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected void AssertContainsSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

    protected ListLoggerFactory LoggerFactory { get; }

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

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Database.Command.Name;

        public Task InitializeAsync()
            => Task.CompletedTask;

        public Task DisposeAsync()
            => TestStore.DisposeAsync();
    }
}
