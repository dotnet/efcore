// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Sql.Internal;
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
        ///         to an <see cref="IServiceCollection" />. You use this method when using dependency injection
        ///         in your application, such as with ASP.NET. For more information on setting up dependency
        ///         injection, see http://go.microsoft.com/fwlink/?LinkId=526890.
        ///     </para>
        ///     <para>
        ///         You only need to use this functionality when you want Entity Framework to resolve the services it uses
        ///         from an external dependency injection container. If you are not using an external
        ///         dependency injection container, Entity Framework will take care of creating the services it requires.
        ///     </para>
        /// </summary>
        /// <example>
        ///     <code>
        ///           public void ConfigureServices(IServiceCollection services)
        ///           {
        ///               var connectionString = "connection string to database";
        ///
        ///               services
        ///                   .AddEntityFrameworkSqlite()
        ///                   .AddDbContext&lt;MyContext&gt;((serviceProvider, options) =>
        ///                       options.UseSqlite(connectionString)
        ///                              .UseInternalServiceProvider(serviceProvider));
        ///           }
        ///       </code>
        /// </example>
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
                .TryAdd<IQuerySqlGeneratorFactory, SqliteQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalResultOperatorHandler, SqliteResultOperatorHandler>()

                // New Query Pipeline
                .TryAdd<IMethodCallTranslatorProvider, SqliteMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, SqliteMemberTranslatorProvider>()
                .TryAdd<IQuerySqlGeneratorFactory2, SqliteQuerySqlGeneratorFactory2>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, SqliteSqlTranslatingExpressionVisitorFactory>()

                .TryAddProviderSpecificServices(
                    b => b.TryAddScoped<ISqliteRelationalConnection, SqliteRelationalConnection>());

            builder.TryAddCoreServices();

            return serviceCollection;
        }
    }
}
