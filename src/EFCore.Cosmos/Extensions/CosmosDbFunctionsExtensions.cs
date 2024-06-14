// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Extensions;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Cosmos with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosDbFunctionsExtensions
{
    /// <summary>
    ///     Returns a boolean indicating if the property has been assigned a value. Corresponds to the Cosmos <c>IS_DEFINED</c> function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Cosmos with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="expression">The expression to check.</param>
    /// <seealso href="https://learn.microsoft.com/azure/cosmos-db/nosql/query/is-defined">Cosmos <c>IS_DEFINED_</c> function</seealso>
    public static bool IsDefined(this DbFunctions _, object? expression)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(IsDefined)));

    /// <summary>
    ///     Coalesces a Cosmos <c>undefined</c> value via the <c>??</c> operator.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Cosmos with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="expression1">
    ///     The expression to coalesce. This expression will be returned unless it is <c>undefined</c>, in which case
    ///     <paramref name="expression2" /> will be returned.
    /// </param>
    /// <param name="expression2">The expression to be returned if <paramref name="expression1" /> is <c>undefined</c>.</param>
    /// <seealso href="https://learn.microsoft.com/azure/cosmos-db/nosql/query/ternary-coalesce-operators#coalesce-operator">Cosmos coalesce operator</seealso>
    public static T CoalesceUndefined<T>(
        this DbFunctions _,
        T expression1,
        T expression2)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(CoalesceUndefined)));
}
