// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Query.Sql.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    ///     SQL Server specific extension methods for <see cref="IServiceCollection" />.
    /// </summary>
    public static class SqlServerServiceCollectionExtensions
    {
        /// <summary>
        ///     <para>
        ///         Adds the services required by the Microsoft SQL Server database provider for Entity Framework
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
        ///                   .AddEntityFrameworkSqlServer()
        ///                   .AddDbContext&lt;MyContext&gt;((serviceProvider, options) =>
        ///                       options.UseSqlServer(connectionString)
        ///                              .UseInternalServiceProvider(serviceProvider));
        ///           }
        ///       </code>
        /// </example>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <returns>
        ///     The same service collection so that multiple calls can be chained.
        /// </returns>
        public static IServiceCollection AddEntityFrameworkSqlServer([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var serviceCollectionMap = new ServiceCollectionMap(serviceCollection)
                .TryAddSingletonEnumerable<IDatabaseProvider, DatabaseProvider<SqlServerOptionsExtension>>()
                .TryAddSingleton<ISqlServerValueGeneratorCache, SqlServerValueGeneratorCache>()
                .TryAddSingleton<IValueGeneratorCache>(p => p.GetService<ISqlServerValueGeneratorCache>())
                .TryAddSingleton<IRelationalTypeMapper, SqlServerTypeMapper>()
                .TryAddSingleton<ISqlGenerationHelper, SqlServerSqlGenerationHelper>()
                .TryAddSingleton<IRelationalAnnotationProvider, SqlServerAnnotationProvider>()
                .TryAddSingleton<IMigrationsAnnotationProvider, SqlServerMigrationsAnnotationProvider>()
                .TryAddScoped<IRelationalValueBufferFactoryFactory, UntypedRelationalValueBufferFactoryFactory>()
                .TryAddScoped<IModelValidator, SqlServerModelValidator>()
                .TryAddScoped<IConventionSetBuilder, SqlServerConventionSetBuilder>()
                .TryAddScoped<ISqlServerUpdateSqlGenerator, SqlServerUpdateSqlGenerator>()
                .TryAddScoped<IUpdateSqlGenerator>(p => p.GetService<ISqlServerUpdateSqlGenerator>())
                .TryAddScoped<ISqlServerSequenceValueGeneratorFactory, SqlServerSequenceValueGeneratorFactory>()
                .TryAddScoped<IModificationCommandBatchFactory, SqlServerModificationCommandBatchFactory>()
                .TryAddScoped<IValueGeneratorSelector, SqlServerValueGeneratorSelector>()
                .TryAddScoped<ISqlServerConnection, SqlServerConnection>()
                .TryAddScoped<IRelationalConnection>(p => p.GetService<ISqlServerConnection>())
                .TryAddScoped<IMigrationsSqlGenerator, SqlServerMigrationsSqlGenerator>()
                .TryAddScoped<IRelationalDatabaseCreator, SqlServerDatabaseCreator>()
                .TryAddScoped<IHistoryRepository, SqlServerHistoryRepository>()
                .TryAddScoped<IEntityQueryModelVisitorFactory, SqlServerQueryModelVisitorFactory>()
                .TryAddScoped<ICompiledQueryCacheKeyGenerator, SqlServerCompiledQueryCacheKeyGenerator>()
                .TryAddScoped<IExecutionStrategyFactory, SqlServerExecutionStrategyFactory>()
                .TryAddScoped<IQueryCompilationContextFactory, SqlServerQueryCompilationContextFactory>()
                .TryAddScoped<IMemberTranslator, SqlServerCompositeMemberTranslator>()
                .TryAddScoped<IMethodCallTranslator, SqlServerCompositeMethodCallTranslator>()
                .TryAddScoped<IQuerySqlGeneratorFactory, SqlServerQuerySqlGeneratorFactory>();

            ServiceCollectionRelationalProviderInfrastructure.TryAddDefaultRelationalServices(serviceCollectionMap);

            return serviceCollection;
        }
    }
}
