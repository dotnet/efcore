// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class TransportationContext : PoolableDbContext
    {
        public TransportationContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Operator> Operators { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>(eb => eb.HasKey(e => e.Name));
            modelBuilder.Entity<Engine>(
                eb =>
                {
                    eb.HasKey(e => e.VehicleName);
                    eb.HasOne(e => e.Vehicle)
                        .WithOne(e => e.Engine)
                        .HasForeignKey<Engine>(e => e.VehicleName);
                });

            modelBuilder.Entity<ContinuousCombustionEngine>();
            modelBuilder.Entity<IntermittentCombustionEngine>();
            modelBuilder.Entity<SolidRocket>();

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
                    // Make dependent optional
                    // #9005
                    //eb.HasOne(e => e.Vehicle)
                    //    .WithOne()
                    //    .HasForeignKey<FuelTank>(e => e.VehicleName);
                    eb.Ignore(e => e.Vehicle);
                });

            modelBuilder.Entity<SolidFuelTank>(
                eb =>
                {
                    eb.HasOne(e => e.Rocket)
                        .WithOne(e => e.SolidFuelTank)
                        .HasForeignKey<SolidFuelTank>(e => e.VehicleName);
                });
        }

        public void Seed()
        {
            Vehicles.AddRange(CreateVehicles());
            SaveChanges();
        }

        public void AssertSeeded()
        {
            var expected = CreateVehicles().OrderBy(v => v.Name).ToList();
            var actual = Vehicles
                            .Include(v => v.Operator)
                            .Include(v => ((PoweredVehicle)v).Engine)
                            .ThenInclude(e => (e as CombustionEngine).FuelTank)
                            .OrderBy(v => v.Name).ToList();
            Assert.Equal(expected, actual);
        }

        protected IEnumerable<Vehicle> CreateVehicles()
            => new List<Vehicle>
            {
                new Vehicle
                {
                    Name = "Trek Pro Fit Madone 6 Series",
                    SeatingCapacity = 1,
                    Operator = new Operator
                    {
                        Name = "Lance Armstrong",
                        VehicleName = "Trek Pro Fit Madone 6 Series"
                    }
                },
                // This should be a PoweredVehicle when Engine is made optional
                // #9005
                new Vehicle
                {
                    Name = "1984 California Car",
                    SeatingCapacity = 34,
                    Operator = new LicensedOperator
                    {
                        Name = "Albert Williams",
                        LicenseType = "Muni Transit",
                        VehicleName = "1984 California Car"
                    }
                },
                new PoweredVehicle
                {
                    Name = "P85 2012 Tesla Model S Performance Edition",
                    SeatingCapacity = 5,
                    Engine = new Engine
                    {
                        Description = "416 hp three phase, four pole AC induction",
                        VehicleName = "P85 2012 Tesla Model S Performance Edition"
                    },
                    Operator = new LicensedOperator
                    {
                        Name = "Elon Musk",
                        LicenseType = "Driver",
                        VehicleName = "P85 2012 Tesla Model S Performance Edition"
                    }
                },
                new PoweredVehicle
                {
                    Name = "North American X-15A-2",
                    SeatingCapacity = 1,
                    Engine = new ContinuousCombustionEngine
                    {
                        Description = "Reaction Motors XLR99 throttleable, restartable liquid-propellant rocket engine",
                        FuelTank = new FuelTank
                        {
                            FuelType = "Liquid oxygen and anhydrous ammonia",
                            Capacity = "11250 kg",
                            VehicleName = "North American X-15A-2"
                        },
                        VehicleName = "North American X-15A-2"
                    },
                    Operator = new LicensedOperator
                    {
                        Name = "William J. Knight",
                        LicenseType = "Air Force Test Pilot",
                        VehicleName = "North American X-15A-2"
                    }
                },
                new PoweredVehicle
                {
                    Name = "AIM-9M Sidewinder",
                    Engine = new SolidRocket
                    {
                        Description = "Hercules/Bermite MK 36 Solid-fuel rocket",
                        FuelTank = new SolidFuelTank
                        {
                            FuelType = "Reduced smoke Hydroxyl-Terminated Polybutadiene",
                            Capacity = "22 kg",
                            GrainGeometry = "Cylindrical",
                            VehicleName = "AIM-9M Sidewinder"
                        },
                        VehicleName = "AIM-9M Sidewinder"
                    },
                    // This should be null
                    // #9005
                    Operator = new Operator
                    {
                        Name = "Infrared homing guidance",
                        VehicleName = "AIM-9M Sidewinder"
                    }
                }
            };
    }
}
