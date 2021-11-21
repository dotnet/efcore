// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore;

public abstract class ServiceProviderFixtureBase : FixtureBase
{
    public IServiceProvider ServiceProvider { get; }
    protected abstract ITestStoreFactory TestStoreFactory { get; }

    private ListLoggerFactory _listLoggerFactory;

    public ListLoggerFactory ListLoggerFactory
        => _listLoggerFactory ??= (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    protected ServiceProviderFixtureBase()
    {
        ServiceProvider = AddServices(TestStoreFactory.AddProviderServices(new ServiceCollection()))
            .BuildServiceProvider(validateScopes: true);
    }

    public DbContextOptions CreateOptions(TestStore testStore)
        => AddOptions(testStore.AddProviderOptions(new DbContextOptionsBuilder()))
            .EnableDetailedErrors()
            .UseInternalServiceProvider(ServiceProvider)
            .EnableServiceProviderCaching(false)
            .Options;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(ShouldLogCategory));

    protected virtual bool ShouldLogCategory(string logCategory)
        => false;
}
