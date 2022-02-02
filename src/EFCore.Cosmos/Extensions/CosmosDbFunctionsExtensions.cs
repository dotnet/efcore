// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class CosmosDbFunctionsExtensions
{
    /// <summary>
    ///     A DbFunction method stub that can be used in LINQ queries to target the Cosmos DB <c>IS_DEFINED</c> store function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="propertyReference">The property to validate.</param>
    public static bool IsDefined<TProperty>(
        this DbFunctions _,
        TProperty propertyReference)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsDefined)));
}
