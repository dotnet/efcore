// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Internal;
using Microsoft.Data.Entity.Query.ExpressionTranslators.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Query.Sql.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqlServerEntityFrameworkServicesBuilderExtensions
    {
        public static EntityFrameworkServicesBuilder AddSqlServer([NotNull] this EntityFrameworkServicesBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            var service = builder.AddRelational().GetService();

            service.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProvider, DatabaseProvider<SqlServerDatabaseProviderServices, SqlServerOptionsExtension>>());

            service.TryAdd(new ServiceCollection()
                .AddSingleton<SqlServerConventionSetBuilder>()
                .AddSingleton<ISqlServerValueGeneratorCache, SqlServerValueGeneratorCache>()
                .AddSingleton<SqlServerTypeMapper>()
                .AddSingleton<SqlServerSqlGenerator>()
                .AddSingleton<SqlServerModelSource>()
                .AddSingleton<SqlServerAnnotationProvider>()
                .AddSingleton<SqlServerMigrationsAnnotationProvider>()
                .AddScoped<ISqlServerUpdateSqlGenerator, SqlServerUpdateSqlGenerator>()
                .AddScoped<ISqlServerSequenceValueGeneratorFactory, SqlServerSequenceValueGeneratorFactory>()
                .AddScoped<SqlServerModificationCommandBatchFactory>()
                .AddScoped<SqlServerValueGeneratorSelector>()
                .AddScoped<SqlServerDatabaseProviderServices>()
                .AddScoped<ISqlServerConnection, SqlServerConnection>()
                .AddScoped<SqlServerMigrationsSqlGenerator>()
                .AddScoped<SqlServerDatabaseCreator>()
                .AddScoped<SqlServerHistoryRepository>()
                .AddScoped<SqlServerQueryModelVisitorFactory>()
                .AddScoped<SqlServerCompiledQueryCacheKeyGenerator>()
                .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
            => serviceCollection
                .AddScoped<SqlServerQueryCompilationContextFactory>()
                .AddScoped<SqlServerCompositeMemberTranslator>()
                .AddScoped<SqlServerCompositeMethodCallTranslator>()
                .AddScoped<SqlServerQuerySqlGeneratorFactory>();
    }
}
