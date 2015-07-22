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
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqliteEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlite([NotNull] this EntityFrameworkServicesBuilder services)
        {
            Check.NotNull(services, nameof(services));

            var service = services.AddRelational().GetService();

            service.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<SqliteDatabaseProviderServices, SqliteOptionsExtension>>());

            service.TryAdd(new ServiceCollection()
                    .AddSingleton<SqliteValueGeneratorCache>()
                    .AddSingleton<SqliteUpdateSqlGenerator>()
                    .AddSingleton<SqliteMetadataExtensionProvider>()
                    .AddSingleton<SqliteTypeMapper>()
                    .AddSingleton<SqliteModelSource>()
                    .AddSingleton<SqliteMigrationAnnotationProvider>()
                    .AddSingleton<SqliteConventionSetBuilder>()
                    .AddScoped<SqliteModificationCommandBatchFactory>()
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
