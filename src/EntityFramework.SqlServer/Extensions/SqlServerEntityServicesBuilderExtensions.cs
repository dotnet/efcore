// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Migrations;
using Microsoft.Data.Entity.SqlServer.Query;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqlServerEntityServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlServer([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            ((IAccessor<IServiceCollection>)builder.AddRelational()).Service
                .AddScoped<IDataStoreSource, SqlServerDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<ISqlServerModelBuilderFactory, SqlServerModelBuilderFactory>()
                    .AddSingleton<ISqlServerValueGeneratorCache, SqlServerValueGeneratorCache>()
                    .AddSingleton<SqlServerSequenceValueGeneratorFactory>()
                    .AddSingleton<SqlServerSqlGenerator>()
                    .AddSingleton<SqlStatementExecutor>()
                    .AddSingleton<SqlServerTypeMapper>()
                    .AddSingleton<SqlServerModificationCommandBatchFactory>()
                    .AddSingleton<SqlServerCommandBatchPreparer>()
                    .AddSingleton<ISqlServerModelSource, SqlServerModelSource>()
                    .AddScoped<ISqlServerQueryContextFactory, SqlServerQueryContextFactory>()
                    .AddScoped<ISqlServerValueGeneratorSelector, SqlServerValueGeneratorSelector>()
                    .AddScoped<SqlServerBatchExecutor>()
                    .AddScoped<ISqlServerDataStoreServices, SqlServerDataStoreServices>()
                    .AddScoped<ISqlServerDataStore, SqlServerDataStore>()
                    .AddScoped<ISqlServerConnection, SqlServerConnection>()
                    .AddScoped<ISqlServerModelDiffer, SqlServerModelDiffer>()
                    .AddScoped<SqlServerDatabase>()
                    .AddScoped<ISqlServerMigrationSqlGenerator, SqlServerMigrationSqlGenerator>()
                    .AddScoped<ISqlServerDataStoreCreator, SqlServerDataStoreCreator>()
                    .AddScoped<ISqlServerHistoryRepository, SqlServerHistoryRepository>());

            return builder;
        }
    }
}
