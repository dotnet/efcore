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
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId]
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
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId]
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
ORDER BY [r].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r2].[AssociateTypeRootEntityId]
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
ORDER BY [m].[Id], [r].[Id], [r0].[Id], [s].[RootEntityId], [s].[Id], [s].[AssociateTypeRootEntityId], [s].[AssociateTypeId], [s].[Id0], [o].[AssociateTypeRootEntityId], [o].[Id], [r3].[AssociateTypeRootEntityId]
""");
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
