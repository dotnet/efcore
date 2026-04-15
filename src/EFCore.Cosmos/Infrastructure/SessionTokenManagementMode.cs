// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;

/// <summary>
///     Defines the behavior of EF Core regarding the management of Cosmos DB session tokens.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-cosmos-session">Cosmos session consistency</see> for more info.
/// </remarks>
public enum SessionTokenManagementMode
{
    /// <summary>
    ///     The default mode.
    ///     Uses the underlying Cosmos DB SDK automatic session token management.
    ///     EF will not track or parse session tokens returned from Cosmos DB. <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> and <see cref="CosmosDatabaseFacadeExtensions.GetSessionTokens(DatabaseFacade)"/> methods will throw when invoked.
    /// </summary>
    FullyAutomatic,

    /// <summary>
    ///     Allows the usage of <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> to overwrite the default Cosmos DB SDK automatic session token management.
    ///     If 'UseSessionTokens' has not been invoked for a container, the default Cosmos DB SDK automatic session token management will be used.
    ///     EF will track and parse session tokens returned from Cosmos DB, which can be retrieved via <see cref="CosmosDatabaseFacadeExtensions.GetSessionTokens(DatabaseFacade)"/>.
    /// </summary>
    SemiAutomatic,

    /// <summary>
    ///     Fully overwrites the Cosmos DB SDK automatic session token management, and only uses session tokens specified via <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/>.
    ///     If 'UseSessionTokens' has not been invoked for a container, no session token will be used.
    ///     EF will track and parse session tokens returned from Cosmos DB, which can be retrieved via <see cref="CosmosDatabaseFacadeExtensions.GetSessionTokens(DatabaseFacade)"/>.
    /// </summary>
    Manual,

    /// <summary>
    ///     Same as <see cref="Manual"/>, but will throw an exception if <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> was not invoked before executing a read.
    /// </summary>
    EnforcedManual
}
