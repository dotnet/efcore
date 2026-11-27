// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsMiscellaneousSqlServerTest(
    OwnedNavigationsSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedNavigationsMiscellaneousRelationalTestBase<OwnedNavigationsSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[RootEntityId], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r7].[AssociateTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Ints], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r5].[AssociateTypeId] AS [AssociateTypeId0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r6].[AssociateTypeId] AS [AssociateTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Ints] AS [Ints0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Ints] AS [Ints1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Ints] AS [Ints2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r3].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r3].[Id] = [r6].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[AssociateTypeRootEntityId]
WHERE [r0].[Int] = 8
ORDER BY [r].[Id], [r0].[RootEntityId], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r7].[AssociateTypeRootEntityId], [r7].[Id]
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r7].[AssociateTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Ints], [r7].[Name], [r7].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r5].[AssociateTypeId] AS [AssociateTypeId0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r6].[AssociateTypeId] AS [AssociateTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Ints] AS [Ints0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Ints] AS [Ints1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Ints] AS [Ints2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r3].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r3].[Id] = [r6].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[AssociateTypeRootEntityId]
WHERE [o].[Int] = 8
ORDER BY [r].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r7].[AssociateTypeRootEntityId], [r7].[Id]
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r7].[AssociateTypeRootEntityId], [r7].[Id], [r7].[Int], [r7].[Ints], [r7].[Name], [r7].[String], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String]
FROM [RootEntity] AS [r]
LEFT JOIN [RequiredRelated] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated] AS [o] ON [r].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r2] ON [r0].[RootEntityId] = [r2].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r5].[AssociateTypeId] AS [AssociateTypeId0], [r6].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r6].[AssociateTypeId] AS [AssociateTypeId1], [r6].[Id] AS [Id0], [r6].[Int] AS [Int0], [r6].[Ints] AS [Ints0], [r6].[Name] AS [Name0], [r6].[String] AS [String0], [r4].[Id] AS [Id1], [r4].[Int] AS [Int1], [r4].[Ints] AS [Ints1], [r4].[Name] AS [Name1], [r4].[String] AS [String1], [r5].[Id] AS [Id2], [r5].[Int] AS [Int2], [r5].[Ints] AS [Ints2], [r5].[Name] AS [Name2], [r5].[String] AS [String2]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r3].[Id] = [r5].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r6] ON [r3].[RootEntityId] = [r6].[AssociateTypeRootEntityId] AND [r3].[Id] = [r6].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r7] ON [r0].[RootEntityId] = [r7].[AssociateTypeRootEntityId]
WHERE [r1].[Int] = 8
ORDER BY [r].[Id], [r0].[RootEntityId], [r1].[AssociateTypeRootEntityId], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r2].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r7].[AssociateTypeRootEntityId], [r7].[Id]
""");
    }

    #endregion Simple filters

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r].[RootEntityId], [r0].[AssociateTypeRootEntityId], [r1].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [o0].[Id], [o0].[Int], [o0].[Ints], [o0].[Name], [o0].[String], [o1].[Id], [o1].[Int], [o1].[Ints], [o1].[Name], [o1].[String], [r].[Id], [r].[Int], [r].[Ints], [r].[Name], [r].[String], [r6].[AssociateTypeRootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String]
FROM (
    SELECT * FROM [RootEntity]
) AS [m]
LEFT JOIN [OptionalRelated] AS [o] ON [m].[Id] = [o].[RootEntityId]
LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated] AS [r] ON [m].[Id] = [r].[RootEntityId]
LEFT JOIN [RequiredRelated_OptionalNested] AS [r0] ON [r].[RootEntityId] = [r0].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_RequiredNested] AS [r1] ON [r].[RootEntityId] = [r1].[AssociateTypeRootEntityId]
LEFT JOIN (
    SELECT [r2].[RootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r3].[AssociateTypeRootEntityId], [r3].[AssociateTypeId], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r4].[AssociateTypeId] AS [AssociateTypeId0], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r5].[AssociateTypeId] AS [AssociateTypeId1], [r5].[Id] AS [Id0], [r5].[Int] AS [Int0], [r5].[Ints] AS [Ints0], [r5].[Name] AS [Name0], [r5].[String] AS [String0], [r3].[Id] AS [Id1], [r3].[Int] AS [Int1], [r3].[Ints] AS [Ints1], [r3].[Name] AS [Name1], [r3].[String] AS [String1], [r4].[Id] AS [Id2], [r4].[Int] AS [Int2], [r4].[Ints] AS [Ints2], [r4].[Name] AS [Name2], [r4].[String] AS [String2]
    FROM [RelatedCollection] AS [r2]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r3] ON [r2].[RootEntityId] = [r3].[AssociateTypeRootEntityId] AND [r2].[Id] = [r3].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r4] ON [r2].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r2].[Id] = [r4].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r5] ON [r2].[RootEntityId] = [r5].[AssociateTypeRootEntityId] AND [r2].[Id] = [r5].[AssociateTypeId]
) AS [s] ON [m].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [o].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r6] ON [r].[RootEntityId] = [r6].[AssociateTypeRootEntityId]
ORDER BY [m].[Id], [o].[RootEntityId], [o0].[AssociateTypeRootEntityId], [o1].[AssociateTypeRootEntityId], [r].[RootEntityId], [r0].[AssociateTypeRootEntityId], [r1].[AssociateTypeRootEntityId], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r6].[AssociateTypeRootEntityId], [r6].[Id]
""");
    }

    #region GroupBy element

    public override async Task Where_on_root_property_on_GroupBy_First()
    {
        await base.Where_on_root_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r18].[Id], [r18].[Name], [r16].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r18].[RootEntityId], [r18].[Id0], [r18].[Int], [r18].[Ints], [r18].[Name0], [r18].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r18].[AssociateTypeRootEntityId], [r18].[Id1], [r18].[Int0], [r18].[Ints0], [r18].[Name1], [r18].[String0], [r18].[AssociateTypeRootEntityId0], [r18].[Id2], [r18].[Int1], [r18].[Ints1], [r18].[Name2], [r18].[String1], [r18].[RootEntityId0], [r18].[Id3], [r18].[Int2], [r18].[Ints2], [r18].[Name3], [r18].[String2], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r18].[AssociateTypeRootEntityId1], [r18].[Id4], [r18].[Int3], [r18].[Ints3], [r18].[Name4], [r18].[String3], [r18].[AssociateTypeRootEntityId2], [r18].[Id5], [r18].[Int4], [r18].[Ints4], [r18].[Name5], [r18].[String4]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r11].[Id]
        FROM (
            SELECT [r12].[Id], [r12].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r12]
            LEFT JOIN [OptionalRelated] AS [o3] ON [r12].[Id] = [o3].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o4] ON [o3].[RootEntityId] = [o4].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o5] ON [o3].[RootEntityId] = [o5].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r13] ON [r12].[Id] = [r13].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r14] ON [r13].[RootEntityId] = [r14].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r15] ON [r13].[RootEntityId] = [r15].[AssociateTypeRootEntityId]
        ) AS [r11]
        WHERE ([r0].[Key] = [r11].[Key] OR ([r0].[Key] IS NULL AND [r11].[Key] IS NULL)) AND [r11].[Id] > 0
        ORDER BY [r11].[Id]) > 1
) AS [r16]
LEFT JOIN (
    SELECT [r17].[Id], [r17].[Name], [r17].[RootEntityId], [r17].[Id0], [r17].[Int], [r17].[Ints], [r17].[Name0], [r17].[String], [r17].[AssociateTypeRootEntityId], [r17].[Id1], [r17].[Int0], [r17].[Ints0], [r17].[Name1], [r17].[String0], [r17].[AssociateTypeRootEntityId0], [r17].[Id2], [r17].[Int1], [r17].[Ints1], [r17].[Name2], [r17].[String1], [r17].[RootEntityId0], [r17].[Id3], [r17].[Int2], [r17].[Ints2], [r17].[Name3], [r17].[String2], [r17].[AssociateTypeRootEntityId1], [r17].[Id4], [r17].[Int3], [r17].[Ints3], [r17].[Name4], [r17].[String3], [r17].[AssociateTypeRootEntityId2], [r17].[Id5], [r17].[Int4], [r17].[Ints4], [r17].[Name5], [r17].[String4], [r17].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId0], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[RootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [o].[RootEntityId], [o].[Id] AS [Id0], [o].[Int], [o].[Ints], [o].[Name] AS [Name0], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id1], [o0].[Int] AS [Int0], [o0].[Ints] AS [Ints0], [o0].[Name] AS [Name1], [o0].[String] AS [String0], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id2], [o1].[Int] AS [Int1], [o1].[Ints] AS [Ints1], [o1].[Name] AS [Name2], [o1].[String] AS [String1], [r3].[RootEntityId] AS [RootEntityId0], [r3].[Id] AS [Id3], [r3].[Int] AS [Int2], [r3].[Ints] AS [Ints2], [r3].[Name] AS [Name3], [r3].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r17]
    WHERE [r17].[row] <= 1
) AS [r18] ON [r16].[Key] = [r18].[Key] OR ([r16].[Key] IS NULL AND [r18].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r18].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r18].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r18].[RootEntityId0] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r16].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    public override async Task Where_on_associate_scalar_property_on_GroupBy_First()
    {
        await base.Where_on_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r18].[Id], [r18].[Name], [r16].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r18].[RootEntityId], [r18].[Id0], [r18].[Int], [r18].[Ints], [r18].[Name0], [r18].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r18].[AssociateTypeRootEntityId], [r18].[Id1], [r18].[Int0], [r18].[Ints0], [r18].[Name1], [r18].[String0], [r18].[AssociateTypeRootEntityId0], [r18].[Id2], [r18].[Int1], [r18].[Ints1], [r18].[Name2], [r18].[String1], [r18].[RootEntityId0], [r18].[Id3], [r18].[Int2], [r18].[Ints2], [r18].[Name3], [r18].[String2], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r18].[AssociateTypeRootEntityId1], [r18].[Id4], [r18].[Int3], [r18].[Ints3], [r18].[Name4], [r18].[String3], [r18].[AssociateTypeRootEntityId2], [r18].[Id5], [r18].[Int4], [r18].[Ints4], [r18].[Name5], [r18].[String4]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r11].[Int2]
        FROM (
            SELECT [r12].[Id], [r12].[Id] % 2 AS [Key], [r13].[Int] AS [Int2]
            FROM [RootEntity] AS [r12]
            LEFT JOIN [OptionalRelated] AS [o3] ON [r12].[Id] = [o3].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o4] ON [o3].[RootEntityId] = [o4].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o5] ON [o3].[RootEntityId] = [o5].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r13] ON [r12].[Id] = [r13].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r14] ON [r13].[RootEntityId] = [r14].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r15] ON [r13].[RootEntityId] = [r15].[AssociateTypeRootEntityId]
        ) AS [r11]
        WHERE ([r0].[Key] = [r11].[Key] OR ([r0].[Key] IS NULL AND [r11].[Key] IS NULL)) AND [r11].[Id] > 0
        ORDER BY [r11].[Id]) = 8
) AS [r16]
LEFT JOIN (
    SELECT [r17].[Id], [r17].[Name], [r17].[RootEntityId], [r17].[Id0], [r17].[Int], [r17].[Ints], [r17].[Name0], [r17].[String], [r17].[AssociateTypeRootEntityId], [r17].[Id1], [r17].[Int0], [r17].[Ints0], [r17].[Name1], [r17].[String0], [r17].[AssociateTypeRootEntityId0], [r17].[Id2], [r17].[Int1], [r17].[Ints1], [r17].[Name2], [r17].[String1], [r17].[RootEntityId0], [r17].[Id3], [r17].[Int2], [r17].[Ints2], [r17].[Name3], [r17].[String2], [r17].[AssociateTypeRootEntityId1], [r17].[Id4], [r17].[Int3], [r17].[Ints3], [r17].[Name4], [r17].[String3], [r17].[AssociateTypeRootEntityId2], [r17].[Id5], [r17].[Int4], [r17].[Ints4], [r17].[Name5], [r17].[String4], [r17].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId0], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[RootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [o].[RootEntityId], [o].[Id] AS [Id0], [o].[Int], [o].[Ints], [o].[Name] AS [Name0], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id1], [o0].[Int] AS [Int0], [o0].[Ints] AS [Ints0], [o0].[Name] AS [Name1], [o0].[String] AS [String0], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id2], [o1].[Int] AS [Int1], [o1].[Ints] AS [Ints1], [o1].[Name] AS [Name2], [o1].[String] AS [String1], [r3].[RootEntityId] AS [RootEntityId0], [r3].[Id] AS [Id3], [r3].[Int] AS [Int2], [r3].[Ints] AS [Ints2], [r3].[Name] AS [Name3], [r3].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r17]
    WHERE [r17].[row] <= 1
) AS [r18] ON [r16].[Key] = [r18].[Key] OR ([r16].[Key] IS NULL AND [r18].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r18].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r18].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r18].[RootEntityId0] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r16].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    public override async Task Where_on_nested_associate_scalar_property_on_GroupBy_First()
    {
        await base.Where_on_nested_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r18].[Id], [r18].[Name], [r16].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r18].[RootEntityId], [r18].[Id0], [r18].[Int], [r18].[Ints], [r18].[Name0], [r18].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r18].[AssociateTypeRootEntityId], [r18].[Id1], [r18].[Int0], [r18].[Ints0], [r18].[Name1], [r18].[String0], [r18].[AssociateTypeRootEntityId0], [r18].[Id2], [r18].[Int1], [r18].[Ints1], [r18].[Name2], [r18].[String1], [r18].[RootEntityId0], [r18].[Id3], [r18].[Int2], [r18].[Ints2], [r18].[Name3], [r18].[String2], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r18].[AssociateTypeRootEntityId1], [r18].[Id4], [r18].[Int3], [r18].[Ints3], [r18].[Name4], [r18].[String3], [r18].[AssociateTypeRootEntityId2], [r18].[Id5], [r18].[Int4], [r18].[Ints4], [r18].[Name5], [r18].[String4]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r11].[Int4]
        FROM (
            SELECT [r12].[Id], [r12].[Id] % 2 AS [Key], [r15].[Int] AS [Int4]
            FROM [RootEntity] AS [r12]
            LEFT JOIN [OptionalRelated] AS [o3] ON [r12].[Id] = [o3].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o4] ON [o3].[RootEntityId] = [o4].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o5] ON [o3].[RootEntityId] = [o5].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r13] ON [r12].[Id] = [r13].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r14] ON [r13].[RootEntityId] = [r14].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r15] ON [r13].[RootEntityId] = [r15].[AssociateTypeRootEntityId]
        ) AS [r11]
        WHERE ([r0].[Key] = [r11].[Key] OR ([r0].[Key] IS NULL AND [r11].[Key] IS NULL)) AND [r11].[Id] > 0
        ORDER BY [r11].[Id]) = 8
) AS [r16]
LEFT JOIN (
    SELECT [r17].[Id], [r17].[Name], [r17].[RootEntityId], [r17].[Id0], [r17].[Int], [r17].[Ints], [r17].[Name0], [r17].[String], [r17].[AssociateTypeRootEntityId], [r17].[Id1], [r17].[Int0], [r17].[Ints0], [r17].[Name1], [r17].[String0], [r17].[AssociateTypeRootEntityId0], [r17].[Id2], [r17].[Int1], [r17].[Ints1], [r17].[Name2], [r17].[String1], [r17].[RootEntityId0], [r17].[Id3], [r17].[Int2], [r17].[Ints2], [r17].[Name3], [r17].[String2], [r17].[AssociateTypeRootEntityId1], [r17].[Id4], [r17].[Int3], [r17].[Ints3], [r17].[Name4], [r17].[String3], [r17].[AssociateTypeRootEntityId2], [r17].[Id5], [r17].[Int4], [r17].[Ints4], [r17].[Name5], [r17].[String4], [r17].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId0], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[RootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [o].[RootEntityId], [o].[Id] AS [Id0], [o].[Int], [o].[Ints], [o].[Name] AS [Name0], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id1], [o0].[Int] AS [Int0], [o0].[Ints] AS [Ints0], [o0].[Name] AS [Name1], [o0].[String] AS [String0], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id2], [o1].[Int] AS [Int1], [o1].[Ints] AS [Ints1], [o1].[Name] AS [Name2], [o1].[String] AS [String1], [r3].[RootEntityId] AS [RootEntityId0], [r3].[Id] AS [Id3], [r3].[Int] AS [Int2], [r3].[Ints] AS [Ints2], [r3].[Name] AS [Name3], [r3].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r17]
    WHERE [r17].[row] <= 1
) AS [r18] ON [r16].[Key] = [r18].[Key] OR ([r16].[Key] IS NULL AND [r18].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r18].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r18].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r18].[RootEntityId0] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r16].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    public override async Task Select_multiple_associates_on_GroupBy_OrderByDescending_First()
    {
        await base.Select_multiple_associates_on_GroupBy_OrderByDescending_First();

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [r21].[Id]
    FROM (
        SELECT [r22].[Id], [r22].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r22]
        LEFT JOIN [OptionalRelated] AS [o9] ON [r22].[Id] = [o9].[RootEntityId]
        LEFT JOIN [OptionalRelated_OptionalNested] AS [o10] ON [o9].[RootEntityId] = [o10].[AssociateTypeRootEntityId]
        LEFT JOIN [OptionalRelated_RequiredNested] AS [o11] ON [o9].[RootEntityId] = [o11].[AssociateTypeRootEntityId]
        LEFT JOIN [RequiredRelated] AS [r23] ON [r22].[Id] = [r23].[RootEntityId]
        LEFT JOIN [RequiredRelated_OptionalNested] AS [r24] ON [r23].[RootEntityId] = [r24].[AssociateTypeRootEntityId]
        LEFT JOIN [RequiredRelated_RequiredNested] AS [r25] ON [r23].[RootEntityId] = [r25].[AssociateTypeRootEntityId]
    ) AS [r21]
    WHERE [r26].[Key] = [r21].[Key] OR ([r26].[Key] IS NULL AND [r21].[Key] IS NULL)
    ORDER BY [r21].[Id] DESC), [r28].[Id], [r28].[Name], [r26].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r28].[RootEntityId], [r28].[Id0], [r28].[Int], [r28].[Ints], [r28].[Name0], [r28].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r28].[AssociateTypeRootEntityId], [r28].[Id1], [r28].[Int0], [r28].[Ints0], [r28].[Name1], [r28].[String0], [r28].[AssociateTypeRootEntityId0], [r28].[Id2], [r28].[Int1], [r28].[Ints1], [r28].[Name2], [r28].[String1], [r28].[RootEntityId0], [r28].[Id3], [r28].[Int2], [r28].[Ints2], [r28].[Name3], [r28].[String2], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r28].[AssociateTypeRootEntityId1], [r28].[Id4], [r28].[Int3], [r28].[Ints3], [r28].[Name4], [r28].[String3], [r28].[AssociateTypeRootEntityId2], [r28].[Id5], [r28].[Int4], [r28].[Ints4], [r28].[Name5], [r28].[String4]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r11].[Int2]
        FROM (
            SELECT [r12].[Id], [r12].[Id] % 2 AS [Key], [r13].[Int] AS [Int2]
            FROM [RootEntity] AS [r12]
            LEFT JOIN [OptionalRelated] AS [o3] ON [r12].[Id] = [o3].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o4] ON [o3].[RootEntityId] = [o4].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o5] ON [o3].[RootEntityId] = [o5].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r13] ON [r12].[Id] = [r13].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r14] ON [r13].[RootEntityId] = [r14].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r15] ON [r13].[RootEntityId] = [r15].[AssociateTypeRootEntityId]
        ) AS [r11]
        WHERE [r0].[Key] = [r11].[Key] OR ([r0].[Key] IS NULL AND [r11].[Key] IS NULL)
        ORDER BY [r11].[Id] DESC) AS [c], (
        SELECT TOP(1) [r16].[Id]
        FROM (
            SELECT [r17].[Id], [r17].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r17]
            LEFT JOIN [OptionalRelated] AS [o6] ON [r17].[Id] = [o6].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o7] ON [o6].[RootEntityId] = [o7].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o8] ON [o6].[RootEntityId] = [o8].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r18] ON [r17].[Id] = [r18].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r19] ON [r18].[RootEntityId] = [r19].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r20] ON [r18].[RootEntityId] = [r20].[AssociateTypeRootEntityId]
        ) AS [r16]
        WHERE [r0].[Key] = [r16].[Key] OR ([r0].[Key] IS NULL AND [r16].[Key] IS NULL)
        ORDER BY [r16].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r26]
