// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

// ReSharper disable VirtualMemberCallInConstructor
namespace Microsoft.EntityFrameworkCore;

public abstract class NonSharedModelTestBase : IDisposable, IAsyncLifetime
{
    protected abstract string StoreName { get; }
    protected abstract ITestStoreFactory TestStoreFactory { get; }

    private ServiceProvider _serviceProvider;

    protected IServiceProvider ServiceProvider
        => _serviceProvider
            ?? throw new InvalidOperationException(
                $"You must call `await {nameof(InitializeAsync)}(\"DatabaseName\");` at the beginning of the test.");

    private TestStore _testStore;

    protected TestStore TestStore
        => _testStore
            ?? throw new InvalidOperationException(
                $"You must call `await {nameof(InitializeAsync)}(\"DatabaseName\");` at the beginning of the test.");

    private ListLoggerFactory _listLoggerFactory;

    protected ListLoggerFactory ListLoggerFactory
        => _listLoggerFactory ??= (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    public virtual Task InitializeAsync()
        => Task.CompletedTask;

    protected virtual ContextFactory<TContext> Initialize<TContext>(
        Action<ModelBuilder> onModelCreating = null,
        Action<DbContextOptionsBuilder> onConfiguring = null,
        Action<IServiceCollection> addServices = null,
        Action<TContext> seed = null,
        Func<string, bool> shouldLogCategory = null,
        Func<TestStore> createTestStore = null,
        bool usePooling = true)
        where TContext : DbContext
    {
        var contextFactory = Initialize<TContext>(
            onModelCreating, onConfiguring, addServices, shouldLogCategory, createTestStore, usePooling);

        TestStore.Initialize(_serviceProvider, contextFactory.CreateContext, seed == null ? null : c => seed((TContext)c));

        ListLoggerFactory.Clear();

        return contextFactory;
    }

    protected virtual Task<ContextFactory<TContext>> InitializeAsync<TContext>(
        Action<ModelBuilder> onModelCreating = null,
        Action<DbContextOptionsBuilder> onConfiguring = null,
        Action<IServiceCollection> addServices = null,
        Action<TContext> seed = null,
        Func<string, bool> shouldLogCategory = null,
        Func<TestStore> createTestStore = null,
        bool usePooling = true)
        where TContext : DbContext
    {
        var contextFactory = Initialize<TContext>(
            onModelCreating, onConfiguring, addServices, shouldLogCategory, createTestStore, usePooling);

        TestStore.Initialize(_serviceProvider, contextFactory.CreateContext, seed == null ? null : c => seed((TContext)c));

        ListLoggerFactory.Clear();

        return Task.FromResult(contextFactory);
    }

    private ContextFactory<TContext> Initialize<TContext>(
        Action<ModelBuilder> onModelCreating,
        Action<DbContextOptionsBuilder> onConfiguring,
        Action<IServiceCollection> addServices,
        Func<string, bool> shouldLogCategory,
        Func<TestStore> createTestStore,
        bool usePooling)
        where TContext : DbContext
    {
        _testStore = createTestStore?.Invoke() ?? CreateTestStore();

        shouldLogCategory ??= _ => false;
        var services = TestStoreFactory.AddProviderServices(new ServiceCollection())
            .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(shouldLogCategory));

        if (onModelCreating != null)
        {
            services = services.AddSingleton(TestModelSource.GetFactory(onModelCreating));
        }

        if (addServices != null)
        {
            addServices(services);
        }

        services = usePooling
            ? services.AddDbContextPool(typeof(TContext), (s, b) => ConfigureOptions(s, b, onConfiguring))
            : services.AddDbContext(
                typeof(TContext),
                (s, b) => ConfigureOptions(s, b, onConfiguring),
                ServiceLifetime.Transient,
                ServiceLifetime.Singleton);

        _serviceProvider = services.BuildServiceProvider(validateScopes: true);

        var contextFactory = new ContextFactory<TContext>(_serviceProvider, usePooling, _testStore);
        return contextFactory;
    }

    private DbContextOptionsBuilder ConfigureOptions(
        IServiceProvider serviceProvider,
        DbContextOptionsBuilder optionsBuilder,
        Action<DbContextOptionsBuilder> onConfiguring)
    {
        optionsBuilder = AddOptions(TestStore.AddProviderOptions(optionsBuilder))
            .UseInternalServiceProvider(serviceProvider);
        onConfiguring?.Invoke(optionsBuilder);
        return optionsBuilder;
    }

    protected virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(
                b => b.Default(WarningBehavior.Throw)
                    .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                    .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning));

    protected virtual TestStore CreateTestStore()
        => TestStoreFactory.Create(StoreName);

    // Called after DisposeAsync
    public virtual void Dispose()
    {
    }

    public virtual async Task DisposeAsync()
    {
        if (_testStore != null)
        {
            await _testStore.DisposeAsync();
            _testStore = null;
        }

        _serviceProvider?.Dispose();
        _serviceProvider = null;

        _listLoggerFactory = null;
    }

    protected class ContextFactory<TContext>
        where TContext : DbContext
    {
        public ContextFactory(IServiceProvider serviceProvider, bool usePooling, TestStore testStore)
        {
            ServiceProvider = serviceProvider;
            UsePooling = usePooling;
            if (usePooling)
            {
                ContextPool ??= (IDbContextPool)ServiceProvider
                    .GetRequiredService(typeof(IDbContextPool<>).MakeGenericType(typeof(TContext)));
            }

            TestStore = testStore;
        }

        public IServiceProvider ServiceProvider { get; }
        protected virtual bool UsePooling { get; }
        private IDbContextPool ContextPool { get; }
        public TestStore TestStore { get; }

        public virtual TContext CreateContext()
            => UsePooling
                ? (TContext)new DbContextLease(ContextPool, standalone: true).Context
                : (TContext)ServiceProvider.GetRequiredService(typeof(TContext));
    }
}
