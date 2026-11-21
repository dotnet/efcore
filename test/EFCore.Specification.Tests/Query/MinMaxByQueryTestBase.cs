// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class MinMaxByQueryTestBase : NonSharedModelTestBase, IClassFixture<NonSharedFixture>
{
    protected MinMaxByQueryTestBase(NonSharedFixture fixture, ITestOutputHelper output)
        : base(fixture)
        => TestOutputHelper = output;

    protected override string NonSharedStoreName
        => "MinMaxByQueryTest";

    public static IEnumerable<object[]> NullableKeyData()
    {
        // Mixed keys, all-null keys, an empty table, non-null keys, pagination, ties, and a zero-valued identity.
        foreach (var scenario in new[] { 0, 1, 2, 3, 4, 5, 6 })
        {
            foreach (var maximum in new[] { false, true })
            {
                foreach (var stringKey in new[] { false, true })
                {
                    foreach (var async in new[] { false, true })
                    {
                        yield return [async, maximum, stringKey, scenario];
                    }
                }
            }
        }
    }

    [Theory, MemberData(nameof(NullableKeyData))]
    public virtual async Task Nullable_keys(bool async, bool maximum, bool stringKey, int scenario)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Item>().Property(i => i.Id).ValueGeneratedNever(),
            seed: c =>
            {
                if (scenario != 2)
                {
                    c.AddRange(CreateItems());
                }
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var source = context.Set<Item>().Where(i => i.Scenario == scenario);
        var query = scenario == 4
            ? context.Set<Item>().OrderBy(i => i.Scenario).ThenByDescending(i => i.Id)
                .Where(i => i.Scenario == scenario).Skip(1).Take(2)
            : scenario == 5 ? source.OrderByDescending(i => i.Id) : source.OrderBy(i => i.Id);
        var expectedSource = CreateItems().Where(i => i.Scenario == scenario);
        var data = scenario == 4
            ? expectedSource.OrderByDescending(i => i.Id).Skip(1).Take(2)
            : scenario == 5 ? expectedSource.OrderByDescending(i => i.Id) : expectedSource.OrderBy(i => i.Id);
        Item? expected;
        Item? actual;
        if (stringKey)
        {
            expected = maximum ? data.MaxBy(i => i.StringKey) : data.MinBy(i => i.StringKey);
            actual = maximum
                ? async ? await query.MaxByAsync(i => i.StringKey) : query.MaxBy(i => i.StringKey)
                : async ? await query.MinByAsync(i => i.StringKey) : query.MinBy(i => i.StringKey);
        }
        else
        {
            expected = maximum ? data.MaxBy(i => i.Key) : data.MinBy(i => i.Key);
            actual = maximum
                ? async ? await query.MaxByAsync(i => i.Key) : query.MaxBy(i => i.Key)
                : async ? await query.MinByAsync(i => i.Key) : query.MinBy(i => i.Key);
        }

        Assert.Equal(expected?.Id, actual?.Id);
        Assert.Equal(expected?.Key, actual?.Key);
        Assert.Equal(expected?.StringKey, actual?.StringKey);
    }

    private static Item[] CreateItems()
        =>
        [
            new() { Id = 1, Scenario = 0 },
            new() { Id = 2, Scenario = 0, Key = 1, StringKey = "a" },
            new() { Id = 3, Scenario = 0, Key = 2, StringKey = "b" },
            new() { Id = 4, Scenario = 1 },
            new() { Id = 5, Scenario = 1 },
            new() { Id = 6, Scenario = 3, Key = 1, StringKey = "a" },
            new() { Id = 7, Scenario = 3, Key = 2, StringKey = "b" },
            new() { Id = 8, Scenario = 4 },
            new() { Id = 9, Scenario = 4 },
            new() { Id = 10, Scenario = 4 },
            new() { Id = 11, Scenario = 5, Key = 1, StringKey = "a" },
            new() { Id = 12, Scenario = 5, Key = 1, StringKey = "a" },
            new() { Id = 0, Scenario = 6, Key = 1, StringKey = "a" },
            new() { Id = 13, Scenario = 6, Key = 2, StringKey = "b" }
        ];

    protected class MinMaxByContext(DbContextOptions options) : DbContext(options);

    protected class Item
    {
        public int Id { get; set; }
        public int Scenario { get; set; }
        public int? Key { get; set; }
        public string? StringKey { get; set; }
    }
}
