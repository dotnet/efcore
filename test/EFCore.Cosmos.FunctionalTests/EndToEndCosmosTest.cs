// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class EndToEndCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            b => b.Entity<Customer>(),
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = contextFactory.CreateContext())
        {
            ListLoggerFactory.Clear();

            context.Add(customer);

            context.SaveChanges();

            var logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedCreateItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("CreateItem", logEntry.Message);

            Assert.Equal(1, ListLoggerFactory.Log.Count(l => l.Id == CosmosEventId.SyncNotSupported));
        }

        using (var context = contextFactory.CreateContext())
        {
            ListLoggerFactory.Clear();
            var customerFromStore = context.Set<Customer>().Single();

            var logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedReadNext);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadNext", logEntry.Message);
            Assert.Single(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
            ListLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);

            customerFromStore.Name = "Theon Greyjoy";

            context.SaveChanges();

            logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedReplaceItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReplaceItem", logEntry.Message);

            Assert.Single(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            ListLoggerFactory.Clear();
            var customerFromStore = context.Find<Customer>(42);

            var logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedReadItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadItem", logEntry.Message);
            Assert.Single(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
            ListLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            context.Remove(customerFromStore);

            context.SaveChanges();

            logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedDeleteItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("DeleteItem", logEntry.Message);

            Assert.Single(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            ListLoggerFactory.Clear();
            Assert.Empty(context.Set<Customer>().ToList());

            Assert.Single(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end_async()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            b => b.Entity<Customer>(),
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();

            var logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedCreateItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("CreateItem", logEntry.Message);

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            var logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedReadNext);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadNext", logEntry.Message);
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
            ListLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);

            customerFromStore.Name = "Theon Greyjoy";

            await context.SaveChangesAsync();

            logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedReplaceItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReplaceItem", logEntry.Message);
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.FindAsync<Customer>(42);

            var logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedReadItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadItem", logEntry.Message);
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
            ListLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();

            logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedDeleteItem);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("DeleteItem", logEntry.Message);
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(await context.Set<Customer>().ToListAsync());
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_detached_entity_end_to_end_async()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            b => b.Entity<Customer>(),
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "Theon" };
        string storeId = null;
        using (var context = contextFactory.CreateContext())
        {
            var entry = await context.AddAsync(customer);

            await context.SaveChangesAsync();

            await context.AddAsync(customer);

            storeId = entry.Property<string>(CosmosJsonIdConvention.DefaultIdPropertyName).CurrentValue;

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        Assert.Equal("42", storeId);

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            customer.Name = "Theon Greyjoy";

            var entry = context.Entry(customer);

            entry.Property<string>(CosmosJsonIdConvention.DefaultIdPropertyName).CurrentValue = storeId;

            entry.State = EntityState.Modified;

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var entry = context.Entry(customer);
            entry.Property<string>(CosmosJsonIdConvention.DefaultIdPropertyName).CurrentValue = storeId;
            entry.State = EntityState.Deleted;

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(await context.Set<Customer>().ToListAsync());
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }
    }

    [ConditionalFact]
    public async Task Can_add_update_untracked_properties_async()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            b => b.Entity<Customer>(),
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = contextFactory.CreateContext())
        {
            var entry = await context.AddAsync(customer);

            await context.SaveChangesAsync();

            var document = entry.Property<JObject>("__jObject").CurrentValue;
            Assert.NotNull(document);
            Assert.Equal("Theon", document["Name"]);

            context.Remove(customer);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
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

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
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

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            var entry = context.Entry(customerFromStore);
            var document = entry.Property<JObject>("__jObject").CurrentValue;
            Assert.Equal("value1.1", document["key1"]);
            Assert.Equal("value2", document["key2"]);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(await context.Set<Customer>().ToListAsync());
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end_with_Guid_async()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            b => b.Entity<CustomerGuid>(
                b =>
                {
                    b.Property(c => c.Id).ToJsonProperty("id");
                    b.Property(c => c.PartitionKey).HasConversion<string>().ToJsonProperty("pk");
                    b.HasPartitionKey(c => c.PartitionKey);
                }),
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        var customer = new CustomerGuid
        {
            Id = Guid.NewGuid(),
            Name = "Theon",
            PartitionKey = 42
        };

        using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<CustomerGuid>().SingleAsync();

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);

            customerFromStore.Name = "Theon Greyjoy";

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<CustomerGuid>().SingleAsync();

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(await context.Set<CustomerGuid>().ToListAsync());
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end_with_DateTime_async()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            b => b.Entity<CustomerDateTime>(
                b =>
                {
                    b.Property(c => c.Id);
                    b.Property(c => c.PartitionKey).HasConversion<string>();
                    b.HasPartitionKey(c => c.PartitionKey);
                    b.HasKey(c => new { c.Id, c.Name });
                }),
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        var customer = new CustomerDateTime
        {
            Id = DateTime.MinValue,
            Name = "Theon/\\#\\\\?",
            PartitionKey = 42
        };

        using (var context = contextFactory.CreateContext())
        {
            var entry = await context.AddAsync(customer);

            Assert.Equal("0001-01-01T00:00:00.0000000|Theon^2F^5C^23^5C^5C^3F", entry.CurrentValues["__id"]);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<CustomerDateTime>().SingleAsync();

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal("Theon/\\#\\\\?", customerFromStore.Name);

            customerFromStore.Value = 23;

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<CustomerDateTime>().SingleAsync();

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal(23, customerFromStore.Value);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(await context.Set<CustomerDateTime>().ToListAsync());
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }
    }

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PartitionKey1 { get; set; }
        public bool PartitionKey3 { get; set; }
        public string PartitionKey2 { get; set; }
    }

    private class CustomerWithResourceId
    {
        public string id { get; set; }
        public string Name { get; set; }
        public int PartitionKey1 { get; set; }
        public decimal PartitionKey2 { get; set; }
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

    [ConditionalFact]
    public async Task Can_add_update_delete_with_dateTime_string_end_to_end_async()
    {
        var contextFactory = await InitializeAsync<DbContext>(
            b => b.Entity<Customer>(),
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "2021-08-23T06:23:40+00:00" };

        using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            var logEntry = ListLoggerFactory.Log.Single(e => e.Id == CosmosEventId.ExecutedReadNext);
            Assert.Equal(LogLevel.Information, logEntry.Level);
            Assert.Contains("ReadNext", logEntry.Message);
            ListLoggerFactory.Clear();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("2021-08-23T06:23:40+00:00", customerFromStore.Name);

            customerFromStore.Name = "2021-08-23T06:23:40+02:00";

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.FindAsync<Customer>(42);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("2021-08-23T06:23:40+02:00", customerFromStore.Name);

            context.Remove(customerFromStore);

            await context.SaveChangesAsync();

            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(await context.Set<Customer>().ToListAsync());
            Assert.DoesNotContain(ListLoggerFactory.Log, l => l.Id == CosmosEventId.SyncNotSupported);
        }
    }

    [ConditionalFact]
    public async Task Entities_with_null_PK_can_be_added_with_normal_use_of_DbContext_methods_and_have_id_shadow_value_and_PK_created()
    {
        var contextFactory = await InitializeAsync<IdentifierShadowValuePresenceTestContext>(
            usePooling: false,
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var context = contextFactory.CreateContext();
        var item = new GItem();

        Assert.Null(item.Id);

        var entry = await context.AddAsync(item);

        var id = entry.Property("Id").CurrentValue;

        Assert.NotNull(item.Id);
        Assert.NotNull(id);

        Assert.Equal(item.Id, id);
        Assert.Equal(EntityState.Added, entry.State);
    }

    [ConditionalFact]
    public async Task
        Entities_can_be_tracked_with_normal_use_of_DbContext_methods_and_have_correct_resultant_state_and_id_shadow_value()
    {
        var contextFactory = await InitializeAsync<IdentifierShadowValuePresenceTestContext>(
            usePooling: false,
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var context = contextFactory.CreateContext();

        var item = new Item { Id = 1337 };
        var entry = context.Attach(item);

        Assert.Equal($"{item.Id}", entry.Property("__id").CurrentValue);
        Assert.Equal(EntityState.Unchanged, entry.State);

        entry.State = EntityState.Detached;
        entry = context.Update(item = new Item { Id = 71 });

        Assert.Equal($"{item.Id}", entry.Property("__id").CurrentValue);
        Assert.Equal(EntityState.Modified, entry.State);

        entry.State = EntityState.Detached;
        entry = context.Remove(item = new Item { Id = 33 });

        Assert.Equal($"{item.Id}", entry.Property("__id").CurrentValue);
        Assert.Equal(EntityState.Deleted, entry.State);
    }

    protected class IdentifierShadowValuePresenceTestContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        public DbSet<GItem> GItems { get; set; }
        public DbSet<Item> Items { get; set; }
    }

    protected class GItem
    {
        public Guid? Id { get; set; }
    }

    protected class Item
    {
        public int Id { get; set; }
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_with_collections()
    {
        await Can_add_update_delete_with_collection(
            [1, 2],
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

        // See #34026
        await Can_add_update_delete_with_collection(
            [
                Discriminator.Base,
                Discriminator.Derived,
                Discriminator.Derived
            ],
            c =>
            {
                c.Collection.Clear();
                c.Collection.Add(Discriminator.Base);
            },
            new List<Discriminator> { Discriminator.Base },
            modelBuilder => modelBuilder.Entity<CustomerWithCollection<List<Discriminator>>>(
                c =>
                    c.Property(s => s.Collection)
                        .HasConversion(
                            m => m.Select(v => (int)v).ToList(), p => p.Select(v => (Discriminator)v).ToList(),
                            new ListOfValueTypesComparer<List<Discriminator>, Discriminator>(
                                ValueComparer.CreateDefault(typeof(Discriminator), false)))));

        await Can_add_update_delete_with_collection(
            [1f, 2],
            c =>
            {
                c.Collection[0] = 3f;
            },
            new[] { 3f, 2 });

        await Can_add_update_delete_with_collection(
            [1, null],
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
            [[1, 2]],
            c =>
            {
                c.Collection.Clear();
                c.Collection.Add([3]);
            },
            new List<List<short>> { new() { 3 } });

        await Can_add_update_delete_with_collection<IList<byte?[]>>(
            new List<byte?[]>(),
            c =>
            {
                c.Collection.Add([3, null]);
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
            [[1f], [2]],
            c =>
            {
                c.Collection[1][0] = 3f;
            },
            new List<float>[] { [1f], [3f] });

        await Can_add_update_delete_with_collection(
            [[1, null]],
            c =>
            {
                c.Collection[0][1] = 3;
            },
            new[] { new decimal?[] { 1, 3 } });

        await Can_add_update_delete_with_collection(
            new Dictionary<string, List<int>> { { "1", [1] } },
            c =>
            {
                c.Collection["2"] = [3];
            },
            new Dictionary<string, List<int>> { { "1", [1] }, { "2", [3] } });

        // Issue #34105
        await Can_add_update_delete_with_collection(
            new Dictionary<string, string[]> { { "1", ["1"] } },
            c =>
            {
                c.Collection["2"] = ["3"];
            },
            new Dictionary<string, string[]> { { "1", ["1"] }, { "2", ["3"] } });

        await Can_add_update_delete_with_collection<IDictionary<string, long?[]>>(
            new SortedDictionary<string, long?[]> { { "2", [2] }, { "1", [1] } },
            c =>
            {
                c.Collection.Clear();
                c.Collection["2"] = null;
            },
            new SortedDictionary<string, long?[]> { { "2", null } });

        await Can_add_update_delete_with_collection<IReadOnlyDictionary<string, Dictionary<string, short?>>>(
            new Dictionary<string, Dictionary<string, short?>>
            {
                { "2", new Dictionary<string, short?> { { "value", 2 } } }, { "1", new Dictionary<string, short?> { { "value", 1 } } }
            },
            c =>
            {
                c.Collection = new Dictionary<string, Dictionary<string, short?>>
                {
                    { "1", new Dictionary<string, short?> { { "value", 1 } } }, { "2", null }
                };
            },
            new Dictionary<string, Dictionary<string, short?>>
            {
                { "1", new Dictionary<string, short?> { { "value", 1 } } }, { "2", null }
            });

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
        var contextFactory = await InitializeAsync<CollectionCustomerContext<TCollection>>(
            shouldLogCategory: _ => true,
            onModelCreating: onModelBuilder,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new CustomerWithCollection<TCollection>
        {
            Id = 42,
            Name = "Theon",
            Collection = initialValue
        };

        using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Customers.SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal(initialValue, customerFromStore.Collection);

            modify(customerFromStore);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Customers.SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal(modifiedValue, customerFromStore.Collection);

            customerFromStore.Collection = null;

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
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

    private class CollectionCustomerContext<TCollection>(DbContextOptions dbContextOptions, Action<ModelBuilder> onModelBuilder = null)
        : DbContext(dbContextOptions)
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public DbSet<CustomerWithCollection<TCollection>> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => onModelBuilder?.Invoke(modelBuilder);
    }

    [ConditionalFact]
    public async Task Can_read_with_find_with_resource_id_async()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextWithResourceId>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        const int pk1 = 1;
        const int pk2 = 2;

        var customer = new CustomerWithResourceId
        {
            id = "42",
            Name = "Theon",
            PartitionKey1 = pk1,
            PartitionKey2 = 3.15m
        };

        await using (var context = contextFactory.CreateContext())
        {
            Assert.Null(
                context.Model.FindEntityType(typeof(CustomerWithResourceId))!
                    .FindProperty(CosmosJsonIdConvention.DefaultIdPropertyName));

            await context.AddAsync(customer);
            await context.AddAsync(
                new CustomerWithResourceId
                {
                    id = "42",
                    Name = "Theon Twin",
                    PartitionKey1 = pk2,
                    PartitionKey2 = 3.15m
                });

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<CustomerWithResourceId>()
                .FindAsync(pk1, 3.15m, "42");

            Assert.Equal("42", customerFromStore.id);
            Assert.Equal("Theon", customerFromStore.Name);
            Assert.Equal(pk1, customerFromStore.PartitionKey1);
            Assert.Equal(3.15m, customerFromStore.PartitionKey2);
            AssertSql(context, """ReadItem([1.0,3.15], 42)""");

            customerFromStore.Name = "Theon Greyjoy";

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<CustomerWithResourceId>()
                .WithPartitionKey(pk1, 3.15m)
                .SingleAsync();

            Assert.Equal("42", customerFromStore.id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            Assert.Equal(pk1, customerFromStore.PartitionKey1);
            Assert.Equal(3.15m, customerFromStore.PartitionKey2);
        }
    }

    [ConditionalFact]
    public async Task Find_with_empty_resource_id_throws()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextWithResourceId>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        using (var context = contextFactory.CreateContext())
        {
            context.Database.EnsureCreated();

            Assert.Equal(
                CosmosStrings.InvalidResourceId,
                Assert.Throws<InvalidOperationException>(() => context.Set<CustomerWithResourceId>().Find(1, 3.15m, "")).Message);
        }
    }

    [ConditionalFact]
    public async Task Can_read_with_find_with_partition_key_and_value_generator_async()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextCustomValueGenerator>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)),
            addServices: s => s.AddSingleton<IJsonIdDefinitionFactory, CustomJsonIdDefinitionFactory>());

        const int pk1 = 1;
        const int pk2 = 2;

        var customer = new Customer
        {
            Id = 42,
            Name = "Theon",
            PartitionKey1 = pk1,
            PartitionKey2 = "One",
            PartitionKey3 = true
        };

        await using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);
            await context.AddAsync(
                new Customer
                {
                    Id = 42,
                    Name = "Theon Twin",
                    PartitionKey1 = pk2,
                    PartitionKey2 = "Two",
                    PartitionKey3 = false
                });

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>()
                .FindAsync(pk1, 42, "One", true);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            Assert.Equal(pk1, customerFromStore.PartitionKey1);
            Assert.Equal("One", customerFromStore.PartitionKey2);
            Assert.True(customerFromStore.PartitionKey3);

            customerFromStore.Name = "Theon Greyjoy";

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>()
                .WithPartitionKey(pk1, "One", true)
                .SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            Assert.Equal(pk1, customerFromStore.PartitionKey1);
            Assert.Equal("One", customerFromStore.PartitionKey2);
            Assert.True(customerFromStore.PartitionKey3);
        }
    }

    [ConditionalFact]
    public async Task Can_read_with_find_with_partition_key_without_value_generator()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextNoValueGenerator>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        const int pk1 = 1;

        var customer = new Customer
        {
            Id = 42,
            Name = "Theon",
            PartitionKey1 = pk1,
            PartitionKey2 = "One",
            PartitionKey3 = true
        };

        using (var context = contextFactory.CreateContext())
        {
            var customerEntry = context.Entry(customer);
            customerEntry.Property(CosmosJsonIdConvention.DefaultIdPropertyName).CurrentValue = "42";
            customerEntry.State = EntityState.Added;

            context.SaveChanges();
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<Customer>()
                .Find(pk1, "One", true, 42);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            Assert.Equal(pk1, customerFromStore.PartitionKey1);
            Assert.Equal("One", customerFromStore.PartitionKey2);
            Assert.True(customerFromStore.PartitionKey3);
            AssertSql(
                context,
                """
ReadItem([1.0,"One",true], 42)
""");

            customerFromStore.Name = "Theon Greyjoy";

            context.SaveChanges();
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<Customer>()
                .WithPartitionKey(pk1, "One", true)
                .Single();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);
            Assert.Equal(pk1, customerFromStore.PartitionKey1);
            Assert.Equal("One", customerFromStore.PartitionKey2);
            Assert.True(customerFromStore.PartitionKey3);
        }
    }

    [ConditionalFact]
    public async Task Can_read_with_find_with_partition_key_not_part_of_primary_key()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextNonPrimaryKey>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer
        {
            Id = 42,
            Name = "Theon",
            PartitionKey1 = 1,
            PartitionKey2 = "One",
            PartitionKey3 = true
        };

        using (var context = contextFactory.CreateContext())
        {
            await context.Database.EnsureCreatedAsync();

            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<Customer>().Find(42);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            AssertSql(context, """ReadItem(None, 42)""");
        }
    }

    [ConditionalFact]
    public async Task Can_read_with_find_without_partition_key()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextEntityWithNoPartitionKey>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new CustomerNoPartitionKey { Id = 42, Name = "Theon" };

        await using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<CustomerNoPartitionKey>().Find(42);

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            AssertSql(context, @"ReadItem(None, 42)");
        }
    }

    [ConditionalFact]
    public async Task Can_read_with_find_with_PK_partition_key()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextPrimaryKey>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        var customer = new CustomerGuid { Id = Guid.NewGuid(), Name = "Theon" };

        await using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<CustomerGuid>().Find(customer.Id);

            Assert.Equal(customer.Id, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);
            AssertSql(context, @$"ReadItem([""{customer.Id}""], {customer.Id})");
        }
    }

    [ConditionalFact]
    public async Task Can_read_with_find_with_PK_resource_id()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextWithPrimaryKeyResourceId>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        var customer = new CustomerWithResourceId { id = "42", Name = "Theon" };

        await using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        await using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<CustomerWithResourceId>().Find("42");

            Assert.Equal("42", customerFromStore.id);
            Assert.Equal("Theon", customerFromStore.Name);
            AssertSql(
                context,
                """
@p='42'

SELECT VALUE c
FROM root c
WHERE (c["id"] = @p)
OFFSET 0 LIMIT 1
""");
        }
    }

    private class PartitionKeyContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.HasPartitionKey(
                        c => new
                        {
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.PartitionKey3
                        });
                    cb.HasKey(
                        c => new
                        {
                            c.Id,
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.PartitionKey3
                        });
                });
    }

    private class PartitionKeyContextEntityWithNoPartitionKey(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerNoPartitionKey>();
    }

    private class PartitionKeyContextCustomValueGenerator(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDiscriminatorInJsonIds();

            modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.HasShadowId();

                    cb.HasPartitionKey(
                        c => new
                        {
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.PartitionKey3
                        });
                    cb.HasKey(
                        c => new
                        {
                            c.PartitionKey1,
                            c.Id,
                            c.PartitionKey2,
                            c.PartitionKey3
                        });
                });
        }
    }

    private class PartitionKeyContextNoValueGenerator(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.HasPartitionKey(
                        c => new
                        {
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.PartitionKey3
                        });
                    cb.HasKey(
                        c => new
                        {
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.PartitionKey3,
                            c.Id
                        });
                });
    }

    private class PartitionKeyContextNonPrimaryKey(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>();
    }

    private class PartitionKeyContextPrimaryKey(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerGuid>(
                cb =>
                {
                    cb.Property(c => c.Id).ToJsonProperty("id");
                    cb.HasPartitionKey(c => c.Id);
                });
    }

    private class PartitionKeyContextWithPrimaryKeyResourceId(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerWithResourceId>(
                cb =>
                {
                    cb.HasPartitionKey(c => new { c.PartitionKey1, c.PartitionKey2 });
                    cb.Property(c => c.id).HasConversion<string>();
                    cb.HasKey(c => new { c.id });
                });
    }

    private class PartitionKeyContextWithResourceId(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<CustomerWithResourceId>(
                cb =>
                {
                    cb.HasPartitionKey(c => new { c.PartitionKey1, c.PartitionKey2 });
                    cb.HasKey(
                        c => new
                        {
                            c.PartitionKey1,
                            c.PartitionKey2,
                            c.id
                        });
                });
    }

    [ConditionalFact]
    public async Task Can_use_detached_entities_without_discriminators()
    {
        var contextFactory = await InitializeAsync<NoDiscriminatorCustomerContext>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            (await context.AddAsync(customer)).State = EntityState.Modified;

            customer.Name = "Theon Greyjoy";

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<Customer>().AsNoTracking().Single();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon Greyjoy", customerFromStore.Name);

            (await context.AddAsync(customer)).State = EntityState.Deleted;

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(await context.Set<Customer>().ToListAsync());
        }
    }

    private class NoDiscriminatorCustomerContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>().HasNoDiscriminator();
    }

    [ConditionalFact]
    public async Task Can_update_unmapped_properties()
    {
        var contextFactory = await InitializeAsync<ExtraCustomerContext>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = contextFactory.CreateContext())
        {
            var entry = context.Add(customer);
            entry.Property<string>("EMail").CurrentValue = "theon.g@winterfell.com";

            context.SaveChanges();
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = context.Set<Customer>().Single();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Equal("Theon", customerFromStore.Name);

            customerFromStore.Name = "Theon Greyjoy";

            context.SaveChanges();
        }

        using (var context = contextFactory.CreateContext())
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

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(context.Set<Customer>().ToList());
        }
    }

    private class ExtraCustomerContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>().Property<string>("EMail").ToJsonProperty("e-mail");
    }

    [ConditionalFact]
    public async Task Can_use_non_persisted_properties()
    {
        var contextFactory = await InitializeAsync<UnmappedCustomerContext>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var customer = new Customer { Id = 42, Name = "Theon" };

        using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(customer);

            await context.SaveChangesAsync();
            Assert.Equal("Theon", customer.Name);
        }

        using (var context = contextFactory.CreateContext())
        {
            var customerFromStore = await context.Set<Customer>().SingleAsync();

            Assert.Equal(42, customerFromStore.Id);
            Assert.Null(customerFromStore.Name);

            customerFromStore.Name = "Theon Greyjoy";

            Assert.Equal(0, await context.SaveChangesAsync());
        }
    }

    private class UnmappedCustomerContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Customer>().Property(c => c.Name).ToJsonProperty("");
    }

    [ConditionalFact(Skip = "Fails only on C.I. See #33402")]
    public async Task Add_update_delete_query_throws_if_no_container()
    {
        await using var testDatabase = await CosmosTestStore.CreateInitializedAsync("EndToEndEmpty");

        var options = new DbContextOptionsBuilder<EndToEndEmptyContext>()
            .UseCosmos(testDatabase.ConnectionString, "EndToEndEmpty")
            .Options;

        var customer = new Customer { Id = 42, Name = "Theon" };
        using (var context = new EndToEndEmptyContext(options))
        {
            await context.AddAsync(customer);

            Assert.StartsWith(
                "Response status code does not indicate success: NotFound (404); Substatus: 0",
                (await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())).InnerException!.Message);
        }

        using (var context = new EndToEndEmptyContext(options))
        {
            (await context.AddAsync(customer)).State = EntityState.Modified;

            Assert.StartsWith(
                "Response status code does not indicate success: NotFound (404); Substatus: 0",
                (await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())).InnerException!.Message);
        }

        using (var context = new EndToEndEmptyContext(options))
        {
            (await context.AddAsync(customer)).State = EntityState.Deleted;

            Assert.StartsWith(
                "Response status code does not indicate success: NotFound (404); Substatus: 0",
                (await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())).InnerException!.Message);
        }

        using (var context = new EndToEndEmptyContext(options))
        {
            Assert.StartsWith(
                "Response status code does not indicate success: NotFound (404); Substatus: 0",
                (await Assert.ThrowsAsync<CosmosException>(() => context.Set<Customer>().SingleAsync())).Message);
        }
    }

    private class EndToEndEmptyContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Customer> Customers
            => Set<Customer>();
    }

    [ConditionalFact]
    public async Task Using_a_conflicting_incompatible_id_throws()
    {
        var contextFactory = await InitializeAsync<PartitionKeyContextPrimaryKey>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)));

        using var context = contextFactory.CreateContext();

        await Assert.ThrowsAnyAsync<Exception>(
            async () =>
            {
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

    public class ConflictingIncompatibleIdContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ConflictingIncompatibleId>();
    }

    [ConditionalFact]
    public async Task Can_add_update_delete_end_to_end_with_conflicting_id()
    {
        var contextFactory = await InitializeAsync<ConflictingIdContext>(
            shouldLogCategory: _ => true,
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        var entity = new ConflictingId { id = "42", Name = "Theon" };

        using (var context = contextFactory.CreateContext())
        {
            await context.AddAsync(entity);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var entityFromStore = context.Set<ConflictingId>().Single();

            Assert.Equal("42", entityFromStore.id);
            Assert.Equal("Theon", entityFromStore.Name);
        }

        using (var context = contextFactory.CreateContext())
        {
            entity.Name = "Theon Greyjoy";

            context.Update(entity);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            var entityFromStore = context.Set<ConflictingId>().Single();

            Assert.Equal("42", entityFromStore.id);
            Assert.Equal("Theon Greyjoy", entityFromStore.Name);
        }

        using (var context = contextFactory.CreateContext())
        {
            context.Remove(entity);

            await context.SaveChangesAsync();
        }

        using (var context = contextFactory.CreateContext())
        {
            Assert.Empty(context.Set<ConflictingId>().ToList());
        }
    }

    private class ConflictingId
    {
        public string id { get; set; }
        public string Name { get; set; }
    }

    public class ConflictingIdContext(DbContextOptions dbContextOptions) : DbContext(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<ConflictingId>();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Can_have_non_string_property_named_Discriminator(bool useDiscriminator)
    {
        var contextFactory = await InitializeAsync<DbContext>(
            shouldLogCategory: _ => true,
            onModelCreating: b =>
            {
                if (useDiscriminator)
                {
                    b.Entity<NonStringDiscriminator>()
                        .HasDiscriminator(m => m.Discriminator)
                        .HasValue(Discriminator.Base);
                }
                else
                {
                    b.Entity<NonStringDiscriminator>();
                }
            },
            onConfiguring: o => o.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported, CosmosEventId.NoPartitionKeyDefined)));

        using var context = contextFactory.CreateContext();

        var entry = await context.AddAsync(new NonStringDiscriminator { Id = 1 });
        await context.SaveChangesAsync();

        var document = entry.Property<JObject>("__jObject").CurrentValue;
        Assert.NotNull(document);
        Assert.Equal("0", document["Discriminator"]);

        var baseEntity = await context.Set<NonStringDiscriminator>().OrderBy(e => e.Id).FirstOrDefaultAsync();
        Assert.NotNull(baseEntity);

        AssertSql(
            context,
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
OFFSET 0 LIMIT 1
""");

        ListLoggerFactory.Clear();
        Assert.Equal(
            baseEntity, await context.Set<NonStringDiscriminator>()
                .Where(e => e.Discriminator == Discriminator.Base).OrderBy(e => e.Id).FirstOrDefaultAsync());

            AssertSql(
                context,
                """
SELECT VALUE c
FROM root c
WHERE (c["Discriminator"] = 0)
ORDER BY c["Id"]
OFFSET 0 LIMIT 1
""");

        ListLoggerFactory.Clear();
        Assert.Equal(
            baseEntity, await context.Set<NonStringDiscriminator>()
                .Where(e => e.GetType() == typeof(NonStringDiscriminator)).OrderBy(e => e.Id).FirstOrDefaultAsync());

            AssertSql(
                context,
                """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
OFFSET 0 LIMIT 1
""");

        ListLoggerFactory.Clear();
        Assert.Equal(
            baseEntity, await context.Set<NonStringDiscriminator>()
                .Where(e => e is NonStringDiscriminator).OrderBy(e => e.Id).FirstOrDefaultAsync());

        AssertSql(
            context,
            """
SELECT VALUE c
FROM root c
ORDER BY c["Id"]
OFFSET 0 LIMIT 1
""");
    }

    private class NonStringDiscriminator
    {
        public int Id { get; set; }
        public Discriminator Discriminator { get; set; }
    }

    private enum Discriminator
    {
        Base,
        Derived
    }

    private void AssertSql(DbContext context, params string[] expected)
    {
        var logger = (TestSqlLoggerFactory)context.GetService<ILoggerFactory>();
        logger.AssertBaseline(expected);
    }

    protected ListLoggerFactory LoggerFactory { get; }

    protected override string StoreName
        => nameof(EndToEndCosmosTest);

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected ContextFactory<DbContext> ContextFactory { get; private set; }

    protected async Task InitializeAsync(
        Action<ModelBuilder> onModelCreating,
        Func<DbContextOptionsBuilder, Task> onConfiguring = null,
        Func<DbContext, Task> seed = null,
        bool sensitiveLogEnabled = true)
        => ContextFactory = await InitializeAsync(
            onModelCreating,
            seed: seed,
            shouldLogCategory: _ => true,
            onConfiguring: options =>
            {
                options.ConfigureWarnings(w => w.Log(CosmosEventId.SyncNotSupported)).EnableSensitiveDataLogging(sensitiveLogEnabled);
                onConfiguring?.Invoke(options);
            }
        );
}
