// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore;

public class TableSplittingSqlServerTest : TableSplittingTestBase
{
    public TableSplittingSqlServerTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Can_use_with_redundant_relationships()
    {
        await base.Can_use_with_redundant_relationships();

        // TODO: [Name] shouldn't be selected multiple times and no joins are needed
        AssertSql(
            @"SELECT [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [t].[Name], [t].[Operator_Discriminator], [t].[Operator_Name], [t].[LicenseType], [t0].[Name], [t0].[Active], [t0].[Type], [t2].[Name], [t2].[Computed], [t2].[Description], [t2].[Engine_Discriminator], [t4].[Name], [t4].[Capacity], [t4].[FuelTank_Discriminator], [t4].[FuelType], [t4].[GrainGeometry]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
    FROM [Vehicles] AS [v0]
    INNER JOIN [Vehicles] AS [v1] ON [v0].[Name] = [v1].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
LEFT JOIN (
    SELECT [v2].[Name], [v2].[Active], [v2].[Type]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name]
        FROM [Vehicles] AS [v3]
        INNER JOIN [Vehicles] AS [v4] ON [v3].[Name] = [v4].[Name]
    ) AS [t1] ON [v2].[Name] = [t1].[Name]
    WHERE [v2].[Active] IS NOT NULL
) AS [t0] ON [t].[Name] = CASE
    WHEN [t0].[Active] IS NOT NULL THEN [t0].[Name]
END
LEFT JOIN (
    SELECT [v5].[Name], [v5].[Computed], [v5].[Description], [v5].[Engine_Discriminator]
    FROM [Vehicles] AS [v5]
    INNER JOIN (
        SELECT [v6].[Name]
        FROM [Vehicles] AS [v6]
        WHERE [v6].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t3] ON [v5].[Name] = [t3].[Name]
    WHERE [v5].[Computed] IS NOT NULL AND [v5].[Engine_Discriminator] IS NOT NULL
) AS [t2] ON [v].[Name] = CASE
    WHEN [t2].[Computed] IS NOT NULL AND [t2].[Engine_Discriminator] IS NOT NULL THEN [t2].[Name]
END
LEFT JOIN (
    SELECT [v7].[Name], [v7].[Capacity], [v7].[FuelTank_Discriminator], [v7].[FuelType], [v7].[GrainGeometry]
    FROM [Vehicles] AS [v7]
    INNER JOIN (
        SELECT [v8].[Name], [v8].[Discriminator], [v8].[SeatingCapacity], [v8].[AttachedVehicleName]
        FROM [Vehicles] AS [v8]
        WHERE [v8].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t5] ON [v7].[Name] = [t5].[Name]
    WHERE [v7].[Capacity] IS NOT NULL AND [v7].[FuelTank_Discriminator] IS NOT NULL
    UNION
    SELECT [v9].[Name], [v9].[Capacity], [v9].[FuelTank_Discriminator], [v9].[FuelType], [v9].[GrainGeometry]
    FROM [Vehicles] AS [v9]
    INNER JOIN (
        SELECT [v10].[Name], [v10].[Computed], [v10].[Description], [v10].[Engine_Discriminator], [t7].[Name] AS [Name0]
        FROM [Vehicles] AS [v10]
        INNER JOIN (
            SELECT [v11].[Name], [v11].[Discriminator], [v11].[SeatingCapacity], [v11].[AttachedVehicleName]
            FROM [Vehicles] AS [v11]
            WHERE [v11].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
        ) AS [t7] ON [v10].[Name] = [t7].[Name]
        WHERE [v10].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
    ) AS [t6] ON [v9].[Name] = [t6].[Name]
    WHERE [v9].[Capacity] IS NOT NULL AND [v9].[FuelTank_Discriminator] IS NOT NULL
) AS [t4] ON CASE
    WHEN [t2].[Computed] IS NOT NULL AND [t2].[Engine_Discriminator] IS NOT NULL THEN [t2].[Name]
END = CASE
    WHEN [t4].[Capacity] IS NOT NULL AND [t4].[FuelTank_Discriminator] IS NOT NULL THEN [t4].[Name]
END
ORDER BY [v].[Name]");
    }

    public override async Task Can_query_shared()
    {
        await base.Can_query_shared();

        AssertSql(
            @"SELECT [v].[Name], [v].[Operator_Discriminator], [v].[Operator_Name], [v].[LicenseType]
FROM [Vehicles] AS [v]
INNER JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]");
    }

    public override async Task Can_query_shared_nonhierarchy()
    {
        await base.Can_query_shared_nonhierarchy();

        AssertSql(
            @"SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
INNER JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]");
    }

    public override async Task Can_query_shared_nonhierarchy_with_nonshared_dependent()
    {
        await base.Can_query_shared_nonhierarchy_with_nonshared_dependent();

        AssertSql(
            @"SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
INNER JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]");
    }

    public override async Task Can_query_shared_derived_hierarchy()
    {
        await base.Can_query_shared_derived_hierarchy();

        AssertSql(
            @"SELECT [v].[Name], [v].[Capacity], [v].[FuelTank_Discriminator], [v].[FuelType], [v].[GrainGeometry]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity], [v0].[AttachedVehicleName]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Capacity] IS NOT NULL AND [v].[FuelTank_Discriminator] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelTank_Discriminator], [v1].[FuelType], [v1].[GrainGeometry]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator], [t2].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity], [v3].[AttachedVehicleName]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t2] ON [v2].[Name] = [t2].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]
WHERE [v1].[Capacity] IS NOT NULL AND [v1].[FuelTank_Discriminator] IS NOT NULL");
    }

    public override async Task Can_query_shared_derived_nonhierarchy()
    {
        await base.Can_query_shared_derived_nonhierarchy();

        AssertSql(
            @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity], [v0].[AttachedVehicleName]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Capacity] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelType]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator], [t2].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity], [v3].[AttachedVehicleName]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t2] ON [v2].[Name] = [t2].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]
