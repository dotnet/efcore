// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        ///          public void ConfigureServices(IServiceCollection services)
        ///          {
        ///              var connectionString = "connection string to database";
        /// 
        ///              services
        ///                  .AddEntityFrameworkSqlite()
        ///                  .AddDbContext&lt;MyContext&gt;((serviceProvider, options) =>
        ///                      options.UseSqlite(connectionString)
        ///                             .UseInternalServiceProvider(serviceProvider));
        ///          }
        ///      </code>
        /// </example>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkSqlite([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.TryAddEnumerable(
                ServiceDescriptor.Singleton<IDatabaseProvider, DatabaseProvider<SqliteOptionsExtension>>());

            serviceCollection.TryAdd(new ServiceCollection()
                .AddSingleton<IRelationalAnnotationProvider, SqliteAnnotationProvider>()
                .AddSingleton<IRelationalTypeMapper, SqliteTypeMapper>()
                .AddSingleton<ISqlGenerationHelper, SqliteSqlGenerationHelper>()
                .AddSingleton<IMigrationsAnnotationProvider, SqliteMigrationsAnnotationProvider>()
                .AddScoped<IConventionSetBuilder, SqliteConventionSetBuilder>()
                .AddScoped<IUpdateSqlGenerator, SqliteUpdateSqlGenerator>()
                .AddScoped<IModificationCommandBatchFactory, SqliteModificationCommandBatchFactory>()
                .AddScoped<ISqliteRelationalConnection, SqliteRelationalConnection>()
                .AddScoped<IRelationalConnection>(p => p.GetService<ISqliteRelationalConnection>())
                .AddScoped<IMigrationsSqlGenerator, SqliteMigrationsSqlGenerator>()
                .AddScoped<IRelationalDatabaseCreator, SqliteDatabaseCreator>()
                .AddScoped<IHistoryRepository, SqliteHistoryRepository>()
                .AddScoped<IMemberTranslator, SqliteCompositeMemberTranslator>()
                .AddScoped<IMethodCallTranslator, SqliteCompositeMethodCallTranslator>()
                .AddScoped<IQuerySqlGeneratorFactory, SqliteQuerySqlGeneratorFactory>());

            ServiceCollectionRelationalProviderInfrastructure.TryAddDefaultRelationalServices(serviceCollection);

            return serviceCollection;
        }
    }
}
