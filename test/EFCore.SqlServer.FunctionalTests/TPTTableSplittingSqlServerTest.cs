// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class TPTTableSplittingSqlServerTest(ITestOutputHelper testOutputHelper) : TPTTableSplittingTestBase(testOutputHelper)
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Can_use_with_redundant_relationships()
    {
        await base.Can_use_with_redundant_relationships();

        AssertSql(
            """
SELECT [v].[Name], [v].[SeatingCapacity], [c].[AttachedVehicleName], CASE
    WHEN [c].[Name] IS NOT NULL THEN N'CompositeVehicle'
    WHEN [p].[Name] IS NOT NULL THEN N'PoweredVehicle'
END AS [Discriminator], [s].[Name], [s].[Operator_Name], [s].[LicenseType], [s].[Discriminator], [v2].[Name], [v2].[Active], [v2].[Type], [s1].[Name], [s1].[Computed], [s1].[Description], [s1].[Discriminator], [s3].[VehicleName], [s3].[Capacity], [s3].[FuelType], [s3].[GrainGeometry], [s3].[Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN [CompositeVehicles] AS [c] ON [v].[Name] = [c].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
    END AS [Discriminator]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
) AS [s] ON [v].[Name] = [s].[Name]
LEFT JOIN (
    SELECT [v1].[Name], [v1].[Active], [v1].[Type]
    FROM [Vehicles] AS [v1]
    WHERE [v1].[Active] IS NOT NULL
) AS [v2] ON [s].[Name] = CASE
    WHEN [v2].[Active] IS NOT NULL THEN [v2].[Name]
END
LEFT JOIN (
    SELECT [p0].[Name], [p0].[Computed], [p0].[Description], CASE
        WHEN [s0].[VehicleName] IS NOT NULL THEN N'SolidRocket'
        WHEN [i].[VehicleName] IS NOT NULL THEN N'IntermittentCombustionEngine'
        WHEN [c0].[VehicleName] IS NOT NULL THEN N'ContinuousCombustionEngine'
    END AS [Discriminator]
    FROM [PoweredVehicles] AS [p0]
    LEFT JOIN [ContinuousCombustionEngines] AS [c0] ON [p0].[Name] = [c0].[VehicleName]
    LEFT JOIN [IntermittentCombustionEngines] AS [i] ON [p0].[Name] = [i].[VehicleName]
    LEFT JOIN [SolidRockets] AS [s0] ON [p0].[Name] = [s0].[VehicleName]
    WHERE [p0].[Computed] IS NOT NULL
) AS [s1] ON [v].[Name] = CASE
    WHEN [s1].[Computed] IS NOT NULL THEN [s1].[Name]
END
LEFT JOIN (
    SELECT [c1].[VehicleName], [c1].[Capacity], [c1].[FuelType], [s2].[GrainGeometry], CASE
        WHEN [s2].[VehicleName] IS NOT NULL THEN N'SolidFuelTank'
    END AS [Discriminator]
    FROM [CombustionEngines] AS [c1]
    LEFT JOIN [SolidFuelTanks] AS [s2] ON [c1].[VehicleName] = [s2].[VehicleName]
    WHERE [c1].[Capacity] IS NOT NULL
) AS [s3] ON [s1].[Name] = CASE
    WHEN [s3].[Capacity] IS NOT NULL THEN [s3].[VehicleName]
END
ORDER BY [v].[Name]
""");
    }

    public override async Task Can_query_shared()
    {
        await base.Can_query_shared();

        AssertSql(
            """
SELECT [v].[Name], [v].[Operator_Name], [l].[LicenseType], CASE
    WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
END AS [Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [LicensedOperators] AS [l] ON [v].[Name] = [l].[VehicleName]
""");
    }

    public override async Task Can_query_shared_nonhierarchy()
    {
        await base.Can_query_shared_nonhierarchy();

        AssertSql(
            """
SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
""");
    }

    public override async Task Can_query_shared_nonhierarchy_with_nonshared_dependent()
    {
        await base.Can_query_shared_nonhierarchy_with_nonshared_dependent();

        AssertSql(
            """
SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
""");
    }

    public override async Task Can_query_shared_derived_hierarchy()
    {
        await base.Can_query_shared_derived_hierarchy();

        AssertSql(
            """
SELECT [c].[VehicleName], [c].[Capacity], [c].[FuelType], [s].[GrainGeometry], CASE
    WHEN [s].[VehicleName] IS NOT NULL THEN N'SolidFuelTank'
END AS [Discriminator]
FROM [CombustionEngines] AS [c]
LEFT JOIN [SolidFuelTanks] AS [s] ON [c].[VehicleName] = [s].[VehicleName]
WHERE [c].[Capacity] IS NOT NULL
""");
    }

    public override async Task Can_query_shared_derived_nonhierarchy()
    {
        await base.Can_query_shared_derived_nonhierarchy();

        AssertSql(
            """
SELECT [c].[VehicleName], [c].[Capacity], [c].[FuelType]
FROM [CombustionEngines] AS [c]
WHERE [c].[Capacity] IS NOT NULL
""");
    }

    public override async Task Can_query_shared_derived_nonhierarchy_all_required()
    {
        await base.Can_query_shared_derived_nonhierarchy_all_required();

        AssertSql(
            """
SELECT [c].[VehicleName], [c].[Capacity], [c].[FuelType]
FROM [CombustionEngines] AS [c]
WHERE [c].[Capacity] IS NOT NULL AND [c].[FuelType] IS NOT NULL
""");
    }

    public override async Task Can_change_dependent_instance_non_derived()
    {
        await base.Can_change_dependent_instance_non_derived();
        AssertSql(
            """
@p0='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p1='Repair' (Size = 4000)
@p3='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p2='repairman' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [LicensedOperators] ([VehicleName], [LicenseType])
VALUES (@p0, @p1);
UPDATE [Vehicles] SET [Operator_Name] = @p2
OUTPUT 1
WHERE [Name] = @p3;
""",
            //
            """
SELECT TOP(2) [v].[Name], [v].[SeatingCapacity], [c].[AttachedVehicleName], CASE
    WHEN [c].[Name] IS NOT NULL THEN N'CompositeVehicle'
    WHEN [p].[Name] IS NOT NULL THEN N'PoweredVehicle'
END AS [Discriminator], [s].[Name], [s].[Operator_Name], [s].[LicenseType], [s].[Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN [CompositeVehicles] AS [c] ON [v].[Name] = [c].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
    END AS [Discriminator]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
) AS [s] ON [v].[Name] = [s].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'
""");
    }

    public override async Task Can_change_principal_instance_non_derived()
    {
        await base.Can_change_principal_instance_non_derived();

        AssertSql(
            """
@p1='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='2'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Vehicles] SET [SeatingCapacity] = @p0
OUTPUT 1
WHERE [Name] = @p1;
""",
            //
            """
SELECT TOP(2) [v].[Name], [v].[SeatingCapacity], [c].[AttachedVehicleName], CASE
    WHEN [c].[Name] IS NOT NULL THEN N'CompositeVehicle'
    WHEN [p].[Name] IS NOT NULL THEN N'PoweredVehicle'
END AS [Discriminator], [s].[Name], [s].[Operator_Name], [s].[LicenseType], [s].[Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN [CompositeVehicles] AS [c] ON [v].[Name] = [c].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
    END AS [Discriminator]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
) AS [s] ON [v].[Name] = [s].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'
""");
    }

    public override async Task Optional_dependent_materialized_when_no_properties()
    {
        await base.Optional_dependent_materialized_when_no_properties();

        AssertSql(
            """
SELECT TOP(1) [v].[Name], [v].[SeatingCapacity], [c].[AttachedVehicleName], CASE
    WHEN [c].[Name] IS NOT NULL THEN N'CompositeVehicle'
    WHEN [p].[Name] IS NOT NULL THEN N'PoweredVehicle'
END AS [Discriminator], [s].[Name], [s].[Operator_Name], [s].[LicenseType], [s].[Discriminator], [v2].[Name], [v2].[Active], [v2].[Type]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN [CompositeVehicles] AS [c] ON [v].[Name] = [c].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
    END AS [Discriminator]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
) AS [s] ON [v].[Name] = [s].[Name]
LEFT JOIN (
    SELECT [v1].[Name], [v1].[Active], [v1].[Type]
    FROM [Vehicles] AS [v1]
    WHERE [v1].[Active] IS NOT NULL
) AS [v2] ON [s].[Name] = CASE
    WHEN [v2].[Active] IS NOT NULL THEN [v2].[Name]
END
WHERE [v].[Name] = N'AIM-9M Sidewinder'
ORDER BY [v].[Name]
""");
    }

    public override Task Can_insert_dependent_with_just_one_parent()
        // This scenario is not valid for TPT
        => Task.CompletedTask;
}
