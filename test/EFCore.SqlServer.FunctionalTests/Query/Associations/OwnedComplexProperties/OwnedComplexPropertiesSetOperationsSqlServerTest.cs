// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesSetOperationsSqlServerTest(OwnedComplexPropertiesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedComplexPropertiesSetOperationsRelationalTestBase<OwnedComplexPropertiesSqlServerFixture>(fixture, testOutputHelper)
{
    public override async Task Over_associate_collections()
    {
        await base.Over_associate_collections();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[RootEntityId], [r2].[String], [r2].[RequiredNestedAssociate_Id], [r2].[RequiredNestedAssociate_Int], [r2].[RequiredNestedAssociate_Ints], [r2].[RequiredNestedAssociate_Name], [r2].[RequiredNestedAssociate_String], [r2].[NestedCollection], [r2].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r2] ON [r].[Id] = [r2].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM [RootEntity_AssociateCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM [RootEntity_AssociateCollection] AS [r1]
        WHERE [r].[Id] = [r1].[RootEntityId] AND [r1].[String] = N'foo'
    ) AS [u]) = 4
ORDER BY [r].[Id]
""");
    }

    public override async Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_associate_collection_projected(queryTrackingBehavior);

        AssertSql();
    }

    public override async Task Over_assocate_collection_Select_nested_with_aggregates_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Over_assocate_collection_Select_nested_with_aggregates_projected(queryTrackingBehavior);

        AssertSql(
            """
SELECT (
    SELECT ISNULL(SUM([s].[value]), 0)
    FROM (
        SELECT [r0].[NestedCollection] AS [NestedCollection]
        FROM [RootEntity_AssociateCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] = 8
        UNION ALL
        SELECT [r1].[NestedCollection] AS [NestedCollection]
        FROM [RootEntity_AssociateCollection] AS [r1]
        WHERE [r].[Id] = [r1].[RootEntityId] AND [r1].[String] = N'foo'
    ) AS [u]
    OUTER APPLY (
        SELECT ISNULL(SUM([n].[Int]), 0) AS [value]
        FROM OPENJSON([u].[NestedCollection], '$') WITH ([Int] int '$.Int') AS [n]
    ) AS [s])
FROM [RootEntity] AS [r]
""");
    }

    public override async Task Over_nested_associate_collection()
    {
        await base.Over_nested_associate_collection();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[RootEntityId], [r2].[String], [r2].[RequiredNestedAssociate_Id], [r2].[RequiredNestedAssociate_Int], [r2].[RequiredNestedAssociate_Ints], [r2].[RequiredNestedAssociate_Name], [r2].[RequiredNestedAssociate_String], [r2].[NestedCollection], [r2].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r2] ON [r].[Id] = [r2].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT 1 AS empty
        FROM OPENJSON([r].[RequiredAssociate_NestedCollection], '$') WITH ([Int] int '$.Int') AS [r0]
        WHERE [r0].[Int] = 8
        UNION ALL
        SELECT 1 AS empty
        FROM OPENJSON([r].[RequiredAssociate_NestedCollection], '$') WITH ([String] nvarchar(max) '$.String') AS [r1]
        WHERE [r1].[String] = N'foo'
    ) AS [u]) = 4
ORDER BY [r].[Id]
""");
    }

    public override async Task Over_different_collection_properties()
    {
        await base.Over_different_collection_properties();

        AssertSql();
    }

    public override async Task Union_over_associate_collections()
    {
        await base.Union_over_associate_collections();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[RootEntityId], [r2].[String], [r2].[RequiredNestedAssociate_Id], [r2].[RequiredNestedAssociate_Int], [r2].[RequiredNestedAssociate_Ints], [r2].[RequiredNestedAssociate_Name], [r2].[RequiredNestedAssociate_String], [r2].[NestedCollection], [r2].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r2] ON [r].[Id] = [r2].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection] AS [NestedCollection], [r0].[OptionalNestedAssociate] AS [OptionalNestedAssociate]
        FROM [RootEntity_AssociateCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] = 8
        UNION
        SELECT [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection] AS [NestedCollection], [r1].[OptionalNestedAssociate] AS [OptionalNestedAssociate]
        FROM [RootEntity_AssociateCollection] AS [r1]
        WHERE [r].[Id] = [r1].[RootEntityId] AND [r1].[String] = N'foo'
    ) AS [u]) = 2
ORDER BY [r].[Id]
""");
    }

    public override async Task Union_associates_then_project_nested_associate()
    {
        await base.Union_associates_then_project_nested_associate();

        AssertSql(
            """
SELECT [u].[OptionalNestedAssociate]
FROM (
    SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection] AS [NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate] AS [OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
    FROM [RootEntity] AS [r]
    WHERE [r].[Id] < 3
    UNION
    SELECT [r0].[Id], [r0].[RequiredAssociate_Id], [r0].[RequiredAssociate_Int], [r0].[RequiredAssociate_Ints], [r0].[RequiredAssociate_Name], [r0].[RequiredAssociate_String], [r0].[RequiredAssociate_NestedCollection] AS [NestedCollection], [r0].[RequiredAssociate_OptionalNestedAssociate] AS [OptionalNestedAssociate], [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
    FROM [RootEntity] AS [r0]
    WHERE [r0].[Id] > 4
) AS [u]
""");
    }

    public override async Task Union_associates_then_filter_on_nested_associate()
    {
        await base.Union_associates_then_filter_on_nested_associate();

        AssertSql(
            """
SELECT [u].[RequiredAssociate_Id]
FROM (
    SELECT [r].[Id], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection] AS [NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate] AS [OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
    FROM [RootEntity] AS [r]
    WHERE [r].[Id] < 3
    UNION
    SELECT [r0].[Id], [r0].[RequiredAssociate_Id], [r0].[RequiredAssociate_Int], [r0].[RequiredAssociate_Ints], [r0].[RequiredAssociate_Name], [r0].[RequiredAssociate_String], [r0].[RequiredAssociate_NestedCollection] AS [NestedCollection], [r0].[RequiredAssociate_OptionalNestedAssociate] AS [OptionalNestedAssociate], [r0].[RequiredAssociate_RequiredNestedAssociate_Id], [r0].[RequiredAssociate_RequiredNestedAssociate_Int], [r0].[RequiredAssociate_RequiredNestedAssociate_Ints], [r0].[RequiredAssociate_RequiredNestedAssociate_Name], [r0].[RequiredAssociate_RequiredNestedAssociate_String]
    FROM [RootEntity] AS [r0]
    WHERE [r0].[Id] > 4
) AS [u]
WHERE CAST(JSON_VALUE([u].[OptionalNestedAssociate], '$.Int') AS int) = 8
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
