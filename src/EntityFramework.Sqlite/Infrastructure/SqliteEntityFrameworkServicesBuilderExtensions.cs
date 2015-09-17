// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Infrastructure.Internal;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Query.ExpressionTranslators.Internal;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Framework.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    public static class SqliteEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlite([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var service = builder.AddRelational().GetService();

            service.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<SqliteDatabaseProviderServices, SqliteOptionsExtension>>());

            service.TryAdd(new ServiceCollection()
                .AddSingleton<SqliteValueGeneratorCache>()
                .AddSingleton<SqliteUpdateSqlGenerator>()
                .AddSingleton<SqliteAnnotationProvider>()
                .AddSingleton<SqliteTypeMapper>()
                .AddSingleton<SqliteModelSource>()
                .AddSingleton<SqliteMigrationsAnnotationProvider>()
                .AddSingleton<SqliteConventionSetBuilder>()
                .AddScoped<SqliteModificationCommandBatchFactory>()
                .AddScoped<SqliteDatabaseProviderServices>()
                .AddScoped<SqliteRelationalConnection>()
                .AddScoped<SqliteMigrationsSqlGenerator>()
                .AddScoped<SqliteDatabaseCreator>()
                .AddScoped<SqliteHistoryRepository>()
                .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<SqliteCompositeMemberTranslator>()
                .AddScoped<SqliteCompositeMethodCallTranslator>()
                .AddScoped<SqliteQuerySqlGeneratorFactory>();
    }
}
