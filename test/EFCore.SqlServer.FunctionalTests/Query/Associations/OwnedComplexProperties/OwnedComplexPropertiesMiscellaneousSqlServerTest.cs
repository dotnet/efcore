// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesMiscellaneousSqlServerTest(OwnedComplexPropertiesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedComplexPropertiesMiscellaneousRelationalTestBase<OwnedComplexPropertiesSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Where_on_associate_scalar_property()
    {
        await base.Where_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_Int] = 8
ORDER BY [r].[Id]
""");
    }

    public override async Task Where_on_optional_associate_scalar_property()
    {
        await base.Where_on_optional_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[OptionalAssociate_Int] = 8
ORDER BY [r].[Id]
""");
    }

    public override async Task Where_on_nested_associate_scalar_property()
    {
        await base.Where_on_nested_associate_scalar_property();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE [r].[RequiredAssociate_RequiredNestedAssociate_Int] = 8
ORDER BY [r].[Id]
""");
    }

    public override async Task FromSql_on_root()
    {
        await base.FromSql_on_root();

        AssertSql(
            """
SELECT [m].[Id], [m].[Name], [r].[Id], [r0].[Id], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r0].[RequiredAssociate_Id], [r0].[RequiredAssociate_Int], [r0].[RequiredAssociate_Ints], [r0].[RequiredAssociate_Name], [r0].[RequiredAssociate_String], [r0].[RequiredAssociate_NestedCollection], [r0].[RequiredAssociate_OptionalNestedAssociate], [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT * FROM [RootEntity]
) AS [m]
LEFT JOIN [RootEntity] AS [r] ON [m].[Id] = [r].[Id]
LEFT JOIN [RootEntity] AS [r0] ON [m].[Id] = [r0].[Id]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [m].[Id] = [r1].[RootEntityId]
ORDER BY [m].[Id], [r].[Id], [r0].[Id]
""");
    }

    #region GroupBy element

    public override async Task Where_on_root_property_on_GroupBy_First()
    {
        await base.Where_on_root_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r8].[Id], [r8].[Name], [r6].[Key], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[RootEntityId], [r3].[String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String], [r3].[NestedCollection], [r3].[OptionalNestedAssociate], [r8].[OptionalAssociate_Id], [r8].[OptionalAssociate_Int], [r8].[OptionalAssociate_Ints], [r8].[OptionalAssociate_Name], [r8].[OptionalAssociate_String], [r8].[c], [r8].[c0], [r8].[OptionalAssociate_RequiredNestedAssociate_Id], [r8].[OptionalAssociate_RequiredNestedAssociate_Int], [r8].[OptionalAssociate_RequiredNestedAssociate_Ints], [r8].[OptionalAssociate_RequiredNestedAssociate_Name], [r8].[OptionalAssociate_RequiredNestedAssociate_String], [r8].[RequiredAssociate_Id], [r8].[RequiredAssociate_Int], [r8].[RequiredAssociate_Ints], [r8].[RequiredAssociate_Name], [r8].[RequiredAssociate_String], [r8].[c1], [r8].[c2], [r8].[RequiredAssociate_RequiredNestedAssociate_Id], [r8].[RequiredAssociate_RequiredNestedAssociate_Int], [r8].[RequiredAssociate_RequiredNestedAssociate_Ints], [r8].[RequiredAssociate_RequiredNestedAssociate_Name], [r8].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r4].[Id]
        FROM (
            SELECT [r5].[Id], [r5].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r5]
        ) AS [r4]
        WHERE ([r0].[Key] = [r4].[Key] OR ([r0].[Key] IS NULL AND [r4].[Key] IS NULL)) AND [r4].[Id] > 0
        ORDER BY [r4].[Id]) > 1
) AS [r6]
LEFT JOIN (
    SELECT [r7].[Id], [r7].[Name], [r7].[OptionalAssociate_Id], [r7].[OptionalAssociate_Int], [r7].[OptionalAssociate_Ints], [r7].[OptionalAssociate_Name], [r7].[OptionalAssociate_String], [r7].[c] AS [c], [r7].[c0] AS [c0], [r7].[OptionalAssociate_RequiredNestedAssociate_Id], [r7].[OptionalAssociate_RequiredNestedAssociate_Int], [r7].[OptionalAssociate_RequiredNestedAssociate_Ints], [r7].[OptionalAssociate_RequiredNestedAssociate_Name], [r7].[OptionalAssociate_RequiredNestedAssociate_String], [r7].[RequiredAssociate_Id], [r7].[RequiredAssociate_Int], [r7].[RequiredAssociate_Ints], [r7].[RequiredAssociate_Name], [r7].[RequiredAssociate_String], [r7].[c1] AS [c1], [r7].[c2] AS [c2], [r7].[RequiredAssociate_RequiredNestedAssociate_Id], [r7].[RequiredAssociate_RequiredNestedAssociate_Int], [r7].[RequiredAssociate_RequiredNestedAssociate_Ints], [r7].[RequiredAssociate_RequiredNestedAssociate_Name], [r7].[RequiredAssociate_RequiredNestedAssociate_String], [r7].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[c1] AS [c1], [r1].[c2] AS [c2], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_NestedCollection] AS [c], [r2].[OptionalAssociate_OptionalNestedAssociate] AS [c0], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_NestedCollection] AS [c1], [r2].[RequiredAssociate_OptionalNestedAssociate] AS [c2], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r7]
    WHERE [r7].[row] <= 1
) AS [r8] ON [r6].[Key] = [r8].[Key] OR ([r6].[Key] IS NULL AND [r8].[Key] IS NULL)
LEFT JOIN [RootEntity_AssociateCollection] AS [r3] ON [r8].[Id] = [r3].[RootEntityId]
ORDER BY [r6].[Key]
""");
    }

    public override async Task Where_on_associate_scalar_property_on_GroupBy_First()
    {
        await base.Where_on_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r8].[Id], [r8].[Name], [r6].[Key], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[RootEntityId], [r3].[String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String], [r3].[NestedCollection], [r3].[OptionalNestedAssociate], [r8].[OptionalAssociate_Id], [r8].[OptionalAssociate_Int], [r8].[OptionalAssociate_Ints], [r8].[OptionalAssociate_Name], [r8].[OptionalAssociate_String], [r8].[c], [r8].[c0], [r8].[OptionalAssociate_RequiredNestedAssociate_Id], [r8].[OptionalAssociate_RequiredNestedAssociate_Int], [r8].[OptionalAssociate_RequiredNestedAssociate_Ints], [r8].[OptionalAssociate_RequiredNestedAssociate_Name], [r8].[OptionalAssociate_RequiredNestedAssociate_String], [r8].[RequiredAssociate_Id], [r8].[RequiredAssociate_Int], [r8].[RequiredAssociate_Ints], [r8].[RequiredAssociate_Name], [r8].[RequiredAssociate_String], [r8].[c1], [r8].[c2], [r8].[RequiredAssociate_RequiredNestedAssociate_Id], [r8].[RequiredAssociate_RequiredNestedAssociate_Int], [r8].[RequiredAssociate_RequiredNestedAssociate_Ints], [r8].[RequiredAssociate_RequiredNestedAssociate_Name], [r8].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
    HAVING (
        SELECT TOP(1) [r4].[RequiredAssociate_Int]
        FROM (
            SELECT [r5].[Id], [r5].[Id] % 2 AS [Key], [r5].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r5]
        ) AS [r4]
        WHERE ([r0].[Key] = [r4].[Key] OR ([r0].[Key] IS NULL AND [r4].[Key] IS NULL)) AND [r4].[Id] > 0
        ORDER BY [r4].[Id]) = 8
) AS [r6]
LEFT JOIN (
    SELECT [r7].[Id], [r7].[Name], [r7].[OptionalAssociate_Id], [r7].[OptionalAssociate_Int], [r7].[OptionalAssociate_Ints], [r7].[OptionalAssociate_Name], [r7].[OptionalAssociate_String], [r7].[c] AS [c], [r7].[c0] AS [c0], [r7].[OptionalAssociate_RequiredNestedAssociate_Id], [r7].[OptionalAssociate_RequiredNestedAssociate_Int], [r7].[OptionalAssociate_RequiredNestedAssociate_Ints], [r7].[OptionalAssociate_RequiredNestedAssociate_Name], [r7].[OptionalAssociate_RequiredNestedAssociate_String], [r7].[RequiredAssociate_Id], [r7].[RequiredAssociate_Int], [r7].[RequiredAssociate_Ints], [r7].[RequiredAssociate_Name], [r7].[RequiredAssociate_String], [r7].[c1] AS [c1], [r7].[c2] AS [c2], [r7].[RequiredAssociate_RequiredNestedAssociate_Id], [r7].[RequiredAssociate_RequiredNestedAssociate_Int], [r7].[RequiredAssociate_RequiredNestedAssociate_Ints], [r7].[RequiredAssociate_RequiredNestedAssociate_Name], [r7].[RequiredAssociate_RequiredNestedAssociate_String], [r7].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[c1] AS [c1], [r1].[c2] AS [c2], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id]) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_NestedCollection] AS [c], [r2].[OptionalAssociate_OptionalNestedAssociate] AS [c0], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_NestedCollection] AS [c1], [r2].[RequiredAssociate_OptionalNestedAssociate] AS [c2], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
        WHERE [r1].[Id] > 0
    ) AS [r7]
    WHERE [r7].[row] <= 1
) AS [r8] ON [r6].[Key] = [r8].[Key] OR ([r6].[Key] IS NULL AND [r8].[Key] IS NULL)
LEFT JOIN [RootEntity_AssociateCollection] AS [r3] ON [r8].[Id] = [r3].[RootEntityId]
ORDER BY [r6].[Key]
""");
    }

    public override async Task Select_multiple_associates_on_GroupBy_OrderByDescending_First()
    {
        await base.Select_multiple_associates_on_GroupBy_OrderByDescending_First();

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [r8].[Id]
    FROM (
        SELECT [r9].[Id], [r9].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r9]
    ) AS [r8]
    WHERE [r10].[Key] = [r8].[Key] OR ([r10].[Key] IS NULL AND [r8].[Key] IS NULL)
    ORDER BY [r8].[Id] DESC), [r12].[Id], [r12].[Name], [r10].[Key], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[RootEntityId], [r3].[String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String], [r3].[NestedCollection], [r3].[OptionalNestedAssociate], [r12].[OptionalAssociate_Id], [r12].[OptionalAssociate_Int], [r12].[OptionalAssociate_Ints], [r12].[OptionalAssociate_Name], [r12].[OptionalAssociate_String], [r12].[c], [r12].[c0], [r12].[OptionalAssociate_RequiredNestedAssociate_Id], [r12].[OptionalAssociate_RequiredNestedAssociate_Int], [r12].[OptionalAssociate_RequiredNestedAssociate_Ints], [r12].[OptionalAssociate_RequiredNestedAssociate_Name], [r12].[OptionalAssociate_RequiredNestedAssociate_String], [r12].[RequiredAssociate_Id], [r12].[RequiredAssociate_Int], [r12].[RequiredAssociate_Ints], [r12].[RequiredAssociate_Name], [r12].[RequiredAssociate_String], [r12].[c1], [r12].[c2], [r12].[RequiredAssociate_RequiredNestedAssociate_Id], [r12].[RequiredAssociate_RequiredNestedAssociate_Int], [r12].[RequiredAssociate_RequiredNestedAssociate_Ints], [r12].[RequiredAssociate_RequiredNestedAssociate_Name], [r12].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r4].[RequiredAssociate_Int]
        FROM (
            SELECT [r5].[Id], [r5].[Id] % 2 AS [Key], [r5].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r5]
        ) AS [r4]
        WHERE [r0].[Key] = [r4].[Key] OR ([r0].[Key] IS NULL AND [r4].[Key] IS NULL)
        ORDER BY [r4].[Id] DESC) AS [c], (
        SELECT TOP(1) [r6].[Id]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE [r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)
        ORDER BY [r6].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r10]
LEFT JOIN (
    SELECT [r11].[Id], [r11].[Name], [r11].[OptionalAssociate_Id], [r11].[OptionalAssociate_Int], [r11].[OptionalAssociate_Ints], [r11].[OptionalAssociate_Name], [r11].[OptionalAssociate_String], [r11].[c] AS [c], [r11].[c0] AS [c0], [r11].[OptionalAssociate_RequiredNestedAssociate_Id], [r11].[OptionalAssociate_RequiredNestedAssociate_Int], [r11].[OptionalAssociate_RequiredNestedAssociate_Ints], [r11].[OptionalAssociate_RequiredNestedAssociate_Name], [r11].[OptionalAssociate_RequiredNestedAssociate_String], [r11].[RequiredAssociate_Id], [r11].[RequiredAssociate_Int], [r11].[RequiredAssociate_Ints], [r11].[RequiredAssociate_Name], [r11].[RequiredAssociate_String], [r11].[c1] AS [c1], [r11].[c2] AS [c2], [r11].[RequiredAssociate_RequiredNestedAssociate_Id], [r11].[RequiredAssociate_RequiredNestedAssociate_Int], [r11].[RequiredAssociate_RequiredNestedAssociate_Ints], [r11].[RequiredAssociate_RequiredNestedAssociate_Name], [r11].[RequiredAssociate_RequiredNestedAssociate_String], [r11].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[c1] AS [c1], [r1].[c2] AS [c2], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_NestedCollection] AS [c], [r2].[OptionalAssociate_OptionalNestedAssociate] AS [c0], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_NestedCollection] AS [c1], [r2].[RequiredAssociate_OptionalNestedAssociate] AS [c2], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r11]
    WHERE [r11].[row] <= 1
) AS [r12] ON [r10].[Key] = [r12].[Key] OR ([r10].[Key] IS NULL AND [r12].[Key] IS NULL)
LEFT JOIN [RootEntity_AssociateCollection] AS [r3] ON [r12].[Id] = [r3].[RootEntityId]
ORDER BY [r10].[c] DESC, [r10].[c0], [r10].[Key]
""");
    }

    public override async Task Select_multiple_associates_on_GroupBy_MaxBy()
    {
        await base.Select_multiple_associates_on_GroupBy_MaxBy();

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [r8].[Id]
    FROM (
        SELECT [r9].[Id], [r9].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r9]
    ) AS [r8]
    WHERE [r10].[Key] = [r8].[Key] OR ([r10].[Key] IS NULL AND [r8].[Key] IS NULL)
    ORDER BY [r8].[Id] DESC), [r12].[Id], [r12].[Name], [r10].[Key], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[RootEntityId], [r3].[String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String], [r3].[NestedCollection], [r3].[OptionalNestedAssociate], [r12].[OptionalAssociate_Id], [r12].[OptionalAssociate_Int], [r12].[OptionalAssociate_Ints], [r12].[OptionalAssociate_Name], [r12].[OptionalAssociate_String], [r12].[c], [r12].[c0], [r12].[OptionalAssociate_RequiredNestedAssociate_Id], [r12].[OptionalAssociate_RequiredNestedAssociate_Int], [r12].[OptionalAssociate_RequiredNestedAssociate_Ints], [r12].[OptionalAssociate_RequiredNestedAssociate_Name], [r12].[OptionalAssociate_RequiredNestedAssociate_String], [r12].[RequiredAssociate_Id], [r12].[RequiredAssociate_Int], [r12].[RequiredAssociate_Ints], [r12].[RequiredAssociate_Name], [r12].[RequiredAssociate_String], [r12].[c1], [r12].[c2], [r12].[RequiredAssociate_RequiredNestedAssociate_Id], [r12].[RequiredAssociate_RequiredNestedAssociate_Int], [r12].[RequiredAssociate_RequiredNestedAssociate_Ints], [r12].[RequiredAssociate_RequiredNestedAssociate_Name], [r12].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r4].[RequiredAssociate_Int]
        FROM (
            SELECT [r5].[Id], [r5].[Id] % 2 AS [Key], [r5].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r5]
        ) AS [r4]
        WHERE [r0].[Key] = [r4].[Key] OR ([r0].[Key] IS NULL AND [r4].[Key] IS NULL)
        ORDER BY [r4].[Id] DESC) AS [c], (
        SELECT TOP(1) [r6].[Id]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE [r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)
        ORDER BY [r6].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r10]
