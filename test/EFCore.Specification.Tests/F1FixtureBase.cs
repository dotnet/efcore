// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

namespace Microsoft.EntityFrameworkCore;

public abstract class F1FixtureBase<TRowVersion> : SharedStoreFixtureBase<F1Context>
{
    protected override string StoreName
        => "F1Test";

    protected override bool UsePooling
        => true;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder)
            .UseModel(CreateModelExternal())
            .ConfigureWarnings(
                w => w.Ignore(CoreEventId.SaveChangesStarting, CoreEventId.SaveChangesCompleted));

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection.AddSingleton<ISingletonInterceptor, F1MaterializationInterceptor>());

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Update.Name;

    private IModel CreateModelExternal()
    {
        // Doing this differently here from other tests to have regression coverage for
        // building models externally from the context instance.
        var builder = CreateModelBuilder();

        BuildModelExternal(builder);

        return (IModel)builder.Model;
    }

    public abstract TestHelpers TestHelpers { get; }

    public ModelBuilder CreateModelBuilder()
        => TestHelpers.CreateConventionBuilder();

    protected virtual void BuildModelExternal(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chassis>(
            b =>
            {
                b.HasKey(c => c.TeamId);
                ConfigureConstructorBinding<Chassis>(b.Metadata, nameof(Chassis.TeamId), nameof(Chassis.Name));
            });

        modelBuilder.Entity<Engine>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.EngineSupplierId).IsConcurrencyToken();
                b.Property(e => e.Name).IsConcurrencyToken();
                b.OwnsOne(
                    e => e.StorageLocation, lb =>
                    {
                        lb.Property(l => l.Latitude).IsConcurrencyToken();
                        lb.Property(l => l.Longitude).IsConcurrencyToken();
                    });
                ConfigureConstructorBinding<Engine>(b.Metadata, nameof(Engine.Id), nameof(Engine.Name));
            });

        modelBuilder.Entity<EngineSupplier>(
            b =>
            {
                b.HasKey(e => e.Name);
                ConfigureConstructorBinding<EngineSupplier>(b.Metadata, nameof(EngineSupplier.Name));
            });

        modelBuilder.Entity<Gearbox>(
            b =>
            {
                ConfigureConstructorBinding<Gearbox>(b.Metadata, nameof(Gearbox.Id), nameof(Gearbox.Name));
            });

        modelBuilder.Entity<Sponsor>(
            b =>
            {
                b.Property<int?>(Sponsor.ClientTokenPropertyName)
                    .IsConcurrencyToken();
            });

        modelBuilder.Entity<Team>(
            b =>
            {
                b.HasOne(e => e.Gearbox).WithOne().HasForeignKey<Team>(e => e.GearboxId);
                b.HasOne(e => e.Chassis).WithOne(e => e.Team).HasForeignKey<Chassis>(e => e.TeamId);

                b.HasMany(t => t.Sponsors)
                    .WithMany(s => s.Teams)
                    .UsingEntity<TeamSponsor>(
                        ts => ts
                            .HasOne(t => t.Sponsor)
                            .WithMany(),
                        ts => ts
                            .HasOne(t => t.Team)
                            .WithMany())
                    .HasKey(ts => new { ts.SponsorId, ts.TeamId });

                ConfigureConstructorBinding<Team>(
                    b.Metadata,
                    nameof(Team.Id),
                    nameof(Team.Name),
                    nameof(Team.Constructor),
                    nameof(Team.Tire),
                    nameof(Team.Principal),
                    nameof(Team.ConstructorsChampionships),
                    nameof(Team.DriversChampionships),
                    nameof(Team.Races),
                    nameof(Team.Victories),
                    nameof(Team.Poles),
                    nameof(Team.FastestLaps),
                    nameof(Team.GearboxId)
                );
            });

        modelBuilder.Entity<Driver>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                ConfigureConstructorBinding<Driver>(
                    b.Metadata,
                    nameof(Driver.Id),
                    nameof(Driver.Name),
                    nameof(Driver.CarNumber),
                    nameof(Driver.Championships),
                    nameof(Driver.Races),
                    nameof(Driver.Wins),
                    nameof(Driver.Podiums),
                    nameof(Driver.Poles),
                    nameof(Driver.FastestLaps),
                    nameof(Driver.TeamId)
                );
            });

        modelBuilder.Entity<TestDriver>(
            b =>
            {
                ConfigureConstructorBinding<TestDriver, Driver>(
                    b.Metadata,
                    nameof(Driver.Id),
                    nameof(Driver.Name),
                    nameof(Driver.CarNumber),
                    nameof(Driver.Championships),
                    nameof(Driver.Races),
                    nameof(Driver.Wins),
                    nameof(Driver.Podiums),
                    nameof(Driver.Poles),
                    nameof(Driver.FastestLaps),
                    nameof(Driver.TeamId)
                );
            });

        modelBuilder.Entity<Sponsor>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
            });

        modelBuilder.Entity<TitleSponsor>(
            b =>
            {
                // TODO: Configure as ComplexProperty when optional complex types are supported
                // Issue #31376
                b.OwnsOne(
                    s => s.Details, eb =>
                    {
                        eb.Property(d => d.Space);
                        eb.Property<TRowVersion>("Version").IsRowVersion();
                        eb.Property<int?>(Sponsor.ClientTokenPropertyName).IsConcurrencyToken();
                    });
                ConfigureConstructorBinding<TitleSponsor>(b.Metadata);
            });

        modelBuilder.Entity<Chassis>().Property<TRowVersion>("Version").IsRowVersion();
        modelBuilder.Entity<Driver>().Property<TRowVersion>("Version").IsRowVersion();

        modelBuilder.Entity<Team>().Property<TRowVersion>("Version")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        modelBuilder.Entity<Sponsor>(
            eb =>
            {
                eb.Property<TRowVersion>("Version").IsRowVersion();
                eb.Property<int?>(Sponsor.ClientTokenPropertyName);
            });

        modelBuilder.Entity<Fan>();
        modelBuilder.Entity<SuperFan>();
        modelBuilder.Entity<MegaFan>();

        modelBuilder.Entity<FanTpt>();
        modelBuilder.Entity<SuperFanTpt>();
        modelBuilder.Entity<MegaFanTpt>();

        modelBuilder.Entity<FanTpc>();
        modelBuilder.Entity<SuperFanTpc>();
        modelBuilder.Entity<MegaFanTpc>();

        modelBuilder.Entity<Circuit>();
        modelBuilder.Entity<StreetCircuit>().HasOne(e => e.City).WithOne().HasForeignKey<City>(e => e.Id);
        modelBuilder.Entity<OvalCircuit>();
        modelBuilder.Entity<City>();

        modelBuilder.Entity<CircuitTpt>();
        modelBuilder.Entity<StreetCircuitTpt>().HasOne(e => e.City).WithOne().HasForeignKey<CityTpt>(e => e.Id);
        modelBuilder.Entity<OvalCircuitTpt>();
        modelBuilder.Entity<CityTpt>();

        modelBuilder.Entity<CircuitTpc>();
        modelBuilder.Entity<StreetCircuitTpc>().HasOne(e => e.City).WithOne().HasForeignKey<CityTpc>(e => e.Id);
        modelBuilder.Entity<OvalCircuitTpc>();
        modelBuilder.Entity<CityTpc>();
    }

    private static void ConfigureConstructorBinding<TEntity>(IMutableEntityType mutableEntityType, params string[] propertyNames)
        => ConfigureConstructorBinding<TEntity, TEntity>(mutableEntityType, propertyNames);

    private static void ConfigureConstructorBinding<TEntity, TLoaderEntity>(
        IMutableEntityType mutableEntityType,
        params string[] propertyNames)
    {
        var entityType = (EntityType)mutableEntityType;
        var loaderField = typeof(TLoaderEntity).GetField("_loader", BindingFlags.Instance | BindingFlags.NonPublic);
        var parameterBindings = new List<ParameterBinding>();

        if (loaderField != null)
        {
            var loaderProperty = entityType.FindServiceProperty(loaderField.Name)!;
            parameterBindings.Add(new DependencyInjectionParameterBinding(typeof(ILazyLoader), typeof(ILazyLoader), loaderProperty));
        }

        foreach (var propertyName in propertyNames)
        {
            parameterBindings.Add(new PropertyParameterBinding(entityType.FindProperty(propertyName)!));
        }

        entityType.ConstructorBinding
            = new ConstructorBinding(
                typeof(TEntity).GetTypeInfo().DeclaredConstructors.Single(c => c.GetParameters().Length == parameterBindings.Count),
                parameterBindings
            );
    }

    protected override Task SeedAsync(F1Context context)
        => F1Context.SeedAsync(context);
}
