// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
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
    ///     Relational database specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class RelationalServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the base services required by a relational database provider.
        ///     </para>
        ///     <para>
        ///         This method is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection" /> to add services to. </param>
        public static IServiceCollection AddRelational([NotNull] this IServiceCollection services)
        {
            Check.NotNull(services, nameof(services));

            services.AddEntityFramework();

            services
                .TryAdd(new ServiceCollection()
                    .AddSingleton(s => new DiagnosticListener("Microsoft.EntityFrameworkCore"))
                    .AddSingleton<DiagnosticSource>(s => s.GetService<DiagnosticListener>())
                    .AddSingleton<ParameterNameGeneratorFactory>()
                    .AddSingleton<IComparer<ModificationCommand>, ModificationCommandComparer>()
                    .AddSingleton<IMigrationsIdGenerator, MigrationsIdGenerator>()
                    .AddSingleton<IKeyValueIndexFactorySource, KeyValueIndexFactorySource>()
                    .AddSingleton<UntypedRelationalValueBufferFactoryFactory>()
                    .AddSingleton<TypedRelationalValueBufferFactoryFactory>()
                    .AddSingleton<MigrationsAnnotationProvider>()
                    .AddScoped<RelationalModelValidator>()
                    .AddScoped<IMigrator, Migrator>()
                    .AddScoped<IMigrationCommandExecutor, MigrationCommandExecutor>()
                    .AddScoped<IMigrationsAssembly, MigrationsAssembly>()
                    .AddScoped<RelationalDatabase>()
                    .AddScoped<BatchExecutor>()
                    .AddScoped<MigrationsModelDiffer>()
                    .AddScoped<RelationalValueGeneratorSelector>()
                    .AddScoped<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                    .AddScoped<IRawSqlCommandBuilder, RawSqlCommandBuilder>()
                    .AddScoped<CommandBatchPreparer>()
                    .AddScoped<IMigrationsModelDiffer, MigrationsModelDiffer>()
                    .AddScoped<MigrationsSqlGenerator>()
                    .AddScoped<RelationalExecutionStrategyFactory>()
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ParameterNameGeneratorFactory))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).SqlGenerationHelper))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).CompositeMethodCallTranslator))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).CompositeMemberTranslator))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).CompositeExpressionFragmentTranslator))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).MigrationsAnnotationProvider))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).HistoryRepository))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).MigrationsSqlGenerator))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).RelationalConnection))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).TypeMapper))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ModificationCommandBatchFactory))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).CommandBatchPreparer))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).BatchExecutor))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).ValueBufferFactoryFactory))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).RelationalDatabaseCreator))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).UpdateSqlGenerator))
                    .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).AnnotationProvider))
                    .AddQuery());

            return services;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<IMaterializerFactory, MaterializerFactory>()
                .AddScoped<IShaperCommandContextFactory, ShaperCommandContextFactory>()
                .AddScoped<IConditionalRemovingExpressionVisitorFactory, ConditionalRemovingExpressionVisitorFactory>()
                .AddScoped<ICompositePredicateExpressionVisitorFactory, CompositePredicateExpressionVisitorFactory>()
                .AddScoped<IIncludeExpressionVisitorFactory, IncludeExpressionVisitorFactory>()
                .AddScoped<IQueryFlattenerFactory, QueryFlattenerFactory>()
                .AddScoped<ISelectExpressionFactory, SelectExpressionFactory>()
                .AddScoped<RelationalExpressionPrinter>()
                .AddScoped<RelationalResultOperatorHandler>()
                .AddScoped<RelationalQueryContextFactory>()
                .AddScoped<RelationalQueryCompilationContextFactory>()
                .AddScoped<RelationalEntityQueryableExpressionVisitorFactory>()
                .AddScoped<RelationalQueryModelVisitorFactory>()
                .AddScoped<RelationalProjectionExpressionVisitorFactory>()
                .AddScoped<RelationalCompiledQueryCacheKeyGenerator>()
                .AddScoped<RelationalCompositeExpressionFragmentTranslator>()
                .AddScoped<SqlTranslatingExpressionVisitorFactory>()
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).QuerySqlGeneratorFactory))
                .AddScoped(p => p.InjectAdditionalServices(GetProviderServices(p).SqlTranslatingExpressionVisitorFactory));

        private static IRelationalDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
        {
            var providerServices = serviceProvider.GetRequiredService<IDbContextServices>().DatabaseProviderServices
                as IRelationalDatabaseProviderServices;

            if (providerServices == null)
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return providerServices;
        }
    }
}
