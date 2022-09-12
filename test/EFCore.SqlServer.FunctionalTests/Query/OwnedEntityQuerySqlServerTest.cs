// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public class OwnedEntityQuerySqlServerTest : OwnedEntityQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Multiple_single_result_in_projection_containing_owned_types(bool async)
    {
        await base.Multiple_single_result_in_projection_containing_owned_types(async);

        AssertSql(
            @"SELECT [e].[Id], [t0].[Id], [t0].[Entity20277Id], [t0].[Owned_IsDeleted], [t0].[Owned_Value], [t0].[Type], [t0].[c], [t1].[Id], [t1].[Entity20277Id], [t1].[Owned_IsDeleted], [t1].[Owned_Value], [t1].[Type], [t1].[c]
FROM [Entities] AS [e]
LEFT JOIN (
    SELECT [t].[Id], [t].[Entity20277Id], [t].[Owned_IsDeleted], [t].[Owned_Value], [t].[Type], [t].[c]
    FROM (
        SELECT [c].[Id], [c].[Entity20277Id], [c].[Owned_IsDeleted], [c].[Owned_Value], [c].[Type], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [c].[Entity20277Id] ORDER BY [c].[Entity20277Id], [c].[Id]) AS [row]
        FROM [Child20277] AS [c]
        WHERE [c].[Type] = 1
    ) AS [t]
    WHERE [t].[row] <= 1
) AS [t0] ON [e].[Id] = [t0].[Entity20277Id]
LEFT JOIN (
    SELECT [t2].[Id], [t2].[Entity20277Id], [t2].[Owned_IsDeleted], [t2].[Owned_Value], [t2].[Type], [t2].[c]
    FROM (
        SELECT [c0].[Id], [c0].[Entity20277Id], [c0].[Owned_IsDeleted], [c0].[Owned_Value], [c0].[Type], 1 AS [c], ROW_NUMBER() OVER(PARTITION BY [c0].[Entity20277Id] ORDER BY [c0].[Entity20277Id], [c0].[Id]) AS [row]
        FROM [Child20277] AS [c0]
        WHERE [c0].[Type] = 2
    ) AS [t2]
    WHERE [t2].[row] <= 1
) AS [t1] ON [e].[Id] = [t1].[Entity20277Id]");
    }

    public override async Task Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(bool async)
    {
        await base.Multiple_owned_reference_mapped_to_own_table_containing_owned_collection_in_split_query(async);

        AssertSql(
            @"SELECT TOP(2) [r].[Id], [m].[Id], [m].[Enabled], [m].[RootId], [m0].[Id], [m0].[RootId]
FROM [Root24777] AS [r]
LEFT JOIN [MiddleB24777] AS [m] ON [r].[Id] = [m].[RootId]
LEFT JOIN [ModdleA24777] AS [m0] ON [r].[Id] = [m0].[RootId]
WHERE [r].[Id] = 3
ORDER BY [r].[Id], [m].[Id], [m0].[Id]",
            //
            @"SELECT [l].[ModdleAId], [l].[UnitThreshold], [t].[Id], [t].[Id0], [t].[Id1]
FROM (
    SELECT TOP(1) [r].[Id], [m].[Id] AS [Id0], [m0].[Id] AS [Id1]
    FROM [Root24777] AS [r]
    LEFT JOIN [MiddleB24777] AS [m] ON [r].[Id] = [m].[RootId]
    LEFT JOIN [ModdleA24777] AS [m0] ON [r].[Id] = [m0].[RootId]
    WHERE [r].[Id] = 3
) AS [t]
INNER JOIN [Leaf24777] AS [l] ON [t].[Id1] = [l].[ModdleAId]
ORDER BY [t].[Id], [t].[Id0], [t].[Id1]");
    }

    public override async Task Projecting_owned_collection_and_aggregate(bool async)
    {
        await base.Projecting_owned_collection_and_aggregate(async);

        AssertSql(
            @"SELECT [b].[Id], (
    SELECT COALESCE(SUM([p].[CommentsCount]), 0)
    FROM [Post24133] AS [p]
    WHERE [b].[Id] = [p].[BlogId]), [p0].[Title], [p0].[CommentsCount], [p0].[BlogId], [p0].[Id]
FROM [Blog24133] AS [b]
LEFT JOIN [Post24133] AS [p0] ON [b].[Id] = [p0].[BlogId]
ORDER BY [b].[Id], [p0].[BlogId]");
    }

    public override async Task Projecting_correlated_collection_property_for_owned_entity(bool async)
    {
        await base.Projecting_correlated_collection_property_for_owned_entity(async);

        AssertSql(
            @"SELECT [w].[WarehouseCode], [w].[Id], [w0].[CountryCode], [w0].[WarehouseCode], [w0].[Id]
FROM [Warehouses] AS [w]
LEFT JOIN [WarehouseDestinationCountry] AS [w0] ON [w].[WarehouseCode] = [w0].[WarehouseCode]
ORDER BY [w].[Id], [w0].[WarehouseCode]");
    }

    public override async Task Owned_collection_basic_split_query(bool async)
    {
        await base.Owned_collection_basic_split_query(async);

        AssertSql(
            @"@__id_0='6c1ae3e5-30b9-4c77-8d98-f02075974a0a'

SELECT TOP(1) [l].[Id]
FROM [Location25680] AS [l]
WHERE [l].[Id] = @__id_0
ORDER BY [l].[Id]");
    }

    public override async Task Owned_reference_mapped_to_different_table_updated_correctly_after_subquery_pushdown(bool async)
    {
        await base.Owned_reference_mapped_to_different_table_updated_correctly_after_subquery_pushdown(async);

        AssertSql(
            @"@__p_0='10'

SELECT TOP(@__p_0) [c].[Id], [c].[Name], [c0].[CompanyId], [c0].[AdditionalCustomerData], [c0].[Id], [s].[CompanyId], [s].[AdditionalSupplierData], [s].[Id]
FROM [Companies] AS [c]
LEFT JOIN [CustomerData] AS [c0] ON [c].[Id] = [c0].[CompanyId]
LEFT JOIN [SupplierData] AS [s] ON [c].[Id] = [s].[CompanyId]
WHERE [c0].[CompanyId] IS NOT NULL
ORDER BY [c].[Id]");
    }

    public override async Task Owned_reference_mapped_to_different_table_nested_updated_correctly_after_subquery_pushdown(bool async)
    {
        await base.Owned_reference_mapped_to_different_table_nested_updated_correctly_after_subquery_pushdown(async);

        AssertSql(
            @"@__p_0='10'

SELECT TOP(@__p_0) [o].[Id], [o].[Name], [i].[OwnerId], [i].[Id], [i].[Name], [i0].[IntermediateOwnedEntityOwnerId], [i0].[AdditionalCustomerData], [i0].[Id], [i1].[IntermediateOwnedEntityOwnerId], [i1].[AdditionalSupplierData], [i1].[Id]
FROM [Owners] AS [o]
LEFT JOIN [IntermediateOwnedEntity] AS [i] ON [o].[Id] = [i].[OwnerId]
LEFT JOIN [IM_CustomerData] AS [i0] ON [i].[OwnerId] = [i0].[IntermediateOwnedEntityOwnerId]
LEFT JOIN [IM_SupplierData] AS [i1] ON [i].[OwnerId] = [i1].[IntermediateOwnedEntityOwnerId]
WHERE [i0].[IntermediateOwnedEntityOwnerId] IS NOT NULL
ORDER BY [o].[Id]");
    }

    public override async Task Owned_entity_with_all_null_properties_materializes_when_not_containing_another_owned_entity(bool async)
    {
        await base.Owned_entity_with_all_null_properties_materializes_when_not_containing_another_owned_entity(async);

        AssertSql(
            @"SELECT [r].[Id], [r].[Buyer], [r].[Rot_ApartmentNo], [r].[Rot_ServiceType], [r].[Rut_Value]
FROM [RotRutCases] AS [r]
ORDER BY [r].[Buyer]");
    }
}