LEFT JOIN (
    SELECT [r27].[Id], [r27].[Name], [r27].[RootEntityId], [r27].[Id0], [r27].[Int], [r27].[Ints], [r27].[Name0], [r27].[String], [r27].[AssociateTypeRootEntityId], [r27].[Id1], [r27].[Int0], [r27].[Ints0], [r27].[Name1], [r27].[String0], [r27].[AssociateTypeRootEntityId0], [r27].[Id2], [r27].[Int1], [r27].[Ints1], [r27].[Name2], [r27].[String1], [r27].[RootEntityId0], [r27].[Id3], [r27].[Int2], [r27].[Ints2], [r27].[Name3], [r27].[String2], [r27].[AssociateTypeRootEntityId1], [r27].[Id4], [r27].[Int3], [r27].[Ints3], [r27].[Name4], [r27].[String3], [r27].[AssociateTypeRootEntityId2], [r27].[Id5], [r27].[Int4], [r27].[Ints4], [r27].[Name5], [r27].[String4], [r27].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId0], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[RootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [o].[RootEntityId], [o].[Id] AS [Id0], [o].[Int], [o].[Ints], [o].[Name] AS [Name0], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id1], [o0].[Int] AS [Int0], [o0].[Ints] AS [Ints0], [o0].[Name] AS [Name1], [o0].[String] AS [String0], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id2], [o1].[Int] AS [Int1], [o1].[Ints] AS [Ints1], [o1].[Name] AS [Name2], [o1].[String] AS [String1], [r3].[RootEntityId] AS [RootEntityId0], [r3].[Id] AS [Id3], [r3].[Int] AS [Int2], [r3].[Ints] AS [Ints2], [r3].[Name] AS [Name3], [r3].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
    ) AS [r27]
    WHERE [r27].[row] <= 1
) AS [r28] ON [r26].[Key] = [r28].[Key] OR ([r26].[Key] IS NULL AND [r28].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r28].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r28].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r28].[RootEntityId0] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r26].[c] DESC, [r26].[c0], [r26].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    public override async Task Select_multiple_associates_on_GroupBy_MaxBy()
    {
        await base.Select_multiple_associates_on_GroupBy_MaxBy();

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [r21].[Id]
    FROM (
        SELECT [r22].[Id], [r22].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r22]
        LEFT JOIN [OptionalRelated] AS [o9] ON [r22].[Id] = [o9].[RootEntityId]
        LEFT JOIN [OptionalRelated_OptionalNested] AS [o10] ON [o9].[RootEntityId] = [o10].[AssociateTypeRootEntityId]
        LEFT JOIN [OptionalRelated_RequiredNested] AS [o11] ON [o9].[RootEntityId] = [o11].[AssociateTypeRootEntityId]
        LEFT JOIN [RequiredRelated] AS [r23] ON [r22].[Id] = [r23].[RootEntityId]
        LEFT JOIN [RequiredRelated_OptionalNested] AS [r24] ON [r23].[RootEntityId] = [r24].[AssociateTypeRootEntityId]
        LEFT JOIN [RequiredRelated_RequiredNested] AS [r25] ON [r23].[RootEntityId] = [r25].[AssociateTypeRootEntityId]
    ) AS [r21]
    WHERE [r26].[Key] = [r21].[Key] OR ([r26].[Key] IS NULL AND [r21].[Key] IS NULL)
    ORDER BY [r21].[Id] DESC), [r28].[Id], [r28].[Name], [r26].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r28].[RootEntityId], [r28].[Id0], [r28].[Int], [r28].[Ints], [r28].[Name0], [r28].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r28].[AssociateTypeRootEntityId], [r28].[Id1], [r28].[Int0], [r28].[Ints0], [r28].[Name1], [r28].[String0], [r28].[AssociateTypeRootEntityId0], [r28].[Id2], [r28].[Int1], [r28].[Ints1], [r28].[Name2], [r28].[String1], [r28].[RootEntityId0], [r28].[Id3], [r28].[Int2], [r28].[Ints2], [r28].[Name3], [r28].[String2], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r28].[AssociateTypeRootEntityId1], [r28].[Id4], [r28].[Int3], [r28].[Ints3], [r28].[Name4], [r28].[String3], [r28].[AssociateTypeRootEntityId2], [r28].[Id5], [r28].[Int4], [r28].[Ints4], [r28].[Name5], [r28].[String4]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r11].[Int2]
        FROM (
            SELECT [r12].[Id], [r12].[Id] % 2 AS [Key], [r13].[Int] AS [Int2]
            FROM [RootEntity] AS [r12]
            LEFT JOIN [OptionalRelated] AS [o3] ON [r12].[Id] = [o3].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o4] ON [o3].[RootEntityId] = [o4].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o5] ON [o3].[RootEntityId] = [o5].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r13] ON [r12].[Id] = [r13].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r14] ON [r13].[RootEntityId] = [r14].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r15] ON [r13].[RootEntityId] = [r15].[AssociateTypeRootEntityId]
        ) AS [r11]
        WHERE [r0].[Key] = [r11].[Key] OR ([r0].[Key] IS NULL AND [r11].[Key] IS NULL)
        ORDER BY [r11].[Id] DESC) AS [c], (
        SELECT TOP(1) [r16].[Id]
        FROM (
            SELECT [r17].[Id], [r17].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r17]
            LEFT JOIN [OptionalRelated] AS [o6] ON [r17].[Id] = [o6].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o7] ON [o6].[RootEntityId] = [o7].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o8] ON [o6].[RootEntityId] = [o8].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r18] ON [r17].[Id] = [r18].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r19] ON [r18].[RootEntityId] = [r19].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r20] ON [r18].[RootEntityId] = [r20].[AssociateTypeRootEntityId]
        ) AS [r16]
        WHERE [r0].[Key] = [r16].[Key] OR ([r0].[Key] IS NULL AND [r16].[Key] IS NULL)
        ORDER BY [r16].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r26]
