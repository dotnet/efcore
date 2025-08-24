// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsMiscellaneousSqlServerTest(
    NavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : NavigationsMiscellaneousRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_related_property()
    {
        await base.Where_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [r0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n] ON [r1].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r0].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r0].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r1].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r2]
    LEFT JOIN [NestedType] AS [n4] ON [r2].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r2].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r2].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r0].[Id] = [n7].[CollectionRelatedId]
WHERE [r0].[Int] = 8
ORDER BY [r].[Id], [r0].[Id], [r1].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Where_optional_related_property()
    {
        await base.Where_optional_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n].[Id], [n0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r0] ON [r].[OptionalRelatedId] = [r0].[Id]
LEFT JOIN [NestedType] AS [n] ON [r0].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r0].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r1] ON [r].[RequiredRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r1].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r1].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r0].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r2]
    LEFT JOIN [NestedType] AS [n4] ON [r2].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r2].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r2].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r1].[Id] = [n7].[CollectionRelatedId]
WHERE [r0].[Int] = 8
ORDER BY [r].[Id], [r0].[Id], [n].[Id], [n0].[Id], [r1].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Where_nested_related_property()
    {
        await base.Where_nested_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [r0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [RelatedType] AS [r0] ON [r].[RequiredRelatedId] = [r0].[Id]
INNER JOIN [NestedType] AS [n] ON [r0].[RequiredNestedId] = [n].[Id]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[OptionalNestedId] = [n0].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r1].[RequiredNestedId] = [n1].[Id]
LEFT JOIN [NestedType] AS [n2] ON [r0].[OptionalNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r1].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r2].[Id], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r2]
    LEFT JOIN [NestedType] AS [n4] ON [r2].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r2].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r2].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r0].[Id] = [n7].[CollectionRelatedId]
WHERE [n].[Int] = 8
ORDER BY [r].[Id], [r0].[Id], [n].[Id], [r1].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    #endregion Simple filters

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
