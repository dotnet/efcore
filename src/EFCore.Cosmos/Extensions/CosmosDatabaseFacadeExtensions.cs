// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Extension methods for the <see cref="DatabaseFacade" /> returned from <see cref="DbContext.Database" />
///     that can be used only with the Cosmos provider.
/// </summary>
public static class CosmosDatabaseFacadeExtensions
{
    /// <summary>
    ///     Gets the underlying <see cref="CosmosClient" /> for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The <see cref="CosmosClient" /></returns>
    public static CosmosClient GetCosmosClient(this DatabaseFacade databaseFacade)
        => GetService<ISingletonCosmosClientWrapper>(databaseFacade).Client;

    /// <summary>
    ///     Gets the composite session token for the default container for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>See https://aka.ms/efcore-docs-cosmos-session for more information.</remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The session token for the default container in the context, or <see langword="null"/> if none present.</returns>
    public static string? GetSessionToken(this DatabaseFacade databaseFacade)
        => GetSessionTokenStorage(databaseFacade).GetDefaultContainerTrackedToken();

    /// <summary>
    ///     Gets a dictionary that contains the composite session token per container for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>See https://aka.ms/efcore-docs-cosmos-session for more information.</remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The session token dictionary.</returns>
    public static IReadOnlyDictionary<string, string?> GetSessionTokens(this DatabaseFacade databaseFacade)
        => GetSessionTokenStorage(databaseFacade).GetTrackedTokens();

    /// <summary>
    ///     Sets the composite session token for the default container for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>See https://aka.ms/efcore-docs-cosmos-session for more information.</remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sessionToken">The session token to set.</param>
    public static void UseSessionToken(this DatabaseFacade databaseFacade, string sessionToken)
        => GetSessionTokenStorage(databaseFacade).SetDefaultContainerSessionToken(sessionToken);

    /// <summary>
    ///     Sets the composite sessions token per container for this <see cref="DbContext" /> with the tokens specified in <paramref name="sessionTokens"/>.
    /// </summary>
    /// <remarks>See https://aka.ms/efcore-docs-cosmos-session for more information.</remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sessionTokens">The session tokens to set per container.</param>
    public static void UseSessionTokens(this DatabaseFacade databaseFacade, IReadOnlyDictionary<string, string?> sessionTokens)
    {
        var sessionTokenStorage = GetSessionTokenStorage(databaseFacade);

        sessionTokenStorage.SetSessionTokens(sessionTokens);
    }

    /// <summary>
    ///     Appends the composite session token for the default container for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>See https://aka.ms/efcore-docs-cosmos-session for more information.</remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sessionToken">The session token to append.</param>
    public static void AppendSessionToken(this DatabaseFacade databaseFacade, string sessionToken)
        => GetSessionTokenStorage(databaseFacade).AppendDefaultContainerSessionToken(sessionToken);

    /// <summary>
    ///     Appends the composite sessions token per container for this <see cref="DbContext" /> with the tokens specified in <paramref name="sessionTokens"/>.
    /// </summary>
    /// <remarks>See https://aka.ms/efcore-docs-cosmos-session for more information.</remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <param name="sessionTokens">The session tokens to append per container.</param>
    public static void AppendSessionTokens(this DatabaseFacade databaseFacade, IReadOnlyDictionary<string, string> sessionTokens)
    {
        var sessionTokenStorage = GetSessionTokenStorage(databaseFacade);

        sessionTokenStorage.AppendSessionTokens(sessionTokens);
    }

    private static ISessionTokenStorage GetSessionTokenStorage(DatabaseFacade databaseFacade)
    {
        var db = GetService<IDatabase>(databaseFacade);
        if (db is not CosmosDatabaseWrapper dbWrapper)
        {
            throw new InvalidOperationException(CosmosStrings.CosmosNotInUse);
        }

        return dbWrapper.SessionTokenStorage;
    }

    private static TService GetService<TService>(IInfrastructure<IServiceProvider> databaseFacade)
        where TService : class
    {
        var service = databaseFacade.GetService<TService>();
        if (service == null)
        {
            throw new InvalidOperationException(CosmosStrings.CosmosNotInUse);
        }

        return service;
    }

    /// <summary>
    ///     Gets the configured database name for this <see cref="DbContext" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
    /// <returns>The database name.</returns>
    public static string GetCosmosDatabaseId(this DatabaseFacade databaseFacade)
    {
        var cosmosOptions = databaseFacade.GetService<IDbContextOptions>().FindExtension<CosmosOptionsExtension>();
        if (cosmosOptions == null)
        {
            throw new InvalidOperationException(CosmosStrings.CosmosNotInUse);
        }

        return cosmosOptions.DatabaseName;
    }

    /// <summary>
    ///     Returns <see langword="true" /> if the database provider currently in use is the Cosmos provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method can only be used after the <see cref="DbContext" /> has been configured because
    ///         it is only then that the provider is known. This means that this method cannot be used
    ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
    ///         provider to use as part of configuring the context.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="database">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns><see langword="true" /> if the Cosmos provider is being used.</returns>
    public static bool IsCosmos(this DatabaseFacade database)
        => database.ProviderName == typeof(CosmosOptionsExtension).Assembly.GetName().Name;
}
