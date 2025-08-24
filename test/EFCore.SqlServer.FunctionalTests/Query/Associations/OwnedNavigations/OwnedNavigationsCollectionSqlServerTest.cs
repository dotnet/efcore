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

    #region Distinct

    public override async Task Distinct()
    {
        await base.Distinct();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r2].[RootEntityId], [r3].[RelatedTypeRootEntityId], [r4].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r9].[RelatedTypeRootEntityId], [r9].[Id], [r9].[Int], [r9].[Name], [r9].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r2] ON [r].[Id] = [r2].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r3] ON [r2].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r4] ON [r2].[RootEntityId] = [r4].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r5].[RootEntityId], [r5].[Id], [r5].[Int], [r5].[Name], [r5].[String], [r6].[RelatedTypeRootEntityId], [r6].[RelatedTypeId], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r7].[RelatedTypeId] AS [RelatedTypeId0], [r8].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r8].[RelatedTypeId] AS [RelatedTypeId1], [r8].[Id] AS [Id0], [r8].[Int] AS [Int0], [r8].[Name] AS [Name0], [r8].[String] AS [String0], [r6].[Id] AS [Id1], [r6].[Int] AS [Int1], [r6].[Name] AS [Name1], [r6].[String] AS [String1], [r7].[Id] AS [Id2], [r7].[Int] AS [Int2], [r7].[Name] AS [Name2], [r7].[String] AS [String2]
    FROM [RelatedCollection] AS [r5]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r6] ON [r5].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r5].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r7] ON [r5].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r5].[Id] = [r7].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r8] ON [r5].[RootEntityId] = [r8].[RelatedTypeRootEntityId] AND [r5].[Id] = [r8].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r9] ON [r2].[RootEntityId] = [r9].[RelatedTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String]
        FROM [RelatedCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId]
    ) AS [r1]) = 2
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r2].[RootEntityId], [r3].[RelatedTypeRootEntityId], [r4].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r9].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[Id00], [s].[Int00], [s].[Name00], [s].[String00], [s].[RelatedTypeRootEntityId00], [s].[RelatedTypeId00], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1]
FROM [RootEntity] AS [r]
OUTER APPLY (
    SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r4].[RelatedTypeRootEntityId], [r4].[RelatedTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r1].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r1].[RelatedTypeId] AS [RelatedTypeId0], [r1].[Id0] AS [Id00], [r1].[Int0] AS [Int00], [r1].[Name0] AS [Name00], [r1].[String0] AS [String00], [r1].[RelatedTypeRootEntityId0] AS [RelatedTypeRootEntityId00], [r1].[RelatedTypeId0] AS [RelatedTypeId00], [r1].[Id1], [r1].[Int1], [r1].[Name1], [r1].[String1]
    FROM (
        SELECT DISTINCT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r2].[RelatedTypeRootEntityId], [r2].[RelatedTypeId], [r2].[Id] AS [Id0], [r2].[Int] AS [Int0], [r2].[Name] AS [Name0], [r2].[String] AS [String0], [r3].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r3].[RelatedTypeId] AS [RelatedTypeId0], [r3].[Id] AS [Id1], [r3].[Int] AS [Int1], [r3].[Name] AS [Name1], [r3].[String] AS [String1]
        FROM [RelatedCollection] AS [r0]
        LEFT JOIN [RelatedCollection_OptionalNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r0].[Id] = [r2].[RelatedTypeId]
        LEFT JOIN [RelatedCollection_RequiredNested] AS [r3] ON [r0].[RootEntityId] = [r3].[RelatedTypeRootEntityId] AND [r0].[Id] = [r3].[RelatedTypeId]
        WHERE [r].[Id] = [r0].[RootEntityId]
    ) AS [r1]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r1].[RootEntityId] = [r4].[RelatedTypeRootEntityId] AND [r1].[Id] = [r4].[RelatedTypeId]
) AS [s]
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId]
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
WHERE 16 IN (
    SELECT COALESCE(SUM([r0].[Int]), 0)
    FROM [RelatedCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    GROUP BY [r0].[String]
)
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RootEntityId], [r2].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r8].[RelatedTypeRootEntityId]
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
        WHERE [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r0].[Id] = [r1].[RelatedTypeId]
    ) AS [s]
    WHERE [r].[Id] = [r0].[RootEntityId])
FROM [RootEntity] AS [r]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
