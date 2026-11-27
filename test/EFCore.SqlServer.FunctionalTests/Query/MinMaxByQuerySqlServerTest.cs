// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class MinMaxByQuerySqlServerTest(NonSharedFixture fixture, ITestOutputHelper output) : MinMaxByQueryTestBase(fixture, output)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Nullable_keys_with_global_query_filter(bool async, bool maximum)
    {
        await base.Nullable_keys_with_global_query_filter(async, maximum);

        AssertSql(
            $$"""
SELECT TOP(1) [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
FROM [Item] AS [i]
WHERE [i].[Scenario] = 1 AND EXISTS (
    SELECT [i].[Key]
    INTERSECT
    SELECT (
        SELECT TOP(1) [i0].[Key]
        FROM [Item] AS [i0]
        WHERE [i0].[Scenario] = 1 AND [i0].[Key] IS NOT NULL
        ORDER BY [i0].[Key]{{(maximum ? " DESC" : "")}}) AS [Key])
ORDER BY [i].[Id]
""");
    }

    public override async Task Non_nullable_keys_preserve_source_order(bool maximum)
    {
        await base.Non_nullable_keys_preserve_source_order(maximum);

        AssertSql(
            $$"""
SELECT TOP(1) [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
FROM [Item] AS [i]
ORDER BY [i].[Scenario]{{(maximum ? " DESC" : "")}}, [i].[Id] DESC
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
@p='1'
@p2='2'

SELECT TOP(1) [i0].[Id], [i0].[Key], [i0].[Scenario], [i0].[StringKey]
FROM (
    SELECT [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
    FROM [Item] AS [i]
    WHERE [i].[Scenario] = @scenario
    ORDER BY [i].[Scenario], [i].[Id] DESC
    OFFSET @p ROWS FETCH NEXT @p2 ROWS ONLY
) AS [i0]
ORDER BY CASE
    WHEN [i0].[{{key}}] IS NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END, [i0].[{{key}}]{{(maximum ? " DESC" : "")}}, [i0].[Scenario], [i0].[Id] DESC
"""
                : $$"""
@scenario='{{scenario}}'

SELECT TOP(1) [i].[Id], [i].[Key], [i].[Scenario], [i].[StringKey]
FROM [Item] AS [i]
WHERE [i].[Scenario] = @scenario AND EXISTS (
    SELECT [i].[{{key}}]
    INTERSECT
    SELECT (
        SELECT TOP(1) [i0].[{{key}}]
        FROM [Item] AS [i0]
        WHERE [i0].[Scenario] = @scenario AND [i0].[{{key}}] IS NOT NULL
        ORDER BY [i0].[{{key}}]{{(maximum ? " DESC" : "")}}) AS [{{key}}])
ORDER BY [i].[Id]{{(scenario == 5 ? " DESC" : "")}}
""");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Collection_auto_include(bool async)
    {
        var factory = await InitializeNonSharedTest<MinMaxByContext>(
            onModelCreating: b => b.Entity<Parent>().Navigation(p => p.Children).AutoInclude(),
            seed: c =>
            {
                c.AddRange(new Parent(), new Parent { Score = 7, Children = [new(), new()] });
                return c.SaveChangesAsync();
            });
        using var context = factory.CreateDbContext();
        var rows = context.Set<Parent>().AsNoTracking();
        var winner = async ? await rows.MinByAsync(p => p.Score) : rows.MinBy(p => p.Score);
        Assert.Equal(7, winner!.Score);
        Assert.Equal(2, winner.Children.Count);
        Assert.All(winner.Children, child => Assert.Equal(winner.Id, child.ParentId));
    }

    private class Parent
    {
        public int Id { get; set; }
        public int? Score { get; set; }
        public List<Child> Children { get; set; } = [];
    }

    private class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
    }

    private void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
