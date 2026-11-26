// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocAdvancedMappingsQuerySqliteTest(NonSharedFixture fixture) : AdHocAdvancedMappingsQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsBetween_with_converted_TimeOnly_parameters(bool async)
    {
        var factory = await InitializeTimes();
        await using var db = factory.CreateDbContext();
        var start = new TimeOnly(9, 0);
        var end = new TimeOnly(17, 0);
        var query = db.Set<TimeRow>().Where(t => t.ConvertedTime.IsBetween(start, end)).OrderBy(t => t.Id).Select(t => t.Id);
        foreach (var wraps in new[] { false, true })
        {
            ClearLog();
            Assert.Equal(wraps ? new[] { 1, 2, 4, 5 } : [3], async ? await query.ToArrayAsync() : query.ToArray());
            AssertSql(
                $$"""
@start='{{start.Ticks}}'
@end='{{end.Ticks}}'

SELECT "t"."Id"
FROM "TimeRow" AS "t"
WHERE (@start <= @end AND "t"."ConvertedTime" >= @start AND "t"."ConvertedTime" < @end) OR (@start > @end AND ("t"."ConvertedTime" >= @start OR "t"."ConvertedTime" < @end))
ORDER BY "t"."Id"
""");
            (start, end) = (end, start);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsBetween_with_native_receiver_and_converted_end_is_not_translated(bool async)
    {
        var factory = await InitializeTimes();
        await using var db = factory.CreateDbContext();
        var start = new TimeOnly(9, 0);
        var query = db.Set<TimeRow>().Where(t => t.NativeTime.IsBetween(start, t.ConvertedTime)).OrderBy(t => t.Id).Select(t => t.Id);
        var exception = async
            ? await Assert.ThrowsAsync<InvalidOperationException>(() => query.ToArrayAsync())
            : Assert.Throws<InvalidOperationException>(() => query.ToArray());
        Assert.Contains(nameof(TimeOnly.IsBetween), exception.Message);
        AssertSql();

        var projection = db.Set<TimeRow>().OrderBy(t => t.Id).Select(t => t.NativeTime.IsBetween(start, t.ConvertedTime));
        Assert.Equal(new[] { true, false, true, false, false }, async ? await projection.ToArrayAsync() : projection.ToArray());
        AssertSql(
            """
SELECT "t"."NativeTime", "t"."ConvertedTime"
FROM "TimeRow" AS "t"
ORDER BY "t"."Id"
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task IsBetween_with_short_fractional_text(bool async)
    {
        var factory = await InitializeTimes();
        await using var db = factory.CreateDbContext();
        await db.Database.ExecuteSqlRawAsync(
            """
UPDATE TimeRow SET NativeTime = CASE Id
    WHEN 1 THEN '15:30:10.0999999'
    WHEN 2 THEN '15:30:10.1'
    WHEN 3 THEN '15:30:10.100'
    WHEN 4 THEN '15:30:10.1000000'
    WHEN 5 THEN '15:30:10.1000001'
END;
""");
        var boundary = new TimeOnly(15, 30, 10, 100);
        var query = db.Set<TimeRow>().OrderBy(t => t.Id).Select(t => new
        {
            AtStart = t.NativeTime.IsBetween(boundary, new TimeOnly(16, 0)),
            AtEnd = t.NativeTime.IsBetween(new TimeOnly(15, 0), boundary),
            AtConstantStart = t.NativeTime.IsBetween(new TimeOnly(15, 30, 10, 100), new TimeOnly(16, 0)),
            WrappedWithColumnStart = new TimeOnly(20, 0).IsBetween(t.NativeTime, boundary),
            WrappedWithColumnEnd = new TimeOnly(20, 0).IsBetween(boundary, t.NativeTime)
        });
        var rows = async ? await query.ToListAsync() : query.ToList();
        Assert.Equal(new[] { false, true, true, true, true }, rows.Select(t => t.AtStart));
        Assert.Equal(new[] { true, false, false, false, false }, rows.Select(t => t.AtEnd));
        Assert.Equal(rows.Select(t => t.AtStart), rows.Select(t => t.AtConstantStart));
        Assert.Equal(new[] { false, false, false, false, true }, rows.Select(t => t.WrappedWithColumnStart));
        Assert.Equal(new[] { true, false, false, false, false }, rows.Select(t => t.WrappedWithColumnEnd));
    }

    [Fact]
    public async Task IsBetween_with_equivalent_native_mappings()
    {
        var factory = await InitializeTimes();
        await using var db = factory.CreateDbContext();
        var ids = await db.Set<TimeRow>()
            .Where(t => t.NativeTime.IsBetween(new TimeOnly(9, 0), t.NativeEnd))
            .OrderBy(t => t.Id).Select(t => t.Id).ToArrayAsync();
        Assert.Equal(new[] { 1, 3 }, ids);
    }

    private Task<ContextFactory<DbContext>> InitializeTimes()
        => InitializeNonSharedTest<DbContext>(
            b => b.Entity<TimeRow>(t =>
            {
                t.Property(r => r.NativeTime).HasColumnType("text");
                t.Property(r => r.ConvertedTime).HasConversion(v => v.Ticks, ticks => new TimeOnly(ticks));
            }),
            seed: async db =>
            {
                db.AddRange(
                    new TimeRow { Id = 1, NativeTime = new(12, 0), ConvertedTime = new(17, 0) },
                    new TimeRow { Id = 2, NativeTime = new(20, 0), ConvertedTime = new(17, 0) },
                    new TimeRow { Id = 3, NativeTime = new(9, 0), ConvertedTime = new(10, 0) },
                    new TimeRow { Id = 4, NativeTime = new(17, 0), ConvertedTime = new(17, 0) },
                    new TimeRow { Id = 5, NativeTime = new(8, 0), ConvertedTime = new(8, 0) });
                await db.SaveChangesAsync();
            },
            shouldLogCategory: category => category == DbLoggerCategory.Database.Command.Name);

    private class TimeRow
    {
        public int Id { get; set; }
        public TimeOnly NativeTime { get; set; }
        public TimeOnly NativeEnd { get; set; } = new(17, 0);
        public TimeOnly ConvertedTime { get; set; }
    }
}
