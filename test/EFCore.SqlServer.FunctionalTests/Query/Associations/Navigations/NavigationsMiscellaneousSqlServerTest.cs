// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsMiscellaneousSqlServerTest(
    NavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : NavigationsMiscellaneousRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[Id], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[Id], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[Id], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[Id] AS [Id1], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[Id] AS [Id2], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [a].[Int] = 8
ORDER BY [r].[Id], [s].[Id], [s].[Id0], [n6].[Id]
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[Id], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[Id], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[Id], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [AssociateType] AS [a] ON [r].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [r].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[Id] AS [Id1], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[Id] AS [Id2], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
WHERE [a].[Int] = 8
ORDER BY [r].[Id], [s].[Id], [s].[Id0], [n6].[Id]
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalAssociateId], [r].[RequiredAssociateId], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a0].[Id], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n0].[Id], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n2].[Id], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String], [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String]
FROM [RootEntity] AS [r]
INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
INNER JOIN [NestedAssociateType] AS [n] ON [a].[RequiredNestedAssociateId] = [n].[Id]
LEFT JOIN [AssociateType] AS [a0] ON [r].[OptionalAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[OptionalNestedAssociateId] = [n0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[RequiredNestedAssociateId] = [n1].[Id]
LEFT JOIN [NestedAssociateType] AS [n2] ON [a].[OptionalNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[Id] AS [Id1], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[Id] AS [Id2], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a0].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a].[Id] = [n7].[CollectionAssociateId]
WHERE [n].[Int] = 8
ORDER BY [r].[Id], [s].[Id], [s].[Id0], [n6].[Id]
""");
    }

    #endregion Simple filters

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [m].[OptionalAssociateId], [m].[RequiredAssociateId], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [n].[Id], [n].[CollectionAssociateId], [n].[Int], [n].[Ints], [n].[Name], [n].[String], [n0].[Id], [n0].[CollectionAssociateId], [n0].[Int], [n0].[Ints], [n0].[Name], [n0].[String], [a0].[Id], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [n1].[Id], [n1].[CollectionAssociateId], [n1].[Int], [n1].[Ints], [n1].[Name], [n1].[String], [n2].[Id], [n2].[CollectionAssociateId], [n2].[Int], [n2].[Ints], [n2].[Name], [n2].[String]
FROM (
    SELECT * FROM [RootEntity]
) AS [m]
LEFT JOIN [AssociateType] AS [a] ON [m].[OptionalAssociateId] = [a].[Id]
LEFT JOIN [NestedAssociateType] AS [n] ON [a].[OptionalNestedAssociateId] = [n].[Id]
LEFT JOIN [NestedAssociateType] AS [n0] ON [a].[RequiredNestedAssociateId] = [n0].[Id]
INNER JOIN [AssociateType] AS [a0] ON [m].[RequiredAssociateId] = [a0].[Id]
LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[OptionalNestedAssociateId] = [n1].[Id]
INNER JOIN [NestedAssociateType] AS [n2] ON [a0].[RequiredNestedAssociateId] = [n2].[Id]
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[Id] AS [Id1], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[Id] AS [Id2], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s] ON [m].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [a].[Id] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [a0].[Id] = [n7].[CollectionAssociateId]
ORDER BY [m].[Id], [s].[Id], [s].[Id0], [n6].[Id]
""");
    }

    #region GroupBy

    public override async Task GroupBy_OrderByDescending_First_on_associate_scalar_property()
    {
        await base.GroupBy_OrderByDescending_First_on_associate_scalar_property();

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name], [s0].[OptionalAssociateId], [s0].[RequiredAssociateId], [r3].[Key], [s1].[Id], [s1].[CollectionRootId], [s1].[Int], [s1].[Ints], [s1].[Name], [s1].[OptionalNestedAssociateId], [s1].[RequiredNestedAssociateId], [s1].[String], [s1].[Id0], [s1].[CollectionAssociateId], [s1].[Int0], [s1].[Ints0], [s1].[Name0], [s1].[String0], [s1].[Id1], [s1].[CollectionAssociateId0], [s1].[Int1], [s1].[Ints1], [s1].[Name1], [s1].[String1], [s1].[Id2], [s1].[CollectionAssociateId1], [s1].[Int2], [s1].[Ints2], [s1].[Name2], [s1].[String2], [s0].[Id0], [s0].[CollectionRootId], [s0].[Int], [s0].[Ints], [s0].[Name0], [s0].[OptionalNestedAssociateId], [s0].[RequiredNestedAssociateId], [s0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [s0].[Id1], [s0].[CollectionAssociateId], [s0].[Int0], [s0].[Ints0], [s0].[Name1], [s0].[String0], [s0].[Id2], [s0].[CollectionAssociateId0], [s0].[Int1], [s0].[Ints1], [s0].[Name2], [s0].[String1], [s0].[Id3], [s0].[CollectionRootId0], [s0].[Int2], [s0].[Ints2], [s0].[Name3], [s0].[OptionalNestedAssociateId0], [s0].[RequiredNestedAssociateId0], [s0].[String2], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [s0].[Id4], [s0].[CollectionAssociateId1], [s0].[Int3], [s0].[Ints3], [s0].[Name4], [s0].[String3], [s0].[Id5], [s0].[CollectionAssociateId2], [s0].[Int4], [s0].[Ints4], [s0].[Name5], [s0].[String4]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r3]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[OptionalAssociateId], [s].[RequiredAssociateId], [s].[Id0], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name0], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id1], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name1], [s].[String0], [s].[Id2], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name2], [s].[String1], [s].[Id3], [s].[CollectionRootId0], [s].[Int2], [s].[Ints2], [s].[Name3], [s].[OptionalNestedAssociateId0], [s].[RequiredNestedAssociateId0], [s].[String2], [s].[Id4], [s].[CollectionAssociateId1], [s].[Int3], [s].[Ints3], [s].[Name4], [s].[String3], [s].[Id5], [s].[CollectionAssociateId2], [s].[Int4], [s].[Ints4], [s].[Name5], [s].[String4], [s].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociateId], [r1].[RequiredAssociateId], [a0].[Id] AS [Id0], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name] AS [Name0], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n].[Id] AS [Id1], [n].[CollectionAssociateId], [n].[Int] AS [Int0], [n].[Ints] AS [Ints0], [n].[Name] AS [Name1], [n].[String] AS [String0], [n0].[Id] AS [Id2], [n0].[CollectionAssociateId] AS [CollectionAssociateId0], [n0].[Int] AS [Int1], [n0].[Ints] AS [Ints1], [n0].[Name] AS [Name2], [n0].[String] AS [String1], [a].[Id] AS [Id3], [a].[CollectionRootId] AS [CollectionRootId0], [a].[Int] AS [Int2], [a].[Ints] AS [Ints2], [a].[Name] AS [Name3], [a].[OptionalNestedAssociateId] AS [OptionalNestedAssociateId0], [a].[RequiredNestedAssociateId] AS [RequiredNestedAssociateId0], [a].[String] AS [String2], [n1].[Id] AS [Id4], [n1].[CollectionAssociateId] AS [CollectionAssociateId1], [n1].[Int] AS [Int3], [n1].[Ints] AS [Ints3], [n1].[Name] AS [Name4], [n1].[String] AS [String3], [n2].[Id] AS [Id5], [n2].[CollectionAssociateId] AS [CollectionAssociateId2], [n2].[Int] AS [Int4], [n2].[Ints] AS [Ints4], [n2].[Name] AS [Name5], [n2].[String] AS [String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [a].[Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[OptionalAssociateId], [r2].[RequiredAssociateId], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        INNER JOIN [AssociateType] AS [a] ON [r1].[RequiredAssociateId] = [a].[Id]
        LEFT JOIN [AssociateType] AS [a0] ON [r1].[OptionalAssociateId] = [a0].[Id]
        LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
        LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
        LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[OptionalNestedAssociateId] = [n1].[Id]
        INNER JOIN [NestedAssociateType] AS [n2] ON [a].[RequiredNestedAssociateId] = [n2].[Id]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [r3].[Key] = [s0].[Key] OR ([r3].[Key] IS NULL AND [s0].[Key] IS NULL)
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[Id] AS [Id1], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[Id] AS [Id2], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s1] ON [s0].[Id] = [s1].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [s0].[Id0] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [s0].[Id3] = [n7].[CollectionAssociateId]
ORDER BY [r3].[Key], [s1].[Id], [s1].[Id0], [n6].[Id]
""");
    }

    public override async Task GroupBy_MaxBy_on_associate_scalar_property()
    {
        await base.GroupBy_MaxBy_on_associate_scalar_property();

        AssertSql(
            """
SELECT [s0].[Id], [s0].[Name], [s0].[OptionalAssociateId], [s0].[RequiredAssociateId], [r3].[Key], [s1].[Id], [s1].[CollectionRootId], [s1].[Int], [s1].[Ints], [s1].[Name], [s1].[OptionalNestedAssociateId], [s1].[RequiredNestedAssociateId], [s1].[String], [s1].[Id0], [s1].[CollectionAssociateId], [s1].[Int0], [s1].[Ints0], [s1].[Name0], [s1].[String0], [s1].[Id1], [s1].[CollectionAssociateId0], [s1].[Int1], [s1].[Ints1], [s1].[Name1], [s1].[String1], [s1].[Id2], [s1].[CollectionAssociateId1], [s1].[Int2], [s1].[Ints2], [s1].[Name2], [s1].[String2], [s0].[Id0], [s0].[CollectionRootId], [s0].[Int], [s0].[Ints], [s0].[Name0], [s0].[OptionalNestedAssociateId], [s0].[RequiredNestedAssociateId], [s0].[String], [n6].[Id], [n6].[CollectionAssociateId], [n6].[Int], [n6].[Ints], [n6].[Name], [n6].[String], [s0].[Id1], [s0].[CollectionAssociateId], [s0].[Int0], [s0].[Ints0], [s0].[Name1], [s0].[String0], [s0].[Id2], [s0].[CollectionAssociateId0], [s0].[Int1], [s0].[Ints1], [s0].[Name2], [s0].[String1], [s0].[Id3], [s0].[CollectionRootId0], [s0].[Int2], [s0].[Ints2], [s0].[Name3], [s0].[OptionalNestedAssociateId0], [s0].[RequiredNestedAssociateId0], [s0].[String2], [n7].[Id], [n7].[CollectionAssociateId], [n7].[Int], [n7].[Ints], [n7].[Name], [n7].[String], [s0].[Id4], [s0].[CollectionAssociateId1], [s0].[Int3], [s0].[Ints3], [s0].[Name4], [s0].[String3], [s0].[Id5], [s0].[CollectionAssociateId2], [s0].[Int4], [s0].[Ints4], [s0].[Name5], [s0].[String4]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r3]
LEFT JOIN (
    SELECT [s].[Id], [s].[Name], [s].[OptionalAssociateId], [s].[RequiredAssociateId], [s].[Id0], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name0], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id1], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name1], [s].[String0], [s].[Id2], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name2], [s].[String1], [s].[Id3], [s].[CollectionRootId0], [s].[Int2], [s].[Ints2], [s].[Name3], [s].[OptionalNestedAssociateId0], [s].[RequiredNestedAssociateId0], [s].[String2], [s].[Id4], [s].[CollectionAssociateId1], [s].[Int3], [s].[Ints3], [s].[Name4], [s].[String3], [s].[Id5], [s].[CollectionAssociateId2], [s].[Int4], [s].[Ints4], [s].[Name5], [s].[String4], [s].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociateId], [r1].[RequiredAssociateId], [a0].[Id] AS [Id0], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name] AS [Name0], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n].[Id] AS [Id1], [n].[CollectionAssociateId], [n].[Int] AS [Int0], [n].[Ints] AS [Ints0], [n].[Name] AS [Name1], [n].[String] AS [String0], [n0].[Id] AS [Id2], [n0].[CollectionAssociateId] AS [CollectionAssociateId0], [n0].[Int] AS [Int1], [n0].[Ints] AS [Ints1], [n0].[Name] AS [Name2], [n0].[String] AS [String1], [a].[Id] AS [Id3], [a].[CollectionRootId] AS [CollectionRootId0], [a].[Int] AS [Int2], [a].[Ints] AS [Ints2], [a].[Name] AS [Name3], [a].[OptionalNestedAssociateId] AS [OptionalNestedAssociateId0], [a].[RequiredNestedAssociateId] AS [RequiredNestedAssociateId0], [a].[String] AS [String2], [n1].[Id] AS [Id4], [n1].[CollectionAssociateId] AS [CollectionAssociateId1], [n1].[Int] AS [Int3], [n1].[Ints] AS [Ints3], [n1].[Name] AS [Name4], [n1].[String] AS [String3], [n2].[Id] AS [Id5], [n2].[CollectionAssociateId] AS [CollectionAssociateId2], [n2].[Int] AS [Int4], [n2].[Ints] AS [Ints4], [n2].[Name] AS [Name5], [n2].[String] AS [String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [a].[Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[OptionalAssociateId], [r2].[RequiredAssociateId], [r2].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        INNER JOIN [AssociateType] AS [a] ON [r1].[RequiredAssociateId] = [a].[Id]
        LEFT JOIN [AssociateType] AS [a0] ON [r1].[OptionalAssociateId] = [a0].[Id]
        LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
        LEFT JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
        LEFT JOIN [NestedAssociateType] AS [n1] ON [a].[OptionalNestedAssociateId] = [n1].[Id]
        INNER JOIN [NestedAssociateType] AS [n2] ON [a].[RequiredNestedAssociateId] = [n2].[Id]
    ) AS [s]
    WHERE [s].[row] <= 1
) AS [s0] ON [r3].[Key] = [s0].[Key] OR ([r3].[Key] IS NULL AND [s0].[Key] IS NULL)
LEFT JOIN (
    SELECT [a1].[Id], [a1].[CollectionRootId], [a1].[Int], [a1].[Ints], [a1].[Name], [a1].[OptionalNestedAssociateId], [a1].[RequiredNestedAssociateId], [a1].[String], [n5].[Id] AS [Id0], [n5].[CollectionAssociateId], [n5].[Int] AS [Int0], [n5].[Ints] AS [Ints0], [n5].[Name] AS [Name0], [n5].[String] AS [String0], [n3].[Id] AS [Id1], [n3].[CollectionAssociateId] AS [CollectionAssociateId0], [n3].[Int] AS [Int1], [n3].[Ints] AS [Ints1], [n3].[Name] AS [Name1], [n3].[String] AS [String1], [n4].[Id] AS [Id2], [n4].[CollectionAssociateId] AS [CollectionAssociateId1], [n4].[Int] AS [Int2], [n4].[Ints] AS [Ints2], [n4].[Name] AS [Name2], [n4].[String] AS [String2]
    FROM [AssociateType] AS [a1]
    LEFT JOIN [NestedAssociateType] AS [n3] ON [a1].[OptionalNestedAssociateId] = [n3].[Id]
    INNER JOIN [NestedAssociateType] AS [n4] ON [a1].[RequiredNestedAssociateId] = [n4].[Id]
    LEFT JOIN [NestedAssociateType] AS [n5] ON [a1].[Id] = [n5].[CollectionAssociateId]
) AS [s1] ON [s0].[Id] = [s1].[CollectionRootId]
LEFT JOIN [NestedAssociateType] AS [n6] ON [s0].[Id0] = [n6].[CollectionAssociateId]
LEFT JOIN [NestedAssociateType] AS [n7] ON [s0].[Id3] = [n7].[CollectionAssociateId]
ORDER BY [r3].[Key], [s1].[Id], [s1].[Id0], [n6].[Id]
""");
    }

    public override async Task GroupBy_Max_on_associate_scalar_property()
    {
        await base.GroupBy_Max_on_associate_scalar_property();

        AssertSql(
            """
SELECT [s].[Key], MAX([s].[Int]) AS [Max]
FROM (
    SELECT [a].[Int], [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
    INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
) AS [s]
GROUP BY [s].[Key]
""");
    }

    public override async Task GroupBy_Count_with_predicate_on_associate_scalar_property()
    {
        await base.GroupBy_Count_with_predicate_on_associate_scalar_property();

        AssertSql(
            """
SELECT [s].[Key], COUNT(CASE
    WHEN [s].[Int] = 8 THEN 1
END) AS [Count]
FROM (
    SELECT [a].[Int], [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
    INNER JOIN [AssociateType] AS [a] ON [r].[RequiredAssociateId] = [a].[Id]
) AS [s]
GROUP BY [s].[Key]
""");
    }

    #endregion GroupBy

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
