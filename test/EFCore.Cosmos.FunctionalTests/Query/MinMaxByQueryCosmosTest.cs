// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQueryCosmosTest(NonSharedFixture fixture, ITestOutputHelper output) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Compiled_member_access_with_nullable_key(bool maximum)
    {
        var factory = await InitializeItems();
        await using var context = factory.CreateDbContext();
        ListLoggerFactory.SetTestOutputHelper(output);
        var query = maximum
            ? EF.CompileAsyncQuery((Context c) => c.Set<Item>().MaxBy(i => i.Key)!.Key)
            : EF.CompileAsyncQuery((Context c) => c.Set<Item>().MinBy(i => i.Key)!.Key);

        Assert.Equal(maximum ? 8 : 3, await query(context));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ordered_source_with_default_indexes(bool maximum)
    {
        var factory = await InitializeItems();
        await using var context = factory.CreateDbContext();
        ListLoggerFactory.SetTestOutputHelper(output);
        var ordered = context.Set<Item>().OrderBy(i => i.Id);
        var winner = maximum ? await ordered.MaxByAsync(i => i.Key) : await ordered.MinByAsync(i => i.Key);
        Assert.Equal(maximum ? 2 : 1, winner!.Id);

        // A multi-property ORDER BY requires a composite index on Azure Cosmos DB.
        var sql = Assert.Single(((TestSqlLoggerFactory)ListLoggerFactory).SqlStatements);
        Assert.DoesNotMatch(@"(?m)^ORDER BY [^\r\n]*,", sql);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ordered_nullable_keys_select_the_first_tie(bool maximum)
    {
        var factory = await InitializeItems();
        await using var context = factory.CreateDbContext();
        var items = context.Set<Item>();
        var ordered = items.OrderByDescending(i => i.Id);
        var result = maximum ? await ordered.MaxByAsync(i => i.Key) : await ordered.MinByAsync(i => i.Key);
        Assert.Equal(maximum ? 4 : 3, result!.Id);
        Assert.Same(result, Assert.Single(context.ChangeTracker.Entries<Item>()).Entity);

        var byTwoFields = items.OrderBy(i => i.Group).ThenByDescending(i => i.Id);
        Assert.Equal(result.Id, (maximum ? await byTwoFields.MaxByAsync(i => i.Key) : await byTwoFields.MinByAsync(i => i.Key))!.Id);

        var allNull = items.Where(i => i.Group == 2).OrderByDescending(i => i.Id);
        Assert.Equal(6, (maximum ? await allNull.MaxByAsync(i => i.Key) : await allNull.MinByAsync(i => i.Key))!.Id);
        var empty = items.Where(i => i.Group == 3).OrderBy(i => i.Id);
        Assert.Null(maximum ? await empty.MaxByAsync(i => i.Key) : await empty.MinByAsync(i => i.Key));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ties_preserve_text_and_large_integer_ordering(bool descending)
    {
        var factory = await InitializeItems();
        await using var context = factory.CreateDbContext();
        var tied = context.Set<Item>().Where(i => i.Key == 3);
        var ordered = descending ? tied.OrderByDescending(i => i.Label) : tied.OrderBy(i => i.Label);
        var expected = await ordered.Select(i => i.Id).FirstAsync();
        Assert.Equal(expected, (await ordered.MinByAsync(i => i.Key))!.Id);

        var numeric = descending ? tied.OrderByDescending(i => i.Sequence) : tied.OrderBy(i => i.Sequence);
        Assert.Equal(descending ? 1 : 3, (await numeric.MaxByAsync(i => i.Key))!.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ordered_non_nullable_keys_with_default_indexes(bool maximum)
    {
        var factory = await InitializeItems();
        await using var context = factory.CreateDbContext();
        var ordered = context.Set<Item>().OrderByDescending(i => i.Id);
        var winner = maximum ? await ordered.MaxByAsync(i => i.Group) : await ordered.MinByAsync(i => i.Group);
        Assert.Equal(maximum ? 6 : 4, winner!.Id);
        var sql = Assert.Single(((TestSqlLoggerFactory)ListLoggerFactory).SqlStatements);
        Assert.DoesNotMatch(@"(?m)^ORDER BY [^\r\n]*,", sql);
    }

    private Task<ContextFactory<Context>> InitializeItems()
        => InitializeNonSharedTest<Context>(
            onModelCreating: b => b.Entity<Item>().HasPartitionKey(i => i.Group),
            seed: async c =>
            {
                c.AddRange(new Item { Id = 1, Group = 1, Key = 3, Label = "\uE000", Sequence = 9007199254740993L }, new Item { Id = 2, Group = 1, Key = 8 },
                    new Item { Id = 3, Group = 1, Key = 3, Label = "\U00010000", Sequence = 9007199254740992L }, new Item { Id = 4, Group = 1, Key = 8 },
                    new Item { Id = 5, Group = 2 }, new Item { Id = 6, Group = 2 });
                await c.SaveChangesAsync();
            }, shouldLogCategory: category => category == DbLoggerCategory.Database.Command.Name);

    protected override string NonSharedStoreName
        => "MinMaxByCosmos";

    private class Context(DbContextOptions options) : DbContext(options);

    private class Item
    {
        public int Id { get; set; }
        public int Group { get; set; }
        public int? Key { get; set; }
        public long Sequence { get; set; }
        public string? Label { get; set; }
    }
}