WHERE [v1].[Capacity] IS NOT NULL");
    }

    public override async Task Can_query_shared_derived_nonhierarchy_all_required()
    {
        await base.Can_query_shared_derived_nonhierarchy_all_required();

        AssertSql(
            @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity], [v0].[AttachedVehicleName]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Capacity] IS NOT NULL AND [v].[FuelType] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelType]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator], [t2].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity], [v3].[AttachedVehicleName]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t2] ON [v2].[Name] = [t2].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]
WHERE [v1].[Capacity] IS NOT NULL AND [v1].[FuelType] IS NOT NULL");
    }

    public override async Task Can_change_dependent_instance_non_derived()
    {
        await base.Can_change_dependent_instance_non_derived();

        AssertSql(
            @"@p3='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='LicensedOperator' (Nullable = false) (Size = 4000)
@p1='Repair' (Size = 4000)
@p2='repairman' (Size = 4000)

SET NOCOUNT ON;
UPDATE [Vehicles] SET [Operator_Discriminator] = @p0, [LicenseType] = @p1, [Operator_Name] = @p2
WHERE [Name] = @p3;
SELECT @@ROWCOUNT;",
            //
            @"SELECT TOP(2) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [t].[Name], [t].[Operator_Discriminator], [t].[Operator_Name], [t].[LicenseType]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
    FROM [Vehicles] AS [v0]
    INNER JOIN [Vehicles] AS [v1] ON [v0].[Name] = [v1].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'");
    }

    public override async Task Can_change_principal_instance_non_derived()
    {
        await base.Can_change_principal_instance_non_derived();

        AssertSql(
            @"@p1='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='2'

SET NOCOUNT ON;
UPDATE [Vehicles] SET [SeatingCapacity] = @p0
WHERE [Name] = @p1;
SELECT @@ROWCOUNT;",
            //
            @"SELECT TOP(2) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [t].[Name], [t].[Operator_Discriminator], [t].[Operator_Name], [t].[LicenseType]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
    FROM [Vehicles] AS [v0]
    INNER JOIN [Vehicles] AS [v1] ON [v0].[Name] = [v1].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'");
    }

    public override async Task Optional_dependent_materialized_when_no_properties()
    {
        await base.Optional_dependent_materialized_when_no_properties();

        AssertSql(
            @"SELECT TOP(1) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [t].[Name], [t].[Operator_Discriminator], [t].[Operator_Name], [t].[LicenseType], [t0].[Name], [t0].[Active], [t0].[Type]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
    FROM [Vehicles] AS [v0]
    INNER JOIN [Vehicles] AS [v1] ON [v0].[Name] = [v1].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
LEFT JOIN (
    SELECT [v2].[Name], [v2].[Active], [v2].[Type]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name]
        FROM [Vehicles] AS [v3]
        INNER JOIN [Vehicles] AS [v4] ON [v3].[Name] = [v4].[Name]
    ) AS [t1] ON [v2].[Name] = [t1].[Name]
    WHERE [v2].[Active] IS NOT NULL
) AS [t0] ON [t].[Name] = CASE
    WHEN [t0].[Active] IS NOT NULL THEN [t0].[Name]
END
WHERE [v].[Name] = N'AIM-9M Sidewinder'
ORDER BY [v].[Name]");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Engine>().ToTable("Vehicles")
            .Property(e => e.Computed).HasComputedColumnSql("1", stored: true);
    }
}
