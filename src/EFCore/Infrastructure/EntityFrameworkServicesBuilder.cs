// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A builder API designed for database providers to use when registering services.
    ///     </para>
    ///     <para>
    ///         Providers should create an instance of this class, use its methods to register
    ///         services, and then call <see cref="TryAddCoreServices" /> to fill out the remaining Entity
    ///         Framework services.
    ///     </para>
    ///     <para>
    ///         Relational providers should use 'EntityFrameworkRelationalServicesBuilder instead.
    ///     </para>
    ///     <para>
    ///         Entity Framework ensures that services are registered with the appropriate scope. In some cases a provider
    ///         may register a service with a different scope, but great care must be taken that all its dependencies
    ///         can handle the new scope, and that it does not cause issue for services that depend on it.
    ///     </para>
    /// </summary>
    public class EntityFrameworkServicesBuilder
    {
        /// <summary>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        ///     <para>
        ///         This dictionary is exposed for testing and provider-validation only.
        ///         It should not be used from application code.
        ///     </para>
        /// </summary>
        public static readonly IDictionary<Type, ServiceCharacteristics> CoreServices
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(IDatabaseProvider), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
                { typeof(IDbSetFinder), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IDbSetInitializer), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IDbSetSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IDbQuerySource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IEntityFinderSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IEntityMaterializerSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ICoreConventionSetBuilder), new ServiceCharacteristics(ServiceLifetime.Singleton) },
#pragma warning disable 618
                { typeof(ITypeMapper), new ServiceCharacteristics(ServiceLifetime.Singleton) },
