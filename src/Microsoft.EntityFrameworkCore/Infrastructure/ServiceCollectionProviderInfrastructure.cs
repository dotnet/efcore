// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
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
                .AddSingleton<IDbSetFinder, DbSetFinder>()
                .AddSingleton<IDbSetInitializer, DbSetInitializer>()
                .AddSingleton<IDbSetSource, DbSetSource>()
                .AddSingleton<IEntityFinderSource, EntityFinderSource>()
                .AddSingleton<IEntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<ICoreConventionSetBuilder, CoreConventionSetBuilder>()
                .AddSingleton<IModelCustomizer, ModelCustomizer>()
                .AddSingleton<IModelCacheKeyFactory, ModelCacheKeyFactory>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton<IModelSource, ModelSource>()
                .AddSingleton<IInternalEntityEntryFactory, InternalEntityEntryFactory>()
                .AddSingleton<IInternalEntityEntrySubscriber, InternalEntityEntrySubscriber>()
                .AddSingleton<IEntityEntryGraphIterator, EntityEntryGraphIterator>()
                .AddSingleton<IEntityGraphAttacher, EntityGraphAttacher>()
                .AddSingleton<IValueGeneratorCache, ValueGeneratorCache>()
                .AddSingleton<INodeTypeProviderFactory, DefaultMethodInfoBasedNodeTypeRegistryFactory>()
                .AddScoped<IKeyPropagator, KeyPropagator>()
                .AddScoped<INavigationFixer, NavigationFixer>()
                .AddScoped<ILocalViewListener, LocalViewListener>()
                .AddScoped<IStateManager, StateManager>()
                .AddScoped<IConcurrencyDetector, ConcurrencyDetector>()
                .AddScoped<IInternalEntityEntryNotifier, InternalEntityEntryNotifier>()
                .AddScoped<IValueGenerationManager, ValueGenerationManager>()
                .AddScoped<IChangeTrackerFactory, ChangeTrackerFactory>()
                .AddScoped<IChangeDetector, ChangeDetector>()
                .AddScoped<IDbContextServices, DbContextServices>()
                .AddScoped(typeof(ISensitiveDataLogger<>), typeof(SensitiveDataLogger<>))
                .AddScoped(typeof(ILogger<>), typeof(InterceptingLogger<>))
                .AddScoped(p => GetContextServices(p).Model)
                .AddScoped(p => GetContextServices(p).CurrentContext)
                .AddScoped(p => GetContextServices(p).ContextOptions)
                .AddScoped<IValueGeneratorSelector, ValueGeneratorSelector>()
                .AddScoped<IConventionSetBuilder, NullConventionSetBuilder>()
                .AddScoped<IModelValidator, CoreModelValidator>()
                .AddScoped<IExecutionStrategyFactory, ExecutionStrategyFactory>()
                .AddScoped<ICompiledQueryCache, CompiledQueryCache>()
                .AddScoped<IAsyncQueryProvider, EntityQueryProvider>()
                .AddScoped<IQueryCompiler, QueryCompiler>()
                .AddScoped<IQueryAnnotationExtractor, QueryAnnotationExtractor>()
                .AddScoped<IQueryOptimizer, QueryOptimizer>()
                .AddScoped<IEntityTrackingInfoFactory, EntityTrackingInfoFactory>()
                .AddScoped<ISubQueryMemberPushDownExpressionVisitor, SubQueryMemberPushDownExpressionVisitor>()
                .AddScoped<ITaskBlockingExpressionVisitor, TaskBlockingExpressionVisitor>()
                .AddScoped<IEntityResultFindingExpressionVisitorFactory, EntityResultFindingExpressionVisitorFactory>()
                .AddScoped<IMemberAccessBindingExpressionVisitorFactory, MemberAccessBindingExpressionVisitorFactory>()
                .AddScoped<INavigationRewritingExpressionVisitorFactory, NavigationRewritingExpressionVisitorFactory>()
                .AddScoped<IOrderingExpressionVisitorFactory, OrderingExpressionVisitorFactory>()
                .AddScoped<IQuerySourceTracingExpressionVisitorFactory, QuerySourceTracingExpressionVisitorFactory>()
                .AddScoped<IRequiresMaterializationExpressionVisitorFactory, RequiresMaterializationExpressionVisitorFactory>()
                .AddScoped<IExpressionPrinter, ExpressionPrinter>()
                .AddScoped<IQueryCompilationContextFactory, QueryCompilationContextFactory>()
                .AddScoped<ICompiledQueryCacheKeyGenerator, CompiledQueryCacheKeyGenerator>()
                .AddScoped<IResultOperatorHandler, ResultOperatorHandler>()
                .AddScoped<IProjectionExpressionVisitorFactory, ProjectionExpressionVisitorFactory>());



            foreach (var descriptor in serviceCollection.ToList())
            {
                var serviceType = descriptor.ServiceType;

                if (serviceType.GetTypeInfo().IsAssignableFrom(typeof(IServiceInjectionSite)))
                {
                    if (descriptor.ImplementationType != null)
                    {
                        
                    }

                    serviceCollection[serviceCollection.IndexOf(descriptor)]
                        = new ServiceDescriptor(serviceType, p => InjectAdditionalServices(p, serviceType), descriptor.Lifetime);
                }
                

                Type replacementType;
                if (replacedServices.TryGetValue(serviceType, out replacementType))
                {
                    services[services.IndexOf(descriptor)]
                        = new ServiceDescriptor(serviceType, replacementType, descriptor.Lifetime);
                }
            }

        }

        private static object InjectAdditionalServices(IServiceProvider serviceProvider, Type serviceType)
        {
            var service = serviceProvider.GetService(serviceType);

            (service as IServiceInjectionSite)?.InjectServices(serviceProvider);

            return service;
        }


        private static IDbContextServices GetContextServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IDbContextServices>();
    }

    public interface I

    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IServiceInjectionSite
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        void InjectServices([NotNull] IServiceProvider serviceProvider);
    }

}
