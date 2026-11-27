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

    #region GroupBy

    public override async Task GroupBy_OrderByDescending_First_on_associate_scalar_property()
    {
        await base.GroupBy_OrderByDescending_First_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r13].[Id], [r13].[Name], [r11].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r13].[RootEntityId0], [r13].[Id1], [r13].[Int0], [r13].[Ints0], [r13].[Name1], [r13].[String0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r13].[AssociateTypeRootEntityId], [r13].[Id2], [r13].[Int1], [r13].[Ints1], [r13].[Name2], [r13].[String1], [r13].[AssociateTypeRootEntityId0], [r13].[Id3], [r13].[Int2], [r13].[Ints2], [r13].[Name3], [r13].[String2], [r13].[RootEntityId], [r13].[Id0], [r13].[Int], [r13].[Ints], [r13].[Name0], [r13].[String], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r13].[AssociateTypeRootEntityId1], [r13].[Id4], [r13].[Int3], [r13].[Ints3], [r13].[Name4], [r13].[String3], [r13].[AssociateTypeRootEntityId2], [r13].[Id5], [r13].[Int4], [r13].[Ints4], [r13].[Name5], [r13].[String4]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r11]
LEFT JOIN (
    SELECT [r12].[Id], [r12].[Name], [r12].[RootEntityId0], [r12].[Id1], [r12].[Int0], [r12].[Ints0], [r12].[Name1], [r12].[String0], [r12].[AssociateTypeRootEntityId], [r12].[Id2], [r12].[Int1], [r12].[Ints1], [r12].[Name2], [r12].[String1], [r12].[AssociateTypeRootEntityId0], [r12].[Id3], [r12].[Int2], [r12].[Ints2], [r12].[Name3], [r12].[String2], [r12].[RootEntityId], [r12].[Id0], [r12].[Int], [r12].[Ints], [r12].[Name0], [r12].[String], [r12].[AssociateTypeRootEntityId1], [r12].[Id4], [r12].[Int3], [r12].[Ints3], [r12].[Name4], [r12].[String3], [r12].[AssociateTypeRootEntityId2], [r12].[Id5], [r12].[Int4], [r12].[Ints4], [r12].[Name5], [r12].[String4], [r12].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId0], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[AssociateTypeRootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r3].[RootEntityId], [r3].[Id] AS [Id0], [r3].[Int], [r3].[Ints], [r3].[Name] AS [Name0], [r3].[String], [o].[RootEntityId] AS [RootEntityId0], [o].[Id] AS [Id1], [o].[Int] AS [Int0], [o].[Ints] AS [Ints0], [o].[Name] AS [Name1], [o].[String] AS [String0], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id2], [o0].[Int] AS [Int1], [o0].[Ints] AS [Ints1], [o0].[Name] AS [Name2], [o0].[String] AS [String1], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id3], [o1].[Int] AS [Int2], [o1].[Ints] AS [Ints2], [o1].[Name] AS [Name3], [o1].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
    ) AS [r12]
    WHERE [r12].[row] <= 1
) AS [r13] ON [r11].[Key] = [r13].[Key] OR ([r11].[Key] IS NULL AND [r13].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r13].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r13].[RootEntityId0] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r13].[RootEntityId] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r11].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    public override async Task GroupBy_MaxBy_on_associate_scalar_property()
    {
        await base.GroupBy_MaxBy_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r13].[Id], [r13].[Name], [r11].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[Id1], [s].[Int1], [s].[Ints1], [s].[Name1], [s].[String1], [s].[Id2], [s].[Int2], [s].[Ints2], [s].[Name2], [s].[String2], [r13].[RootEntityId0], [r13].[Id1], [r13].[Int0], [r13].[Ints0], [r13].[Name1], [r13].[String0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [o2].[Int], [o2].[Ints], [o2].[Name], [o2].[String], [r13].[AssociateTypeRootEntityId], [r13].[Id2], [r13].[Int1], [r13].[Ints1], [r13].[Name2], [r13].[String1], [r13].[AssociateTypeRootEntityId0], [r13].[Id3], [r13].[Int2], [r13].[Ints2], [r13].[Name3], [r13].[String2], [r13].[RootEntityId], [r13].[Id0], [r13].[Int], [r13].[Ints], [r13].[Name0], [r13].[String], [r10].[AssociateTypeRootEntityId], [r10].[Id], [r10].[Int], [r10].[Ints], [r10].[Name], [r10].[String], [r13].[AssociateTypeRootEntityId1], [r13].[Id4], [r13].[Int3], [r13].[Ints3], [r13].[Name4], [r13].[String3], [r13].[AssociateTypeRootEntityId2], [r13].[Id5], [r13].[Int4], [r13].[Ints4], [r13].[Name5], [r13].[String4]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r11]
