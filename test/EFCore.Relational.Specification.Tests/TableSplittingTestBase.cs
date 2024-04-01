// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class TableSplittingTestBase : NonSharedModelTestBase
{
    protected TableSplittingTestBase(ITestOutputHelper testOutputHelper)
    {
        // TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual async Task Can_query_shared()
    {
        await InitializeAsync(OnModelCreating);

        using var context = CreateContext();
        Assert.Equal(5, context.Set<Operator>().ToList().Count);
    }

    [ConditionalFact]
    public virtual async Task Can_query_shared_nonhierarchy()
    {
        await InitializeAsync(
            modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                modelBuilder.Ignore<LicensedOperator>();
            });

        using var context = CreateContext();
        Assert.Equal(5, context.Set<Operator>().ToList().Count);
    }

    [ConditionalFact]
    public virtual async Task Can_query_shared_nonhierarchy_with_nonshared_dependent()
    {
        await InitializeAsync(
            modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                modelBuilder.Ignore<LicensedOperator>();
                modelBuilder.Entity<OperatorDetails>().ToTable("OperatorDetails");
            });

        using var context = CreateContext();
        Assert.Equal(5, context.Set<Operator>().ToList().Count);
    }

    [ConditionalFact]
    public virtual async Task Can_query_shared_derived_hierarchy()
    {
        await InitializeAsync(OnModelCreating);

        using var context = CreateContext();
        Assert.Equal(2, context.Set<FuelTank>().ToList().Count);
    }

    [ConditionalFact]
    public virtual async Task Can_query_shared_derived_nonhierarchy()
    {
        await InitializeAsync(
            modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                modelBuilder.Ignore<SolidFuelTank>();
            });

        using var context = CreateContext();
        Assert.Equal(2, context.Set<FuelTank>().ToList().Count);
    }

    [ConditionalFact]
    public virtual async Task Can_query_shared_derived_nonhierarchy_all_required()
    {
        await InitializeAsync(
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
            });

        using var context = CreateContext();
        Assert.Equal(2, context.Set<FuelTank>().ToList().Count);
    }

    [ConditionalFact]
    public virtual Task Can_use_with_redundant_relationships()
        => Test_roundtrip(OnModelCreating);

    [ConditionalFact]
    public virtual Task Can_use_with_chained_relationships()
        => Test_roundtrip(
            modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                modelBuilder.Entity<FuelTank>(eb => { eb.Ignore(e => e.Vehicle); });
            });

    [ConditionalFact]
    public virtual Task Can_use_with_fanned_relationships()
        => Test_roundtrip(
            modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                modelBuilder.Entity<CombustionEngine>().HasOne(e => e.FuelTank).WithOne().HasForeignKey<FuelTank>(e => e.VehicleName);
                modelBuilder.Entity<FuelTank>(eb => eb.Ignore(e => e.Engine));
            });

    [ConditionalFact]
    public virtual async Task Can_share_required_columns()
    {
        await InitializeAsync(
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
                modelBuilder.Entity<CombustionEngine>().HasOne(e => e.FuelTank).WithOne().HasForeignKey<FuelTank>(e => e.VehicleName);
                modelBuilder.Entity<FuelTank>().Ignore(f => f.Engine);
            }, seed: false);

        using (var context = CreateContext())
        {
            var scooterEntry = await context.AddAsync(
                new PoweredVehicle
                {
                    Name = "Electric scooter",
                    SeatingCapacity = 1,
                    Engine = new Engine(),
                    Operator = new Operator { Name = "Kai Saunders" }
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

    [ConditionalFact]
    public virtual async Task Can_share_required_columns_with_complex_types()
    {
        await InitializeAsync(
            modelBuilder =>
            {
                OnModelCreatingComplex(modelBuilder);
                modelBuilder.Entity<Vehicle>(
                    vb =>
                    {
                        vb.Property(v => v.SeatingCapacity).HasColumnName("SeatingCapacity");
                    });
                modelBuilder.Entity<PoweredVehicle>(
                    vb =>
                    {
                        vb.ComplexProperty(
                            v => v.Engine, eb =>
                            {
                                eb.Property<int>("SeatingCapacity").HasColumnName("SeatingCapacity");
                            });
                    });
            }, seed: false);

        using (var context = CreateContext())
        {
            var scooterEntry = await context.AddAsync(
                new PoweredVehicle
                {
                    Name = "Electric scooter",
                    SeatingCapacity = 1,
                    Engine = new Engine(),
                    Operator = new Operator { Name = "Kai Saunders", Details = new OperatorDetails() }
                });

            context.SaveChanges();

            //Assert.Equal(scooter.SeatingCapacity, scooterEntry.ComplexProperty(v => v.Engine).TargetEntry.Property<int>("SeatingCapacity").CurrentValue);
        }

        //using (var context = CreateContext())
        //{
        //    var scooter = context.Set<PoweredVehicle>().Single(v => v.Name == "Electric scooter");

        //    Assert.Equal(scooter.SeatingCapacity, context.Entry(scooter).ComplexProperty(v => v.Engine).TargetEntry.Property<int>("SeatingCapacity").CurrentValue);
        //}
    }

    [ConditionalFact]
    public virtual async Task Can_use_optional_dependents_with_shared_concurrency_tokens()
    {
        await InitializeAsync(
            modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                modelBuilder.Entity<Vehicle>(
                    vb =>
                    {
                        vb.Property(v => v.SeatingCapacity).HasColumnName("SeatingCapacity").IsConcurrencyToken();
                    });
                modelBuilder.Entity<Engine>(
                    cb =>
                    {
                        cb.Property<int>("SeatingCapacity").HasColumnName("SeatingCapacity").IsConcurrencyToken();
                    });
                modelBuilder.Entity<CombustionEngine>().HasOne(e => e.FuelTank).WithOne().HasForeignKey<FuelTank>(e => e.VehicleName);
                modelBuilder.Entity<FuelTank>().Ignore(f => f.Engine);
            }, seed: false);

        using (var context = CreateContext())
        {
            var scooterEntry = await context.AddAsync(
                new PoweredVehicle
                {
                    Name = "Electric scooter",
                    SeatingCapacity = 1,
                    Operator = new Operator { Name = "Kai Saunders" }
                });

            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var scooter = context.Set<PoweredVehicle>().Include(v => v.Engine).Single(v => v.Name == "Electric scooter");

            Assert.Equal(1, scooter.SeatingCapacity);

            scooter.Engine = new Engine();

            var engineCapacityEntry = context.Entry(scooter.Engine).Property<int>("SeatingCapacity");

            Assert.Equal(0, engineCapacityEntry.OriginalValue);

            context.SaveChanges();

            Assert.Equal(0, engineCapacityEntry.OriginalValue);
            Assert.Equal(0, engineCapacityEntry.CurrentValue);
        }

        using (var context = CreateContext())
        {
            var scooter = context.Set<PoweredVehicle>().Include(v => v.Engine).Single(v => v.Name == "Electric scooter");

            Assert.Equal(scooter.SeatingCapacity, context.Entry(scooter.Engine).Property<int>("SeatingCapacity").CurrentValue);

            scooter.SeatingCapacity = 2;
            context.SaveChanges();
        }

        using (var context = CreateContext())
        {
            var scooter = context.Set<PoweredVehicle>().Include(v => v.Engine).Single(v => v.Name == "Electric scooter");

            Assert.Equal(2, scooter.SeatingCapacity);
            Assert.Equal(2, context.Entry(scooter.Engine).Property<int>("SeatingCapacity").CurrentValue);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_use_optional_dependents_with_shared_concurrency_tokens_with_complex_types()
    {
        await InitializeAsync(
            modelBuilder =>
            {
                OnModelCreatingComplex(modelBuilder);
                modelBuilder.Entity<Vehicle>(
                    vb =>
                    {
                        vb.Property(v => v.SeatingCapacity).HasColumnName("SeatingCapacity").IsConcurrencyToken();
                    });
                modelBuilder.Entity<PoweredVehicle>(
                    vb =>
                    {
                        vb.ComplexProperty(
                            v => v.Engine, eb =>
                            {
                                eb.Property<int>("SeatingCapacity").HasColumnName("SeatingCapacity").IsConcurrencyToken();
                            });
                    });
            }, seed: false);

        using (var context = CreateContext())
        {
            var scooterEntry = await context.AddAsync(
                new PoweredVehicle
                {
                    Name = "Electric scooter",
                    SeatingCapacity = 1,
                    Engine = new Engine(),
                    Operator = new Operator { Name = "Kai Saunders", Details = new OperatorDetails() }
                });

            context.SaveChanges();
        }

        //using (var context = CreateContext())
        //{
        //    var scooter = context.Set<PoweredVehicle>().Single(v => v.Name == "Electric scooter");

        //    Assert.Equal(1, scooter.SeatingCapacity);

        //    scooter.Engine = new Engine();

        //    var engineCapacityEntry = context.Entry(scooter).ComplexProperty(v => v.Engine).TargetEntry.Property<int>("SeatingCapacity");

        //    Assert.Equal(0, engineCapacityEntry.OriginalValue);

        //    context.SaveChanges();

        //    Assert.Equal(0, engineCapacityEntry.OriginalValue);
        //    Assert.Equal(0, engineCapacityEntry.CurrentValue);
        //}

        //using (var context = CreateContext())
        //{
        //    var scooter = context.Set<PoweredVehicle>().Single(v => v.Name == "Electric scooter");

        //    Assert.Equal(scooter.SeatingCapacity, context.Entry(scooter).ComplexProperty(v => v.Engine).TargetEntry.Property<int>("SeatingCapacity").CurrentValue);

        //    scooter.SeatingCapacity = 2;
        //    context.SaveChanges();
        //}

        //using (var context = CreateContext())
        //{
        //    var scooter = context.Set<PoweredVehicle>().Include(v => v.Engine).Single(v => v.Name == "Electric scooter");

        //    Assert.Equal(2, scooter.SeatingCapacity);
        //    Assert.Equal(2, context.Entry(scooter).ComplexProperty(v => v.Engine).TargetEntry.Property<int>("SeatingCapacity").CurrentValue);
        //}
    }

    protected async Task Test_roundtrip(Action<ModelBuilder> onModelCreating)
    {
        await InitializeAsync(onModelCreating);

        using var context = CreateContext();
        context.AssertSeeded();
    }

    [ConditionalFact]
    public virtual async Task Can_manipulate_entities_sharing_row_independently()
    {
        await InitializeAsync(
            modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                modelBuilder.Entity<CombustionEngine>().HasOne(e => e.FuelTank).WithOne().HasForeignKey<FuelTank>(e => e.VehicleName);
                modelBuilder.Entity<FuelTank>(eb => eb.Ignore(e => e.Engine));
            });

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
            var streetcarFromStore = context.Set<PoweredVehicle>().AsNoTracking()
                .Include(v => v.Engine).Include(v => v.Operator)
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

    [ConditionalFact]
    public virtual async Task Can_update_just_dependents()
    {
        await InitializeAsync(OnModelCreating);

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

    [ConditionalFact]
    public virtual async Task Can_insert_dependent_with_just_one_parent()
    {
        await InitializeAsync(OnModelCreating);

        using (var context = CreateContext())
        {
            await context.AddAsync(
                new PoweredVehicle
                {
                    Name = "Fuel transport",
                    SeatingCapacity = 1,
                    Operator = new LicensedOperator { Name = "Jack Jackson", LicenseType = "Class A CDC" }
                });
            await context.AddAsync(
                new FuelTank
                {
                    Capacity = 10000_1,
                    FuelType = "Gas",
                    VehicleName = "Fuel transport"
                });

            context.SaveChanges();

            var savedEntries = context.ChangeTracker.Entries().ToList();
            Assert.Equal(3, savedEntries.Count);
            Assert.All(savedEntries, e => Assert.Equal(EntityState.Unchanged, e.State));
        }

        using (var context = CreateContext())
        {
            var transport = context.Vehicles.Include(v => v.Operator)
                .Single(v => v.Name == "Fuel transport");
            var tank = context.Set<FuelTank>().Include(v => v.Vehicle)
                .Single(v => v.VehicleName == "Fuel transport");
            Assert.NotNull(transport.Operator.Name);
            Assert.Null(tank.Engine);
            Assert.Same(transport, tank.Vehicle);
        }
    }

    [ConditionalFact]
    public virtual async Task Can_change_dependent_instance_non_derived()
    {
        await InitializeAsync(
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
            });

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

    [ConditionalFact]
    public virtual async Task Can_change_principal_instance_non_derived()
    {
        await InitializeAsync(
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
            });

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
            await context.AddAsync(newBike);

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

    [ConditionalFact]
    public virtual async Task Can_change_principal_and_dependent_instance_non_derived()
    {
        await InitializeAsync(
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
            });

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
            await context.AddAsync(newBike);

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

    [ConditionalFact]
    public virtual async Task Optional_dependent_materialized_when_no_properties()
    {
        await InitializeAsync(OnModelCreating);

        using (var context = CreateContext())
        {
            var vehicle = context.Set<Vehicle>()
                .Where(e => e.Name == "AIM-9M Sidewinder")
                .OrderBy(e => e.Name)
                .Include(e => e.Operator.Details).First();
            Assert.Equal(0, vehicle.SeatingCapacity);
            Assert.Equal("Heat-seeking", vehicle.Operator.Details.Type);
            Assert.Null(vehicle.Operator.Name);
        }
    }

    [ConditionalFact]
    public virtual async Task Warn_when_save_optional_dependent_with_null_values_sensitive()
    {
        await InitializeSharedAsync(OnSharedModelCreating);

        var meterReading = new MeterReading { MeterReadingDetails = new MeterReadingDetail() };

        using var context = CreateSharedContext();
        await context.AddAsync(meterReading);

        context.SaveChanges();

        var expected = RelationalResources
            .LogOptionalDependentWithAllNullPropertiesSensitive(new TestLogger<TestRelationalLoggingDefinitions>())
            .GenerateMessage(nameof(MeterReadingDetail), "{Id: -2147482647}");

        var log = TestSqlLoggerFactory.Log.Single(l => l.Level == LogLevel.Warning);

        Assert.Equal(expected, log.Message);
    }

    [ConditionalFact]
    public virtual async Task Warn_when_save_optional_dependent_with_null_values()
    {
        await InitializeSharedAsync(OnSharedModelCreating, sensitiveLogEnabled: false);

        var meterReading = new MeterReading { MeterReadingDetails = new MeterReadingDetail() };

        using var context = CreateSharedContext();
        await context.AddAsync(meterReading);

        TestSqlLoggerFactory.Clear();

        context.SaveChanges();

        var expected = RelationalResources.LogOptionalDependentWithAllNullProperties(new TestLogger<TestRelationalLoggingDefinitions>())
            .GenerateMessage(nameof(MeterReadingDetail));

        var log = TestSqlLoggerFactory.Log.Single(l => l.Level == LogLevel.Warning);

        Assert.Equal(expected, log.Message);

        TestSqlLoggerFactory.Clear();

        meterReading.MeterReadingDetails = new MeterReadingDetail { CurrentRead = "100" };

        context.SaveChanges();

        Assert.Empty(TestSqlLoggerFactory.Log.Where(l => l.Level == LogLevel.Warning));

        meterReading.MeterReadingDetails = new MeterReadingDetail();

        context.SaveChanges();

        log = TestSqlLoggerFactory.Log.Single(l => l.Level == LogLevel.Warning);

        Assert.Equal(expected, log.Message);
    }

    [ConditionalFact]
    public virtual async Task No_warn_when_save_optional_dependent_at_least_one_none_null()
    {
        await InitializeSharedAsync(OnSharedModelCreating, sensitiveLogEnabled: false);

        using var context = CreateSharedContext();

        var meterReading = new MeterReading { MeterReadingDetails = new MeterReadingDetail { CurrentRead = "100" } };

        await context.AddAsync(meterReading);

        TestSqlLoggerFactory.Clear();

        context.SaveChanges();

        meterReading.MeterReadingDetails = new MeterReadingDetail { CurrentRead = "100" };

        context.SaveChanges();

        Assert.Empty(TestSqlLoggerFactory.Log.Where(l => l.Level == LogLevel.Warning));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task ExecuteDelete_throws_for_table_sharing(bool async)
    {
        await InitializeAsync(OnModelCreating);

        await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context => Assert.Contains(
                RelationalStrings.NonQueryTranslationFailedWithDetails(
                    "", RelationalStrings.ExecuteDeleteOnTableSplitting("Vehicles"))[21..],
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        if (async)
                        {
                            await context.Set<Vehicle>().ExecuteDeleteAsync();
                        }
                        else
                        {
                            context.Set<Vehicle>().ExecuteDelete();
                        }
                    })).Message));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task ExecuteUpdate_works_for_table_sharing(bool async)
    {
        await InitializeAsync(OnModelCreating);

        await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                if (async)
                {
                    await context.Set<Vehicle>().ExecuteUpdateAsync(s => s.SetProperty(e => e.SeatingCapacity, 1));
                }
                else
                {
                    context.Set<Vehicle>().ExecuteUpdate(s => s.SetProperty(e => e.SeatingCapacity, 1));
                }
            }, async context =>
            {
                Assert.True(
                    async
                        ? await context.Set<Vehicle>().AllAsync(e => e.SeatingCapacity == 1)
                        : context.Set<Vehicle>().All(e => e.SeatingCapacity == 1));
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Optional_dependent_without_required_property(bool async)
    {
        var contextFactory = await InitializeAsync<Context29196>(
            onConfiguring: e => e.ConfigureWarnings(w => w.Log(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning)));

        using (var context = contextFactory.CreateContext())
        {
            var query = context.DetailedOrders.Where(o => o.Status == OrderStatus.Pending);

            var result = async
                ? await query.ToListAsync()
                : query.ToList();
        }
    }

    protected class Context29196(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Order> Orders
            => Set<Order>();

        public DbSet<DetailedOrder> DetailedOrders
            => Set<DetailedOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DetailedOrder>(
                dob =>
                {
                    dob.ToTable("Orders");
                    dob.Property(o => o.Status).HasColumnName("Status");
                    dob.Property(o => o.Version).IsRowVersion().HasColumnName("Version");
                });

            modelBuilder.Entity<Order>(
                ob =>
                {
                    ob.ToTable("Orders");
                    ob.Property(o => o.Status).HasColumnName("Status");
                    ob.HasOne(o => o.DetailedOrder).WithOne().HasForeignKey<DetailedOrder>(o => o.Id);
                    ob.Property<byte[]>("Version").IsRowVersion().HasColumnName("Version");
                });
        }

        public Task SeedAsync()
        {
            Add(
                new Order
                {
                    Status = OrderStatus.Pending,
                    DetailedOrder = new DetailedOrder
                    {
                        Status = OrderStatus.Pending,
                        ShippingAddress = "221 B Baker St, London",
                        BillingAddress = "11 Wall Street, New York"
                    }
                });

            return SaveChangesAsync();
        }
    }

    public class DetailedOrder
    {
        public int Id { get; set; }
        public OrderStatus? Status { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public byte[] Version { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public OrderStatus? Status { get; set; }
        public DetailedOrder DetailedOrder { get; set; }
    }

    public enum OrderStatus
    {
        Pending,
        Shipped
    }

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override string StoreName
        => "TableSplittingTest";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected ContextFactory<TransportationContext> ContextFactory { get; private set; }
    protected ContextFactory<SharedTableContext> SharedContextFactory { get; private set; }

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
        modelBuilder.Entity<CompositeVehicle>();

        modelBuilder.Entity<Engine>().ToTable("Vehicles")
            .Property(e => e.Computed).ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<Operator>().ToTable("Vehicles");
        modelBuilder.Entity<OperatorDetails>().ToTable("Vehicles");
        modelBuilder.Entity<FuelTank>().ToTable("Vehicles");
    }

    protected virtual void OnModelCreatingComplex(ModelBuilder modelBuilder)
    {
        OnModelCreating(modelBuilder);
        modelBuilder.Ignore<Engine>();
        modelBuilder.Ignore<CombustionEngine>();
        modelBuilder.Ignore<ContinuousCombustionEngine>();
        modelBuilder.Ignore<IntermittentCombustionEngine>();
        modelBuilder.Ignore<SolidRocket>();
        modelBuilder.Ignore<Operator>();
        modelBuilder.Ignore<LicensedOperator>();
        modelBuilder.Ignore<OperatorDetails>();
        modelBuilder.Entity<Vehicle>(
            vb =>
            {
                vb.Property(v => v.Name).HasColumnName("Name");
                vb.Ignore(v => v.Operator);
                vb.ComplexProperty(
                    v => v.Operator, ob =>
                    {
                        ob.IsRequired();
                        ob.Property(o => o.VehicleName).HasColumnName("Name");
                        ob.ComplexProperty(o => o.Details)
                            .IsRequired()
                            .Property(o => o.VehicleName).HasColumnName("Name");
                    });
            });
        modelBuilder.Entity<PoweredVehicle>(
            vb =>
            {
                vb.Ignore(v => v.Engine);
                vb.ComplexProperty(
                    v => v.Engine, eb =>
                    {
                        eb.IsRequired();
                        eb.Property(o => o.VehicleName).HasColumnName("Name");
                    });
            });
    }

    protected virtual void OnSharedModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeterReadingDetail>(
            dob =>
            {
                dob.ToTable("MeterReadings");
                dob.Property(o => o.ReadingStatus).HasColumnName("ReadingStatus");
            });
        modelBuilder.Entity<MeterReading>(
            ob =>
            {
                ob.ToTable("MeterReadings");
                ob.Property(o => o.ReadingStatus).HasColumnName("ReadingStatus");
                ob.HasOne(o => o.MeterReadingDetails).WithOne()
                    .HasForeignKey<MeterReadingDetail>(o => o.Id);
            });
    }

    protected async Task InitializeAsync(Action<ModelBuilder> onModelCreating, bool seed = true)
        => ContextFactory = await InitializeAsync<TransportationContext>(
            onModelCreating, shouldLogCategory: _ => true, seed: seed ? c => c.SeedAsync() : null);

    protected async Task InitializeSharedAsync(Action<ModelBuilder> onModelCreating, bool sensitiveLogEnabled = true)
        => SharedContextFactory = await InitializeAsync<SharedTableContext>(
            onModelCreating,
            shouldLogCategory: _ => true,
            onConfiguring: options =>
            {
                options.ConfigureWarnings(w => w.Log(RelationalEventId.OptionalDependentWithAllNullPropertiesWarning))
                    .ConfigureWarnings(w => w.Log(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning))
                    .EnableSensitiveDataLogging(sensitiveLogEnabled);
            }
        );

    protected virtual TransportationContext CreateContext()
        => ContextFactory.CreateContext();

    protected virtual SharedTableContext CreateSharedContext()
        => SharedContextFactory.CreateContext();

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();

        ContextFactory = null;
        SharedContextFactory = null;
    }

    protected class SharedTableContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<MeterReading> MeterReadings { get; set; }
        public DbSet<MeterReadingDetail> MeterReadingDetails { get; set; }
    }

    protected class MeterReading
    {
        public int Id { get; set; }
        public MeterReadingStatus? ReadingStatus { get; set; }
        public MeterReadingDetail MeterReadingDetails { get; set; }
    }

    protected class MeterReadingDetail
    {
        public int Id { get; set; }
        public MeterReadingStatus? ReadingStatus { get; set; }
        public string CurrentRead { get; set; }
        public string PreviousRead { get; set; }
    }

    protected enum MeterReadingStatus
    {
        Running = 0,
        NotAccesible = 2
    }
}
