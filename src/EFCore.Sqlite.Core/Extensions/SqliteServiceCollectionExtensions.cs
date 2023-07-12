// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Update.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///     SQLite specific extension methods for <see cref="IServiceCollection" />.
/// </summary>
public static class SqliteServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the given Entity Framework <see cref="DbContext" /> as a service in the <see cref="IServiceCollection" />
    ///     and configures it to connect to a SQLite database.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is a shortcut for configuring a <see cref="DbContext" /> to use SQLite. It does not support all options.
    ///         Use <see cref="O:EntityFrameworkServiceCollectionExtensions.AddDbContext" /> and related methods for full control of
    ///         this process.
    ///     </para>
    ///     <para>
    ///         Use this method when using dependency injection in your application, such as with ASP.NET Core.
    ///         For applications that don't use dependency injection, consider creating <see cref="DbContext" />
    ///         instances directly with its constructor. The <see cref="DbContext.OnConfiguring" /> method can then be
    ///         overridden to configure the SQLite provider and connection string.
    ///     </para>
    ///     <para>
    ///         To configure the <see cref="DbContextOptions{TContext}" /> for the context, either override the
    ///         <see cref="DbContext.OnConfiguring" /> method in your derived context, or supply
    ///         an optional action to configure the <see cref="DbContextOptions" /> for the context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-di">Using DbContext with dependency injection</see> for more information and examples.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-sqlite">Accessing SQLite databases with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of context to be registered.</typeparam>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <param name="connectionString">The connection string of the database to connect to.</param>
    /// <param name="sqliteOptionsAction">An optional action to allow additional SQLite specific configuration.</param>
    /// <param name="optionsAction">An optional action to configure the <see cref="DbContextOptions" /> for the context.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddSqlite<TContext>(
        this IServiceCollection serviceCollection,
        string? connectionString,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null,
        Action<DbContextOptionsBuilder>? optionsAction = null)
        where TContext : DbContext
        => serviceCollection.AddDbContext<TContext>(
            (_, options) =>
            {
                optionsAction?.Invoke(options);
                options.UseSqlite(connectionString, sqliteOptionsAction);
            });

    /// <summary>
    ///     <para>
    ///         Adds the services required by the SQLite database provider for Entity Framework
    ///         to an <see cref="IServiceCollection" />.
    ///     </para>
    ///     <para>
    ///         Warning: Do not call this method accidentally. It is much more likely you need
    ///         to call <see cref="AddSqlite{TContext}" />.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     Calling this method is no longer necessary when building most applications, including those that
    ///     use dependency injection in ASP.NET or elsewhere.
    ///     It is only needed when building the internal service provider for use with
    ///     the <see cref="DbContextOptionsBuilder.UseInternalServiceProvider" /> method.
    ///     This is not recommend other than for some advanced scenarios.
    /// </remarks>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add services to.</param>
    /// <returns>
    ///     The same service collection so that multiple calls can be chained.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IServiceCollection AddEntityFrameworkSqlite(this IServiceCollection serviceCollection)
    {
        var builder = new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd<LoggingDefinitions, SqliteLoggingDefinitions>()
            .TryAdd<IDatabaseProvider, DatabaseProvider<SqliteOptionsExtension>>()
            .TryAdd<IRelationalTypeMappingSource, SqliteTypeMappingSource>()
            .TryAdd<ISqlGenerationHelper, SqliteSqlGenerationHelper>()
            .TryAdd<IRelationalAnnotationProvider, SqliteAnnotationProvider>()
            .TryAdd<IModelValidator, SqliteModelValidator>()
            .TryAdd<IProviderConventionSetBuilder, SqliteConventionSetBuilder>()
            .TryAdd<IModificationCommandBatchFactory, SqliteModificationCommandBatchFactory>()
            .TryAdd<IModificationCommandFactory, SqliteModificationCommandFactory>()
            .TryAdd<IRelationalConnection>(p => p.GetRequiredService<ISqliteRelationalConnection>())
            .TryAdd<IMigrationsSqlGenerator, SqliteMigrationsSqlGenerator>()
            .TryAdd<IRelationalDatabaseCreator, SqliteDatabaseCreator>()
            .TryAdd<IHistoryRepository, SqliteHistoryRepository>()
            .TryAdd<IRelationalQueryStringFactory, SqliteQueryStringFactory>()
            .TryAdd<IMethodCallTranslatorProvider, SqliteMethodCallTranslatorProvider>()
            .TryAdd<IAggregateMethodCallTranslatorProvider, SqliteAggregateMethodCallTranslatorProvider>()
            .TryAdd<IMemberTranslatorProvider, SqliteMemberTranslatorProvider>()
            .TryAdd<IQuerySqlGeneratorFactory, SqliteQuerySqlGeneratorFactory>()
            .TryAdd<IQueryableMethodTranslatingExpressionVisitorFactory, SqliteQueryableMethodTranslatingExpressionVisitorFactory>()
            .TryAdd<IRelationalSqlTranslatingExpressionVisitorFactory, SqliteSqlTranslatingExpressionVisitorFactory>()
            .TryAdd<IQueryTranslationPostprocessorFactory, SqliteQueryTranslationPostprocessorFactory>()
            .TryAdd<IUpdateSqlGenerator, SqliteUpdateSqlGenerator>()
            .TryAdd<ISqlExpressionFactory, SqliteSqlExpressionFactory>()
            .TryAdd<IRelationalParameterBasedSqlProcessorFactory, SqliteParameterBasedSqlProcessorFactory>()
            .TryAddProviderSpecificServices(
                b => b.TryAddScoped<ISqliteRelationalConnection, SqliteRelationalConnection>());

        builder.TryAddCoreServices();

        return serviceCollection;
    }
}
