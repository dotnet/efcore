// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQueryCosmosTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => CosmosTestStoreFactory.Instance;

    [Fact]
    public async Task Nullable_keys()
    {
        var factory = await InitializeNonSharedTest<Context>(
            onModelCreating: b => b.Entity<Item>().HasPartitionKey(i => i.Group),
            seed: async c =>
            {
                c.AddRange(new Item { Id = 1, Group = 1 }, new Item { Id = 2, Group = 1, Key = 8 },
                    new Item { Id = 3, Group = 1, Key = 3 }, new Item { Id = 4, Group = 2 },
                    new Item { Id = 5, Group = 1, Key = 3, Label = "a" });
                await c.SaveChangesAsync();
            });

        await using var context = factory.CreateDbContext();
        var items = context.Set<Item>();
        var minimum = await items.Where(i => i.Group == 1).MinByAsync(i => i.Key);
        Assert.Equal(3, minimum!.Key);
        Assert.Same(minimum, Assert.Single(context.ChangeTracker.Entries<Item>()).Entity);
        Assert.Equal(2, (await items.Where(i => i.Group == 1).MaxByAsync(i => i.Key))!.Id);
        Assert.Equal(4, (await items.Where(i => i.Group == 2).MinByAsync(i => i.Key))!.Id);
        Assert.Null(await items.Where(i => i.Group == 3).MinByAsync(i => i.Key));
        Assert.Null(await items.Where(i => i.Group == 3).MaxByAsync(i => i.Key));
        Assert.Equal(4, (await items.Where(i => i.Group == 2).MaxByAsync(i => i.Key))!.Id);
        Assert.Equal(5, (await items.Where(i => i.Group == 1).MinByAsync(i => i.Label))!.Id);
        Assert.Equal(3, await items.Where(i => i.Group == 1).Select(i => i.Key).MinByAsync(k => k));
        var projection = await items.Where(i => i.Group == 1).Select(i => new { i.Id, i.Label }).MinByAsync(i => i.Label);
        Assert.Equal(5, projection!.Id);

        var compiled = EF.CompileAsyncQuery((Context c, int group) => c.Set<Item>().Where(i => i.Group == group).MinBy(i => i.Key));
        Assert.Equal(3, (await compiled(context, 1))!.Key);
        Assert.Equal(4, (await compiled(context, 2))!.Id);
        Assert.Null(await compiled(context, 3));
    }

    protected override string NonSharedStoreName
        => "MinMaxByCosmos";

    private class Context(DbContextOptions options) : DbContext(options);

    private class Item
    {
        public int Id { get; set; }
        public int Group { get; set; }
        public int? Key { get; set; }
        public string? Label { get; set; }
    }
}
