// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

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
"""
SELECT [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType], [t].[Name], [t].[Active], [t].[Type], [t0].[Name], [t0].[Computed], [t0].[Description], [t0].[Engine_Discriminator], [t1].[Name], [t1].[Capacity], [t1].[FuelTank_Discriminator], [t1].[FuelType], [t1].[GrainGeometry]
FROM [Vehicles] AS [v]
LEFT JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]
LEFT JOIN (
    SELECT [v1].[Name], [v1].[Active], [v1].[Type]
    FROM [Vehicles] AS [v1]
    WHERE [v1].[Active] IS NOT NULL
) AS [t] ON [v0].[Name] = CASE
    WHEN [t].[Active] IS NOT NULL THEN [t].[Name]
END
LEFT JOIN (
    SELECT [v2].[Name], [v2].[Computed], [v2].[Description], [v2].[Engine_Discriminator]
    FROM [Vehicles] AS [v2]
    WHERE ([v2].[Computed] IS NOT NULL) AND ([v2].[Engine_Discriminator] IS NOT NULL)
) AS [t0] ON [v].[Name] = [t0].[Name]
LEFT JOIN (
    SELECT [v3].[Name], [v3].[Capacity], [v3].[FuelTank_Discriminator], [v3].[FuelType], [v3].[GrainGeometry]
    FROM [Vehicles] AS [v3]
    WHERE ([v3].[Capacity] IS NOT NULL) AND ([v3].[FuelTank_Discriminator] IS NOT NULL)
) AS [t1] ON [t0].[Name] = [t1].[Name]
ORDER BY [v].[Name]
""");
    }

    public override async Task Can_query_shared()
    {
        await base.Can_query_shared();

        AssertSql(
"""
SELECT [v].[Name], [v].[Operator_Discriminator], [v].[Operator_Name], [v].[LicenseType]
FROM [Vehicles] AS [v]
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
SELECT [v].[Name], [v].[Capacity], [v].[FuelTank_Discriminator], [v].[FuelType], [v].[GrainGeometry]
FROM [Vehicles] AS [v]
WHERE ([v].[Capacity] IS NOT NULL) AND ([v].[FuelTank_Discriminator] IS NOT NULL)
""");
    }

    public override async Task Can_query_shared_derived_nonhierarchy()
    {
        await base.Can_query_shared_derived_nonhierarchy();

        AssertSql(
"""
SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
WHERE [v].[Capacity] IS NOT NULL
""");
    }

    public override async Task Can_query_shared_derived_nonhierarchy_all_required()
    {
        await base.Can_query_shared_derived_nonhierarchy_all_required();

        AssertSql(
"""
SELECT [v].[Name], [v].[Capacity], [v].[FuelType]
FROM [Vehicles] AS [v]
WHERE ([v].[Capacity] IS NOT NULL) AND ([v].[FuelType] IS NOT NULL)
""");
    }

    public override async Task Can_change_dependent_instance_non_derived()
    {
        await base.Can_change_dependent_instance_non_derived();

        AssertSql(
"""
@p3='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='LicensedOperator' (Nullable = false) (Size = 21)
@p1='Repair' (Size = 4000)
@p2='repairman' (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Vehicles] SET [Operator_Discriminator] = @p0, [LicenseType] = @p1, [Operator_Name] = @p2
OUTPUT 1
WHERE [Name] = @p3;
""",
            //
"""
SELECT TOP(2) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
FROM [Vehicles] AS [v]
LEFT JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]
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
SELECT TOP(2) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType]
FROM [Vehicles] AS [v]
LEFT JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]
WHERE [v].[Name] = N'Trek Pro Fit Madone 6 Series'
""");
    }

    public override async Task Optional_dependent_materialized_when_no_properties()
    {
        await base.Optional_dependent_materialized_when_no_properties();

        AssertSql(
"""
SELECT TOP(1) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType], [t].[Name], [t].[Active], [t].[Type]
FROM [Vehicles] AS [v]
LEFT JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]
LEFT JOIN (
    SELECT [v1].[Name], [v1].[Active], [v1].[Type]
    FROM [Vehicles] AS [v1]
    WHERE [v1].[Active] IS NOT NULL
) AS [t] ON [v0].[Name] = CASE
    WHEN [t].[Active] IS NOT NULL THEN [t].[Name]
END
WHERE [v].[Name] = N'AIM-9M Sidewinder'
ORDER BY [v].[Name]
""");
    }

    public override async Task ExecuteUpdate_works_for_table_sharing(bool async)
    {
        await base.ExecuteUpdate_works_for_table_sharing(async);

        AssertSql(
"""
UPDATE [v]
SET [v].[SeatingCapacity] = 1
FROM [Vehicles] AS [v]
""",
            //
"""
SELECT CASE
    WHEN NOT EXISTS (
        SELECT 1
        FROM [Vehicles] AS [v]
        WHERE [v].[SeatingCapacity] <> 1) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END
""");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Engine>().ToTable("Vehicles")
            .Property(e => e.Computed).HasComputedColumnSql("1", stored: true);
    }
}