LEFT JOIN (
    SELECT [r27].[Id], [r27].[Name], [r27].[RootEntityId], [r27].[Id0], [r27].[Int], [r27].[Ints], [r27].[Name0], [r27].[String], [r27].[AssociateTypeRootEntityId], [r27].[Id1], [r27].[Int0], [r27].[Ints0], [r27].[Name1], [r27].[String0], [r27].[AssociateTypeRootEntityId0], [r27].[Id2], [r27].[Int1], [r27].[Ints1], [r27].[Name2], [r27].[String1], [r27].[RootEntityId0], [r27].[Id3], [r27].[Int2], [r27].[Ints2], [r27].[Name3], [r27].[String2], [r27].[AssociateTypeRootEntityId1], [r27].[Id4], [r27].[Int3], [r27].[Ints3], [r27].[Name4], [r27].[String3], [r27].[AssociateTypeRootEntityId2], [r27].[Id5], [r27].[Int4], [r27].[Ints4], [r27].[Name5], [r27].[String4], [r27].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId0], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[RootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [o].[RootEntityId], [o].[Id] AS [Id0], [o].[Int], [o].[Ints], [o].[Name] AS [Name0], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id1], [o0].[Int] AS [Int0], [o0].[Ints] AS [Ints0], [o0].[Name] AS [Name1], [o0].[String] AS [String0], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id2], [o1].[Int] AS [Int1], [o1].[Ints] AS [Ints1], [o1].[Name] AS [Name2], [o1].[String] AS [String1], [r3].[RootEntityId] AS [RootEntityId0], [r3].[Id] AS [Id3], [r3].[Int] AS [Int2], [r3].[Ints] AS [Ints2], [r3].[Name] AS [Name3], [r3].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
    ) AS [r27]
    WHERE [r27].[row] <= 1
) AS [r28] ON [r26].[Key] = [r28].[Key] OR ([r26].[Key] IS NULL AND [r28].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r28].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r28].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r28].[RootEntityId0] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r26].[c] DESC, [r26].[c0], [r26].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    public override async Task OrderBy_associate_scalar_property_on_GroupBy_First()
    {
        await base.OrderBy_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r23].[Id], [r23].[Name], [r21].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r23].[RootEntityId], [r23].[Id0], [r23].[Int], [r23].[Ints], [r23].[Name0], [r23].[String], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r23].[AssociateTypeRootEntityId], [r23].[Id1], [r23].[Int0], [r23].[Ints0], [r23].[Name1], [r23].[String0], [r23].[AssociateTypeRootEntityId0], [r23].[Id2], [r23].[Int1], [r23].[Ints1], [r23].[Name2], [r23].[String1], [r23].[RootEntityId0], [r23].[Id3], [r23].[Int2], [r23].[Ints2], [r23].[Name3], [r23].[String2], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r23].[AssociateTypeRootEntityId1], [r23].[Id4], [r23].[Int3], [r23].[Ints3], [r23].[Name4], [r23].[String3], [r23].[AssociateTypeRootEntityId2], [r23].[Id5], [r23].[Int4], [r23].[Ints4], [r23].[Name5], [r23].[String4]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r11].[Int2]
        FROM (
            SELECT [r12].[Id], [r12].[Id] % 2 AS [Key], [r13].[Int] AS [Int2]
            FROM [RootEntity] AS [r12]
            LEFT JOIN [OptionalRelated] AS [o3] ON [r12].[Id] = [o3].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o4] ON [o3].[RootEntityId] = [o4].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o5] ON [o3].[RootEntityId] = [o5].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r13] ON [r12].[Id] = [r13].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r14] ON [r13].[RootEntityId] = [r14].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r15] ON [r13].[RootEntityId] = [r15].[AssociateTypeRootEntityId]
        ) AS [r11]
        WHERE [r0].[Key] = [r11].[Key] OR ([r0].[Key] IS NULL AND [r11].[Key] IS NULL)
        ORDER BY [r11].[Id] DESC) AS [c], (
        SELECT TOP(1) [r16].[Id]
        FROM (
            SELECT [r17].[Id], [r17].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r17]
            LEFT JOIN [OptionalRelated] AS [o6] ON [r17].[Id] = [o6].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o7] ON [o6].[RootEntityId] = [o7].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o8] ON [o6].[RootEntityId] = [o8].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r18] ON [r17].[Id] = [r18].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r19] ON [r18].[RootEntityId] = [r19].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r20] ON [r18].[RootEntityId] = [r20].[AssociateTypeRootEntityId]
        ) AS [r16]
        WHERE [r0].[Key] = [r16].[Key] OR ([r0].[Key] IS NULL AND [r16].[Key] IS NULL)
        ORDER BY [r16].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r21]
