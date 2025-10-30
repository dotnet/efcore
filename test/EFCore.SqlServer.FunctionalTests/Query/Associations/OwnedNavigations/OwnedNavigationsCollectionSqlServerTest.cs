// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsCollectionSqlServerTest(OwnedNavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedNavigationsCollectionRelationalTestBase<OwnedNavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r8].[AssociateTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Ints], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r5].[AssociateTypeRootEntityId], [r5].[AssociateTypeId], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r6].[AssociateTypeId] AS [AssociateTypeId0], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r7].[AssociateTypeId] AS [AssociateTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Ints] AS [Ints0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Ints] AS [Ints1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Ints] AS [Ints2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r4].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r4].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r4].[Id] = [r7].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[AssociateTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]) = 2
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r8].[AssociateTypeRootEntityId]
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r8].[AssociateTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Ints], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r5].[AssociateTypeRootEntityId], [r5].[AssociateTypeId], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r6].[AssociateTypeId] AS [AssociateTypeId0], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r7].[AssociateTypeId] AS [AssociateTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Ints] AS [Ints0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Ints] AS [Ints1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Ints] AS [Ints2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r4].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r4].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r4].[Id] = [r7].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[AssociateTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] <> 8) = 2
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r8].[AssociateTypeRootEntityId]
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r8].[AssociateTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Ints], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r5].[AssociateTypeRootEntityId], [r5].[AssociateTypeId], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r6].[AssociateTypeId] AS [AssociateTypeId0], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r7].[AssociateTypeId] AS [AssociateTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Ints] AS [Ints0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Ints] AS [Ints1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Ints] AS [Ints2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r4].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r4].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r4].[Id] = [r7].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[AssociateTypeRootEntityId]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r8].[AssociateTypeRootEntityId]
""");
    }

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r2].[RootEntityId], [r3].[AssociateTypeRootEntityId], [r4].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r9].[AssociateTypeRootEntityId], [r9].[Id], [r9].[Int], [r9].[Ints], [r9].[Name], [r9].[String], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r2] ON [r].[Id] = [r2].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r3] ON [r2].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r4] ON [r2].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r5].[RootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r6].[AssociateTypeRootEntityId], [r6].[AssociateTypeId], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r7].[AssociateTypeId] AS [AssociateTypeId0], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r8].[AssociateTypeId] AS [AssociateTypeId1], [r8].[Id] AS [Id0], [r8].[Int] AS [Int0], [r8].[Ints] AS [Ints0], [r8].[Name] AS [Name0], [r8].[String] AS [String0], [r6].[Id] AS [Id1], [r6].[Int] AS [Int1], [r6].[Ints] AS [Ints1], [r6].[Name] AS [Name1], [r6].[String] AS [String1], [r7].[Id] AS [Id2], [r7].[Int] AS [Int2], [r7].[Ints] AS [Ints2], [r7].[Name] AS [Name2], [r7].[String] AS [String2]
    FROM [RelatedCollection] AS [r5]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r6] ON [r5].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r5].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r7] ON [r5].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r5].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r8] ON [r5].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r5].[Id] = [r8].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r9] ON [r2].[RootEntityId] = [r9].[AssociateTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String]
        FROM [RelatedCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId]
    ) AS [r1]) = 2
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r2].[RootEntityId], [r3].[AssociateTypeRootEntityId], [r4].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r9].[AssociateTypeRootEntityId]
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[Id00], [s].[Int00], [s].[Ints00], [s].[Name00], [s].[String00], [s].[AssociateTypeRootEntityId00], [s].[AssociateTypeId00], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r1].[AssociateTypeId] AS [AssociateTypeId0], [r1].[Id0] AS [Id00], [r1].[Int0] AS [Int00], [r1].[Ints0] AS [Ints00], [r1].[Name0] AS [Name00], [r1].[String0] AS [String00], [r1].[AssociateTypeRootEntityId0] AS [AssociateTypeRootEntityId00], [r1].[AssociateTypeId0] AS [AssociateTypeId00], [r1].[Id1], [r1].[Int1], [r1].[Ints1], [r1].[Name1], [r1].[String1]
    FROM (
        SELECT DISTINCT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r2].[AssociateTypeRootEntityId], [r2].[AssociateTypeId], [r2].[Id] AS [Id0], [r2].[Int] AS [Int0], [r2].[Ints] AS [Ints0], [r2].[Name] AS [Name0], [r2].[String] AS [String0], [r3].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r3].[AssociateTypeId] AS [AssociateTypeId0], [r3].[Id] AS [Id1], [r3].[Int] AS [Int1], [r3].[Ints] AS [Ints1], [r3].[Name] AS [Name1], [r3].[String] AS [String1]
        FROM [RelatedCollection] AS [r0]
        LEFT JOIN [RelatedCollection_OptionalNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId] AND [r0].[Id] = [r2].[AssociateTypeId]
        LEFT JOIN [RelatedCollection_RequiredNested] AS [r3] ON [r0].[RootEntityId] = [r3].[AssociateTypeRootEntityId] AND [r0].[Id] = [r3].[AssociateTypeId]
        WHERE [r].[Id] = [r0].[RootEntityId]
    ) AS [r1]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r1].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r1].[Id] = [r4].[AssociateTypeId]
) AS [s]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId]
""");
        }
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
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r8].[AssociateTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Ints], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Ints], [r4].[Name], [r4].[String], [r5].[AssociateTypeRootEntityId], [r5].[AssociateTypeId], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r6].[AssociateTypeId] AS [AssociateTypeId0], [r7].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r7].[AssociateTypeId] AS [AssociateTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Ints] AS [Ints0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Ints] AS [Ints1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Ints] AS [Ints2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r4].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r4].[Id] = [r6].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r4].[Id] = [r7].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[AssociateTypeRootEntityId]
WHERE 16 IN (
    SELECT COALESCE(SUM([r0].[Int]), 0)
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    GROUP BY [r0].[String]
)
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[RootEntityId], [r2].[AssociateTypeRootEntityId], [r3].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r8].[AssociateTypeRootEntityId]
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
    FROM [RelatedCollection] AS [r0]
    OUTER APPLY (
        SELECT MAX([r1].[Int]) AS [value]
        FROM [RelatedCollection_NestedCollection] AS [r1]
        WHERE [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
    ) AS [s]
    WHERE [r].[Id] = [r0].[RootEntityId])
FROM [RootEntity] AS [r]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
