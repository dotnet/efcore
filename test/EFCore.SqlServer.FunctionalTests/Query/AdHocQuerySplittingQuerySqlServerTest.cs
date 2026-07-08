// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocQuerySplittingQuerySqlServerTest(NonSharedFixture fixture) : AdHocQuerySplittingQueryTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    private static readonly FieldInfo _querySplittingBehaviorFieldInfo =
        typeof(RelationalOptionsExtension).GetField("_querySplittingBehavior", BindingFlags.NonPublic | BindingFlags.Instance);

    protected override DbContextOptionsBuilder SetQuerySplittingBehavior(
        DbContextOptionsBuilder optionsBuilder,
        QuerySplittingBehavior splittingBehavior)
    {
        new SqlServerDbContextOptionsBuilder(optionsBuilder).UseQuerySplittingBehavior(splittingBehavior);

        return optionsBuilder;
    }

    protected override DbContextOptionsBuilder ClearQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder)
    {
        var extension = optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>();
        if (extension == null)
        {
            extension = new SqlServerOptionsExtension();
        }
        else
        {
            _querySplittingBehaviorFieldInfo.SetValue(extension, null);
        }

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        return optionsBuilder;
    }

    protected override TestStore CreateTestStore25225()
    {
        var testStore = SqlServerTestStore.Create(NonSharedStoreName, multipleActiveResultSets: true);
        testStore.UseConnectionString = true;
        return testStore;
    }

    public override async Task Can_configure_SingleQuery_at_context_level()
    {
        await base.Can_configure_SingleQuery_at_context_level();

        AssertSql(
            """
SELECT [p].[Id], [c].[Id], [c].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [p].[Id], [c].[Id], [c].[ParentId], [a].[Id], [a].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
LEFT JOIN [AnotherChild] AS [a] ON [p].[Id] = [a].[ParentId]
ORDER BY [p].[Id], [c].[Id]
""");
    }

    public override async Task Can_configure_SplitQuery_at_context_level()
    {
        await base.Can_configure_SplitQuery_at_context_level();

        AssertSql(
            """
SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [p].[Id], [c].[Id], [c].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [AnotherChild] AS [a] ON [p].[Id] = [a].[ParentId]
ORDER BY [p].[Id]
""");
    }

    public override async Task Unconfigured_query_splitting_behavior_throws_a_warning()
    {
        await base.Unconfigured_query_splitting_behavior_throws_a_warning();

        AssertSql(
            """
SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [AnotherChild] AS [a] ON [p].[Id] = [a].[ParentId]
ORDER BY [p].[Id]
""");
    }

    public override async Task Using_AsSingleQuery_without_context_configuration_does_not_throw_warning()
    {
        await base.Using_AsSingleQuery_without_context_configuration_does_not_throw_warning();

        AssertSql(
            """
SELECT [p].[Id], [c].[Id], [c].[ParentId], [a].[Id], [a].[ParentId]
FROM [Parents] AS [p]
LEFT JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
LEFT JOIN [AnotherChild] AS [a] ON [p].[Id] = [a].[ParentId]
ORDER BY [p].[Id], [c].[Id]
""");
    }

    public override async Task SplitQuery_disposes_inner_data_readers()
    {
        await base.SplitQuery_disposes_inner_data_readers();

        AssertSql(
            """
SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [AnotherChild] AS [a] ON [p].[Id] = [a].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [AnotherChild] AS [a] ON [p].[Id] = [a].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT TOP(2) [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p0].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [p0]
INNER JOIN [Child] AS [c] ON [p0].[Id] = [c].[ParentId]
ORDER BY [p0].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ParentId], [p1].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [p1]
INNER JOIN [AnotherChild] AS [a] ON [p1].[Id] = [a].[ParentId]
ORDER BY [p1].[Id]
""",
            //
            """
SELECT TOP(2) [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p0].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [p0]
INNER JOIN [Child] AS [c] ON [p0].[Id] = [c].[ParentId]
ORDER BY [p0].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ParentId], [p1].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [p1]
INNER JOIN [AnotherChild] AS [a] ON [p1].[Id] = [a].[ParentId]
ORDER BY [p1].[Id]
""");
    }

    [Fact]
    public virtual async Task Using_AsSplitQuery_without_multiple_active_result_sets_works()
    {
        var contextFactory = await InitializeNonSharedTest<Context21355>(
            seed: c => c.SeedAsync(),
            createTestStore: () => SqlServerTestStore.Create(NonSharedStoreName, multipleActiveResultSets: false));

        using var context = contextFactory.CreateDbContext();
        context.Parents.Include(p => p.Children1).Include(p => p.Children2).AsSplitQuery().ToList();

        AssertSql(
            """
SELECT [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [c].[Id], [c].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [Child] AS [c] ON [p].[Id] = [c].[ParentId]
ORDER BY [p].[Id]
""",
            //
            """
SELECT [a].[Id], [a].[ParentId], [p].[Id]
FROM [Parents] AS [p]
INNER JOIN [AnotherChild] AS [a] ON [p].[Id] = [a].[ParentId]
ORDER BY [p].[Id]
""");
    }

    public override async Task NoTracking_split_query_creates_only_required_instances(bool async)
    {
        await base.NoTracking_split_query_creates_only_required_instances(async);

        AssertSql(
            """
SELECT TOP(1) [t].[Id], [t].[Value]
FROM [Tests] AS [t]
ORDER BY [t].[Id]
""");
    }

    public override async Task NoTrackingWithIdentityResolution_split_query_basic(bool async)
    {
        await base.NoTrackingWithIdentityResolution_split_query_basic(async);

        AssertSql(
            """
SELECT [t].[Id]
FROM [Tests] AS [t]
ORDER BY [t].[Id]
""");
    }

    public override async Task NoTrackingWithIdentityResolution_split_query_complex(bool async)
    {
        await base.NoTrackingWithIdentityResolution_split_query_complex(async);

        AssertSql(
            """
SELECT [t].[Id]
FROM [Tests] AS [t]
ORDER BY [t].[Id]
""");
    }

    public override async Task Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_async()
    {
        await base.Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_async();

        Assert.Equal(400, TestSqlLoggerFactory.SqlStatements.Count);

        AssertContainsSql(
            """
@parentId='e79c82f4-3ae7-4c65-85db-04e08cba6fa7'

SELECT TOP(2) [p].[Id]
FROM [Parents] AS [p]
WHERE [p].[Id] = @parentId
ORDER BY [p].[Id]
""",
            //
            """
@parentId='d6457b52-690a-419e-8982-a1a8551b4572'

SELECT TOP(2) [p].[Id]
FROM [Parents] AS [p]
WHERE [p].[Id] = @parentId
ORDER BY [p].[Id]
""",
            //
            """
@parentId='e79c82f4-3ae7-4c65-85db-04e08cba6fa7'

SELECT [c2].[Id], [c2].[ParentId], [p0].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    WHERE [p].[Id] = @parentId
    ORDER BY [p].[Id]
) AS [p0]
INNER JOIN [Collection] AS [c2] ON [p0].[Id] = [c2].[ParentId]
ORDER BY [p0].[Id]
""",
            //
            """
@parentId='d6457b52-690a-419e-8982-a1a8551b4572'

SELECT [c2].[Id], [c2].[ParentId], [p0].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    WHERE [p].[Id] = @parentId
    ORDER BY [p].[Id]
) AS [p0]
INNER JOIN [Collection] AS [c2] ON [p0].[Id] = [c2].[ParentId]
ORDER BY [p0].[Id]
""");
    }

    public override async Task Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_sync()
    {
        await base.Can_query_with_nav_collection_in_projection_with_split_query_in_parallel_sync();

        Assert.Equal(40, TestSqlLoggerFactory.SqlStatements.Count);

        AssertContainsSql(
            """
@parentId='e79c82f4-3ae7-4c65-85db-04e08cba6fa7'

SELECT TOP(2) [p].[Id]
FROM [Parents] AS [p]
WHERE [p].[Id] = @parentId
ORDER BY [p].[Id]
""",
            //
            """
@parentId='d6457b52-690a-419e-8982-a1a8551b4572'

SELECT TOP(2) [p].[Id]
FROM [Parents] AS [p]
WHERE [p].[Id] = @parentId
ORDER BY [p].[Id]
""",
            //
            """
@parentId='e79c82f4-3ae7-4c65-85db-04e08cba6fa7'

SELECT [c2].[Id], [c2].[ParentId], [p0].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    WHERE [p].[Id] = @parentId
    ORDER BY [p].[Id]
) AS [p0]
INNER JOIN [Collection] AS [c2] ON [p0].[Id] = [c2].[ParentId]
ORDER BY [p0].[Id]
""",
            //
            """
@parentId='d6457b52-690a-419e-8982-a1a8551b4572'

SELECT [c2].[Id], [c2].[ParentId], [p0].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    WHERE [p].[Id] = @parentId
    ORDER BY [p].[Id]
) AS [p0]
INNER JOIN [Collection] AS [c2] ON [p0].[Id] = [c2].[ParentId]
ORDER BY [p0].[Id]
""");
    }

    private void AssertContainsSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
