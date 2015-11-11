// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Query.ExpressionVisitors.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class RelationalEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddRelational([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.GetInfrastructure()
                .TryAdd(new ServiceCollection()
                    .AddSingleton(s => new DiagnosticListener("Microsoft.Data.Entity"))
                    .AddSingleton<DiagnosticSource>(s => s.GetService<DiagnosticListener>())
                    .AddSingleton<ParameterNameGeneratorFactory>()
                    .AddSingleton<IComparer<ModificationCommand>, ModificationCommandComparer>()
                    .AddSingleton<IMigrationsIdGenerator, MigrationsIdGenerator>()
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
                    .AddScoped<RelationalSqlExecutor>()
                    .AddScoped<IRelationalCommandBuilderFactory, RelationalCommandBuilderFactory>()
                    .AddScoped<ISqlCommandBuilder, SqlCommandBuilder>()
                    .AddScoped<CommandBatchPreparer>()
                    .AddScoped<IMigrationsModelDiffer, MigrationsModelDiffer>()
                    .AddScoped<MigrationsSqlGenerator>()
                    .AddScoped(typeof(ISensitiveDataLogger<>), typeof(SensitiveDataLogger<>))
                    .AddScoped(p => GetProviderServices(p).ParameterNameGeneratorFactory)
                    .AddScoped(p => GetProviderServices(p).SqlGenerator)
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
