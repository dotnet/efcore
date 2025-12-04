// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsSetOperationsSqlServerTest(
    NavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : NavigationsSetOperationsRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Over_associate_collections()
    {
        await base.Over_associate_collections();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a1].[Id], [n].[Id], [n0].[Id], [a2].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a2].[CollectionRootId], [a2].[Int], [a2].[Ints], [a2].[Name], [a2].[OptionalNestedAssociateId], [a2].[RequiredNestedAssociateId], [a2].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a1] ON [r].[OptionalAssociateId] = [a1].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a1].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a1].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a2] ON [r].[RequiredAssociateId] = [a2].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a2].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a2].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a3].[Id], [a3].[CollectionRootId], [a3].[Int], [a3].[Ints], [a3].[Name], [a3].[OptionalNestedAssociateId], [a3].[RequiredNestedAssociateId], [a3].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a3]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a3].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a3].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a3].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a1].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a2].[Id] = [n7].[CollectionAssociateId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [AssociateType] AS [a]
        WHERE [r].[Id] = [a].[CollectionRootId] AND [a].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM [AssociateType] AS [a0]
        WHERE [r].[Id] = [a0].[CollectionRootId] AND [a0].[String] = N'foo'
    ) AS [u]) = 4
ORDER BY [r].[Id], [a1].[Id], [n].[Id], [n0].[Id], [a2].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_associate_collection_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Over_assocate_collection_Select_nested_with_aggregates_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_assocate_collection_Select_nested_with_aggregates_projected(queryTrackingBehavior);

        AssertSql(
            """
SELECT (
    SELECT ISNULL(SUM([s].[value]), 0)
    FROM (
        SELECT [a].[Id]
        FROM [AssociateType] AS [a]
        WHERE [r].[Id] = [a].[CollectionRootId] AND [a].[Int] = 8
        UNION ALL
        SELECT [a0].[Id]
        FROM [AssociateType] AS [a0]
        WHERE [r].[Id] = [a0].[CollectionRootId] AND [a0].[String] = N'foo'
    ) AS [u]
    OUTER APPLY (
        SELECT ISNULL(SUM([n].[Int]), 0) AS [value]
        FROM [NestedAssociateType] AS [n]
        WHERE [u].[Id] = [n].[CollectionAssociateId]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Over_nested_associate_collection()
    {
        await base.Over_nested_associate_collection();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n8].[Id], [n8].[CollectionAssociateId], [n8].[Int], [n8].[Ints], [n8].[Name], [n8].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n9].[Id], [n9].[CollectionAssociateId], [n9].[Int], [n9].[Ints], [n9].[Name], [n9].[String], [n3].[CollectionAssociateId], [n3].[Int], [n3].[Ints], [n3].[Name], [n3].[String], [n4].[CollectionAssociateId], [n4].[Int], [n4].[Ints], [n4].[Name], [n4].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN [NestedAssociateType] AS [n3] ON [a].[OptionalNestedAssociateId] = [n3].[Id]
INNER JOIN [NestedAssociateType] AS [n4] ON [a].[RequiredNestedAssociateId] = [n4].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n6].[Id] AS [Id1], [n7].[Id] AS [Id2], [n7].[CollectionAssociateId], [n7].[Int] AS [Int0], [n7].[Ints] AS [Ints0], [n7].[Name] AS [Name0], [n7].[String] AS [String0], [n5].[CollectionAssociateId] AS [CollectionAssociateId0], [n5].[Int] AS [Int1], [n5].[Ints] AS [Ints1], [n5].[Name] AS [Name1], [n5].[String] AS [String1], [n6].[CollectionAssociateId] AS [CollectionAssociateId1], [n6].[Int] AS [Int2], [n6].[Ints] AS [Ints2], [n6].[Name] AS [Name2], [n6].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[OptionalNestedAssociateId] = [n5].[Id]
    INNER JOIN [NestedAssociateType] AS [n6] ON [a1].[RequiredNestedAssociateId] = [n6].[Id]
    LEFT JOIN [NestedAssociateType] AS [n7] ON [a1].[Id] = [n7].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n8] ON [a0].[Id] = [n8].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n9] ON [a].[Id] = [n9].[CollectionAssociateId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [NestedAssociateType] AS [n]
        WHERE [a].[Id] = [n].[CollectionAssociateId] AND [n].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM [NestedAssociateType] AS [n0]
        WHERE [a].[Id] = [n0].[CollectionAssociateId] AND [n0].[String] = N'foo'
    ) AS [u]) = 4
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n8].[Id]
""");
    }

    public override async Task Over_different_collection_properties()
    {
        await base.Over_different_collection_properties();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n8].[Id], [n8].[CollectionAssociateId], [n8].[Int], [n8].[Ints], [n8].[Name], [n8].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n9].[Id], [n9].[CollectionAssociateId], [n9].[Int], [n9].[Ints], [n9].[Name], [n9].[String], [n3].[CollectionAssociateId], [n3].[Int], [n3].[Ints], [n3].[Name], [n3].[String], [n4].[CollectionAssociateId], [n4].[Int], [n4].[Ints], [n4].[Name], [n4].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN [NestedAssociateType] AS [n3] ON [a].[OptionalNestedAssociateId] = [n3].[Id]
INNER JOIN [NestedAssociateType] AS [n4] ON [a].[RequiredNestedAssociateId] = [n4].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n6].[Id] AS [Id1], [n7].[Id] AS [Id2], [n7].[CollectionAssociateId], [n7].[Int] AS [Int0], [n7].[Ints] AS [Ints0], [n7].[Name] AS [Name0], [n7].[String] AS [String0], [n5].[CollectionAssociateId] AS [CollectionAssociateId0], [n5].[Int] AS [Int1], [n5].[Ints] AS [Ints1], [n5].[Name] AS [Name1], [n5].[String] AS [String1], [n6].[CollectionAssociateId] AS [CollectionAssociateId1], [n6].[Int] AS [Int2], [n6].[Ints] AS [Ints2], [n6].[Name] AS [Name2], [n6].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[OptionalNestedAssociateId] = [n5].[Id]
    INNER JOIN [NestedAssociateType] AS [n6] ON [a1].[RequiredNestedAssociateId] = [n6].[Id]
    LEFT JOIN [NestedAssociateType] AS [n7] ON [a1].[Id] = [n7].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n8] ON [a0].[Id] = [n8].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n9] ON [a].[Id] = [n9].[CollectionAssociateId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [NestedAssociateType] AS [n]
        WHERE [a].[Id] = [n].[CollectionAssociateId]
        UNION ALL
        SELECT 1 AS empty
        FROM [NestedAssociateType] AS [n0]
        WHERE [a0].[Id] IS NOT NULL AND [a0].[Id] = [n0].[CollectionAssociateId]
    ) AS [u]) = 4
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n4].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n8].[Id]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
