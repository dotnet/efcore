// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public class NavigationsCollectionSqlServerTest(NavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : NavigationsCollectionRelationalTestBase<NavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
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
WHERE (
    SELECT COUNT(*)
    FROM [AssociateType] AS [a]
    WHERE [r].[Id] = [a].[CollectionRootId]) = 2
ORDER BY [r].[Id], [a0].[Id], [n].[Id], [n0].[Id], [a1].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
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
WHERE (
    SELECT COUNT(*)
    FROM [AssociateType] AS [a]
    WHERE [r].[Id] = [a].[CollectionRootId] AND [a].[Int] <> 8) = 2
ORDER BY [r].[Id], [a0].[Id], [n].[Id], [n0].[Id], [a1].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
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
WHERE (
    SELECT [a].[Int]
    FROM [AssociateType] AS [a]
    WHERE [r].[Id] = [a].[CollectionRootId]
    ORDER BY [a].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [a0].[Id], [n].[Id], [n0].[Id], [a1].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

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
        SELECT DISTINCT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String]
        FROM [AssociateType] AS [a]
        WHERE [r].[Id] = [a].[CollectionRootId]
    ) AS [a0]) = 2
ORDER BY [r].[Id], [a1].[Id], [n].[Id], [n0].[Id], [a2].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Ints], [s].[Name], [s].[OptionalNestedAssociateId], [s].[RequiredNestedAssociateId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionAssociateId], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[CollectionAssociateId0], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[CollectionAssociateId1], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT [a0].[Id], [a0].[CollectionRootId], [a0].[Int], [a0].[Ints], [a0].[Name], [a0].[OptionalNestedAssociateId], [a0].[RequiredNestedAssociateId], [a0].[String], [n].[Id] AS [Id0], [n0].[Id] AS [Id1], [n1].[Id] AS [Id2], [n1].[CollectionAssociateId], [n1].[Int] AS [Int0], [n1].[Ints] AS [Ints0], [n1].[Name] AS [Name0], [n1].[String] AS [String0], [n].[CollectionAssociateId] AS [CollectionAssociateId0], [n].[Int] AS [Int1], [n].[Ints] AS [Ints1], [n].[Name] AS [Name1], [n].[String] AS [String1], [n0].[CollectionAssociateId] AS [CollectionAssociateId1], [n0].[Int] AS [Int2], [n0].[Ints] AS [Ints2], [n0].[Name] AS [Name2], [n0].[String] AS [String2]
    FROM (
        SELECT DISTINCT [a].[Id], [a].[CollectionRootId], [a].[Int], [a].[Ints], [a].[Name], [a].[OptionalNestedAssociateId], [a].[RequiredNestedAssociateId], [a].[String]
        FROM [AssociateType] AS [a]
        WHERE [r].[Id] = [a].[CollectionRootId]
    ) AS [a0]
    LEFT JOIN [NestedAssociateType] AS [n] ON [a0].[OptionalNestedAssociateId] = [n].[Id]
    INNER JOIN [NestedAssociateType] AS [n0] ON [a0].[RequiredNestedAssociateId] = [n0].[Id]
    LEFT JOIN [NestedAssociateType] AS [n1] ON [a0].[Id] = [n1].[CollectionAssociateId]
) AS [s]
ORDER BY [r].[Id], [s].[Id], [s].[Id0], [s].[Id1]
""");
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        await base.Distinct_over_projected_nested_collection();

        AssertSql();
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        await base.Distinct_over_projected_filtered_nested_collection();

        AssertSql();
    }

    #endregion Distinct

    #region Index

    public override async Task Index_constant()
    {
        await base.Index_constant();

        AssertSql();
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql();
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        AssertSql();
    }

    public override async Task Index_on_nested_collection()
    {
        await base.Index_on_nested_collection();

        AssertSql();
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql();
    }

    #endregion Index

    #region GroupBy

    [ConditionalFact]
    public override async Task GroupBy()
    {
        await base.GroupBy();

        AssertSql(
            """
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
WHERE 16 IN (
    SELECT COALESCE(SUM([a].[Int]), 0)
    FROM [AssociateType] AS [a]
    WHERE [r].[Id] = [a].[CollectionRootId]
    GROUP BY [a].[String]
)
ORDER BY [r].[Id], [a0].[Id], [n].[Id], [n0].[Id], [a1].[Id], [n1].[Id], [n2].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2], [n6].[Id]
""");
    }

    #endregion GroupBy

    public override async Task Select_within_Select_within_Select_with_aggregates()
    {
        await base.Select_within_Select_within_Select_with_aggregates();

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM [AssociateType] AS [a]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM [NestedAssociateType] AS [n]
        WHERE [a].[Id] = [n].[CollectionAssociateId]
    ) AS [s]
    WHERE [r].[Id] = [a].[CollectionRootId])
FROM [RootEntity] AS [r]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
