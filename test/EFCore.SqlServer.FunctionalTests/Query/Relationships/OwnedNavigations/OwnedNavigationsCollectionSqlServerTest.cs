// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedNavigationsCollectionSqlServerTest(OwnedNavigationsSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedNavigationsCollectionRelationalTestBase<OwnedNavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r8].[RelatedTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r5].[RelatedTypeRootEntityId], [r5].[RelatedTypeId], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r6].[RelatedTypeId] AS [RelatedTypeId0], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r7].[RelatedTypeId] AS [RelatedTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r4].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r4].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r4].[Id] = [r7].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[RelatedTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]) = 2
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r8].[RelatedTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r5].[RelatedTypeRootEntityId], [r5].[RelatedTypeId], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r6].[RelatedTypeId] AS [RelatedTypeId0], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r7].[RelatedTypeId] AS [RelatedTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r4].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r4].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r4].[Id] = [r7].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[RelatedTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] <> 8) = 2
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r8].[RelatedTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r5].[RelatedTypeRootEntityId], [r5].[RelatedTypeId], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r6].[RelatedTypeId] AS [RelatedTypeId0], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r7].[RelatedTypeId] AS [RelatedTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r4].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r4].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r4].[Id] = [r7].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[RelatedTypeRootEntityId]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Index_constant()
    {
        await base.Index_constant();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r8].[RelatedTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r5].[RelatedTypeRootEntityId], [r5].[RelatedTypeId], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r6].[RelatedTypeId] AS [RelatedTypeId0], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r7].[RelatedTypeId] AS [RelatedTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r4].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r4].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r4].[Id] = [r7].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[RelatedTypeRootEntityId]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    ORDER BY (SELECT 1)
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql(
            """
@i='?' (DbType = Int32)

SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r8].[RelatedTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r5].[RelatedTypeRootEntityId], [r5].[RelatedTypeId], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r6].[RelatedTypeId] AS [RelatedTypeId0], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r7].[RelatedTypeId] AS [RelatedTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r4].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r4].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r4].[Id] = [r7].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[RelatedTypeRootEntityId]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    ORDER BY (SELECT 1)
    OFFSET @i ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r8].[RelatedTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r5].[RelatedTypeRootEntityId], [r5].[RelatedTypeId], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r6].[RelatedTypeId] AS [RelatedTypeId0], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r7].[RelatedTypeId] AS [RelatedTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r4].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r4].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r4].[Id] = [r7].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[RelatedTypeRootEntityId]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    ORDER BY (SELECT 1)
    OFFSET [r].[Id] - 1 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r8].[RelatedTypeRootEntityId], [r8].[Id], [r8].[Int], [r8].[Name], [r8].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r1].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r3] ON [r1].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r4].[RootEntityId], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String], [r5].[RelatedTypeRootEntityId], [r5].[RelatedTypeId], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r6].[RelatedTypeId] AS [RelatedTypeId0], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r7].[RelatedTypeId] AS [RelatedTypeId1], [r7].[Id] AS [Id0], [r7].[Int] AS [Int0], [r7].[Name] AS [Name0], [r7].[String] AS [String0], [r5].[Id] AS [Id1], [r5].[Int] AS [Int1], [r5].[Name] AS [Name1], [r5].[String] AS [String1], [r6].[Id] AS [Id2], [r6].[Int] AS [Int2], [r6].[Name] AS [Name2], [r6].[String] AS [String2]
    FROM [RelatedCollection] AS [r4]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r5] ON [r4].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r4].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r6] ON [r4].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r4].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r7] ON [r4].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r4].[Id] = [r7].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r8] ON [r1].[RootEntityId] = [r8].[RelatedTypeRootEntityId]
WHERE (
    SELECT [r0].[Int]
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    ORDER BY (SELECT 1)
    OFFSET 9999 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
