// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqlServerEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddSqlServer([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.AddRelational().ServiceCollection
                // TODO: Need to be able to pick the appropriate identity generator for the data store in use
                .AddSingleton<IdentityGeneratorFactory, SqlServerIdentityGeneratorFactory>()
                .AddSingleton<DataStoreSource, SqlServerDataStoreSource>()
                .AddSingleton<SqlServerSqlGenerator, SqlServerSqlGenerator>()
                .AddSingleton<SqlStatementExecutor, SqlStatementExecutor>()
                .AddScoped<SqlServerDataStore, SqlServerDataStore>()
                .AddScoped<SqlServerConnection, SqlServerConnection>()
                .AddScoped<SqlServerBatchExecutor, SqlServerBatchExecutor>()
                .AddScoped<ModelDiffer, ModelDiffer>()
                .AddScoped<SqlServerMigrationOperationSqlGenerator, SqlServerMigrationOperationSqlGenerator>()
                .AddScoped<SqlServerDataStoreCreator, SqlServerDataStoreCreator>();

            return builder;
        }
    }
}
