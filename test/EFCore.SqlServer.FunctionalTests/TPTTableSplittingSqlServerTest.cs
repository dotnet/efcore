// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class TPTTableSplittingSqlServerTest : TPTTableSplittingTestBase
    {
        public TPTTableSplittingSqlServerTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override void Can_use_with_redundant_relationships()
        {
            base.Can_use_with_redundant_relationships();

            AssertSql(
                @"SELECT [v].[Name], [v].[SeatingCapacity], [c].[AttachedVehicleName], CASE
    WHEN [c].[Name] IS NOT NULL THEN N'CompositeVehicle'
    WHEN [p].[Name] IS NOT NULL THEN N'PoweredVehicle'
END AS [Discriminator], [t0].[Name], [t0].[Operator_Name], [t0].[LicenseType], [t0].[Discriminator], [t3].[Name], [t3].[Type], [t5].[Name], [t5].[Computed], [t5].[Description], [t5].[Discriminator], [t7].[VehicleName], [t7].[Capacity], [t7].[FuelType], [t7].[GrainGeometry], [t7].[Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN [CompositeVehicles] AS [c] ON [v].[Name] = [c].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
    END AS [Discriminator]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
    INNER JOIN (
        SELECT [v1].[Name]
        FROM [Vehicles] AS [v1]
    ) AS [t] ON [v0].[Name] = [t].[Name]
) AS [t0] ON [v].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [v2].[Name], [v2].[Type]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name]
        FROM [Vehicles] AS [v3]
        INNER JOIN (
            SELECT [v4].[Name]
            FROM [Vehicles] AS [v4]
        ) AS [t1] ON [v3].[Name] = [t1].[Name]
    ) AS [t2] ON [v2].[Name] = [t2].[Name]
    WHERE [v2].[Type] IS NOT NULL
) AS [t3] ON [t0].[Name] = [t3].[Name]
LEFT JOIN (
    SELECT [p2].[Name], [p2].[Computed], [p2].[Description], CASE
        WHEN [s].[VehicleName] IS NOT NULL THEN N'SolidRocket'
        WHEN [i].[VehicleName] IS NOT NULL THEN N'IntermittentCombustionEngine'
        WHEN [c3].[VehicleName] IS NOT NULL THEN N'ContinuousCombustionEngine'
    END AS [Discriminator]
    FROM [PoweredVehicles] AS [p2]
    LEFT JOIN [ContinuousCombustionEngines] AS [c3] ON [p2].[Name] = [c3].[VehicleName]
    LEFT JOIN [IntermittentCombustionEngines] AS [i] ON [p2].[Name] = [i].[VehicleName]
    LEFT JOIN [SolidRockets] AS [s] ON [p2].[Name] = [s].[VehicleName]
    INNER JOIN (
        SELECT [v5].[Name]
        FROM [Vehicles] AS [v5]
        INNER JOIN [PoweredVehicles] AS [p3] ON [v5].[Name] = [p3].[Name]
    ) AS [t4] ON [p2].[Name] = [t4].[Name]
    WHERE [p2].[Computed] IS NOT NULL
) AS [t5] ON [v].[Name] = [t5].[Name]
LEFT JOIN (
    SELECT [c5].[VehicleName], [c5].[Capacity], [c5].[FuelType], [s0].[GrainGeometry], CASE
        WHEN [s0].[VehicleName] IS NOT NULL THEN N'SolidFuelTank'
    END AS [Discriminator]
    FROM [CombustionEngines] AS [c5]
    LEFT JOIN [SolidFuelTanks] AS [s0] ON [c5].[VehicleName] = [s0].[VehicleName]
    INNER JOIN (
        SELECT [p4].[Name]
        FROM [PoweredVehicles] AS [p4]
        INNER JOIN [CombustionEngines] AS [c6] ON [p4].[Name] = [c6].[VehicleName]
    ) AS [t6] ON [c5].[VehicleName] = [t6].[Name]
    WHERE [c5].[FuelType] IS NOT NULL OR [c5].[Capacity] IS NOT NULL
) AS [t7] ON [t5].[Name] = [t7].[VehicleName]
ORDER BY [v].[Name]");
        }

        public override void Can_query_shared()
        {
            base.Can_query_shared();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name], [l].[LicenseType], CASE
    WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
