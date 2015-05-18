// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Relational.ValueGeneration;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

// Intentionally in this namespace since this is for use by other relational providers rather than
// by top-level app developers.

namespace Microsoft.Data.Entity.Relational
{
    public static class RelationalEntityServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddRelational([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            ((IAccessor<IServiceCollection>)builder).Service.TryAdd(new ServiceCollection()
                .AddSingleton<IParameterNameGeneratorFactory, ParameterNameGeneratorFactory>()
                .AddSingleton<IComparer<ModificationCommand>, ModificationCommandComparer>()
                .AddSingleton<IMigrationIdGenerator, MigrationIdGenerator>()
                .AddSingleton<ISqlStatementExecutor, SqlStatementExecutor>()
                .AddSingleton<UntypedValueBufferFactoryFactory>()
                .AddSingleton<TypedValueBufferFactoryFactory>()
                .AddSingleton<IMigrationModelFactory, MigrationModelFactory>()
                .AddScoped<IMigrator, Migrator>()
                .AddScoped<IMigrationAssembly, MigrationAssembly>()
                .AddScoped<RelationalQueryContextFactory>()
                .AddScoped<BatchExecutor>()
                .AddScoped<ModelDiffer>()
                .AddScoped<RelationalDatabaseFactory>()
                .AddScoped<RelationalValueGeneratorSelector>()
                .AddScoped<CommandBatchPreparer>()
                .AddScoped(p => GetStoreServices(p).ModelDiffer)
                .AddScoped(p => GetStoreServices(p).HistoryRepository)
                .AddScoped(p => GetStoreServices(p).MigrationSqlGenerator)
                .AddScoped(p => GetStoreServices(p).RelationalConnection)
                .AddScoped(p => GetStoreServices(p).TypeMapper)
                .AddScoped(p => GetStoreServices(p).ModificationCommandBatchFactory)
                .AddScoped(p => GetStoreServices(p).CommandBatchPreparer)
                .AddScoped(p => GetStoreServices(p).BatchExecutor)
                .AddScoped(p => GetStoreServices(p).ValueBufferFactoryFactory)
                .AddScoped(p => GetStoreServices(p).RelationalDataStoreCreator)
                .AddScoped(p => GetStoreServices(p).SqlGenerator)
                .AddScoped(p => GetStoreServices(p).MetadataExtensionProvider));

            return builder;
        }

        private static IRelationalDataStoreServices GetStoreServices(IServiceProvider serviceProvider)
            => (IRelationalDataStoreServices)serviceProvider.GetRequiredService<IDbContextServices>().DataStoreServices;
    }
}
