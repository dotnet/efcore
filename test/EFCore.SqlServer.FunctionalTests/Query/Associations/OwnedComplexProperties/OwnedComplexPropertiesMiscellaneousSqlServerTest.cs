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

    #region GroupBy

    public override async Task GroupBy_OrderByDescending_First_on_associate_scalar_property()
    {
        await base.GroupBy_OrderByDescending_First_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r6].[Id], [r6].[Name], [r4].[Key], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[RootEntityId], [r3].[String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String], [r3].[NestedCollection], [r3].[OptionalNestedAssociate], [r6].[OptionalAssociate_Id], [r6].[OptionalAssociate_Int], [r6].[OptionalAssociate_Ints], [r6].[OptionalAssociate_Name], [r6].[OptionalAssociate_String], [r6].[c], [r6].[c0], [r6].[OptionalAssociate_RequiredNestedAssociate_Id], [r6].[OptionalAssociate_RequiredNestedAssociate_Int], [r6].[OptionalAssociate_RequiredNestedAssociate_Ints], [r6].[OptionalAssociate_RequiredNestedAssociate_Name], [r6].[OptionalAssociate_RequiredNestedAssociate_String], [r6].[RequiredAssociate_Id], [r6].[RequiredAssociate_Int], [r6].[RequiredAssociate_Ints], [r6].[RequiredAssociate_Name], [r6].[RequiredAssociate_String], [r6].[c1], [r6].[c2], [r6].[RequiredAssociate_RequiredNestedAssociate_Id], [r6].[RequiredAssociate_RequiredNestedAssociate_Int], [r6].[RequiredAssociate_RequiredNestedAssociate_Ints], [r6].[RequiredAssociate_RequiredNestedAssociate_Name], [r6].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r4]
LEFT JOIN (
    SELECT [r5].[Id], [r5].[Name], [r5].[OptionalAssociate_Id], [r5].[OptionalAssociate_Int], [r5].[OptionalAssociate_Ints], [r5].[OptionalAssociate_Name], [r5].[OptionalAssociate_String], [r5].[c] AS [c], [r5].[c0] AS [c0], [r5].[OptionalAssociate_RequiredNestedAssociate_Id], [r5].[OptionalAssociate_RequiredNestedAssociate_Int], [r5].[OptionalAssociate_RequiredNestedAssociate_Ints], [r5].[OptionalAssociate_RequiredNestedAssociate_Name], [r5].[OptionalAssociate_RequiredNestedAssociate_String], [r5].[RequiredAssociate_Id], [r5].[RequiredAssociate_Int], [r5].[RequiredAssociate_Ints], [r5].[RequiredAssociate_Name], [r5].[RequiredAssociate_String], [r5].[c1] AS [c1], [r5].[c2] AS [c2], [r5].[RequiredAssociate_RequiredNestedAssociate_Id], [r5].[RequiredAssociate_RequiredNestedAssociate_Int], [r5].[RequiredAssociate_RequiredNestedAssociate_Ints], [r5].[RequiredAssociate_RequiredNestedAssociate_Name], [r5].[RequiredAssociate_RequiredNestedAssociate_String], [r5].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[c1] AS [c], [r1].[c2] AS [c0], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[c] AS [c1], [r1].[c0] AS [c2], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[RequiredAssociate_Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_NestedCollection] AS [c], [r2].[RequiredAssociate_OptionalNestedAssociate] AS [c0], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_NestedCollection] AS [c1], [r2].[OptionalAssociate_OptionalNestedAssociate] AS [c2], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r5]
    WHERE [r5].[row] <= 1
) AS [r6] ON [r4].[Key] = [r6].[Key] OR ([r4].[Key] IS NULL AND [r6].[Key] IS NULL)
LEFT JOIN [RootEntity_AssociateCollection] AS [r3] ON [r6].[Id] = [r3].[RootEntityId]
ORDER BY [r4].[Key]
""");
    }

    public override async Task GroupBy_MaxBy_on_associate_scalar_property()
    {
        await base.GroupBy_MaxBy_on_associate_scalar_property();

        AssertSql(
            """
