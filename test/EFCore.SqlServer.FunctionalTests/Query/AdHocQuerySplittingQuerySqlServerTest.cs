﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocQuerySplittingQuerySqlServerTest : AdHocQuerySplittingQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    private static readonly FieldInfo _querySplittingBehaviorFieldInfo =
        typeof(RelationalOptionsExtension).GetField("_querySplittingBehavior", BindingFlags.NonPublic | BindingFlags.Instance);

    protected override DbContextOptionsBuilder SetQuerySplittingBehavior(DbContextOptionsBuilder optionsBuilder, QuerySplittingBehavior splittingBehavior)
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
        => SqlServerTestStore.CreateInitialized(StoreName, multipleActiveResultSets: true);

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
SELECT [c].[Id], [c].[ParentId], [t].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [t]
INNER JOIN [Child] AS [c] ON [t].[Id] = [c].[ParentId]
ORDER BY [t].[Id]
""",
                //
                """
SELECT [a].[Id], [a].[ParentId], [t].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [t]
INNER JOIN [AnotherChild] AS [a] ON [t].[Id] = [a].[ParentId]
ORDER BY [t].[Id]
""",
                //
                """
SELECT TOP(2) [p].[Id]
FROM [Parents] AS [p]
ORDER BY [p].[Id]
""",
                //
                """
SELECT [c].[Id], [c].[ParentId], [t].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [t]
INNER JOIN [Child] AS [c] ON [t].[Id] = [c].[ParentId]
ORDER BY [t].[Id]
""",
                //
                """
SELECT [a].[Id], [a].[ParentId], [t].[Id]
FROM (
    SELECT TOP(1) [p].[Id]
    FROM [Parents] AS [p]
    ORDER BY [p].[Id]
) AS [t]
INNER JOIN [AnotherChild] AS [a] ON [t].[Id] = [a].[ParentId]
ORDER BY [t].[Id]
""");
    }

    [ConditionalFact]
    public virtual async Task Using_AsSplitQuery_without_multiple_active_result_sets_works()
    {
        var contextFactory = await InitializeAsync<Context21355>(
            seed: c => c.Seed(),
            createTestStore: () => SqlServerTestStore.CreateInitialized(StoreName, multipleActiveResultSets: false));

        using var context = contextFactory.CreateContext();
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
}
