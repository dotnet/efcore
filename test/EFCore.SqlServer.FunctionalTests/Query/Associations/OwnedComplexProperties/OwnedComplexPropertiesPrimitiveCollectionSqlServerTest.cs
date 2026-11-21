// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesPrimitiveCollectionSqlServerTest(OwnedComplexPropertiesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedComplexPropertiesPrimitiveCollectionRelationalTestBase<OwnedComplexPropertiesSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Count()
    {
        await base.Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RequiredAssociate_Ints]) AS [r0]) = 3
ORDER BY [r].[Id]
""");
    }

    public override async Task Index()
    {
        await base.Index();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate_Ints], '$[0]') AS int) = 1
ORDER BY [r].[Id]
""");
    }

    public override async Task Contains()
    {
        await base.Contains();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE 3 IN (
    SELECT [r0].[value]
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r0]
)
ORDER BY [r].[Id]
""");
    }

    public override async Task Any_predicate()
    {
        await base.Any_predicate();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE 2 IN (
    SELECT [r0].[value]
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r0]
)
ORDER BY [r].[Id]
""");
    }

    public override async Task Nested_Count()
    {
        await base.Nested_Count();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM OPENJSON([r].[RequiredAssociate_RequiredNestedAssociate_Ints]) AS [r0]) = 3
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_Sum()
    {
        await base.Select_Sum();

        AssertSql(
            """
SELECT (
    SELECT ISNULL(SUM([r1].[value]), 0)
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r1])
FROM [RootEntity] AS [r]
WHERE (
    SELECT ISNULL(SUM([r0].[value]), 0)
    FROM OPENJSON([r].[RequiredAssociate_Ints]) WITH ([value] int '$') AS [r0]) >= 6
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
