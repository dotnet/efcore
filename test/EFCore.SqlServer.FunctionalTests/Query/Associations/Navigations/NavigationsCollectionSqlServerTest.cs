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
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n] ON [r1].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r2] ON [r].[RequiredRelatedId] = [r2].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r2].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r2].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r1].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r3].[Id], [r3].[CollectionRootId], [r3].[Int], [r3].[Name], [r3].[OptionalNestedId], [r3].[RequiredNestedId], [r3].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r3]
    LEFT JOIN [NestedType] AS [n4] ON [r3].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r3].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r3].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r2].[Id] = [n7].[CollectionRelatedId]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId]) = 2
ORDER BY [r].[Id], [r1].[Id], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n] ON [r1].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r2] ON [r].[RequiredRelatedId] = [r2].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r2].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r2].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r1].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r3].[Id], [r3].[CollectionRootId], [r3].[Int], [r3].[Name], [r3].[OptionalNestedId], [r3].[RequiredNestedId], [r3].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r3]
    LEFT JOIN [NestedType] AS [n4] ON [r3].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r3].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r3].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r2].[Id] = [n7].[CollectionRelatedId]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId] AND [r0].[Int] <> 8) = 2
ORDER BY [r].[Id], [r1].[Id], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n] ON [r1].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r2] ON [r].[RequiredRelatedId] = [r2].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r2].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r2].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r1].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r3].[Id], [r3].[CollectionRootId], [r3].[Int], [r3].[Name], [r3].[OptionalNestedId], [r3].[RequiredNestedId], [r3].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r3]
    LEFT JOIN [NestedType] AS [n4] ON [r3].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r3].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r3].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r2].[Id] = [n7].[CollectionRelatedId]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [r1].[Id], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

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
        SELECT DISTINCT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
        FROM [RelatedType] AS [r0]
        WHERE [r].[Id] = [r0].[CollectionRootId]
    ) AS [r1]) = 2
