// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable VirtualMemberCallInConstructor

namespace Microsoft.EntityFrameworkCore;

public abstract class SharedStoreFixtureBase<TContext> : FixtureBase, IAsyncLifetime
    where TContext : DbContext
{
    protected virtual Type ContextType { get; } = typeof(TContext);

    private IServiceProvider? _serviceProvider;

    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider
            ?? throw new InvalidOperationException(
                $"You must override the {nameof(InitializeAsync)} method and call `await base.{nameof(InitializeAsync)}();`. At this point the {nameof(ServiceProvider)} property will be available.");
        private set => _serviceProvider = value;
    }

    protected abstract string StoreName { get; }
    protected abstract ITestStoreFactory TestStoreFactory { get; }
    protected virtual bool RecreateStore { get; } = false;

    public TestStore TestStore
    {
        get => field
            ?? throw new InvalidOperationException(
                $"You must override the {nameof(InitializeAsync)} method and call `await base.{nameof(InitializeAsync)}();`. At this point the {nameof(TestStore)} property will be available.");
        private set;
    }

    protected virtual bool UsePooling
        => true;

    private object ContextFactory
        => field ??= ServiceProvider
            .GetRequiredService(typeof(IDbContextFactory<>).MakeGenericType(ContextType));

    public ListLoggerFactory ListLoggerFactory
        => field ??= (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    private MethodInfo? _createDbContext;

    public virtual async ValueTask InitializeAsync()
    {
        TestStore = RecreateStore ? TestStoreFactory.Create(StoreName) : TestStoreFactory.GetOrCreate(StoreName);

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
                    .Single(mi => mi.GetParameters().Length == 0
                        && mi.GetGenericArguments().Length == 0);
        }

        ServiceProvider = services.BuildServiceProvider(validateScopes: true);

        await TestStore.InitializeAsync(ServiceProvider, CreateContext, c => SeedAsync((TContext)c), CleanAsync);

        ListLoggerFactory.Clear();
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

    public virtual async Task ReseedAsync()
    {
        using var context = CreateContext();
        await TestStore.CleanAsync(context);
        await CleanAsync(context);
        await SeedAsync(context);
    }

    protected virtual Task SeedAsync(TContext context)
        => Task.CompletedTask;

    protected virtual void Clean(DbContext context)
    {
    }

    protected virtual Task CleanAsync(DbContext context)
    {
        Clean(context);
        return Task.CompletedTask;
    }

    public virtual async ValueTask DisposeAsync()
    {
        try
        {
            await TestStore.DisposeAsync();
        }
        finally
        {
            if (_serviceProvider is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