SELECT [r6].[Id], [r6].[Name], [r4].[Key], [r3].[Id], [r3].[Int], [r3].[Ints], [r3].[Name], [r3].[RootEntityId], [r3].[String], [r3].[RequiredNestedAssociate_Id], [r3].[RequiredNestedAssociate_Int], [r3].[RequiredNestedAssociate_Ints], [r3].[RequiredNestedAssociate_Name], [r3].[RequiredNestedAssociate_String], [r3].[NestedCollection], [r3].[OptionalNestedAssociate], [r6].[OptionalAssociate_Id], [r6].[OptionalAssociate_Int], [r6].[OptionalAssociate_Ints], [r6].[OptionalAssociate_Name], [r6].[OptionalAssociate_String], [r6].[c], [r6].[c0], [r6].[OptionalAssociate_RequiredNestedAssociate_Id], [r6].[OptionalAssociate_RequiredNestedAssociate_Int], [r6].[OptionalAssociate_RequiredNestedAssociate_Ints], [r6].[OptionalAssociate_RequiredNestedAssociate_Name], [r6].[OptionalAssociate_RequiredNestedAssociate_String], [r6].[RequiredAssociate_Id], [r6].[RequiredAssociate_Int], [r6].[RequiredAssociate_Ints], [r6].[RequiredAssociate_Name], [r6].[RequiredAssociate_String], [r6].[c1], [r6].[c2], [r6].[RequiredAssociate_RequiredNestedAssociate_Id], [r6].[RequiredAssociate_RequiredNestedAssociate_Int], [r6].[RequiredAssociate_RequiredNestedAssociate_Ints], [r6].[RequiredAssociate_RequiredNestedAssociate_Name], [r6].[RequiredAssociate_RequiredNestedAssociate_String]
FROM (
    SELECT [r0].[Key]
    FROM (
        SELECT [r].[Id] % 2 AS [Key]
        FROM [RootEntity] AS [r]
    ) AS [r0]
    GROUP BY [r0].[Key]
) AS [r4]
LEFT JOIN (
    SELECT [r5].[Id], [r5].[Name], [r5].[OptionalAssociate_Id], [r5].[OptionalAssociate_Int], [r5].[OptionalAssociate_Ints], [r5].[OptionalAssociate_Name], [r5].[OptionalAssociate_String], [r5].[c] AS [c], [r5].[c0] AS [c0], [r5].[OptionalAssociate_RequiredNestedAssociate_Id], [r5].[OptionalAssociate_RequiredNestedAssociate_Int], [r5].[OptionalAssociate_RequiredNestedAssociate_Ints], [r5].[OptionalAssociate_RequiredNestedAssociate_Name], [r5].[OptionalAssociate_RequiredNestedAssociate_String], [r5].[RequiredAssociate_Id], [r5].[RequiredAssociate_Int], [r5].[RequiredAssociate_Ints], [r5].[RequiredAssociate_Name], [r5].[RequiredAssociate_String], [r5].[c1] AS [c1], [r5].[c2] AS [c2], [r5].[RequiredAssociate_RequiredNestedAssociate_Id], [r5].[RequiredAssociate_RequiredNestedAssociate_Int], [r5].[RequiredAssociate_RequiredNestedAssociate_Ints], [r5].[RequiredAssociate_RequiredNestedAssociate_Name], [r5].[RequiredAssociate_RequiredNestedAssociate_String], [r5].[Key]
    FROM (
        SELECT [r1].[Id], [r1].[Name], [r1].[OptionalAssociate_Id], [r1].[OptionalAssociate_Int], [r1].[OptionalAssociate_Ints], [r1].[OptionalAssociate_Name], [r1].[OptionalAssociate_String], [r1].[c1] AS [c], [r1].[c2] AS [c0], [r1].[OptionalAssociate_RequiredNestedAssociate_Id], [r1].[OptionalAssociate_RequiredNestedAssociate_Int], [r1].[OptionalAssociate_RequiredNestedAssociate_Ints], [r1].[OptionalAssociate_RequiredNestedAssociate_Name], [r1].[OptionalAssociate_RequiredNestedAssociate_String], [r1].[RequiredAssociate_Id], [r1].[RequiredAssociate_Int], [r1].[RequiredAssociate_Ints], [r1].[RequiredAssociate_Name], [r1].[RequiredAssociate_String], [r1].[c] AS [c1], [r1].[c0] AS [c2], [r1].[RequiredAssociate_RequiredNestedAssociate_Id], [r1].[RequiredAssociate_RequiredNestedAssociate_Int], [r1].[RequiredAssociate_RequiredNestedAssociate_Ints], [r1].[RequiredAssociate_RequiredNestedAssociate_Name], [r1].[RequiredAssociate_RequiredNestedAssociate_String], [r1].[Key], ROW_NUMBER() OVER(PARTITION BY [r1].[Key] ORDER BY [r1].[RequiredAssociate_Int] DESC) AS [row]
        FROM (
            SELECT [r2].[Id], [r2].[Name], [r2].[Id] % 2 AS [Key], [r2].[RequiredAssociate_Id], [r2].[RequiredAssociate_Int], [r2].[RequiredAssociate_Ints], [r2].[RequiredAssociate_Name], [r2].[RequiredAssociate_String], [r2].[RequiredAssociate_NestedCollection] AS [c], [r2].[RequiredAssociate_OptionalNestedAssociate] AS [c0], [r2].[RequiredAssociate_RequiredNestedAssociate_Id], [r2].[RequiredAssociate_RequiredNestedAssociate_Int], [r2].[RequiredAssociate_RequiredNestedAssociate_Ints], [r2].[RequiredAssociate_RequiredNestedAssociate_Name], [r2].[RequiredAssociate_RequiredNestedAssociate_String], [r2].[OptionalAssociate_Id], [r2].[OptionalAssociate_Int], [r2].[OptionalAssociate_Ints], [r2].[OptionalAssociate_Name], [r2].[OptionalAssociate_String], [r2].[OptionalAssociate_NestedCollection] AS [c1], [r2].[OptionalAssociate_OptionalNestedAssociate] AS [c2], [r2].[OptionalAssociate_RequiredNestedAssociate_Id], [r2].[OptionalAssociate_RequiredNestedAssociate_Int], [r2].[OptionalAssociate_RequiredNestedAssociate_Ints], [r2].[OptionalAssociate_RequiredNestedAssociate_Name], [r2].[OptionalAssociate_RequiredNestedAssociate_String]
            FROM [RootEntity] AS [r2]
        ) AS [r1]
    ) AS [r5]
    WHERE [r5].[row] <= 1
) AS [r6] ON [r4].[Key] = [r6].[Key] OR ([r4].[Key] IS NULL AND [r6].[Key] IS NULL)
LEFT JOIN [RootEntity_AssociateCollection] AS [r3] ON [r6].[Id] = [r3].[RootEntityId]
ORDER BY [r4].[Key]
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
