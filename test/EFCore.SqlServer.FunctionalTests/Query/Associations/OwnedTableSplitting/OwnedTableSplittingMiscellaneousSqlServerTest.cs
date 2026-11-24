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

    #region GroupBy element

    public override async Task Where_on_root_property_on_GroupBy_First()
    {
        await base.Where_on_root_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r10].[Id], [r10].[Name], [r8].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r10].[OptionalAssociate_Id], [r10].[OptionalAssociate_Int], [r10].[OptionalAssociate_Ints], [r10].[OptionalAssociate_Name], [r10].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r10].[OptionalAssociate_OptionalNestedAssociate_Id], [r10].[OptionalAssociate_OptionalNestedAssociate_Int], [r10].[OptionalAssociate_OptionalNestedAssociate_Ints], [r10].[OptionalAssociate_OptionalNestedAssociate_Name], [r10].[OptionalAssociate_OptionalNestedAssociate_String], [r10].[OptionalAssociate_RequiredNestedAssociate_Id], [r10].[OptionalAssociate_RequiredNestedAssociate_Int], [r10].[OptionalAssociate_RequiredNestedAssociate_Ints], [r10].[OptionalAssociate_RequiredNestedAssociate_Name], [r10].[OptionalAssociate_RequiredNestedAssociate_String], [r10].[RequiredAssociate_Id], [r10].[RequiredAssociate_Int], [r10].[RequiredAssociate_Ints], [r10].[RequiredAssociate_Name], [r10].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r10].[RequiredAssociate_OptionalNestedAssociate_Id], [r10].[RequiredAssociate_OptionalNestedAssociate_Int], [r10].[RequiredAssociate_OptionalNestedAssociate_Ints], [r10].[RequiredAssociate_OptionalNestedAssociate_Name], [r10].[RequiredAssociate_OptionalNestedAssociate_String], [r10].[RequiredAssociate_RequiredNestedAssociate_Id], [r10].[RequiredAssociate_RequiredNestedAssociate_Int], [r10].[RequiredAssociate_RequiredNestedAssociate_Ints], [r10].[RequiredAssociate_RequiredNestedAssociate_Name], [r10].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r6].[Id]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE ([r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)) AND [r6].[Id] > 0
        ORDER BY [r6].[Id]) > 1
) AS [r8]
LEFT JOIN (
    SELECT [r9].[Id], [r9].[Name], [r9].[OptionalAssociate_Id], [r9].[OptionalAssociate_Int], [r9].[OptionalAssociate_Ints], [r9].[OptionalAssociate_Name], [r9].[OptionalAssociate_String], [r9].[OptionalAssociate_OptionalNestedAssociate_Id], [r9].[OptionalAssociate_OptionalNestedAssociate_Int], [r9].[OptionalAssociate_OptionalNestedAssociate_Ints], [r9].[OptionalAssociate_OptionalNestedAssociate_Name], [r9].[OptionalAssociate_OptionalNestedAssociate_String], [r9].[OptionalAssociate_RequiredNestedAssociate_Id], [r9].[OptionalAssociate_RequiredNestedAssociate_Int], [r9].[OptionalAssociate_RequiredNestedAssociate_Ints], [r9].[OptionalAssociate_RequiredNestedAssociate_Name], [r9].[OptionalAssociate_RequiredNestedAssociate_String], [r9].[RequiredAssociate_Id], [r9].[RequiredAssociate_Int], [r9].[RequiredAssociate_Ints], [r9].[RequiredAssociate_Name], [r9].[RequiredAssociate_String], [r9].[RequiredAssociate_OptionalNestedAssociate_Id], [r9].[RequiredAssociate_OptionalNestedAssociate_Int], [r9].[RequiredAssociate_OptionalNestedAssociate_Ints], [r9].[RequiredAssociate_OptionalNestedAssociate_Name], [r9].[RequiredAssociate_OptionalNestedAssociate_String], [r9].[RequiredAssociate_RequiredNestedAssociate_Id], [r9].[RequiredAssociate_RequiredNestedAssociate_Int], [r9].[RequiredAssociate_RequiredNestedAssociate_Ints], [r9].[RequiredAssociate_RequiredNestedAssociate_Name], [r9].[RequiredAssociate_RequiredNestedAssociate_String], [r9].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r9]
    WHERE [r9].[row] <= 1
) AS [r10] ON [r8].[Key] = [r10].[Key] OR ([r8].[Key] IS NULL AND [r10].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r10].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r10].[OptionalAssociate_Id] IS NOT NULL AND [r10].[OptionalAssociate_Int] IS NOT NULL AND [r10].[OptionalAssociate_Ints] IS NOT NULL AND [r10].[OptionalAssociate_Name] IS NOT NULL AND [r10].[OptionalAssociate_String] IS NOT NULL THEN [r10].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r10].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r8].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    public override async Task Where_on_associate_scalar_property_on_GroupBy_First()
    {
        await base.Where_on_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r10].[Id], [r10].[Name], [r8].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r10].[OptionalAssociate_Id], [r10].[OptionalAssociate_Int], [r10].[OptionalAssociate_Ints], [r10].[OptionalAssociate_Name], [r10].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r10].[OptionalAssociate_OptionalNestedAssociate_Id], [r10].[OptionalAssociate_OptionalNestedAssociate_Int], [r10].[OptionalAssociate_OptionalNestedAssociate_Ints], [r10].[OptionalAssociate_OptionalNestedAssociate_Name], [r10].[OptionalAssociate_OptionalNestedAssociate_String], [r10].[OptionalAssociate_RequiredNestedAssociate_Id], [r10].[OptionalAssociate_RequiredNestedAssociate_Int], [r10].[OptionalAssociate_RequiredNestedAssociate_Ints], [r10].[OptionalAssociate_RequiredNestedAssociate_Name], [r10].[OptionalAssociate_RequiredNestedAssociate_String], [r10].[RequiredAssociate_Id], [r10].[RequiredAssociate_Int], [r10].[RequiredAssociate_Ints], [r10].[RequiredAssociate_Name], [r10].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r10].[RequiredAssociate_OptionalNestedAssociate_Id], [r10].[RequiredAssociate_OptionalNestedAssociate_Int], [r10].[RequiredAssociate_OptionalNestedAssociate_Ints], [r10].[RequiredAssociate_OptionalNestedAssociate_Name], [r10].[RequiredAssociate_OptionalNestedAssociate_String], [r10].[RequiredAssociate_RequiredNestedAssociate_Id], [r10].[RequiredAssociate_RequiredNestedAssociate_Int], [r10].[RequiredAssociate_RequiredNestedAssociate_Ints], [r10].[RequiredAssociate_RequiredNestedAssociate_Name], [r10].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r6].[RequiredAssociate_Int]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key], [r7].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE ([r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)) AND [r6].[Id] > 0
        ORDER BY [r6].[Id]) = 8
) AS [r8]
LEFT JOIN (
    SELECT [r9].[Id], [r9].[Name], [r9].[OptionalAssociate_Id], [r9].[OptionalAssociate_Int], [r9].[OptionalAssociate_Ints], [r9].[OptionalAssociate_Name], [r9].[OptionalAssociate_String], [r9].[OptionalAssociate_OptionalNestedAssociate_Id], [r9].[OptionalAssociate_OptionalNestedAssociate_Int], [r9].[OptionalAssociate_OptionalNestedAssociate_Ints], [r9].[OptionalAssociate_OptionalNestedAssociate_Name], [r9].[OptionalAssociate_OptionalNestedAssociate_String], [r9].[OptionalAssociate_RequiredNestedAssociate_Id], [r9].[OptionalAssociate_RequiredNestedAssociate_Int], [r9].[OptionalAssociate_RequiredNestedAssociate_Ints], [r9].[OptionalAssociate_RequiredNestedAssociate_Name], [r9].[OptionalAssociate_RequiredNestedAssociate_String], [r9].[RequiredAssociate_Id], [r9].[RequiredAssociate_Int], [r9].[RequiredAssociate_Ints], [r9].[RequiredAssociate_Name], [r9].[RequiredAssociate_String], [r9].[RequiredAssociate_OptionalNestedAssociate_Id], [r9].[RequiredAssociate_OptionalNestedAssociate_Int], [r9].[RequiredAssociate_OptionalNestedAssociate_Ints], [r9].[RequiredAssociate_OptionalNestedAssociate_Name], [r9].[RequiredAssociate_OptionalNestedAssociate_String], [r9].[RequiredAssociate_RequiredNestedAssociate_Id], [r9].[RequiredAssociate_RequiredNestedAssociate_Int], [r9].[RequiredAssociate_RequiredNestedAssociate_Ints], [r9].[RequiredAssociate_RequiredNestedAssociate_Name], [r9].[RequiredAssociate_RequiredNestedAssociate_String], [r9].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r9]
    WHERE [r9].[row] <= 1
) AS [r10] ON [r8].[Key] = [r10].[Key] OR ([r8].[Key] IS NULL AND [r10].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r10].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r10].[OptionalAssociate_Id] IS NOT NULL AND [r10].[OptionalAssociate_Int] IS NOT NULL AND [r10].[OptionalAssociate_Ints] IS NOT NULL AND [r10].[OptionalAssociate_Name] IS NOT NULL AND [r10].[OptionalAssociate_String] IS NOT NULL THEN [r10].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r10].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r8].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    public override async Task Where_on_nested_associate_scalar_property_on_GroupBy_First()
    {
        await base.Where_on_nested_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r10].[Id], [r10].[Name], [r8].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r10].[OptionalAssociate_Id], [r10].[OptionalAssociate_Int], [r10].[OptionalAssociate_Ints], [r10].[OptionalAssociate_Name], [r10].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r10].[OptionalAssociate_OptionalNestedAssociate_Id], [r10].[OptionalAssociate_OptionalNestedAssociate_Int], [r10].[OptionalAssociate_OptionalNestedAssociate_Ints], [r10].[OptionalAssociate_OptionalNestedAssociate_Name], [r10].[OptionalAssociate_OptionalNestedAssociate_String], [r10].[OptionalAssociate_RequiredNestedAssociate_Id], [r10].[OptionalAssociate_RequiredNestedAssociate_Int], [r10].[OptionalAssociate_RequiredNestedAssociate_Ints], [r10].[OptionalAssociate_RequiredNestedAssociate_Name], [r10].[OptionalAssociate_RequiredNestedAssociate_String], [r10].[RequiredAssociate_Id], [r10].[RequiredAssociate_Int], [r10].[RequiredAssociate_Ints], [r10].[RequiredAssociate_Name], [r10].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r10].[RequiredAssociate_OptionalNestedAssociate_Id], [r10].[RequiredAssociate_OptionalNestedAssociate_Int], [r10].[RequiredAssociate_OptionalNestedAssociate_Ints], [r10].[RequiredAssociate_OptionalNestedAssociate_Name], [r10].[RequiredAssociate_OptionalNestedAssociate_String], [r10].[RequiredAssociate_RequiredNestedAssociate_Id], [r10].[RequiredAssociate_RequiredNestedAssociate_Int], [r10].[RequiredAssociate_RequiredNestedAssociate_Ints], [r10].[RequiredAssociate_RequiredNestedAssociate_Name], [r10].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r6].[RequiredAssociate_RequiredNestedAssociate_Int]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key], [r7].[RequiredAssociate_RequiredNestedAssociate_Int]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE ([r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)) AND [r6].[Id] > 0
        ORDER BY [r6].[Id]) = 8
) AS [r8]
LEFT JOIN (
    SELECT [r9].[Id], [r9].[Name], [r9].[OptionalAssociate_Id], [r9].[OptionalAssociate_Int], [r9].[OptionalAssociate_Ints], [r9].[OptionalAssociate_Name], [r9].[OptionalAssociate_String], [r9].[OptionalAssociate_OptionalNestedAssociate_Id], [r9].[OptionalAssociate_OptionalNestedAssociate_Int], [r9].[OptionalAssociate_OptionalNestedAssociate_Ints], [r9].[OptionalAssociate_OptionalNestedAssociate_Name], [r9].[OptionalAssociate_OptionalNestedAssociate_String], [r9].[OptionalAssociate_RequiredNestedAssociate_Id], [r9].[OptionalAssociate_RequiredNestedAssociate_Int], [r9].[OptionalAssociate_RequiredNestedAssociate_Ints], [r9].[OptionalAssociate_RequiredNestedAssociate_Name], [r9].[OptionalAssociate_RequiredNestedAssociate_String], [r9].[RequiredAssociate_Id], [r9].[RequiredAssociate_Int], [r9].[RequiredAssociate_Ints], [r9].[RequiredAssociate_Name], [r9].[RequiredAssociate_String], [r9].[RequiredAssociate_OptionalNestedAssociate_Id], [r9].[RequiredAssociate_OptionalNestedAssociate_Int], [r9].[RequiredAssociate_OptionalNestedAssociate_Ints], [r9].[RequiredAssociate_OptionalNestedAssociate_Name], [r9].[RequiredAssociate_OptionalNestedAssociate_String], [r9].[RequiredAssociate_RequiredNestedAssociate_Id], [r9].[RequiredAssociate_RequiredNestedAssociate_Int], [r9].[RequiredAssociate_RequiredNestedAssociate_Ints], [r9].[RequiredAssociate_RequiredNestedAssociate_Name], [r9].[RequiredAssociate_RequiredNestedAssociate_String], [r9].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r9]
    WHERE [r9].[row] <= 1
) AS [r10] ON [r8].[Key] = [r10].[Key] OR ([r8].[Key] IS NULL AND [r10].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r10].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r10].[OptionalAssociate_Id] IS NOT NULL AND [r10].[OptionalAssociate_Int] IS NOT NULL AND [r10].[OptionalAssociate_Ints] IS NOT NULL AND [r10].[OptionalAssociate_Name] IS NOT NULL AND [r10].[OptionalAssociate_String] IS NOT NULL THEN [r10].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r10].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r8].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    public override async Task Select_multiple_associates_on_GroupBy_OrderByDescending_First()
    {
        await base.Select_multiple_associates_on_GroupBy_OrderByDescending_First();

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [r10].[Id]
    FROM (
        SELECT [r11].[Id], [r11].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r11]
    ) AS [r10]
    WHERE [r12].[Key] = [r10].[Key] OR ([r12].[Key] IS NULL AND [r10].[Key] IS NULL)
    ORDER BY [r10].[Id] DESC), [r14].[Id], [r14].[Name], [r12].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r14].[OptionalAssociate_Id], [r14].[OptionalAssociate_Int], [r14].[OptionalAssociate_Ints], [r14].[OptionalAssociate_Name], [r14].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r14].[OptionalAssociate_OptionalNestedAssociate_Id], [r14].[OptionalAssociate_OptionalNestedAssociate_Int], [r14].[OptionalAssociate_OptionalNestedAssociate_Ints], [r14].[OptionalAssociate_OptionalNestedAssociate_Name], [r14].[OptionalAssociate_OptionalNestedAssociate_String], [r14].[OptionalAssociate_RequiredNestedAssociate_Id], [r14].[OptionalAssociate_RequiredNestedAssociate_Int], [r14].[OptionalAssociate_RequiredNestedAssociate_Ints], [r14].[OptionalAssociate_RequiredNestedAssociate_Name], [r14].[OptionalAssociate_RequiredNestedAssociate_String], [r14].[RequiredAssociate_Id], [r14].[RequiredAssociate_Int], [r14].[RequiredAssociate_Ints], [r14].[RequiredAssociate_Name], [r14].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r14].[RequiredAssociate_OptionalNestedAssociate_Id], [r14].[RequiredAssociate_OptionalNestedAssociate_Int], [r14].[RequiredAssociate_OptionalNestedAssociate_Ints], [r14].[RequiredAssociate_OptionalNestedAssociate_Name], [r14].[RequiredAssociate_OptionalNestedAssociate_String], [r14].[RequiredAssociate_RequiredNestedAssociate_Id], [r14].[RequiredAssociate_RequiredNestedAssociate_Int], [r14].[RequiredAssociate_RequiredNestedAssociate_Ints], [r14].[RequiredAssociate_RequiredNestedAssociate_Name], [r14].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r6].[RequiredAssociate_Int]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key], [r7].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE [r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)
        ORDER BY [r6].[Id] DESC) AS [c], (
        SELECT TOP(1) [r8].[Id]
        FROM (
            SELECT [r9].[Id], [r9].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r9]
        ) AS [r8]
        WHERE [r0].[Key] = [r8].[Key] OR ([r0].[Key] IS NULL AND [r8].[Key] IS NULL)
        ORDER BY [r8].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r12]
