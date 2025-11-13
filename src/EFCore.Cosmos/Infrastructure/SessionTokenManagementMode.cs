// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Infrastructure;

/// <summary>
///     Defines the behaviour of EF regarding the management of Cosmos DB session tokens.
/// </summary>
/// <remarks>
///     See <see href="https://docs.azure.cn/en-us/cosmos-db/consistency-levels#session-consistency">Consistency level choices</see> for more info.
/// </remarks>
public enum SessionTokenManagementMode
{
    /// <summary>
    ///     The default mode.
    ///     Uses the underlying Cosmos DB SDK automatic session token management.
    ///     EF will not track or parse session tokens returned from Cosmos DB. <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> and <see cref="CosmosDatabaseFacadeExtensions.GetSessionTokens(DatabaseFacade)"/> methods will throw when invoked.
    ///     Use this mode when every request for the same user will land on the same instance of your app.
    ///     This means you either have 1 application instance, or maintain session affinity between requests.
    ///     Otherwhise, use of one of the other modes is required to guarantee session consistency between requests.
    /// </summary>
    FullyAutomatic,

    /// <summary>
    ///     Allows the usage of <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> to overwrite the default Cosmos DB SDK automatic session token management by use of the <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> method on a <see cref="DbContext.Database"/> instance.
    ///     If <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> has not been invoked for an container, the default Cosmos DB SDK automatic session token management will be used.
    ///     EF will track and parse session tokens returned from Cosmos DB, which can be retrieved via <see cref="CosmosDatabaseFacadeExtensions.GetSessionTokens(DatabaseFacade)"/>.
    /// </summary>
    SemiAutomatic,

    /// <summary>
    ///     Fully overwrites the Cosmos DB SDK automatic session token management, and only uses session tokens specified via <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/>.
    ///     If <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> has not been invoked for an container, no session token will be used.
    ///     EF will track and parse session tokens returned from Cosmos DB, which can be retrieved via <see cref="CosmosDatabaseFacadeExtensions.GetSessionTokens(DatabaseFacade)"/>.
    /// </summary>
    Manual,

    /// <summary>
    ///     Same as <see cref="Manual"/>, but will throw an exception if <see cref="CosmosDatabaseFacadeExtensions.UseSessionTokens(DatabaseFacade, IReadOnlyDictionary{string, string?})"/> was not invoked before executong a read.
    /// </summary>
    EnforcedManual
}
