// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     A single page of results returned from a user query; can be used to paginate through long result-sets.
///     Returned by <see cref="CosmosQueryableExtensions.ToPageAsync{T}" />.
/// </summary>
/// <param name="values">The values contained in this page.</param>
/// <param name="continuationToken">
///     The continuation token for fetching further results from the query. Is <see langword="null" /> or empty when there are no more
///     results.
/// </param>
/// <typeparam name="T">The type of values contained in the page.</typeparam>
[Experimental(EFDiagnostics.PagingExperimental)]
public readonly struct CosmosPage<T>(IReadOnlyList<T> values, string? continuationToken)
{
    /// <summary>
    ///     The values contained in this page.
    /// </summary>
    public IReadOnlyList<T> Values { get; } = values;

    /// <summary>
    ///     The continuation token for fetching further results from the query. Is <see langword="null" /> or empty when there are no more
    ///     results.
    /// </summary>
    public string? ContinuationToken { get; } = continuationToken;
}
