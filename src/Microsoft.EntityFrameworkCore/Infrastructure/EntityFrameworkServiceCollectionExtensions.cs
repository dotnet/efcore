// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EntityFrameworkServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds the services required by the core of Entity Framework to an <see cref="IServiceCollection" />.
        ///     You use this method when using dependency injection in your application, such as with ASP.NET.
        ///     For more information on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external <see cref="IServiceProvider" />. If you are not using an external
        ///         <see cref="IServiceProvider" /> Entity Framework will take care of creating the services it requires.
        ///     </para>
        ///     <para>
        ///         The database you are using will also define extension methods that can be called on the returned
        ///         <see cref="IServiceCollection" /> to register the services required by the database.
        ///         For example, when using Microsoft.EntityFrameworkCore.SqlServer you would call
        ///         <c>collection.AddEntityFrameworkSqlServer()</c>.
        ///     </para>
        ///     <para>
        ///         For derived contexts to be registered in the <see cref="IServiceProvider" /> and resolve their services
        ///         from the <see cref="IServiceProvider" /> you must chain a call to the
        ///         <see
        ///             cref="Microsoft.Extensions.DependencyInjection.EntityFrameworkServiceCollectionExtensions.AddDbContext{TContext}(IServiceCollection, Action{DbContextOptionsBuilder}, ServiceLifetime)" />
        ///         method on the returned <see cref="IServiceCollection" />.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services) 
        ///         {
        ///             var connectionString = "connection string to database";
        /// 
        ///             services.AddDbContext&lt;MyContext&gt;(options => options.UseSqlServer(connectionString)); 
        ///         }
        ///     </code>
        /// </example>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public static IServiceCollection AddEntityFramework(
            [NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.TryAddEnumerable(new ServiceCollection()
                .AddScoped<IEntityStateListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<INavigationListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<IKeyListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<IQueryTrackingListener, INavigationFixer>(p => p.GetService<INavigationFixer>())
                .AddScoped<IPropertyListener, IChangeDetector>(p => p.GetService<IChangeDetector>())
                .AddScoped<IEntityStateListener, ILocalViewListener>(p => p.GetService<ILocalViewListener>()));

            serviceCollection.TryAdd(new ServiceCollection()
                .AddSingleton<IDbSetFinder, DbSetFinder>()
                .AddSingleton<IDbSetInitializer, DbSetInitializer>()
                .AddSingleton<IDbSetSource, DbSetSource>()
                .AddSingleton<IEntityFinderSource, EntityFinderSource>()
                .AddSingleton<IEntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<ICoreConventionSetBuilder, CoreConventionSetBuilder>()
                .AddSingleton<IModelCustomizer, ModelCustomizer>()
                .AddSingleton<IModelCacheKeyFactory, ModelCacheKeyFactory>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton<NoopModelValidator>()
                .AddScoped<CoreModelValidator>()
                .AddScoped<IKeyPropagator, KeyPropagator>()
                .AddScoped<INavigationFixer, NavigationFixer>()
                .AddScoped<ILocalViewListener, LocalViewListener>()
                .AddScoped<IStateManager, StateManager>()
                .AddScoped<IConcurrencyDetector, ConcurrencyDetector>()
                .AddScoped<IInternalEntityEntryFactory, InternalEntityEntryFactory>()
                .AddScoped<IInternalEntityEntryNotifier, InternalEntityEntryNotifier>()
                .AddScoped<IInternalEntityEntrySubscriber, InternalEntityEntrySubscriber>()
                .AddScoped<IValueGenerationManager, ValueGenerationManager>()
                .AddScoped<IChangeTrackerFactory, ChangeTrackerFactory>()
                .AddScoped<IChangeDetector, ChangeDetector>()
                .AddScoped<IEntityEntryGraphIterator, EntityEntryGraphIterator>()
                .AddScoped<IDbContextServices, DbContextServices>()
                .AddScoped<IDatabaseProviderSelector, DatabaseProviderSelector>()
                .AddScoped<IEntityGraphAttacher, EntityGraphAttacher>()
                .AddScoped<ValueGeneratorSelector>()
                .AddSingleton<ExecutionStrategyFactory>()
                .AddScoped(typeof(ISensitiveDataLogger<>), typeof(SensitiveDataLogger<>))
                .AddScoped(typeof(ILogger<>), typeof(InterceptingLogger<>))
                .AddScoped(p => GetContextServices(p).Model)
                .AddScoped(p => GetContextServices(p).CurrentContext)
                .AddScoped(p => GetContextServices(p).ContextOptions)
                .AddScoped(p => GetContextServices(p).DatabaseProviderServices)
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).Database))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).TransactionManager))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ValueGeneratorSelector))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).Creator))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ConventionSetBuilder))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ValueGeneratorCache))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ModelSource))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ModelValidator))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ExecutionStrategyFactory))
                .AddQuery());

            return serviceCollection;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddMemoryCache()
                .AddSingleton(_ => MethodInfoBasedNodeTypeRegistry.CreateFromRelinqAssembly())
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
                .AddScoped<CompiledQueryCacheKeyGenerator>()
                .AddScoped<ExpressionPrinter>()
                .AddScoped<ResultOperatorHandler>()
                .AddScoped<QueryCompilationContextFactory>()
                .AddScoped<ProjectionExpressionVisitorFactory>()
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).QueryContextFactory))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).QueryCompilationContextFactory))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).CompiledQueryCacheKeyGenerator))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).EntityQueryModelVisitorFactory))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).EntityQueryableExpressionVisitorFactory))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ExpressionPrinter))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ResultOperatorHandler))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ProjectionExpressionVisitorFactory));

        private static IDbContextServices GetContextServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IDbContextServices>();

        private static IDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
            => GetContextServices(serviceProvider).DatabaseProviderServices;
    }
}