LEFT JOIN (
    SELECT [r11].[Id], [r11].[Name], [r11].[OptionalAssociate_Id], [r11].[OptionalAssociate_Int], [r11].[OptionalAssociate_Ints], [r11].[OptionalAssociate_Name], [r11].[OptionalAssociate_String], [r11].[c] AS [c], [r11].[c0] AS [c0], [r11].[OptionalAssociate_RequiredNestedAssociate_Id], [r11].[OptionalAssociate_RequiredNestedAssociate_Int], [r11].[OptionalAssociate_RequiredNestedAssociate_Ints], [r11].[OptionalAssociate_RequiredNestedAssociate_Name], [r11].[OptionalAssociate_RequiredNestedAssociate_String], [r11].[RequiredAssociate_Id], [r11].[RequiredAssociate_Int], [r11].[RequiredAssociate_Ints], [r11].[RequiredAssociate_Name], [r11].[RequiredAssociate_String], [r11].[c1] AS [c1], [r11].[c2] AS [c2], [r11].[RequiredAssociate_RequiredNestedAssociate_Id], [r11].[RequiredAssociate_RequiredNestedAssociate_Int], [r11].[RequiredAssociate_RequiredNestedAssociate_Ints], [r11].[RequiredAssociate_RequiredNestedAssociate_Name], [r11].[RequiredAssociate_RequiredNestedAssociate_String], [r11].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[c1] AS [c1], [r1].[c2] AS [c2], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_NestedCollection] AS [c], [r2].[OptionalAssociate_OptionalNestedAssociate] AS [c0], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_NestedCollection] AS [c1], [r2].[RequiredAssociate_OptionalNestedAssociate] AS [c2], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r11]
    WHERE [r11].[row] <= 1
) AS [r12] ON [r10].[Key] = [r12].[Key] OR ([r10].[Key] IS NULL AND [r12].[Key] IS NULL)
LEFT JOIN [RootEntity_AssociateCollection] AS [r3] ON [r12].[Id] = [r3].[RootEntityId]
ORDER BY [r10].[c] DESC, [r10].[c0], [r10].[Key]
""");
    }

    public override async Task OrderBy_associate_scalar_property_on_GroupBy_First()
    {
        await base.OrderBy_associate_scalar_property_on_GroupBy_First();

        AssertSql(
            """
