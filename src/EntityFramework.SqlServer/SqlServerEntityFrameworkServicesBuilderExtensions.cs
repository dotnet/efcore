// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.SqlServer;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Query.ExpressionTranslators;
using Microsoft.Data.Entity.SqlServer.Update;
using Microsoft.Data.Entity.SqlServer.ValueGeneration;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
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
                    .AddSingleton<ISqlServerUpdateSqlGenerator, SqlServerUpdateSqlGenerator>()
                    .AddSingleton<SqlServerTypeMapper>()
                    .AddSingleton<SqlServerModelSource>()
                    .AddSingleton<SqlServerMetadataExtensionProvider>()
                    .AddSingleton<SqlServerMigrationsAnnotationProvider>()
                    .AddScoped<ISqlServerSequenceValueGeneratorFactory, SqlServerSequenceValueGeneratorFactory>()
                    .AddScoped<SqlServerModificationCommandBatchFactory>()
                    .AddScoped<SqlServerValueGeneratorSelector>()
                    .AddScoped<SqlServerDatabaseProviderServices>()
                    .AddScoped<ISqlServerConnection, SqlServerConnection>()
                    .AddScoped<SqlServerMigrationsSqlGenerator>()
                    .AddScoped<SqlServerDatabaseCreator>()
                    .AddScoped<SqlServerHistoryRepository>()
                    .AddQuery());

            return builder;
        }

        private static IServiceCollection AddQuery(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<SqlServerQueryCompilationContextFactory>()
                .AddScoped<SqlServerCompositeExpressionFragmentTranslator>()
                .AddScoped<SqlServerCompositeMemberTranslator>()
                .AddScoped<SqlServerCompositeMethodCallTranslator>();
        }
    }
}
