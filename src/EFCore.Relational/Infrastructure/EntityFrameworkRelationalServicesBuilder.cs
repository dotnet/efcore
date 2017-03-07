// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
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
        private static readonly IDictionary<Type, ServiceCharacteristics> _relationalServices
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(IKeyValueIndexFactorySource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IParameterNameGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IComparer<ModificationCommand>), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationsIdGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ISqlGenerationHelper), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalAnnotationProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationsAnnotationProvider), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrationCommandExecutor), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrationsAssembly), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IBatchExecutor), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalCommandBuilderFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRawSqlCommandBuilder), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ICommandBatchPreparer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrationsModelDiffer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrationsSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalTypeMapper), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalValueBufferFactoryFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMaterializerFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IShaperCommandContextFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IConditionalRemovingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ICompositePredicateExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IIncludeExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ISelectExpressionFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalResultOperatorHandler), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IExpressionFragmentTranslator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ISqlTranslatingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IUpdateSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IModificationCommandBatchFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalConnection), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalDatabaseCreator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IHistoryRepository), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMemberTranslator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMethodCallTranslator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IQuerySqlGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) }
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override ServiceCharacteristics GetServiceCharacteristics(Type serviceType)
        {
            ServiceCharacteristics characteristics;
            return _relationalServices.TryGetValue(serviceType, out characteristics)
                ? characteristics
                : base.GetServiceCharacteristics(serviceType);
        }

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
            TryAdd<IModelSource, RelationalModelSource>();
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
            TryAdd<IRelationalTypeMapper, RelationalTypeMapper>();
            TryAdd<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>();
            TryAdd<IDatabaseCreator>(p => p.GetService<IRelationalDatabaseCreator>());
            TryAdd<IDbContextTransactionManager>(p => p.GetService<IRelationalConnection>());
            TryAdd<IMaterializerFactory, MaterializerFactory>();
            TryAdd<IShaperCommandContextFactory, ShaperCommandContextFactory>();
            TryAdd<IConditionalRemovingExpressionVisitorFactory, ConditionalRemovingExpressionVisitorFactory>();
            TryAdd<ICompositePredicateExpressionVisitorFactory, CompositePredicateExpressionVisitorFactory>();
            TryAdd<IIncludeExpressionVisitorFactory, IncludeExpressionVisitorFactory>();
            TryAdd<ISelectExpressionFactory, SelectExpressionFactory>();
            TryAdd<IExpressionPrinter, RelationalExpressionPrinter>();
            TryAdd<IRelationalResultOperatorHandler, RelationalResultOperatorHandler>();
            TryAdd<IQueryContextFactory, RelationalQueryContextFactory>();
            TryAdd<IQueryCompilationContextFactory, RelationalQueryCompilationContextFactory>();
            TryAdd<IEntityQueryableExpressionVisitorFactory, RelationalEntityQueryableExpressionVisitorFactory>();
            TryAdd<IEntityQueryModelVisitorFactory, RelationalQueryModelVisitorFactory>();
            TryAdd<IProjectionExpressionVisitorFactory, RelationalProjectionExpressionVisitorFactory>();
            TryAdd<ICompiledQueryCacheKeyGenerator, RelationalCompiledQueryCacheKeyGenerator>();
            TryAdd<IExpressionFragmentTranslator, RelationalCompositeExpressionFragmentTranslator>();
            TryAdd<ISqlTranslatingExpressionVisitorFactory, SqlTranslatingExpressionVisitorFactory>();

            ServiceCollectionMap
                .TryAddSingleton(s => new DiagnosticListener("Microsoft.EntityFrameworkCore"))
                .TryAddSingleton<DiagnosticSource>(s => s.GetService<DiagnosticListener>());

            ServiceCollectionMap.GetInfrastructure()
                .AddDependencySingleton<RelationalCompositeMemberTranslatorDependencies>()
                .AddDependencySingleton<RelationalSqlGenerationHelperDependencies>()
                .AddDependencySingleton<RelationalTypeMapperDependencies>()
                .AddDependencySingleton<RelationalCompositeExpressionFragmentTranslatorDependencies>()
                .AddDependencyScoped<RelationalConventionSetBuilderDependencies>()
                .AddDependencyScoped<UpdateSqlGeneratorDependencies>()
                .AddDependencyScoped<QuerySqlGeneratorDependencies>()
                .AddDependencyScoped<RelationalDatabaseCreatorDependencies>()
                .AddDependencyScoped<RelationalCompositeMethodCallTranslatorDependencies>()
                .AddDependencyScoped<MigrationsSqlGeneratorDependencies>()
                .AddDependencyScoped<HistoryRepositoryDependencies>()
                .AddDependencyScoped<MigrationsAnnotationProviderDependencies>()
                .AddDependencyScoped<RelationalCompiledQueryCacheKeyGeneratorDependencies>()
                .AddDependencyScoped<RelationalModelValidatorDependencies>()
                .AddDependencyScoped<SqlTranslatingExpressionVisitorDependencies>()
                .AddDependencyScoped<RelationalProjectionExpressionVisitorDependencies>()
                .AddDependencyScoped<ParameterNameGeneratorDependencies>()
                .AddDependencyScoped<RelationalQueryModelVisitorDependencies>()
                .AddDependencyScoped<RelationalEntityQueryableExpressionVisitorDependencies>()
                .AddDependencyScoped<RelationalConnectionDependencies>()
                .AddDependencyScoped<RelationalDatabaseDependencies>()
                .AddDependencyScoped<SelectExpressionDependencies>()
                .AddDependencyScoped<RelationalValueBufferFactoryDependencies>()
                .AddDependencyScoped<RelationalQueryCompilationContextDependencies>();

            return base.TryAddCoreServices();
        }
    }
}
