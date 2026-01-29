// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsStructuralEqualitySqlServerTest(
    NavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : NavigationsStructuralEqualityRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Two_associates()
    {
        await base.Two_associates();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [a].[Id] = [a0].[Id]
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a].[OptionalNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [n].[Id] = [n0].[Id]
ORDER BY [r].[Id], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [a].[Id] <> [a0].[Id] OR [a0].[Id] IS NULL
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Associate_with_inline_null()
    {
        await base.Associate_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
WHERE [a].[Id] IS NULL
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Associate_with_parameter_null()
    {
        await base.Associate_with_parameter_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
WHERE [a].[Id] IS NULL
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Nested_associate_with_inline_null()
    {
        await base.Nested_associate_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[OptionalNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[RequiredNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [n].[Id] IS NULL
ORDER BY [r].[Id], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Optional_associate_nested_associate_with_inline_null()
    {
        await base.Optional_associate_nested_associate_with_inline_null();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
WHERE [n].[Id] IS NULL
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Optional_associate_nested_associate_with_inline_not_null()
    {
        await base.Optional_associate_nested_associate_with_inline_not_null();


        AssertSql(
        """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
WHERE [n].[Id] IS NOT NULL
ORDER BY [r].[Id], [a].[Id], [n].[Id], [n0].[Id], [a0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Nested_associate_with_inline()
    {
        await base.Nested_associate_with_inline();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[OptionalNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[RequiredNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a].[OptionalNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [n].[Id] = 1000
ORDER BY [r].[Id], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Nested_associate_with_parameter()
    {
        await base.Nested_associate_with_parameter();

        AssertSql(
            """
@entity_equality_nested_Id='1000' (Nullable = true)

SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[OptionalNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[RequiredNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a].[OptionalNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [n].[Id] = @entity_equality_nested_Id
ORDER BY [r].[Id], [a].[Id], [n].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [a].[Id] = [a0].[Id]
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql();
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql();
    }

    #region Contains

    public override async Task Contains_with_inline()
    {
        await base.Contains_with_inline();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n8].[Id], [n8].[CollectionAssociateId], [n8].[Int], [n8].[Ints], [n8].[Name], [n8].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [n3].[CollectionAssociateId], [n3].[Int], [n3].[Ints], [n3].[Name], [n3].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[OptionalNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[RequiredNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a].[OptionalNestedAssociateId] = [n2].[Id]
INNER JOIN [NestedAssociateType] AS [n3] ON [a].[RequiredNestedAssociateId] = [n3].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionAssociateId], [n6].[Int] AS [Int0], [n6].[Ints] AS [Ints0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionAssociateId] AS [CollectionAssociateId0], [n4].[Int] AS [Int1], [n4].[Ints] AS [Ints1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionAssociateId] AS [CollectionAssociateId1], [n5].[Int] AS [Int2], [n5].[Ints] AS [Ints2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n4] ON [a1].[OptionalNestedAssociateId] = [n4].[Id]
    INNER JOIN [NestedAssociateType] AS [n5] ON [a1].[RequiredNestedAssociateId] = [n5].[Id]
    LEFT JOIN [NestedAssociateType] AS [n6] ON [a1].[Id] = [n6].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n8] ON [a].[Id] = [n8].[CollectionAssociateId]
WHERE EXISTS (
    SELECT 1
    FROM [NestedAssociateType] AS [n]
    WHERE [a].[Id] = [n].[CollectionAssociateId] AND [n].[Id] = 1002)
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n7].[Id]
""");
    }

    public override async Task Contains_with_parameter()
    {
        await base.Contains_with_parameter();

        AssertSql(
            """
@entity_equality_nested_Id='1002' (Nullable = true)

SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n8].[Id], [n8].[CollectionAssociateId], [n8].[Int], [n8].[Ints], [n8].[Name], [n8].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [n3].[CollectionAssociateId], [n3].[Int], [n3].[Ints], [n3].[Name], [n3].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[OptionalNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[RequiredNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a].[OptionalNestedAssociateId] = [n2].[Id]
INNER JOIN [NestedAssociateType] AS [n3] ON [a].[RequiredNestedAssociateId] = [n3].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionAssociateId], [n6].[Int] AS [Int0], [n6].[Ints] AS [Ints0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionAssociateId] AS [CollectionAssociateId0], [n4].[Int] AS [Int1], [n4].[Ints] AS [Ints1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionAssociateId] AS [CollectionAssociateId1], [n5].[Int] AS [Int2], [n5].[Ints] AS [Ints2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n4] ON [a1].[OptionalNestedAssociateId] = [n4].[Id]
    INNER JOIN [NestedAssociateType] AS [n5] ON [a1].[RequiredNestedAssociateId] = [n5].[Id]
    LEFT JOIN [NestedAssociateType] AS [n6] ON [a1].[Id] = [n6].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n8] ON [a].[Id] = [n8].[CollectionAssociateId]
WHERE EXISTS (
    SELECT 1
    FROM [NestedAssociateType] AS [n]
    WHERE [a].[Id] = [n].[CollectionAssociateId] AND [n].[Id] = @entity_equality_nested_Id)
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n7].[Id]
""");
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await base.Contains_with_operators_composed_on_the_collection();

        AssertSql(
            """
@get_Item_Int='106'
@entity_equality_get_Item_Id='3003' (Nullable = true)

SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n8].[Id], [n8].[CollectionAssociateId], [n8].[Int], [n8].[Ints], [n8].[Name], [n8].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [n3].[CollectionAssociateId], [n3].[Int], [n3].[Ints], [n3].[Name], [n3].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[OptionalNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[RequiredNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a].[OptionalNestedAssociateId] = [n2].[Id]
INNER JOIN [NestedAssociateType] AS [n3] ON [a].[RequiredNestedAssociateId] = [n3].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionAssociateId], [n6].[Int] AS [Int0], [n6].[Ints] AS [Ints0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionAssociateId] AS [CollectionAssociateId0], [n4].[Int] AS [Int1], [n4].[Ints] AS [Ints1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionAssociateId] AS [CollectionAssociateId1], [n5].[Int] AS [Int2], [n5].[Ints] AS [Ints2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n4] ON [a1].[OptionalNestedAssociateId] = [n4].[Id]
    INNER JOIN [NestedAssociateType] AS [n5] ON [a1].[RequiredNestedAssociateId] = [n5].[Id]
    LEFT JOIN [NestedAssociateType] AS [n6] ON [a1].[Id] = [n6].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n8] ON [a].[Id] = [n8].[CollectionAssociateId]
WHERE EXISTS (
    SELECT 1
    FROM [NestedAssociateType] AS [n]
    WHERE [a].[Id] = [n].[CollectionAssociateId] AND [n].[Int] > @get_Item_Int AND [n].[Id] = @entity_equality_get_Item_Id)
ORDER BY [r].[Id], [a].[Id], [a0].[Id], [n0].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n7].[Id]
""");
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        await base.Contains_with_nested_and_composed_operators();

        AssertSql(
            """
@get_Item_Id='302'
@entity_equality_get_Item_Id='303' (Nullable = true)

SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [a0].[Id], [n].[Id], [n0].[Id], [a1].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a1] ON [r].[RequiredAssociateId] = [a1].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a1].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a1].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a2].[Id], [a2].[CollectionRootId], [a2].[Int], [a2].[Ints], [a2].[Name], [a2].[OptionalNestedAssociateId], [a2].[RequiredNestedAssociateId], [a2].[String], [n3].[Id] AS [Id0], [n4].[Id] AS [Id1], [n5].[Id] AS [Id2], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a2]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a2].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a2].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a2].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a1].[Id] = [n7].[CollectionAssociateId]
WHERE EXISTS (
    SELECT 1
    FROM [AssociateType] AS [a]
    WHERE [r].[Id] = [a].[CollectionRootId] AND [a].[Id] > @get_Item_Id AND [a].[Id] = @entity_equality_get_Item_Id)
ORDER BY [r].[Id], [a0].[Id], [n].[Id], [n0].[Id], [a1].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    #endregion Contains

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
