// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EntityFrameworkServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Registers the given context as a service in the <see cref="IServiceCollection" /> and ensures services that the context
        ///         uses are resolved from the <see cref="IServiceProvider" />.
        ///     </para>
        ///     <para>
        ///         You use this method when using dependency injection in your application, such as with ASP.NET.
        ///         For more information on setting up dependency injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services) 
        ///         {
        ///             var connectionString = "connection string to database";
        /// 
        ///             services.AddEntityFramework() 
        ///                 .AddSqlServer()
        ///                 .AddDbContext&lt;MyContext&gt;(options => options.UseSqlServer(connectionString)); 
        ///         }
        ///     </code>
        /// </example>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="optionsAction">
        ///     <para>
        ///         An optional action to configure the <see cref="DbContextOptions" /> for the context. This provides an
        ///         alternative to performing configuration of the context by overriding the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context.
        ///     </para>
        ///     <para>
        ///         If an action is supplied here, the <see cref="DbContext.OnConfiguring" /> method will still be run if it has
        ///         been overridden on the derived context. <see cref="DbContext.OnConfiguring" /> configuration will be applied
        ///         in addition to configuration performed here.
        ///     </para>
        ///     <para>
        ///         You do not need to expose a <see cref="DbContextOptions" /> constructor parameter for the options to be passed to the
        ///         context. If you choose to expose a constructor parameter, we recommend typing it as the generic
        ///         <see cref="DbContextOptions{TContext}" />. You can use the non-generic <see cref="DbContextOptions" /> but this will only
        ///         work if you have one derived context type registered in your <see cref="IServiceProvider" />.
        ///     </para>
        /// </param>
        /// <returns>
        ///     A builder that allows further Entity Framework specific setup of the <see cref="IServiceCollection" />.
        /// </returns>
        public static IServiceCollection AddDbContext<TContext>(
            [NotNull] this IServiceCollection serviceCollection,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            serviceCollection.AddCaching();
            serviceCollection.AddLogging();

            serviceCollection.AddSingleton(_ => DbContextOptionsFactory<TContext>(optionsAction));
            serviceCollection.AddSingleton<DbContextOptions>(p => p.GetRequiredService<DbContextOptions<TContext>>());

            serviceCollection.AddScoped<TContext>();

            return serviceCollection;
        }

        private static DbContextOptions<TContext> DbContextOptionsFactory<TContext>(
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var options = new DbContextOptions<TContext>(new Dictionary<Type, IDbContextOptionsExtension>());

            if (optionsAction != null)
            {
                var builder = new DbContextOptionsBuilder<TContext>(options);
                optionsAction(builder);
                options = builder.Options;
            }

            return options;
        }

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
        ///         <c>collection.AddEntityFramework().AddSqlServer()</c>.
        ///     </para>
        ///     <para>
        ///         For derived contexts to be registered in the <see cref="IServiceProvider" /> and resolve their services
        ///         from the <see cref="IServiceProvider" /> you must chain a call to the
        ///         <see cref="AddDbContext{TContext}" /> method on the returned
        ///         <see cref="IServiceCollection" />.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code>
        ///         public void ConfigureServices(IServiceCollection services) 
        ///         {
        ///             var connectionString = "connection string to database";
        /// 
        ///             services.AddEntityFramework() 
        ///                 .AddSqlServer()
        ///                 .AddDbContext&lt;MyContext&gt;(options => options.UseSqlServer(connectionString)); 
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
                .AddScoped<IPropertyListener, IChangeDetector>(p => p.GetService<IChangeDetector>()));

            serviceCollection.TryAdd(new ServiceCollection()
                .AddSingleton<IDbSetFinder, DbSetFinder>()
                .AddSingleton<IDbSetInitializer, DbSetInitializer>()
                .AddSingleton<IDbSetSource, DbSetSource>()
                .AddSingleton<ICollectionTypeFactory, CollectionTypeFactory>()
                .AddSingleton<IEntityMaterializerSource, EntityMaterializerSource>()
                .AddSingleton<IMemberMapper, MemberMapper>()
                .AddSingleton<IFieldMatcher, FieldMatcher>()
                .AddSingleton<ICoreConventionSetBuilder, CoreConventionSetBuilder>()
                .AddSingleton<IModelCustomizer, ModelCustomizer>()
                .AddSingleton<IModelCacheKeyFactory, ModelCacheKeyFactory>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddScoped<LoggingModelValidator>()
                .AddScoped<IKeyPropagator, KeyPropagator>()
                .AddScoped<INavigationFixer, NavigationFixer>()
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
                .AddScoped(typeof(ISensitiveDataLogger<>), typeof(SensitiveDataLogger<>))
                .AddScoped(typeof(ILogger<>), typeof(InterceptingLogger<>))
                .AddScoped(p => GetContextServices(p).Model)
                .AddScoped(p => GetContextServices(p).Context)
                .AddScoped(p => GetContextServices(p).ContextOptions)
                .AddScoped(p => GetContextServices(p).DatabaseProviderServices)
                .AddScoped(p => GetProviderServices(p).Database)
                .AddScoped(p => GetProviderServices(p).TransactionManager)
                .AddScoped(p => GetProviderServices(p).ValueGeneratorSelector)
                .AddScoped(p => GetProviderServices(p).Creator)
                .AddScoped(p => GetProviderServices(p).ConventionSetBuilder)
                .AddScoped(p => GetProviderServices(p).ValueGeneratorCache)
                .AddScoped(p => GetProviderServices(p).ModelSource)
                .AddScoped(p => GetProviderServices(p).ModelValidator)
                .AddQuery());

            return serviceCollection;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddCaching();

            return serviceCollection
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
                .AddScoped(p => GetProviderServices(p).QueryContextFactory)
                .AddScoped(p => GetProviderServices(p).QueryCompilationContextFactory)
                .AddScoped(p => GetProviderServices(p).CompiledQueryCacheKeyGenerator)
                .AddScoped(p => GetProviderServices(p).EntityQueryModelVisitorFactory)
                .AddScoped(p => GetProviderServices(p).EntityQueryableExpressionVisitorFactory)
                .AddScoped(p => GetProviderServices(p).ExpressionPrinter)
                .AddScoped(p => GetProviderServices(p).ResultOperatorHandler)
                .AddScoped(p => GetProviderServices(p).ProjectionExpressionVisitorFactory);
        }

        private static IDbContextServices GetContextServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IDbContextServices>();

        private static IDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
            => GetContextServices(serviceProvider).DatabaseProviderServices;
    }
}
