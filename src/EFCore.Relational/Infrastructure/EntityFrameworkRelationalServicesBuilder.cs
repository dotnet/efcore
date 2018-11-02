// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
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
using ReLinq = Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;

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
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        ///     <para>
        ///         This dictionary is exposed for testing and provider-validation only.
        ///         It should not be used from application code.
        ///     </para>
        /// </summary>
        public static readonly IDictionary<Type, ServiceCharacteristics> RelationalServices
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(IKeyValueIndexFactorySource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IParameterNameGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IComparer<ModificationCommand>), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationsIdGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ISqlGenerationHelper), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationsAnnotationProvider), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationCommandExecutor), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalCommandBuilderFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRawSqlCommandBuilder), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMigrationsSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
#pragma warning disable 618
                { typeof(IRelationalTypeMapper), new ServiceCharacteristics(ServiceLifetime.Singleton) },
#pragma warning restore 618
                { typeof(IRelationalTypeMappingSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalValueBufferFactoryFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMaterializerFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IShaperCommandContextFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IConditionalRemovingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ISelectExpressionFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IExpressionFragmentTranslator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ISqlTranslatingExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IUpdateSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ISingletonUpdateSqlGenerator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMemberTranslator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ICompositeMethodCallTranslator), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IQuerySqlGeneratorFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IRelationalTransactionFactory), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(ICommandBatchPreparer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IModificationCommandBatchFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrationsModelDiffer), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMigrationsAssembly), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IBatchExecutor), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(ICompositePredicateExpressionVisitorFactory), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalResultOperatorHandler), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalConnection), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalDatabaseCreator), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IHistoryRepository), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(INamedConnectionStringResolver), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IRelationalTypeMappingSourcePlugin), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
                { typeof(IMethodCallTranslatorPlugin), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) },
                { typeof(IMemberTranslatorPlugin), new ServiceCharacteristics(ServiceLifetime.Singleton, multipleRegistrations: true) }
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
            return RelationalServices.TryGetValue(serviceType, out var characteristics)
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
            TryAdd<IModelCustomizer, RelationalModelCustomizer>();
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
#pragma warning disable 618
            TryAdd<IRelationalTypeMapper, ObsoleteRelationalTypeMapper>();
            TryAdd<ITypeMapper>(p => p.GetService<IRelationalTypeMapper>());
#pragma warning restore 618
            TryAdd<IRelationalTypeMappingSource, FallbackRelationalTypeMappingSource>();
            TryAdd<ITypeMappingSource>(p => p.GetService<IRelationalTypeMappingSource>());
            TryAdd<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>();
            TryAdd<IDatabaseCreator>(p => p.GetService<IRelationalDatabaseCreator>());
            TryAdd<IDbContextTransactionManager>(p => p.GetService<IRelationalConnection>());
            TryAdd<IMaterializerFactory, MaterializerFactory>();
            TryAdd<IShaperCommandContextFactory, ShaperCommandContextFactory>();
            TryAdd<IConditionalRemovingExpressionVisitorFactory, ConditionalRemovingExpressionVisitorFactory>();
            TryAdd<ICompositePredicateExpressionVisitorFactory, CompositePredicateExpressionVisitorFactory>();
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
            TryAdd<INamedConnectionStringResolver, NamedConnectionStringResolver>();
            TryAdd<ReLinq.IEvaluatableExpressionFilter, ReLinqRelationalEvaluatableExpressionFilter>();
            TryAdd<IEvaluatableExpressionFilter, RelationalEvaluatableExpressionFilter>();
            TryAdd<IRelationalTransactionFactory, RelationalTransactionFactory>();

            TryAdd<ISingletonUpdateSqlGenerator>(
                p =>
                {
                    using (var scope = p.CreateScope())
                    {
                        return scope.ServiceProvider.GetService<IUpdateSqlGenerator>();
                    }
                });

            ServiceCollectionMap.GetInfrastructure()
                .AddDependencySingleton<RelationalCompositeMemberTranslatorDependencies>()
                .AddDependencySingleton<RelationalSqlGenerationHelperDependencies>()
                .AddDependencySingleton<RelationalTypeMapperDependencies>()
                .AddDependencySingleton<RelationalTypeMappingSourceDependencies>()
                .AddDependencySingleton<RelationalCompositeExpressionFragmentTranslatorDependencies>()
                .AddDependencySingleton<RelationalModelValidatorDependencies>()
                .AddDependencySingleton<UpdateSqlGeneratorDependencies>()
                .AddDependencySingleton<QuerySqlGeneratorDependencies>()
                .AddDependencySingleton<RelationalCompositeMethodCallTranslatorDependencies>()
                .AddDependencySingleton<MigrationsSqlGeneratorDependencies>()
                .AddDependencySingleton<MigrationsAnnotationProviderDependencies>()
                .AddDependencySingleton<SqlTranslatingExpressionVisitorDependencies>()
                .AddDependencySingleton<ParameterNameGeneratorDependencies>()
                .AddDependencySingleton<SelectExpressionDependencies>()
                .AddDependencySingleton<RelationalValueBufferFactoryDependencies>()
                .AddDependencySingleton<RelationalProjectionExpressionVisitorDependencies>()
                .AddDependencySingleton<RelationalTransactionFactoryDependencies>()
                .AddDependencyScoped<RelationalConventionSetBuilderDependencies>()
                .AddDependencyScoped<CommandBatchPreparerDependencies>()
                .AddDependencyScoped<RelationalDatabaseCreatorDependencies>()
                .AddDependencyScoped<HistoryRepositoryDependencies>()
                .AddDependencyScoped<RelationalCompiledQueryCacheKeyGeneratorDependencies>()
                .AddDependencyScoped<RelationalQueryModelVisitorDependencies>()
                .AddDependencyScoped<RelationalEntityQueryableExpressionVisitorDependencies>()
                .AddDependencyScoped<RelationalConnectionDependencies>()
                .AddDependencyScoped<RelationalDatabaseDependencies>()
                .AddDependencyScoped<RelationalQueryCompilationContextDependencies>();

            return base.TryAddCoreServices();
        }
    }
}
