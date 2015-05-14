// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Sqlite;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;
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
                    .AddSingleton<SqliteValueGeneratorCache>()
                    .AddSingleton<SqliteSqlGenerator>()
                    .AddSingleton<SqliteMetadataExtensionsAccessor>()
                    .AddScoped<SqliteTypeMapper>()
                    .AddScoped<SqliteModificationCommandBatchFactory>()
                    .AddSingleton<SqliteModelSource>()
                    .AddScoped<SqliteDataStoreServices>()
                    .AddScoped<SqliteDataStore>()
                    .AddScoped<SqliteDataStoreConnection>()
                    .AddScoped<SqliteMigrationSqlGenerator>()
                    .AddScoped<SqliteDataStoreCreator>()
                    .AddScoped<SqliteHistoryRepository>());

            return services;
        }
    }
}
