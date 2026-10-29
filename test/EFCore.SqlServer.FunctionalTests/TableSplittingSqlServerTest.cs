// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

namespace Microsoft.EntityFrameworkCore;

public class TableSplittingSqlServerTest(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
    : TableSplittingTestBase(fixture, testOutputHelper)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    public override async Task Can_use_with_redundant_relationships()
    {
        await base.Can_use_with_redundant_relationships();

        // TODO: [Name] shouldn't be selected multiple times and no joins are needed
        AssertSql(
            """
SELECT [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType], [v2].[Name], [v2].[Active], [v2].[Type], [v4].[Name], [v4].[Computed], [v4].[Description], [v4].[Engine_Discriminator], [v6].[Name], [v6].[Capacity], [v6].[FuelTank_Discriminator], [v6].[FuelType], [v6].[GrainGeometry]
FROM [Vehicles] AS [v]
LEFT JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]
LEFT JOIN (
    SELECT [v1].[Name], [v1].[Active], [v1].[Type]
    FROM [Vehicles] AS [v1]
    WHERE [v1].[Active] IS NOT NULL
) AS [v2] ON [v0].[Name] = CASE
    WHEN [v2].[Active] IS NOT NULL THEN [v2].[Name]
END
LEFT JOIN (
    SELECT [v3].[Name], [v3].[Computed], [v3].[Description], [v3].[Engine_Discriminator]
    FROM [Vehicles] AS [v3]
    WHERE [v3].[Computed] IS NOT NULL AND [v3].[Engine_Discriminator] IS NOT NULL
) AS [v4] ON [v].[Name] = [v4].[Name]
LEFT JOIN (
    SELECT [v5].[Name], [v5].[Capacity], [v5].[FuelTank_Discriminator], [v5].[FuelType], [v5].[GrainGeometry]
    FROM [Vehicles] AS [v5]
    WHERE [v5].[Capacity] IS NOT NULL AND [v5].[FuelTank_Discriminator] IS NOT NULL
) AS [v6] ON [v4].[Name] = [v6].[Name]
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
WHERE [v].[Capacity] IS NOT NULL AND [v].[FuelTank_Discriminator] IS NOT NULL
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
WHERE [v].[Capacity] IS NOT NULL AND [v].[FuelType] IS NOT NULL
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
SELECT TOP(1) [v].[Name], [v].[Discriminator], [v].[SeatingCapacity], [v].[AttachedVehicleName], [v0].[Name], [v0].[Operator_Discriminator], [v0].[Operator_Name], [v0].[LicenseType], [v2].[Name], [v2].[Active], [v2].[Type]
FROM [Vehicles] AS [v]
LEFT JOIN [Vehicles] AS [v0] ON [v].[Name] = [v0].[Name]
LEFT JOIN (
    SELECT [v1].[Name], [v1].[Active], [v1].[Type]
    FROM [Vehicles] AS [v1]
    WHERE [v1].[Active] IS NOT NULL
) AS [v2] ON [v0].[Name] = CASE
    WHEN [v2].[Active] IS NOT NULL THEN [v2].[Name]
END
WHERE [v].[Name] = N'AIM-9M Sidewinder'
ORDER BY [v].[Name]
""");
    }

    public override async Task Can_remove_optional_dependent_with_json_complex_property()
    {
        await base.Can_remove_optional_dependent_with_json_complex_property();

        AssertSql(
            """
@p0=NULL
@p2='1'
@p1=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Cars] SET [Specs] = @p0, [Color] = @p1
OUTPUT 1
WHERE [Id] = @p2;
""",
            //
            """
SELECT [c1].[c]
FROM [Cars] AS [c]
LEFT JOIN (
    SELECT [c0].[Id], [c0].[Color], [c0].[Specs] AS [c], [c0].[Specs]
    FROM [Cars] AS [c0]
    WHERE [c0].[Color] IS NOT NULL AND [c0].[Specs] IS NOT NULL
) AS [c1] ON [c].[Id] = CASE
    WHEN [c1].[Color] IS NOT NULL AND [c1].[Specs] IS NOT NULL THEN [c1].[Id]
END
ORDER BY [c].[Id]
""");
    }

    public override async Task Can_query_optional_dependent_with_nullable_json_and_scalar_properties(bool buffered)
    {
        await base.Can_query_optional_dependent_with_nullable_json_and_scalar_properties(buffered);

        AssertSql(
            """
SELECT [g].[Id], [g1].[Id], [g1].[Label], [g1].[c]
FROM [Gadgets] AS [g]
LEFT JOIN (
    SELECT [g0].[Id], [g0].[Label], [g0].[Specs] AS [c], [g0].[Specs]
    FROM [Gadgets] AS [g0]
    WHERE [g0].[Label] IS NOT NULL OR [g0].[Specs] IS NOT NULL
) AS [g1] ON [g].[Id] = CASE
    WHEN [g1].[Label] IS NOT NULL OR [g1].[Specs] IS NOT NULL THEN [g1].[Id]
END
ORDER BY [g].[Id]
""",
            //
            """
SELECT CASE
    WHEN [g].[Label] IS NOT NULL OR [g].[Specs] IS NOT NULL THEN [g].[Id]
END
FROM [Gadgets] AS [g]
WHERE [g].[Label] IS NOT NULL OR [g].[Specs] IS NOT NULL
ORDER BY CASE
    WHEN [g].[Label] IS NOT NULL OR [g].[Specs] IS NOT NULL THEN [g].[Id]
END
""",
            //
            """
SELECT [g].[Id]
FROM [Gadgets] AS [g]
LEFT JOIN (
    SELECT [g0].[Id], [g0].[Label], [g0].[Specs] AS [c], [g0].[Specs]
    FROM [Gadgets] AS [g0]
    WHERE [g0].[Label] IS NOT NULL OR [g0].[Specs] IS NOT NULL
) AS [g1] ON [g].[Id] = CASE
    WHEN [g1].[Label] IS NOT NULL OR [g1].[Specs] IS NOT NULL THEN [g1].[Id]
END
WHERE [g1].[Label] IS NULL AND [g1].[c] IS NULL
""",
            //
            """
SELECT [g].[Id]
FROM [Gadgets] AS [g]
LEFT JOIN (
    SELECT [g0].[Id], [g0].[Label], [g0].[Specs] AS [c], [g0].[Specs]
    FROM [Gadgets] AS [g0]
    WHERE [g0].[Label] IS NOT NULL OR [g0].[Specs] IS NOT NULL
) AS [g1] ON [g].[Id] = CASE
    WHEN [g1].[Label] IS NOT NULL OR [g1].[Specs] IS NOT NULL THEN [g1].[Id]
END
WHERE [g1].[Label] IS NOT NULL OR [g1].[c] IS NOT NULL
ORDER BY [g].[Id]
""",
            //
            """
SELECT [g].[Id], [g].[Label], [g].[Specs]
FROM [Gadgets] AS [g]
WHERE [g].[Label] IS NOT NULL OR [g].[Specs] IS NOT NULL
ORDER BY CASE
    WHEN [g].[Label] IS NOT NULL OR [g].[Specs] IS NOT NULL THEN [g].[Id]
END
""",
            //
            """
SELECT [m].[Id], [m].[Label], [m].[Specs]
FROM (
    SELECT Specs, Id, Label FROM Gadgets
) AS [m]
""");
    }

    public override async Task Can_query_optional_dependent_with_only_json_complex_collection()
    {
        await base.Can_query_optional_dependent_with_only_json_complex_collection();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b1].[Id], [b1].[c]
FROM [Boats] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Boats] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
ORDER BY [b].[Id]
""",
            //
            """