LEFT JOIN (
    SELECT [r22].[Id], [r22].[Name], [r22].[RootEntityId], [r22].[Id0], [r22].[Int], [r22].[Ints], [r22].[Name0], [r22].[String], [r22].[AssociateTypeRootEntityId], [r22].[Id1], [r22].[Int0], [r22].[Ints0], [r22].[Name1], [r22].[String0], [r22].[AssociateTypeRootEntityId0], [r22].[Id2], [r22].[Int1], [r22].[Ints1], [r22].[Name2], [r22].[String1], [r22].[RootEntityId0], [r22].[Id3], [r22].[Int2], [r22].[Ints2], [r22].[Name3], [r22].[String2], [r22].[AssociateTypeRootEntityId1], [r22].[Id4], [r22].[Int3], [r22].[Ints3], [r22].[Name4], [r22].[String3], [r22].[AssociateTypeRootEntityId2], [r22].[Id5], [r22].[Int4], [r22].[Ints4], [r22].[Name5], [r22].[String4], [r22].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId0], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[RootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [o].[RootEntityId], [o].[Id] AS [Id0], [o].[Int], [o].[Ints], [o].[Name] AS [Name0], [o].[String], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id1], [o0].[Int] AS [Int0], [o0].[Ints] AS [Ints0], [o0].[Name] AS [Name1], [o0].[String] AS [String0], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id2], [o1].[Int] AS [Int1], [o1].[Ints] AS [Ints1], [o1].[Name] AS [Name2], [o1].[String] AS [String1], [r3].[RootEntityId] AS [RootEntityId0], [r3].[Id] AS [Id3], [r3].[Int] AS [Int2], [r3].[Ints] AS [Ints2], [r3].[Name] AS [Name3], [r3].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
    ) AS [r22]
    WHERE [r22].[row] <= 1
) AS [r23] ON [r21].[Key] = [r23].[Key] OR ([r21].[Key] IS NULL AND [r23].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r23].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r23].[RootEntityId] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r23].[RootEntityId0] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r21].[c], [r21].[c0], [r21].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    #endregion GroupBy element

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
