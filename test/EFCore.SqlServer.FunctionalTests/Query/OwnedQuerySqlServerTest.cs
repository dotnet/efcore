// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class OwnedQuerySqlServerTest : OwnedQueryRelationalTestBase<OwnedQuerySqlServerTest.OwnedQuerySqlServerFixture>
{
    public OwnedQuerySqlServerTest(OwnedQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Query_with_owned_entity_equality_operator(bool async)
    {
        await base.Query_with_owned_entity_equality_operator(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o1].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
CROSS JOIN (
    SELECT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Discriminator] = N'LeafB'
) AS [o1]
LEFT JOIN (
    SELECT [o2].[ClientId], [o2].[Id], [o2].[OrderDate], [o3].[OrderClientId], [o3].[OrderId], [o3].[Id] AS [Id0], [o3].[Detail]
    FROM [Order] AS [o2]
    LEFT JOIN [OrderDetail] AS [o3] ON [o2].[ClientId] = [o3].[OrderClientId] AND [o2].[Id] = [o3].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE 0 = 1
ORDER BY [o].[Id], [o1].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Query_with_owned_entity_equality_method(bool async)
    {
        await base.Query_with_owned_entity_equality_method(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o1].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
CROSS JOIN (
    SELECT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Discriminator] = N'LeafB'
) AS [o1]
LEFT JOIN (
    SELECT [o2].[ClientId], [o2].[Id], [o2].[OrderDate], [o3].[OrderClientId], [o3].[OrderId], [o3].[Id] AS [Id0], [o3].[Detail]
    FROM [Order] AS [o2]
    LEFT JOIN [OrderDetail] AS [o3] ON [o2].[ClientId] = [o3].[OrderClientId] AND [o2].[Id] = [o3].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE 0 = 1
ORDER BY [o].[Id], [o1].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Query_with_owned_entity_equality_object_method(bool async)
    {
        await base.Query_with_owned_entity_equality_object_method(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o1].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
CROSS JOIN (
    SELECT [o0].[Id]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Discriminator] = N'LeafB'
) AS [o1]
LEFT JOIN (
    SELECT [o2].[ClientId], [o2].[Id], [o2].[OrderDate], [o3].[OrderClientId], [o3].[OrderId], [o3].[Id] AS [Id0], [o3].[Detail]
    FROM [Order] AS [o2]
    LEFT JOIN [OrderDetail] AS [o3] ON [o2].[ClientId] = [o3].[OrderClientId] AND [o2].[Id] = [o3].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE 0 = 1
ORDER BY [o].[Id], [o1].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Query_for_base_type_loads_all_owned_navs(bool async)
    {
        await base.Query_for_base_type_loads_all_owned_navs(async);

        // See issue #10067
        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task No_ignored_include_warning_when_implicit_load(bool async)
    {
        await base.No_ignored_include_warning_when_implicit_load(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task Query_for_branch_type_loads_all_owned_navs(bool async)
    {
        await base.Query_for_branch_type_loads_all_owned_navs(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Query_for_branch_type_loads_all_owned_navs_tracking(bool async)
    {
        await base.Query_for_branch_type_loads_all_owned_navs_tracking(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Query_for_leaf_type_loads_all_owned_navs(bool async)
    {
        await base.Query_for_leaf_type_loads_all_owned_navs(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Query_when_subquery(bool async)
    {
        await base.Query_when_subquery(async);

        AssertSql(
            """
@__p_0='5'

SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o3].[PersonAddress_AddressLine], [o3].[PersonAddress_PlaceType], [o3].[PersonAddress_ZipCode], [o3].[PersonAddress_Country_Name], [o3].[PersonAddress_Country_PlanetId], [o3].[BranchAddress_BranchName], [o3].[BranchAddress_PlaceType], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o3].[LeafBAddress_LeafBType], [o3].[LeafBAddress_PlaceType], [o3].[LeafBAddress_Country_Name], [o3].[LeafBAddress_Country_PlanetId], [o3].[LeafAAddress_LeafType], [o3].[LeafAAddress_PlaceType], [o3].[LeafAAddress_Country_Name], [o3].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [o0].[Id], [o0].[Discriminator], [o0].[Name], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_PlaceType], [o0].[PersonAddress_ZipCode], [o0].[PersonAddress_Country_Name], [o0].[PersonAddress_Country_PlanetId], [o0].[BranchAddress_BranchName], [o0].[BranchAddress_PlaceType], [o0].[BranchAddress_Country_Name], [o0].[BranchAddress_Country_PlanetId], [o0].[LeafBAddress_LeafBType], [o0].[LeafBAddress_PlaceType], [o0].[LeafBAddress_Country_Name], [o0].[LeafBAddress_Country_PlanetId], [o0].[LeafAAddress_LeafType], [o0].[LeafAAddress_PlaceType], [o0].[LeafAAddress_Country_Name], [o0].[LeafAAddress_Country_PlanetId]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o]
    ) AS [o0]
    ORDER BY [o0].[Id]
) AS [o3]
LEFT JOIN (
    SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id] AS [Id0], [o2].[Detail]
    FROM [Order] AS [o1]
    LEFT JOIN [OrderDetail] AS [o2] ON [o1].[ClientId] = [o2].[OrderClientId] AND [o1].[Id] = [o2].[OrderId]
) AS [s] ON [o3].[Id] = [s].[ClientId]
ORDER BY [o3].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_projecting_scalar(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_projecting_scalar(async);

        AssertSql(
            """
SELECT [o].[PersonAddress_Country_Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[PersonAddress_Country_Name] = N'USA'
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_projecting_entity(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_projecting_entity(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[PersonAddress_Country_Name] = N'USA'
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_collection(bool async)
    {
        await base.Navigation_rewrite_on_owned_collection(async);

        AssertSql(
            """
SELECT [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id] AS [Id0], [o2].[Detail]
    FROM [Order] AS [o1]
    LEFT JOIN [OrderDetail] AS [o2] ON [o1].[ClientId] = [o2].[OrderClientId] AND [o1].[Id] = [o2].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o0]
    WHERE [o].[Id] = [o0].[ClientId]) > 0
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_collection_with_composition(bool async)
    {
        await base.Navigation_rewrite_on_owned_collection_with_composition(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT TOP(1) CASE
        WHEN [o0].[Id] <> 42 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
    FROM [Order] AS [o0]
    WHERE [o].[Id] = [o0].[ClientId]
    ORDER BY [o0].[Id]), CAST(0 AS bit))
FROM [OwnedPerson] AS [o]
ORDER BY [o].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_collection_with_composition_complex(bool async)
    {
        await base.Navigation_rewrite_on_owned_collection_with_composition_complex(async);

        AssertSql(
            """
SELECT (
    SELECT TOP(1) [o1].[PersonAddress_Country_Name]
    FROM [Order] AS [o0]
    LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[ClientId] = [o1].[Id]
    WHERE [o].[Id] = [o0].[ClientId]
    ORDER BY [o0].[Id])
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task SelectMany_on_owned_collection(bool async)
    {
        await base.SelectMany_on_owned_collection(async);

        AssertSql(
            """
SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o].[Id], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id], [o1].[Detail]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o1].[OrderClientId], [o1].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Name], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
""");
    }

    public override async Task Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(bool async)
    {
        await base.Filter_owned_entity_chained_with_regular_entity_followed_by_projecting_owned_collection(async);

        AssertSql(
            """
SELECT [o].[Id], [p].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [p].[Id] <> 42 OR [p].[Id] IS NULL
ORDER BY [o].[Id], [p].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Project_multiple_owned_navigations(bool async)
    {
        await base.Project_multiple_owned_navigations(async);

        AssertSql(
            """
SELECT [o].[Id], [p].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [p].[Name], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
ORDER BY [o].[Id], [p].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Project_multiple_owned_navigations_with_expansion_on_owned_collections(bool async)
    {
        await base.Project_multiple_owned_navigations_with_expansion_on_owned_collections(async);

        AssertSql(
            """
SELECT (
    SELECT COUNT(*)
    FROM [Order] AS [o0]
    LEFT JOIN [OwnedPerson] AS [o1] ON [o0].[ClientId] = [o1].[Id]
    LEFT JOIN [Planet] AS [p0] ON [o1].[PersonAddress_Country_PlanetId] = [p0].[Id]
    LEFT JOIN [Star] AS [s] ON [p0].[StarId] = [s].[Id]
    WHERE [o].[Id] = [o0].[ClientId] AND ([s].[Id] <> 42 OR [s].[Id] IS NULL)) AS [Count], [p].[Id], [p].[Name], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_filter(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [p].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [p].[Id] <> 7 OR [p].[Id] IS NULL
ORDER BY [o].[Id], [p].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_property(async);

        AssertSql(
            """
SELECT [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection(async);

        AssertSql(
            """
SELECT [o].[Id], [p].[Id], [m].[Id], [m].[Diameter], [m].[PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
ORDER BY [o].[Id], [p].[Id]
""");
    }

    public override async Task SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(bool async)
    {
        await base.SelectMany_on_owned_reference_followed_by_regular_entity_and_collection(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Diameter], [m].[PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
""");
    }

    public override async Task SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(bool async)
    {
        await base.SelectMany_on_owned_reference_with_entity_in_between_ending_in_owned_collection(async);

        AssertSql(
            """
SELECT [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
INNER JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[Name], [o].[Id], [p].[Id], [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
ORDER BY [o].[Id], [p].[Id], [s].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(
        bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_and_scalar(async);

        AssertSql(
            """
SELECT [s].[Name]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
""");
    }

    public override async Task
        Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_another_reference_in_predicate_and_projection(
            async);

        AssertSql(
            """
SELECT [s].[Id], [s].[Name], [o].[Id], [p].[Id], [e].[Id], [e].[Name], [e].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
LEFT JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
WHERE [s].[Name] = N'Sol'
ORDER BY [o].[Id], [p].[Id], [s].[Id]
""");
    }

    public override async Task Query_with_OfType_eagerly_loads_correct_owned_navigations(bool async)
    {
        await base.Query_with_OfType_eagerly_loads_correct_owned_navigations(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Preserve_includes_when_applying_skip_take_after_anonymous_type_select(bool async)
    {
        await base.Preserve_includes_when_applying_skip_take_after_anonymous_type_select(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='100'

SELECT [o3].[Id], [o3].[Discriminator], [o3].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o3].[PersonAddress_AddressLine], [o3].[PersonAddress_PlaceType], [o3].[PersonAddress_ZipCode], [o3].[PersonAddress_Country_Name], [o3].[PersonAddress_Country_PlanetId], [o3].[BranchAddress_BranchName], [o3].[BranchAddress_PlaceType], [o3].[BranchAddress_Country_Name], [o3].[BranchAddress_Country_PlanetId], [o3].[LeafBAddress_LeafBType], [o3].[LeafBAddress_PlaceType], [o3].[LeafBAddress_Country_Name], [o3].[LeafBAddress_Country_PlanetId], [o3].[LeafAAddress_LeafType], [o3].[LeafAAddress_PlaceType], [o3].[LeafAAddress_Country_Name], [o3].[LeafAAddress_Country_PlanetId], [o3].[c]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], (
        SELECT COUNT(*)
        FROM [OwnedPerson] AS [o2]) AS [c]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o3]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o3].[Id] = [s].[ClientId]
ORDER BY [o3].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Unmapped_property_projection_loads_owned_navigations(bool async)
    {
        await base.Unmapped_property_projection_loads_owned_navigations(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Client_method_skip_loads_owned_navigations(bool async)
    {
        await base.Client_method_skip_loads_owned_navigations(async);

        AssertSql(
            """
@__p_0='1'

SELECT [o2].[Id], [o2].[Discriminator], [o2].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o2].[PersonAddress_AddressLine], [o2].[PersonAddress_PlaceType], [o2].[PersonAddress_ZipCode], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId], [o2].[BranchAddress_BranchName], [o2].[BranchAddress_PlaceType], [o2].[BranchAddress_Country_Name], [o2].[BranchAddress_Country_PlanetId], [o2].[LeafBAddress_LeafBType], [o2].[LeafBAddress_PlaceType], [o2].[LeafBAddress_Country_Name], [o2].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [o2]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o2].[Id] = [s].[ClientId]
ORDER BY [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Client_method_take_loads_owned_navigations(bool async)
    {
        await base.Client_method_take_loads_owned_navigations(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o2].[Id], [o2].[Discriminator], [o2].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o2].[PersonAddress_AddressLine], [o2].[PersonAddress_PlaceType], [o2].[PersonAddress_ZipCode], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId], [o2].[BranchAddress_BranchName], [o2].[BranchAddress_PlaceType], [o2].[BranchAddress_Country_Name], [o2].[BranchAddress_Country_PlanetId], [o2].[LeafBAddress_LeafBType], [o2].[LeafBAddress_PlaceType], [o2].[LeafBAddress_Country_Name], [o2].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
) AS [o2]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o2].[Id] = [s].[ClientId]
ORDER BY [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Client_method_skip_take_loads_owned_navigations(bool async)
    {
        await base.Client_method_skip_take_loads_owned_navigations(async);

        AssertSql(
            """
@__p_0='1'
@__p_1='2'

SELECT [o2].[Id], [o2].[Discriminator], [o2].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o2].[PersonAddress_AddressLine], [o2].[PersonAddress_PlaceType], [o2].[PersonAddress_ZipCode], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId], [o2].[BranchAddress_BranchName], [o2].[BranchAddress_PlaceType], [o2].[BranchAddress_Country_Name], [o2].[BranchAddress_Country_PlanetId], [o2].[LeafBAddress_LeafBType], [o2].[LeafBAddress_PlaceType], [o2].[LeafBAddress_Country_Name], [o2].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o2]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o2].[Id] = [s].[ClientId]
ORDER BY [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Client_method_skip_loads_owned_navigations_variation_2(bool async)
    {
        await base.Client_method_skip_loads_owned_navigations_variation_2(async);

        AssertSql(
            """
@__p_0='1'

SELECT [o2].[Id], [o2].[Discriminator], [o2].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o2].[PersonAddress_AddressLine], [o2].[PersonAddress_PlaceType], [o2].[PersonAddress_ZipCode], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId], [o2].[BranchAddress_BranchName], [o2].[BranchAddress_PlaceType], [o2].[BranchAddress_Country_Name], [o2].[BranchAddress_Country_PlanetId], [o2].[LeafBAddress_LeafBType], [o2].[LeafBAddress_PlaceType], [o2].[LeafBAddress_Country_Name], [o2].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS
) AS [o2]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o2].[Id] = [s].[ClientId]
ORDER BY [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Client_method_take_loads_owned_navigations_variation_2(bool async)
    {
        await base.Client_method_take_loads_owned_navigations_variation_2(async);

        AssertSql(
            """
@__p_0='2'

SELECT [o2].[Id], [o2].[Discriminator], [o2].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o2].[PersonAddress_AddressLine], [o2].[PersonAddress_PlaceType], [o2].[PersonAddress_ZipCode], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId], [o2].[BranchAddress_BranchName], [o2].[BranchAddress_PlaceType], [o2].[BranchAddress_Country_Name], [o2].[BranchAddress_Country_PlanetId], [o2].[LeafBAddress_LeafBType], [o2].[LeafBAddress_PlaceType], [o2].[LeafBAddress_Country_Name], [o2].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT TOP(@__p_0) [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
) AS [o2]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o2].[Id] = [s].[ClientId]
ORDER BY [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Client_method_skip_take_loads_owned_navigations_variation_2(bool async)
    {
        await base.Client_method_skip_take_loads_owned_navigations_variation_2(async);

        AssertSql(
            """
@__p_0='1'
@__p_1='2'

SELECT [o2].[Id], [o2].[Discriminator], [o2].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o2].[PersonAddress_AddressLine], [o2].[PersonAddress_PlaceType], [o2].[PersonAddress_ZipCode], [o2].[PersonAddress_Country_Name], [o2].[PersonAddress_Country_PlanetId], [o2].[BranchAddress_BranchName], [o2].[BranchAddress_PlaceType], [o2].[BranchAddress_Country_Name], [o2].[BranchAddress_Country_PlanetId], [o2].[LeafBAddress_LeafBType], [o2].[LeafBAddress_PlaceType], [o2].[LeafBAddress_Country_Name], [o2].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
    ORDER BY [o].[Id]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o2]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o2].[Id] = [s].[ClientId]
ORDER BY [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Where_owned_collection_navigation_ToList_Count(bool async)
    {
        await base.Where_owned_collection_navigation_ToList_Count(async);

        AssertSql(
            """
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_ToArray_Count(bool async)
    {
        await base.Where_collection_navigation_ToArray_Count(async);

        AssertSql(
            """
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_AsEnumerable_Count(bool async)
    {
        await base.Where_collection_navigation_AsEnumerable_Count(async);

        AssertSql(
            """
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_ToList_Count_member(bool async)
    {
        await base.Where_collection_navigation_ToList_Count_member(async);

        AssertSql(
            """
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Where_collection_navigation_ToArray_Length_member(bool async)
    {
        await base.Where_collection_navigation_ToArray_Length_member(async);

        AssertSql(
            """
SELECT [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId], [o2].[Id], [o2].[Detail]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o0] ON [o].[Id] = [o0].[ClientId]
LEFT JOIN [OrderDetail] AS [o2] ON [o0].[ClientId] = [o2].[OrderClientId] AND [o0].[Id] = [o2].[OrderId]
WHERE (
    SELECT COUNT(*)
    FROM [OrderDetail] AS [o1]
    WHERE [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]) = 0
ORDER BY [o].[Id], [o0].[ClientId], [o0].[Id], [o2].[OrderClientId], [o2].[OrderId]
""");
    }

    public override async Task Can_query_on_indexer_properties(bool async)
    {
        await base.Can_query_on_indexer_properties(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Can_query_on_owned_indexer_properties(bool async)
    {
        await base.Can_query_on_owned_indexer_properties(async);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[PersonAddress_ZipCode] = 38654
""");
    }

    public override async Task Can_query_on_indexer_property_when_property_name_from_closure(bool async)
    {
        await base.Can_query_on_indexer_property_when_property_name_from_closure(async);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE [o].[Name] = N'Mona Cy'
""");
    }

    public override async Task Can_project_indexer_properties(bool async)
    {
        await base.Can_project_indexer_properties(async);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task Can_project_owned_indexer_properties(bool async)
    {
        await base.Can_project_owned_indexer_properties(async);

        AssertSql(
            """
SELECT [o].[PersonAddress_AddressLine]
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task Can_project_indexer_properties_converted(bool async)
    {
        await base.Can_project_indexer_properties_converted(async);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task Can_project_owned_indexer_properties_converted(bool async)
    {
        await base.Can_project_owned_indexer_properties_converted(async);

        AssertSql(
            """
SELECT [o].[PersonAddress_AddressLine]
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task Can_OrderBy_indexer_properties(bool async)
    {
        await base.Can_OrderBy_indexer_properties(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
ORDER BY [o].[Name], [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Can_OrderBy_indexer_properties_converted(bool async)
    {
        await base.Can_OrderBy_indexer_properties_converted(async);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[Name], [o].[Id]
""");
    }

    public override async Task Can_OrderBy_owned_indexer_properties(bool async)
    {
        await base.Can_OrderBy_owned_indexer_properties(async);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[PersonAddress_ZipCode], [o].[Id]
""");
    }

    public override async Task Can_OrderBy_owened_indexer_properties_converted(bool async)
    {
        await base.Can_OrderBy_owened_indexer_properties_converted(async);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[PersonAddress_ZipCode], [o].[Id]
""");
    }

    public override async Task Can_group_by_indexer_property(bool isAsync)
    {
        await base.Can_group_by_indexer_property(isAsync);

        AssertSql(
            """
SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[Name]
""");
    }

    public override async Task Can_group_by_converted_indexer_property(bool isAsync)
    {
        await base.Can_group_by_converted_indexer_property(isAsync);

        AssertSql(
            """
SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[Name]
""");
    }

    public override async Task Can_group_by_owned_indexer_property(bool isAsync)
    {
        await base.Can_group_by_owned_indexer_property(isAsync);

        AssertSql(
            """
SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[PersonAddress_ZipCode]
""");
    }

    public override async Task Can_group_by_converted_owned_indexer_property(bool isAsync)
    {
        await base.Can_group_by_converted_owned_indexer_property(isAsync);

        AssertSql(
            """
SELECT COUNT(*)
FROM [OwnedPerson] AS [o]
GROUP BY [o].[PersonAddress_ZipCode]
""");
    }

    public override async Task Can_join_on_indexer_property_on_query(bool isAsync)
    {
        await base.Can_join_on_indexer_property_on_query(isAsync);

        AssertSql(
            """
SELECT [o].[Id], [o0].[PersonAddress_Country_Name] AS [Name]
FROM [OwnedPerson] AS [o]
INNER JOIN [OwnedPerson] AS [o0] ON [o].[PersonAddress_ZipCode] = [o0].[PersonAddress_ZipCode]
""");
    }

    public override async Task Projecting_indexer_property_ignores_include(bool isAsync)
    {
        await base.Projecting_indexer_property_ignores_include(isAsync);

        AssertSql(
            """
SELECT [o].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task Projecting_indexer_property_ignores_include_converted(bool isAsync)
    {
        await base.Projecting_indexer_property_ignores_include_converted(isAsync);

        AssertSql(
            """
SELECT [o].[PersonAddress_ZipCode] AS [Nation]
FROM [OwnedPerson] AS [o]
""");
    }

    public override async Task Indexer_property_is_pushdown_into_subquery(bool isAsync)
    {
        await base.Indexer_property_is_pushdown_into_subquery(isAsync);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE (
    SELECT TOP(1) [o0].[Name]
    FROM [OwnedPerson] AS [o0]
    WHERE [o0].[Id] = [o].[Id]) = N'Mona Cy'
""");
    }

    public override async Task Can_query_indexer_property_on_owned_collection(bool isAsync)
    {
        await base.Can_query_indexer_property_on_owned_collection(isAsync);

        AssertSql(
            """
SELECT [o].[Name]
FROM [OwnedPerson] AS [o]
WHERE (
    SELECT COUNT(*)
    FROM [Order] AS [o0]
    WHERE [o].[Id] = [o0].[ClientId] AND DATEPART(year, [o0].[OrderDate]) = 2018) = 1
""");
    }

    public override async Task Query_for_base_type_loads_all_owned_navs_split(bool async)
    {
        await base.Query_for_base_type_loads_all_owned_navs_split(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
ORDER BY [o].[Id]
""",
            //
            """
SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""",
            //
            """
SELECT [o3].[OrderClientId], [o3].[OrderId], [o3].[Id], [o3].[Detail], [o].[Id], [o1].[ClientId], [o1].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
INNER JOIN [OrderDetail] AS [o3] ON [o1].[ClientId] = [o3].[OrderClientId] AND [o1].[Id] = [o3].[OrderId]
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""");
    }

    public override async Task Query_for_branch_type_loads_all_owned_navs_split(bool async)
    {
        await base.Query_for_branch_type_loads_all_owned_navs_split(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id]
""",
            //
            """
SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""",
            //
            """
SELECT [o3].[OrderClientId], [o3].[OrderId], [o3].[Id], [o3].[Detail], [o].[Id], [o1].[ClientId], [o1].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
INNER JOIN [OrderDetail] AS [o3] ON [o1].[ClientId] = [o3].[OrderClientId] AND [o1].[Id] = [o3].[OrderId]
WHERE [o].[Discriminator] IN (N'Branch', N'LeafA')
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""");
    }

    public override async Task Query_when_subquery_split(bool async)
    {
        await base.Query_when_subquery_split(async);

        AssertSql(
            """
@__p_0='5'

SELECT TOP(@__p_0) [o0].[Id], [o0].[Discriminator], [o0].[Name], [o0].[PersonAddress_AddressLine], [o0].[PersonAddress_PlaceType], [o0].[PersonAddress_ZipCode], [o0].[PersonAddress_Country_Name], [o0].[PersonAddress_Country_PlanetId], [o0].[BranchAddress_BranchName], [o0].[BranchAddress_PlaceType], [o0].[BranchAddress_Country_Name], [o0].[BranchAddress_Country_PlanetId], [o0].[LeafBAddress_LeafBType], [o0].[LeafBAddress_PlaceType], [o0].[LeafBAddress_Country_Name], [o0].[LeafBAddress_Country_PlanetId], [o0].[LeafAAddress_LeafType], [o0].[LeafAAddress_PlaceType], [o0].[LeafAAddress_Country_Name], [o0].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [OwnedPerson] AS [o]
) AS [o0]
ORDER BY [o0].[Id]
""",
            //
            """
@__p_0='5'

SELECT [o2].[ClientId], [o2].[Id], [o2].[OrderDate], [o5].[Id]
FROM (
    SELECT TOP(@__p_0) [o0].[Id]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o]
    ) AS [o0]
    ORDER BY [o0].[Id]
) AS [o5]
INNER JOIN [Order] AS [o2] ON [o5].[Id] = [o2].[ClientId]
ORDER BY [o5].[Id], [o2].[ClientId], [o2].[Id]
""",
            //
            """
@__p_0='5'

SELECT [o4].[OrderClientId], [o4].[OrderId], [o4].[Id], [o4].[Detail], [o5].[Id], [o2].[ClientId], [o2].[Id]
FROM (
    SELECT TOP(@__p_0) [o0].[Id]
    FROM (
        SELECT DISTINCT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o]
    ) AS [o0]
    ORDER BY [o0].[Id]
) AS [o5]
INNER JOIN [Order] AS [o2] ON [o5].[Id] = [o2].[ClientId]
INNER JOIN [OrderDetail] AS [o4] ON [o2].[ClientId] = [o4].[OrderClientId] AND [o2].[Id] = [o4].[OrderId]
ORDER BY [o5].[Id], [o2].[ClientId], [o2].[Id]
""");
    }

    public override async Task Project_multiple_owned_navigations_split(bool async)
    {
        await base.Project_multiple_owned_navigations_split(async);

        AssertSql(
            """
SELECT [o].[Id], [p].[Id], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [p].[Name], [p].[StarId]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id], [p].[Id]
""",
            //
            """
SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o].[Id], [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
ORDER BY [o].[Id], [p].[Id], [o1].[ClientId], [o1].[Id]
""",
            //
            """
SELECT [o3].[OrderClientId], [o3].[OrderId], [o3].[Id], [o3].[Detail], [o].[Id], [p].[Id], [o1].[ClientId], [o1].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
INNER JOIN [OrderDetail] AS [o3] ON [o1].[ClientId] = [o3].[OrderClientId] AND [o1].[Id] = [o3].[OrderId]
ORDER BY [o].[Id], [p].[Id], [o1].[ClientId], [o1].[Id]
""");
    }

    public override async Task Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(bool async)
    {
        await base.Navigation_rewrite_on_owned_reference_followed_by_regular_entity_and_collection_split(async);

        AssertSql(
            """
SELECT [o].[Id], [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
ORDER BY [o].[Id], [p].[Id]
""",
            //
            """
SELECT [m].[Id], [m].[Diameter], [m].[PlanetId], [o].[Id], [p].[Id]
FROM [OwnedPerson] AS [o]
LEFT JOIN [Planet] AS [p] ON [o].[PersonAddress_Country_PlanetId] = [p].[Id]
INNER JOIN [Moon] AS [m] ON [p].[Id] = [m].[PlanetId]
ORDER BY [o].[Id], [p].[Id]
""");
    }

    public override async Task Query_with_OfType_eagerly_loads_correct_owned_navigations_split(bool async)
    {
        await base.Query_with_OfType_eagerly_loads_correct_owned_navigations_split(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id]
""",
            //
            """
SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""",
            //
            """
SELECT [o3].[OrderClientId], [o3].[OrderId], [o3].[Id], [o3].[Detail], [o].[Id], [o1].[ClientId], [o1].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
INNER JOIN [OrderDetail] AS [o3] ON [o1].[ClientId] = [o3].[OrderClientId] AND [o1].[Id] = [o3].[OrderId]
WHERE [o].[Discriminator] = N'LeafA'
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""");
    }

    public override async Task Unmapped_property_projection_loads_owned_navigations_split(bool async)
    {
        await base.Unmapped_property_projection_loads_owned_navigations_split(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Id] = 1
ORDER BY [o].[Id]
""",
            //
            """
SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""",
            //
            """
SELECT [o3].[OrderClientId], [o3].[OrderId], [o3].[Id], [o3].[Detail], [o].[Id], [o1].[ClientId], [o1].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
INNER JOIN [OrderDetail] AS [o3] ON [o1].[ClientId] = [o3].[OrderClientId] AND [o1].[Id] = [o3].[OrderId]
WHERE [o].[Id] = 1
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""");
    }

    public override async Task Can_query_on_indexer_properties_split(bool async)
    {
        await base.Can_query_on_indexer_properties_split(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id]
""",
            //
            """
SELECT [o1].[ClientId], [o1].[Id], [o1].[OrderDate], [o].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""",
            //
            """
SELECT [o3].[OrderClientId], [o3].[OrderId], [o3].[Id], [o3].[Detail], [o].[Id], [o1].[ClientId], [o1].[Id]
FROM [OwnedPerson] AS [o]
INNER JOIN [Order] AS [o1] ON [o].[Id] = [o1].[ClientId]
INNER JOIN [OrderDetail] AS [o3] ON [o1].[ClientId] = [o3].[OrderClientId] AND [o1].[Id] = [o3].[OrderId]
WHERE [o].[Name] = N'Mona Cy'
ORDER BY [o].[Id], [o1].[ClientId], [o1].[Id]
""");
    }

    public override async Task GroupBy_with_multiple_aggregates_on_owned_navigation_properties(bool async)
    {
        await base.GroupBy_with_multiple_aggregates_on_owned_navigation_properties(async);

        AssertSql(
            """
SELECT (
    SELECT AVG(CAST([s].[Id] AS float))
    FROM (
        SELECT 1 AS [Key], [o2].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o2]
    ) AS [o1]
    LEFT JOIN [Planet] AS [p] ON [o1].[PersonAddress_Country_PlanetId] = [p].[Id]
    LEFT JOIN [Star] AS [s] ON [p].[StarId] = [s].[Id]
    WHERE [o0].[Key] = [o1].[Key]) AS [p1], (
    SELECT COALESCE(SUM([s0].[Id]), 0)
    FROM (
        SELECT 1 AS [Key], [o4].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o4]
    ) AS [o3]
    LEFT JOIN [Planet] AS [p0] ON [o3].[PersonAddress_Country_PlanetId] = [p0].[Id]
    LEFT JOIN [Star] AS [s0] ON [p0].[StarId] = [s0].[Id]
    WHERE [o0].[Key] = [o3].[Key]) AS [p2], (
    SELECT MAX(CAST(LEN([s1].[Name]) AS int))
    FROM (
        SELECT 1 AS [Key], [o6].[PersonAddress_Country_PlanetId]
        FROM [OwnedPerson] AS [o6]
    ) AS [o5]
    LEFT JOIN [Planet] AS [p1] ON [o5].[PersonAddress_Country_PlanetId] = [p1].[Id]
    LEFT JOIN [Star] AS [s1] ON [p1].[StarId] = [s1].[Id]
    WHERE [o0].[Key] = [o5].[Key]) AS [p3]
FROM (
    SELECT 1 AS [Key]
    FROM [OwnedPerson] AS [o]
) AS [o0]
GROUP BY [o0].[Key]
""");
    }

    public override async Task Ordering_by_identifying_projection(bool async)
    {
        await base.Ordering_by_identifying_projection(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
ORDER BY [o].[PersonAddress_PlaceType], [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool async)
    {
        await base.Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(async);

        AssertSql(
            """
SELECT [m].[Id], [m].[Discriminator], [m].[Name], [o].[Id], [o0].[Id], [o1].[Id], [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o0].[BranchAddress_BranchName], [o0].[BranchAddress_PlaceType], [o0].[BranchAddress_Country_Name], [o0].[BranchAddress_Country_PlanetId], [o1].[LeafBAddress_LeafBType], [o1].[LeafBAddress_PlaceType], [o1].[LeafBAddress_Country_Name], [o1].[LeafBAddress_Country_PlanetId], [o2].[LeafAAddress_LeafType], [o2].[LeafAAddress_PlaceType], [o2].[LeafAAddress_Country_Name], [o2].[LeafAAddress_Country_PlanetId]
FROM (
    SELECT * FROM "OwnedPerson"
) AS [m]
LEFT JOIN [OwnedPerson] AS [o] ON [m].[Id] = [o].[Id]
LEFT JOIN [OwnedPerson] AS [o0] ON [m].[Id] = [o0].[Id]
LEFT JOIN [OwnedPerson] AS [o1] ON [m].[Id] = [o1].[Id]
LEFT JOIN [OwnedPerson] AS [o2] ON [m].[Id] = [o2].[Id]
LEFT JOIN (
    SELECT [o3].[ClientId], [o3].[Id], [o3].[OrderDate], [o4].[OrderClientId], [o4].[OrderId], [o4].[Id] AS [Id0], [o4].[Detail]
    FROM [Order] AS [o3]
    LEFT JOIN [OrderDetail] AS [o4] ON [o3].[ClientId] = [o4].[OrderClientId] AND [o3].[Id] = [o4].[OrderId]
) AS [s] ON [m].[Id] = [s].[ClientId]
ORDER BY [m].[Id], [o].[Id], [o0].[Id], [o1].[Id], [o2].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(
        bool async)
    {
        await base.Projecting_collection_correlated_with_keyless_entity_after_navigation_works_using_parent_identifiers(async);

        AssertSql(
            """
SELECT [b].[Throned_Value], [f].[Id], [b].[Id], [p].[Id], [p].[Name], [p].[StarId]
FROM [Fink] AS [f]
LEFT JOIN [Barton] AS [b] ON [f].[BartonId] = [b].[Id]
LEFT JOIN [Planet] AS [p] ON [b].[Throned_Value] <> [p].[Id] OR [b].[Throned_Value] IS NULL
ORDER BY [f].[Id], [b].[Id]
""");
    }

    public override async Task Filter_on_indexer_using_closure(bool async)
    {
        await base.Filter_on_indexer_using_closure(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[PersonAddress_ZipCode] = 38654
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Filter_on_indexer_using_function_argument(bool async)
    {
        await base.Filter_on_indexer_using_function_argument(async);

        AssertSql(
            """
SELECT [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
FROM [OwnedPerson] AS [o]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
WHERE [o].[PersonAddress_ZipCode] = 38654
ORDER BY [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId]
""");
    }

    public override async Task Simple_query_entity_with_owned_collection(bool async)
    {
        await base.Simple_query_entity_with_owned_collection(async);

        AssertSql(
            """
SELECT [s].[Id], [s].[Name], [e].[Id], [e].[Name], [e].[StarId]
FROM [Star] AS [s]
LEFT JOIN [Element] AS [e] ON [s].[Id] = [e].[StarId]
ORDER BY [s].[Id]
""");
    }

    public override async Task Left_join_on_entity_with_owned_navigations(bool async)
    {
        await base.Left_join_on_entity_with_owned_navigations(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Name], [p].[StarId], [o].[Id], [o].[Discriminator], [o].[Name], [s].[ClientId], [s].[Id], [s].[OrderDate], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s].[Detail], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId], [s0].[ClientId], [s0].[Id], [s0].[OrderDate], [s0].[OrderClientId], [s0].[OrderId], [s0].[Id0], [s0].[Detail]
FROM [Planet] AS [p]
LEFT JOIN [OwnedPerson] AS [o] ON [p].[Id] = [o].[Id]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s] ON [o].[Id] = [s].[ClientId]
LEFT JOIN (
    SELECT [o2].[ClientId], [o2].[Id], [o2].[OrderDate], [o3].[OrderClientId], [o3].[OrderId], [o3].[Id] AS [Id0], [o3].[Detail]
    FROM [Order] AS [o2]
    LEFT JOIN [OrderDetail] AS [o3] ON [o2].[ClientId] = [o3].[OrderClientId] AND [o2].[Id] = [o3].[OrderId]
) AS [s0] ON [o].[Id] = [s0].[ClientId]
ORDER BY [p].[Id], [o].[Id], [s].[ClientId], [s].[Id], [s].[OrderClientId], [s].[OrderId], [s].[Id0], [s0].[ClientId], [s0].[Id], [s0].[OrderClientId], [s0].[OrderId]
""");
    }

    public override async Task Left_join_on_entity_with_owned_navigations_complex(bool async)
    {
        await base.Left_join_on_entity_with_owned_navigations_complex(async);

        AssertSql(
            """
SELECT [p].[Id], [p].[Name], [p].[StarId], [s].[Id], [s].[Name], [s].[StarId], [s].[Id0], [s].[Discriminator], [s].[Name0], [s0].[ClientId], [s0].[Id], [s0].[OrderDate], [s0].[OrderClientId], [s0].[OrderId], [s0].[Id0], [s0].[Detail], [s].[PersonAddress_AddressLine], [s].[PersonAddress_PlaceType], [s].[PersonAddress_ZipCode], [s].[PersonAddress_Country_Name], [s].[PersonAddress_Country_PlanetId], [s].[BranchAddress_BranchName], [s].[BranchAddress_PlaceType], [s].[BranchAddress_Country_Name], [s].[BranchAddress_Country_PlanetId], [s].[LeafBAddress_LeafBType], [s].[LeafBAddress_PlaceType], [s].[LeafBAddress_Country_Name], [s].[LeafBAddress_Country_PlanetId], [s].[LeafAAddress_LeafType], [s].[LeafAAddress_PlaceType], [s].[LeafAAddress_Country_Name], [s].[LeafAAddress_Country_PlanetId]
FROM [Planet] AS [p]
LEFT JOIN (
    SELECT DISTINCT [p0].[Id], [p0].[Name], [p0].[StarId], [o].[Id] AS [Id0], [o].[Discriminator], [o].[Name] AS [Name0], [o].[PersonAddress_AddressLine], [o].[PersonAddress_PlaceType], [o].[PersonAddress_ZipCode], [o].[PersonAddress_Country_Name], [o].[PersonAddress_Country_PlanetId], [o].[BranchAddress_BranchName], [o].[BranchAddress_PlaceType], [o].[BranchAddress_Country_Name], [o].[BranchAddress_Country_PlanetId], [o].[LeafBAddress_LeafBType], [o].[LeafBAddress_PlaceType], [o].[LeafBAddress_Country_Name], [o].[LeafBAddress_Country_PlanetId], [o].[LeafAAddress_LeafType], [o].[LeafAAddress_PlaceType], [o].[LeafAAddress_Country_Name], [o].[LeafAAddress_Country_PlanetId]
    FROM [Planet] AS [p0]
    LEFT JOIN [OwnedPerson] AS [o] ON [p0].[Id] = [o].[Id]
) AS [s] ON [p].[Id] = [s].[Id0]
LEFT JOIN (
    SELECT [o0].[ClientId], [o0].[Id], [o0].[OrderDate], [o1].[OrderClientId], [o1].[OrderId], [o1].[Id] AS [Id0], [o1].[Detail]
    FROM [Order] AS [o0]
    LEFT JOIN [OrderDetail] AS [o1] ON [o0].[ClientId] = [o1].[OrderClientId] AND [o0].[Id] = [o1].[OrderId]
) AS [s0] ON [s].[Id0] = [s0].[ClientId]
ORDER BY [p].[Id], [s].[Id], [s].[Id0], [s0].[ClientId], [s0].[Id], [s0].[OrderClientId], [s0].[OrderId]
""");
    }

    public override async Task GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(bool async)
    {
        await base.GroupBy_aggregate_on_owned_navigation_in_aggregate_selector(async);

        AssertSql(
            """
SELECT [o].[Id] AS [Key], (
    SELECT COALESCE(SUM([o0].[PersonAddress_Country_PlanetId]), 0)
    FROM [OwnedPerson] AS [o0]
    WHERE [o].[Id] = [o0].[Id]) AS [Sum]
FROM [OwnedPerson] AS [o]
GROUP BY [o].[Id]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class OwnedQuerySqlServerFixture : RelationalOwnedQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }
}
