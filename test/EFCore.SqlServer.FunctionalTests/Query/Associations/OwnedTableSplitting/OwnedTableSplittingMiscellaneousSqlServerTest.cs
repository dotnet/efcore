// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public class OwnedTableSplittingMiscellaneousSqlServerTest(
    OwnedTableSplittingSqlServerFixture fixture,
    ITestOutputHelper testOutputHelper)
    : OwnedTableSplittingMiscellaneousRelationalTestBase<OwnedTableSplittingSqlServerFixture>(fixture, testOutputHelper)
{
    #region Simple filters

    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r2].[AssociateTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Ints] AS [Ints0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNestedAssociate_Id], [r0].[OptionalNestedAssociate_Int], [r0].[OptionalNestedAssociate_Ints], [r0].[OptionalNestedAssociate_Name], [r0].[OptionalNestedAssociate_String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[AssociateTypeRootEntityId]
WHERE [r].[RequiredAssociate_Int] = 8
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId], [r2].[Id]
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r2].[AssociateTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Ints] AS [Ints0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNestedAssociate_Id], [r0].[OptionalNestedAssociate_Int], [r0].[OptionalNestedAssociate_Ints], [r0].[OptionalNestedAssociate_Name], [r0].[OptionalNestedAssociate_String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[AssociateTypeRootEntityId]
WHERE [r].[OptionalAssociate_Int] = 8
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId], [r2].[Id]
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r2].[AssociateTypeRootEntityId], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[String], [r].[RequiredAssociate_OptionalNestedAssociate_Id], [r].[RequiredAssociate_OptionalNestedAssociate_Int], [r].[RequiredAssociate_OptionalNestedAssociate_Ints], [r].[RequiredAssociate_OptionalNestedAssociate_Name], [r].[RequiredAssociate_OptionalNestedAssociate_String], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT [r0].[RootEntityId], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[String], [r1].[AssociateTypeRootEntityId], [r1].[AssociateTypeId], [r1].[Id] AS [Id0], [r1].[Int] AS [Int0], [r1].[Ints] AS [Ints0], [r1].[Name] AS [Name0], [r1].[String] AS [String0], [r0].[OptionalNestedAssociate_Id], [r0].[OptionalNestedAssociate_Int], [r0].[OptionalNestedAssociate_Ints], [r0].[OptionalNestedAssociate_Name], [r0].[OptionalNestedAssociate_String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r0]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r1] ON [r0].[RootEntityId] = [r1].[AssociateTypeRootEntityId] AND [r0].[Id] = [r1].[AssociateTypeId]
) AS [s] ON [r].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r2] ON [r].[Id] = [r2].[AssociateTypeRootEntityId]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Int] = 8
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId], [r2].[Id]
""");
    }

    #endregion Simple filters

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [r].[Id], [r0].[Id], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r].[OptionalAssociate_OptionalNestedAssociate_Id], [r].[OptionalAssociate_OptionalNestedAssociate_Int], [r].[OptionalAssociate_OptionalNestedAssociate_Ints], [r].[OptionalAssociate_OptionalNestedAssociate_Name], [r].[OptionalAssociate_OptionalNestedAssociate_String], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r0].[RequiredAssociate_Id], [r0].[RequiredAssociate_Int], [r0].[RequiredAssociate_Ints], [r0].[RequiredAssociate_Name], [r0].[RequiredAssociate_String], [r3].[AssociateTypeRootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r0].[RequiredAssociate_OptionalNestedAssociate_Id], [r0].[RequiredAssociate_OptionalNestedAssociate_Int], [r0].[RequiredAssociate_OptionalNestedAssociate_Ints], [r0].[RequiredAssociate_OptionalNestedAssociate_Name], [r0].[RequiredAssociate_OptionalNestedAssociate_String], [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT * FROM [RootEntity]
) AS [m]
LEFT JOIN [RootEntity] AS [r] ON [m].[Id] = [r].[Id]
LEFT JOIN [RootEntity] AS [r0] ON [m].[Id] = [r0].[Id]
LEFT JOIN (
    SELECT [r1].[RootEntityId], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[String], [r2].[AssociateTypeRootEntityId], [r2].[AssociateTypeId], [r2].[Id] AS [Id0], [r2].[Int] AS [Int0], [r2].[Ints] AS [Ints0], [r2].[Name] AS [Name0], [r2].[String] AS [String0], [r1].[OptionalNestedAssociate_Id], [r1].[OptionalNestedAssociate_Int], [r1].[OptionalNestedAssociate_Ints], [r1].[OptionalNestedAssociate_Name], [r1].[OptionalNestedAssociate_String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r1]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r2] ON [r1].[RootEntityId] = [r2].[AssociateTypeRootEntityId] AND [r1].[Id] = [r2].[AssociateTypeId]
) AS [s] ON [m].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r].[OptionalAssociate_Id] IS NOT NULL AND [r].[OptionalAssociate_Int] IS NOT NULL AND [r].[OptionalAssociate_Ints] IS NOT NULL AND [r].[OptionalAssociate_Name] IS NOT NULL AND [r].[OptionalAssociate_String] IS NOT NULL THEN [r].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r3] ON [r0].[Id] = [r3].[AssociateTypeRootEntityId]
ORDER BY [m].[Id], [r].[Id], [r0].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r3].[AssociateTypeRootEntityId], [r3].[Id]
""");
    }

    #region GroupBy

    public override async Task GroupBy_OrderByDescending_First_on_associate_scalar_property()
    {
        await base.GroupBy_OrderByDescending_First_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r8].[Id], [r8].[Name], [r6].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r8].[OptionalAssociate_Id], [r8].[OptionalAssociate_Int], [r8].[OptionalAssociate_Ints], [r8].[OptionalAssociate_Name], [r8].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r8].[OptionalAssociate_OptionalNestedAssociate_Id], [r8].[OptionalAssociate_OptionalNestedAssociate_Int], [r8].[OptionalAssociate_OptionalNestedAssociate_Ints], [r8].[OptionalAssociate_OptionalNestedAssociate_Name], [r8].[OptionalAssociate_OptionalNestedAssociate_String], [r8].[OptionalAssociate_RequiredNestedAssociate_Id], [r8].[OptionalAssociate_RequiredNestedAssociate_Int], [r8].[OptionalAssociate_RequiredNestedAssociate_Ints], [r8].[OptionalAssociate_RequiredNestedAssociate_Name], [r8].[OptionalAssociate_RequiredNestedAssociate_String], [r8].[RequiredAssociate_Id], [r8].[RequiredAssociate_Int], [r8].[RequiredAssociate_Ints], [r8].[RequiredAssociate_Name], [r8].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r8].[RequiredAssociate_OptionalNestedAssociate_Id], [r8].[RequiredAssociate_OptionalNestedAssociate_Int], [r8].[RequiredAssociate_OptionalNestedAssociate_Ints], [r8].[RequiredAssociate_OptionalNestedAssociate_Name], [r8].[RequiredAssociate_OptionalNestedAssociate_String], [r8].[RequiredAssociate_RequiredNestedAssociate_Id], [r8].[RequiredAssociate_RequiredNestedAssociate_Int], [r8].[RequiredAssociate_RequiredNestedAssociate_Ints], [r8].[RequiredAssociate_RequiredNestedAssociate_Name], [r8].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r6]
