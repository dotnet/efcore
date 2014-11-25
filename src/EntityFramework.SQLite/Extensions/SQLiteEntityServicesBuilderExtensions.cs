// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Sqlite;
using Microsoft.Data.Entity.Sqlite.Utilities;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;
using Microsoft.Data.Entity.Storage;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqliteEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddSqlite([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder
                .AddMigrations().ServiceCollection
                .AddSingleton<SqliteValueGeneratorCache>()
                .AddSingleton<SqliteValueGeneratorSelector>()
                .AddSingleton<SqliteSqlGenerator>()
                .AddSingleton<SqlStatementExecutor>()
                .AddSingleton<SqliteTypeMapper>()
                .AddSingleton<SqliteModificationCommandBatchFactory>()
                .AddSingleton<SqliteCommandBatchPreparer>()
                .AddSingleton<SqliteMetadataExtensionProvider>()
                .AddSingleton<SqliteMigrationOperationFactory>()
                .AddScoped<SqliteBatchExecutor>()
                .AddScoped<DataStoreSource, SqliteDataStoreSource>()
                .AddScoped<SqliteDataStoreServices>()
                .AddScoped<SqliteDataStore>()
                .AddScoped<SqliteConnection>()
                .AddScoped<SqliteMigrationOperationSqlGeneratorFactory>()
                .AddScoped<SqliteDataStoreCreator>()
                .AddScoped<SqliteMigrator>()
                .AddScoped<SqliteDatabase>()
                // TODO: Move to an AddMigrations extension method?
                // Issue #556                
                .AddScoped<SqliteMigrationOperationProcessor>()
                .AddScoped<SqliteModelDiffer>()
                .AddScoped<MigrationAssembly>()
                .AddScoped<HistoryRepository>();

            return builder;
        }
    }
}
