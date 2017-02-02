// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Methods used by database providers for setting up Entity Framework related
    ///     services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionProviderInfrastructure
    {
        private static readonly HashSet<Type> _servicesToInject
            = new HashSet<Type> { typeof(IValueGeneratorSelector) };

        /// <summary>
        ///     Do not call this method from application code. This method must be called by database providers
        ///     after registering provider-specific services to fill-in the remaining services with Entity
        ///     Framework defaults. Relational providers should call
        ///     'ServiceCollectionRelationalProviderInfrastructure.TryAddDefaultRelationalServices' instead.
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        public static void TryAddDefaultEntityFrameworkServices([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.TryAddEnumerable(new ServiceCollection()
                .AddScoped<IEntityStateListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<INavigationListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<IKeyListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<IQueryTrackingListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<IPropertyListener, IChangeDetector>(p => p.GetService<IChangeDetector>())
                .AddScoped<IEntityStateListener, ILocalViewListener>(p => p.GetService<ILocalViewListener>())
                .AddScoped<IResettableService, IStateManager>(p => p.GetService<IStateManager>())
                .AddScoped<IResettableService, IDbContextTransactionManager>(p => p.GetService<IDbContextTransactionManager>()));

            serviceCollection.TryAdd(new ServiceCollection()
                .AddMemoryCache()
                .AddEntityFrameworkSingleton<IDbSetFinder, DbSetFinder>()
                .AddEntityFrameworkSingleton<IDbSetInitializer, DbSetInitializer>()
                .AddEntityFrameworkSingleton<IDbSetSource, DbSetSource>()
                .AddEntityFrameworkSingleton<IEntityFinderSource, EntityFinderSource>()
                .AddEntityFrameworkSingleton<IEntityMaterializerSource, EntityMaterializerSource>()
                .AddEntityFrameworkSingleton<ICoreConventionSetBuilder, CoreConventionSetBuilder>()
                .AddEntityFrameworkSingleton<IModelCustomizer, ModelCustomizer>()
                .AddEntityFrameworkSingleton<IModelCacheKeyFactory, ModelCacheKeyFactory>()
                .AddEntityFrameworkSingleton<ILoggerFactory, LoggerFactory>()
                .AddEntityFrameworkSingleton<IModelSource, ModelSource>()
                .AddEntityFrameworkSingleton<IInternalEntityEntryFactory, InternalEntityEntryFactory>()
                .AddEntityFrameworkSingleton<IInternalEntityEntrySubscriber, InternalEntityEntrySubscriber>()
                .AddEntityFrameworkSingleton<IEntityEntryGraphIterator, EntityEntryGraphIterator>()
                .AddEntityFrameworkSingleton<IEntityGraphAttacher, EntityGraphAttacher>()
                .AddEntityFrameworkSingleton<IValueGeneratorCache, ValueGeneratorCache>()
                .AddEntityFrameworkSingleton<INodeTypeProviderFactory, DefaultMethodInfoBasedNodeTypeRegistryFactory>()
                .AddEntityFrameworkScoped<IKeyPropagator, KeyPropagator>()
                .AddEntityFrameworkScoped<INavigationFixer, NavigationFixer>()
                .AddEntityFrameworkScoped<ILocalViewListener, LocalViewListener>()
                .AddEntityFrameworkScoped<IStateManager, StateManager>()
                .AddEntityFrameworkScoped<IConcurrencyDetector, ConcurrencyDetector>()
                .AddEntityFrameworkScoped<IInternalEntityEntryNotifier, InternalEntityEntryNotifier>()
                .AddEntityFrameworkScoped<IValueGenerationManager, ValueGenerationManager>()
                .AddEntityFrameworkScoped<IChangeTrackerFactory, ChangeTrackerFactory>()
                .AddEntityFrameworkScoped<IChangeDetector, ChangeDetector>()
                .AddEntityFrameworkScoped<IDbContextServices, DbContextServices>()
                .AddEntityFrameworkScoped(typeof(ISensitiveDataLogger<>), typeof(SensitiveDataLogger<>))
                .AddEntityFrameworkScoped(typeof(ILogger<>), typeof(InterceptingLogger<>))
                .AddEntityFrameworkScoped(p => GetContextServices(p).Model)
                .AddEntityFrameworkScoped(p => GetContextServices(p).CurrentContext)
                .AddEntityFrameworkScoped(p => GetContextServices(p).ContextOptions)
                .AddEntityFrameworkScoped<IValueGeneratorSelector, ValueGeneratorSelector>()
                .AddEntityFrameworkScoped<IConventionSetBuilder, NullConventionSetBuilder>()
                .AddEntityFrameworkScoped<IModelValidator, CoreModelValidator>()
                .AddEntityFrameworkScoped<IExecutionStrategyFactory, ExecutionStrategyFactory>()
                .AddEntityFrameworkScoped<ICompiledQueryCache, CompiledQueryCache>()
                .AddEntityFrameworkScoped<IAsyncQueryProvider, EntityQueryProvider>()
                .AddEntityFrameworkScoped<IQueryCompiler, QueryCompiler>()
                .AddEntityFrameworkScoped<IQueryAnnotationExtractor, QueryAnnotationExtractor>()
                .AddEntityFrameworkScoped<IQueryOptimizer, QueryOptimizer>()
                .AddEntityFrameworkScoped<IEntityTrackingInfoFactory, EntityTrackingInfoFactory>()
                .AddEntityFrameworkScoped<ISubQueryMemberPushDownExpressionVisitor, SubQueryMemberPushDownExpressionVisitor>()
                .AddEntityFrameworkScoped<ITaskBlockingExpressionVisitor, TaskBlockingExpressionVisitor>()
                .AddEntityFrameworkScoped<IEntityResultFindingExpressionVisitorFactory, EntityResultFindingExpressionVisitorFactory>()
                .AddEntityFrameworkScoped<IMemberAccessBindingExpressionVisitorFactory, MemberAccessBindingExpressionVisitorFactory>()
                .AddEntityFrameworkScoped<INavigationRewritingExpressionVisitorFactory, NavigationRewritingExpressionVisitorFactory>()
                .AddEntityFrameworkScoped<IOrderingExpressionVisitorFactory, OrderingExpressionVisitorFactory>()
                .AddEntityFrameworkScoped<IQuerySourceTracingExpressionVisitorFactory, QuerySourceTracingExpressionVisitorFactory>()
                .AddEntityFrameworkScoped<IRequiresMaterializationExpressionVisitorFactory, RequiresMaterializationExpressionVisitorFactory>()
                .AddEntityFrameworkScoped<IExpressionPrinter, ExpressionPrinter>()
                .AddEntityFrameworkScoped<IQueryCompilationContextFactory, QueryCompilationContextFactory>()
                .AddEntityFrameworkScoped<ICompiledQueryCacheKeyGenerator, CompiledQueryCacheKeyGenerator>()
                .AddEntityFrameworkScoped<IResultOperatorHandler, ResultOperatorHandler>()
                .AddEntityFrameworkScoped<IProjectionExpressionVisitorFactory, ProjectionExpressionVisitorFactory>());
        }

        /// <summary>
        /// </summary>
        public static IServiceCollection AddEntityFrameworkSingleton<TService, TImplementation>(
            [NotNull] this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            if (_servicesToInject.Contains(typeof(TService)))
            {
                serviceCollection.AddSingleton<TImplementation>();
                serviceCollection.AddSingleton<TService>(InjectServices<TImplementation>);
            }
            else
            {
                serviceCollection.AddSingleton<TService, TImplementation>();
            }

            return serviceCollection;
        }

        /// <summary>
        /// </summary>
        public static IServiceCollection AddEntityFrameworkScoped<TService, TImplementation>(
            [NotNull] this IServiceCollection serviceCollection)
            where TService : class
            where TImplementation : class, TService
        {
            if (_servicesToInject.Contains(typeof(TService)))
            {
                serviceCollection.AddScoped<TImplementation>();
                serviceCollection.AddScoped<TService>(InjectServices<TImplementation>);
            }
            else
            {
                serviceCollection.AddScoped<TService, TImplementation>();
            }

            return serviceCollection;
        }

        /// <summary>
        /// </summary>
        public static IServiceCollection AddEntityFrameworkScoped<TService>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (_servicesToInject.Contains(typeof(TService)))
            {
                serviceCollection.AddScoped(p => InjectServices(p, implementationFactory));
            }
            else
            {
                serviceCollection.AddScoped(implementationFactory);
            }

            return serviceCollection;
        }

        /// <summary>
        /// </summary>
        public static IServiceCollection AddEntityFrameworkScoped(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Type serviceType,
            [NotNull] Type implemenationType)
        {
            if (_servicesToInject.Contains(serviceType))
            {
                serviceCollection.AddScoped(implemenationType, implemenationType);
                serviceCollection.AddScoped(serviceType, p => InjectServices(p, implemenationType));
            }
            else
            {
                serviceCollection.AddScoped(serviceType, implemenationType);
            }

            return serviceCollection;
        }

        /// <summary>
        /// </summary>
        public static IServiceCollection AddEntityFrameworkSingleton<TService>(
            [NotNull] this IServiceCollection serviceCollection,
            [NotNull] Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            if (_servicesToInject.Contains(typeof(TService)))
            {
                serviceCollection.AddSingleton(p => InjectServices(p, implementationFactory));
            }
            else
            {
                serviceCollection.AddSingleton(implementationFactory);
            }

            return serviceCollection;
        }

        /// <summary>
        /// </summary>
        public static IServiceCollection AddEntityFrameworkSingleton<TService>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] TService implementationInstance)
            where TService : class
        {
            if (_servicesToInject.Contains(typeof(TService)))
            {
                serviceCollection.AddSingleton(p => InjectServices(p, implementationInstance));
            }
            else
            {
                serviceCollection.AddSingleton(implementationInstance);
            }

            return serviceCollection;
        }

        private static TImplementation InjectServices<TImplementation>(
            IServiceProvider serviceProvider) where TImplementation : class
        {
            var service = serviceProvider.GetService<TImplementation>();

            (service as IServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }

        private static TService InjectServices<TService>(
            IServiceProvider serviceProvider,
            Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            var service = implementationFactory(serviceProvider);

            (service as IServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }

        private static object InjectServices(
            IServiceProvider serviceProvider,
            Type implementationType)
        {
            var service = serviceProvider.GetService(implementationType);

            (service as IServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }

        private static TService InjectServices<TService>(
            IServiceProvider serviceProvider, 
            TService service) where TService : class
        {
            (service as IServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }

        //private static object InjectAdditionalServices(IServiceProvider serviceProvider, Type concreteType)
        //{
        //    var service = serviceProvider.GetService(concreteType);

        //    (service as IServiceInjectionSite)?.InjectServices(serviceProvider);

        //    return service;
        //}

        private static IDbContextServices GetContextServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IDbContextServices>();
    }


}
