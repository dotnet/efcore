// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SQLite;
using Microsoft.Data.Entity.SQLite.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Framework.DependencyInjection
{
    public static class SQLiteEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddSQLite([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddRelational().ServiceCollection
                .AddSingleton<DataStoreSource, SQLiteDataStoreSource>()
                .AddSingleton<SQLiteSqlGenerator>()
                .AddSingleton<SqlStatementExecutor>()
                .AddSingleton<SQLiteTypeMapper>()
                .AddScoped<SQLiteDataStore>()
                .AddScoped<SQLiteConnectionConnection>()
                .AddScoped<SQLiteBatchExecutor>()
                .AddScoped<ModelDiffer>()
                .AddScoped<SQLiteMigrationOperationSqlGenerator>()
                .AddScoped<SQLiteDataStoreCreator>();

            return builder;
        }
    }
}
