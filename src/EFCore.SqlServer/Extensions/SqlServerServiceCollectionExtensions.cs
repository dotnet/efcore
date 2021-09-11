// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Migrations.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

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
        ///         Registers the given Entity Framework <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />
        ///         and configures it to connect to a SQL Server database.
        ///     </para>
        ///     <para>
        ///         This method is a shortcut for configuring a <see cref="DbContext" /> to use SQL Server. It does not support all options.
        ///         Use <see cref="M:EntityFrameworkServiceCollectionExtensions.AddDbContext"/> and related methods for full control of
        ///         this process.
        ///     </para>
        ///     <para>
        ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
        ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
        ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
        ///         overridden to configure the SQL Server provider and connection string.
        ///     </para>
        ///     <para>
        ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
        ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply 
        ///         an optional action to configure the <see cref="DbContextOptions" /> for the context. 
        ///     </para>
        ///     <para>
        ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
        ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and SQL Azure databases with EF Core</see>
        ///     for more information.
        /// </remarks>
        /// <typeparam name="TContext"> The type of context to be registered. </typeparam>
        /// <param name="serviceCollection"> The <see cref="IServiceCollection" /> to add services to. </param>
        /// <param name="connectionString"> The connection string of the database to connect to. </param>
        /// <param name="sqlServerOptionsAction"> An optional action to allow additional SQL Server specific configuration. </param>
        /// <param name="optionsAction"> An optional action to configure the <see cref="DbContextOptions" /> for the context. </param>
        /// <returns> The same service collection so that multiple calls can be chained. </returns>
        public static IServiceCollection AddSqlServer<TContext>(
            this IServiceCollection serviceCollection,
            string connectionString,
            Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null,
            Action<DbContextOptionsBuilder>? optionsAction = null)
            where TContext : DbContext
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));
            Check.NotEmpty(connectionString, nameof(connectionString));

            return serviceCollection.AddDbContext<TContext>(
                (serviceProvider, options) =>
                {
                    optionsAction?.Invoke(options);
                    options.UseSqlServer(connectionString, sqlServerOptionsAction);
                });
        }

        /// <summary>
        ///     <para>
        ///         Adds the services required by the Microsoft SQL Server database provider for Entity Framework
        ///         to an <see cref="IServiceCollection" />.
        ///     </para>
        ///     <para>
        ///         Warning: Do not call this method accidentally. It is much more likely you need
        ///         to call <see cref="AddSqlServer{TContext}" />.
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
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IServiceCollection AddEntityFrameworkSqlServer(this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            new EntityFrameworkRelationalServicesBuilder(serviceCollection)
                .TryAdd<LoggingDefinitions, SqlServerLoggingDefinitions>()
                .TryAdd<IDatabaseProvider, DatabaseProvider<SqlServerOptionsExtension>>()
                .TryAdd<IValueGeneratorCache>(p => p.GetRequiredService<ISqlServerValueGeneratorCache>())
                .TryAdd<IRelationalTypeMappingSource, SqlServerTypeMappingSource>()
                .TryAdd<ISqlGenerationHelper, SqlServerSqlGenerationHelper>()
                .TryAdd<IRelationalAnnotationProvider, SqlServerAnnotationProvider>()
                .TryAdd<IMigrationsAnnotationProvider, SqlServerMigrationsAnnotationProvider>()
                .TryAdd<IModelValidator, SqlServerModelValidator>()
                .TryAdd<IProviderConventionSetBuilder, SqlServerConventionSetBuilder>()
                .TryAdd<IUpdateSqlGenerator>(p => p.GetRequiredService<ISqlServerUpdateSqlGenerator>())
                .TryAdd<IEvaluatableExpressionFilter, SqlServerEvaluatableExpressionFilter>()
                .TryAdd<IRelationalTransactionFactory, SqlServerTransactionFactory>()
                .TryAdd<IModificationCommandBatchFactory, SqlServerModificationCommandBatchFactory>()
                .TryAdd<IValueGeneratorSelector, SqlServerValueGeneratorSelector>()
                .TryAdd<IRelationalConnection>(p => p.GetRequiredService<ISqlServerConnection>())
                .TryAdd<IMigrationsSqlGenerator, SqlServerMigrationsSqlGenerator>()
                .TryAdd<IRelationalDatabaseCreator, SqlServerDatabaseCreator>()
                .TryAdd<IHistoryRepository, SqlServerHistoryRepository>()
                .TryAdd<IExecutionStrategyFactory, SqlServerExecutionStrategyFactory>()
                .TryAdd<IRelationalQueryStringFactory, SqlServerQueryStringFactory>()
                .TryAdd<ICompiledQueryCacheKeyGenerator, SqlServerCompiledQueryCacheKeyGenerator>()
                .TryAdd<IQueryCompilationContextFactory, SqlServerQueryCompilationContextFactory>()
                .TryAdd<IMethodCallTranslatorProvider, SqlServerMethodCallTranslatorProvider>()
                .TryAdd<IMemberTranslatorProvider, SqlServerMemberTranslatorProvider>()
                .TryAdd<IQuerySqlGeneratorFactory, SqlServerQuerySqlGeneratorFactory>()
                .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, SqlServerSqlTranslatingExpressionVisitorFactory>()
                .TryAdd<IRelationalParameterBasedSqlProcessorFactory, SqlServerParameterBasedSqlProcessorFactory>()
                .TryAdd<INavigationExpansionExtensibilityHelper, SqlServerNavigationExpansionExtensibilityHelper>()
                .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, SqlServerQueryableMethodTranslatingExpressionVisitorFactory>()
                .TryAddProviderSpecificServices(
                    b => b
                        .TryAddSingleton<ISqlServerValueGeneratorCache, SqlServerValueGeneratorCache>()
                        .TryAddSingleton<ISqlServerUpdateSqlGenerator, SqlServerUpdateSqlGenerator>()
                        .TryAddSingleton<ISqlServerSequenceValueGeneratorFactory, SqlServerSequenceValueGeneratorFactory>()
                        .TryAddScoped<ISqlServerConnection, SqlServerConnection>())
                .TryAddCoreServices();

            return serviceCollection;
        }
    }
}
