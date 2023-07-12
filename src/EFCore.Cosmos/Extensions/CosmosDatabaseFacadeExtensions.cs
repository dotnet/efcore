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
