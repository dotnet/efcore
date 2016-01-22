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
    public static class RelationalEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddRelational([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.GetInfrastructure()
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
                    .AddScoped(p => GetProviderServices(p).ParameterNameGeneratorFactory)
                    .AddScoped(p => GetProviderServices(p).SqlGenerationHelper)
                    .AddScoped(p => GetProviderServices(p).CompositeMethodCallTranslator)
                    .AddScoped(p => GetProviderServices(p).CompositeMemberTranslator)
                    .AddScoped(p => GetProviderServices(p).CompositeExpressionFragmentTranslator)
                    .AddScoped(p => GetProviderServices(p).MigrationsAnnotationProvider)
                    .AddScoped(p => GetProviderServices(p).HistoryRepository)
                    .AddScoped(p => GetProviderServices(p).MigrationsSqlGenerator)
                    .AddScoped(p => GetProviderServices(p).RelationalConnection)
                    .AddScoped(p => GetProviderServices(p).TypeMapper)
                    .AddScoped(p => GetProviderServices(p).ModificationCommandBatchFactory)
                    .AddScoped(p => GetProviderServices(p).CommandBatchPreparer)
                    .AddScoped(p => GetProviderServices(p).BatchExecutor)
                    .AddScoped(p => GetProviderServices(p).ValueBufferFactoryFactory)
                    .AddScoped(p => GetProviderServices(p).RelationalDatabaseCreator)
                    .AddScoped(p => GetProviderServices(p).UpdateSqlGenerator)
                    .AddScoped(p => GetProviderServices(p).AnnotationProvider)
                    .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<IMaterializerFactory, MaterializerFactory>()
                .AddScoped<IShaperCommandContextFactory, ShaperCommandContextFactory>()
                .AddScoped<ICompositePredicateExpressionVisitorFactory, CompositePredicateExpressionVisitorFactory>()
                .AddScoped<IIncludeExpressionVisitorFactory, IncludeExpressionVisitorFactory>()
                .AddScoped<IQueryFlattenerFactory, QueryFlattenerFactory>()
                .AddScoped<ISqlTranslatingExpressionVisitorFactory, SqlTranslatingExpressionVisitorFactory>()
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
                .AddScoped(p => GetProviderServices(p).QuerySqlGeneratorFactory);

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
