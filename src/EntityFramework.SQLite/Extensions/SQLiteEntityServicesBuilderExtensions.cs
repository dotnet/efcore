// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite;
using Microsoft.Data.Entity.SQLite.Metadata;
using Microsoft.Data.Entity.SQLite.Migrations;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.Entity.Storage;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SQLiteEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddSQLite([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder
                .AddMigrations().ServiceCollection
                .AddSingleton<SQLiteValueGeneratorCache>()
                .AddSingleton<SQLiteValueGeneratorSelector>()
                .AddSingleton<SQLiteSqlGenerator>()
                .AddSingleton<SqlStatementExecutor>()
                .AddSingleton<SQLiteTypeMapper>()
                .AddSingleton<SQLiteModificationCommandBatchFactory>()
                .AddSingleton<SQLiteCommandBatchPreparer>()
                .AddSingleton<SQLiteMetadataExtensionProvider>()
                .AddSingleton<SQLiteMigrationOperationFactory>()
                .AddScoped<SQLiteBatchExecutor>()
                .AddScoped<DataStoreSource, SQLiteDataStoreSource>()
                .AddScoped<SQLiteDataStoreServices>()
                .AddScoped<SQLiteDataStore>()
                .AddScoped<SQLiteConnection>()
                .AddScoped<SQLiteMigrationOperationSqlGeneratorFactory>()
                .AddScoped<SQLiteDataStoreCreator>()
                .AddScoped<SQLiteMigrator>()
                .AddScoped<SQLiteDatabase>()
                // TODO: Move to an AddMigrations extension method?
                // Issue #556                
                .AddScoped<SQLiteMigrationOperationProcessor>()
                .AddScoped<SQLiteModelDiffer>()
                .AddScoped<MigrationAssembly>()
                .AddScoped<HistoryRepository>();

            return builder;
        }
    }
}
