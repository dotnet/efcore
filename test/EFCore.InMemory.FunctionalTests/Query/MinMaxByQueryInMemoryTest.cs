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
