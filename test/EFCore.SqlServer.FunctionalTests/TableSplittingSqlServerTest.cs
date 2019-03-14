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

            // TODO: [Name] shouldn't be selected multiple times and left joins are not needed
            AssertSql(
                @"SELECT [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [t].[Name], [t].[Operator_Discriminator], [t].[Operator_Name], [t].[LicenseType], [t0].[Name], [t0].[Description], [t0].[Engine_Discriminator], [t1].[Name], [t1].[Capacity], [t1].[FuelTank_Discriminator], [t1].[FuelType], [t1].[GrainGeometry]
FROM [Vehicles] AS [v]
LEFT JOIN (
    SELECT [v.Operator].*
    FROM [Vehicles] AS [v.Operator]
    WHERE [v.Operator].[Discriminator] IN (N'PoweredVehicle', N'Vehicle') AND [v.Operator].[Operator_Discriminator] IN (N'LicensedOperator', N'Operator')
) AS [t] ON [v].[Name] = [t].[Name]
LEFT JOIN (
    SELECT [v.Operator.Engine].*
    FROM [Vehicles] AS [v.Operator.Engine]
    WHERE ([v.Operator.Engine].[Discriminator] = N'PoweredVehicle') AND [v.Operator.Engine].[Engine_Discriminator] IN (N'SolidRocket', N'IntermittentCombustionEngine', N'ContinuousCombustionEngine', N'Engine')
) AS [t0] ON [v].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [v.Operator.Engine.FuelTank].*
    FROM [Vehicles] AS [v.Operator.Engine.FuelTank]
    WHERE (([v.Operator.Engine.FuelTank].[Discriminator] = N'PoweredVehicle') AND [v.Operator.Engine.FuelTank].[FuelTank_Discriminator] IN (N'SolidFuelTank', N'FuelTank')) OR ((([v.Operator.Engine.FuelTank].[Discriminator] = N'PoweredVehicle') AND [v.Operator.Engine.FuelTank].[Engine_Discriminator] IN (N'SolidRocket', N'IntermittentCombustionEngine', N'ContinuousCombustionEngine')) AND [v.Operator.Engine.FuelTank].[FuelTank_Discriminator] IN (N'SolidFuelTank', N'FuelTank'))
) AS [t1] ON [t0].[Name] = [t1].[Name]
WHERE [v].[Discriminator] IN (N'PoweredVehicle', N'Vehicle')
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
WHERE [v].[Discriminator] IN (N'PoweredVehicle', N'Vehicle') AND [v].[Operator_Discriminator] IN (N'LicensedOperator', N'Operator')");
        }

        public override void Can_query_shared_derived_hierarchy()
        {
            base.Can_query_shared_derived_hierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelTank_Discriminator], [v].[FuelType], [v].[GrainGeometry]
FROM [Vehicles] AS [v]
WHERE (([v].[Discriminator] = N'PoweredVehicle') AND [v].[FuelTank_Discriminator] IN (N'SolidFuelTank', N'FuelTank')) OR ((([v].[Discriminator] = N'PoweredVehicle') AND [v].[Engine_Discriminator] IN (N'SolidRocket', N'IntermittentCombustionEngine', N'ContinuousCombustionEngine')) AND [v].[FuelTank_Discriminator] IN (N'SolidFuelTank', N'FuelTank'))");
        }

        public override void Can_query_shared_derived_nonhierarchy()
        {
            base.Can_query_shared_derived_nonhierarchy();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
WHERE (([v].[Discriminator] = N'PoweredVehicle') AND ([v].[FuelType] IS NOT NULL OR [v].[Capacity] IS NOT NULL)) OR ((([v].[Discriminator] = N'PoweredVehicle') AND [v].[Engine_Discriminator] IN (N'SolidRocket', N'IntermittentCombustionEngine', N'ContinuousCombustionEngine')) AND ([v].[FuelType] IS NOT NULL OR [v].[Capacity] IS NOT NULL))");
        }

        public override void Can_query_shared_derived_nonhierarchy_all_required()
        {
            base.Can_query_shared_derived_nonhierarchy_all_required();

            AssertSql(
                @"SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
WHERE (([v].[Discriminator] = N'PoweredVehicle') AND ([v].[FuelType] IS NOT NULL AND [v].[Capacity] IS NOT NULL)) OR ((([v].[Discriminator] = N'PoweredVehicle') AND [v].[Engine_Discriminator] IN (N'SolidRocket', N'IntermittentCombustionEngine', N'ContinuousCombustionEngine')) AND ([v].[FuelType] IS NOT NULL AND [v].[Capacity] IS NOT NULL))");
        }

        public override void Can_change_dependent_instance_non_derived()
        {
            base.Can_change_dependent_instance_non_derived();

            AssertContainsSql(
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

            AssertContainsSql(
                @"@p1='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='2'

SET NOCOUNT ON;
UPDATE [Vehicles] SET [SeatingCapacity] = @p0
WHERE [Name] = @p1;
SELECT @@ROWCOUNT;");
        }
    }
}
