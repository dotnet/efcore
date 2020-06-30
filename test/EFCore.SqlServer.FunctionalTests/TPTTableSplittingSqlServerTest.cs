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

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override void Can_use_with_redundant_relationships()
        {
            base.Can_use_with_redundant_relationships();

            AssertSql(
                @"SELECT [v].[Name], [v].[SeatingCapacity], CASE
    WHEN [p].[Name] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsPoweredVehicle], [t0].[Name], [t0].[Operator_Name], [t0].[RequiredInt], [t0].[LicenseType], [t0].[IsLicensedOperator], [t3].[Name], [t3].[Type], [t5].[Name], [t5].[Description], [t5].[IsContinuousCombustionEngine], [t5].[IsIntermittentCombustionEngine], [t5].[IsSolidRocket], [t7].[VehicleName], [t7].[Capacity], [t7].[FuelType], [t7].[GrainGeometry], [t7].[IsSolidFuelTank]
FROM [Vehicles] AS [v]
LEFT JOIN [PoweredVehicles] AS [p] ON [v].[Name] = [p].[Name]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Name], [v0].[RequiredInt], [l].[LicenseType], CASE
        WHEN [l].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsLicensedOperator], [t].[Name] AS [Name0]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [LicensedOperators] AS [l] ON [v0].[Name] = [l].[VehicleName]
    INNER JOIN (
        SELECT [v1].[Name], [v1].[SeatingCapacity], CASE
            WHEN [p0].[Name] IS NOT NULL THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS [IsPoweredVehicle]
        FROM [Vehicles] AS [v1]
        LEFT JOIN [PoweredVehicles] AS [p0] ON [v1].[Name] = [p0].[Name]
    ) AS [t] ON [v0].[Name] = [t].[Name]
    WHERE [v0].[RequiredInt] IS NOT NULL
) AS [t0] ON [v].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [v2].[Name], [v2].[Type], [t2].[Name] AS [Name0], [t2].[Name0] AS [Name00]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Operator_Name], [v3].[RequiredInt], [l0].[LicenseType], CASE
            WHEN [l0].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS [IsLicensedOperator], [t1].[Name] AS [Name0]
        FROM [Vehicles] AS [v3]
        LEFT JOIN [LicensedOperators] AS [l0] ON [v3].[Name] = [l0].[VehicleName]
        INNER JOIN (
            SELECT [v4].[Name], [v4].[SeatingCapacity], CASE
                WHEN [p1].[Name] IS NOT NULL THEN CAST(1 AS bit)
                ELSE CAST(0 AS bit)
            END AS [IsPoweredVehicle]
            FROM [Vehicles] AS [v4]
            LEFT JOIN [PoweredVehicles] AS [p1] ON [v4].[Name] = [p1].[Name]
        ) AS [t1] ON [v3].[Name] = [t1].[Name]
        WHERE [v3].[RequiredInt] IS NOT NULL
    ) AS [t2] ON [v2].[Name] = [t2].[Name]
    WHERE [v2].[Type] IS NOT NULL
) AS [t3] ON [t0].[Name] = [t3].[Name]
LEFT JOIN (
    SELECT [p2].[Name], [p2].[Description], CASE
        WHEN [c0].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsContinuousCombustionEngine], CASE
        WHEN [i].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsIntermittentCombustionEngine], CASE
        WHEN [s].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsSolidRocket], [t4].[Name] AS [Name0]
    FROM [PoweredVehicles] AS [p2]
    LEFT JOIN [CombustionEngines] AS [c] ON [p2].[Name] = [c].[VehicleName]
    LEFT JOIN [ContinuousCombustionEngines] AS [c0] ON [p2].[Name] = [c0].[VehicleName]
    LEFT JOIN [IntermittentCombustionEngines] AS [i] ON [p2].[Name] = [i].[VehicleName]
    LEFT JOIN [SolidRockets] AS [s] ON [p2].[Name] = [s].[VehicleName]
    INNER JOIN (
        SELECT [v5].[Name], [v5].[SeatingCapacity]
        FROM [Vehicles] AS [v5]
        INNER JOIN [PoweredVehicles] AS [p3] ON [v5].[Name] = [p3].[Name]
    ) AS [t4] ON [p2].[Name] = [t4].[Name]
    WHERE [p2].[Description] IS NOT NULL
) AS [t5] ON [v].[Name] = [t5].[Name]
LEFT JOIN (
    SELECT [c1].[VehicleName], [c1].[Capacity], [c1].[FuelType], [s0].[GrainGeometry], CASE
        WHEN [s0].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsSolidFuelTank], [t6].[Name]
    FROM [CombustionEngines] AS [c1]
    LEFT JOIN [SolidFuelTanks] AS [s0] ON [c1].[VehicleName] = [s0].[VehicleName]
    INNER JOIN (
        SELECT [p4].[Name], [p4].[Description], CASE
            WHEN [c3].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS [IsContinuousCombustionEngine], CASE
            WHEN [i0].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS [IsIntermittentCombustionEngine], CASE
            WHEN [s1].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
            ELSE CAST(0 AS bit)
        END AS [IsSolidRocket]
        FROM [PoweredVehicles] AS [p4]
        INNER JOIN [CombustionEngines] AS [c2] ON [p4].[Name] = [c2].[VehicleName]
        LEFT JOIN [ContinuousCombustionEngines] AS [c3] ON [p4].[Name] = [c3].[VehicleName]
        LEFT JOIN [IntermittentCombustionEngines] AS [i0] ON [p4].[Name] = [i0].[VehicleName]
        LEFT JOIN [SolidRockets] AS [s1] ON [p4].[Name] = [s1].[VehicleName]
    ) AS [t6] ON [c1].[VehicleName] = [t6].[Name]
    WHERE [c1].[FuelType] IS NOT NULL OR [c1].[Capacity] IS NOT NULL
) AS [t7] ON [t5].[Name] = [t7].[VehicleName]
ORDER BY [v].[Name]");
        }

        public override void Can_query_shared()
        {
            base.Can_query_shared();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name], [v].[RequiredInt], [l].[LicenseType], CASE
    WHEN [l].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsLicensedOperator]