#pragma warning restore 618
                { typeof(ITypeMappingSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IModelCustomizer), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IModelCacheKeyFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ILoggerFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IModelSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IInternalEntityEntryFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IInternalEntityEntrySubscriber), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IEntityEntryGraphIterator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IValueGeneratorCache), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(INodeTypeProviderFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ISingletonOptionsInitializer), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ILoggingOptions), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ICoreSingletonOptions), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IModelValidator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ICompiledQueryCache), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IQueryAnnotationExtractor), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IEntityTrackingInfoFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ITaskBlockingExpressionVisitor), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMemberAccessBindingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(INavigationRewritingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IEagerLoadingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IQuerySourceTracingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IProjectionExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IDiagnosticsLogger<>), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IValueConverterSelector), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IConstructorBindingFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRegisteredServices), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IPropertyParameterBindingFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IParameterBindingFactories), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMemberClassifier), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IEntityGraphAttacher), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IKeyPropagator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(INavigationFixer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ILocalViewListener), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IStateManager), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(Func<IStateManager>), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IConcurrencyDetector), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IInternalEntityEntryNotifier), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IValueGenerationManager), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IChangeTrackerFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IChangeDetector), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IDbContextServices), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IValueGeneratorSelector), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IExpressionPrinter), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IExecutionStrategyFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IAsyncQueryProvider), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IQueryCompiler), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IQueryModelGenerator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IQueryOptimizer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IEntityResultFindingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRequiresMaterializationExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IQueryCompilationContextFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ICompiledQueryCacheKeyGenerator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IResultOperatorHandler), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IModel), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ICurrentDbContext), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IDbContextDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IDbContextOptions), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IDatabase), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IDatabaseCreator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IDbContextTransactionManager), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IQueryContextFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IEntityQueryableExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IEntityQueryModelVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ILazyLoader), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IParameterBindingFactory), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
                { typeof(IConventionSetBuilder), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                { typeof(IEntityStateListener), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                { typeof(INavigationListener), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                { typeof(IKeyListener), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                { typeof(IQueryTrackingListener), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                { typeof(IPropertyListener), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                { typeof(IResettableService), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                { typeof(ISingletonOptions), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
                { typeof(IEvaluatableExpressionFilter), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ITypeMappingSourcePlugin), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) }
            };

        /// <summary>
        ///     Used by database providers to create a new <see cref="EntityFrameworkServicesBuilder" /> for
        ///     registration of provider services. Relational providers should use
        ///     'EntityFrameworkRelationalServicesBuilder'.
        /// </summary>
        /// <param name="serviceCollection"> The collection to which services will be registered. </param>
        public EntityFrameworkServicesBuilder([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            ServiceCollectionMap = new ServiceCollectionMap(serviceCollection);
        }

        /// <summary>
        ///     Access to the underlying <see cref="ServiceCollectionMap" />.
        /// </summary>
        protected virtual ServiceCollectionMap ServiceCollectionMap { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual ServiceCharacteristics GetServiceCharacteristics([NotNull] Type serviceType)
        {
            if (!CoreServices.TryGetValue(serviceType, out var characteristics))
            {
                throw new InvalidOperationException(CoreStrings.NotAnEFService(serviceType.Name));
            }

            return characteristics;
        }

        /// <summary>
        ///     Database providers should call this method for access to the underlying
        ///     <see cref="ServiceCollectionMap" /> such that provider-specific services can be registered.
        ///     Note that implementations of Entity Framework services should be registered directly on the
        ///     <see cref="EntityFrameworkServicesBuilder" /> and not through this method.
        /// </summary>
        /// <param name="serviceMap"> The underlying map to which provider services should be added.</param>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAddProviderSpecificServices([NotNull] Action<ServiceCollectionMap> serviceMap)
        {
            Check.NotNull(serviceMap, nameof(serviceMap));

            serviceMap(ServiceCollectionMap);

            return this;
        }

        /// <summary>
        ///     Registers default implementations of all services not already registered by the provider.
        ///     Database providers must call this method as the last step of service registration--that is,
        ///     after all provider services have been registered.
        /// </summary>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAddCoreServices()
        {
            TryAdd<IDbSetFinder, DbSetFinder>();
            TryAdd<IDbSetInitializer, DbSetInitializer>();
            TryAdd<IDbSetSource, DbSetSource>();
            TryAdd<IDbQuerySource, DbSetSource>();
            TryAdd<IEntityFinderSource, EntityFinderSource>();
            TryAdd<IEntityMaterializerSource, EntityMaterializerSource>();
            TryAdd<ICoreConventionSetBuilder, CoreConventionSetBuilder>();
#pragma warning disable 618
            TryAdd<ITypeMapper, CoreTypeMapper>();
#pragma warning restore 618
            TryAdd<ITypeMappingSource, FallbackTypeMappingSource>();
            TryAdd<IModelCustomizer, ModelCustomizer>();
            TryAdd<IModelCacheKeyFactory, ModelCacheKeyFactory>();
            TryAdd<ILoggerFactory, LoggerFactory>();
            TryAdd<IModelSource, ModelSource>();
            TryAdd<IInternalEntityEntryFactory, InternalEntityEntryFactory>();
            TryAdd<IInternalEntityEntrySubscriber, InternalEntityEntrySubscriber>();
            TryAdd<IEntityEntryGraphIterator, EntityEntryGraphIterator>();
            TryAdd<IEntityGraphAttacher, EntityGraphAttacher>();
            TryAdd<IValueGeneratorCache, ValueGeneratorCache>();
            TryAdd<INodeTypeProviderFactory, DefaultMethodInfoBasedNodeTypeRegistryFactory>();
            TryAdd<IKeyPropagator, KeyPropagator>();
            TryAdd<INavigationFixer, NavigationFixer>();
            TryAdd<ILocalViewListener, LocalViewListener>();
            TryAdd<IStateManager, StateManager>();
            TryAdd<IConcurrencyDetector, ConcurrencyDetector>();
            TryAdd<IInternalEntityEntryNotifier, InternalEntityEntryNotifier>();
            TryAdd<IValueGenerationManager, ValueGenerationManager>();
            TryAdd<IChangeTrackerFactory, ChangeTrackerFactory>();
            TryAdd<IChangeDetector, ChangeDetector>();
            TryAdd<IDbContextServices, DbContextServices>();
            TryAdd<IDbContextDependencies, DbContextDependencies>();
            TryAdd<IValueGeneratorSelector, ValueGeneratorSelector>();
            TryAdd<IConventionSetBuilder, NullConventionSetBuilder>();
            TryAdd<IModelValidator, ModelValidator>();
            TryAdd<IExecutionStrategyFactory, ExecutionStrategyFactory>();
            TryAdd<ICompiledQueryCache, CompiledQueryCache>();
            TryAdd<IAsyncQueryProvider, EntityQueryProvider>();
            TryAdd<IQueryCompiler, QueryCompiler>();
            TryAdd<IQueryModelGenerator, QueryModelGenerator>();
            TryAdd<IQueryAnnotationExtractor, QueryAnnotationExtractor>();
            TryAdd<IQueryOptimizer, QueryOptimizer>();
            TryAdd<IEntityTrackingInfoFactory, EntityTrackingInfoFactory>();
            TryAdd<ITaskBlockingExpressionVisitor, TaskBlockingExpressionVisitor>();
            TryAdd<IEntityResultFindingExpressionVisitorFactory, EntityResultFindingExpressionVisitorFactory>();
            TryAdd<IMemberAccessBindingExpressionVisitorFactory, MemberAccessBindingExpressionVisitorFactory>();
            TryAdd<INavigationRewritingExpressionVisitorFactory, NavigationRewritingExpressionVisitorFactory>();
            TryAdd<IEagerLoadingExpressionVisitorFactory, EagerLoadingExpressionVisitorFactory>();
            TryAdd<IQuerySourceTracingExpressionVisitorFactory, QuerySourceTracingExpressionVisitorFactory>();
            TryAdd<IRequiresMaterializationExpressionVisitorFactory, RequiresMaterializationExpressionVisitorFactory>();
            TryAdd<IExpressionPrinter, ExpressionPrinter>();
            TryAdd<IQueryCompilationContextFactory, QueryCompilationContextFactory>();
            TryAdd<ICompiledQueryCacheKeyGenerator, CompiledQueryCacheKeyGenerator>();
            TryAdd<IResultOperatorHandler, ResultOperatorHandler>();
            TryAdd<IProjectionExpressionVisitorFactory, ProjectionExpressionVisitorFactory>();
            TryAdd<ISingletonOptionsInitializer, SingletonOptionsInitializer>();
            TryAdd(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>));
            TryAdd<ILoggingOptions, LoggingOptions>();
            TryAdd<ICoreSingletonOptions, CoreSingletonOptions>();
            TryAdd<ISingletonOptions, ILoggingOptions>(p => p.GetService<ILoggingOptions>());
            TryAdd<ISingletonOptions, ICoreSingletonOptions>(p => p.GetService<ICoreSingletonOptions>());
            TryAdd(p => GetContextServices(p).Model);
            TryAdd(p => GetContextServices(p).CurrentContext);
            TryAdd(p => GetContextServices(p).ContextOptions);
            TryAdd<IEntityStateListener, INavigationFixer>(p => p.GetService<INavigationFixer>());
            TryAdd<INavigationListener, INavigationFixer>(p => p.GetService<INavigationFixer>());
            TryAdd<IKeyListener, INavigationFixer>(p => p.GetService<INavigationFixer>());
            TryAdd<IQueryTrackingListener, INavigationFixer>(p => p.GetService<INavigationFixer>());
            TryAdd<IPropertyListener, IChangeDetector>(p => p.GetService<IChangeDetector>());
            TryAdd<IEntityStateListener, ILocalViewListener>(p => p.GetService<ILocalViewListener>());
            TryAdd<IResettableService, IStateManager>(p => p.GetService<IStateManager>());
            TryAdd<IResettableService, IDbContextTransactionManager>(p => p.GetService<IDbContextTransactionManager>());
            TryAdd<Func<IStateManager>>(p => p.GetService<IStateManager>);
            TryAdd<IEvaluatableExpressionFilter, EvaluatableExpressionFilter>();
            TryAdd<IValueConverterSelector, ValueConverterSelector>();
            TryAdd<IConstructorBindingFactory, ConstructorBindingFactory>();
            TryAdd<ILazyLoader, LazyLoader>();
            TryAdd<IParameterBindingFactories, ParameterBindingFactories>();
            TryAdd<IMemberClassifier, MemberClassifier>();
            TryAdd<IPropertyParameterBindingFactory, PropertyParameterBindingFactory>();
            TryAdd<IParameterBindingFactory, LazyLoaderParameterBindingFactory>();
            TryAdd<IParameterBindingFactory, ContextParameterBindingFactory>();
            TryAdd<IParameterBindingFactory, EntityTypeParameterBindingFactory>();

            ServiceCollectionMap
                .TryAddSingleton<DiagnosticSource>(p => new DiagnosticListener(DbLoggerCategory.Name));

            ServiceCollectionMap.GetInfrastructure()
                .AddDependencySingleton<DatabaseProviderDependencies>()
                .AddDependencySingleton<ResultOperatorHandlerDependencies>()
                .AddDependencySingleton<ModelSourceDependencies>()
                .AddDependencySingleton<ValueGeneratorCacheDependencies>()
                .AddDependencySingleton<ModelValidatorDependencies>()
                .AddDependencySingleton<CoreConventionSetBuilderDependencies>()
#pragma warning disable 618
                .AddDependencySingleton<CoreTypeMapperDependencies>()
#pragma warning restore 618
                .AddDependencySingleton<TypeMappingSourceDependencies>()
                .AddDependencySingleton<ModelCustomizerDependencies>()
                .AddDependencySingleton<ModelCacheKeyFactoryDependencies>()
                .AddDependencySingleton<ValueConverterSelectorDependencies>()
                .AddDependencyScoped<StateManagerDependencies>()
                .AddDependencyScoped<ExecutionStrategyDependencies>()
                .AddDependencyScoped<CompiledQueryCacheKeyGeneratorDependencies>()
                .AddDependencyScoped<QueryContextDependencies>()
                .AddDependencyScoped<ValueGeneratorSelectorDependencies>()
                .AddDependencyScoped<EntityQueryModelVisitorDependencies>()
                .AddDependencyScoped<DatabaseDependencies>()
                .AddDependencyScoped<QueryCompilationContextDependencies>()
                .ServiceCollection.AddMemoryCache();

            ServiceCollectionMap.TryAddSingleton<IRegisteredServices>(
                new RegisteredServices(ServiceCollectionMap.ServiceCollection.Select(s => s.ServiceType)));

            return this;
        }

        private static IDbContextServices GetContextServices(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IDbContextServices>();

        /// <summary>
        ///     Adds an implementation of an Entity Framework service only if one has not already been registered.
        ///     The scope of the service is automatically defined by Entity Framework.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAdd<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), typeof(TImplementation));

        /// <summary>
        ///     Adds an implementation of an Entity Framework service only if one has not already been registered.
        ///     The scope of the service is automatically defined by Entity Framework.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAdd([NotNull] Type serviceType, [NotNull] Type implementationType)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementationType, nameof(implementationType));

            var characteristics = GetServiceCharacteristics(serviceType);

            if (characteristics.MultipleRegistrations)
            {
                ServiceCollectionMap.TryAddEnumerable(serviceType, implementationType, characteristics.Lifetime);
            }
            else
            {
                ServiceCollectionMap.TryAdd(serviceType, implementationType, characteristics.Lifetime);
            }

            return this;
        }

        /// <summary>
        ///     Adds a factory for an Entity Framework service only if one has not already been registered.
        ///     The scope of the service is automatically defined by Entity Framework.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="factory"> The factory that will create the service instance. </param>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAdd<TService>([NotNull] Func<IServiceProvider, TService> factory)
            where TService : class
            => TryAdd(typeof(TService), typeof(TService), factory);

        /// <summary>
        ///     Adds a factory for an Entity Framework service only if one has not already been registered.
        ///     The scope of the service is automatically defined by Entity Framework.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <typeparam name="TImplementation"> The concrete type that implements the service. </typeparam>
        /// <param name="factory"> The factory that will create the service instance. </param>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAdd<TService, TImplementation>(
            [NotNull] Func<IServiceProvider, TImplementation> factory)
            where TService : class
            where TImplementation : class, TService
            => TryAdd(typeof(TService), typeof(TImplementation), factory);

        /// <summary>
        ///     Adds a factory for an Entity Framework service only if one has not already been registered.
        ///     The scope of the service is automatically defined by Entity Framework.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementationType"> The concrete type that implements the service. </param>
        /// <param name="factory"> The factory that will create the service instance. </param>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAdd(
            [NotNull] Type serviceType,
            [NotNull] Type implementationType,
            [NotNull] Func<IServiceProvider, object> factory)
        {
            Check.NotNull(serviceType, nameof(serviceType));
            Check.NotNull(implementationType, nameof(implementationType));
            Check.NotNull(factory, nameof(factory));

            var characteristics = GetServiceCharacteristics(serviceType);

            if (characteristics.MultipleRegistrations)
            {
                if (implementationType == serviceType
                    || implementationType == typeof(object))
                {
                    throw new InvalidOperationException(CoreStrings.ImplementationTypeRequired(serviceType.Name));
                }

                ServiceCollectionMap.TryAddEnumerable(serviceType, implementationType, factory, characteristics.Lifetime);
            }
            else
            {
                ServiceCollectionMap.TryAdd(serviceType, factory, characteristics.Lifetime);
            }

            return this;
        }

        /// <summary>
        ///     Adds an implementation of an Entity Framework service only if one has not already been registered.
        ///     This method can only be used for singleton services.
        /// </summary>
        /// <typeparam name="TService"> The contract for the service. </typeparam>
        /// <param name="implementation"> The implementation of the service. </param>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAdd<TService>([CanBeNull] TService implementation)
            where TService : class
            => TryAdd(typeof(TService), implementation);

        /// <summary>
        ///     Adds an implementation of an Entity Framework service only if one has not already been registered.
        ///     This method can only be used for singleton services.
        /// </summary>
        /// <param name="serviceType"> The contract for the service. </param>
        /// <param name="implementation"> The implementation of the service. </param>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public virtual EntityFrameworkServicesBuilder TryAdd(
            [NotNull] Type serviceType,
            [CanBeNull] object implementation)
        {
            Check.NotNull(serviceType, nameof(serviceType));

            var characteristics = GetServiceCharacteristics(serviceType);

            if (characteristics.Lifetime != ServiceLifetime.Singleton)
            {
                throw new InvalidOperationException(CoreStrings.SingletonRequired(characteristics.Lifetime, serviceType.Name));
            }

            if (characteristics.MultipleRegistrations)
            {
                if (implementation == null)
                {
                    throw new InvalidOperationException(CoreStrings.ImplementationTypeRequired(serviceType.Name));
                }

                ServiceCollectionMap.TryAddSingletonEnumerable(serviceType, implementation);
            }
            else
            {
                ServiceCollectionMap.TryAddSingleton(serviceType, implementation);
            }

            return this;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public readonly struct ServiceCharacteristics
        {
            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public ServiceLifetime Lifetime { get; }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public bool MultipleRegistrations { get; }

            /// <summary>
            ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public ServiceCharacteristics(ServiceLifetime lifetime, bool multipleRegistrations = false)
            {
                Lifetime = lifetime;
                MultipleRegistrations = multipleRegistrations;
            }
        }
    }
}