SELECT [r10].[Id], [r10].[Name], [r8].[Key], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[RootEntityId], [r3].[String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String], [r3].[NestedCollection], [r3].[OptionalNestedAssociate], [r10].[OptionalAssociate_Id], [r10].[OptionalAssociate_Int], [r10].[OptionalAssociate_Ints], [r10].[OptionalAssociate_Name], [r10].[OptionalAssociate_String], [r10].[c], [r10].[c0], [r10].[OptionalAssociate_RequiredNestedAssociate_Id], [r10].[OptionalAssociate_RequiredNestedAssociate_Int], [r10].[OptionalAssociate_RequiredNestedAssociate_Ints], [r10].[OptionalAssociate_RequiredNestedAssociate_Name], [r10].[OptionalAssociate_RequiredNestedAssociate_String], [r10].[RequiredAssociate_Id], [r10].[RequiredAssociate_Int], [r10].[RequiredAssociate_Ints], [r10].[RequiredAssociate_Name], [r10].[RequiredAssociate_String], [r10].[c1], [r10].[c2], [r10].[RequiredAssociate_RequiredNestedAssociate_Id], [r10].[RequiredAssociate_RequiredNestedAssociate_Int], [r10].[RequiredAssociate_RequiredNestedAssociate_Ints], [r10].[RequiredAssociate_RequiredNestedAssociate_Name], [r10].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key], (
        SELECT TOP(1) [r4].[RequiredAssociate_Int]
        FROM (
            SELECT [r5].[Id], [r5].[Id] % 2 AS [Key], [r5].[RequiredAssociate_Int]
            FROM [RootEntity] AS [r5]
        ) AS [r4]
        WHERE [r0].[Key] = [r4].[Key] OR ([r0].[Key] IS NULL AND [r4].[Key] IS NULL)
        ORDER BY [r4].[Id] DESC) AS [c], (
        SELECT TOP(1) [r6].[Id]
        FROM (
            SELECT [r7].[Id], [r7].[Id] % 2 AS [Key]
            FROM [RootEntity] AS [r7]
        ) AS [r6]
        WHERE [r0].[Key] = [r6].[Key] OR ([r0].[Key] IS NULL AND [r6].[Key] IS NULL)
        ORDER BY [r6].[Id] DESC) AS [c0]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r8]
