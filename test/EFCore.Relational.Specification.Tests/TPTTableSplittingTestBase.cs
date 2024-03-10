// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class TPTTableSplittingTestBase : TableSplittingTestBase
{
    protected TPTTableSplittingTestBase(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    public override Task Can_use_optional_dependents_with_shared_concurrency_tokens()
        // TODO: Issue #22060
        => Task.CompletedTask;

    // This fails in TPT rather than table sharing. We have coverage for it elsewhere
    public override Task ExecuteDelete_throws_for_table_sharing(bool async)
        => Task.CompletedTask;

    protected override string StoreName
        => "TPTTableSplittingTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().ToTable("Vehicles");
        modelBuilder.Entity<PoweredVehicle>().ToTable("PoweredVehicles");
        modelBuilder.Entity<CompositeVehicle>().ToTable("CompositeVehicles");

        modelBuilder.Entity<Operator>(
            eb =>
            {
                eb.ToTable("Vehicles");
                eb.HasOne(e => e.Vehicle)
                    .WithOne(e => e.Operator)
                    .HasForeignKey<Operator>(e => e.VehicleName)
                    .OnDelete(DeleteBehavior.ClientCascade);
                eb.HasOne(e => e.Details)
                    .WithOne()
                    .HasForeignKey<OperatorDetails>(e => e.VehicleName)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

        modelBuilder.Entity<LicensedOperator>().ToTable("LicensedOperators");

        modelBuilder.Entity<OperatorDetails>().ToTable("Vehicles");

        modelBuilder.Entity<Engine>().ToTable("PoweredVehicles")
            .HasOne(e => e.Vehicle).WithOne(e => e.Engine).OnDelete(DeleteBehavior.ClientCascade);
        modelBuilder.Entity<CombustionEngine>().ToTable("CombustionEngines");
        modelBuilder.Entity<IntermittentCombustionEngine>().ToTable("IntermittentCombustionEngines");
        modelBuilder.Entity<ContinuousCombustionEngine>().ToTable("ContinuousCombustionEngines");
        modelBuilder.Entity<SolidRocket>().ToTable("SolidRockets").Ignore(e => e.SolidFuelTank);

        modelBuilder.Entity<FuelTank>(
            eb =>
            {
                eb.ToTable("CombustionEngines");

                eb.HasOne(e => e.Engine)
                    .WithOne(e => e.FuelTank)
                    .HasForeignKey<FuelTank>(e => e.VehicleName)
                    .OnDelete(DeleteBehavior.ClientCascade);
                eb.HasOne(e => e.Vehicle)
                    .WithOne()
                    .HasForeignKey<FuelTank>(e => e.VehicleName)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });
        modelBuilder.Entity<SolidFuelTank>().ToTable("SolidFuelTanks").Ignore(e => e.Rocket);
    }
}
