// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Sqlite;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Sqlite.Query;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqliteEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlite([NotNull] this EntityFrameworkServicesBuilder services)
        {
            Check.NotNull(services, nameof(services));

            ((IAccessor<IServiceCollection>)services.AddRelational()).Service
                .AddSingleton<IDataStoreSource, SqliteDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<ISqliteModelBuilderFactory, SqliteModelBuilderFactory>()
                    .AddSingleton<ISqliteValueGeneratorCache, SqliteValueGeneratorCache>()
                    .AddSingleton<ISqliteSqlGenerator, SqliteSqlGenerator>()
                    .AddScoped<ISqlStatementExecutor, SqlStatementExecutor>()
                    .AddScoped<ISqliteTypeMapper, SqliteTypeMapper>()
                    .AddSingleton<ISqliteModificationCommandBatchFactory, SqliteModificationCommandBatchFactory>()
                    .AddScoped<ISqliteCommandBatchPreparer, SqliteCommandBatchPreparer>()
                    .AddSingleton<ISqliteModelSource, SqliteModelSource>()
                    .AddSingleton<ISqliteValueReaderFactoryFactory, SqliteValueReaderFactoryFactory>()
                    .AddScoped<ISqliteQueryContextFactory, SqliteQueryContextFactory>()
                    .AddScoped<ISqliteValueGeneratorSelector, SqliteValueGeneratorSelector>()
                    .AddScoped<ISqliteBatchExecutor, SqliteBatchExecutor>()
                    .AddScoped<ISqliteDataStoreServices, SqliteDataStoreServices>()
                    .AddScoped<ISqliteDataStore, SqliteDataStore>()
                    .AddScoped<ISqliteConnection, SqliteDataStoreConnection>()
                    .AddScoped<ISqliteModelDiffer, SqliteModelDiffer>()
                    .AddScoped<ISqliteDatabaseFactory, SqliteDatabaseFactory>()
                    .AddScoped<ISqliteMigrationSqlGenerator, SqliteMigrationSqlGenerator>()
                    .AddScoped<ISqliteDataStoreCreator, SqliteDataStoreCreator>()
                    .AddScoped<ISqliteHistoryRepository, SqliteHistoryRepository>());

            return services;
        }
    }
}
