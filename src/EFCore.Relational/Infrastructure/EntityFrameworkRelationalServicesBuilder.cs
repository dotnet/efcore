// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         A builder API designed for relational database providers to use when registering services.
    ///     </para>
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
    /// </summary>
    public class EntityFrameworkRelationalServicesBuilder : EntityFrameworkServicesBuilder
    {
        /// <summary>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        ///     <para>
        ///         This dictionary is exposed for testing and provider-validation only.
        ///         It should not be used from application code.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public static readonly IDictionary<Type, ServiceCharacteristics> RelationalServices
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(IKeyValueIndexFactorySource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IParameterNameGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IComparer<ModificationCommand>), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationsIdGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ISqlGenerationHelper), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalAnnotationProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationsAnnotationProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationCommandExecutor), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalTypeMappingSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalValueBufferFactoryFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IUpdateSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalTransactionFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalCommandBuilderFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRawSqlCommandBuilder), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IQuerySqlGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalSqlTranslatingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMethodCallTranslatorProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMemberTranslatorProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ISqlExpressionFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalQueryStringFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ICommandBatchPreparer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IModificationCommandBatchFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
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
                { typeof(IInterceptor), new ServiceCharacteristics(ServiceLifetime.Scoped, multipleRegistrations: true) },
                {
                    typeof(IRelationalTypeMappingSourcePlugin),
                    new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true)
                },
                {
                    typeof(IMethodCallTranslatorPlugin),
                    new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true)
                },
                { typeof(IMemberTranslatorPlugin), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
                { typeof(IRelationalParameterBasedSqlProcessorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) }
            };

        /// <summary>
        ///     Used by relational database providers to create a new <see cref="EntityFrameworkRelationalServicesBuilder" /> for
        ///     registration of provider services.
        /// </summary>
        /// <param name="serviceCollection"> The collection to which services will be registered. </param>
        public EntityFrameworkRelationalServicesBuilder([NotNull] IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }

        /// <summary>
        ///     Gets the <see cref="ServiceCharacteristics" /> for the given service type.
        /// </summary>
        /// <param name="serviceType"> The type that defines the service API. </param>
        /// <returns> The <see cref="ServiceCharacteristics" /> for the type. </returns>
        /// <exception cref="InvalidOperationException"> when the type is not an EF service. </exception>
        protected override ServiceCharacteristics GetServiceCharacteristics(Type serviceType)
            => RelationalServices.TryGetValue(serviceType, out var characteristics)
                ? characteristics
                : base.GetServiceCharacteristics(serviceType);

        /// <summary>
        ///     Registers default implementations of all services, including relational services, not already
        ///     registered by the provider. Relational database providers must call this method as the last
        ///     step of service registration--that is, after all provider services have been registered.
        /// </summary>
        /// <returns> This builder, such that further calls can be chained. </returns>
        public override EntityFrameworkServicesBuilder TryAddCoreServices()
        {
            TryAdd<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>();
            TryAdd<IComparer<ModificationCommand>, ModificationCommandComparer>();
            TryAdd<IMigrationsIdGenerator, MigrationsIdGenerator>();
            TryAdd<IKeyValueIndexFactorySource, KeyValueIndexFactorySource>();
            TryAdd<IModelCustomizer, RelationalModelCustomizer>();
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
            TryAdd<IMigrationsModelDiffer, MigrationsModelDiffer>();
            TryAdd<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
            TryAdd<IExecutionStrategyFactory, RelationalExecutionStrategyFactory>();
            TryAdd<ITypeMappingSource>(p => p.GetService<IRelationalTypeMappingSource>());
            TryAdd<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>();
            TryAdd<IDatabaseCreator>(p => p.GetService<IRelationalDatabaseCreator>());
            TryAdd<IDbContextTransactionManager>(p => p.GetService<IRelationalConnection>());
            TryAdd<IQueryContextFactory, RelationalQueryContextFactory>();
            TryAdd<ICompiledQueryCacheKeyGenerator, RelationalCompiledQueryCacheKeyGenerator>();
            TryAdd<INamedConnectionStringResolver, NamedConnectionStringResolver>();
            TryAdd<IEvaluatableExpressionFilter, RelationalEvaluatableExpressionFilter>();
            TryAdd<IRelationalTransactionFactory, RelationalTransactionFactory>();
            TryAdd<IDatabaseFacadeDependencies>(p => p.GetService<IRelationalDatabaseFacadeDependencies>());
            TryAdd<IRelationalDatabaseFacadeDependencies, RelationalDatabaseFacadeDependencies>();
            TryAdd<IInterceptorAggregator, DbConnectionInterceptorAggregator>();
            TryAdd<IInterceptorAggregator, DbTransactionInterceptorAggregator>();
            TryAdd<IInterceptorAggregator, DbCommandInterceptorAggregator>();
            TryAdd<IQuerySqlGeneratorFactory, QuerySqlGeneratorFactory>();
            TryAdd<IShapedQueryCompilingExpressionVisitorFactory, RelationalShapedQueryCompilingExpressionVisitorFactory>();
            TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, RelationalQueryableMethodTranslatingExpressionVisitorFactory>();
            TryAdd<IMethodCallTranslatorProvider, RelationalMethodCallTranslatorProvider>();
            TryAdd<IMemberTranslatorProvider, RelationalMemberTranslatorProvider>();
            TryAdd<IQueryTranslationPostprocessorFactory, RelationalQueryTranslationPostprocessorFactory>();
            TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, RelationalSqlTranslatingExpressionVisitorFactory>();
            TryAdd<ISqlExpressionFactory, SqlExpressionFactory>();
            TryAdd<IQueryTranslationPreprocessorFactory, RelationalQueryTranslationPreprocessorFactory>();
            TryAdd<IRelationalParameterBasedSqlProcessorFactory, RelationalParameterBasedSqlProcessorFactory>();
            TryAdd<IRelationalQueryStringFactory, RelationalQueryStringFactory>();
            TryAdd<IQueryCompilationContextFactory, RelationalQueryCompilationContextFactory>();

            ServiceCollectionMap.GetInfrastructure()
                .AddDependencySingleton<RelationalSqlGenerationHelperDependencies>()
                .AddDependencySingleton<RelationalTypeMappingSourceDependencies>()
                .AddDependencySingleton<RelationalModelValidatorDependencies>()
                .AddDependencySingleton<UpdateSqlGeneratorDependencies>()
                .AddDependencySingleton<RelationalAnnotationProviderDependencies>()
                .AddDependencySingleton<MigrationsAnnotationProviderDependencies>()
                .AddDependencySingleton<ParameterNameGeneratorDependencies>()
                .AddDependencySingleton<RelationalValueBufferFactoryDependencies>()
                .AddDependencySingleton<RelationalTransactionFactoryDependencies>()
                .AddDependencySingleton<RelationalCommandBuilderDependencies>()
                .AddDependencySingleton<SqlExpressionFactoryDependencies>()
                .AddDependencySingleton<RelationalMethodCallTranslatorProviderDependencies>()
                .AddDependencySingleton<RelationalMemberTranslatorProviderDependencies>()
                .AddDependencySingleton<RelationalSqlTranslatingExpressionVisitorDependencies>()
                .AddDependencySingleton<RelationalQueryableMethodTranslatingExpressionVisitorDependencies>()
                .AddDependencySingleton<QuerySqlGeneratorDependencies>()
                .AddDependencySingleton<RelationalShapedQueryCompilingExpressionVisitorDependencies>()
                .AddDependencySingleton<RelationalQueryTranslationPostprocessorDependencies>()
                .AddDependencySingleton<RelationalEvaluatableExpressionFilterDependencies>()
                .AddDependencySingleton<RelationalQueryTranslationPreprocessorDependencies>()
                .AddDependencySingleton<RelationalParameterBasedSqlProcessorDependencies>()
                .AddDependencyScoped<MigrationsSqlGeneratorDependencies>()
                .AddDependencyScoped<RelationalConventionSetBuilderDependencies>()
                .AddDependencyScoped<ModificationCommandBatchFactoryDependencies>()
                .AddDependencyScoped<CommandBatchPreparerDependencies>()
                .AddDependencyScoped<RelationalDatabaseCreatorDependencies>()
                .AddDependencyScoped<HistoryRepositoryDependencies>()
                .AddDependencyScoped<RelationalCompiledQueryCacheKeyGeneratorDependencies>()
                .AddDependencyScoped<RelationalConnectionDependencies>()
                .AddDependencyScoped<RelationalDatabaseDependencies>()
                .AddDependencyScoped<RelationalQueryContextDependencies>()
                .AddDependencyScoped<RelationalQueryCompilationContextDependencies>();

            return base.TryAddCoreServices();
        }
    }
}