SELECT CASE
    WHEN [b].[Specs] IS NOT NULL THEN [b].[Id]
END
FROM [Boats] AS [b]
WHERE [b].[Specs] IS NOT NULL
ORDER BY CASE
    WHEN [b].[Specs] IS NOT NULL THEN [b].[Id]
END
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [Boats] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Boats] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
WHERE [b1].[c] IS NULL
""",
            //
            """
SELECT [b].[Id]
FROM [Boats] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Boats] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
WHERE [b1].[c] IS NOT NULL
ORDER BY [b].[Id]
""",
            //
            """
@p0=NULL
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Boats] SET [Specs] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT [b].[Id], [b].[Name], [b1].[Id], [b1].[c]
FROM [Boats] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Boats] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
ORDER BY [b].[Id]
""");
    }

    public override async Task Can_query_optional_dependent_with_only_json_complex_property()
    {
        await base.Can_query_optional_dependent_with_only_json_complex_property();

        AssertSql(
            """
SELECT [b].[Id], [b].[Name], [b1].[Id], [b1].[c]
FROM [Bikes] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Bikes] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
ORDER BY [b].[Id]
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Specs]
FROM [Bikes] AS [b]
WHERE [b].[Specs] IS NOT NULL
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [Bikes] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Bikes] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
WHERE [b1].[c] IS NULL
""",
            //
            """