FROM [Vehicles] AS [v]
LEFT JOIN [LicensedOperators] AS [l] ON [v].[Name] = [l].[VehicleName]
INNER JOIN (
    SELECT [v0].[Name], [v0].[SeatingCapacity], CASE
        WHEN [p].[Name] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsPoweredVehicle]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [PoweredVehicles] AS [p] ON [v0].[Name] = [p].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[RequiredInt] IS NOT NULL");
        }

        public override void Can_query_shared_nonhierarchy()
        {
            base.Can_query_shared_nonhierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name], [v].[RequiredInt]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[SeatingCapacity], CASE
        WHEN [p].[Name] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsPoweredVehicle]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [PoweredVehicles] AS [p] ON [v0].[Name] = [p].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[RequiredInt] IS NOT NULL");
        }

        public override void Can_query_shared_nonhierarchy_with_nonshared_dependent()
        {
            base.Can_query_shared_nonhierarchy_with_nonshared_dependent();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name], [v].[RequiredInt]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[SeatingCapacity], CASE
        WHEN [p].[Name] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsPoweredVehicle]
    FROM [Vehicles] AS [v0]
    LEFT JOIN [PoweredVehicles] AS [p] ON [v0].[Name] = [p].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[RequiredInt] IS NOT NULL");
        }

        public override void Can_query_shared_derived_hierarchy()
        {
            base.Can_query_shared_derived_hierarchy();

            AssertSql(
                @"SELECT [c].[VehicleName], [c].[Capacity], [c].[FuelType], [s].[GrainGeometry], CASE
    WHEN [s].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [IsSolidFuelTank]
FROM [CombustionEngines] AS [c]
LEFT JOIN [SolidFuelTanks] AS [s] ON [c].[VehicleName] = [s].[VehicleName]
INNER JOIN (
    SELECT [p].[Name], [p].[Description], CASE
        WHEN [c1].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsContinuousCombustionEngine], CASE
        WHEN [i].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsIntermittentCombustionEngine], CASE
        WHEN [s0].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsSolidRocket]
    FROM [PoweredVehicles] AS [p]
    INNER JOIN [CombustionEngines] AS [c0] ON [p].[Name] = [c0].[VehicleName]
    LEFT JOIN [ContinuousCombustionEngines] AS [c1] ON [p].[Name] = [c1].[VehicleName]
    LEFT JOIN [IntermittentCombustionEngines] AS [i] ON [p].[Name] = [i].[VehicleName]
    LEFT JOIN [SolidRockets] AS [s0] ON [p].[Name] = [s0].[VehicleName]
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
    SELECT [p].[Name], [p].[Description], CASE
        WHEN [c1].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsContinuousCombustionEngine], CASE
        WHEN [i].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsIntermittentCombustionEngine], CASE
        WHEN [s].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsSolidRocket]
    FROM [PoweredVehicles] AS [p]
    INNER JOIN [CombustionEngines] AS [c0] ON [p].[Name] = [c0].[VehicleName]
    LEFT JOIN [ContinuousCombustionEngines] AS [c1] ON [p].[Name] = [c1].[VehicleName]
    LEFT JOIN [IntermittentCombustionEngines] AS [i] ON [p].[Name] = [i].[VehicleName]
    LEFT JOIN [SolidRockets] AS [s] ON [p].[Name] = [s].[VehicleName]
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
    SELECT [p].[Name], [p].[Description], CASE
        WHEN [c1].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsContinuousCombustionEngine], CASE
        WHEN [i].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsIntermittentCombustionEngine], CASE
        WHEN [s].[VehicleName] IS NOT NULL THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS [IsSolidRocket]
    FROM [PoweredVehicles] AS [p]
    INNER JOIN [CombustionEngines] AS [c0] ON [p].[Name] = [c0].[VehicleName]
    LEFT JOIN [ContinuousCombustionEngines] AS [c1] ON [p].[Name] = [c1].[VehicleName]
    LEFT JOIN [IntermittentCombustionEngines] AS [i] ON [p].[Name] = [i].[VehicleName]
    LEFT JOIN [SolidRockets] AS [s] ON [p].[Name] = [s].[VehicleName]
) AS [t] ON [c].[VehicleName] = [t].[Name]
WHERE [c].[FuelType] IS NOT NULL AND [c].[Capacity] IS NOT NULL");
        }

        public override void Can_change_dependent_instance_non_derived()
        {
            base.Can_change_dependent_instance_non_derived();

            AssertSql(
                @"@p3='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='LicensedOperator' (Nullable = false) (Size = 4000)
@p1='repairman' (Size = 4000)
@p2='Repair' (Size = 4000)

SET NOCOUNT ON;
UPDATE [Vehicles] SET [Operator_Discriminator] = @p0, [Operator_Name] = @p1, [LicenseType] = @p2
WHERE [Name] = @p3;
SELECT @@ROWCOUNT;");
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
SELECT @@ROWCOUNT;");
        }
    }
}
