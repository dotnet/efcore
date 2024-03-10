// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class DesignTimeTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : DesignTimeTestBase<TFixture>.DesignTimeFixtureBase
{
    protected TFixture Fixture { get; }

    protected DesignTimeTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected abstract Assembly ProviderAssembly { get; }

    [ConditionalFact]
    public void Can_get_reverse_engineering_services()
    {
        using var context = Fixture.CreateContext();
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices();
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                    throwOnError: true))!)
            .ConfigureDesignTimeServices(serviceCollection);
        using var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        var reverseEngineerScaffolder = services.CreateScope().ServiceProvider.GetService<IReverseEngineerScaffolder>();

        Assert.NotNull(reverseEngineerScaffolder);
    }

    [ConditionalFact]
    public void Can_get_migrations_services()
    {
        using var context = Fixture.CreateContext();
        var serviceCollection = new ServiceCollection()
            .AddEntityFrameworkDesignTimeServices()
            .AddDbContextDesignTimeServices(context);
        ((IDesignTimeServices)Activator.CreateInstance(
                ProviderAssembly.GetType(
                    ProviderAssembly.GetCustomAttribute<DesignTimeProviderServicesAttribute>().TypeName,
                    throwOnError: true))!)
            .ConfigureDesignTimeServices(serviceCollection);
        using var services = serviceCollection.BuildServiceProvider(validateScopes: true);

        var migrationsScaffolder = services.CreateScope().ServiceProvider.GetService<IMigrationsScaffolder>();

        Assert.NotNull(migrationsScaffolder);
    }

    public abstract class DesignTimeFixtureBase : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => "DesignTimeTest";
    }
}