LEFT JOIN (
    SELECT [r9].[Id], [r9].[Name], [r9].[OptionalAssociate_Id], [r9].[OptionalAssociate_Int], [r9].[OptionalAssociate_Ints], [r9].[OptionalAssociate_Name], [r9].[OptionalAssociate_String], [r9].[c] AS [c], [r9].[c0] AS [c0], [r9].[OptionalAssociate_RequiredNestedAssociate_Id], [r9].[OptionalAssociate_RequiredNestedAssociate_Int], [r9].[OptionalAssociate_RequiredNestedAssociate_Ints], [r9].[OptionalAssociate_RequiredNestedAssociate_Name], [r9].[OptionalAssociate_RequiredNestedAssociate_String], [r9].[RequiredAssociate_Id], [r9].[RequiredAssociate_Int], [r9].[RequiredAssociate_Ints], [r9].[RequiredAssociate_Name], [r9].[RequiredAssociate_String], [r9].[c1] AS [c1], [r9].[c2] AS [c2], [r9].[RequiredAssociate_RequiredNestedAssociate_Id], [r9].[RequiredAssociate_RequiredNestedAssociate_Int], [r9].[RequiredAssociate_RequiredNestedAssociate_Ints], [r9].[RequiredAssociate_RequiredNestedAssociate_Name], [r9].[RequiredAssociate_RequiredNestedAssociate_String], [r9].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[c] AS [c], [r1].[c0] AS [c0], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[c1] AS [c1], [r1].[c2] AS [c2], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[Id] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_NestedCollection] AS [c], [r2].[OptionalAssociate_OptionalNestedAssociate] AS [c0], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_NestedCollection] AS [c1], [r2].[RequiredAssociate_OptionalNestedAssociate] AS [c2], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r9]
    WHERE [r9].[row] <= 1
) AS [r10] ON [r8].[Key] = [r10].[Key] OR ([r8].[Key] IS NULL AND [r10].[Key] IS NULL)
LEFT JOIN [RootEntity_AssociateCollection] AS [r3] ON [r10].[Id] = [r3].[RootEntityId]
ORDER BY [r8].[c], [r8].[c0], [r8].[Key]
""");
    }

    #endregion GroupBy element

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
