// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable RCS1202 // Avoid NullReferenceException.

namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

#nullable disable

public class TransportationContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Vehicle> Vehicles { get; set; }

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
                eb.HasOne(e => e.Details)
                    .WithOne()
                    .HasForeignKey<OperatorDetails>(e => e.VehicleName);
            });
        modelBuilder.Entity<LicensedOperator>();

        modelBuilder.Entity<Vehicle>(
            vb =>
            {
                vb.Navigation(v => v.Operator).IsRequired();
            });

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

        modelBuilder.Entity<SolidFuelTank>(
            eb =>
            {
                eb.HasOne(e => e.Rocket)
                    .WithOne(e => e.SolidFuelTank)
                    .HasForeignKey<SolidFuelTank>(e => e.VehicleName);
            });

        modelBuilder.Entity<OperatorDetails>(
            eb =>
            {
                eb.HasKey(e => e.VehicleName);
            });
    }

    public Task SeedAsync()
    {
        Vehicles.AddRange(CreateVehicles());
        return SaveChangesAsync();
    }

    public void AssertSeeded()
    {
        var expected = CreateVehicles().OrderBy(v => v.Name).ToList();
        var actual = Vehicles
            .Include(v => v.Operator)
            .ThenInclude(v => v.Details)
            .Include(v => ((PoweredVehicle)v).Engine)
            .ThenInclude(e => (e as CombustionEngine).FuelTank)
            .OrderBy(v => v.Name).ToList();

        Assert.Equal(expected, actual);
    }

    protected IEnumerable<Vehicle> CreateVehicles()
        => new List<Vehicle>
        {
            new()
            {
                Name = "Trek Pro Fit Madone 6 Series",
                SeatingCapacity = 1,
                Operator = new Operator { Name = "Lance Armstrong", VehicleName = "Trek Pro Fit Madone 6 Series" }
            },
            new PoweredVehicle
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
                Engine =
                    new Engine
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
                Engine =
                    new ContinuousCombustionEngine
                    {
                        Description = "Reaction Motors XLR99 throttleable, restartable liquid-propellant rocket engine",
                        FuelTank = new FuelTank
                        {
                            FuelType = "Liquid oxygen and anhydrous ammonia",
                            Capacity = 11250,
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
                        Capacity = 22,
                        GrainGeometry = "Cylindrical",
                        VehicleName = "AIM-9M Sidewinder"
                    },
                    VehicleName = "AIM-9M Sidewinder"
                },
                Operator = new Operator
                {
                    Details = new OperatorDetails { Type = "Heat-seeking", VehicleName = "AIM-9M Sidewinder" },
                    VehicleName = "AIM-9M Sidewinder"
                }
            }
        };
}