LEFT JOIN (
    SELECT [r13].[Id], [r13].[Name], [r13].[OptionalAssociate_Id], [r13].[OptionalAssociate_Int], [r13].[OptionalAssociate_Ints], [r13].[OptionalAssociate_Name], [r13].[OptionalAssociate_String], [r13].[OptionalAssociate_OptionalNestedAssociate_Id], [r13].[OptionalAssociate_OptionalNestedAssociate_Int], [r13].[OptionalAssociate_OptionalNestedAssociate_Ints], [r13].[OptionalAssociate_OptionalNestedAssociate_Name], [r13].[OptionalAssociate_OptionalNestedAssociate_String], [r13].[OptionalAssociate_RequiredNestedAssociate_Id], [r13].[OptionalAssociate_RequiredNestedAssociate_Int], [r13].[OptionalAssociate_RequiredNestedAssociate_Ints], [r13].[OptionalAssociate_RequiredNestedAssociate_Name], [r13].[OptionalAssociate_RequiredNestedAssociate_String], [r13].[RequiredAssociate_Id], [r13].[RequiredAssociate_Int], [r13].[RequiredAssociate_Ints], [r13].[RequiredAssociate_Name], [r13].[RequiredAssociate_String], [r13].[RequiredAssociate_OptionalNestedAssociate_Id], [r13].[RequiredAssociate_OptionalNestedAssociate_Int], [r13].[RequiredAssociate_OptionalNestedAssociate_Ints], [r13].[RequiredAssociate_OptionalNestedAssociate_Name], [r13].[RequiredAssociate_OptionalNestedAssociate_String], [r13].[RequiredAssociate_RequiredNestedAssociate_Id], [r13].[RequiredAssociate_RequiredNestedAssociate_Int], [r13].[RequiredAssociate_RequiredNestedAssociate_Ints], [r13].[RequiredAssociate_RequiredNestedAssociate_Name], [r13].[RequiredAssociate_RequiredNestedAssociate_String], [r13].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r13]
    WHERE [r13].[row] <= 1
) AS [r14] ON [r12].[Key] = [r14].[Key] OR ([r12].[Key] IS NULL AND [r14].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r14].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r14].[OptionalAssociate_Id] IS NOT NULL AND [r14].[OptionalAssociate_Int] IS NOT NULL AND [r14].[OptionalAssociate_Ints] IS NOT NULL AND [r14].[OptionalAssociate_Name] IS NOT NULL AND [r14].[OptionalAssociate_String] IS NOT NULL THEN [r14].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r14].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r12].[c] DESC, [r12].[c0], [r12].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    public override async Task Select_multiple_associates_on_GroupBy_MaxBy()
    {
        await base.Select_multiple_associates_on_GroupBy_MaxBy();

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [r10].[Id]
    FROM (
        SELECT [r11].[Id], [r11].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r11]
    ) AS [r10]
    WHERE [r12].[Key] = [r10].[Key] OR ([r12].[Key] IS NULL AND [r10].[Key] IS NULL)
    ORDER BY [r10].[Id] DESC), [r14].[Id], [r14].[Name], [r12].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r14].[OptionalAssociate_Id], [r14].[OptionalAssociate_Int], [r14].[OptionalAssociate_Ints], [r14].[OptionalAssociate_Name], [r14].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r14].[OptionalAssociate_OptionalNestedAssociate_Id], [r14].[OptionalAssociate_OptionalNestedAssociate_Int], [r14].[OptionalAssociate_OptionalNestedAssociate_Ints], [r14].[OptionalAssociate_OptionalNestedAssociate_Name], [r14].[OptionalAssociate_OptionalNestedAssociate_String], [r14].[OptionalAssociate_RequiredNestedAssociate_Id], [r14].[OptionalAssociate_RequiredNestedAssociate_Int], [r14].[OptionalAssociate_RequiredNestedAssociate_Ints], [r14].[OptionalAssociate_RequiredNestedAssociate_Name], [r14].[OptionalAssociate_RequiredNestedAssociate_String], [r14].[RequiredAssociate_Id], [r14].[RequiredAssociate_Int], [r14].[RequiredAssociate_Ints], [r14].[RequiredAssociate_Name], [r14].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r14].[RequiredAssociate_OptionalNestedAssociate_Id], [r14].[RequiredAssociate_OptionalNestedAssociate_Int], [r14].[RequiredAssociate_OptionalNestedAssociate_Ints], [r14].[RequiredAssociate_OptionalNestedAssociate_Name], [r14].[RequiredAssociate_OptionalNestedAssociate_String], [r14].[RequiredAssociate_RequiredNestedAssociate_Id], [r14].[RequiredAssociate_RequiredNestedAssociate_Int], [r14].[RequiredAssociate_RequiredNestedAssociate_Ints], [r14].[RequiredAssociate_RequiredNestedAssociate_Name], [r14].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r6].[RequiredAssociate_Int]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key], [r7].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE [r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)
        ORDER BY [r6].[Id] DESC) AS [c], (
        SELECT TOP(1) [r8].[Id]
        FROM (
            SELECT [r9].[Id], [r9].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r9]
        ) AS [r8]
        WHERE [r0].[Key] = [r8].[Key] OR ([r0].[Key] IS NULL AND [r8].[Key] IS NULL)
        ORDER BY [r8].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r12]
