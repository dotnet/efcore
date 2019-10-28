// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class TableSplittingTestBase
    {
        protected TableSplittingTestBase(ITestOutputHelper testOutputHelper)
        {
            TestSqlLoggerFactory = (TestSqlLoggerFactory)TestStoreFactory.CreateListLoggerFactory(_ => true);
            //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public virtual void Can_update_just_dependents()
        {
            using (CreateTestStore(OnModelCreating))
            {
                Operator firstOperator;
                Engine firstEngine;
                using (var context = CreateContext())
                {
                    firstOperator = context.Set<Operator>().OrderBy(o => o.VehicleName).First();
                    firstOperator.Name += "1";
                    firstEngine = context.Set<Engine>().OrderBy(o => o.VehicleName).First();
                    firstEngine.Description += "1";

                    context.SaveChanges();

                    Assert.Empty(context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged));
                }

                using (var context = CreateContext())
                {
                    Assert.Equal(firstOperator.Name, context.Set<Operator>().OrderBy(o => o.VehicleName).First().Name);
                    Assert.Equal(firstEngine.Description, context.Set<Engine>().OrderBy(o => o.VehicleName).First().Description);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_shared()
        {
            using (CreateTestStore(OnModelCreating))
            {
                using (var context = CreateContext())
                {
                    Assert.Equal(5, context.Set<Operator>().ToList().Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_shared_nonhierarchy()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Ignore<LicensedOperator>();
                }))
            {
                using (var context = CreateContext())
                {
                    Assert.Equal(5, context.Set<Operator>().ToList().Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_shared_nonhierarchy_with_nonshared_dependent()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Ignore<LicensedOperator>();
                    modelBuilder.Entity<OperatorDetails>().ToTable("OperatorDetails");
                }))
            {
                using (var context = CreateContext())
                {
                    Assert.Equal(5, context.Set<Operator>().ToList().Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_shared_derived_hierarchy()
        {
            using (CreateTestStore(OnModelCreating))
            {
                using (var context = CreateContext())
                {
                    Assert.Equal(2, context.Set<FuelTank>().ToList().Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_shared_derived_nonhierarchy()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Ignore<SolidFuelTank>();
                }))
            {
                using (var context = CreateContext())
                {
                    Assert.Equal(2, context.Set<FuelTank>().ToList().Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_query_shared_derived_nonhierarchy_all_required()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Ignore<SolidFuelTank>();
                    modelBuilder.Entity<FuelTank>(
                        eb =>
                        {
                            eb.Property(t => t.Capacity).IsRequired();
                            eb.Property(t => t.FuelType).IsRequired();
                        });
                }))
            {
                using (var context = CreateContext())
                {
                    Assert.Equal(2, context.Set<FuelTank>().ToList().Count);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_use_with_redundant_relationships()
        {
            Test_roundtrip(OnModelCreating);
        }

        [ConditionalFact]
        public virtual void Can_use_with_chained_relationships()
        {
            Test_roundtrip(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<FuelTank>(eb => { eb.Ignore(e => e.Vehicle); });
                });
        }

        [ConditionalFact]
        public virtual void Can_use_with_fanned_relationships()
        {
            Test_roundtrip(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<FuelTank>(eb => eb.Ignore(e => e.Engine));
                });
        }

        [ConditionalFact]
        public virtual void Can_share_required_columns()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<Vehicle>(
                        vb =>
                        {
                            vb.Property(v => v.SeatingCapacity).HasColumnName("SeatingCapacity");
                        });
                    modelBuilder.Entity<Engine>(
                        cb =>
                        {
                            cb.Property<int>("SeatingCapacity").HasColumnName("SeatingCapacity");
                        });
                    modelBuilder.Entity<FuelTank>(
                        fb =>
                        {
                            fb.Ignore(f => f.Engine);
                        });
                }, seed: false))
            {
                using (var context = CreateContext())
                {
                    var scooterEntry = context.Add(
                        new PoweredVehicle
                        {
                            Name = "Electric scooter",
                            SeatingCapacity = 1,
                            Engine = new Engine()
                        });

                    scooterEntry.Reference(v => v.Engine).TargetEntry.Property<int>("SeatingCapacity").CurrentValue = 1;

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var scooter = context.Set<PoweredVehicle>().Include(v => v.Engine).Single(v => v.Name == "Electric scooter");

                    Assert.Equal(scooter.SeatingCapacity, context.Entry(scooter.Engine).Property<int>("SeatingCapacity").CurrentValue);
                }
            }
        }

        protected void Test_roundtrip(Action<ModelBuilder> onModelCreating)
        {
            using (CreateTestStore(onModelCreating))
            {
                using var context = CreateContext();
                context.AssertSeeded();
            }
        }

        [ConditionalFact]
        public virtual void Can_manipulate_entities_sharing_row_independently()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<FuelTank>(eb => eb.Ignore(e => e.Engine));
                }))
            {
                PoweredVehicle streetcar;
                using (var context = CreateContext())
                {
                    streetcar = context.Set<PoweredVehicle>().Include(v => v.Engine)
                        .Single(v => v.Name == "1984 California Car");

                    Assert.Null(streetcar.Engine);

                    streetcar.Engine = new Engine { Description = "Streetcar engine" };

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var streetcarFromStore = context.Set<PoweredVehicle>().Include(v => v.Engine).AsNoTracking()
                        .Single(v => v.Name == "1984 California Car");

                    Assert.Equal("Streetcar engine", streetcarFromStore.Engine.Description);

                    streetcarFromStore.Engine.Description = "Line";

                    context.Update(streetcarFromStore);
                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var streetcarFromStore = context.Set<PoweredVehicle>().Include(v => v.Engine)
                        .Single(v => v.Name == "1984 California Car");

                    Assert.Equal("Line", streetcarFromStore.Engine.Description);

                    streetcarFromStore.SeatingCapacity = 40;
                    streetcarFromStore.Engine.Description = "Streetcar engine";

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var streetcarFromStore = context.Set<PoweredVehicle>().Include(v => v.Engine).AsNoTracking()
                        .Single(v => v.Name == "1984 California Car");

                    Assert.Equal(40, streetcarFromStore.SeatingCapacity);
                    Assert.Equal("Streetcar engine", streetcarFromStore.Engine.Description);

                    context.Remove(streetcarFromStore.Engine);

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    var streetcarFromStore = context.Set<PoweredVehicle>().Include(v => v.Engine).AsNoTracking()
                        .Single(v => v.Name == "1984 California Car");

                    Assert.Null(streetcarFromStore.Engine);

                    context.Remove(streetcarFromStore);

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    Assert.Null(context.Set<PoweredVehicle>().AsNoTracking().SingleOrDefault(v => v.Name == "1984 California Car"));
                    Assert.Null(context.Set<Engine>().AsNoTracking().SingleOrDefault(e => e.VehicleName == "1984 California Car"));
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_insert_dependent_with_just_one_parent()
        {
            using (CreateTestStore(OnModelCreating))
            {
                using (var context = CreateContext())
                {
                    context.Add(
                        new PoweredVehicle
                        {
                            Name = "Fuel transport",
                            SeatingCapacity = 1,
                            Operator = new LicensedOperator { Name = "Jack Jackson", LicenseType = "Class A CDC" }
                        });
                    context.Add(
                        new FuelTank
                        {
                            Capacity = "10000 l",
                            FuelType = "Gas",
                            VehicleName = "Fuel transport"
                        });

                    context.SaveChanges();
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_change_dependent_instance_non_derived()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<Engine>().ToTable("Engines");
                    modelBuilder.Entity<FuelTank>(
                        eb =>
                        {
                            eb.ToTable("FuelTanks");
                            eb.HasOne(e => e.Engine)
                                .WithOne(e => e.FuelTank)
                                .HasForeignKey<FuelTank>(e => e.VehicleName)
                                .OnDelete(DeleteBehavior.Restrict);
                        });
                    modelBuilder.Ignore<SolidFuelTank>();
                    modelBuilder.Ignore<SolidRocket>();
                }))
            {
                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Include(v => v.Operator).Single(v => v.Name == "Trek Pro Fit Madone 6 Series");

                    bike.Operator = new Operator { Name = "Chris Horner" };

                    context.ChangeTracker.DetectChanges();

                    bike.Operator = new LicensedOperator { Name = "repairman", LicenseType = "Repair" };

                    TestSqlLoggerFactory.Clear();
                    context.SaveChanges();

                    Assert.Empty(context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged));
                }

                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Include(v => v.Operator).Single(v => v.Name == "Trek Pro Fit Madone 6 Series");
                    Assert.Equal("repairman", bike.Operator.Name);
                    Assert.Equal("Repair", ((LicensedOperator)bike.Operator).LicenseType);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_change_principal_instance_non_derived()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<Engine>().ToTable("Engines");
                    modelBuilder.Entity<FuelTank>(
                        eb =>
                        {
                            eb.ToTable("FuelTanks");
                            eb.HasOne(e => e.Engine)
                                .WithOne(e => e.FuelTank)
                                .HasForeignKey<FuelTank>(e => e.VehicleName)
                                .OnDelete(DeleteBehavior.Restrict);
                        });
                    modelBuilder.Ignore<SolidFuelTank>();
                    modelBuilder.Ignore<SolidRocket>();
                }))
            {
                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Single(v => v.Name == "Trek Pro Fit Madone 6 Series");

                    var newBike = new Vehicle
                    {
                        Name = "Trek Pro Fit Madone 6 Series",
                        Operator = bike.Operator,
                        SeatingCapacity = 2
                    };

                    context.Remove(bike);
                    context.Add(newBike);

                    TestSqlLoggerFactory.Clear();
                    context.SaveChanges();

                    Assert.Empty(context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged));
                }

                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Include(v => v.Operator).Single(v => v.Name == "Trek Pro Fit Madone 6 Series");

                    Assert.Equal(2, bike.SeatingCapacity);
                    Assert.NotNull(bike.Operator);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_change_principal_and_dependent_instance_non_derived()
        {
            using (CreateTestStore(
                modelBuilder =>
                {
                    OnModelCreating(modelBuilder);
                    modelBuilder.Entity<Engine>().ToTable("Engines");
                    modelBuilder.Entity<FuelTank>(
                        eb =>
                        {
                            eb.ToTable("FuelTanks");
                            eb.HasOne(e => e.Engine)
                                .WithOne(e => e.FuelTank)
                                .HasForeignKey<FuelTank>(e => e.VehicleName)
                                .OnDelete(DeleteBehavior.Restrict);
                        });
                    modelBuilder.Ignore<SolidFuelTank>();
                    modelBuilder.Ignore<SolidRocket>();
                }))
            {
                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Include(v => v.Operator).Single(v => v.Name == "Trek Pro Fit Madone 6 Series");

                    var newBike = new Vehicle
                    {
                        Name = "Trek Pro Fit Madone 6 Series",
                        Operator = new LicensedOperator { Name = "repairman", LicenseType = "Repair" },
                        SeatingCapacity = 2
                    };

                    context.Remove(bike);
                    context.Add(newBike);

                    TestSqlLoggerFactory.Clear();
                    context.SaveChanges();

                    Assert.Empty(context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged));
                }

                using (var context = CreateContext())
                {
                    var bike = context.Vehicles.Include(v => v.Operator).Single(v => v.Name == "Trek Pro Fit Madone 6 Series");
                    Assert.Equal(2, bike.SeatingCapacity);
                    Assert.Equal("repairman", bike.Operator.Name);
                    Assert.Equal("Repair", ((LicensedOperator)bike.Operator).LicenseType);
                }
            }
        }

        protected readonly string DatabaseName = "TableSplittingTest";
        protected TestStore TestStore { get; set; }
        protected abstract ITestStoreFactory TestStoreFactory { get; }
        protected IServiceProvider ServiceProvider { get; set; }
        protected TestSqlLoggerFactory TestSqlLoggerFactory { get; }

        protected void AssertSql(params string[] expected)
            => TestSqlLoggerFactory.AssertBaseline(expected);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>(
                eb =>
                {
                    eb.HasDiscriminator<string>("Discriminator");
                    eb.Property<string>("Discriminator").HasColumnName("Discriminator");
                    eb.ToTable("Vehicles");
                });

            modelBuilder.Entity<Engine>().ToTable("Vehicles");
            modelBuilder.Entity<Operator>().ToTable("Vehicles");
            modelBuilder.Entity<OperatorDetails>().ToTable("Vehicles");
            modelBuilder.Entity<FuelTank>().ToTable("Vehicles");
        }

        protected TestStore CreateTestStore(Action<ModelBuilder> onModelCreating, bool seed = true)
        {
            TestStore = TestStoreFactory.Create(DatabaseName);

            ServiceProvider = TestStoreFactory.AddProviderServices(new ServiceCollection())
                .AddSingleton(TestModelSource.GetFactory(onModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);

            TestStore.Initialize(
                ServiceProvider, CreateContext, c =>
                {
                    if (seed)
                    {
                        ((TransportationContext)c).Seed();
                    }
                });

            TestSqlLoggerFactory.Clear();

            return TestStore;
        }

        protected virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(
                    b => b.Default(WarningBehavior.Throw)
                        .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                        .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));

        protected virtual TransportationContext CreateContext()
        {
            var options = AddOptions(TestStore.AddProviderOptions(new DbContextOptionsBuilder()))
                .UseInternalServiceProvider(ServiceProvider).Options;
            return new TransportationContext(options);
        }
    }
}