ORDER BY [r].[Id], [r2].[Id], [n].[Id], [n0].[Id], [r3].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        AssertSql(
            """
SELECT [r].[Id], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n].[Id] AS [Id0], [n0].[Id] AS [Id1], [n1].[Id] AS [Id2], [n1].[CollectionRelatedId], [n1].[Int] AS [Int0], [n1].[Name] AS [Name0], [n1].[String] AS [String0], [n].[CollectionRelatedId] AS [CollectionRelatedId0], [n].[Int] AS [Int1], [n].[Name] AS [Name1], [n].[String] AS [String1], [n0].[CollectionRelatedId] AS [CollectionRelatedId1], [n0].[Int] AS [Int2], [n0].[Name] AS [Name2], [n0].[String] AS [String2]
    FROM (
        SELECT DISTINCT [r0].[Id], [r0].[CollectionRootId], [r0].[Int], [r0].[Name], [r0].[OptionalNestedId], [r0].[RequiredNestedId], [r0].[String]
        FROM [RelatedType] AS [r0]
        WHERE [r].[Id] = [r0].[CollectionRootId]
    ) AS [r1]
    LEFT JOIN [NestedType] AS [n] ON [r1].[OptionalNestedId] = [n].[Id]
    INNER JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
    LEFT JOIN [NestedType] AS [n1] ON [r1].[Id] = [n1].[CollectionRelatedId]
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
SELECT [r].[Id], [r].[Name], [r].[OptionalRelatedId], [r].[RequiredRelatedId], [r1].[Id], [r1].[CollectionRootId], [r1].[Int], [r1].[Name], [r1].[OptionalNestedId], [r1].[RequiredNestedId], [r1].[String], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [n3].[CollectionRelatedId], [n3].[Int], [n3].[Name], [n3].[String], [n].[CollectionRelatedId], [n].[Int], [n].[Name], [n].[String], [n0].[CollectionRelatedId], [n0].[Int], [n0].[Name], [n0].[String], [s].[Id], [s].[CollectionRootId], [s].[Int], [s].[Name], [s].[OptionalNestedId], [s].[RequiredNestedId], [s].[String], [s].[Id0], [s].[Id1], [s].[Id2], [s].[CollectionRelatedId], [s].[Int0], [s].[Name0], [s].[String0], [s].[CollectionRelatedId0], [s].[Int1], [s].[Name1], [s].[String1], [s].[CollectionRelatedId1], [s].[Int2], [s].[Name2], [s].[String2], [r2].[CollectionRootId], [r2].[Int], [r2].[Name], [r2].[OptionalNestedId], [r2].[RequiredNestedId], [r2].[String], [n7].[Id], [n7].[CollectionRelatedId], [n7].[Int], [n7].[Name], [n7].[String], [n1].[CollectionRelatedId], [n1].[Int], [n1].[Name], [n1].[String], [n2].[CollectionRelatedId], [n2].[Int], [n2].[Name], [n2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RelatedType] AS [r1] ON [r].[OptionalRelatedId] = [r1].[Id]
LEFT JOIN [NestedType] AS [n] ON [r1].[OptionalNestedId] = [n].[Id]
LEFT JOIN [NestedType] AS [n0] ON [r1].[RequiredNestedId] = [n0].[Id]
INNER JOIN [RelatedType] AS [r2] ON [r].[RequiredRelatedId] = [r2].[Id]
LEFT JOIN [NestedType] AS [n1] ON [r2].[OptionalNestedId] = [n1].[Id]
INNER JOIN [NestedType] AS [n2] ON [r2].[RequiredNestedId] = [n2].[Id]
LEFT JOIN [NestedType] AS [n3] ON [r1].[Id] = [n3].[CollectionRelatedId]
LEFT JOIN (
    SELECT [r3].[Id], [r3].[CollectionRootId], [r3].[Int], [r3].[Name], [r3].[OptionalNestedId], [r3].[RequiredNestedId], [r3].[String], [n4].[Id] AS [Id0], [n5].[Id] AS [Id1], [n6].[Id] AS [Id2], [n6].[CollectionRelatedId], [n6].[Int] AS [Int0], [n6].[Name] AS [Name0], [n6].[String] AS [String0], [n4].[CollectionRelatedId] AS [CollectionRelatedId0], [n4].[Int] AS [Int1], [n4].[Name] AS [Name1], [n4].[String] AS [String1], [n5].[CollectionRelatedId] AS [CollectionRelatedId1], [n5].[Int] AS [Int2], [n5].[Name] AS [Name2], [n5].[String] AS [String2]
    FROM [RelatedType] AS [r3]
    LEFT JOIN [NestedType] AS [n4] ON [r3].[OptionalNestedId] = [n4].[Id]
    INNER JOIN [NestedType] AS [n5] ON [r3].[RequiredNestedId] = [n5].[Id]
    LEFT JOIN [NestedType] AS [n6] ON [r3].[Id] = [n6].[CollectionRelatedId]
) AS [s] ON [r].[Id] = [s].[CollectionRootId]
LEFT JOIN [NestedType] AS [n7] ON [r2].[Id] = [n7].[CollectionRelatedId]
WHERE 16 IN (
    SELECT COALESCE(SUM([r0].[Int]), 0)
    FROM [RelatedType] AS [r0]
    WHERE [r].[Id] = [r0].[CollectionRootId]
    GROUP BY [r0].[String]
)
ORDER BY [r].[Id], [r1].[Id], [n].[Id], [n0].[Id], [r2].[Id], [n1].[Id], [n2].[Id], [n3].[Id], [s].[Id], [s].[Id0], [s].[Id1], [s].[Id2]
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
    FROM [RelatedType] AS [r0]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM [NestedType] AS [n]
        WHERE [r0].[Id] = [n].[CollectionRelatedId]
    ) AS [s]
    WHERE [r].[Id] = [r0].[CollectionRootId])
FROM [RootEntity] AS [r]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
