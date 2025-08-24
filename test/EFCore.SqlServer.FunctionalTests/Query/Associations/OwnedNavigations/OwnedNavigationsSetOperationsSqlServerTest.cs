// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsSetOperationsSqlServerTest(
    OwnedNavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedNavigationsSetOperationsRelationalTestBase<OwnedNavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task On_related()
    {
        await base.On_related();

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
        SELECT 1 AS empty
        FROM [RelatedCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM [RelatedCollection] AS [r1]
        WHERE [r].[Id] = [r1].[RootEntityId] AND [r1].[String] = N'foo'
    ) AS [u]) = 4
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r2].[RootEntityId], [r3].[RelatedTypeRootEntityId], [r4].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r9].[RelatedTypeRootEntityId]
""");
    }

    public override Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
        => Assert.ThrowsAnyAsync<Exception>(() => base.On_related_projected(queryTrackingBehavior));

    public override async Task On_related_Select_nested_with_aggregates(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.On_related_Select_nested_with_aggregates(queryTrackingBehavior);

        AssertSql(
            """
SELECT (
    SELECT COALESCE(SUM([s].[value]), 0)
    FROM (
        SELECT [r0].[RootEntityId], [r0].[Id]
        FROM [RelatedCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] = 8
        UNION ALL
        SELECT [r1].[RootEntityId], [r1].[Id]
        FROM [RelatedCollection] AS [r1]
        WHERE [r].[Id] = [r1].[RootEntityId] AND [r1].[String] = N'foo'
    ) AS [u]
    OUTER APPLY (
        SELECT COALESCE(SUM([r2].[Int]), 0) AS [value]
        FROM [RelatedCollection_NestedCollection] AS [r2]
        WHERE [u].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [u].[Id] = [r2].[RelatedTypeId]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
    }

    public override async Task On_nested()
    {
        await base.On_nested();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [r0].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [r4].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r9].[RelatedTypeRootEntityId], [r9].[Id], [r9].[Int], [r9].[Name], [r9].[String], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[Id], [r4].[Int], [r4].[Name], [r4].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r3] ON [r0].[RootEntityId] = [r3].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r4] ON [r0].[RootEntityId] = [r4].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r5].[RootEntityId], [r5].[Id], [r5].[Int], [r5].[Name], [r5].[String], [r6].[RelatedTypeRootEntityId], [r6].[RelatedTypeId], [r7].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r7].[RelatedTypeId] AS [RelatedTypeId0], [r8].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r8].[RelatedTypeId] AS [RelatedTypeId1], [r8].[Id] AS [Id0], [r8].[Int] AS [Int0], [r8].[Name] AS [Name0], [r8].[String] AS [String0], [r6].[Id] AS [Id1], [r6].[Int] AS [Int1], [r6].[Name] AS [Name1], [r6].[String] AS [String1], [r7].[Id] AS [Id2], [r7].[Int] AS [Int2], [r7].[Name] AS [Name2], [r7].[String] AS [String2]
    FROM [RelatedCollection] AS [r5]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r6] ON [r5].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r5].[Id] = [r6].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r7] ON [r5].[RootEntityId] = [r7].[RelatedTypeRootEntityId] AND [r5].[Id] = [r7].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r8] ON [r5].[RootEntityId] = [r8].[RelatedTypeRootEntityId] AND [r5].[Id] = [r8].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r9] ON [r0].[RootEntityId] = [r9].[RelatedTypeRootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [RequiredRelated_NestedCollection] AS [r1]
        WHERE [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId] AND [r1].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM [RequiredRelated_NestedCollection] AS [r2]
        WHERE [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId] AND [r2].[String] = N'foo'
    ) AS [u]) = 4
ORDER BY [r].[Id], [r0].[RootEntityId], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r3].[RelatedTypeRootEntityId], [r4].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r9].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Over_different_collection_properties()
    {
        await base.Over_different_collection_properties();

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