END AS [Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [LicensedOperators] AS [l] ON [v].[Name] = [l].[VehicleName]
INNER JOIN (
    SELECT [v0].[Name]
    FROM [Vehicles] AS [v0]
) AS [t] ON [v].[Name] = [t].[Name]");
        }

        public override void Can_query_shared_nonhierarchy()
        {
            base.Can_query_shared_nonhierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name]
    FROM [Vehicles] AS [v0]
) AS [t] ON [v].[Name] = [t].[Name]");
        }

        public override void Can_query_shared_nonhierarchy_with_nonshared_dependent()
        {
            base.Can_query_shared_nonhierarchy_with_nonshared_dependent();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name]
    FROM [Vehicles] AS [v0]
) AS [t] ON [v].[Name] = [t].[Name]");
        }

        public override void Can_query_shared_derived_hierarchy()
        {
            base.Can_query_shared_derived_hierarchy();

            AssertSql(
                @"SELECT [c].[VehicleName], [c].[Capacity], [c].[FuelType], [s].[GrainGeometry], CASE
    WHEN [s].[VehicleName] IS NOT NULL THEN N'SolidFuelTank'
END AS [Discriminator]
FROM [CombustionEngines] AS [c]
LEFT JOIN [SolidFuelTanks] AS [s] ON [c].[VehicleName] = [s].[VehicleName]
INNER JOIN (
    SELECT [p].[Name]
    FROM [PoweredVehicles] AS [p]
    INNER JOIN [CombustionEngines] AS [c0] ON [p].[Name] = [c0].[VehicleName]
) AS [t] ON [c].[VehicleName] = [t].[Name]
WHERE [c].[FuelType] IS NOT NULL OR [c].[Capacity] IS NOT NULL");
        }

        public override void Can_query_shared_derived_nonhierarchy()
        {
            base.Can_query_shared_derived_nonhierarchy();

            AssertSql(
                @"SELECT [c].[VehicleName], [c].[Capacity], [c].[FuelType]
FROM [CombustionEngines] AS [c]
INNER JOIN (
    SELECT [p].[Name]
    FROM [PoweredVehicles] AS [p]
    INNER JOIN [CombustionEngines] AS [c0] ON [p].[Name] = [c0].[VehicleName]
) AS [t] ON [c].[VehicleName] = [t].[Name]
WHERE [c].[FuelType] IS NOT NULL OR [c].[Capacity] IS NOT NULL");
        }

        public override void Can_query_shared_derived_nonhierarchy_all_required()
        {
            base.Can_query_shared_derived_nonhierarchy_all_required();

            AssertSql(
                @"SELECT [c].[VehicleName], [c].[Capacity], [c].[FuelType]
FROM [CombustionEngines] AS [c]
INNER JOIN (
    SELECT [p].[Name]
    FROM [PoweredVehicles] AS [p]
    INNER JOIN [CombustionEngines] AS [c0] ON [p].[Name] = [c0].[VehicleName]
) AS [t] ON [c].[VehicleName] = [t].[Name]
WHERE [c].[FuelType] IS NOT NULL AND [c].[Capacity] IS NOT NULL");
        }

        public override void Can_change_dependent_instance_non_derived()
        {
            base.Can_change_dependent_instance_non_derived();
            AssertSql(
                            @"@p0='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p1='Repair' (Size = 4000)

SET NOCOUNT ON;
INSERT INTO [LicensedOperators] ([VehicleName], [LicenseType])
VALUES (@p0, @p1);",
                            //
                            @"@p1='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='repairman' (Size = 4000)

SET NOCOUNT ON;
UPDATE [Vehicles] SET [Operator_Name] = @p0
WHERE [Name] = @p1;
SELECT @@ROWCOUNT;",
                            //
                            @"SELECT TOP(2) [v].[Name], [v].[SeatingCapacity], [c].[AttachedVehicleName], CASE
    WHEN [c].[Name] IS NOT NULL THEN N'CompositeVehicle'
    WHEN [p].[Name] IS NOT NULL THEN N'PoweredVehicle'
END AS [Discriminator], [t0].[Name], [t0].[Operator_Name], [t0].[LicenseType], [t0].[Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN [CompositeVehicles] AS [c] ON [v].[Name] = [c].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
    END AS [Discriminator]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
    INNER JOIN (
        SELECT [v1].[Name]
        FROM [Vehicles] AS [v1]
    ) AS [t] ON [v0].[Name] = [t].[Name]
) AS [t0] ON [v].[Name] = [t0].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'");
        }

        public override void Can_change_principal_instance_non_derived()
        {
            base.Can_change_principal_instance_non_derived();

            AssertSql(
               @"@p1='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='2'

SET NOCOUNT ON;
UPDATE [Vehicles] SET [SeatingCapacity] = @p0
WHERE [Name] = @p1;
SELECT @@ROWCOUNT;",
               //
               @"SELECT TOP(2) [v].[Name], [v].[SeatingCapacity], [c].[AttachedVehicleName], CASE
    WHEN [c].[Name] IS NOT NULL THEN N'CompositeVehicle'
    WHEN [p].[Name] IS NOT NULL THEN N'PoweredVehicle'
END AS [Discriminator], [t0].[Name], [t0].[Operator_Name], [t0].[LicenseType], [t0].[Discriminator]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN [CompositeVehicles] AS [c] ON [v].[Name] = [c].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN N'LicensedOperator'
    END AS [Discriminator]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
    INNER JOIN (
        SELECT [v1].[Name]
        FROM [Vehicles] AS [v1]
    ) AS [t] ON [v0].[Name] = [t].[Name]
) AS [t0] ON [v].[Name] = [t0].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'");
        }
    }
}
