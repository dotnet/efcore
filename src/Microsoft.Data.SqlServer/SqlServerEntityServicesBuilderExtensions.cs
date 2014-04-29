// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Migrations;
using Microsoft.Data.Relational;
using Microsoft.Data.SqlServer;
using Microsoft.Data.SqlServer.Utilities;

namespace Microsoft.Data.Entity
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
