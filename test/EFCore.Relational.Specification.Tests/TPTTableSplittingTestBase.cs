// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class TPTTableSplittingTestBase : TableSplittingTestBase
    {
        protected TPTTableSplittingTestBase(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        public override void Can_use_optional_dependents_with_shared_concurrency_tokens()
        {
            // TODO: Issue #22060
        }

        protected override string DatabaseName { get; } = "TPTTableSplittingTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>().ToTable("Vehicles");
            modelBuilder.Entity<PoweredVehicle>().ToTable("PoweredVehicles");

            modelBuilder.Entity<Operator>().ToTable("Vehicles");
            modelBuilder.Entity<LicensedOperator>().ToTable("LicensedOperators");

            modelBuilder.Entity<OperatorDetails>().ToTable("Vehicles");

            modelBuilder.Entity<Engine>().ToTable("PoweredVehicles")
                .HasOne(e => e.Vehicle).WithOne(e => e.Engine).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<CombustionEngine>().ToTable("CombustionEngines");
            modelBuilder.Entity<IntermittentCombustionEngine>().ToTable("IntermittentCombustionEngines");
            modelBuilder.Entity<ContinuousCombustionEngine>().ToTable("ContinuousCombustionEngines");
            modelBuilder.Entity<SolidRocket>().ToTable("SolidRockets").Ignore(e => e.SolidFuelTank);

            modelBuilder.Entity<FuelTank>().ToTable("CombustionEngines")
                .HasOne(e => e.Vehicle).WithOne().OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<SolidFuelTank>().ToTable("SolidFuelTanks").Ignore(e => e.Rocket);
        }
    }
}
