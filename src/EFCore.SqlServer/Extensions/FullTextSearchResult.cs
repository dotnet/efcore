// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Represents the results from a call to
///     <see cref="SqlServerQueryableExtensions.FreeTextTable{T, TKey}(DbSet{T}, Expression{Func{T, object}}, string, string?, int?)" /> or
///     <see cref="SqlServerQueryableExtensions.ContainsTable{T, TKey}(DbSet{T}, Expression{Func{T, object}}, string, string?, int?)" />.
/// </summary>
/// <typeparam name="TKey">The type of the full-text key column for the table being searched.</typeparam>
public readonly struct FullTextSearchResult<TKey>(TKey key, int rank)
{
    /// <summary>
    ///     The full-text key value of the matching row.
    /// </summary>
    /// <remarks>
    ///     This corresponds to the unique key column defined in the full-text index.
    ///     Use this value to join back to the original table.
    /// </remarks>
    public TKey Key { get; } = key;

    /// <summary>
    ///     The rank value indicating the relevance of the match.
    /// </summary>
    /// <remarks>
    ///     Higher values indicate more relevant matches. The rank is computed based on the number of matches,
    ///     the number of unique words matching, and other factors depending on the search method used.
    /// </remarks>
    public int Rank { get; } = rank;

    /// <summary>
    ///     Deconstructs the result into the key and rank.
    /// </summary>
    /// <param name="key">The full-text key value of the matching row.</param>
    /// <param name="rank">The rank value indicating the relevance of the match.</param>
    public void Deconstruct(out TKey key, out int rank)
    {
        key = Key;
        rank = Rank;
    }
}
