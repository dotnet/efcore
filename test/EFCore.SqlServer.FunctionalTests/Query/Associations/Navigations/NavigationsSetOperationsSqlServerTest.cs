// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

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
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n].[Id], [n0].[Id], [r3].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r3].[CollectionRootId], [r3].[Int], [r3].[Name], [r3].[OptionalNestedId], [r3].[RequiredNestedId], [r3].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r2] ON [r].[OptionalRelatedId] = [r2].[Id]
LEFT JOIN [NestedType] AS [n] ON [r2].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r2].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r3] ON [r].[RequiredRelatedId] = [r3].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r3].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r3].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r2].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r4].[Id], [r4].[CollectionRootId], [r4].[Int], [r4].[Name], [r4].[OptionalNestedId], [r4].[RequiredNestedId], [r4].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r4]
    LEFT JOIN [NestedType] AS [n4] ON [r4].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r4].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r4].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r3].[Id] = [n7].[CollectionRelatedId]
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
ORDER BY [r].[Id], [r2].[Id], [n].[Id], [n0].[Id], [r3].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
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
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [r0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [n5].[Id], [n5].[CollectionRelatedId], [n5].[Int], [n5].[Name], [n5].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n9].[Id], [n9].[CollectionRelatedId], [n9].[Int], [n9].[Name], [n9].[String], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n4].[CollectionRelatedId], [n4].[Int], [n4].[Name], [n4].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r1].[OptionalNestedId] = [n1].[Id]
LEFT JOIN [NestedType] AS [n2] ON [r1].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r0].[OptionalNestedId] = [n3].[Id]
INNER JOIN [NestedType] AS [n4] ON [r0].[RequiredNestedId] = [n4].[Id]
LEFT JOIN [NestedType] AS [n5] ON [r1].[Id] = [n5].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n6].[Id] AS [Id0], [n7].[Id] AS [Id1], [n8].[Id] AS [Id2], [n8].[CollectionRelatedId], [n8].[Int] AS [Int0], [n8].[Name] AS [Name0], [n8].[String] AS [String0], [n6].[CollectionRelatedId] AS [CollectionRelatedId0], [n6].[Int] AS [Int1], [n6].[Name] AS [Name1], [n6].[String] AS [String1], [n7].[CollectionRelatedId] AS [CollectionRelatedId1], [n7].[Int] AS [Int2], [n7].[Name] AS [Name2], [n7].[String] AS [String2]
    FROM [RelatedType] AS [r2]
    LEFT JOIN [NestedType] AS [n6] ON [r2].[OptionalNestedId] = [n6].[Id]
    INNER JOIN [NestedType] AS [n7] ON [r2].[RequiredNestedId] = [n7].[Id]
    LEFT JOIN [NestedType] AS [n8] ON [r2].[Id] = [n8].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n9] ON [r0].[Id] = [n9].[CollectionRelatedId]
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
ORDER BY [r].[Id], [r0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [n5].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Over_different_collection_properties()
    {
        await base.Over_different_collection_properties();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [r0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [n5].[Id], [n5].[CollectionRelatedId], [n5].[Int], [n5].[Name], [n5].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n9].[Id], [n9].[CollectionRelatedId], [n9].[Int], [n9].[Name], [n9].[String], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n4].[CollectionRelatedId], [n4].[Int], [n4].[Name], [n4].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r1].[OptionalNestedId] = [n1].[Id]
LEFT JOIN [NestedType] AS [n2] ON [r1].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r0].[OptionalNestedId] = [n3].[Id]
INNER JOIN [NestedType] AS [n4] ON [r0].[RequiredNestedId] = [n4].[Id]
LEFT JOIN [NestedType] AS [n5] ON [r1].[Id] = [n5].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n6].[Id] AS [Id0], [n7].[Id] AS [Id1], [n8].[Id] AS [Id2], [n8].[CollectionRelatedId], [n8].[Int] AS [Int0], [n8].[Name] AS [Name0], [n8].[String] AS [String0], [n6].[CollectionRelatedId] AS [CollectionRelatedId0], [n6].[Int] AS [Int1], [n6].[Name] AS [Name1], [n6].[String] AS [String1], [n7].[CollectionRelatedId] AS [CollectionRelatedId1], [n7].[Int] AS [Int2], [n7].[Name] AS [Name2], [n7].[String] AS [String2]
    FROM [RelatedType] AS [r2]
    LEFT JOIN [NestedType] AS [n6] ON [r2].[OptionalNestedId] = [n6].[Id]
    INNER JOIN [NestedType] AS [n7] ON [r2].[RequiredNestedId] = [n7].[Id]
    LEFT JOIN [NestedType] AS [n8] ON [r2].[Id] = [n8].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n9] ON [r0].[Id] = [n9].[CollectionRelatedId]
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
ORDER BY [r].[Id], [r0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [n5].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
