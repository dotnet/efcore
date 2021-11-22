// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class ReloadTest
{
    public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { true }, new object[] { false } };

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Entity_reference_can_be_reloaded(bool async)
    {
        await using var testDatabase = CosmosTestStore.CreateInitialized("ReloadTest");

        using var context = new ReloadTestContext(testDatabase);
        await context.Database.EnsureCreatedAsync();

        var entry = context.Add(new Item { Id = 1337 });

        await context.SaveChangesAsync();

        var itemJson = entry.Property<JObject>("__jObject").CurrentValue;
        itemJson["unmapped"] = 2;

        if (async)
        {
            await entry.ReloadAsync();
        }
        else
        {
            entry.Reload();
        }

        itemJson = entry.Property<JObject>("__jObject").CurrentValue;
        Assert.Null(itemJson["unmapped"]);
    }

    public class ReloadTestContext : DbContext
    {
        private readonly string _connectionUri;
        private readonly string _authToken;
        private readonly string _name;

        public ReloadTestContext(CosmosTestStore testStore)
        {
            _connectionUri = testStore.ConnectionUri;
            _authToken = testStore.AuthToken;
            _name = testStore.Name;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseCosmos(
                    _connectionUri,
                    _authToken,
                    _name,
                    b => b.ApplyConfiguration());

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<Item> Items { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
    }
}
