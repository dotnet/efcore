// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     In-memory specific extension methods for <see cref="DbContextOptionsBuilder" />.
/// </summary>
public static class InMemoryDbContextOptionsExtensions
{
    /// <summary>
    ///     Configures the context to connect to an in-memory database.
    ///     The in-memory database is shared anywhere the same name is used, but only for a given
    ///     service provider. To use the same in-memory database across service providers, call
    ///     <see
    ///         cref="UseInMemoryDatabase{TContext}(DbContextOptionsBuilder{TContext},string,InMemoryDatabaseRoot,Action{InMemoryDbContextOptionsBuilder})" />
    ///     passing a shared <see cref="InMemoryDatabaseRoot" /> on which to root the database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context being configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="databaseName">
    ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
    ///     independently of the context. The in-memory database is shared anywhere the same name is used.
    /// </param>
    /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseInMemoryDatabase<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string databaseName,
        Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseInMemoryDatabase(
            (DbContextOptionsBuilder)optionsBuilder, databaseName, inMemoryOptionsAction);

    /// <summary>
    ///     Configures the context to connect to a named in-memory database.
    ///     The in-memory database is shared anywhere the same name is used, but only for a given
    ///     service provider. To use the same in-memory database across service providers, call
    ///     <see cref="UseInMemoryDatabase(DbContextOptionsBuilder,string,InMemoryDatabaseRoot,Action{InMemoryDbContextOptionsBuilder})" />
    ///     passing a shared <see cref="InMemoryDatabaseRoot" /> on which to root the database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="databaseName">
    ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
    ///     independently of the context. The in-memory database is shared anywhere the same name is used.
    /// </param>
    /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseInMemoryDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        string databaseName,
        Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null)
        => UseInMemoryDatabase(optionsBuilder, databaseName, null, inMemoryOptionsAction);

    /// <summary>
    ///     Configures the context to connect to an in-memory database.
    ///     The in-memory database is shared anywhere the same name is used, but only for a given
    ///     service provider.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TContext">The type of context being configured.</typeparam>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="databaseName">
    ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
    ///     independently of the context. The in-memory database is shared anywhere the same name is used.
    /// </param>
    /// <param name="databaseRoot">
    ///     All in-memory databases will be rooted in this object, allowing the application
    ///     to control their lifetime. This is useful when sometimes the context instance
    ///     is created explicitly with <see langword="new" /> while at other times it is resolved using dependency injection.
    /// </param>
    /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder<TContext> UseInMemoryDatabase<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string databaseName,
        InMemoryDatabaseRoot? databaseRoot,
        Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null)
        where TContext : DbContext
        => (DbContextOptionsBuilder<TContext>)UseInMemoryDatabase(
            (DbContextOptionsBuilder)optionsBuilder, databaseName, databaseRoot, inMemoryOptionsAction);

    /// <summary>
    ///     Configures the context to connect to a named in-memory database.
    ///     The in-memory database is shared anywhere the same name is used, but only for a given
    ///     service provider.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-in-memory">The EF Core in-memory database provider</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The builder being used to configure the context.</param>
    /// <param name="databaseName">
    ///     The name of the in-memory database. This allows the scope of the in-memory database to be controlled
    ///     independently of the context. The in-memory database is shared anywhere the same name is used.
    /// </param>
    /// <param name="databaseRoot">
    ///     All in-memory databases will be rooted in this object, allowing the application
    ///     to control their lifetime. This is useful when sometimes the context instance
    ///     is created explicitly with <see langword="new" /> while at other times it is resolved using dependency injection.
    /// </param>
    /// <param name="inMemoryOptionsAction">An optional action to allow additional in-memory specific configuration.</param>
    /// <returns>The options builder so that further configuration can be chained.</returns>
    public static DbContextOptionsBuilder UseInMemoryDatabase(
        this DbContextOptionsBuilder optionsBuilder,
        string databaseName,
        InMemoryDatabaseRoot? databaseRoot,
        Action<InMemoryDbContextOptionsBuilder>? inMemoryOptionsAction = null)
    {
        Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        Check.NotEmpty(databaseName, nameof(databaseName));

        var extension = optionsBuilder.Options.FindExtension<InMemoryOptionsExtension>()
            ?? new InMemoryOptionsExtension();

        extension = extension.WithStoreName(databaseName);

        if (databaseRoot != null)
        {
            extension = extension.WithDatabaseRoot(databaseRoot);
        }

        extension = extension.WithNullabilityCheckEnabled(true);

        ConfigureWarnings(optionsBuilder);

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        inMemoryOptionsAction?.Invoke(new InMemoryDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    private static void ConfigureWarnings(DbContextOptionsBuilder optionsBuilder)
    {
        // Set warnings defaults
        var coreOptionsExtension
            = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()
            ?? new CoreOptionsExtension();

        coreOptionsExtension = coreOptionsExtension.WithWarningsConfiguration(
            coreOptionsExtension.WarningsConfiguration.TryWithExplicit(
                InMemoryEventId.TransactionIgnoredWarning, WarningBehavior.Throw));

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(coreOptionsExtension);
    }
}
