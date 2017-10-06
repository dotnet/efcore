// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class TransportationContext : DbContext
    {
        public TransportationContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Operator> Operators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>(eb => { eb.HasKey(e => e.Name); });
            modelBuilder.Entity<Engine>(
                eb =>
                    {
                        eb.HasKey(e => e.VehicleName);
                        eb.HasOne(e => e.Vehicle)
                            .WithOne(e => e.Engine)
                            .HasForeignKey<Engine>(e => e.VehicleName);
                    });
            modelBuilder.Entity<CombustionEngine>();

            modelBuilder.Entity<Operator>(
                eb =>
                    {
                        eb.HasKey(e => e.VehicleName);
                        eb.HasOne(e => e.Vehicle)
                            .WithOne(e => e.Operator)
                            .HasForeignKey<Operator>(e => e.VehicleName);
                    });
            modelBuilder.Entity<LicensedOperator>();

            modelBuilder.Entity<FuelTank>(
                eb =>
                    {
                        eb.HasKey(e => e.VehicleName);
                        eb.HasOne(e => e.Engine)
                            .WithOne(e => e.FuelTank)
                            .HasForeignKey<FuelTank>(e => e.VehicleName);
                        eb.HasOne(e => e.Vehicle)
                            .WithOne()
                            .HasForeignKey<FuelTank>(e => e.VehicleName);
                    });
        }

        public void Seed()
        {
            Vehicles.AddRange(CreateVehicles());
            SaveChanges();
        }

        public void AssertSeeded()
        {
            Assert.Equal(CreateVehicles().OrderBy(v => v.Name).ToList(), Load(Vehicles).OrderBy(v => v.Name).ToList());
        }

        // TODO: Instead use derived includes when available
        public DbSet<Vehicle> Load(DbSet<Vehicle> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                Load(vehicle);
            }

            return vehicles;
        }

        private void Load(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                switch (vehicle)
                {
                    case PoweredVehicle v:
                        Entry(v).Reference(e => e.Engine).Load();
                        Load(v.Engine);
                        goto default;
                    default:
                        Entry(vehicle).Reference(e => e.Operator).Load();
                        break;
                }
            }
        }

        private void Load(Engine engine)
        {
            if (engine != null)
            {
                switch (engine)
                {
                    case CombustionEngine en:
                        en.FuelTank = Set<FuelTank>().SingleOrDefault(f => f.VehicleName == en.VehicleName);
                        break;
                }
            }
        }

        protected IEnumerable<Vehicle> CreateVehicles()
            => new List<Vehicle>
            {
                new Vehicle
                {
                    Name = "Trek Pro Fit Madone 6 Series",
                    SeatingCapacity = 1,
                    Operator = new Operator { Name = "Lance Armstrong", VehicleName = "Trek Pro Fit Madone 6 Series" }
                },
                new PoweredVehicle
                {
                    Name = "1984 California Car",
                    SeatingCapacity = 34,
                    Operator = new LicensedOperator { Name = "Albert Williams", LicenseType = "Muni Transit", VehicleName = "1984 California Car" }
                },
                new PoweredVehicle
                {
                    Name = "P85 2012 Tesla Model S Performance Edition",
                    SeatingCapacity = 5,
                    Engine = new Engine { Description = "416 hp three phase, four pole AC induction", VehicleName = "P85 2012 Tesla Model S Performance Edition" },
                    Operator = new LicensedOperator { Name = "Elon Musk", LicenseType = "Driver", VehicleName = "P85 2012 Tesla Model S Performance Edition" }
                },
                new PoweredVehicle
                {
                    Name = "North American X-15A-2",
                    SeatingCapacity = 1,
                    Engine = new CombustionEngine
                    {
                        Description = "Reaction Motors XLR99 throttleable, restartable liquid-propellant rocket engine",
                        FuelTank = new FuelTank { FuelType = "Liquid oxygen and anhydrous ammonia", Capacity = "11250 kg", VehicleName = "North American X-15A-2" },
                        VehicleName = "North American X-15A-2"
                    },
                    Operator = new LicensedOperator { Name = "William J. Knight", LicenseType = "Air Force Test Pilot", VehicleName = "North American X-15A-2" }
                }
            };
    }
}
