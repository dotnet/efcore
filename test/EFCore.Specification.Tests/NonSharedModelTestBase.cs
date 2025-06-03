// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable VirtualMemberCallInConstructor

namespace Microsoft.EntityFrameworkCore;

public abstract class NonSharedModelTestBase : IAsyncLifetime
{
    public static readonly IEnumerable<object[]> IsAsyncData = [[false], [true]];

    protected abstract string StoreName { get; }
    protected abstract ITestStoreFactory TestStoreFactory { get; }
    protected NonSharedFixture? Fixture { get; set; }
    protected virtual ITestOutputHelper? TestOutputHelper { get; set; }

    private ServiceProvider? _serviceProvider;
    protected IServiceProvider ServiceProvider
        => _serviceProvider
            ?? throw new InvalidOperationException(
                $"You must call `await {nameof(InitializeAsync)}(\"DatabaseName\");` at the beginning of the test.");

    private TestStore? _testStore;
    protected TestStore TestStore
        => _testStore
            ?? throw new InvalidOperationException(
                $"You must call `await {nameof(InitializeAsync)}(\"DatabaseName\");` at the beginning of the test.");

    private ListLoggerFactory? _listLoggerFactory;
    protected ListLoggerFactory ListLoggerFactory
        => _listLoggerFactory ??= (ListLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    protected NonSharedModelTestBase()
    { }

    protected NonSharedModelTestBase(NonSharedFixture fixture)
        => Fixture = fixture;

    public virtual Task InitializeAsync()
        => Task.CompletedTask;

    protected virtual async Task<ContextFactory<TContext>> InitializeAsync<TContext>(
        Action<ModelBuilder>? onModelCreating = null,
        Action<DbContextOptionsBuilder>? onConfiguring = null,
        Func<IServiceCollection, IServiceCollection>? addServices = null,
        Action<ModelConfigurationBuilder>? configureConventions = null,
        Func<TContext, Task>? seed = null,
        Func<string, bool>? shouldLogCategory = null,
        Func<TestStore>? createTestStore = null,
        bool usePooling = true,
        bool useServiceProvider = true)
        where TContext : DbContext
    {
        if (Fixture == null && _testStore != null)
        {
            await _testStore.DisposeAsync();
            _serviceProvider?.Dispose();
        }

        var contextFactory = CreateContextFactory<TContext>(
            onModelCreating,
            onConfiguring,
            addServices,
            configureConventions,
            shouldLogCategory,
            createTestStore,
            usePooling,
            useServiceProvider);

        await TestStore.InitializeAsync(_serviceProvider, contextFactory.CreateContext, seed == null ? null : c => seed((TContext)c));

        ListLoggerFactory.Clear();

        return contextFactory;
    }

    protected virtual ContextFactory<TContext> CreateContextFactory<TContext>(
        Action<ModelBuilder>? onModelCreating = null,
        Action<DbContextOptionsBuilder>? onConfiguring = null,
        Func<IServiceCollection, IServiceCollection>? addServices = null,
        Action<ModelConfigurationBuilder>? configureConventions = null,
        Func<string, bool>? shouldLogCategory = null,
        Func<TestStore>? createTestStore = null,
        bool usePooling = true,
        bool useServiceProvider = true)
        where TContext : DbContext
    {
        if (createTestStore != null)
        {
            _testStore = createTestStore();
            Fixture = null;
        }
        else
        {
            _testStore = Fixture != null
                ? Fixture.GetOrCreateTestStore(CreateTestStore)
                : CreateTestStore();
        }

        shouldLogCategory ??= _ => false;
        var services = AddServices(
            (useServiceProvider
                ? TestStoreFactory.AddProviderServices(new ServiceCollection())
                : new ServiceCollection())
            .AddSingleton<ILoggerFactory>(TestStoreFactory.CreateListLoggerFactory(shouldLogCategory)));

        if (onModelCreating != null)
        {
            services = services.AddSingleton(TestModelSource.GetFactory(onModelCreating, configureConventions));
        }

        addServices?.Invoke(services);

        services = usePooling
            ? services.AddPooledDbContextFactory(
                typeof(TContext), (s, b) => ConfigureOptions(useServiceProvider ? s : null, b, onConfiguring))
            : services.AddDbContext(
                typeof(TContext),
                (s, b) => ConfigureOptions(useServiceProvider ? s : null, b, onConfiguring),
                ServiceLifetime.Transient,
                ServiceLifetime.Singleton);

        _serviceProvider = services.BuildServiceProvider(validateScopes: true);

        return new ContextFactory<TContext>(_serviceProvider, usePooling, _testStore);
    }

    private DbContextOptionsBuilder ConfigureOptions(
        IServiceProvider? serviceProvider,
        DbContextOptionsBuilder optionsBuilder,
        Action<DbContextOptionsBuilder>? onConfiguring)
    {
        optionsBuilder = AddOptions(TestStore.AddProviderOptions(optionsBuilder));
        if (serviceProvider == null)
        {
            optionsBuilder.EnableServiceProviderCaching(false);
        }
        else
        {
            optionsBuilder.UseInternalServiceProvider(serviceProvider);
        }

        onConfiguring?.Invoke(optionsBuilder);
        return optionsBuilder;
    }

    protected virtual IServiceCollection AddServices(IServiceCollection serviceCollection)
        => serviceCollection;

    protected virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(
                b => b.Default(WarningBehavior.Throw)
                    .Log(CoreEventId.SensitiveDataLoggingEnabledWarning)
                    .Log(CoreEventId.PossibleUnintendedReferenceComparisonWarning)
                    .Ignore(
                        CoreEventId.MappedEntityTypeIgnoredWarning,
                        CoreEventId.MappedPropertyIgnoredWarning,
                        CoreEventId.MappedNavigationIgnoredWarning));

    protected virtual TestStore CreateTestStore()
        => TestStoreFactory.Create(StoreName);

    // Called after DisposeAsync
    public virtual void Dispose()
    {
    }

    public virtual async Task DisposeAsync()
    {
        if (Fixture == null && _testStore != null)
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
                PooledContextFactory = (IDbContextFactory<TContext>)ServiceProvider
                    .GetRequiredService(typeof(IDbContextFactory<TContext>));
            }

            TestStore = testStore;
        }

        public IServiceProvider ServiceProvider { get; }
        protected virtual bool UsePooling { get; }

        private IDbContextFactory<TContext>? PooledContextFactory { get; }

        public TestStore TestStore { get; }

        public virtual TContext CreateContext()
            => UsePooling
                ? PooledContextFactory!.CreateDbContext()
                : (TContext)ServiceProvider.GetRequiredService(typeof(TContext));

        public virtual DbContextOptions GetOptions()
            => ServiceProvider.GetRequiredService<DbContextOptions>();
    }
}
