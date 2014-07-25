// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Data.Entity.Storage;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqlServerEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddSqlServer([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddRelational().ServiceCollection
                .AddSingleton<SqlServerValueGeneratorCache>()
                .AddSingleton<SqlServerValueGeneratorSelector>()
                .AddSingleton<SimpleValueGeneratorFactory<SequentialGuidValueGenerator>>()
                .AddSingleton<SqlServerSequenceValueGeneratorFactory>()
                .AddSingleton<SqlServerSqlGenerator>()
                .AddSingleton<SqlStatementExecutor>()
                .AddSingleton<SqlServerTypeMapper>()
                .AddSingleton<SqlServerBatchExecutor>()
                .AddSingleton<ModificationCommandBatchFactory, SqlServerModificationCommandBatchFactory>()
                .AddScoped<DataStoreSource, SqlServerDataStoreSource>()
                .AddScoped<SqlServerDataStoreServices>()
                .AddScoped<SqlServerDataStore>()
                .AddScoped<SqlServerConnection>()
                .AddScoped<ModelDiffer>()
                .AddScoped<SqlServerMigrationOperationSqlGeneratorFactory>()
                // TODO: Update code to use SqlServerMigrationOperationSqlGeneratorFactory, then remove the line below.
                .AddScoped<SqlServerMigrationOperationSqlGenerator>()
                .AddScoped<SqlServerDataStoreCreator>()
                .AddScoped<MigrationAssembly>()
                .AddScoped<HistoryRepository>()
                .AddScoped<Migrator, SqlServerMigrator>();

            return builder;
        }
    }
}
