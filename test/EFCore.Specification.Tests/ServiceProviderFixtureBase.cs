// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

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
            .Options;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(ShouldLogCategory))
            .AddSingleton<IModelCacheKeyFactory>(new FuncCacheKeyFactory(GetAdditionalModelCacheKey));

    protected virtual bool ShouldLogCategory(string logCategory)
        => false;

    protected virtual object GetAdditionalModelCacheKey(DbContext context)
        => null;

    private class FuncCacheKeyFactory(Func<DbContext, object> getAdditionalKey) : IModelCacheKeyFactory
    {
        private readonly Func<DbContext, object> _getAdditionalKey = getAdditionalKey;

        public object Create(DbContext context)
            => Tuple.Create(context.GetType(), _getAdditionalKey(context));

        public object Create(DbContext context, bool designTime)
            => Tuple.Create(context.GetType(), _getAdditionalKey(context), designTime);
    }
}
