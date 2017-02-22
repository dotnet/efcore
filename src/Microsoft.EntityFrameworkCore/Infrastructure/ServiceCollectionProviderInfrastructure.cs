// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        /// <param name="serviceCollectionMap"> The <see cref="ServiceCollectionMap" /> to add services to. </param>
        public static void TryAddDefaultEntityFrameworkServices([NotNull] ServiceCollectionMap serviceCollectionMap)
        {
            Check.NotNull(serviceCollectionMap, nameof(serviceCollectionMap));

            serviceCollectionMap
                .TryAddSingleton<IDbSetFinder, DbSetFinder>()
                .TryAddSingleton<IDbSetInitializer, DbSetInitializer>()
                .TryAddSingleton<IDbSetSource, DbSetSource>()
                .TryAddSingleton<IEntityFinderSource, EntityFinderSource>()
                .TryAddSingleton<IEntityMaterializerSource, EntityMaterializerSource>()
                .TryAddSingleton<ICoreConventionSetBuilder, CoreConventionSetBuilder>()
                .TryAddSingleton<IModelCustomizer, ModelCustomizer>()
                .TryAddSingleton<IModelCacheKeyFactory, ModelCacheKeyFactory>()
                .TryAddSingleton<ILoggerFactory, LoggerFactory>()
                .TryAddSingleton<IModelSource, ModelSource>()
                .TryAddSingleton<IInternalEntityEntryFactory, InternalEntityEntryFactory>()
                .TryAddSingleton<IInternalEntityEntrySubscriber, InternalEntityEntrySubscriber>()
                .TryAddSingleton<IEntityEntryGraphIterator, EntityEntryGraphIterator>()
                .TryAddSingleton<IEntityGraphAttacher, EntityGraphAttacher>()
                .TryAddSingleton<IValueGeneratorCache, ValueGeneratorCache>()
                .TryAddSingleton<INodeTypeProviderFactory, DefaultMethodInfoBasedNodeTypeRegistryFactory>()
                .TryAddScoped<IKeyPropagator, KeyPropagator>()
                .TryAddScoped<INavigationFixer, NavigationFixer>()
                .TryAddScoped<ILocalViewListener, LocalViewListener>()
                .TryAddScoped<IStateManager, StateManager>()
                .TryAddScoped<IConcurrencyDetector, ConcurrencyDetector>()
                .TryAddScoped<IInternalEntityEntryNotifier, InternalEntityEntryNotifier>()
                .TryAddScoped<IValueGenerationManager, ValueGenerationManager>()
                .TryAddScoped<IChangeTrackerFactory, ChangeTrackerFactory>()
                .TryAddScoped<IChangeDetector, ChangeDetector>()
                .TryAddScoped<IDbContextServices, DbContextServices>()
                .TryAddScoped<IValueGeneratorSelector, ValueGeneratorSelector>()
                .TryAddScoped<IConventionSetBuilder, NullConventionSetBuilder>()
                .TryAddScoped<IModelValidator, CoreModelValidator>()
                .TryAddScoped<IExecutionStrategyFactory, ExecutionStrategyFactory>()
                .TryAddScoped<ICompiledQueryCache, CompiledQueryCache>()
                .TryAddScoped<IAsyncQueryProvider, EntityQueryProvider>()
                .TryAddScoped<IQueryCompiler, QueryCompiler>()
                .TryAddScoped<IQueryAnnotationExtractor, QueryAnnotationExtractor>()
                .TryAddScoped<IQueryOptimizer, QueryOptimizer>()
                .TryAddScoped<IEntityTrackingInfoFactory, EntityTrackingInfoFactory>()
                .TryAddScoped<ISubQueryMemberPushDownExpressionVisitor, SubQueryMemberPushDownExpressionVisitor>()
                .TryAddScoped<ITaskBlockingExpressionVisitor, TaskBlockingExpressionVisitor>()
                .TryAddScoped<IEntityResultFindingExpressionVisitorFactory, EntityResultFindingExpressionVisitorFactory>()
                .TryAddScoped<IMemberAccessBindingExpressionVisitorFactory, MemberAccessBindingExpressionVisitorFactory>()
                .TryAddScoped<INavigationRewritingExpressionVisitorFactory, NavigationRewritingExpressionVisitorFactory>()
                .TryAddScoped<IOrderingExpressionVisitorFactory, OrderingExpressionVisitorFactory>()
                .TryAddScoped<IQuerySourceTracingExpressionVisitorFactory, QuerySourceTracingExpressionVisitorFactory>()
                .TryAddScoped<IRequiresMaterializationExpressionVisitorFactory, RequiresMaterializationExpressionVisitorFactory>()
                .TryAddScoped<IExpressionPrinter, ExpressionPrinter>()
                .TryAddScoped<IQueryCompilationContextFactory, QueryCompilationContextFactory>()
                .TryAddScoped<ICompiledQueryCacheKeyGenerator, CompiledQueryCacheKeyGenerator>()
                .TryAddScoped<IResultOperatorHandler, ResultOperatorHandler>()
                .TryAddScoped<IProjectionExpressionVisitorFactory, ProjectionExpressionVisitorFactory>()
                .TryAddScoped(typeof(ISensitiveDataLogger<>), typeof(SensitiveDataLogger<>))
                .TryAddScoped(typeof(ILogger<>), typeof(InterceptingLogger<>))
                .TryAddScoped(p => GetContextServices(p).Model)
                .TryAddScoped(p => GetContextServices(p).CurrentContext)
                .TryAddScoped(p => GetContextServices(p).ContextOptions)
                .TryAddScopedEnumerable<IEntityStateListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .TryAddScopedEnumerable<INavigationListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .TryAddScopedEnumerable<IKeyListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .TryAddScopedEnumerable<IQueryTrackingListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .TryAddScopedEnumerable<IPropertyListener, IChangeDetector>(p => p.GetService<IChangeDetector>())
                .TryAddScopedEnumerable<IEntityStateListener, ILocalViewListener>(p => p.GetService<ILocalViewListener>())
                .TryAddScopedEnumerable<IResettableService, IStateManager>(p => p.GetService<IStateManager>())
                .TryAddScopedEnumerable<IResettableService, IDbContextTransactionManager>(p => p.GetService<IDbContextTransactionManager>());

            // Note: does TryAdd on all services
            serviceCollectionMap.ServiceCollection.AddMemoryCache();
        }

        private static IDbContextServices GetContextServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IDbContextServices>();
    }
}
