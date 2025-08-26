// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsMiscellaneousSqlServerTest(
    OwnedNavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedNavigationsMiscellaneousRelationalTestBase<OwnedNavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_related_property()
    {
        await base.Where_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [r0].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r7].[RelatedTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[RelatedTypeRootEntityId], [r4].[RelatedTypeId], [r5].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r5].[RelatedTypeId] AS [RelatedTypeId0], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r6].[RelatedTypeId] AS [RelatedTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[RelatedTypeRootEntityId] AND [r3].[Id] = [r4].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r3].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r3].[Id] = [r6].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[RelatedTypeRootEntityId]
WHERE [r0].[Int] = 8
ORDER BY [r].[Id], [r0].[RootEntityId], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r7].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Where_optional_related_property()
    {
        await base.Where_optional_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r7].[RelatedTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[RelatedTypeRootEntityId], [r4].[RelatedTypeId], [r5].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r5].[RelatedTypeId] AS [RelatedTypeId0], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r6].[RelatedTypeId] AS [RelatedTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[RelatedTypeRootEntityId] AND [r3].[Id] = [r4].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r3].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r3].[Id] = [r6].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[RelatedTypeRootEntityId]
WHERE [o].[Int] = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r7].[RelatedTypeRootEntityId]
""");
    }

    public override async Task Where_nested_related_property()
    {
        await base.Where_nested_related_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o].[Id], [o].[Int], [o].[Name], [o].[String], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Name], [o1].[String], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Name], [s].[String], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [s].[Int0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Name2], [s].[String2], [r0].[Id], [r0].[Int], [r0].[Name], [r0].[String], [r7].[RelatedTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Name], [r7].[String], [r2].[Id], [r2].[Int], [r2].[Name], [r2].[String], [r1].[Id], [r1].[Int], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r1] ON [r0].[RootEntityId] = [r1].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[RelatedTypeRootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r0].[RootEntityId] = [r2].[RelatedTypeRootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[RelatedTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Name], [r3].[String], [r4].[RelatedTypeRootEntityId], [r4].[RelatedTypeId], [r5].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId0], [r5].[RelatedTypeId] AS [RelatedTypeId0], [r6].[RelatedTypeRootEntityId] AS [RelatedTypeRootEntityId1], [r6].[RelatedTypeId] AS [RelatedTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[RelatedTypeRootEntityId] AND [r3].[Id] = [r4].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[RelatedTypeRootEntityId] AND [r3].[Id] = [r5].[RelatedTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[RelatedTypeRootEntityId] AND [r3].[Id] = [r6].[RelatedTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[RelatedTypeRootEntityId]
WHERE [r1].[Int] = 8
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[RelatedTypeRootEntityId], [o].[RootEntityId], [o0].[RelatedTypeRootEntityId], [o1].[RelatedTypeRootEntityId], [r2].[RelatedTypeRootEntityId], [o2].[RelatedTypeRootEntityId], [o2].[Id], [s].[RootEntityId], [s].[Id], [s].[RelatedTypeRootEntityId], [s].[RelatedTypeId], [s].[RelatedTypeRootEntityId0], [s].[RelatedTypeId0], [s].[RelatedTypeRootEntityId1], [s].[RelatedTypeId1], [s].[Id0], [r7].[RelatedTypeRootEntityId]
""");
    }

    #endregion Simple filters

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
