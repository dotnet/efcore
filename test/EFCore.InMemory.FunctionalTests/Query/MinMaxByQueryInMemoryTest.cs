// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQueryInMemoryTest(NonSharedFixture fixture, ITestOutputHelper output) : MinMaxByQueryTestBase(fixture, output)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    [Theory]
    [InlineData(32)]
    [InlineData(128)]
    public async Task Minimum_on_last_row_evaluates_source_once(int count)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Item>().Property(p => p.Id).ValueGeneratedNever(),
            seed: c =>
            {
                c.AddRange(Enumerable.Range(1, count).Select(id => new Item { Id = id, Key = count - id }));
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var counter = new Counter();
        var winner = context.Set<Item>().Where(i => counter.Visit(i.Id)).OrderBy(i => i.Id).MinBy(i => i.Key);
        Assert.Equal(count, winner!.Id);
        Assert.Equal(count, counter.Visits);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Nullable_key_selector_is_evaluated_once_per_item(bool maximum)
    {
        var rows = Enumerable.Range(1, 4).Select(id => new Item { Id = id }).ToArray();
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Item>().Property(i => i.Id).ValueGeneratedNever(),
            seed: c =>
            {
                c.AddRange(rows);
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var reference = new KeyCounter();
        var expected = maximum ? rows.MaxBy(i => reference.GetKey(i.Id)) : rows.MinBy(i => reference.GetKey(i.Id));
        var actual = new KeyCounter();
        var items = context.Set<Item>().OrderBy(i => i.Id);
        var winner = maximum ? items.MaxBy(i => actual.GetKey(i.Id)) : items.MinBy(i => actual.GetKey(i.Id));

        Assert.Equal((expected!.Id, rows.Length), (winner!.Id, actual.Calls));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Non_nullable_key_ties_use_the_original_source_ordering(bool maximum)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Item>().Property(i => i.Id).ValueGeneratedNever(),
            seed: c =>
            {
                c.AddRange(new Item { Id = 1 }, new Item { Id = 2 });
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var counter = new KeyCounter();
        var ordered = context.Set<Item>().OrderByDescending(i => counter.GetKey(i.Id));
        var winner = maximum ? ordered.MaxBy(i => i.Scenario) : ordered.MinBy(i => i.Scenario);
        Assert.Equal((2, 2), (winner!.Id, counter.Calls));
    }

    private sealed class KeyCounter
    {
        private readonly HashSet<int> _seen = [];
        public int Calls { get; private set; }

        public int? GetKey(int id)
        {
            Calls++;
            return _seen.Add(id) ? id : -id;
        }
    }

    private sealed class Counter
    {
        public int Visits { get; private set; }

        public bool Visit(int id)
        {
            Visits++;
            return id > 0;
        }
    }
}
