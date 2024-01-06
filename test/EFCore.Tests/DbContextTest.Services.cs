// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public partial class DbContextTest
    {
        protected static readonly Guid GuidSentinel = new("56D3784D-6F7F-4935-B7F6-E77DC6E1D91E");
        protected static readonly int IntSentinel = 667;

        [ConditionalFact]
        public void Can_log_debug_events_with_OnConfiguring()
            => DebugLogTest(useLoggerFactory: false, configureForDebug: false, shouldLog: true);

        [ConditionalFact]
        public void Cannot_log_debug_events_with_default_UseLoggerFactory()
            => DebugLogTest(useLoggerFactory: true, configureForDebug: false, shouldLog: false);

        [ConditionalFact]
        public void Can_log_debug_events_with_UseLoggerFactory_when_configured()
            => DebugLogTest(useLoggerFactory: true, configureForDebug: true, shouldLog: true);

        private void DebugLogTest(bool useLoggerFactory, bool configureForDebug, bool shouldLog)
        {
            Log.Clear();

            using var context = new InfoLogContext(useLoggerFactory, configureForDebug);
            context.GetService<ILoggerFactory>().AddProvider(new MyLoggerProvider());

            var logger = context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>();

            logger.ServiceProviderCreated(new ServiceCollection().BuildServiceProvider(validateScopes: true));

            var resultQuery = Log.Where(e => e.Id.Id == CoreEventId.ServiceProviderCreated.Id);

            if (shouldLog)
            {
                Assert.Equal(LogLevel.Debug, resultQuery.Single().Level);
            }
            else
            {
                Assert.Empty(resultQuery);
            }
        }

        protected static List<(LogLevel Level, EventId Id, string Message)> Log { get; } = [];

        private class InfoLogContext(bool useLoggerFactory, bool configureForDebug) : DbContext
        {
            private readonly bool _useLoggerFactory = useLoggerFactory;
            private readonly bool _configureForDebug = configureForDebug;

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase(typeof(InfoLogContext).FullName)
                    .ConfigureWarnings(w => w.Default(WarningBehavior.Log));

                if (_useLoggerFactory)
                {
                    var externalProvider =
                        _configureForDebug
                            ? new ServiceCollection()
                                .AddLogging(b => b.SetMinimumLevel(LogLevel.Debug))
                                .BuildServiceProvider(validateScopes: true)
                            : new ServiceCollection()
                                .AddLogging()
                                .BuildServiceProvider(validateScopes: true);

                    optionsBuilder
                        .EnableServiceProviderCaching(false)
                        .UseLoggerFactory(externalProvider.GetService<ILoggerFactory>());
                }
                else
                {
                    var internalServiceProvider = new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider(validateScopes: true);

                    optionsBuilder.UseInternalServiceProvider(internalServiceProvider);
                }
            }
        }

        private class MyLoggerProvider : ILoggerProvider
        {
            private bool _disposed;

            public ILogger CreateLogger(string categoryName)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(MyLoggerProvider));
                }

                return new MyListLogger(Log);
            }

            public void Dispose()
                => _disposed = true;

            private class MyListLogger(List<(LogLevel, EventId, string)> logMessage) : ILogger
            {
                private List<(LogLevel, EventId, string)> LogMessages { get; } = logMessage;

                public void Log<TState>(
                    LogLevel logLevel,
                    EventId eventId,
                    TState state,
                    Exception exception,
                    Func<TState, Exception, string> formatter)
                {
                    var message = new StringBuilder();
                    if (formatter != null)
                    {
                        message.Append(formatter(state, exception));
                    }
                    else if (state != null)
                    {
                        message.Append(state);

                        if (exception != null)
                        {
                            message.Append(Environment.NewLine);
                            message.Append(exception);
                        }
                    }

                    LogMessages?.Add((logLevel, eventId, message.ToString()));
                }

                public bool IsEnabled(LogLevel logLevel)
                    => true;

                public IDisposable BeginScope(object state)
                    => throw new NotImplementedException();

                public IDisposable BeginScope<TState>(TState state)
                    => null;
            }
        }

        [ConditionalTheory]
        [InlineData(ServiceLifetime.Scoped, false)]
        [InlineData(ServiceLifetime.Singleton, false)]
        [InlineData(ServiceLifetime.Singleton, true)]
        public void Logger_factory_registered_on_application_service_provider_is_not_disposed(ServiceLifetime optionsLifetime, bool pool)
        {
            for (var i = 0; i < 2; i++)
            {
                ILoggerFactory loggerFactory;

                var serviceCollection
                    = new ServiceCollection()
                        .AddScoped<Random>()
                        .AddLogging();

                if (pool)
                {
                    serviceCollection.AddDbContextPool<ConstructorTestContext1A>(
                        b => b.UseInMemoryDatabase("Scratch")
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)));
                }
                else
                {
                    serviceCollection.AddDbContext<ConstructorTestContext1A>(
                        b => b.UseInMemoryDatabase("Scratch")
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)),
                        optionsLifetime: optionsLifetime);
                }

                // No scope validation here: see Issue #13540
                var appServiceProvider = serviceCollection.BuildServiceProvider();

                using (appServiceProvider)
                {
                    loggerFactory = appServiceProvider.GetService<ILoggerFactory>();
                    Random scopedExternalService;
                    Random scopedExternalServiceFromContext;

                    using (var scope = appServiceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ConstructorTestContext1A>();

                        // Should not throw
                        var _ = context.Model;

                        Assert.Same(loggerFactory, scope.ServiceProvider.GetService<ILoggerFactory>());

                        scopedExternalService = scope.ServiceProvider.GetService<Random>();
                        Assert.NotNull(scopedExternalService);

                        scopedExternalServiceFromContext = context.GetService<Random>();
                        Assert.NotNull(scopedExternalService);
                    }

                    using (var scope = appServiceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ConstructorTestContext1A>();

                        // Should not throw
                        var _ = context.Model;

                        Assert.Same(loggerFactory, scope.ServiceProvider.GetService<ILoggerFactory>());
                        Assert.NotSame(scopedExternalService, scope.ServiceProvider.GetService<Random>());

                        if (optionsLifetime == ServiceLifetime.Scoped)
                        {
                            Assert.NotSame(scopedExternalServiceFromContext, context.GetService<Random>());
                        }
                        else
                        {
                            // For singleton options or pool, scoped services cannot be obtained through the context
                            // service provider.
                            Assert.Same(scopedExternalServiceFromContext, context.GetService<Random>());
                        }
                    }

                    // Should not throw
                    loggerFactory.CreateLogger("MyLogger");
                }

                Assert.Throws<ObjectDisposedException>(() => loggerFactory.CreateLogger("MyLogger"));
            }
        }

        [ConditionalFact]
        public void GetService_throws_for_unknown_service_type()
        {
            using var context = new EarlyLearningCenter();

            Assert.Equal(
                CoreStrings.NoProviderConfiguredFailedToResolveService("System.Random"),
                Assert.Throws<InvalidOperationException>(() => context.GetService<Random>()).Message);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Resolve_singleton_application_service(bool autoResolve)
        {
            var serviceProvider = AddServiceResolutionContext(autoResolve)
                .AddSingleton<ApplicationService>()
                .BuildServiceProvider(validateScopes: true);

            ApplicationService applicationService;

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceResolutionContext>();
                applicationService = serviceProvider.GetRequiredService<ApplicationService>();
                Assert.Same(applicationService, scope.ServiceProvider.GetRequiredService<ApplicationService>());
                Assert.Same(applicationService, context.GetService<ApplicationService>());
                Assert.Same(applicationService, context.GetService<ApplicationService>());

                var singletonService = (TestSingletonService)context.GetService<IDbSetFinder>();
                Assert.Same(applicationService, singletonService.ApplicationService);

                var scopedService = (TestScopedService)context.GetService<IEntityGraphAttacher>();
                Assert.Same(applicationService, scopedService.ApplicationService);

                var transientService = (TestTransientService)context.GetService<ILazyLoader>();
                Assert.Same(applicationService, transientService.ApplicationService);
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceResolutionContext>();
                Assert.Same(applicationService, context.GetService<ApplicationService>());

                var singletonService = (TestSingletonService)context.GetService<IDbSetFinder>();
                Assert.Same(applicationService, singletonService.ApplicationService);

                var scopedService = (TestScopedService)context.GetService<IEntityGraphAttacher>();
                Assert.Same(applicationService, scopedService.ApplicationService);

                var transientService = (TestTransientService)context.GetService<ILazyLoader>();
                Assert.Same(applicationService, transientService.ApplicationService);
            }
        }

        [ConditionalTheory(Skip = "https://github.com/dotnet/runtime/issues/89109")]
        [InlineData(false)]
        [InlineData(true)]
        public void Resolve_scoped_application_service(bool autoResolve)
        {
            var serviceProvider = AddServiceResolutionContext(autoResolve)
                .AddScoped<ApplicationService>()
                .BuildServiceProvider(validateScopes: true);

            ApplicationService applicationService1;

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceResolutionContext>();
                applicationService1 = scope.ServiceProvider.GetRequiredService<ApplicationService>();
                Assert.Same(applicationService1, scope.ServiceProvider.GetRequiredService<ApplicationService>());
                Assert.Same(applicationService1, context.GetService<ApplicationService>());
                Assert.Same(applicationService1, context.GetService<ApplicationService>());

                var scopedService = (TestScopedService)context.GetService<IEntityGraphAttacher>();
                Assert.Same(applicationService1, scopedService.ApplicationService);

                var transientService = (TestTransientService)context.GetService<ILazyLoader>();
                Assert.Same(applicationService1, transientService.ApplicationService);

                var singletonService = (TestSingletonService)context.GetService<IDbSetFinder>();
                // Cannot resolve scoped service from root provider.
                Assert.Throws<InvalidOperationException>(() => singletonService.ApplicationService);
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceResolutionContext>();
                var applicationService2 = scope.ServiceProvider.GetRequiredService<ApplicationService>();
                Assert.NotSame(applicationService1, applicationService2);
                Assert.Same(applicationService2, context.GetService<ApplicationService>());

                var scopedService = (TestScopedService)context.GetService<IEntityGraphAttacher>();
                Assert.Same(applicationService2, scopedService.ApplicationService);

                var transientService = (TestTransientService)context.GetService<ILazyLoader>();
                Assert.Same(applicationService2, transientService.ApplicationService);

                var singletonService = (TestSingletonService)context.GetService<IDbSetFinder>();
                // Cannot resolve scoped service from root provider.
                Assert.Throws<InvalidOperationException>(() => singletonService.ApplicationService);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Resolve_transient_application_service(bool autoResolve)
        {
            var serviceProvider = AddServiceResolutionContext(autoResolve)
                .AddTransient<ApplicationService>()
                .BuildServiceProvider(validateScopes: true);

            ApplicationService applicationService1;

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceResolutionContext>();
                applicationService1 = scope.ServiceProvider.GetRequiredService<ApplicationService>();
                Assert.NotSame(applicationService1, scope.ServiceProvider.GetRequiredService<ApplicationService>());
                Assert.NotSame(applicationService1, context.GetService<ApplicationService>());
                Assert.NotSame(context.GetService<ApplicationService>(), context.GetService<ApplicationService>());

                var singletonService = (TestSingletonService)context.GetService<IDbSetFinder>();
                Assert.NotSame(applicationService1, singletonService.ApplicationService);

                var scopedService = (TestScopedService)context.GetService<IEntityGraphAttacher>();
                Assert.NotSame(applicationService1, scopedService.ApplicationService);

                var transientService = (TestTransientService)context.GetService<ILazyLoader>();
                Assert.NotSame(applicationService1, transientService.ApplicationService);
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ServiceResolutionContext>();
                Assert.NotSame(applicationService1, scope.ServiceProvider.GetRequiredService<ApplicationService>());
                Assert.NotSame(applicationService1, context.GetService<ApplicationService>());

                var singletonService = (TestSingletonService)context.GetService<IDbSetFinder>();
                Assert.NotSame(applicationService1, singletonService.ApplicationService);

                var scopedService = (TestScopedService)context.GetService<IEntityGraphAttacher>();
                Assert.NotSame(applicationService1, scopedService.ApplicationService);

                var transientService = (TestTransientService)context.GetService<ILazyLoader>();
                Assert.NotSame(applicationService1, transientService.ApplicationService);
            }
        }

        private static IServiceCollection AddServiceResolutionContext(bool autoResolve)
            => new ServiceCollection()
                .AddDbContext<ServiceResolutionContext>(
                    (p, b) =>
                    {
                        b = b.UseInMemoryDatabase(nameof(ServiceResolutionContext))
                            .ReplaceService<IDbSetFinder, TestSingletonService>()
                            .ReplaceService<IEntityGraphAttacher, TestScopedService>()
                            .ReplaceService<ILazyLoader, TestTransientService>();

                        if (autoResolve)
                        {
                            b.UseRootApplicationServiceProvider();
                        }
                        else
                        {
                            b.UseRootApplicationServiceProvider(p.GetService<ServiceProviderAccessor>().RootServiceProvider);
                        }
                    });

        private class TestSingletonService(ICoreSingletonOptions singletonOptions) : DbSetFinder
        {
            private readonly ICoreSingletonOptions _singletonOptions = singletonOptions;

            public ApplicationService ApplicationService
                => _singletonOptions.RootApplicationServiceProvider!.GetService<ApplicationService>();
        }

        private class TestScopedService(ICurrentDbContext currentContext, IEntityEntryGraphIterator graphIterator) : EntityGraphAttacher(graphIterator)
        {
            private readonly ICurrentDbContext _currentContext = currentContext;

            public ApplicationService ApplicationService
                => _currentContext.Context.GetService<ApplicationService>();
        }

        private class TestTransientService(ICurrentDbContext currentContext, IDiagnosticsLogger<DbLoggerCategory.Infrastructure> logger) : LazyLoader(currentContext, logger)
        {
            public ApplicationService ApplicationService
                => Context!.GetService<ApplicationService>();
        }

        private class ApplicationService;

        private class ServiceResolutionContext(DbContextOptions options) : DbContext(options);

        [ConditionalFact]
        public void Can_use_GetInfrastructure_with_inferred_generic_to_get_service_provider()
        {
            using var context = new EarlyLearningCenter();
            Assert.Same(
                context.GetService<IChangeDetector>(),
                context.GetInfrastructure().GetService<IChangeDetector>());
        }

        [ConditionalFact]
        public void Logger_factory_registered_on_internal_service_provider_is_not_disposed()
        {
            var serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider(validateScopes: true);

            var appServiceProvider
                = new ServiceCollection()
                    .AddDbContext<ConstructorTestContext1A>(
                        b => b.UseInMemoryDatabase("Scratch")
                            .UseInternalServiceProvider(serviceProvider))
                    .BuildServiceProvider(validateScopes: true);

            using var scope = appServiceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ConstructorTestContext1A>();
            var _ = context.Model;

            context = scope.ServiceProvider.GetRequiredService<ConstructorTestContext1A>();
            _ = context.Model;
        }

        [ConditionalFact]
        public void Each_context_gets_new_scoped_services()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            IServiceProvider contextServices;
            using (var context = new EarlyLearningCenter(serviceProvider))
            {
                contextServices = ((IInfrastructure<IServiceProvider>)context).Instance;
                Assert.Same(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }

            using (var context = new EarlyLearningCenter(serviceProvider))
            {
                Assert.NotSame(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }
        }

        [ConditionalFact]
        public void Each_context_gets_new_scoped_services_with_explicit_config()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            var options = new DbContextOptionsBuilder().UseInternalServiceProvider(serviceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .Options;

            IServiceProvider contextServices;
            using (var context = new DbContext(options))
            {
                contextServices = ((IInfrastructure<IServiceProvider>)context).Instance;
                Assert.Same(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }

            using (var context = new DbContext(options))
            {
                Assert.NotSame(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }
        }

        [ConditionalFact]
        public void Each_context_gets_new_scoped_services_with_implicit_services_and_explicit_config()
        {
            var options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            IServiceProvider contextServices;
            using (var context = new DbContext(options))
            {
                contextServices = ((IInfrastructure<IServiceProvider>)context).Instance;
                Assert.Same(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }

            using (var context = new DbContext(options))
            {
                Assert.NotSame(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }
        }

        [ConditionalFact]
        public void Scoped_provider_services_can_be_obtained_from_configuration()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            var options = new DbContextOptionsBuilder().UseInternalServiceProvider(serviceProvider)
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .Options;

            IDatabase database;
            IDatabaseCreator creator;

            using (var context = new DbContext(options))
            {
                database = context.GetService<IDatabase>();
                creator = context.GetService<IDatabaseCreator>();

                Assert.Same(database, context.GetService<IDatabase>());
                Assert.Same(creator, context.GetService<IDatabaseCreator>());
            }

            using (var context = new DbContext(options))
            {
                Assert.NotSame(database, context.GetService<IDatabase>());
                Assert.NotSame(creator, context.GetService<IDatabaseCreator>());
            }
        }

        [ConditionalFact]
        public void Scoped_provider_services_can_be_obtained_from_configuration_with_implicit_service_provider()
        {
            var options = new DbContextOptionsBuilder().UseInternalServiceProvider(null)
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .Options;

            IDatabase database;
            IDatabaseCreator creator;

            using (var context = new DbContext(options))
            {
                database = context.GetService<IDatabase>();
                creator = context.GetService<IDatabaseCreator>();

                Assert.Same(database, context.GetService<IDatabase>());
                Assert.Same(creator, context.GetService<IDatabaseCreator>());
            }

            using (var context = new DbContext(options))
            {
                Assert.NotSame(database, context.GetService<IDatabase>());
                Assert.NotSame(creator, context.GetService<IDatabaseCreator>());
            }
        }

        [ConditionalFact]
        public void Requesting_a_singleton_always_returns_same_instance()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = InMemoryTestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = InMemoryTestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices1.GetRequiredService<IDbSetSource>(), contextServices2.GetRequiredService<IDbSetSource>());
        }

        [ConditionalFact]
        public void Requesting_a_scoped_service_always_returns_same_instance_in_scope()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(provider);

            Assert.Same(contextServices.GetRequiredService<IStateManager>(), contextServices.GetRequiredService<IStateManager>());
        }

        [ConditionalFact]
        public void Requesting_a_scoped_service_always_returns_a_different_instance_in_a_different_scope()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();
            var contextServices1 = InMemoryTestHelpers.Instance.CreateContextServices(provider);
            var contextServices2 = InMemoryTestHelpers.Instance.CreateContextServices(provider);

            Assert.NotSame(contextServices1.GetRequiredService<IStateManager>(), contextServices2.GetRequiredService<IStateManager>());
        }

        [ConditionalFact]
        public void Default_services_are_registered_when_parameterless_constructor_used()
        {
            using var context = new EarlyLearningCenter();
            Assert.IsType<DbSetFinder>(context.GetService<IDbSetFinder>());
        }

        [ConditionalFact]
        public void Can_get_singleton_service_from_scoped_configuration()
        {
            using var context = new EarlyLearningCenter();
            Assert.IsType<StateManager>(context.GetService<IStateManager>());
        }

        [ConditionalFact]
        public void Can_start_with_custom_services_by_passing_in_base_service_provider()
        {
            var service = new FakeNavigationFixer();

            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<INavigationFixer>(service)
                .BuildServiceProvider(validateScopes: true);

            using var context = new EarlyLearningCenter(provider);
            Assert.Same(service, context.GetService<INavigationFixer>());
        }

        private class FakeNavigationFixer : INavigationFixer
        {
            public void StateChanging(InternalEntityEntry entry, EntityState newState)
                => throw new NotImplementedException();

            public void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
                => throw new NotImplementedException();

            public void FixupResolved(InternalEntityEntry entry, InternalEntityEntry duplicateEntry)
                => throw new NotImplementedException();

            public bool BeginDelayedFixup()
                => false;

            public void CompleteDelayedFixup()
            {
            }

            public void AbortDelayedFixup()
            {
            }

            public void NavigationReferenceChanged(
                InternalEntityEntry entry,
                INavigationBase navigationBase,
                object oldValue,
                object newValue)
                => throw new NotImplementedException();

            public void NavigationCollectionChanged(
                InternalEntityEntry entry,
                INavigationBase navigationBase,
                IEnumerable<object> added,
                IEnumerable<object> removed)
                => throw new NotImplementedException();

            public void KeyPropertyChanged(
                InternalEntityEntry entry,
                IProperty property,
                IEnumerable<IKey> containingPrincipalKeys,
                IEnumerable<IForeignKey> containingForeignKeys,
                object oldValue,
                object newValue)
                => throw new NotImplementedException();

            public void TrackedFromQuery(InternalEntityEntry entry)
                => throw new NotImplementedException();
        }

        [ConditionalFact]
        public void Required_low_level_services_are_added_if_needed()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var scope = serviceCollection.BuildServiceProvider(validateScopes: true).CreateScope();

            Assert.IsType<ScopedLoggerFactory>(scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
        }

        [ConditionalFact]
        public void Required_low_level_services_are_not_added_if_already_present()
        {
            var serviceCollection = new ServiceCollection();
            var loggerFactory = new ListLoggerFactory();

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var provider = serviceCollection.BuildServiceProvider(validateScopes: true);

            Assert.Same(loggerFactory, provider.GetRequiredService<ILoggerFactory>());
        }

        [ConditionalFact]
        public void Low_level_services_can_be_replaced_after_being_added()
        {
            var serviceCollection = new ServiceCollection();
            var loggerFactory = new ListLoggerFactory();

            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

            var provider = serviceCollection.BuildServiceProvider(validateScopes: true);

            Assert.Same(loggerFactory, provider.GetRequiredService<ILoggerFactory>());
        }

        [ConditionalFact]
        public void Can_replace_already_registered_service_with_new_service()
        {
            var service = new FakeNavigationFixer();
            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<INavigationFixer>(service)
                .BuildServiceProvider(validateScopes: true);

            using var context = new EarlyLearningCenter(provider);
            Assert.Same(service, context.GetService<INavigationFixer>());
        }

        [ConditionalFact]
        public void Can_set_known_singleton_services_using_instance_sugar()
        {
            var modelSource = (IModelSource)new FakeModelSource();

            var services = new ServiceCollection()
                .AddSingleton(modelSource);

            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using var context = new EarlyLearningCenter(provider);
            Assert.Same(modelSource, context.GetService<IModelSource>());
        }

        [ConditionalFact]
        public void Can_set_known_singleton_services_using_type_activation()
        {
            var services = new ServiceCollection()
                .AddSingleton<IModelSource, FakeModelSource>();

            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using var context = new EarlyLearningCenter(provider);
            Assert.IsType<FakeModelSource>(context.GetService<IModelSource>());
        }

        [ConditionalFact]
        public void Can_set_known_context_scoped_services_using_type_activation()
        {
            var services = new ServiceCollection()
                .AddScoped<IStateManager, FakeStateManager>();

            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using var context = new EarlyLearningCenter(provider);
            Assert.IsType<FakeStateManager>(context.GetService<IStateManager>());
        }

        [ConditionalFact]
        public void Replaced_services_are_scoped_appropriately()
        {
            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IModelSource, FakeModelSource>()
                .AddScoped<IStateManager, FakeStateManager>()
                .BuildServiceProvider(validateScopes: true);

            var context = new EarlyLearningCenter(provider);

            var modelSource = context.GetService<IModelSource>();

            context.Dispose();

            context = new EarlyLearningCenter(provider);

            var stateManager = context.GetService<IStateManager>();

            Assert.Same(stateManager, context.GetService<IStateManager>());

            Assert.Same(modelSource, context.GetService<IModelSource>());

            context.Dispose();

            context = new EarlyLearningCenter(provider);

            Assert.NotSame(stateManager, context.GetService<IStateManager>());

            Assert.Same(modelSource, context.GetService<IModelSource>());

            context.Dispose();
        }

        [ConditionalFact]
        public void Can_get_replaced_singleton_service_from_scoped_configuration()
        {
            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IEntityMaterializerSource, FakeEntityMaterializerSource>()
                .BuildServiceProvider(validateScopes: true);

            using var context = new EarlyLearningCenter(provider);
            Assert.IsType<FakeEntityMaterializerSource>(context.GetService<IEntityMaterializerSource>());
        }

        [ComplexType]
        private class Tag
        {
            public string Name { get; set; }

            [Required]
            public Stamp Stamp { get; set; }

            public string[] Notes { get; set; }
        }

        [ComplexType]
        private class Stamp
        {
            public Guid Code { get; set; }
        }

        private class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }

            [Required]
            public Tag Tag { get; set; }

            [Required]
            public Stamp Stamp { get; set; }

            public List<Product> Products { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }

            [Required]
            public Tag Tag { get; set; }

            [Required]
            public Stamp Stamp { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }
        }

        private class TheGu
        {
            public Guid Id { get; set; }
            public string ShirtColor { get; set; }
        }

        private class CategoryWithSentinel
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<ProductWithSentinel> Products { get; set; }
        }

        private class ProductWithSentinel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }

            public int CategoryId { get; set; }
            public CategoryWithSentinel Category { get; set; }
        }

        private class TheGuWithSentinel
        {
            public Guid Id { get; set; }
            public string ShirtColor { get; set; }
        }

        private class EarlyLearningCenter : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public EarlyLearningCenter()
            {
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider, DbContextOptions options)
                : base(options)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<TheGu> Gus { get; set; }
            public DbSet<ProductWithSentinel> ProductWithSentinels { get; set; }
            public DbSet<CategoryWithSentinel> CategoryWithSentinels { get; set; }
            public DbSet<TheGuWithSentinel> GuWithSentinels { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                    .UseInternalServiceProvider(_serviceProvider)
                    .EnableServiceProviderCaching(false);

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CategoryWithSentinel>().Property(e => e.Id).HasSentinel(IntSentinel);
                modelBuilder.Entity<ProductWithSentinel>().Property(e => e.Id).HasSentinel(IntSentinel);
                modelBuilder.Entity<TheGuWithSentinel>().Property(e => e.Id).HasSentinel(GuidSentinel);
            }
        }

        private class FakeEntityMaterializerSource(EntityMaterializerSourceDependencies dependencies) : EntityMaterializerSource(dependencies);

        private class FakeModelSource : IModelSource
        {
            public IModel GetModel(
                DbContext context,
                IConventionSetBuilder conventionSetBuilder)
                => new Model();

            public IModel GetModel(
                DbContext context,
                IConventionSetBuilder conventionSetBuilder,
                ModelDependencies modelDependencies)
                => new Model();

            public IModel GetModel(
                DbContext context,
                ModelCreationDependencies modelCreationDependencies,
                bool designTime)
                => new Model();
        }

        [ConditionalFact]
        public void Can_use_derived_context()
        {
            var singleton = new object[3];

            using (var context = new ConstructorTestContextWithOC1A())
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var context = new ConstructorTestContextWithOC1A())
            {
                // Singleton services not the same because service provider caching is off
                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<ILoggerFactory>());
                Assert.NotSame(singleton[2], context.GetService<IMemoryCache>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_with_external_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider(validateScopes: true);

            var loggerFactory = new WrappingLoggerFactory(appServiceProvider.GetService<ILoggerFactory>());
            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;

            using (var context = new ConstructorTestContextWithOC1B(loggerFactory, memoryCache))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
                Assert.Contains(DbLoggerCategory.Infrastructure.Name, loggerFactory.CreatedLoggers);
            }

            using (var context = new ConstructorTestContextWithOC1B(loggerFactory, memoryCache))
            {
                // Singleton internal services not the same because service provider caching is off
                Assert.NotSame(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_with_options()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .Options;

            var singleton = new object[3];

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                // Singleton services not the same because service provider caching is off
                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<ILoggerFactory>());
                Assert.NotSame(singleton[2], context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_with_options_and_external_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider(validateScopes: true);

            var loggerFactory = new WrappingLoggerFactory(appServiceProvider.GetService<ILoggerFactory>());
            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .UseLoggerFactory(loggerFactory)
                .UseMemoryCache(memoryCache)
                .Options;

            IInMemoryStoreCache singleton;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
                Assert.Contains(DbLoggerCategory.Infrastructure.Name, loggerFactory.CreatedLoggers);
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotSame(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_controlling_internal_services()
        {
            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[2];

            using (var context = new ConstructorTestContextWithOC2A(internalServiceProvider))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                Assert.Same(singleton[0], internalServiceProvider.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], internalServiceProvider.GetService<IMemoryCache>());
            }

            using (var context = new ConstructorTestContextWithOC2A(internalServiceProvider))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_controlling_internal_services_with_options()
        {
            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .UseInternalServiceProvider(internalServiceProvider)
                .Options;

            var singleton = new object[3];

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                Assert.Same(singleton[0], internalServiceProvider.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[2], internalServiceProvider.GetService<IMemoryCache>());
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_with_options_no_OnConfiguring()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .EnableServiceProviderCaching(false)
                .Options;

            var singleton = new object[3];

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<ILoggerFactory>());
                Assert.NotSame(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_with_options_and_external_services_no_OnConfiguring()
        {
            var appServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider(validateScopes: true);

            var loggerFactory = new WrappingLoggerFactory(appServiceProvider.GetService<ILoggerFactory>());
            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .UseLoggerFactory(loggerFactory)
                .UseMemoryCache(memoryCache)
                .EnableServiceProviderCaching(false)
                .Options;

            IInMemoryStoreCache singleton;

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
                Assert.Contains(DbLoggerCategory.Infrastructure.Name, loggerFactory.CreatedLoggers);
            }

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotSame(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_derived_context_controlling_internal_services_with_options_no_OnConfiguring()
        {
            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .UseInternalServiceProvider(internalServiceProvider)
                .Options;

            var singleton = new object[2];

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                Assert.Same(singleton[0], internalServiceProvider.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], internalServiceProvider.GetService<IMemoryCache>());
            }

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_non_derived_context_with_options()
        {
            var options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var singleton = new object[2];

            using (var context = new DbContext(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var context = new DbContext(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_non_derived_context_with_options_and_external_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider(validateScopes: true);

            var loggerFactory = new WrappingLoggerFactory(appServiceProvider.GetService<ILoggerFactory>());
            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            var options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(
                    InMemoryFixture.BuildServiceProvider(
                        new ServiceCollection()
                            .AddSingleton<ILoggerFactory>(loggerFactory)
                            .AddSingleton(memoryCache)))
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .Options;

            IInMemoryStoreCache singleton;

            using (var context = new DbContext(options))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
                Assert.Contains(DbLoggerCategory.Infrastructure.Name, loggerFactory.CreatedLoggers);
            }

            using (var context = new DbContext(options))
            {
                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_use_non_derived_context_controlling_internal_services_with_options()
        {
            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(internalServiceProvider)
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .Options;

            var singleton = new object[2];

            using (var context = new DbContext(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                Assert.Same(singleton[0], internalServiceProvider.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], internalServiceProvider.GetService<IMemoryCache>());
            }

            using (var context = new DbContext(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_add_derived_context(bool useInterface)
        {
            var serviceCollection = new ServiceCollection();

            if (useInterface)
            {
                serviceCollection.AddDbContext<IConstructorTestContextWithOC1A, ConstructorTestContextWithOC1A>();
            }
            else
            {
                serviceCollection.AddDbContext<ConstructorTestContextWithOC1A>();
            }

            var appServiceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context1 = useInterface
                    ? (ConstructorTestContextWithOC1A)serviceScope.ServiceProvider.GetService<IConstructorTestContextWithOC1A>()
                    : serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                if (useInterface)
                {
                    Assert.Same(context1, serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>());
                }

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            Assert.Throws<ObjectDisposedException>(() => context1.Model);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context2 = useInterface
                    ? (ConstructorTestContextWithOC1A)serviceScope.ServiceProvider.GetService<IConstructorTestContextWithOC1A>()
                    : serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                // Singleton services not the same because service provider caching is off
                Assert.NotSame(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.NotSame(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.NotSame(context1, context2);
            Assert.Throws<ObjectDisposedException>(() => context2.Model);
        }

        [ConditionalFact]
        public void Can_add_derived_context_with_external_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC1B>()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider(validateScopes: true);

            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1B>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1B>();

                // Singleton internal services not the same because service provider caching is off
                Assert.NotSame(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
            }
        }

        private class SomeAppService;

        private class SomeScopedAppService;

        [ConditionalFact]
        public void Can_add_derived_context_with_options()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                .AddSingleton<SomeAppService>()
                .AddScoped<SomeScopedAppService>()
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];
            SomeAppService appSingleton;
            SomeScopedAppService appScoped;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                appSingleton = context.GetService<SomeAppService>();
                appScoped = context.GetService<SomeScopedAppService>();
                Assert.NotNull(appSingleton);
                Assert.NotNull(appScoped);

                Assert.Same(appSingleton, serviceScope.ServiceProvider.GetService<SomeAppService>());
                Assert.Same(appScoped, serviceScope.ServiceProvider.GetService<SomeScopedAppService>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());

                var scoped = context.GetService<SomeScopedAppService>();
                Assert.NotSame(appScoped, scoped);
                Assert.Same(scoped, serviceScope.ServiceProvider.GetService<SomeScopedAppService>());

                Assert.Same(appSingleton, context.GetService<SomeAppService>());
                Assert.Same(appSingleton, serviceScope.ServiceProvider.GetService<SomeAppService>());
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_with_options_and_external_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    b => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                .BuildServiceProvider(validateScopes: true);

            ILoggerFactory loggerFactory;
            IMemoryCache memoryCache;

            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(loggerFactory = context.GetService<ILoggerFactory>());
                Assert.NotNull(memoryCache = context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                // Singleton services not the same because service provider caching is off
                Assert.NotSame(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.NotSame(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_controlling_internal_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddLogging(l => l.AddProvider(new MyLoggerProvider()))
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC2A>()
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC2A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC2A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_controlling_internal_services_with_options()
        {
            var internalServiceProvider = new ServiceCollection()
                .AddLogging(l => l.AddProvider(new MyLoggerProvider()))
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    b => b.UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(internalServiceProvider))
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_add_derived_context_one_service_provider_with_options(bool useInterface)
        {
            var serviceCollection = new ServiceCollection().AddEntityFrameworkInMemoryDatabase();

            if (useInterface)
            {
                serviceCollection.AddDbContext<IConstructorTestContextWithOC3A, ConstructorTestContextWithOC3A>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p));
            }
            else
            {
                serviceCollection.AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p));
            }

            var appServiceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            var singleton = new object[4];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = useInterface
                    ? (ConstructorTestContextWithOC3A)serviceScope.ServiceProvider.GetService<IConstructorTestContextWithOC3A>()
                    : serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = useInterface
                    ? (ConstructorTestContextWithOC3A)serviceScope.ServiceProvider.GetService<IConstructorTestContextWithOC3A>()
                    : serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_add_derived_context_one_service_provider_with_options_and_external_services(bool singletonOptions)
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p),
                    ServiceLifetime.Scoped,
                    singletonOptions ? ServiceLifetime.Singleton : ServiceLifetime.Scoped)
                .BuildServiceProvider(); // No scope validation; legacy test that resolves scoped options from singleton

            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                if (singletonOptions)
                {
                    Assert.Same(options, context.GetService<IDbContextOptions>());
                }
                else
                {
                    Assert.NotSame(options, context.GetService<IDbContextOptions>());
                }
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_with_options_no_OnConfiguring()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(
                    b => b.EnableServiceProviderCaching(false)
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_with_options_and_external_services_no_OnConfiguring()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(
                    b => b.EnableServiceProviderCaching(false)
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                .BuildServiceProvider(validateScopes: true);

            ILoggerFactory loggerFactory;
            IMemoryCache memoryCache;
            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(loggerFactory = context.GetService<ILoggerFactory>());
                Assert.NotNull(memoryCache = context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotSame(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.NotSame(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_controlling_internal_services_with_options_no_OnConfiguring()
        {
            var internalServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(
                    b => b.UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(internalServiceProvider))
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_one_provider_with_options_no_OnConfiguring()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_derived_context_one_provider_with_options_and_external_services_no_OnConfiguring()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_non_derived_context_with_options()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<DbContext>(
                    b => b.EnableServiceProviderCaching(false)
                        .UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[4];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_non_derived_context_with_options_and_external_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<DbContext>(
                    (p, b) => b.EnableServiceProviderCaching(false)
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseMemoryCache(p.GetService<IMemoryCache>())
                        .UseLoggerFactory(p.GetService<ILoggerFactory>()))
                .AddMemoryCache()
                .AddLogging()
                .BuildServiceProvider(validateScopes: true);

            var memoryCache = appServiceProvider.GetService<IMemoryCache>();

            IDbContextOptions options;
            IInMemoryStoreCache singleton;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotSame(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotSame(options, context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalFact]
        public void Can_add_non_derived_context_controlling_internal_services_with_options()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            var singleton = new object[3];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[2] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());
                Assert.NotSame(singleton[2], context.GetService<IDbContextOptions>());
            }
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Can_add_derived_context_as_singleton(bool addSingletonFirst, bool useDbContext)
        {
            var appServiceProvider = useDbContext
                ? new ServiceCollection()
                    .AddDbContext<ConstructorTestContextWithOC1A>(ServiceLifetime.Singleton)
                    .BuildServiceProvider(validateScopes: true)
                : (addSingletonFirst
                    ? new ServiceCollection()
                        .AddSingleton<ConstructorTestContextWithOC1A>()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider(validateScopes: true)
                    : new ServiceCollection()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .AddSingleton<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider(validateScopes: true));

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [ConditionalFact]
        public void Throws_when_used_with_parameterless_constructor_context()
        {
            var serviceCollection = new ServiceCollection();

            Assert.Equal(
                CoreStrings.DbContextMissingConstructor(nameof(ConstructorTestContextWithOC1A)),
                Assert.Throws<ArgumentException>(
                    () => serviceCollection.AddDbContext<ConstructorTestContextWithOC1A>(
                        _ => { })).Message);

            Assert.Equal(
                CoreStrings.DbContextMissingConstructor(nameof(ConstructorTestContextWithOC1A)),
                Assert.Throws<ArgumentException>(
                    () => serviceCollection.AddDbContext<ConstructorTestContextWithOC1A>(
                        (_, __) => { })).Message);
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Can_add_derived_context_as_singleton_controlling_internal_services(bool addSingletonFirst, bool useDbContext)
        {
            var appServiceProvider = useDbContext
                ? new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<ConstructorTestContextWithOC3A>(
                        (p, b) => b
                            .UseInternalServiceProvider(p)
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)),
                        ServiceLifetime.Singleton)
                    .BuildServiceProvider() // No scope validation; legacy test that resolves scoped options from singleton
                : (addSingletonFirst
                    ? new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddSingleton<ConstructorTestContextWithOC3A>()
                        .AddDbContext<ConstructorTestContextWithOC3A>(
                            (p, b) => b
                                .UseInternalServiceProvider(p)
                                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                        .BuildServiceProvider() // No scope validation; legacy test that resolves scoped options from singleton
                    : new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddDbContext<ConstructorTestContextWithOC3A>(
                            (p, b) => b
                                .UseInternalServiceProvider(p)
                                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                        .AddSingleton<ConstructorTestContextWithOC3A>()
                        .BuildServiceProvider()); // No scope validation; legacy test that resolves scoped options from singleton

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [ConditionalTheory]
        [InlineData(true, false, ServiceLifetime.Scoped)]
        [InlineData(false, false, ServiceLifetime.Scoped)]
        [InlineData(true, true, ServiceLifetime.Transient)]
        [InlineData(true, true, ServiceLifetime.Scoped)]
        [InlineData(true, true, ServiceLifetime.Singleton)]
        public void Can_add_derived_context_as_transient(bool addTransientFirst, bool useDbContext, ServiceLifetime optionsLifetime)
        {
            var appServiceProvider = useDbContext
                ? new ServiceCollection()
                    .AddDbContext<ConstructorTestContextWithOC1A>(ServiceLifetime.Transient, optionsLifetime)
                    .BuildServiceProvider(validateScopes: true)
                : (addTransientFirst
                    ? new ServiceCollection()
                        .AddTransient<ConstructorTestContextWithOC1A>()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider(validateScopes: true)
                    : new ServiceCollection()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .AddTransient<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider(validateScopes: true));

            var singleton = new object[3];
            DbContextOptions options = null;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();
                var context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                if (useDbContext)
                {
                    options = serviceScope.ServiceProvider.GetService<DbContextOptions>();

                    if (optionsLifetime != ServiceLifetime.Transient)
                    {
                        Assert.Same(options, serviceScope.ServiceProvider.GetService<DbContextOptions>());
                    }
                    else
                    {
                        Assert.NotSame(options, serviceScope.ServiceProvider.GetService<DbContextOptions>());
                    }
                }

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                // Singleton services not the same because service provider caching is off
                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<ILoggerFactory>());
                Assert.NotSame(singleton[2], context.GetService<IMemoryCache>());

                if (useDbContext)
                {
                    if (optionsLifetime == ServiceLifetime.Singleton)
                    {
                        Assert.Same(options, serviceScope.ServiceProvider.GetService<DbContextOptions>());
                    }
                    else
                    {
                        Assert.NotSame(options, serviceScope.ServiceProvider.GetService<DbContextOptions>());
                    }
                }

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Can_add_derived_context_as_transient_controlling_internal_services(bool addTransientFirst, bool useDbContext)
        {
            var appServiceProvider = useDbContext
                ? new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<ConstructorTestContextWithOC3A>(
                        (p, b) => b
                            .UseInternalServiceProvider(p)
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)),
                        ServiceLifetime.Transient)
                    .BuildServiceProvider(validateScopes: true)
                : (addTransientFirst
                    ? new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddTransient<ConstructorTestContextWithOC3A>()
                        .AddDbContext<ConstructorTestContextWithOC3A>(
                            (p, b) => b
                                .UseInternalServiceProvider(p)
                                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                        .BuildServiceProvider(validateScopes: true)
                    : new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddDbContext<ConstructorTestContextWithOC3A>(
                            (p, b) => b
                                .UseInternalServiceProvider(p)
                                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                        .AddTransient<ConstructorTestContextWithOC3A>()
                        .BuildServiceProvider(validateScopes: true));

            var singleton = new object[2];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();
                var context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_add_non_derived_context_as_singleton(bool addSingletonFirst)
        {
            var appServiceProvider = addSingletonFirst
                ? new ServiceCollection()
                    .AddSingleton<DbContext>()
                    .AddDbContext<DbContext>(
                        b => b.EnableServiceProviderCaching(false)
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                    .BuildServiceProvider() // No scope validation; legacy test that resolves scoped options from singleton
                : new ServiceCollection()
                    .AddDbContext<DbContext>(
                        b => b.EnableServiceProviderCaching(false)
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                    .AddSingleton<DbContext>()
                    .BuildServiceProvider(); // No scope validation; legacy test that resolves scoped options from singleton

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void Can_add_non_derived_context_as_singleton_controlling_internal_services(bool addSingletonFirst, bool addEfFirst)
        {
            var serviceCollection = new ServiceCollection();

            if (addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            if (addSingletonFirst)
            {
                serviceCollection
                    .AddSingleton<DbContext>()
                    .AddDbContext<DbContext>(
                        (p, b) => b
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                            .UseInternalServiceProvider(p));
            }
            else
            {
                serviceCollection
                    .AddDbContext<DbContext>(
                        (p, b) => b
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                            .UseInternalServiceProvider(p))
                    .AddSingleton<DbContext>();
            }

            if (!addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            var appServiceProvider = serviceCollection.BuildServiceProvider();

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_add_non_derived_context_as_transient(bool addTransientFirst)
        {
            var appServiceProvider = addTransientFirst
                ? new ServiceCollection()
                    .AddTransient<DbContext>()
                    .AddDbContext<DbContext>(
                        b => b.EnableServiceProviderCaching(false)
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                    .BuildServiceProvider(validateScopes: true)
                : new ServiceCollection()
                    .AddDbContext<DbContext>(
                        b => b.EnableServiceProviderCaching(false)
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                    .AddTransient<DbContext>()
                    .BuildServiceProvider(validateScopes: true);

            var singleton = new object[2];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<DbContext>();
                var context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotSame(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.NotSame(singleton[1], context.GetService<IMemoryCache>());

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [ConditionalTheory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void Can_add_non_derived_context_as_transient_controlling_internal_services(bool addTransientFirst, bool addEfFirst)
        {
            var serviceCollection = new ServiceCollection();

            if (addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            if (addTransientFirst)
            {
                serviceCollection
                    .AddTransient<DbContext>()
                    .AddDbContext<DbContext>(
                        (p, b) => b
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                            .UseInternalServiceProvider(p));
            }
            else
            {
                serviceCollection
                    .AddDbContext<DbContext>(
                        (p, b) => b
                            .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                            .UseInternalServiceProvider(p))
                    .AddTransient<DbContext>();
            }

            if (!addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            var appServiceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            var singleton = new object[2];

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<DbContext>();
                var context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<IMemoryCache>());

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [ConditionalFact]
        public void Can_use_logger_before_context_exists_and_after_disposed()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                Assert.NotNull(serviceScope.ServiceProvider.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(context.Model);
                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());
            }
        }

        [ConditionalFact]
        public void Can_use_logger_before_context_exists_and_after_disposed_when_logger_factory_replaced()
        {
            WrappingLoggerFactory loggerFactory = null;
            Log.Clear();

            var appServiceProvider = new ServiceCollection()
                .AddLogging(l => l.AddProvider(new MyLoggerProvider()))
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>(
                    (p, b) =>
                        b.UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .EnableServiceProviderCaching(false)
                            .UseLoggerFactory(loggerFactory = new WrappingLoggerFactory(p.GetService<ILoggerFactory>())))
                .BuildServiceProvider(validateScopes: true);

            Assert.Null(loggerFactory);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(serviceScope.ServiceProvider.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                var redundantServicesWarning = Log.Single(e => e.Id.Id == CoreEventId.RedundantAddServicesCallWarning.Id);
                Assert.Equal(LogLevel.Warning, redundantServicesWarning.Level);

                Assert.NotNull(context.Model);
                Assert.NotNull(context.GetService<IDiagnosticsLogger<DbLoggerCategory.Infrastructure>>());

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal(3, loggerFactory.CreatedLoggers.Count(n => n == DbLoggerCategory.Infrastructure.Name));
            }

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal(3, loggerFactory.CreatedLoggers.Count(n => n == DbLoggerCategory.Infrastructure.Name));
        }

        [ConditionalFact]
        public void Can_use_memory_cache_before_context_exists_and_after_disposed()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            var memoryCache = appServiceProvider.GetService<IMemoryCache>();
            Assert.NotNull(memoryCache);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(context.Model);
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
            }

            Assert.Same(memoryCache, appServiceProvider.GetService<IMemoryCache>());
        }

        [ConditionalFact]
        public void Can_use_memory_cache_before_context_exists_and_after_disposed_when_logger_factory_replaced()
        {
            var replacecMemoryCache = new MemoryCache(new MemoryCacheOptions());
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<DbContext>(
                    (p, b) =>
                        b.UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                            .EnableServiceProviderCaching(false)
                            .UseMemoryCache(replacecMemoryCache))
                .BuildServiceProvider(validateScopes: true);

            var memoryCache = appServiceProvider.GetService<IMemoryCache>();
            Assert.NotSame(replacecMemoryCache, memoryCache);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(context.Model);
                Assert.Same(replacecMemoryCache, context.GetService<IMemoryCache>());
            }

            Assert.Same(memoryCache, appServiceProvider.GetService<IMemoryCache>());
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_EF_services()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithSets>()
                .UseInternalServiceProvider(new ServiceCollection().BuildServiceProvider(validateScopes: true))
                .Options;

            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithSets(options)).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_EF_services()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithSets>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(
                    () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithSets>()).Message);
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_EF_services_and_no_sets()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(new ServiceCollection().BuildServiceProvider(validateScopes: true))
                .Options;

            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContext1A(options)).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_EF_services_and_no_sets()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(
                    () => serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>()).Message);
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_provider()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            var options = new DbContextOptionsBuilder<ConstructorTestContextWithSets>()
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using var context = new ConstructorTestContextWithSets(options);
            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_provider()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var appServiceProvider = serviceCollection
                .AddDbContext<ConstructorTestContextWithSets>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithSets>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_provider_and_no_sets()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using var context = new ConstructorTestContext1A(options);
            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_provider_and_no_sets()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var appServiceProvider = serviceCollection
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_EF_services_because_parameterless_constructor()
        {
            using var context = new ConstructorTestContextNoConfigurationWithSets();
            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_EF_services_because_parameterless_constructor()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextNoConfigurationWithSets>()
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextNoConfigurationWithSets>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_EF_services_and_no_sets_because_parameterless_constructor()
        {
            using var context = new ConstructorTestContextNoConfiguration();
            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_EF_services_and_no_sets_because_parameterless_constructor()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextNoConfiguration>()
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextNoConfiguration>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        [ConditionalFact]
        public void Can_replace_services_in_OnConfiguring()
        {
            object replacedSingleton;
            object replacedScoped;
            object replacedProviderService;

            using (var context = new ReplaceServiceContext1())
            {
                Assert.NotNull(replacedSingleton = context.GetService<IModelCustomizer>());
                Assert.IsType<CustomModelCustomizer>(replacedSingleton);

                Assert.NotNull(replacedScoped = context.GetService<IValueGeneratorSelector>());
                Assert.IsType<CustomInMemoryValueGeneratorSelector>(replacedScoped);

                Assert.NotNull(replacedProviderService = context.GetService<IInMemoryTableFactory>());
                Assert.IsType<CustomInMemoryTableFactory>(replacedProviderService);
            }

            using (var context = new ReplaceServiceContext1())
            {
                Assert.NotSame(replacedSingleton, context.GetService<IModelCustomizer>());
                Assert.NotSame(replacedScoped, context.GetService<IValueGeneratorSelector>());
                Assert.NotSame(replacedProviderService, context.GetService<IInMemoryTableFactory>());
            }
        }

        private class ReplaceServiceContext1 : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                    .ReplaceService<IValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                    .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                    .EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ConfigureWarnings(w => w.Default(WarningBehavior.Throw));
        }

        private class CustomParameterBindingFactory : IParameterBindingFactory
        {
            public bool CanBind(Type parameterType, string parameterName)
                => false;

            public ParameterBinding Bind(IMutableEntityType entityType, Type parameterType, string parameterName)
                => throw new NotImplementedException();

            public ParameterBinding Bind(IConventionEntityType entityType, Type parameterType, string parameterName)
                => throw new NotImplementedException();

            public ParameterBinding Bind(IReadOnlyEntityType entityType, Type parameterType, string parameterName)
                => throw new NotImplementedException();
        }

        private class CustomParameterBindingFactory2 : IParameterBindingFactory
        {
            public bool CanBind(Type parameterType, string parameterName)
                => false;

            public ParameterBinding Bind(IMutableEntityType entityType, Type parameterType, string parameterName)
                => throw new NotImplementedException();

            public ParameterBinding Bind(IConventionEntityType entityType, Type parameterType, string parameterName)
                => throw new NotImplementedException();

            public ParameterBinding Bind(IReadOnlyEntityType entityType, Type parameterType, string parameterName)
                => throw new NotImplementedException();
        }

        private class CustomModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies);

        private class CustomModelCustomizer2(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies);

        private class CustomInMemoryValueGeneratorSelector(
            ValueGeneratorSelectorDependencies dependencies,
            IInMemoryDatabase inMemoryDatabase) : InMemoryValueGeneratorSelector(dependencies, inMemoryDatabase);

        private class CustomInMemoryTableFactory(ILoggingOptions loggingOptions, IInMemorySingletonOptions options) : InMemoryTableFactory(loggingOptions, options);

        [ConditionalFact]
        public void Can_replace_services_in_passed_options()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                .ReplaceService<IValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                .Options;

            object replacedSingleton;
            object replacedScoped;
            object replacedProviderService;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(replacedSingleton = context.GetService<IModelCustomizer>());
                Assert.IsType<CustomModelCustomizer>(replacedSingleton);
                Assert.Single(context.GetService<IEnumerable<IModelCustomizer>>());

                Assert.NotNull(replacedScoped = context.GetService<IValueGeneratorSelector>());
                Assert.IsType<CustomInMemoryValueGeneratorSelector>(replacedScoped);
                Assert.Single(context.GetService<IEnumerable<IValueGeneratorSelector>>());

                Assert.NotNull(replacedProviderService = context.GetService<IInMemoryTableFactory>());
                Assert.IsType<CustomInMemoryTableFactory>(replacedProviderService);
                Assert.Single(context.GetService<IEnumerable<IInMemoryTableFactory>>());
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                // Singleton internal services not the same because service provider caching is off
                Assert.NotSame(replacedSingleton, context.GetService<IModelCustomizer>());
                Assert.NotSame(replacedScoped, context.GetService<IValueGeneratorSelector>());
                Assert.NotSame(replacedProviderService, context.GetService<IInMemoryTableFactory>());
            }
        }

        [ConditionalFact]
        public void Can_replace_services_using_AddDbContext()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    b => b.ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                        .ReplaceService<IValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                        .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw)))
                .BuildServiceProvider(validateScopes: true);

            object replacedSingleton;
            object replacedScoped;
            object replacedProviderService;

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(replacedSingleton = context.GetService<IModelCustomizer>());
                Assert.IsType<CustomModelCustomizer>(replacedSingleton);
                Assert.Single(context.GetService<IEnumerable<IModelCustomizer>>());

                Assert.NotNull(replacedScoped = context.GetService<IValueGeneratorSelector>());
                Assert.IsType<CustomInMemoryValueGeneratorSelector>(replacedScoped);
                Assert.Single(context.GetService<IEnumerable<IValueGeneratorSelector>>());

                Assert.NotNull(replacedProviderService = context.GetService<IInMemoryTableFactory>());
                Assert.IsType<CustomInMemoryTableFactory>(replacedProviderService);
                Assert.Single(context.GetService<IEnumerable<IInMemoryTableFactory>>());
            }

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                // Singleton services not the same because service provider caching is off
                Assert.NotSame(replacedSingleton, context.GetService<IModelCustomizer>());
                Assert.NotSame(replacedScoped, context.GetService<IValueGeneratorSelector>());
                Assert.NotSame(replacedProviderService, context.GetService<IInMemoryTableFactory>());
            }
        }

        [ConditionalFact]
        public void Can_replace_all_multiple_registrations()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ReplaceService<IParameterBindingFactory, CustomParameterBindingFactory>()
                .Options;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                var replacedServices = context.GetService<IEnumerable<IParameterBindingFactory>>().ToList();
                Assert.Equal(3, replacedServices.Count);

                foreach (var replacedService in replacedServices)
                {
                    Assert.IsType<CustomParameterBindingFactory>(replacedService);
                }
            }
        }

        [ConditionalFact]
        public void Can_replace_specific_implementation_of_multiple_registrations()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ReplaceService<IParameterBindingFactory, EntityTypeParameterBindingFactory, CustomParameterBindingFactory>()
                .Options;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                var replacedServices = context
                    .GetService<IEnumerable<IParameterBindingFactory>>()
                    .OrderBy(e => e.GetType().Name)
                    .ToList();

                Assert.Collection(
                    replacedServices,
                    t => Assert.IsType<ContextParameterBindingFactory>(t),
                    t => Assert.IsType<CustomParameterBindingFactory>(t),
                    t => Assert.IsType<LazyLoaderParameterBindingFactory>(t));
            }
        }

        [ConditionalFact]
        public void Can_replace_specific_implementation_of_single_registration()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ReplaceService<IModelCustomizer, ModelCustomizer, CustomModelCustomizer>()
                .Options;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                var replacedServices = context.GetService<IEnumerable<IModelCustomizer>>().ToList();
                Assert.Single(replacedServices);
                Assert.IsType<CustomModelCustomizer>(replacedServices.Single());
            }
        }

        [ConditionalFact]
        public void Can_replace_specific_implementation_and_all_others()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ReplaceService<IParameterBindingFactory, EntityTypeParameterBindingFactory, CustomParameterBindingFactory>()
                .ReplaceService<IParameterBindingFactory, CustomParameterBindingFactory2>()
                .Options;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                var replacedServices = context.GetService<IEnumerable<IParameterBindingFactory>>().ToList();
                Assert.Equal(3, replacedServices.Count);

                Assert.Equal(2, replacedServices.Count(t => t is CustomParameterBindingFactory2));
                Assert.Single(replacedServices.Where(t => t is CustomParameterBindingFactory));
            }
        }

        [ConditionalFact]
        public void Throws_replacing_services_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using var context = new ReplaceServiceContext2();
            Assert.Equal(
                CoreStrings.InvalidReplaceService(
                    nameof(DbContextOptionsBuilder.ReplaceService), nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        private class ReplaceServiceContext2 : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                    .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkInMemoryDatabase()
                            .BuildServiceProvider(validateScopes: true))
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        [ConditionalFact]
        public void Throws_replacing_services_in_options_when_UseInternalServiceProvider()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider(validateScopes: true))
                .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                .Options;

            Assert.Equal(
                CoreStrings.InvalidReplaceService(
                    nameof(DbContextOptionsBuilder.ReplaceService), nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithOC3A(options)).Message);
        }

        [ConditionalFact]
        public void Throws_replacing_services_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ConfigureWarnings(w => w.Default(WarningBehavior.Throw))
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            Assert.Equal(
                CoreStrings.InvalidReplaceService(
                    nameof(DbContextOptionsBuilder.ReplaceService), nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                Assert.Throws<InvalidOperationException>(
                    () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>()).Message);
        }

        [ConditionalFact]
        public void Throws_setting_LoggerFactory_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using var context = new SetLoggerFactoryContext();
            Assert.Equal(
                CoreStrings.InvalidUseService(
                    nameof(DbContextOptionsBuilder.UseLoggerFactory),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                    nameof(ILoggerFactory)),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        private class SetLoggerFactoryContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseLoggerFactory(new ListLoggerFactory())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkInMemoryDatabase()
                            .BuildServiceProvider(validateScopes: true))
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        [ConditionalFact]
        public void Throws_adding_singleton_interceptors_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using var context = new SingletonInterceptorFactoryContext();
            Assert.Equal(
                CoreStrings.InvalidUseService(
                    nameof(DbContextOptionsBuilder.AddInterceptors),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                    nameof(ISingletonInterceptor)),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        private class SingletonInterceptorFactoryContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .AddInterceptors(new DummyInterceptor())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkInMemoryDatabase()
                            .BuildServiceProvider(validateScopes: true))
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        private class DummyInterceptor : ISingletonInterceptor;

        [ConditionalFact]
        public void Throws_setting_LoggerFactory_in_options_when_UseInternalServiceProvider()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider(validateScopes: true))
                .UseLoggerFactory(new ListLoggerFactory())
                .Options;

            Assert.Equal(
                CoreStrings.InvalidUseService(
                    nameof(DbContextOptionsBuilder.UseLoggerFactory),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                    nameof(ILoggerFactory)),
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithOC3A(options)).Message);
        }

        [ConditionalFact]
        public void Throws_setting_LoggerFactory_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.UseLoggerFactory(new ListLoggerFactory())
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            Assert.Equal(
                CoreStrings.InvalidUseService(
                    nameof(DbContextOptionsBuilder.UseLoggerFactory),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                    nameof(ILoggerFactory)),
                Assert.Throws<InvalidOperationException>(
                    () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>()).Message);
        }

        [ConditionalFact]
        public void Throws_setting_MemoryCache_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using var context = new SetMemoryCacheContext();
            Assert.Equal(
                CoreStrings.InvalidUseService(
                    nameof(DbContextOptionsBuilder.UseMemoryCache),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                    nameof(IMemoryCache)),
                Assert.Throws<InvalidOperationException>(() => context.Model).Message);
        }

        private class SetMemoryCacheContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseMemoryCache(new FakeMemoryCache())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkInMemoryDatabase()
                            .BuildServiceProvider(validateScopes: true))
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        [ConditionalFact]
        public void Throws_setting_MemoryCache_in_options_when_UseInternalServiceProvider()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(
                    new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider(validateScopes: true))
                .UseMemoryCache(new FakeMemoryCache())
                .Options;

            Assert.Equal(
                CoreStrings.InvalidUseService(
                    nameof(DbContextOptionsBuilder.UseMemoryCache),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                    nameof(IMemoryCache)),
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithOC3A(options)).Message);
        }

        [ConditionalFact]
        public void Throws_setting_MemoryCache_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var appServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.UseMemoryCache(new FakeMemoryCache())
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            Assert.Equal(
                CoreStrings.InvalidUseService(
                    nameof(DbContextOptionsBuilder.UseMemoryCache),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                    nameof(IMemoryCache)),
                Assert.Throws<InvalidOperationException>(
                    () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>()).Message);
        }

        private class FakeMemoryCache : IMemoryCache
        {
            public void Dispose()
            {
            }

            public bool TryGetValue(object key, out object value)
                => throw new NotImplementedException();

            public ICacheEntry CreateEntry(object key)
                => throw new NotImplementedException();

            public void Remove(object key)
            {
            }
        }

        [ConditionalFact]
        public void Throws_changing_sensitive_data_logging_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using (var context = new ChangeSdlCacheContext(false))
            {
                Assert.NotNull(context.Model);
            }

            using (var context = new ChangeSdlCacheContext(true))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class ChangeSdlCacheContext(bool on) : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider(validateScopes: true);

            private readonly bool _on = on;

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableSensitiveDataLogging(_on)
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        [ConditionalFact]
        public void Throws_changing_sensitive_data_logging_in_options_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            using (var context = new ConstructorTestContextWithOC3A(
                       new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                           .UseInMemoryDatabase(Guid.NewGuid().ToString())
                           .UseInternalServiceProvider(serviceProvider)
                           .EnableSensitiveDataLogging()
                           .Options))
            {
                Assert.NotNull(context.Model);
            }

            using (var context = new ConstructorTestContextWithOC3A(
                       new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                           .UseInMemoryDatabase(Guid.NewGuid().ToString())
                           .UseInternalServiceProvider(serviceProvider)
                           .EnableSensitiveDataLogging(false)
                           .Options))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public void Throws_changing_sensitive_data_logging_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            using (var serviceScope = new ServiceCollection()
                       .AddDbContext<ConstructorTestContextWithOC3A>(
                           (p, b) => b.EnableSensitiveDataLogging()
                               .UseInMemoryDatabase(Guid.NewGuid().ToString())
                               .UseInternalServiceProvider(serviceProvider))
                       .BuildServiceProvider(validateScopes: true)
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(context.Model);
            }

            using (var serviceScope = new ServiceCollection()
                       .AddDbContext<ConstructorTestContextWithOC3A>(
                           (p, b) => b.EnableSensitiveDataLogging(false)
                               .UseInMemoryDatabase(Guid.NewGuid().ToString())
                               .UseInternalServiceProvider(serviceProvider))
                       .BuildServiceProvider(validateScopes: true)
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public void Throws_changing_warnings_default_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Default(WarningBehavior.Ignore)))
            {
                Assert.NotNull(context.Model);
            }

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Default(WarningBehavior.Log)))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public void Throws_changing_warnings_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Throw(CoreEventId.QueryExecutionPlanned)))
            {
                Assert.NotNull(context.Model);
            }

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Log(CoreEventId.QueryExecutionPlanned)))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class ChangeWarningsCacheContext(
            IServiceProvider serviceProvider,
            Action<WarningsConfigurationBuilder> configAction) : DbContext
        {
            private readonly IServiceProvider _serviceProvider = serviceProvider;
            private readonly Action<WarningsConfigurationBuilder> _configAction = configAction;

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .ConfigureWarnings(_configAction)
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString());
        }

        [ConditionalFact]
        public void Throws_changing_warnings_config_in_options_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            using (var context = new ConstructorTestContextWithOC3A(
                       new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                           .UseInMemoryDatabase(Guid.NewGuid().ToString())
                           .UseInternalServiceProvider(serviceProvider)
                           .ConfigureWarnings(b => b.Default(WarningBehavior.Throw))
                           .Options))
            {
                Assert.NotNull(context.Model);
            }

            using (var context = new ConstructorTestContextWithOC3A(
                       new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                           .UseInMemoryDatabase(Guid.NewGuid().ToString())
                           .UseInternalServiceProvider(serviceProvider)
                           .ConfigureWarnings(b => b.Default(WarningBehavior.Log))
                           .Options))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [ConditionalFact]
        public void Throws_changing_warnings_config_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider(validateScopes: true);

            using (var serviceScope = new ServiceCollection()
                       .AddDbContext<ConstructorTestContextWithOC3A>(
                           (p, b) => b.ConfigureWarnings(wb => wb.Default(WarningBehavior.Throw))
                               .UseInMemoryDatabase(Guid.NewGuid().ToString())
                               .UseInternalServiceProvider(serviceProvider))
                       .BuildServiceProvider(validateScopes: true)
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(context.Model);
            }

            using (var serviceScope = new ServiceCollection()
                       .AddDbContext<ConstructorTestContextWithOC3A>(
                           (p, b) => b.ConfigureWarnings(wb => wb.Default(WarningBehavior.Ignore))
                               .UseInMemoryDatabase(Guid.NewGuid().ToString())
                               .UseInternalServiceProvider(serviceProvider))
                       .BuildServiceProvider(validateScopes: true)
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class WrappingLoggerFactory(ILoggerFactory loggerFactory) : ILoggerFactory
        {
            private readonly ILoggerFactory _loggerFactory = loggerFactory;

            public IList<string> CreatedLoggers { get; } = new List<string>();

            public void Dispose()
                => _loggerFactory.Dispose();

            public ILogger CreateLogger(string categoryName)
            {
                CreatedLoggers.Add(categoryName);

                return _loggerFactory.CreateLogger(categoryName);
            }

            public void AddProvider(ILoggerProvider provider)
                => _loggerFactory.AddProvider(provider);
        }

        private class ConstructorTestContext1A(DbContextOptions options) : DbContext(options);

        private class ConstructorTestContextWithSets(DbContextOptions options) : DbContext(options)
        {
            public DbSet<Product> Products { get; set; }
        }

        private class ConstructorTestContextNoConfiguration : DbContext;

        private class ConstructorTestContextNoConfigurationWithSets : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        private class ConstructorTestContextWithOCBase : DbContext
        {
            private readonly IServiceProvider _internalServicesProvider;
            private readonly ILoggerFactory _loggerFactory;
            private readonly IMemoryCache _memoryCache;
            private readonly bool _isConfigured;

            protected ConstructorTestContextWithOCBase(
                ILoggerFactory loggerFactory = null,
                IMemoryCache memoryCache = null)
            {
                _loggerFactory = loggerFactory;
                _memoryCache = memoryCache;
            }

            protected ConstructorTestContextWithOCBase(
                IServiceProvider internalServicesProvider,
                ILoggerFactory loggerFactory = null,
                IMemoryCache memoryCache = null)
            {
                _internalServicesProvider = internalServicesProvider;
                _loggerFactory = loggerFactory;
                _memoryCache = memoryCache;
            }

            protected ConstructorTestContextWithOCBase(
                DbContextOptions options,
                ILoggerFactory loggerFactory = null,
                IMemoryCache memoryCache = null)
                : base(options)
            {
                _loggerFactory = loggerFactory;
                _memoryCache = memoryCache;
                _isConfigured = true;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Equal(_isConfigured, optionsBuilder.IsConfigured);

                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                }

                if (_internalServicesProvider != null)
                {
                    optionsBuilder.UseInternalServiceProvider(_internalServicesProvider);
                }
                else if (optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider == null)
                {
                    optionsBuilder.EnableServiceProviderCaching(false);
                }

                if (_memoryCache != null)
                {
                    optionsBuilder.UseMemoryCache(_memoryCache);
                }

                if (_loggerFactory != null)
                {
                    optionsBuilder.UseLoggerFactory(_loggerFactory);
                }
            }
        }

        private interface IConstructorTestContextWithOC1A;

        private class ConstructorTestContextWithOC1A : ConstructorTestContextWithOCBase, IConstructorTestContextWithOC1A;

        private class ConstructorTestContextWithOC2A(
            IServiceProvider internalServicesProvider) : ConstructorTestContextWithOCBase(internalServicesProvider);

        private interface IConstructorTestContextWithOC3A;

        private class ConstructorTestContextWithOC3A(
            DbContextOptions options) : ConstructorTestContextWithOCBase(options), IConstructorTestContextWithOC3A;

        private class ConstructorTestContextWithOC1B(
            ILoggerFactory loggerFactory,
            IMemoryCache memoryCache) : ConstructorTestContextWithOCBase(loggerFactory, memoryCache);

        private class ConstructorTestContextWithOC2B(
            IServiceProvider internalServicesProvider,
            ILoggerFactory loggerFactory,
            IMemoryCache memoryCache) : ConstructorTestContextWithOCBase(internalServicesProvider, loggerFactory, memoryCache);

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_add_non_derived_context_and_override_options(bool useOptions)
        {
            var serviceCollection = new ServiceCollection().AddEntityFrameworkInMemoryDatabase();

            if (useOptions)
            {
                serviceCollection.ConfigureDbContext<DbContext>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ReplaceService<IParameterBindingFactory, CustomParameterBindingFactory>()
                        .ReplaceService<IModelCustomizer, CustomModelCustomizer>());
            }
            else
            {
                serviceCollection.AddDbContext<DbContext>(
                    (p, b) => b
                        .UseInMemoryDatabase(Guid.NewGuid().ToString())
                        .ReplaceService<IParameterBindingFactory, CustomParameterBindingFactory>()
                        .ReplaceService<IModelCustomizer, CustomModelCustomizer>());
            }

            serviceCollection.AddDbContext<DbContext>(
                (p, b) => b.EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ReplaceService<IParameterBindingFactory, EntityTypeParameterBindingFactory, CustomParameterBindingFactory2>()
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer2>());

            var appServiceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.IsType<CustomModelCustomizer2>(context.GetService<IModelCustomizer>());
                Assert.Single(context.GetService<IEnumerable<IModelCustomizer>>());

                var replacedServices = context.GetService<IEnumerable<IParameterBindingFactory>>().ToList();
                Assert.Equal(3, replacedServices.Count);

                Assert.Equal(2, replacedServices.Count(t => t is CustomParameterBindingFactory));
                Assert.Single(replacedServices.Where(t => t is CustomParameterBindingFactory2));
            }
        }

        [ConditionalFact]
        public void Non_derived_options_dont_override_derived_options()
        {
            var serviceCollection = new ServiceCollection().AddEntityFrameworkInMemoryDatabase();

            serviceCollection.ConfigureDbContext<DbContext>(
                (p, b) => b
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ReplaceService<IParameterBindingFactory, CustomParameterBindingFactory>()
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer>());

            serviceCollection.AddDbContext<DerivedContext1>(
                (p, b) => b.EnableServiceProviderCaching(false)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ReplaceService<IParameterBindingFactory, EntityTypeParameterBindingFactory, CustomParameterBindingFactory2>()
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer2>());

            serviceCollection.ConfigureDbContext<DbContext>(
                (p, b) => b
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .ReplaceService<IParameterBindingFactory, CustomParameterBindingFactory>()
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer>());

            var appServiceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            using (var serviceScope = appServiceProvider
                       .GetRequiredService<IServiceScopeFactory>()
                       .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DerivedContext1>();

                Assert.IsType<CustomModelCustomizer2>(context.GetService<IModelCustomizer>());
                Assert.Single(context.GetService<IEnumerable<IModelCustomizer>>());

                var replacedServices = context.GetService<IEnumerable<IParameterBindingFactory>>().ToList();
                Assert.Equal(3, replacedServices.Count);

                Assert.Empty(replacedServices.Where(t => t is CustomParameterBindingFactory));
                Assert.Single(replacedServices.Where(t => t is CustomParameterBindingFactory2));
            }
        }

        [ConditionalFact]
        public void Throws_when_wrong_DbContextOptions_used()
        {
            var options = new DbContextOptionsBuilder<NonGenericOptions1>()
                .UseInternalServiceProvider(new ServiceCollection().BuildServiceProvider(validateScopes: true))
                .Options;

            Assert.Equal(
                CoreStrings.NonGenericOptions(nameof(NonGenericOptions2)),
                Assert.Throws<InvalidOperationException>(() => new NonGenericOptions2(options)).Message);
        }

        [ConditionalFact]
        public void Throws_when_adding_two_contexts_using_non_generic_options()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<NonGenericOptions2>(b => b.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .AddDbContext<NonGenericOptions1>(b => b.UseInMemoryDatabase(Guid.NewGuid().ToString()))
                .BuildServiceProvider(validateScopes: true);

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            Assert.Equal(
                CoreStrings.NonGenericOptions(nameof(NonGenericOptions2)),
                Assert.Throws<InvalidOperationException>(
                    () =>
                    {
                        serviceScope.ServiceProvider.GetService<NonGenericOptions1>();
                        serviceScope.ServiceProvider.GetService<NonGenericOptions2>();
                    }).Message);
        }

        private class NonGenericOptions1(DbContextOptions options) : DbContext(options);

        private class NonGenericOptions2(DbContextOptions options) : DbContext(options);

        [ConditionalFact]
        public void AddDbContext_adds_options_for_all_types()
        {
            var services = new ServiceCollection()
                .AddSingleton<DbContextOptions>(_ => new DbContextOptions<NonGenericOptions1>())
                .AddDbContext<NonGenericOptions1>(optionsLifetime: ServiceLifetime.Singleton)
                .AddDbContext<NonGenericOptions2>(optionsLifetime: ServiceLifetime.Singleton)
                .BuildServiceProvider(validateScopes: true);

            Assert.Equal(3, services.GetServices<DbContextOptions>().Count());
            Assert.Equal(
                2, services.GetServices<DbContextOptions>()
                    .Select(o => o.ContextType)
                    .Distinct()
                    .Count());
        }

        [ConditionalFact]
        public void Last_DbContextOptions_in_serviceCollection_selected()
        {
            var services = new ServiceCollection()
                .AddDbContext<NonGenericOptions1>(optionsLifetime: ServiceLifetime.Singleton)
                .AddDbContext<NonGenericOptions2>(optionsLifetime: ServiceLifetime.Singleton)
                .BuildServiceProvider(validateScopes: true);

            Assert.Equal(typeof(NonGenericOptions2), services.GetService<DbContextOptions>().ContextType);
        }

        [ConditionalFact]
        public void Can_resolve_multiple_contexts_in_hierarchy_with_appropriate_constructors()
        {
            var services = new ServiceCollection()
                .AddDbContext<DerivedContext1>(
                    b =>
                        b.EnableServiceProviderCaching(false)
                            .UseInMemoryDatabase(nameof(DerivedContext1)))
                .AddDbContext<DerivedContext2>(
                    b =>
                        b.EnableServiceProviderCaching(false)
                            .UseInMemoryDatabase(nameof(DerivedContext2)))
                .BuildServiceProvider(validateScopes: true);

            using var scope = services.CreateScope();
            var context1 = scope.ServiceProvider.GetService<DerivedContext1>();
            Assert.IsType<DerivedContext1>(context1);
            Assert.Equal(
                nameof(DerivedContext1),
                context1.GetService<IDbContextOptions>().FindExtension<InMemoryOptionsExtension>().StoreName);

            var context2 = scope.ServiceProvider.GetService<DerivedContext2>();
            Assert.IsType<DerivedContext2>(context2);
            Assert.Equal(
                nameof(DerivedContext2),
                context2.GetService<IDbContextOptions>().FindExtension<InMemoryOptionsExtension>().StoreName);
        }

        private class DerivedContext1 : DbContext
        {
            public DerivedContext1(DbContextOptions<DerivedContext1> options)
                : base(options)
            {
            }

            protected DerivedContext1(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class DerivedContext2(DbContextOptions<DerivedContext2> options) : DerivedContext1(options);
    }
}

namespace Microsoft.EntityFrameworkCore.DifferentNamespace
{
    internal class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Product> Products { get; set; }
    }

    internal class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