LEFT JOIN (
    SELECT [r7].[Id], [r7].[Name], [r7].[OptionalAssociate_Id], [r7].[OptionalAssociate_Int], [r7].[OptionalAssociate_Ints], [r7].[OptionalAssociate_Name], [r7].[OptionalAssociate_String], [r7].[OptionalAssociate_OptionalNestedAssociate_Id], [r7].[OptionalAssociate_OptionalNestedAssociate_Int], [r7].[OptionalAssociate_OptionalNestedAssociate_Ints], [r7].[OptionalAssociate_OptionalNestedAssociate_Name], [r7].[OptionalAssociate_OptionalNestedAssociate_String], [r7].[OptionalAssociate_RequiredNestedAssociate_Id], [r7].[OptionalAssociate_RequiredNestedAssociate_Int], [r7].[OptionalAssociate_RequiredNestedAssociate_Ints], [r7].[OptionalAssociate_RequiredNestedAssociate_Name], [r7].[OptionalAssociate_RequiredNestedAssociate_String], [r7].[RequiredAssociate_Id], [r7].[RequiredAssociate_Int], [r7].[RequiredAssociate_Ints], [r7].[RequiredAssociate_Name], [r7].[RequiredAssociate_String], [r7].[RequiredAssociate_OptionalNestedAssociate_Id], [r7].[RequiredAssociate_OptionalNestedAssociate_Int], [r7].[RequiredAssociate_OptionalNestedAssociate_Ints], [r7].[RequiredAssociate_OptionalNestedAssociate_Name], [r7].[RequiredAssociate_OptionalNestedAssociate_String], [r7].[RequiredAssociate_RequiredNestedAssociate_Id], [r7].[RequiredAssociate_RequiredNestedAssociate_Int], [r7].[RequiredAssociate_RequiredNestedAssociate_Ints], [r7].[RequiredAssociate_RequiredNestedAssociate_Name], [r7].[RequiredAssociate_RequiredNestedAssociate_String], [r7].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[RequiredAssociate_Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r7]
    WHERE [r7].[row] <= 1
) AS [r8] ON [r6].[Key] = [r8].[Key] OR ([r6].[Key] IS NULL AND [r8].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r8].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r8].[OptionalAssociate_Id] IS NOT NULL AND [r8].[OptionalAssociate_Int] IS NOT NULL AND [r8].[OptionalAssociate_Ints] IS NOT NULL AND [r8].[OptionalAssociate_Name] IS NOT NULL AND [r8].[OptionalAssociate_String] IS NOT NULL THEN [r8].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r8].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r6].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    public override async Task GroupBy_MaxBy_on_associate_scalar_property()
    {
        await base.GroupBy_MaxBy_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r8].[Id], [r8].[Name], [r6].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r8].[OptionalAssociate_Id], [r8].[OptionalAssociate_Int], [r8].[OptionalAssociate_Ints], [r8].[OptionalAssociate_Name], [r8].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r8].[OptionalAssociate_OptionalNestedAssociate_Id], [r8].[OptionalAssociate_OptionalNestedAssociate_Int], [r8].[OptionalAssociate_OptionalNestedAssociate_Ints], [r8].[OptionalAssociate_OptionalNestedAssociate_Name], [r8].[OptionalAssociate_OptionalNestedAssociate_String], [r8].[OptionalAssociate_RequiredNestedAssociate_Id], [r8].[OptionalAssociate_RequiredNestedAssociate_Int], [r8].[OptionalAssociate_RequiredNestedAssociate_Ints], [r8].[OptionalAssociate_RequiredNestedAssociate_Name], [r8].[OptionalAssociate_RequiredNestedAssociate_String], [r8].[RequiredAssociate_Id], [r8].[RequiredAssociate_Int], [r8].[RequiredAssociate_Ints], [r8].[RequiredAssociate_Name], [r8].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r8].[RequiredAssociate_OptionalNestedAssociate_Id], [r8].[RequiredAssociate_OptionalNestedAssociate_Int], [r8].[RequiredAssociate_OptionalNestedAssociate_Ints], [r8].[RequiredAssociate_OptionalNestedAssociate_Name], [r8].[RequiredAssociate_OptionalNestedAssociate_String], [r8].[RequiredAssociate_RequiredNestedAssociate_Id], [r8].[RequiredAssociate_RequiredNestedAssociate_Int], [r8].[RequiredAssociate_RequiredNestedAssociate_Ints], [r8].[RequiredAssociate_RequiredNestedAssociate_Name], [r8].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r6]
