// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        public static void TryAddDefaultRelationalServices([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection
                .TryAdd(new ServiceCollection()
                    .AddSingleton(s => new DiagnosticListener("Microsoft.EntityFrameworkCore"))
                    .AddSingleton<DiagnosticSource>(s => s.GetService<DiagnosticListener>())
                    .AddEntityFrameworkSingleton<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>()
                    .AddEntityFrameworkSingleton<IComparer<ModificationCommand>, ModificationCommandComparer>()
                    .AddEntityFrameworkSingleton<IMigrationsIdGenerator, MigrationsIdGenerator>()
                    .AddEntityFrameworkSingleton<IKeyValueIndexFactorySource, KeyValueIndexFactorySource>()
                    .AddEntityFrameworkSingleton<IModelSource, RelationalModelSource>()
                    .AddEntityFrameworkScoped<IMigrationsAnnotationProvider, MigrationsAnnotationProvider>()
                    .AddEntityFrameworkScoped<IModelValidator, RelationalModelValidator>()
                    .AddEntityFrameworkScoped<IMigrator, Migrator>()
                    .AddEntityFrameworkScoped<IMigrationCommandExecutor, MigrationCommandExecutor>()
                    .AddEntityFrameworkScoped<IMigrationsAssembly, MigrationsAssembly>()
                    .AddEntityFrameworkScoped<IDatabase, RelationalDatabase>()
                    .AddEntityFrameworkScoped<IBatchExecutor, BatchExecutor>()
                    .AddEntityFrameworkScoped<IValueGeneratorSelector, RelationalValueGeneratorSelector>()
                    .AddEntityFrameworkScoped<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                    .AddEntityFrameworkScoped<IRawSqlCommandBuilder, RawSqlCommandBuilder>()
                    .AddEntityFrameworkScoped<ICommandBatchPreparer, CommandBatchPreparer>()
                    .AddEntityFrameworkScoped<IMigrationsModelDiffer, MigrationsModelDiffer>()
                    .AddEntityFrameworkScoped<IMigrationsSqlGenerator, MigrationsSqlGenerator>()
                    .AddEntityFrameworkScoped<IExecutionStrategyFactory, RelationalExecutionStrategyFactory>()
                    .AddEntityFrameworkScoped<IRelationalTypeMapper, RelationalTypeMapper>()
                    .AddEntityFrameworkScoped<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>()
                    .AddEntityFrameworkScoped<IDatabaseCreator>(p => p.GetService<IRelationalDatabaseCreator>())
                    .AddEntityFrameworkScoped<IDbContextTransactionManager>(p => p.GetService<IRelationalConnection>())
                    .AddEntityFrameworkScoped<IMaterializerFactory, MaterializerFactory>()
                    .AddEntityFrameworkScoped<IShaperCommandContextFactory, ShaperCommandContextFactory>()
                    .AddEntityFrameworkScoped<IConditionalRemovingExpressionVisitorFactory, ConditionalRemovingExpressionVisitorFactory>()
                    .AddEntityFrameworkScoped<ICompositePredicateExpressionVisitorFactory, CompositePredicateExpressionVisitorFactory>()
                    .AddEntityFrameworkScoped<IIncludeExpressionVisitorFactory, IncludeExpressionVisitorFactory>()
                    .AddEntityFrameworkScoped<IQueryFlattenerFactory, QueryFlattenerFactory>()
                    .AddEntityFrameworkScoped<ISelectExpressionFactory, SelectExpressionFactory>()
                    .AddEntityFrameworkScoped<IExpressionPrinter, RelationalExpressionPrinter>()
                    .AddEntityFrameworkScoped<IRelationalResultOperatorHandler, RelationalResultOperatorHandler>()
                    .AddEntityFrameworkScoped<IQueryContextFactory, RelationalQueryContextFactory>()
                    .AddEntityFrameworkScoped<IQueryCompilationContextFactory, RelationalQueryCompilationContextFactory>()
                    .AddEntityFrameworkScoped<IEntityQueryableExpressionVisitorFactory, RelationalEntityQueryableExpressionVisitorFactory>()
                    .AddEntityFrameworkScoped<IEntityQueryModelVisitorFactory, RelationalQueryModelVisitorFactory>()
                    .AddEntityFrameworkScoped<IProjectionExpressionVisitorFactory, RelationalProjectionExpressionVisitorFactory>()
                    .AddEntityFrameworkScoped<ICompiledQueryCacheKeyGenerator, RelationalCompiledQueryCacheKeyGenerator>()
                    .AddEntityFrameworkScoped<IExpressionFragmentTranslator, RelationalCompositeExpressionFragmentTranslator>()
                    .AddEntityFrameworkScoped<ISqlTranslatingExpressionVisitorFactory, SqlTranslatingExpressionVisitorFactory>());

            // Add service dependencies parameter classes.
            // These are added as concrete types because the classes are sealed and the registrations should
            // not be changed by provider or application code.
            serviceCollection
                .TryAdd(new ServiceCollection()
                    .AddScoped<RelationalConnectionDependencies>());

            ServiceCollectionProviderInfrastructure.TryAddDefaultEntityFrameworkServices(serviceCollection);
        }
    }
}
