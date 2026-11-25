// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQuerySqliteTest(NonSharedFixture fixture, ITestOutputHelper output) : MinMaxByQueryTestBase(fixture, output)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    public override async Task Nullable_keys_with_global_query_filter(bool async, bool maximum)
    {
        await base.Nullable_keys_with_global_query_filter(async, maximum);

        AssertSql(
            $$"""
SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
FROM "Item" AS "i"
WHERE "i"."Scenario" = 1 AND ("i"."Key" IS (
    SELECT "i0"."Key"
    FROM "Item" AS "i0"
    WHERE "i0"."Scenario" = 1 AND "i0"."Key" IS NOT NULL
    ORDER BY "i0"."Key"{{(maximum ? " DESC" : "")}}
    LIMIT 1))
ORDER BY "i"."Id"
LIMIT 1
""");
    }

    public override async Task Non_nullable_keys_preserve_source_order(bool maximum)
    {
        await base.Non_nullable_keys_preserve_source_order(maximum);

        AssertSql(
            $$"""
SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
FROM "Item" AS "i"
ORDER BY "i"."Scenario"{{(maximum ? " DESC" : "")}}, "i"."Id" DESC
LIMIT 1
""");
    }

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

SELECT "i0"."Id", "i0"."Key", "i0"."Scenario", "i0"."StringKey"
FROM (
    SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
    FROM "Item" AS "i"
    WHERE "i"."Scenario" = @scenario
    ORDER BY "i"."Scenario", "i"."Id" DESC
    LIMIT @p2 OFFSET @p
) AS "i0"
ORDER BY "i0"."{{key}}" IS NULL, "i0"."{{key}}"{{(maximum ? " DESC" : "")}}, "i0"."Scenario", "i0"."Id" DESC
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
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Sampled_source_is_evaluated_once(bool async, bool useQueryFilter)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b =>
            {
                var entity = b.Entity<Item>();
                entity.Property(i => i.Id).ValueGeneratedNever();
                if (useQueryFilter)
                {
                    entity.HasQueryFilter(i => i.Id == 1 || EF.Functions.Random() > 0.5);
                }
            },
            seed: c =>
            {
                c.AddRange(Enumerable.Range(1, 16).Select(id => new Item { Id = id, Key = id }));
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var rows = context.Set<Item>();
        var sample = (useQueryFilter ? rows.OrderByDescending(i => i.Id) : rows.OrderBy(i => EF.Functions.Random())).Take(1);
        var source = useQueryFilter ? rows.AsQueryable() : sample;
        var winner = async ? await source.MinByAsync(i => i.Key) : source.MinBy(i => i.Key);
        Assert.NotNull(winner);
        Assert.Equal(winner.Id, winner.Key);
        var sql = ((TestSqlLoggerFactory)ListLoggerFactory).Sql;
        Assert.Equal(2, sql.Split("FROM \"Item\"", StringSplitOptions.None).Length);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Regular_ordering_without_extremal_lookup(bool explicitOrdering)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Item>().Property(i => i.Id).ValueGeneratedNever(),
            addServices: explicitOrdering ? null : s => s.AddScoped<IQueryableMethodTranslatingExpressionVisitorFactory,
                RelationalQueryableMethodTranslatingExpressionVisitorFactory>(),
            seed: c =>
            {
                c.AddRange(new Item { Id = 1 }, new Item { Id = 2, Key = 3 });
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var items = context.Set<Item>();
        var winner = explicitOrdering
            ? await items.OrderBy(i => i.Key == null).ThenBy(i => i.Key).FirstAsync()
            : await items.MinByAsync(i => i.Key);
        Assert.Equal(2, winner!.Id);
        AssertSql(
            """
SELECT "i"."Id", "i"."Key", "i"."Scenario", "i"."StringKey"
FROM "Item" AS "i"
ORDER BY "i"."Key" IS NULL, "i"."Key"
LIMIT 1
""");
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
