// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
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

            services.AddRelational().GetService()
                .AddSingleton<IDatabaseProvider, DatabaseProvider<SqliteDatabaseProviderServices, SqliteOptionsExtension>>()
                .TryAdd(new ServiceCollection()
                    .AddSingleton<SqliteValueGeneratorCache>()
                    .AddSingleton<SqliteUpdateSqlGenerator>()
                    .AddSingleton<SqliteMetadataExtensionProvider>()
                    .AddSingleton<SqliteTypeMapper>()
                    .AddSingleton<SqliteModelSource>()
                    .AddSingleton<SqliteMigrationAnnotationProvider>()
                    .AddSingleton<SqliteConventionSetBuilder>()
                    .AddScoped<SqliteModificationCommandBatchFactory>()
                    .AddScoped<SqliteOperationTransformer>()
                    .AddScoped<SqliteDatabaseProviderServices>()
                    .AddScoped<SqliteDatabase>()
                    .AddScoped<SqliteDatabaseConnection>()
                    .AddScoped<SqliteMigrationSqlGenerator>()
                    .AddScoped<SqliteDatabaseCreator>()
                    .AddScoped<SqliteHistoryRepository>()
                    .AddScoped<SqliteCompositeMethodCallTranslator>()
                    .AddScoped<SqliteCompositeMemberTranslator>());

            return services;
        }
    }
}
