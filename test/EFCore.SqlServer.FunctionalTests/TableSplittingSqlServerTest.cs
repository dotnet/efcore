// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
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

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        public override void Can_use_with_redundant_relationships()
        {
            base.Can_use_with_redundant_relationships();

            // TODO: [Name] shouldn't be selected multiple times and no joins are needed
            AssertSql(
                @"SELECT [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [t].[Name], [t].[Operator_Discriminator], [t].[Operator_Name], [t].[LicenseType], [t1].[Name], [t1].[Type], [t3].[Name], [t3].[Computed], [t3].[Description], [t3].[Engine_Discriminator], [t7].[Name], [t7].[Capacity], [t7].[FuelTank_Discriminator], [t7].[FuelType], [t7].[GrainGeometry]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
    FROM [Vehicles] AS [v0]
    INNER JOIN [Vehicles] AS [v1] ON [v0].[Name] = [v1].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
LEFT JOIN (
    SELECT [v2].[Name], [v2].[Type]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name]
        FROM [Vehicles] AS [v3]
        INNER JOIN [Vehicles] AS [v4] ON [v3].[Name] = [v4].[Name]
    ) AS [t0] ON [v2].[Name] = [t0].[Name]
    WHERE [v2].[Type] IS NOT NULL
) AS [t1] ON [t].[Name] = [t1].[Name]
LEFT JOIN (
    SELECT [v5].[Name], [v5].[Computed], [v5].[Description], [v5].[Engine_Discriminator]
    FROM [Vehicles] AS [v5]
    INNER JOIN (
        SELECT [v6].[Name]
        FROM [Vehicles] AS [v6]
        WHERE [v6].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t2] ON [v5].[Name] = [t2].[Name]
    WHERE [v5].[Engine_Discriminator] IS NOT NULL AND [v5].[Computed] IS NOT NULL
) AS [t3] ON [v].[Name] = [t3].[Name]
LEFT JOIN (
    SELECT [v7].[Name], [v7].[Capacity], [v7].[FuelTank_Discriminator], [v7].[FuelType], [v7].[GrainGeometry]
    FROM [Vehicles] AS [v7]
    INNER JOIN (
        SELECT [v8].[Name], [v8].[Discriminator], [v8].[SeatingCapacity], [v8].[AttachedVehicleName]
        FROM [Vehicles] AS [v8]
        WHERE [v8].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t4] ON [v7].[Name] = [t4].[Name]
    WHERE [v7].[FuelTank_Discriminator] IS NOT NULL
    UNION
    SELECT [v9].[Name], [v9].[Capacity], [v9].[FuelTank_Discriminator], [v9].[FuelType], [v9].[GrainGeometry]
    FROM [Vehicles] AS [v9]
    INNER JOIN (
        SELECT [v10].[Name], [v10].[Computed], [v10].[Description], [v10].[Engine_Discriminator], [t5].[Name] AS [Name0]
        FROM [Vehicles] AS [v10]
        INNER JOIN (
            SELECT [v11].[Name], [v11].[Discriminator], [v11].[SeatingCapacity], [v11].[AttachedVehicleName]
            FROM [Vehicles] AS [v11]
            WHERE [v11].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
        ) AS [t5] ON [v10].[Name] = [t5].[Name]
        WHERE [v10].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
    ) AS [t6] ON [v9].[Name] = [t6].[Name]
    WHERE [v9].[FuelTank_Discriminator] IS NOT NULL
) AS [t7] ON [t3].[Name] = [t7].[Name]
ORDER BY [v].[Name]");
        }

        public override void Can_query_shared()
        {
            base.Can_query_shared();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Discriminator], [v].[Operator_Name], [v].[LicenseType]
FROM [Vehicles] AS [v]
INNER JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]");
        }

        public override void Can_query_shared_nonhierarchy()
        {
            base.Can_query_shared_nonhierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
INNER JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]");
        }

        public override void Can_query_shared_nonhierarchy_with_nonshared_dependent()
        {
            base.Can_query_shared_nonhierarchy_with_nonshared_dependent();

            AssertSql(
                @"SELECT [v].[Name], [v].[Operator_Name]
FROM [Vehicles] AS [v]
INNER JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]");
        }

        public override void Can_query_shared_derived_hierarchy()
        {
            base.Can_query_shared_derived_hierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelTank_Discriminator], [v].[FuelType], [v].[GrainGeometry]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity], [v0].[AttachedVehicleName]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[FuelTank_Discriminator] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelTank_Discriminator], [v1].[FuelType], [v1].[GrainGeometry]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator], [t0].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity], [v3].[AttachedVehicleName]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t0] ON [v2].[Name] = [t0].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]
WHERE [v1].[FuelTank_Discriminator] IS NOT NULL");
        }

        public override void Can_query_shared_derived_nonhierarchy()
        {
            base.Can_query_shared_derived_nonhierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity], [v0].[AttachedVehicleName]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[FuelType] IS NOT NULL OR [v].[Capacity] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelType]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator], [t0].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity], [v3].[AttachedVehicleName]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t0] ON [v2].[Name] = [t0].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]
WHERE [v1].[FuelType] IS NOT NULL OR [v1].[Capacity] IS NOT NULL");
        }

        public override void Can_query_shared_derived_nonhierarchy_all_required()
        {
            base.Can_query_shared_derived_nonhierarchy_all_required();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
INNER JOIN (
    SELECT [v0].[Name], [v0].[Discriminator], [v0].[SeatingCapacity], [v0].[AttachedVehicleName]
    FROM [Vehicles] AS [v0]
    WHERE [v0].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[FuelType] IS NOT NULL AND [v].[Capacity] IS NOT NULL
UNION
SELECT [v1].[Name], [v1].[Capacity], [v1].[FuelType]
FROM [Vehicles] AS [v1]
INNER JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator], [t0].[Name] AS [Name0]
    FROM [Vehicles] AS [v2]
    INNER JOIN (
        SELECT [v3].[Name], [v3].[Discriminator], [v3].[SeatingCapacity], [v3].[AttachedVehicleName]
        FROM [Vehicles] AS [v3]
        WHERE [v3].[Discriminator] IN (N'PoweredVehicle', N'CompositeVehicle')
    ) AS [t0] ON [v2].[Name] = [t0].[Name]
    WHERE [v2].[Engine_Discriminator] IN (N'ContinuousCombustionEngine', N'IntermittentCombustionEngine', N'SolidRocket')
) AS [t1] ON [v1].[Name] = [t1].[Name]
WHERE [v1].[FuelType] IS NOT NULL AND [v1].[Capacity] IS NOT NULL");
        }

        public override void Can_change_dependent_instance_non_derived()
        {
            base.Can_change_dependent_instance_non_derived();

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
                @"SELECT TOP(2) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [t].[Name], [t].[Operator_Discriminator], [t].[Operator_Name], [t].[LicenseType]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
    FROM [Vehicles] AS [v0]
    INNER JOIN [Vehicles] AS [v1] ON [v0].[Name] = [v1].[Name]
) AS [t] ON [v].[Name] = [t].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Engine>().ToTable("Vehicles")
                .Property(e => e.Computed).HasComputedColumnSql("1", stored: true);
        }
    }
}
