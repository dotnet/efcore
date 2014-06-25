// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SQLite;
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

            builder.AddRelational().ServiceCollection
                .AddSingleton<DataStoreSource, SQLiteDataStoreSource>()
                .AddSingleton<SQLiteValueGeneratorCache>()
                .AddSingleton<ValueGeneratorSelector>()
                .AddSingleton<SQLiteSqlGenerator>()
                .AddSingleton<SqlStatementExecutor>()
                .AddSingleton<SQLiteTypeMapper>()
                .AddSingleton<SQLiteBatchExecutor>()
                .AddSingleton<ModificationCommandBatchFactory, SQLiteModificationCommandBatchFactory>()
                .AddScoped<SQLiteDataStore>()
                .AddScoped<SQLiteConnection>()
                .AddScoped<ModelDiffer>()
                .AddScoped<SQLiteMigrationOperationSqlGenerator>()
                .AddScoped<SQLiteDataStoreCreator>();

            return builder;
        }
    }
}
