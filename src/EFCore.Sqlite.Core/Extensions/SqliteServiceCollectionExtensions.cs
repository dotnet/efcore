// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Update.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     SQLite specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class SqliteServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the SQLite database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Calling this method is no longer necessary when building most applications, including those that
        ///         use dependency injection in ASP.NET or elsewhere.
        ///         It is only needed when building the internal service provider for use with
        ///         the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
        ///         This is not recommend other than for some advanced scenarios.
        ///     </para>
        /// </summary>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkSqlite([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, SqliteLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<SqliteOptionsExtension>>()
                .TryAdd<IRelationalTypeMappingSource, SqliteTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, SqliteSqlGenerationHelper>()
                .TryAdd<IMigrationsAnnotationProvider, SqliteMigrationsAnnotationProvider>()
                .TryAdd<IModelValidator, SqliteModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, SqliteConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator, SqliteUpdateSqlGenerator>()
                .TryAdd<IModificationCommandBatchFactory, SqliteModificationCommandBatchFactory>()
                .TryAdd<IRelationalConnection>(p => p.GetService<ISqliteRelationalConnection>())
                .TryAdd<IMigrationsSqlGenerator, SqliteMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, SqliteDatabaseCreator>()
                .TryAdd<IHistoryRepository, SqliteHistoryRepository>()

                // New Query Pipeline
                .TryAdd<IMethodCallTranslatorProvider, SqliteMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, SqliteMemberTranslatorProvider>()
                .TryAdd<IQuerySqlGeneratorFactory, SqliteQuerySqlGeneratorFactory>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, SqliteQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, SqliteSqlTranslatingExpressionVisitorFactory>()
                .TryAddProviderSpecificServices(
                    b => b.TryAddScoped<ISqliteRelationalConnection, SqliteRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
