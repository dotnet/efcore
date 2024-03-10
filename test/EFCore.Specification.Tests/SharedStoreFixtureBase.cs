// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore;

public abstract class SharedStoreFixtureBase<TContext> : FixtureBase, IDisposable, IAsyncLifetime
    where TContext : DbContext
{
    protected virtual Type ContextType { get; } = typeof(TContext);

    private IServiceProvider? _serviceProvider;

    public IServiceProvider ServiceProvider
        => _serviceProvider
            ?? throw new InvalidOperationException(
                $"You must override the {nameof(InitializeAsync)} method and call `await base.{nameof(InitializeAsync)}();`. At this point the {nameof(ServiceProvider)} property will be available.");

    protected abstract string StoreName { get; }
    protected abstract ITestStoreFactory TestStoreFactory { get; }

    private TestStore? _testStore;

    public TestStore TestStore
        => _testStore
            ?? throw new InvalidOperationException(
                $"You must override the {nameof(InitializeAsync)} method and call `await base.{nameof(InitializeAsync)}();`. At this point the {nameof(TestStore)} property will be available.");

    protected virtual bool UsePooling
        => true;

    private object? _contextFactory;

    private object ContextFactory
        => _contextFactory ??= ServiceProvider
            .GetRequiredService(typeof(IDbContextFactory<>).MakeGenericType(ContextType));

    private ListLoggerFactory? _listLoggerFactory;

    public ListLoggerFactory ListLoggerFactory
        => _listLoggerFactory ??= (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    private MethodInfo? _createDbContext;

    public virtual Task InitializeAsync()
    {
        _testStore = TestStoreFactory.GetOrCreate(StoreName);

        var services = AddServices(TestStoreFactory.AddProviderServices(new ServiceCollection()));
        services = UsePooling
            ? services.AddPooledDbContextFactory(ContextType, (s, b) => ConfigureOptions(s, b))
            : services.AddDbContext(
                ContextType,
                (s, b) => ConfigureOptions(s, b),
                ServiceLifetime.Transient,
                ServiceLifetime.Singleton);

        if (UsePooling)
        {
            _createDbContext
                = typeof(IDbContextFactory<>).MakeGenericType(ContextType)
                    .GetTypeInfo().GetDeclaredMethods(nameof(IDbContextFactory<TContext>.CreateDbContext))
                    .Single(
                        mi => mi.GetParameters().Length == 0
                            && mi.GetGenericArguments().Length == 0);
        }

        _serviceProvider = services.BuildServiceProvider(validateScopes: true);

        TestStore.Initialize(ServiceProvider, CreateContext, c => Seed((TContext)c), Clean);

        return Task.CompletedTask;
    }

    public virtual TContext CreateContext()
        => UsePooling
            ? (TContext)_createDbContext!.Invoke(ContextFactory, null)!
            : (TContext)ServiceProvider.GetRequiredService(ContextType);

    public DbContextOptions CreateOptions()
        => ConfigureOptions(ServiceProvider, new DbContextOptionsBuilder()).Options;

    private DbContextOptionsBuilder ConfigureOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder optionsBuilder)
        => AddOptions(TestStore.AddProviderOptions(optionsBuilder))
            .UseInternalServiceProvider(serviceProvider);

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(ShouldLogCategory));

    protected virtual bool ShouldLogCategory(string logCategory)
        => false;

    public virtual void Reseed()
    {
        using var context = CreateContext();
        TestStore.Clean(context);
        Clean(context);
        Seed(context);
    }

    public virtual async Task ReseedAsync()
    {
        using var context = CreateContext();
        await TestStore.CleanAsync(context);
        await CleanAsync(context);
        await SeedAsync(context);
    }

    protected virtual void Seed(TContext context)
    {
    }

    protected virtual Task SeedAsync(TContext context)
    {
        Seed(context);
        return Task.CompletedTask;
    }

    protected virtual void Clean(DbContext context)
    {
    }

    protected virtual Task CleanAsync(DbContext context)
    {
        Clean(context);
        return Task.CompletedTask;
    }

    // Called after DisposeAsync
    public virtual void Dispose()
    {
    }

    public virtual Task DisposeAsync()
        => TestStore.DisposeAsync();
}
