// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
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
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Methods used by relational database providers for setting up Entity Framework related
    ///     services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class ServiceCollectionRelationalProviderInfrastructure
    {
        /// <summary>
        ///     Do not call this method from application code. This method must be called by relational database
        ///     providers after registering provider-specific services to fill-in the remaining services with
        ///     Entity Framework defaults.
        /// </summary>
        /// <param name="serviceCollectionMap"> The <see cref="IServiceCollection" /> to add services to. </param>
        public static void TryAddDefaultRelationalServices([NotNull] ServiceCollectionMap serviceCollectionMap)
        {
            Check.NotNull(serviceCollectionMap, nameof(serviceCollectionMap));

            serviceCollectionMap
                .TryAddSingleton(s => new DiagnosticListener("Microsoft.EntityFrameworkCore"))
                .TryAddSingleton<DiagnosticSource>(s => s.GetService<DiagnosticListener>())
                .TryAddSingleton<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>()
                .TryAddSingleton<IComparer<ModificationCommand>, ModificationCommandComparer>()
                .TryAddSingleton<IMigrationsIdGenerator, MigrationsIdGenerator>()
                .TryAddSingleton<IKeyValueIndexFactorySource, KeyValueIndexFactorySource>()
                .TryAddSingleton<IModelSource, RelationalModelSource>()
                .TryAddScoped<IMigrationsAnnotationProvider, MigrationsAnnotationProvider>()
                .TryAddScoped<IModelValidator, RelationalModelValidator>()
                .TryAddScoped<IMigrator, Migrator>()
                .TryAddScoped<IMigrationCommandExecutor, MigrationCommandExecutor>()
                .TryAddScoped<IMigrationsAssembly, MigrationsAssembly>()
                .TryAddScoped<IDatabase, RelationalDatabase>()
                .TryAddScoped<IBatchExecutor, BatchExecutor>()
                .TryAddScoped<IValueGeneratorSelector, RelationalValueGeneratorSelector>()
                .TryAddScoped<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                .TryAddScoped<IRawSqlCommandBuilder, RawSqlCommandBuilder>()
                .TryAddScoped<ICommandBatchPreparer, CommandBatchPreparer>()
                .TryAddScoped<IMigrationsModelDiffer, MigrationsModelDiffer>()
                .TryAddScoped<IMigrationsSqlGenerator, MigrationsSqlGenerator>()
                .TryAddScoped<IExecutionStrategyFactory, RelationalExecutionStrategyFactory>()
                .TryAddScoped<IRelationalTypeMapper, RelationalTypeMapper>()
                .TryAddScoped<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>()
                .TryAddScoped<IDatabaseCreator>(p => p.GetService<IRelationalDatabaseCreator>())
                .TryAddScoped<IDbContextTransactionManager>(p => p.GetService<IRelationalConnection>())
                .TryAddScoped<IMaterializerFactory, MaterializerFactory>()
                .TryAddScoped<IShaperCommandContextFactory, ShaperCommandContextFactory>()
                .TryAddScoped<IConditionalRemovingExpressionVisitorFactory, ConditionalRemovingExpressionVisitorFactory>()
                .TryAddScoped<ICompositePredicateExpressionVisitorFactory, CompositePredicateExpressionVisitorFactory>()
                .TryAddScoped<IIncludeExpressionVisitorFactory, IncludeExpressionVisitorFactory>()
                .TryAddScoped<ISelectExpressionFactory, SelectExpressionFactory>()
                .TryAddScoped<IExpressionPrinter, RelationalExpressionPrinter>()
                .TryAddScoped<IRelationalResultOperatorHandler, RelationalResultOperatorHandler>()
                .TryAddScoped<IQueryContextFactory, RelationalQueryContextFactory>()
                .TryAddScoped<IQueryCompilationContextFactory, RelationalQueryCompilationContextFactory>()
                .TryAddScoped<IEntityQueryableExpressionVisitorFactory, RelationalEntityQueryableExpressionVisitorFactory>()
                .TryAddScoped<IEntityQueryModelVisitorFactory, RelationalQueryModelVisitorFactory>()
                .TryAddScoped<IProjectionExpressionVisitorFactory, RelationalProjectionExpressionVisitorFactory>()
                .TryAddScoped<ICompiledQueryCacheKeyGenerator, RelationalCompiledQueryCacheKeyGenerator>()
                .TryAddScoped<IExpressionFragmentTranslator, RelationalCompositeExpressionFragmentTranslator>()
                .TryAddScoped<ISqlTranslatingExpressionVisitorFactory, SqlTranslatingExpressionVisitorFactory>()
                .TryAddSingleton<RelationalCompositeMemberTranslatorDependencies, RelationalCompositeMemberTranslatorDependencies>()
                .TryAddSingleton<RelationalSqlGenerationHelperDependencies, RelationalSqlGenerationHelperDependencies>()
                .TryAddSingleton<RelationalTypeMapperDependencies, RelationalTypeMapperDependencies>()
                .TryAddSingleton<RelationalCompositeExpressionFragmentTranslatorDependencies, RelationalCompositeExpressionFragmentTranslatorDependencies>()
                .TryAddScoped<RelationalConventionSetBuilderDependencies, RelationalConventionSetBuilderDependencies>()
                .TryAddScoped<UpdateSqlGeneratorDependencies, UpdateSqlGeneratorDependencies>()
                .TryAddScoped<QuerySqlGeneratorDependencies, QuerySqlGeneratorDependencies>()
                .TryAddScoped<RelationalDatabaseCreatorDependencies, RelationalDatabaseCreatorDependencies>()
                .TryAddScoped<RelationalCompositeMethodCallTranslatorDependencies, RelationalCompositeMethodCallTranslatorDependencies>()
                .TryAddScoped<MigrationsSqlGeneratorDependencies, MigrationsSqlGeneratorDependencies>()
                .TryAddScoped<HistoryRepositoryDependencies, HistoryRepositoryDependencies>()
                .TryAddScoped<MigrationsAnnotationProviderDependencies, MigrationsAnnotationProviderDependencies>()
                .TryAddScoped<RelationalCompiledQueryCacheKeyGeneratorDependencies, RelationalCompiledQueryCacheKeyGeneratorDependencies>()
                .TryAddScoped<RelationalModelValidatorDependencies, RelationalModelValidatorDependencies>()
                .TryAddScoped<SqlTranslatingExpressionVisitorDependencies, SqlTranslatingExpressionVisitorDependencies>()
                .TryAddScoped<RelationalProjectionExpressionVisitorDependencies, RelationalProjectionExpressionVisitorDependencies>()
                .TryAddScoped<ParameterNameGeneratorDependencies, ParameterNameGeneratorDependencies>()
                .TryAddScoped<RelationalQueryModelVisitorDependencies, RelationalQueryModelVisitorDependencies>()
                .TryAddScoped<RelationalEntityQueryableExpressionVisitorDependencies, RelationalEntityQueryableExpressionVisitorDependencies>()
                .TryAddScoped<RelationalConnectionDependencies, RelationalConnectionDependencies>()
                .TryAddScoped<RelationalDatabaseDependencies, RelationalDatabaseDependencies>()
                .TryAddScoped<SelectExpressionDependencies, SelectExpressionDependencies>()
                .TryAddScoped<RelationalValueBufferFactoryDependencies, RelationalValueBufferFactoryDependencies>()
                .TryAddScoped<RelationalQueryCompilationContextDependencies, RelationalQueryCompilationContextDependencies>();

            ServiceCollectionProviderInfrastructure.TryAddDefaultEntityFrameworkServices(serviceCollectionMap);
        }
    }
}
