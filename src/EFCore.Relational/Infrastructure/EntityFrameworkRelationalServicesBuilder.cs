// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     A builder API designed for relational database providers to use when registering services.
/// </summary>
/// <remarks>
///     <para>
///         Providers should create an instance of this class, use its methods to register
///         services, and then call <see cref="TryAddCoreServices" /> to fill out the remaining Entity
///         Framework services.
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
public class EntityFrameworkRelationalServicesBuilder : EntityFrameworkServicesBuilder
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
    public static readonly IDictionary<Type, ServiceCharacteristics> RelationalServices
        = new Dictionary<Type, ServiceCharacteristics>
        {
            { typeof(IRowKeyValueFactoryFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRowForeignKeyValueFactoryFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRowIndexValueFactoryFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRowIdentityMapFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IParameterNameGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IComparer<IReadOnlyModificationCommand>), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IMigrationsIdGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ISqlGenerationHelper), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRelationalAnnotationProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IMigrationsAnnotationProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IMigrationCommandExecutor), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRelationalTypeMappingSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IUpdateSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRelationalTransactionFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRelationalCommandBuilderFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IRawSqlCommandBuilder), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IQuerySqlGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(IModificationCommandFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ISqlAliasManagerFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
            { typeof(ICommandBatchPreparer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IModificationCommandBatchFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalSqlTranslatingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IMethodCallTranslatorProvider), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IAggregateMethodCallTranslatorProvider), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IMemberTranslatorProvider), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(ISqlExpressionFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalQueryStringFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalParameterBasedSqlProcessorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IMigrationsModelDiffer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IMigrationsSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IMigrator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IMigrationsAssembly), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IBatchExecutor), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalConnection), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalDatabaseFacadeDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalDatabaseCreator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IHistoryRepository), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(INamedConnectionStringResolver), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalConnectionDiagnosticsLogger), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDiagnosticsLogger<DbLoggerCategory.Database.Connection>), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IRelationalCommandDiagnosticsLogger), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            { typeof(IDiagnosticsLogger<DbLoggerCategory.Database.Command>), new ServiceCharacteristics(ServiceLifetime.Scoped) },
            {
                typeof(IRelationalTypeMappingSourcePlugin),
                new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true)
            },
            { typeof(IMethodCallTranslatorPlugin), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
            {
                typeof(IAggregateMethodCallTranslatorPlugin),
                new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true)
            },
            { typeof(IMemberTranslatorPlugin), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) }
        };

    /// <summary>
    ///     Used by relational database providers to create a new <see cref="EntityFrameworkRelationalServicesBuilder" /> for
    ///     registration of provider services.
    /// </summary>
    /// <param name="serviceCollection">The collection to which services will be registered.</param>
    public EntityFrameworkRelationalServicesBuilder(IServiceCollection serviceCollection)
        : base(serviceCollection)
    {
    }

    /// <summary>
    ///     Gets the <see cref="ServiceCharacteristics" /> for the given service type.
    /// </summary>
    /// <param name="serviceType">The type that defines the service API.</param>
    /// <returns>The <see cref="ServiceCharacteristics" /> for the type or <see langword="null" /> if it's not an EF service.</returns>
    protected override ServiceCharacteristics? TryGetServiceCharacteristics(Type serviceType)
        => RelationalServices.TryGetValue(serviceType, out var characteristics)
            ? characteristics
            : base.TryGetServiceCharacteristics(serviceType);

    /// <summary>
    ///     Registers default implementations of all services, including relational services, not already
    ///     registered by the provider. Relational database providers must call this method as the last
    ///     step of service registration--that is, after all provider services have been registered.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///     for more information and examples.
    /// </remarks>
    /// <returns>This builder, such that further calls can be chained.</returns>
    public override EntityFrameworkServicesBuilder TryAddCoreServices()
    {
        TryAdd<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>();
        TryAdd<IComparer<IReadOnlyModificationCommand>, ModificationCommandComparer>();
        TryAdd<IMigrationsIdGenerator, MigrationsIdGenerator>();
        TryAdd<IRowKeyValueFactoryFactory, RowKeyValueFactoryFactory>();
        TryAdd<IRowForeignKeyValueFactoryFactory, RowForeignKeyValueFactoryFactory>();
        TryAdd<IRowIndexValueFactoryFactory, RowIndexValueFactoryFactory>();
        TryAdd<IRowIdentityMapFactory, RowIdentityMapFactory>();
        TryAdd<IModelCustomizer, RelationalModelCustomizer>();
        TryAdd<IModelRuntimeInitializer, RelationalModelRuntimeInitializer>();
        TryAdd<IRelationalAnnotationProvider, RelationalAnnotationProvider>();
        TryAdd<IMigrationsAnnotationProvider, MigrationsAnnotationProvider>();
        TryAdd<IModelValidator, RelationalModelValidator>();
        TryAdd<IMigrator, Migrator>();
        TryAdd<IMigrationCommandExecutor, MigrationCommandExecutor>();
        TryAdd<IMigrationsAssembly, MigrationsAssembly>();
        TryAdd<IDatabase, RelationalDatabase>();
        TryAdd<IBatchExecutor, BatchExecutor>();
        TryAdd<IValueGeneratorSelector, RelationalValueGeneratorSelector>();
        TryAdd<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>();
        TryAdd<IRawSqlCommandBuilder, RawSqlCommandBuilder>();
        TryAdd<ICommandBatchPreparer, CommandBatchPreparer>();
        TryAdd<IResettableService, ICommandBatchPreparer>(p => p.GetRequiredService<ICommandBatchPreparer>());
        TryAdd<IModificationCommandFactory, ModificationCommandFactory>();
        TryAdd<IMigrationsModelDiffer, MigrationsModelDiffer>();
        TryAdd<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        TryAdd<IExecutionStrategyFactory, RelationalExecutionStrategyFactory>();
        TryAdd<ITypeMappingSource>(p => p.GetRequiredService<IRelationalTypeMappingSource>());
        TryAdd<IDatabaseCreator>(p => p.GetRequiredService<IRelationalDatabaseCreator>());
        TryAdd<IDbContextTransactionManager>(p => p.GetRequiredService<IRelationalConnection>());
        TryAdd<IQueryContextFactory, RelationalQueryContextFactory>();
        TryAdd<ICompiledQueryCacheKeyGenerator, RelationalCompiledQueryCacheKeyGenerator>();
        TryAdd<INamedConnectionStringResolver, NamedConnectionStringResolver>();
        TryAdd<IEvaluatableExpressionFilter, RelationalEvaluatableExpressionFilter>();
        TryAdd<IRelationalTransactionFactory, RelationalTransactionFactory>();
        TryAdd<IDatabaseFacadeDependencies>(p => p.GetRequiredService<IRelationalDatabaseFacadeDependencies>());
        TryAdd<IRelationalDatabaseFacadeDependencies, RelationalDatabaseFacadeDependencies>();
        TryAdd<IRelationalConnectionDiagnosticsLogger, RelationalConnectionDiagnosticsLogger>();
        TryAdd<IDiagnosticsLogger<DbLoggerCategory.Database.Connection>>(
            p => p.GetRequiredService<IRelationalConnectionDiagnosticsLogger>());
        TryAdd<IRelationalCommandDiagnosticsLogger, RelationalCommandDiagnosticsLogger>();
        TryAdd<IDiagnosticsLogger<DbLoggerCategory.Database.Command>>(p => p.GetRequiredService<IRelationalCommandDiagnosticsLogger>());
        TryAdd<IInterceptorAggregator, DbConnectionInterceptorAggregator>();
        TryAdd<IInterceptorAggregator, DbTransactionInterceptorAggregator>();
        TryAdd<IInterceptorAggregator, DbCommandInterceptorAggregator>();
        TryAdd<IQuerySqlGeneratorFactory, QuerySqlGeneratorFactory>();
        TryAdd<IShapedQueryCompilingExpressionVisitorFactory, RelationalShapedQueryCompilingExpressionVisitorFactory>();
        TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, RelationalQueryableMethodTranslatingExpressionVisitorFactory>();
        TryAdd<IMethodCallTranslatorProvider, RelationalMethodCallTranslatorProvider>();
        TryAdd<IAggregateMethodCallTranslatorProvider, RelationalAggregateMethodCallTranslatorProvider>();
        TryAdd<IMemberTranslatorProvider, RelationalMemberTranslatorProvider>();
        TryAdd<IQueryTranslationPostprocessorFactory, RelationalQueryTranslationPostprocessorFactory>();
        TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, RelationalSqlTranslatingExpressionVisitorFactory>();
        TryAdd<ISqlExpressionFactory, SqlExpressionFactory>();
        TryAdd<IQueryTranslationPreprocessorFactory, RelationalQueryTranslationPreprocessorFactory>();
        TryAdd<IRelationalParameterBasedSqlProcessorFactory, RelationalParameterBasedSqlProcessorFactory>();
        TryAdd<IRelationalQueryStringFactory, RelationalQueryStringFactory>();
        TryAdd<IQueryCompilationContextFactory, RelationalQueryCompilationContextFactory>();
        TryAdd<IAdHocMapper, RelationalAdHocMapper>();
        TryAdd<ISqlAliasManagerFactory, SqlAliasManagerFactory>();

        ServiceCollectionMap.GetInfrastructure()
            .AddDependencySingleton<RelationalSqlGenerationHelperDependencies>()
            .AddDependencySingleton<RelationalTypeMappingSourceDependencies>()
            .AddDependencySingleton<RelationalModelValidatorDependencies>()
            .AddDependencySingleton<UpdateSqlGeneratorDependencies>()
            .AddDependencySingleton<RelationalAnnotationProviderDependencies>()
            .AddDependencySingleton<MigrationsAnnotationProviderDependencies>()
            .AddDependencySingleton<ParameterNameGeneratorDependencies>()
            .AddDependencySingleton<RelationalTransactionFactoryDependencies>()
            .AddDependencySingleton<RelationalCommandBuilderDependencies>()
            .AddDependencySingleton<QuerySqlGeneratorDependencies>()
            .AddDependencySingleton<RelationalEvaluatableExpressionFilterDependencies>()
            .AddDependencySingleton<RelationalModelDependencies>()
            .AddDependencySingleton<RelationalModelRuntimeInitializerDependencies>()
            .AddDependencyScoped<MigrationsSqlGeneratorDependencies>()
            .AddDependencyScoped<RelationalConventionSetBuilderDependencies>()
            .AddDependencyScoped<ModificationCommandBatchFactoryDependencies>()
            .AddDependencyScoped<CommandBatchPreparerDependencies>()
            .AddDependencyScoped<RelationalDatabaseCreatorDependencies>()
            .AddDependencyScoped<HistoryRepositoryDependencies>()
            .AddDependencyScoped<RelationalCompiledQueryCacheKeyGeneratorDependencies>()
            .AddDependencyScoped<RelationalMethodCallTranslatorProviderDependencies>()
            .AddDependencyScoped<RelationalAggregateMethodCallTranslatorProviderDependencies>()
            .AddDependencyScoped<RelationalMemberTranslatorProviderDependencies>()
            .AddDependencyScoped<SqlExpressionFactoryDependencies>()
            .AddDependencyScoped<RelationalSqlTranslatingExpressionVisitorDependencies>()
            .AddDependencyScoped<RelationalQueryableMethodTranslatingExpressionVisitorDependencies>()
            .AddDependencyScoped<RelationalShapedQueryCompilingExpressionVisitorDependencies>()
            .AddDependencyScoped<RelationalQueryTranslationPreprocessorDependencies>()
            .AddDependencyScoped<RelationalQueryTranslationPostprocessorDependencies>()
            .AddDependencyScoped<RelationalParameterBasedSqlProcessorDependencies>()
            .AddDependencyScoped<RelationalConnectionDependencies>()
            .AddDependencyScoped<RelationalDatabaseDependencies>()
            .AddDependencyScoped<RelationalQueryContextDependencies>()
            .AddDependencyScoped<RelationalQueryCompilationContextDependencies>()
            .AddDependencyScoped<RelationalAdHocMapperDependencies>();

        return base.TryAddCoreServices();
    }
}