LEFT JOIN (
    SELECT [r12].[Id], [r12].[Name], [r12].[RootEntityId0], [r12].[Id1], [r12].[Int0], [r12].[Ints0], [r12].[Name1], [r12].[String0], [r12].[AssociateTypeRootEntityId], [r12].[Id2], [r12].[Int1], [r12].[Ints1], [r12].[Name2], [r12].[String1], [r12].[AssociateTypeRootEntityId0], [r12].[Id3], [r12].[Int2], [r12].[Ints2], [r12].[Name3], [r12].[String2], [r12].[RootEntityId], [r12].[Id0], [r12].[Int], [r12].[Ints], [r12].[Name0], [r12].[String], [r12].[AssociateTypeRootEntityId1], [r12].[Id4], [r12].[Int3], [r12].[Ints3], [r12].[Name4], [r12].[String3], [r12].[AssociateTypeRootEntityId2], [r12].[Id5], [r12].[Int4], [r12].[Ints4], [r12].[Name5], [r12].[String4], [r12].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[RootEntityId0], [r1].[Id1], [r1].[Int0], [r1].[Ints0], [r1].[Name1], [r1].[String0], [r1].[AssociateTypeRootEntityId], [r1].[Id2], [r1].[Int1], [r1].[Ints1], [r1].[Name2], [r1].[String1], [r1].[AssociateTypeRootEntityId0], [r1].[Id3], [r1].[Int2], [r1].[Ints2], [r1].[Name3], [r1].[String2], [r1].[RootEntityId], [r1].[Id0], [r1].[Int], [r1].[Ints], [r1].[Name0], [r1].[String], [r1].[AssociateTypeRootEntityId1], [r1].[Id4], [r1].[Int3], [r1].[Ints3], [r1].[Name4], [r1].[String3], [r1].[AssociateTypeRootEntityId2], [r1].[Id5], [r1].[Int4], [r1].[Ints4], [r1].[Name5], [r1].[String4], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r3].[RootEntityId], [r3].[Id] AS [Id0], [r3].[Int], [r3].[Ints], [r3].[Name] AS [Name0], [r3].[String], [o].[RootEntityId] AS [RootEntityId0], [o].[Id] AS [Id1], [o].[Int] AS [Int0], [o].[Ints] AS [Ints0], [o].[Name] AS [Name1], [o].[String] AS [String0], [o0].[AssociateTypeRootEntityId], [o0].[Id] AS [Id2], [o0].[Int] AS [Int1], [o0].[Ints] AS [Ints1], [o0].[Name] AS [Name2], [o0].[String] AS [String1], [o1].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [o1].[Id] AS [Id3], [o1].[Int] AS [Int2], [o1].[Ints] AS [Ints2], [o1].[Name] AS [Name3], [o1].[String] AS [String2], [r4].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r4].[Id] AS [Id4], [r4].[Int] AS [Int3], [r4].[Ints] AS [Ints3], [r4].[Name] AS [Name4], [r4].[String] AS [String3], [r5].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId2], [r5].[Id] AS [Id5], [r5].[Int] AS [Int4], [r5].[Ints] AS [Ints4], [r5].[Name] AS [Name5], [r5].[String] AS [String4]
            FROM [RootEntity] AS [r2]
            LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
            LEFT JOIN [OptionalRelated] AS [o] ON [r2].[Id] = [o].[RootEntityId]
            LEFT JOIN [OptionalRelated_OptionalNested] AS [o0] ON [o].[RootEntityId] = [o0].[AssociateTypeRootEntityId]
            LEFT JOIN [OptionalRelated_RequiredNested] AS [o1] ON [o].[RootEntityId] = [o1].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_OptionalNested] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId]
            LEFT JOIN [RequiredRelated_RequiredNested] AS [r5] ON [r3].[RootEntityId] = [r5].[AssociateTypeRootEntityId]
        ) AS [r1]
    ) AS [r12]
    WHERE [r12].[row] <= 1
) AS [r13] ON [r11].[Key] = [r13].[Key] OR ([r11].[Key] IS NULL AND [r13].[Key] IS NULL)
LEFT JOIN (
    SELECT [r6].[RootEntityId], [r6].[Id], [r6].[Int], [r6].[Ints], [r6].[Name], [r6].[String], [r7].[AssociateTypeRootEntityId], [r7].[AssociateTypeId], [r8].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId0], [r8].[AssociateTypeId] AS [AssociateTypeId0], [r9].[AssociateTypeRootEntityId] AS [AssociateTypeRootEntityId1], [r9].[AssociateTypeId] AS [AssociateTypeId1], [r9].[Id] AS [Id0], [r9].[Int] AS [Int0], [r9].[Ints] AS [Ints0], [r9].[Name] AS [Name0], [r9].[String] AS [String0], [r7].[Id] AS [Id1], [r7].[Int] AS [Int1], [r7].[Ints] AS [Ints1], [r7].[Name] AS [Name1], [r7].[String] AS [String1], [r8].[Id] AS [Id2], [r8].[Int] AS [Int2], [r8].[Ints] AS [Ints2], [r8].[Name] AS [Name2], [r8].[String] AS [String2]
    FROM [RelatedCollection] AS [r6]
    LEFT JOIN [RelatedCollection_OptionalNested] AS [r7] ON [r6].[RootEntityId] = [r7].[AssociateTypeRootEntityId] AND [r6].[Id] = [r7].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_RequiredNested] AS [r8] ON [r6].[RootEntityId] = [r8].[AssociateTypeRootEntityId] AND [r6].[Id] = [r8].[AssociateTypeId]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r9] ON [r6].[RootEntityId] = [r9].[AssociateTypeRootEntityId] AND [r6].[Id] = [r9].[AssociateTypeId]
) AS [s] ON [r13].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o2] ON [r13].[RootEntityId0] = [o2].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r10] ON [r13].[RootEntityId] = [r10].[AssociateTypeRootEntityId]
ORDER BY [r11].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[AssociateTypeRootEntityId0], [s].[AssociateTypeId0], [s].[AssociateTypeRootEntityId1], [s].[AssociateTypeId1], [s].[Id0], [o2].[AssociateTypeRootEntityId], [o2].[Id], [r10].[AssociateTypeRootEntityId], [r10].[Id]
""");
    }

    public override async Task GroupBy_Max_on_associate_scalar_property()
    {
        await base.GroupBy_Max_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r0].[Key], (
    SELECT MAX([r1].[Int])
    FROM (
        SELECT [r2].[Id] % 2 AS [Key], [r3].[Int]
        FROM [RootEntity] AS [r2]
        LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
    ) AS [r1]
    WHERE [r0].[Key] = [r1].[Key] OR ([r0].[Key] IS NULL AND [r1].[Key] IS NULL)) AS [Max]
FROM (
    SELECT [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
    }

    public override async Task GroupBy_Count_with_predicate_on_associate_scalar_property()
    {
        await base.GroupBy_Count_with_predicate_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r0].[Key], (
    SELECT COUNT(*)
    FROM (
        SELECT [r2].[Id] % 2 AS [Key], [r3].[Int]
        FROM [RootEntity] AS [r2]
        LEFT JOIN [RequiredRelated] AS [r3] ON [r2].[Id] = [r3].[RootEntityId]
    ) AS [r1]
    WHERE ([r0].[Key] = [r1].[Key] OR ([r0].[Key] IS NULL AND [r1].[Key] IS NULL)) AND [r1].[Int] = 8) AS [Count]
FROM (
    SELECT [r].[Id] % 2 AS [Key]
    FROM [RootEntity] AS [r]
) AS [r0]
GROUP BY [r0].[Key]
""");
    }

    #endregion GroupBy

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