LEFT JOIN (
    SELECT [r7].[Id], [r7].[Name], [r7].[OptionalAssociate_Id], [r7].[OptionalAssociate_Int], [r7].[OptionalAssociate_Ints], [r7].[OptionalAssociate_Name], [r7].[OptionalAssociate_String], [r7].[OptionalAssociate_OptionalNestedAssociate_Id], [r7].[OptionalAssociate_OptionalNestedAssociate_Int], [r7].[OptionalAssociate_OptionalNestedAssociate_Ints], [r7].[OptionalAssociate_OptionalNestedAssociate_Name], [r7].[OptionalAssociate_OptionalNestedAssociate_String], [r7].[OptionalAssociate_RequiredNestedAssociate_Id], [r7].[OptionalAssociate_RequiredNestedAssociate_Int], [r7].[OptionalAssociate_RequiredNestedAssociate_Ints], [r7].[OptionalAssociate_RequiredNestedAssociate_Name], [r7].[OptionalAssociate_RequiredNestedAssociate_String], [r7].[RequiredAssociate_Id], [r7].[RequiredAssociate_Int], [r7].[RequiredAssociate_Ints], [r7].[RequiredAssociate_Name], [r7].[RequiredAssociate_String], [r7].[RequiredAssociate_OptionalNestedAssociate_Id], [r7].[RequiredAssociate_OptionalNestedAssociate_Int], [r7].[RequiredAssociate_OptionalNestedAssociate_Ints], [r7].[RequiredAssociate_OptionalNestedAssociate_Name], [r7].[RequiredAssociate_OptionalNestedAssociate_String], [r7].[RequiredAssociate_RequiredNestedAssociate_Id], [r7].[RequiredAssociate_RequiredNestedAssociate_Int], [r7].[RequiredAssociate_RequiredNestedAssociate_Ints], [r7].[RequiredAssociate_RequiredNestedAssociate_Name], [r7].[RequiredAssociate_RequiredNestedAssociate_String], [r7].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[RequiredAssociate_Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r7]
    WHERE [r7].[row] <= 1
) AS [r8] ON [r6].[Key] = [r8].[Key] OR ([r6].[Key] IS NULL AND [r8].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r8].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r8].[OptionalAssociate_Id] IS NOT NULL AND [r8].[OptionalAssociate_Int] IS NOT NULL AND [r8].[OptionalAssociate_Ints] IS NOT NULL AND [r8].[OptionalAssociate_Name] IS NOT NULL AND [r8].[OptionalAssociate_String] IS NOT NULL THEN [r8].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r8].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r6].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    public override async Task GroupBy_Max_on_associate_scalar_property()
    {
        await base.GroupBy_Max_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r0].[Key], MAX([r0].[RequiredAssociate_Int]) AS [Max]
FROM (
    SELECT [r].[Id] % 2 AS [Key], [r].[RequiredAssociate_Int]
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
SELECT [r0].[Key], COUNT(CASE
    WHEN [r0].[RequiredAssociate_Int] = 8 THEN 1
END) AS [Count]
FROM (
    SELECT [r].[Id] % 2 AS [Key], [r].[RequiredAssociate_Int]
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
