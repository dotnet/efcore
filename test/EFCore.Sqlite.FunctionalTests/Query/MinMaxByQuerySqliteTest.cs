// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQuerySqliteTest(NonSharedFixture fixture, ITestOutputHelper output) : MinMaxByQueryTestBase(fixture, output)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Nullable_keys(bool async, bool maximum, bool stringKey, int scenario)
    {
        await base.Nullable_keys(async, maximum, stringKey, scenario);

        var key = stringKey ? "StringKey" : "Key";
        AssertSql(
            scenario == 4
                ? $$"""
@scenario='4'
@p2='2'
@p='1'

SELECT "i2"."Id", "i2"."Key", "i2"."Scenario", "i2"."StringKey"
FROM (
    SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
    FROM "Item" AS "i"
    WHERE "i"."Scenario" = @scenario
    ORDER BY "i"."Scenario", "i"."Id" DESC
    LIMIT @p2 OFFSET @p
) AS "i2"
WHERE ("i2"."{{key}}" IS (
    SELECT "i1"."{{key}}"
    FROM (
        SELECT "i0"."{{key}}"
        FROM "Item" AS "i0"
        WHERE "i0"."Scenario" = @scenario
        ORDER BY "i0"."Scenario", "i0"."Id" DESC
        LIMIT @p2 OFFSET @p
    ) AS "i1"
    WHERE "i1"."{{key}}" IS NOT NULL
    ORDER BY "i1"."{{key}}"{{(maximum ? " DESC" : "")}}
    LIMIT 1))
ORDER BY "i2"."Scenario", "i2"."Id" DESC
LIMIT 1
"""
                : $$"""
@scenario='{{scenario}}'

SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
FROM "Item" AS "i"
WHERE "i"."Scenario" = @scenario AND ("i"."{{key}}" IS (
    SELECT "i0"."{{key}}"
    FROM "Item" AS "i0"
    WHERE "i0"."Scenario" = @scenario AND "i0"."{{key}}" IS NOT NULL
    ORDER BY "i0"."{{key}}"{{(maximum ? " DESC" : "")}}
    LIMIT 1))
ORDER BY "i"."Id"{{(scenario == 5 ? " DESC" : "")}}
LIMIT 1
""");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Candidate_sources_differ(bool async, bool differentKeySource)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Item>().Property(i => i.Id).ValueGeneratedNever(),
            seed: c =>
            {
                c.AddRange(
                    new Item { Id = 1, Scenario = 0, Key = 1 },
                    new Item { Id = 2, Scenario = 0 },
                    new Item { Id = 3, Scenario = 1, Key = 2 },
                    new Item { Id = 4, Scenario = 1 });
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        Expression<Func<MinMaxByContext, Item?>> query = c => c.Set<Item>()
            .Where(i => i.Scenario == 0).OrderBy(i => i.Id).Where(i => i.Key != null)
            .Where(i => i.Key == c.Set<Item>().Where(j => j.Scenario == (differentKeySource ? 1 : 0))
                .OrderBy(j => j.Id).Where(j => j.Key != null).OrderBy(j => j.Key).Select(j => j.Key).FirstOrDefault())
            .Take(1)
            .Concat(c.Set<Item>().Where(i => i.Scenario == (differentKeySource ? 0 : 1))
                .OrderBy(i => i.Id).Where(i => i.Key == null).Take(1))
            .OrderBy(i => i.Key == null).FirstOrDefault();
        var actual = async ? await EF.CompileAsyncQuery(query)(context) : EF.CompileQuery(query)(context);
        Assert.Equal(differentKeySource ? 2 : 1, actual!.Id);
        Assert.Equal(0, actual.Scenario);
        Assert.Contains("UNION ALL", ((TestSqlLoggerFactory)ListLoggerFactory).Sql);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task All_null_keys_with_relational_nulls(bool async, bool maximum)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onConfiguring: b => new SqliteDbContextOptionsBuilder(b).UseRelationalNulls(),
            onModelCreating: b => b.Entity<Item>().Property(i => i.Id).ValueGeneratedNever(),
            seed: c =>
            {
                c.AddRange(new Item { Id = 1 }, new Item { Id = 2 });
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var query = context.Set<Item>().OrderBy(i => i.Id);
        var actual = maximum
            ? async ? await query.MaxByAsync(i => i.Key) : query.MaxBy(i => i.Key)
            : async ? await query.MinByAsync(i => i.Key) : query.MinBy(i => i.Key);
        Assert.Equal(1, actual!.Id);
        Assert.Null(actual.Key);
        AssertSql(
            $$"""
SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
FROM "Item" AS "i"
WHERE ("i"."Key" IS (
    SELECT "i0"."Key"
    FROM "Item" AS "i0"
    WHERE "i0"."Key" IS NOT NULL
    ORDER BY "i0"."Key"{{(maximum ? " DESC" : "")}}
    LIMIT 1))
ORDER BY "i"."Id"
LIMIT 1
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Volatile_source_is_not_rewritten(bool async)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Item>().Property(i => i.Id).ValueGeneratedNever(),
            seed: c =>
            {
                c.AddRange(new Item { Id = 1, Key = 1 }, new Item { Id = 2 });
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var query = context.Set<Item>().Where(i => EF.Functions.Random() > -1).OrderBy(i => i.Id);
        var actual = async ? await query.MinByAsync(i => i.Key) : query.MinBy(i => i.Key);
        Assert.Equal(1, actual!.Id);
        Assert.Equal(1, actual.Key);
        Assert.Contains("UNION ALL", ((TestSqlLoggerFactory)ListLoggerFactory).Sql);
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
