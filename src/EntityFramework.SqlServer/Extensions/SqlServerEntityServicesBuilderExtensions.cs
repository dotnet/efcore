// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqlServerEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddSqlServer([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddMigrations().ServiceCollection
                .AddScoped<DataStoreSource, SqlServerDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<SqlServerValueGeneratorCache>()
                    .AddSingleton<SqlServerValueGeneratorSelector>()
                    .AddSingleton<SimpleValueGeneratorFactory<SequentialGuidValueGenerator>>()
                    .AddSingleton<SqlServerSequenceValueGeneratorFactory>()
                    .AddSingleton<SqlServerSqlGenerator>()
                    .AddSingleton<SqlStatementExecutor>()
                    .AddSingleton<SqlServerTypeMapper>()
                    .AddSingleton<SqlServerModificationCommandBatchFactory>()
                    .AddSingleton<SqlServerCommandBatchPreparer>()
                    .AddSingleton<SqlServerMetadataExtensionProvider>()
                    .AddSingleton<SqlServerMigrationOperationFactory>()
                    .AddScoped<SqlServerBatchExecutor>()
                    .AddScoped<SqlServerDataStoreServices>()
                    .AddScoped<SqlServerDataStore>()
                    .AddScoped<SqlServerConnection>()
                    .AddScoped<SqlServerMigrationOperationProcessor>()
                    .AddScoped<SqlServerModelDiffer>()
                    .AddScoped<SqlServerDatabase>()
                    .AddScoped<SqlServerMigrationOperationSqlGeneratorFactory>()
                    .AddScoped<SqlServerDataStoreCreator>()
                    .AddScoped<SqlServerMigrator>());

            return builder;
        }
    }
}
