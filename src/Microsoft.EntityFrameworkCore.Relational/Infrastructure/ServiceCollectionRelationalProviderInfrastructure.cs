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
                    .AddSingleton<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>()
                    .AddSingleton<IComparer<ModificationCommand>, ModificationCommandComparer>()
                    .AddSingleton<IMigrationsIdGenerator, MigrationsIdGenerator>()
                    .AddSingleton<IKeyValueIndexFactorySource, KeyValueIndexFactorySource>()
                    .AddSingleton<IModelSource, RelationalModelSource>()
                    .AddScoped<IMigrationsAnnotationProvider, MigrationsAnnotationProvider>()
                    .AddScoped<IModelValidator, RelationalModelValidator>()
                    .AddScoped<IMigrator, Migrator>()
                    .AddScoped<IMigrationCommandExecutor, MigrationCommandExecutor>()
                    .AddScoped<IMigrationsAssembly, MigrationsAssembly>()
                    .AddScoped<IDatabase, RelationalDatabase>()
                    .AddScoped<IBatchExecutor, BatchExecutor>()
                    .AddScoped<IValueGeneratorSelector, RelationalValueGeneratorSelector>()
                    .AddScoped<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                    .AddScoped<IRawSqlCommandBuilder, RawSqlCommandBuilder>()
                    .AddScoped<ICommandBatchPreparer, CommandBatchPreparer>()
                    .AddScoped<IMigrationsModelDiffer, MigrationsModelDiffer>()
                    .AddScoped<IMigrationsSqlGenerator, MigrationsSqlGenerator>()
                    .AddScoped<IExecutionStrategyFactory, RelationalExecutionStrategyFactory>()
                    .AddScoped<IRelationalTypeMapper, RelationalTypeMapper>()
                    .AddScoped<IRelationalValueBufferFactoryFactory, TypedRelationalValueBufferFactoryFactory>()
                    .AddScoped<IDatabaseCreator>(p => p.GetService<IRelationalDatabaseCreator>())
                    .AddScoped<IDbContextTransactionManager>(p => p.GetService<IRelationalConnection>())
                    .AddScoped<IMaterializerFactory, MaterializerFactory>()
                    .AddScoped<IShaperCommandContextFactory, ShaperCommandContextFactory>()
                    .AddScoped<IConditionalRemovingExpressionVisitorFactory, ConditionalRemovingExpressionVisitorFactory>()
                    .AddScoped<ICompositePredicateExpressionVisitorFactory, CompositePredicateExpressionVisitorFactory>()
                    .AddScoped<IIncludeExpressionVisitorFactory, IncludeExpressionVisitorFactory>()
                    .AddScoped<IQueryFlattenerFactory, QueryFlattenerFactory>()
                    .AddScoped<ISelectExpressionFactory, SelectExpressionFactory>()
                    .AddScoped<IExpressionPrinter, RelationalExpressionPrinter>()
                    .AddScoped<IRelationalResultOperatorHandler, RelationalResultOperatorHandler>()
                    .AddScoped<IQueryContextFactory, RelationalQueryContextFactory>()
                    .AddScoped<IQueryCompilationContextFactory, RelationalQueryCompilationContextFactory>()
                    .AddScoped<IEntityQueryableExpressionVisitorFactory, RelationalEntityQueryableExpressionVisitorFactory>()
                    .AddScoped<IEntityQueryModelVisitorFactory, RelationalQueryModelVisitorFactory>()
                    .AddScoped<IProjectionExpressionVisitorFactory, RelationalProjectionExpressionVisitorFactory>()
                    .AddScoped<ICompiledQueryCacheKeyGenerator, RelationalCompiledQueryCacheKeyGenerator>()
                    .AddScoped<IExpressionFragmentTranslator, RelationalCompositeExpressionFragmentTranslator>()
                    .AddScoped<ISqlTranslatingExpressionVisitorFactory, SqlTranslatingExpressionVisitorFactory>());

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