LEFT JOIN (
    SELECT [r13].[Id], [r13].[Name], [r13].[OptionalAssociate_Id], [r13].[OptionalAssociate_Int], [r13].[OptionalAssociate_Ints], [r13].[OptionalAssociate_Name], [r13].[OptionalAssociate_String], [r13].[OptionalAssociate_OptionalNestedAssociate_Id], [r13].[OptionalAssociate_OptionalNestedAssociate_Int], [r13].[OptionalAssociate_OptionalNestedAssociate_Ints], [r13].[OptionalAssociate_OptionalNestedAssociate_Name], [r13].[OptionalAssociate_OptionalNestedAssociate_String], [r13].[OptionalAssociate_RequiredNestedAssociate_Id], [r13].[OptionalAssociate_RequiredNestedAssociate_Int], [r13].[OptionalAssociate_RequiredNestedAssociate_Ints], [r13].[OptionalAssociate_RequiredNestedAssociate_Name], [r13].[OptionalAssociate_RequiredNestedAssociate_String], [r13].[RequiredAssociate_Id], [r13].[RequiredAssociate_Int], [r13].[RequiredAssociate_Ints], [r13].[RequiredAssociate_Name], [r13].[RequiredAssociate_String], [r13].[RequiredAssociate_OptionalNestedAssociate_Id], [r13].[RequiredAssociate_OptionalNestedAssociate_Int], [r13].[RequiredAssociate_OptionalNestedAssociate_Ints], [r13].[RequiredAssociate_OptionalNestedAssociate_Name], [r13].[RequiredAssociate_OptionalNestedAssociate_String], [r13].[RequiredAssociate_RequiredNestedAssociate_Id], [r13].[RequiredAssociate_RequiredNestedAssociate_Int], [r13].[RequiredAssociate_RequiredNestedAssociate_Ints], [r13].[RequiredAssociate_RequiredNestedAssociate_Name], [r13].[RequiredAssociate_RequiredNestedAssociate_String], [r13].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r13]
    WHERE [r13].[row] <= 1
) AS [r14] ON [r12].[Key] = [r14].[Key] OR ([r12].[Key] IS NULL AND [r14].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r14].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r14].[OptionalAssociate_Id] IS NOT NULL AND [r14].[OptionalAssociate_Int] IS NOT NULL AND [r14].[OptionalAssociate_Ints] IS NOT NULL AND [r14].[OptionalAssociate_Name] IS NOT NULL AND [r14].[OptionalAssociate_String] IS NOT NULL THEN [r14].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r14].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r12].[c] DESC, [r12].[c0], [r12].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    public override async Task OrderBy_associate_scalar_property_on_GroupBy_First()
    {
        await base.OrderBy_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r12].[Id], [r12].[Name], [r10].[Key], [s].[RootEntityId], [s].[Id], [s].[Int], [s].[Ints], [s].[Name], [s].[String], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [s].[Int0], [s].[Ints0], [s].[Name0], [s].[String0], [s].[OptionalNestedAssociate_Id], [s].[OptionalNestedAssociate_Int], [s].[OptionalNestedAssociate_Ints], [s].[OptionalNestedAssociate_Name], [s].[OptionalNestedAssociate_String], [s].[RequiredNestedAssociate_Id], [s].[RequiredNestedAssociate_Int], [s].[RequiredNestedAssociate_Ints], [s].[RequiredNestedAssociate_Name], [s].[RequiredNestedAssociate_String], [r12].[OptionalAssociate_Id], [r12].[OptionalAssociate_Int], [r12].[OptionalAssociate_Ints], [r12].[OptionalAssociate_Name], [r12].[OptionalAssociate_String], [o].[AssociateTypeRootEntityId], [o].[Id], [o].[Int], [o].[Ints], [o].[Name], [o].[String], [r12].[OptionalAssociate_OptionalNestedAssociate_Id], [r12].[OptionalAssociate_OptionalNestedAssociate_Int], [r12].[OptionalAssociate_OptionalNestedAssociate_Ints], [r12].[OptionalAssociate_OptionalNestedAssociate_Name], [r12].[OptionalAssociate_OptionalNestedAssociate_String], [r12].[OptionalAssociate_RequiredNestedAssociate_Id], [r12].[OptionalAssociate_RequiredNestedAssociate_Int], [r12].[OptionalAssociate_RequiredNestedAssociate_Ints], [r12].[OptionalAssociate_RequiredNestedAssociate_Name], [r12].[OptionalAssociate_RequiredNestedAssociate_String], [r12].[RequiredAssociate_Id], [r12].[RequiredAssociate_Int], [r12].[RequiredAssociate_Ints], [r12].[RequiredAssociate_Name], [r12].[RequiredAssociate_String], [r5].[AssociateTypeRootEntityId], [r5].[Id], [r5].[Int], [r5].[Ints], [r5].[Name], [r5].[String], [r12].[RequiredAssociate_OptionalNestedAssociate_Id], [r12].[RequiredAssociate_OptionalNestedAssociate_Int], [r12].[RequiredAssociate_OptionalNestedAssociate_Ints], [r12].[RequiredAssociate_OptionalNestedAssociate_Name], [r12].[RequiredAssociate_OptionalNestedAssociate_String], [r12].[RequiredAssociate_RequiredNestedAssociate_Id], [r12].[RequiredAssociate_RequiredNestedAssociate_Int], [r12].[RequiredAssociate_RequiredNestedAssociate_Ints], [r12].[RequiredAssociate_RequiredNestedAssociate_Name], [r12].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r6].[RequiredAssociate_Int]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key], [r7].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE [r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)
        ORDER BY [r6].[Id] DESC) AS [c], (
        SELECT TOP(1) [r8].[Id]
        FROM (
            SELECT [r9].[Id], [r9].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r9]
        ) AS [r8]
        WHERE [r0].[Key] = [r8].[Key] OR ([r0].[Key] IS NULL AND [r8].[Key] IS NULL)
        ORDER BY [r8].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r10]
