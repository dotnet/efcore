// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Sqlite;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Query.ExpressionTranslators;
using Microsoft.Data.Entity.Sqlite.Update;
using Microsoft.Data.Entity.Sqlite.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection.Extensions;

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
                    .AddSingleton<SqliteMigrationsAnnotationProvider>()
                    .AddSingleton<SqliteConventionSetBuilder>()
                    .AddScoped<SqliteModificationCommandBatchFactory>()
                    .AddScoped<SqliteDatabaseProviderServices>()
                    .AddScoped<SqliteDatabaseConnection>()
                    .AddScoped<SqliteMigrationsSqlGenerator>()
                    .AddScoped<SqliteDatabaseCreator>()
                    .AddScoped<SqliteHistoryRepository>()
                    .AddQuery());

            return services;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<SqliteCompositeExpressionFragmentTranslator>()
                .AddScoped<SqliteCompositeMemberTranslator>()
                .AddScoped<SqliteCompositeMethodCallTranslator>()
                .AddScoped<SqliteQuerySqlGeneratorFactory>();
        }
    }
}
