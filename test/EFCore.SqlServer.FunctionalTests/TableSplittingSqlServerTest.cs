// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class TableSplittingSqlServerTest : TableSplittingTestBase
    {
        public TableSplittingSqlServerTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override void Can_use_with_redundant_relationships()
        {
            base.Can_use_with_redundant_relationships();

            // TODO: [Name] shouldn't be selected multiple times and no joins are needed
            AssertSql(
                @"SELECT [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [t0].[Name], [t0].[Operator_Discriminator], [t0].[Operator_Name], [t0].[LicenseType], [t3].[Name], [t3].[Type], [t5].[Name], [t5].[Description], [t5].[Engine_Discriminator], [t9].[Name], [t9].[Capacity], [t9].[FuelTank_Discriminator], [t9].[FuelType], [t9].[GrainGeometry]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType], [t].[Name] AS [Name0]
    FROM [Vehicles] AS [v0]
    INNER JOIN (
        SELECT [v1].[Name], [v1].[Discriminator], [v1].[SeatingCapacity]
        FROM [Vehicles] AS [v1]
        WHERE [v1].[Discriminator] IN (N'Vehicle', N'PoweredVehicle')
    ) AS [t] ON [v0].[Name] = [t].[Name]
    WHERE [v0].[Operator_Discriminator] IN (N'Operator', N'LicensedOperator')
) AS [t0] ON [v].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [v2].[Name], [v2].[Type], [t2].[Name] AS [Name0], [t2].[Name0] AS [Name00]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Operator_Discriminator], [v3].[Operator_Name], [v3].[LicenseType], [t1].[Name] AS [Name0]
        FROM [Vehicles] AS [v3]
        INNER JOIN (
            SELECT [v4].[Name], [v4].[Discriminator], [v4].[SeatingCapacity]
            FROM [Vehicles] AS [v4]
            WHERE [v4].[Discriminator] IN (N'Vehicle', N'PoweredVehicle')
        ) AS [t1] ON [v3].[Name] = [t1].[Name]
        WHERE [v3].[Operator_Discriminator] IN (N'Operator', N'LicensedOperator')
    ) AS [t2] ON [v2].[Name] = [t2].[Name]
    WHERE [v2].[Type] IS NOT NULL
) AS [t3] ON [t0].[Name] = [t3].[Name]
LEFT JOIN (
    SELECT [v5].[Name], [v5].[Description], [v5].[Engine_Discriminator], [t4].[Name] AS [Name0]
    FROM [Vehicles] AS [v5]
    INNER JOIN (
        SELECT [v6].[Name], [v6].[Discriminator], [v6].[SeatingCapacity]
        FROM [Vehicles] AS [v6]
        WHERE [v6].[Discriminator] = N'PoweredVehicle'
    ) AS [t4] ON [v5].[Name] = [t4].[Name]
    WHERE [v5].[Engine_Discriminator] IN (N'Engine', N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t5] ON [v].[Name] = [t5].[Name]
LEFT JOIN (
    SELECT [v7].[Name], [v7].[Capacity], [v7].[FuelTank_Discriminator], [v7].[FuelType], [v7].[GrainGeometry]
    FROM [Vehicles] AS [v7]
    INNER JOIN (
        SELECT [v8].[Name], [v8].[Discriminator], [v8].[SeatingCapacity]
        FROM [Vehicles] AS [v8]
        WHERE [v8].[Discriminator] = N'PoweredVehicle'
    ) AS [t6] ON [v7].[Name] = [t6].[Name]
    WHERE [v7].[FuelTank_Discriminator] IN (N'FuelTank', N'SolidFuelTank')
    UNION
    SELECT [v9].[Name], [v9].[Capacity], [v9].[FuelTank_Discriminator], [v9].[FuelType], [v9].[GrainGeometry]
    FROM [Vehicles] AS [v9]
    INNER JOIN (
        SELECT [v10].[Name], [v10].[Description], [v10].[Engine_Discriminator], [t7].[Name] AS [Name0]
        FROM [Vehicles] AS [v10]
        INNER JOIN (
            SELECT [v11].[Name], [v11].[Discriminator], [v11].[SeatingCapacity]
            FROM [Vehicles] AS [v11]
            WHERE [v11].[Discriminator] = N'PoweredVehicle'
        ) AS [t7] ON [v10].[Name] = [t7].[Name]
        WHERE [v10].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
    ) AS [t8] ON [v9].[Name] = [t8].[Name]
) AS [t9] ON [t5].[Name] = [t9].[Name]
WHERE [v].[Discriminator] IN (N'Vehicle', N'PoweredVehicle')
ORDER BY [v].[Name]");
        }

        public override void Can_use_with_chained_relationships()
        {
            base.Can_use_with_chained_relationships();
        }

        public override void Can_use_with_fanned_relationships()
        {
            base.Can_use_with_fanned_relationships();
        }

        public override void Can_query_shared()
        {
            base.Can_query_shared();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Discriminator], [v].[Operator_Name], [v].[LicenseType]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] IN (N'Vehicle', N'PoweredVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Operator_Discriminator] IN (N'Operator', N'LicensedOperator')");
        }

        public override void Can_query_shared_nonhierarchy()
        {
            base.Can_query_shared_nonhierarchy();

            AssertSql(
                @"SELECT [t0].[Name], [t0].[Operator_Name]
FROM (
    SELECT [v].[Name], [v].[Operator_Name]
    FROM [Vehicles] AS [v]
    WHERE [v].[Operator_Name] IS NOT NULL
    UNION
    SELECT [v0].[Name], [v0].[Operator_Name]
    FROM [Vehicles] AS [v0]
    INNER JOIN (
        SELECT [v1].[Name], [v1].[Type]
        FROM [Vehicles] AS [v1]
        WHERE [v1].[Type] IS NOT NULL
    ) AS [t] ON [v0].[Name] = [t].[Name]
) AS [t0]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Discriminator], [v2].[SeatingCapacity]
    FROM [Vehicles] AS [v2]
    WHERE [v2].[Discriminator] IN (N'Vehicle', N'PoweredVehicle')
) AS [t1] ON [t0].[Name] = [t1].[Name]");
        }

        public override void Can_query_shared_nonhierarchy_with_nonshared_dependent()
        {
            base.Can_query_shared_nonhierarchy_with_nonshared_dependent();

            AssertSql(
                @"SELECT [t].[Name], [t].[Operator_Name]
FROM (
    SELECT [v].[Name], [v].[Operator_Name]
    FROM [Vehicles] AS [v]
    WHERE [v].[Operator_Name] IS NOT NULL
    UNION
    SELECT [v0].[Name], [v0].[Operator_Name]
    FROM [Vehicles] AS [v0]
    INNER JOIN [OperatorDetails] AS [o] ON [v0].[Name] = [o].[VehicleName]
) AS [t]
INNER JOIN (
    SELECT [v1].[Name], [v1].[Discriminator], [v1].[SeatingCapacity]
    FROM [Vehicles] AS [v1]
    WHERE [v1].[Discriminator] IN (N'Vehicle', N'PoweredVehicle')
) AS [t0] ON [t].[Name] = [t0].[Name]");
        }

        public override void Can_query_shared_derived_hierarchy()
        {
            base.Can_query_shared_derived_hierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelTank_Discriminator], [v].[FuelType], [v].[GrainGeometry]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] = N'PoweredVehicle'
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[FuelTank_Discriminator] IN (N'FuelTank', N'SolidFuelTank')
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelTank_Discriminator], [v1].[FuelType], [v1].[GrainGeometry]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Description], [v2].[Engine_Discriminator], [t0].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] = N'PoweredVehicle'
    ) AS [t0] ON [v2].[Name] = [t0].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]");
        }

        public override void Can_query_shared_derived_nonhierarchy()
        {
            base.Can_query_shared_derived_nonhierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] = N'PoweredVehicle'
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[FuelType] IS NOT NULL OR [v].[Capacity] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelType]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Description], [v2].[Engine_Discriminator], [t0].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] = N'PoweredVehicle'
    ) AS [t0] ON [v2].[Name] = [t0].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]");
        }

        public override void Can_query_shared_derived_nonhierarchy_all_required()
        {
            base.Can_query_shared_derived_nonhierarchy_all_required();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] = N'PoweredVehicle'
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[FuelType] IS NOT NULL AND [v].[Capacity] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelType]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Description], [v2].[Engine_Discriminator], [t0].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] = N'PoweredVehicle'
    ) AS [t0] ON [v2].[Name] = [t0].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]");
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
