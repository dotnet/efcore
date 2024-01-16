// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     A builder API designed for database providers to use when registering services.
/// </summary>
/// <remarks>
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
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class EntityFrameworkServicesBuilder
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     This dictionary is exposed for testing and provider-validation only.
    ///     It should not be used from application code.
    /// </remarks>
    [EntityFrameworkInternal]
    public static readonly IDictionary<Type, ServiceCharacteristics> CoreServices
        = new Dictionary<Type, ServiceCharacteristics>
        {
            { typeof(LoggingDefinitions), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IDatabaseProvider), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
            { typeof(IDbSetFinder), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IDbSetInitializer), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IDbSetSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IEntityFinderSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IEntityMaterializerSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ITypeMappingSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IModelCustomizer), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IModelCacheKeyFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IModelSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IModelRuntimeInitializer), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IInternalEntityEntrySubscriber), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IEntityEntryGraphIterator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IValueGeneratorCache), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ISingletonOptionsInitializer), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ILoggingOptions), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ICoreSingletonOptions), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IModelValidator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ICompiledQueryCache), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IValueConverterSelector), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IConstructorBindingFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRegisteredServices), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IPropertyParameterBindingFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IParameterBindingFactories), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IMemberClassifier), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IMemoryCache), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IEvaluatableExpressionFilter), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(INavigationExpansionExtensibilityHelper), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IExceptionDetector), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IJsonValueReaderWriterSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IProviderConventionSetBuilder), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IConventionSetBuilder), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDiagnosticsLogger<>), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IInterceptors), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(ILoggerFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IEntityGraphAttacher), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IKeyPropagator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(INavigationFixer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(ILocalViewListener), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IStateManager), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IConcurrencyDetector), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IInternalEntityEntryNotifier), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IValueGenerationManager), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IChangeTrackerFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IChangeDetector), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDbContextServices), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IValueGeneratorSelector), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IExecutionStrategyFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IExecutionStrategy), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IAsyncQueryProvider), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IQueryCompiler), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(ICompiledQueryCacheKeyGenerator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IModel), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDesignTimeModel), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IUpdateAdapterFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(ICurrentDbContext), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDbContextDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDatabaseFacadeDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDbContextOptions), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDatabase), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDatabaseCreator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDbContextTransactionManager), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IQueryContextFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IQueryCompilationContextFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IQueryableMethodTranslatingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IQueryTranslationPreprocessorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IQueryTranslationPostprocessorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IShapedQueryCompilingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDbContextLogger), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IAdHocMapper), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(ILazyLoader), new ServiceCharacteristics(ServiceLifetime.Transient) },
            { typeof(ILazyLoaderFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IParameterBindingFactory), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
            { typeof(ITypeMappingSourcePlugin), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
            {
                typeof(IEvaluatableExpressionFilterPlugin),
                new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true)
            },
            { typeof(ISingletonOptions), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
            { typeof(IConventionSetPlugin), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
            { typeof(ISingletonInterceptor), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
            { typeof(IResettableService), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
            { typeof(IInterceptor), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
            { typeof(IInterceptorAggregator), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) }
        };

    /// <summary>
    ///     Used by database providers to create a new <see cref="EntityFrameworkServicesBuilder" /> for
    ///     registration of provider services. Relational providers should use
    ///     'EntityFrameworkRelationalServicesBuilder'.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="serviceCollection">The collection to which services will be registered.</param>
    public EntityFrameworkServicesBuilder(IServiceCollection serviceCollection)
    {
        ServiceCollectionMap = new ServiceCollectionMap(serviceCollection);
    }

    /// <summary>
    ///     Access to the underlying <see cref="ServiceCollectionMap" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    protected virtual ServiceCollectionMap ServiceCollectionMap { get; }

    /// <summary>
    ///     Gets the <see cref="ServiceCharacteristics" /> for the given service type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="serviceType">The type that defines the service API.</param>
    /// <returns>The <see cref="ServiceCharacteristics" /> for the type.</returns>
    /// <exception cref="InvalidOperationException">when the type is not an EF service.</exception>
    protected virtual ServiceCharacteristics GetServiceCharacteristics(Type serviceType)
    {
        var characteristics = TryGetServiceCharacteristics(serviceType);
        return characteristics ?? throw new InvalidOperationException(CoreStrings.NotAnEFService(serviceType.Name));
    }

    /// <summary>
    ///     Gets the <see cref="ServiceCharacteristics" /> for the given service type.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="serviceType">The type that defines the service API.</param>
    /// <returns>The <see cref="ServiceCharacteristics" /> for the type or <see langword="null" /> if it's not an EF service.</returns>
    protected virtual ServiceCharacteristics? TryGetServiceCharacteristics(Type serviceType)
        => !CoreServices.TryGetValue(serviceType, out var characteristics)
            ? null
            : characteristics;

    /// <summary>
    ///     Database providers should call this method for access to the underlying
    ///     <see cref="ServiceCollectionMap" /> such that provider-specific services can be registered.
    ///     Note that implementations of Entity Framework services should be registered directly on the
    ///     <see cref="EntityFrameworkServicesBuilder" /> and not through this method.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="serviceMap">The underlying map to which provider services should be added.</param>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAddProviderSpecificServices(Action<ServiceCollectionMap> serviceMap)
    {
        ServiceCollectionMap.Validate = serviceType =>
        {
            if (TryGetServiceCharacteristics(serviceType) != null)
            {
                throw new InvalidOperationException(CoreStrings.NotAProviderService(serviceType.Name));
            }
        };

        serviceMap(ServiceCollectionMap);

        ServiceCollectionMap.Validate = null;

        return this;
    }

    /// <summary>
    ///     Registers default implementations of all services not already registered by the provider.
    ///     Database providers must call this method as the last step of service registration--that is,
    ///     after all provider services have been registered.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAddCoreServices()
    {
        TryAdd<IDbSetFinder, DbSetFinder>();
        TryAdd<IDbSetInitializer, DbSetInitializer>();
        TryAdd<IDbSetSource, DbSetSource>();
        TryAdd<IEntityFinderSource, EntityFinderSource>();
        TryAdd<IEntityMaterializerSource, EntityMaterializerSource>();
        TryAdd<IProviderConventionSetBuilder, ProviderConventionSetBuilder>();
        TryAdd<IConventionSetBuilder, RuntimeConventionSetBuilder>();
        TryAdd<IModelCustomizer, ModelCustomizer>();
        TryAdd<IModelCacheKeyFactory, ModelCacheKeyFactory>();
        TryAdd<ILoggerFactory>(p => ScopedLoggerFactory.Create(p, null));
        TryAdd<IModelSource, ModelSource>();
        TryAdd<IModelRuntimeInitializer, ModelRuntimeInitializer>();
        TryAdd<IInternalEntityEntrySubscriber, InternalEntityEntrySubscriber>();
        TryAdd<IEntityEntryGraphIterator, EntityEntryGraphIterator>();
        TryAdd<IEntityGraphAttacher, EntityGraphAttacher>();
        TryAdd<IValueGeneratorCache, ValueGeneratorCache>();
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
        TryAdd<IDatabaseFacadeDependencies, DatabaseFacadeDependencies>();
        TryAdd<IValueGeneratorSelector, ValueGeneratorSelector>();
        TryAdd<IModelValidator, ModelValidator>();
        TryAdd<IExecutionStrategyFactory, ExecutionStrategyFactory>();
        TryAdd(p => p.GetRequiredService<IExecutionStrategyFactory>().Create());
        TryAdd<ICompiledQueryCache, CompiledQueryCache>();
        TryAdd<IAsyncQueryProvider, EntityQueryProvider>();
        TryAdd<IQueryCompiler, QueryCompiler>();
        TryAdd<ICompiledQueryCacheKeyGenerator, CompiledQueryCacheKeyGenerator>();
        TryAdd<ISingletonOptionsInitializer, SingletonOptionsInitializer>();
        TryAdd(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>));
        TryAdd<IInterceptors, Interceptors>();
        TryAdd<IInterceptorAggregator, SaveChangesInterceptorAggregator>();
        TryAdd<IInterceptorAggregator, IdentityResolutionInterceptorAggregator>();
        TryAdd<IInterceptorAggregator, QueryExpressionInterceptorAggregator>();
        TryAdd<ILoggingOptions, LoggingOptions>();
        TryAdd<ICoreSingletonOptions, CoreSingletonOptions>();
        TryAdd<ISingletonOptions, ILoggingOptions>(p => p.GetRequiredService<ILoggingOptions>());
        TryAdd<ISingletonOptions, ICoreSingletonOptions>(p => p.GetRequiredService<ICoreSingletonOptions>());
        TryAdd(p => GetContextServices(p).Model);
        TryAdd<IDesignTimeModel>(p => new DesignTimeModel(GetContextServices(p)));
        TryAdd(p => GetContextServices(p).CurrentContext);
        TryAdd<IDbContextOptions>(p => GetContextServices(p).ContextOptions);
        TryAdd<IResettableService, ILazyLoaderFactory>(p => p.GetRequiredService<ILazyLoaderFactory>());
        TryAdd<IResettableService, IStateManager>(p => p.GetRequiredService<IStateManager>());
        TryAdd<IResettableService, IDbContextTransactionManager>(p => p.GetRequiredService<IDbContextTransactionManager>());
        TryAdd<IEvaluatableExpressionFilter, EvaluatableExpressionFilter>();
        TryAdd<IValueConverterSelector, ValueConverterSelector>();
        TryAdd<IConstructorBindingFactory, ConstructorBindingFactory>();
        TryAdd<ILazyLoaderFactory, LazyLoaderFactory>();
        TryAdd<ILazyLoader>(p => p.GetRequiredService<ILazyLoaderFactory>().Create());
        TryAdd<IParameterBindingFactories, ParameterBindingFactories>();
        TryAdd<IMemberClassifier, MemberClassifier>();
        TryAdd<IPropertyParameterBindingFactory, PropertyParameterBindingFactory>();
        TryAdd<IParameterBindingFactory, LazyLoaderParameterBindingFactory>();
        TryAdd<IParameterBindingFactory, ContextParameterBindingFactory>();
        TryAdd<IParameterBindingFactory, EntityTypeParameterBindingFactory>();
        TryAdd<IMemoryCache>(_ => new MemoryCache(new MemoryCacheOptions { SizeLimit = 10240 }));
        TryAdd<IUpdateAdapterFactory, UpdateAdapterFactory>();
        TryAdd<IQueryCompilationContextFactory, QueryCompilationContextFactory>();
        TryAdd<IQueryTranslationPreprocessorFactory, QueryTranslationPreprocessorFactory>();
        TryAdd<IQueryTranslationPostprocessorFactory, QueryTranslationPostprocessorFactory>();
        TryAdd<INavigationExpansionExtensibilityHelper, NavigationExpansionExtensibilityHelper>();
        TryAdd<IExceptionDetector, ExceptionDetector>();
        TryAdd<IAdHocMapper, AdHocMapper>();
        TryAdd<IJsonValueReaderWriterSource, JsonValueReaderWriterSource>();

        TryAdd(
            p => p.GetService<IDbContextOptions>()?.FindExtension<CoreOptionsExtension>()?.DbContextLogger
                ?? new NullDbContextLogger());

        // This has to be lazy to avoid creating instances that are not disposed
        ServiceCollectionMap
            .TryAddSingleton<DiagnosticSource>(_ => new DiagnosticListener(DbLoggerCategory.Name));

        ServiceCollectionMap.GetInfrastructure()
            .AddDependencySingleton<LazyLoaderParameterBindingFactoryDependencies>()
            .AddDependencySingleton<DatabaseProviderDependencies>()
            .AddDependencySingleton<ModelSourceDependencies>()
            .AddDependencySingleton<ValueGeneratorCacheDependencies>()
            .AddDependencySingleton<ModelValidatorDependencies>()
            .AddDependencySingleton<TypeMappingSourceDependencies>()
            .AddDependencySingleton<ModelCustomizerDependencies>()
            .AddDependencySingleton<ModelCacheKeyFactoryDependencies>()
            .AddDependencySingleton<ValueConverterSelectorDependencies>()
            .AddDependencySingleton<EntityMaterializerSourceDependencies>()
            .AddDependencySingleton<ShapedQueryCompilingExpressionVisitorDependencies>()
            .AddDependencySingleton<EvaluatableExpressionFilterDependencies>()
            .AddDependencySingleton<RuntimeModelDependencies>()
            .AddDependencySingleton<ModelRuntimeInitializerDependencies>()
            .AddDependencySingleton<NavigationExpansionExtensibilityHelperDependencies>()
            .AddDependencySingleton<JsonValueReaderWriterSourceDependencies>()
            .AddDependencyScoped<ProviderConventionSetBuilderDependencies>()
            .AddDependencyScoped<QueryCompilationContextDependencies>()
            .AddDependencyScoped<StateManagerDependencies>()
            .AddDependencyScoped<ExecutionStrategyDependencies>()
            .AddDependencyScoped<CompiledQueryCacheKeyGeneratorDependencies>()
            .AddDependencyScoped<QueryContextDependencies>()
            .AddDependencyScoped<QueryableMethodTranslatingExpressionVisitorDependencies>()
            .AddDependencyScoped<QueryTranslationPreprocessorDependencies>()
            .AddDependencyScoped<QueryTranslationPostprocessorDependencies>()
            .AddDependencyScoped<ValueGeneratorSelectorDependencies>()
            .AddDependencyScoped<DatabaseDependencies>()
            .AddDependencyScoped<ModelDependencies>()
            .AddDependencyScoped<ModelCreationDependencies>()
            .AddDependencyScoped<AdHocMapperDependencies>();

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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TService">The contract for the service.</typeparam>
    /// <typeparam name="TImplementation">The concrete type that implements the service.</typeparam>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAdd<
        TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        where TService : class
        where TImplementation : class, TService
        => TryAdd(typeof(TService), typeof(TImplementation));

    /// <summary>
    ///     Adds an implementation of an Entity Framework service only if one has not already been registered.
    ///     The scope of the service is automatically defined by Entity Framework.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="serviceType">The contract for the service.</param>
    /// <param name="implementationType">The concrete type that implements the service.</param>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAdd(
        Type serviceType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType)
    {
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TService">The contract for the service.</typeparam>
    /// <param name="factory">The factory that will create the service instance.</param>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAdd
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>(Func<IServiceProvider, TService> factory)
        where TService : class
        => TryAdd(typeof(TService), typeof(TService), factory);

    /// <summary>
    ///     Adds a factory for an Entity Framework service only if one has not already been registered.
    ///     The scope of the service is automatically defined by Entity Framework.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TService">The contract for the service.</typeparam>
    /// <typeparam name="TImplementation">The concrete type that implements the service.</typeparam>
    /// <param name="factory">The factory that will create the service instance.</param>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAdd
        <TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>(
            Func<IServiceProvider, TImplementation> factory)
        where TService : class
        where TImplementation : class, TService
        => TryAdd(typeof(TService), typeof(TImplementation), factory);

    /// <summary>
    ///     Adds a factory for an Entity Framework service only if one has not already been registered.
    ///     The scope of the service is automatically defined by Entity Framework.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="serviceType">The contract for the service.</param>
    /// <param name="implementationType">The concrete type that implements the service.</param>
    /// <param name="factory">The factory that will create the service instance.</param>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAdd(
        Type serviceType,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type implementationType,
        Func<IServiceProvider, object> factory)
    {
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
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TService">The contract for the service.</typeparam>
    /// <param name="implementation">The implementation of the service.</param>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAdd<TService>(TService implementation)
        where TService : class
        => TryAdd(typeof(TService), implementation);

    /// <summary>
    ///     Adds an implementation of an Entity Framework service only if one has not already been registered.
    ///     This method can only be used for singleton services.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="serviceType">The contract for the service.</param>
    /// <param name="implementation">The implementation of the service.</param>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public virtual EntityFrameworkServicesBuilder TryAdd(
        Type serviceType,
        object implementation)
    {
        var characteristics = GetServiceCharacteristics(serviceType);

        if (characteristics.Lifetime != ServiceLifetime.Singleton)
        {
            throw new InvalidOperationException(CoreStrings.SingletonRequired(characteristics.Lifetime, serviceType.Name));
        }

        if (characteristics.MultipleRegistrations)
        {
            ServiceCollectionMap.TryAddSingletonEnumerable(serviceType, implementation);
        }
        else
        {
            ServiceCollectionMap.TryAddSingleton(serviceType, implementation);
        }

        return this;
    }
}
