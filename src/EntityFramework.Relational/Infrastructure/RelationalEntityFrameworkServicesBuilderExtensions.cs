// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Extensions;
using Strings = Microsoft.Data.Entity.Relational.Internal.Strings;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.

namespace Microsoft.Data.Entity.Infrastructure
{
    public static class RelationalEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddRelational([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            builder.GetService().TryAdd(new ServiceCollection()
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
                .AddScoped<SqlStatementExecutor>()
                .AddScoped<CommandBatchPreparer>()
                .AddScoped<IMigrationsModelDiffer, MigrationsModelDiffer>()
                .AddScoped<MigrationsSqlGenerator>()
                .AddScoped(p => GetProviderServices(p).ParameterNameGeneratorFactory)
                .AddScoped(p => GetProviderServices(p).SqlStatementExecutor)
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
                .AddScoped(p => GetProviderServices(p).MetadataExtensionProvider)
                .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<RelationalQueryContextFactory>();
        }

        private static IRelationalDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
        {
            var providerServices = serviceProvider.GetRequiredService<IDbContextServices>().DatabaseProviderServices
                as IRelationalDatabaseProviderServices;

            if (providerServices == null)
            {
                throw new InvalidOperationException(Strings.RelationalNotInUse);
            }

            return providerServices;
        }
    }
}
