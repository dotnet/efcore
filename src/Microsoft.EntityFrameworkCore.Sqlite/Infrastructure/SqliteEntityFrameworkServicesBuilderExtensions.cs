// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqliteEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlite([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var service = builder.AddRelational().GetInfrastructure();

            service.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<SqliteDatabaseProviderServices, SqliteOptionsExtension>>());

            service.TryAdd(new ServiceCollection()
                .AddSingleton<SqliteValueGeneratorCache>()
                .AddSingleton<SqliteAnnotationProvider>()
                .AddSingleton<SqliteTypeMapper>()
                .AddSingleton<SqliteSqlGenerationHelper>()
                .AddSingleton<SqliteModelSource>()
                .AddSingleton<SqliteMigrationsAnnotationProvider>()
                .AddScoped<SqliteModelValidator>()
                .AddScoped<SqliteConventionSetBuilder>()
                .AddScoped<SqliteUpdateSqlGenerator>()
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