SELECT TOP(2) [b].[Id], [b].[Name]
FROM [Bikes] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Bikes] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
WHERE [b1].[c] IS NOT NULL
""",
            //
            """
@p0=NULL
@p1='1'

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Bikes] SET [Specs] = @p0
OUTPUT 1
WHERE [Id] = @p1;
""",
            //
            """
SELECT [b].[Id], [b].[Name], [b1].[Id], [b1].[c]
FROM [Bikes] AS [b]
LEFT JOIN (
    SELECT [b0].[Id], [b0].[Specs] AS [c], [b0].[Specs]
    FROM [Bikes] AS [b0]
    WHERE [b0].[Specs] IS NOT NULL
) AS [b1] ON [b].[Id] = CASE
    WHEN [b1].[Specs] IS NOT NULL THEN [b1].[Id]
END
""");
    }

    public override async Task Can_remove_optional_TPT_dependent_with_json_complex_properties()
    {
        await base.Can_remove_optional_TPT_dependent_with_json_complex_properties();

        AssertSql(
            """
@p0=NULL
@p2='1'
@p1=NULL (Size = 4000)
@p3='1'

SET NOCOUNT ON;
UPDATE [Cars] SET [Specs] = @p0, [Color] = @p1
OUTPUT 1
WHERE [Id] = @p2;
DELETE FROM [SportsCarDetails]
OUTPUT 1
WHERE [Id] = @p3;
""",
            //
            """
SELECT [c].[Id], [c].[Name], [s0].[Id], [s0].[Color], [s0].[c], [s0].[c0], [s0].[Discriminator]
FROM [Cars] AS [c]
LEFT JOIN (
    SELECT [c0].[Id], [c0].[Color], [c0].[Specs] AS [c], [s].[Specs] AS [c0], CASE
        WHEN [s].[Id] IS NOT NULL THEN N'SportsCarDetails'
    END AS [Discriminator], [c0].[Specs]
    FROM [Cars] AS [c0]
    LEFT JOIN [SportsCarDetails] AS [s] ON [c0].[Id] = [s].[Id]
    WHERE [c0].[Color] IS NOT NULL AND [c0].[Specs] IS NOT NULL
) AS [s0] ON [c].[Id] = CASE
    WHEN [s0].[Color] IS NOT NULL AND [s0].[Specs] IS NOT NULL THEN [s0].[Id]
END
ORDER BY [c].[Id]
""");
    }

    public override async Task ExecuteUpdate_works_for_table_sharing(bool async)
    {
        await base.ExecuteUpdate_works_for_table_sharing(async);

        AssertSql(
            """
@p='1'

SET NOCOUNT OFF;
UPDATE [v]
SET [v].[SeatingCapacity] = @p
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
