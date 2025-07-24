// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public class NavigationsSetOperationsSqlServerTest(
    NavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : NavigationsSetOperationsRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task On_related()
    {
        await base.On_related();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [RelatedType] AS [r0]
        WHERE [r].[Id] = [r0].[CollectionRootId] AND [r0].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM [RelatedType] AS [r1]
        WHERE [r].[Id] = [r1].[CollectionRootId] AND [r1].[String] = N'foo'
    ) AS [u]) = 4
""");
    }

    public override async Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.On_related_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task On_related_Select_nested_with_aggregates(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.On_related_Select_nested_with_aggregates(queryTrackingBehavior);

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM (
        SELECT [r0].[Id]
        FROM [RelatedType] AS [r0]
        WHERE [r].[Id] = [r0].[CollectionRootId] AND [r0].[Int] = 8
        UNION ALL
        SELECT [r1].[Id]
        FROM [RelatedType] AS [r1]
        WHERE [r].[Id] = [r1].[CollectionRootId] AND [r1].[String] = N'foo'
    ) AS [u]
    OUTER APPLY (
        SELECT COALESCE(SUM([n].[Int]), 0) AS [value]
        FROM [NestedType] AS [n]
        WHERE [u].[Id] = [n].[CollectionRelatedId]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
    }

    public override async Task On_nested()
    {
        await base.On_nested();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [NestedType] AS [n]
        WHERE [r0].[Id] = [n].[CollectionRelatedId] AND [n].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM [NestedType] AS [n0]
        WHERE [r0].[Id] = [n0].[CollectionRelatedId] AND [n0].[String] = N'foo'
    ) AS [u]) = 4
""");
    }

    public override async Task Over_different_collection_properties()
    {
        await base.Over_different_collection_properties();

        AssertSql(
"""
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [NestedType] AS [n]
        WHERE [r0].[Id] = [n].[CollectionRelatedId]
        UNION ALL
        SELECT 1 AS empty
        FROM [NestedType] AS [n0]
        WHERE [r1].[Id] IS NOT NULL AND [r1].[Id] = [n0].[CollectionRelatedId]
    ) AS [u]) = 4
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