LEFT JOIN (
    SELECT [r11].[Id], [r11].[Name], [r11].[OptionalAssociate_Id], [r11].[OptionalAssociate_Int], [r11].[OptionalAssociate_Ints], [r11].[OptionalAssociate_Name], [r11].[OptionalAssociate_String], [r11].[OptionalAssociate_OptionalNestedAssociate_Id], [r11].[OptionalAssociate_OptionalNestedAssociate_Int], [r11].[OptionalAssociate_OptionalNestedAssociate_Ints], [r11].[OptionalAssociate_OptionalNestedAssociate_Name], [r11].[OptionalAssociate_OptionalNestedAssociate_String], [r11].[OptionalAssociate_RequiredNestedAssociate_Id], [r11].[OptionalAssociate_RequiredNestedAssociate_Int], [r11].[OptionalAssociate_RequiredNestedAssociate_Ints], [r11].[OptionalAssociate_RequiredNestedAssociate_Name], [r11].[OptionalAssociate_RequiredNestedAssociate_String], [r11].[RequiredAssociate_Id], [r11].[RequiredAssociate_Int], [r11].[RequiredAssociate_Ints], [r11].[RequiredAssociate_Name], [r11].[RequiredAssociate_String], [r11].[RequiredAssociate_OptionalNestedAssociate_Id], [r11].[RequiredAssociate_OptionalNestedAssociate_Int], [r11].[RequiredAssociate_OptionalNestedAssociate_Ints], [r11].[RequiredAssociate_OptionalNestedAssociate_Name], [r11].[RequiredAssociate_OptionalNestedAssociate_String], [r11].[RequiredAssociate_RequiredNestedAssociate_Id], [r11].[RequiredAssociate_RequiredNestedAssociate_Int], [r11].[RequiredAssociate_RequiredNestedAssociate_Ints], [r11].[RequiredAssociate_RequiredNestedAssociate_Name], [r11].[RequiredAssociate_RequiredNestedAssociate_String], [r11].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[OptionalAssociate_OptionalNestedAssociate_Id], [r1].[OptionalAssociate_OptionalNestedAssociate_Int], [r1].[OptionalAssociate_OptionalNestedAssociate_Ints], [r1].[OptionalAssociate_OptionalNestedAssociate_Name], [r1].[OptionalAssociate_OptionalNestedAssociate_String], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[RequiredAssociate_OptionalNestedAssociate_Id], [r1].[RequiredAssociate_OptionalNestedAssociate_Int], [r1].[RequiredAssociate_OptionalNestedAssociate_Ints], [r1].[RequiredAssociate_OptionalNestedAssociate_Name], [r1].[RequiredAssociate_OptionalNestedAssociate_String], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_OptionalNestedAssociate_Id], [r2].[OptionalAssociate_OptionalNestedAssociate_Int], [r2].[OptionalAssociate_OptionalNestedAssociate_Ints], [r2].[OptionalAssociate_OptionalNestedAssociate_Name], [r2].[OptionalAssociate_OptionalNestedAssociate_String], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_OptionalNestedAssociate_Id], [r2].[RequiredAssociate_OptionalNestedAssociate_Int], [r2].[RequiredAssociate_OptionalNestedAssociate_Ints], [r2].[RequiredAssociate_OptionalNestedAssociate_Name], [r2].[RequiredAssociate_OptionalNestedAssociate_String], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r11]
    WHERE [r11].[row] <= 1
) AS [r12] ON [r10].[Key] = [r12].[Key] OR ([r10].[Key] IS NULL AND [r12].[Key] IS NULL)
LEFT JOIN (
    SELECT [r3].[RootEntityId], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[String], [r4].[AssociateTypeRootEntityId], [r4].[AssociateTypeId], [r4].[Id] AS [Id0], [r4].[Int] AS [Int0], [r4].[Ints] AS [Ints0], [r4].[Name] AS [Name0], [r4].[String] AS [String0], [r3].[OptionalNestedAssociate_Id], [r3].[OptionalNestedAssociate_Int], [r3].[OptionalNestedAssociate_Ints], [r3].[OptionalNestedAssociate_Name], [r3].[OptionalNestedAssociate_String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String]
    FROM [RelatedCollection] AS [r3]
    LEFT JOIN [RelatedCollection_NestedCollection] AS [r4] ON [r3].[RootEntityId] = [r4].[AssociateTypeRootEntityId] AND [r3].[Id] = [r4].[AssociateTypeId]
) AS [s] ON [r12].[Id] = [s].[RootEntityId]
LEFT JOIN [OptionalRelated_NestedCollection] AS [o] ON CASE
    WHEN [r12].[OptionalAssociate_Id] IS NOT NULL AND [r12].[OptionalAssociate_Int] IS NOT NULL AND [r12].[OptionalAssociate_Ints] IS NOT NULL AND [r12].[OptionalAssociate_Name] IS NOT NULL AND [r12].[OptionalAssociate_String] IS NOT NULL THEN [r12].[Id]
END = [o].[AssociateTypeRootEntityId]
LEFT JOIN [RequiredRelated_NestedCollection] AS [r5] ON [r12].[Id] = [r5].[AssociateTypeRootEntityId]
ORDER BY [r10].[c], [r10].[c0], [r10].[Key], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r5].[AssociateTypeRootEntityId], [r5].[Id]
""");
    }

    #endregion GroupBy element

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
