// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public class OwnedComplexPropertiesCollectionSqlServerTest(OwnedComplexPropertiesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
    : OwnedComplexPropertiesCollectionRelationalTestBase<OwnedComplexPropertiesSqlServerFixture>(fixture, testOutputHelper)
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
    FROM [RootEntity_AssociateCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]) = 2
ORDER BY [r].[Id]
""");
    }

    public override async Task Where()
    {
        await base.Where();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM [RootEntity_AssociateCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId] AND [r0].[Int] <> 8) = 2
ORDER BY [r].[Id]
""");
    }

    public override async Task OrderBy_ElementAt()
    {
        await base.OrderBy_ElementAt();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE (
    SELECT [r0].[Int]
    FROM [RootEntity_AssociateCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    ORDER BY [r0].[Id]
    OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY) = 8
ORDER BY [r].[Id]
""");
    }

    public override async Task Distinct()
    {
        await base.Distinct();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[RootEntityId], [r2].[String], [r2].[RequiredNestedAssociate_Id], [r2].[RequiredNestedAssociate_Int], [r2].[RequiredNestedAssociate_Ints], [r2].[RequiredNestedAssociate_Name], [r2].[RequiredNestedAssociate_String], [r2].[NestedCollection], [r2].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r2] ON [r].[Id] = [r2].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection] AS [c], [r0].[OptionalNestedAssociate] AS [c0]
        FROM [RootEntity_AssociateCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId]
    ) AS [r1]) = 2
ORDER BY [r].[Id]
""");
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        await base.Distinct_projected(queryTrackingBehavior);

        if (queryTrackingBehavior is not QueryTrackingBehavior.TrackAll)
        {
            AssertSql(
                """
SELECT [r].[Id], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[c], [r1].[c0]
FROM [RootEntity] AS [r]
LEFT JOIN (
    SELECT DISTINCT [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection] AS [c], [r0].[OptionalNestedAssociate] AS [c0]
    FROM [RootEntity_AssociateCollection] AS [r0]
) AS [r1] ON [r].[Id] = [r1].[RootEntityId]
ORDER BY [r].[Id]
""");
        }
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        await base.Distinct_over_projected_nested_collection();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r2].[Id], [r2].[Int], [r2].[Ints], [r2].[Name], [r2].[RootEntityId], [r2].[String], [r2].[RequiredNestedAssociate_Id], [r2].[RequiredNestedAssociate_Int], [r2].[RequiredNestedAssociate_Ints], [r2].[RequiredNestedAssociate_Name], [r2].[RequiredNestedAssociate_String], [r2].[NestedCollection], [r2].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r2] ON [r].[Id] = [r2].[RootEntityId]
WHERE (
    SELECT COUNT(*)
    FROM (
        SELECT DISTINCT [r0].[NestedCollection] AS [c]
        FROM [RootEntity_AssociateCollection] AS [r0]
        WHERE [r].[Id] = [r0].[RootEntityId]
    ) AS [r1]) = 2
ORDER BY [r].[Id]
""");
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        await base.Distinct_over_projected_filtered_nested_collection();

        AssertSql();
    }

    public override async Task Index_constant()
    {
        await base.Index_constant();

        AssertSql();
    }

    public override async Task Index_parameter()
    {
        await base.Index_parameter();

        AssertSql();
    }

    public override async Task Index_column()
    {
        await base.Index_column();

        AssertSql();
    }

    public override async Task Index_on_nested_collection()
    {
        await base.Index_on_nested_collection();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r0].[Id], [r0].[Int], [r0].[Ints], [r0].[Name], [r0].[RootEntityId], [r0].[String], [r0].[RequiredNestedAssociate_Id], [r0].[RequiredNestedAssociate_Int], [r0].[RequiredNestedAssociate_Ints], [r0].[RequiredNestedAssociate_Name], [r0].[RequiredNestedAssociate_String], [r0].[NestedCollection], [r0].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r0] ON [r].[Id] = [r0].[RootEntityId]
WHERE CAST(JSON_VALUE([r].[RequiredAssociate_NestedCollection], '$[0].Int') AS int) = 8
ORDER BY [r].[Id]
""");
    }

    public override async Task Index_out_of_bounds()
    {
        await base.Index_out_of_bounds();

        AssertSql();
    }

    public override async Task GroupBy()
    {
        await base.GroupBy();

        AssertSql(
            """
SELECT [r].[Id], [r].[Name], [r1].[Id], [r1].[Int], [r1].[Ints], [r1].[Name], [r1].[RootEntityId], [r1].[String], [r1].[RequiredNestedAssociate_Id], [r1].[RequiredNestedAssociate_Int], [r1].[RequiredNestedAssociate_Ints], [r1].[RequiredNestedAssociate_Name], [r1].[RequiredNestedAssociate_String], [r1].[NestedCollection], [r1].[OptionalNestedAssociate], [r].[OptionalAssociate_Id], [r].[OptionalAssociate_Int], [r].[OptionalAssociate_Ints], [r].[OptionalAssociate_Name], [r].[OptionalAssociate_String], [r].[OptionalAssociate_NestedCollection], [r].[OptionalAssociate_OptionalNestedAssociate], [r].[OptionalAssociate_RequiredNestedAssociate_Id], [r].[OptionalAssociate_RequiredNestedAssociate_Int], [r].[OptionalAssociate_RequiredNestedAssociate_Ints], [r].[OptionalAssociate_RequiredNestedAssociate_Name], [r].[OptionalAssociate_RequiredNestedAssociate_String], [r].[RequiredAssociate_Id], [r].[RequiredAssociate_Int], [r].[RequiredAssociate_Ints], [r].[RequiredAssociate_Name], [r].[RequiredAssociate_String], [r].[RequiredAssociate_NestedCollection], [r].[RequiredAssociate_OptionalNestedAssociate], [r].[RequiredAssociate_RequiredNestedAssociate_Id], [r].[RequiredAssociate_RequiredNestedAssociate_Int], [r].[RequiredAssociate_RequiredNestedAssociate_Ints], [r].[RequiredAssociate_RequiredNestedAssociate_Name], [r].[RequiredAssociate_RequiredNestedAssociate_String]
FROM [RootEntity] AS [r]
LEFT JOIN [RootEntity_AssociateCollection] AS [r1] ON [r].[Id] = [r1].[RootEntityId]
WHERE 16 IN (
    SELECT ISNULL(SUM([r0].[Int]), 0)
    FROM [RootEntity_AssociateCollection] AS [r0]
    WHERE [r].[Id] = [r0].[RootEntityId]
    GROUP BY [r0].[String]
)
ORDER BY [r].[Id]
""");
    }

    public override async Task Select_within_Select_within_Select_with_aggregates()
    {
        await base.Select_within_Select_within_Select_with_aggregates();

        AssertSql(
            """
SELECT (
    SELECT ISNULL(SUM([s].[value]), 0)
    FROM [RootEntity_AssociateCollection] AS [r0]
    OUTER APPLY (
        SELECT MAX([n].[Int]) AS [value]
        FROM OPENJSON([r0].[NestedCollection], '$') WITH ([Int] int '$.Int') AS [n]
    ) AS [s]
    WHERE [r].[Id] = [r0].[RootEntityId])
FROM [RootEntity] AS [r]
""");
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());
}
