// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ReloadTest
{
    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    [ConditionalFact]
    public async Task Entity_reference_can_be_reloaded()
    {
        await using var testDatabase = await CosmosTestStore.CreateInitializedAsync("ReloadTest");

        using var context = new ReloadTestContext(testDatabase);
        await context.Database.EnsureCreatedAsync();

        var entry = await context.AddAsync(new Item { Id = 1337 });

        await context.SaveChangesAsync();

        var itemJson = entry.Property<JObject>("__jObject").CurrentValue;
        itemJson["unmapped"] = 2;

        await entry.ReloadAsync();

        itemJson = entry.Property<JObject>("__jObject").CurrentValue;
        Assert.Null(itemJson["unmapped"]);
    }

    public class ReloadTestContext(CosmosTestStore testStore) : DbContext
    {
        private readonly string _connectionUri = testStore.ConnectionUri;
        private readonly string _authToken = testStore.AuthToken;
        private readonly string _name = testStore.Name;

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
