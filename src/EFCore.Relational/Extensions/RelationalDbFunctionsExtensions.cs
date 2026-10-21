// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Provides CLR methods that get translated to database functions when used in LINQ to Entities queries.
///     The methods on this class are accessed via <see cref="EF.Functions" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
/// </remarks>
public static class RelationalDbFunctionsExtensions
{
    /// <summary>
    ///     Explicitly specifies a collation to be used in a LINQ query. Can be used to generate fragments such as
    ///     <c>WHERE customer.name COLLATE 'de_DE' = 'John Doe'</c>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The available collations and their names vary across databases, consult your database's documentation for more
    ///         information.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TProperty">The type of the operand on which the collation is being specified.</typeparam>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="operand">The operand to which to apply the collation.</param>
    /// <param name="collation">The name of the collation.</param>
    public static TProperty Collate<TProperty>(this DbFunctions _, TProperty operand, [NotParameterized] string collation)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Collate)));

    /// <summary>
    ///     Returns the smallest value from the given list of values. Usually corresponds to the <c>LEAST</c> SQL function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The list of values from which return the smallest value.</param>
    public static T Least<T>(this DbFunctions _, [NotParameterized] params T[] values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Least)));

    /// <summary>
    ///     Returns the greatest value from the given list of values. Usually corresponds to the <c>GREATEST</c> SQL function.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="values">The list of values from which return the greatest value.</param>
    public static T Greatest<T>(this DbFunctions _, [NotParameterized] params T[] values)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Greatest)));

    /// <summary>
    ///     Returns a value indicating whether a given JSON path exists within the specified JSON.
    /// </summary>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="json">The JSON value to check.</param>
    /// <param name="path">The JSON path to look for.</param>
    public static bool JsonPathExists(this DbFunctions _, object json, string path)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(JsonPathExists)));

    /// <summary>
    ///     When used as the key selector of a LINQ <c>GroupBy</c>, groups by the given key using the SQL <c>ROLLUP</c> construct:
    ///     in addition to the groups of the full key, rows are returned for each prefix of the key's members (e.g.
    ///     <c>GROUP BY ROLLUP (a, b)</c> groups by <c>(a, b)</c>, by <c>(a)</c> and by <c>()</c>).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The key must be a scalar or an anonymous/user type composed of nullable scalar members: in rows which aren't
    ///         grouped by a member, that member is <see langword="null" />. Use <see cref="Grouping{T}" /> to distinguish these
    ///         rows from groups whose key value is <see langword="null" />.
    ///     </para>
    ///     <para>
    ///         Only aggregate operators can be applied to the groupings (e.g. <c>g.Count()</c>,
    ///         <c>g.Select(e => e.Id).Distinct().Count()</c>).
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// context.Sales
    ///     .GroupBy(s => EF.Functions.Rollup(new { s.Category, s.Product }))
    ///     .Select(g => new { g.Key.Category, g.Key.Product, Clients = g.Select(s => s.ClientId).Distinct().Count() });
    ///     </code>
    /// </example>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="key">The grouping key.</param>
    /// <returns>The grouping key.</returns>
    public static TKey Rollup<TKey>(this DbFunctions _, TKey key)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Rollup)));

    /// <summary>
    ///     When used as the key selector of a LINQ <c>GroupBy</c>, groups by the given key using the SQL <c>CUBE</c> construct:
    ///     rows are returned for every combination of the key's members (e.g. <c>GROUP BY CUBE (a, b)</c> groups by <c>(a, b)</c>,
    ///     by <c>(a)</c>, by <c>(b)</c> and by <c>()</c>).
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The key must be a scalar or an anonymous/user type composed of nullable scalar members: in rows which aren't
    ///         grouped by a member, that member is <see langword="null" />. Use <see cref="Grouping{T}" /> to distinguish these
    ///         rows from groups whose key value is <see langword="null" />.
    ///     </para>
    ///     <para>
    ///         Only aggregate operators can be applied to the groupings (e.g. <c>g.Count()</c>, <c>g.Sum(e => e.Price)</c>).
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="key">The grouping key.</param>
    /// <returns>The grouping key.</returns>
    public static TKey Cube<TKey>(this DbFunctions _, TKey key)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Cube)));

    /// <summary>
    ///     When used as the key selector of a LINQ <c>GroupBy</c>, groups by the given key using the SQL <c>GROUPING SETS</c>
    ///     construct: rows are returned for each of the given sets of the key's members.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Each set lists members of <paramref name="key" />, referenced by the same expression used in the key; an empty set
    ///         (<c>new object[0]</c>) groups all rows together.
    ///     </para>
    ///     <para>
    ///         The key must be a scalar or an anonymous/user type composed of nullable scalar members: in rows which aren't
    ///         grouped by a member, that member is <see langword="null" />. Use <see cref="Grouping{T}" /> to distinguish these
    ///         rows from groups whose key value is <see langword="null" />.
    ///     </para>
    ///     <para>
    ///         Only aggregate operators can be applied to the groupings (e.g. <c>g.Count()</c>, <c>g.Sum(e => e.Price)</c>).
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code>
    /// // GROUP BY GROUPING SETS ((Category, Product), (Category), ())
    /// context.Sales
    ///     .GroupBy(s => EF.Functions.GroupingSets(
    ///         new { s.Category, s.Product },
    ///         new object[] { s.Category, s.Product },
    ///         new object[] { s.Category },
    ///         new object[0]))
    ///     .Select(g => new { g.Key.Category, g.Key.Product, Count = g.Count() });
    ///     </code>
    /// </example>
    /// <typeparam name="TKey">The type of the grouping key.</typeparam>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="key">The grouping key.</param>
    /// <param name="sets">The grouping sets, each one composed of members of <paramref name="key" />.</param>
    /// <returns>The grouping key.</returns>
    public static TKey GroupingSets<TKey>(this DbFunctions _, TKey key, params object?[][] sets)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(GroupingSets)));

    /// <summary>
    ///     Returns <c>1</c> if the given grouping key member isn't part of the grouping of the current row, and <c>0</c> if it is.
    ///     Corresponds to the SQL <c>GROUPING</c> function.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Used in projections over groupings created with <see cref="Rollup{TKey}" />, <see cref="Cube{TKey}" /> or
    ///         <see cref="GroupingSets{TKey}" />, to distinguish super-aggregate rows (where the member is <see langword="null" />
    ///         because it was rolled up) from groups whose key value is actually <see langword="null" />.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="T">The type of the grouping key member.</typeparam>
    /// <param name="_">The <see cref="DbFunctions" /> instance.</param>
    /// <param name="keyMember">A member of the grouping key, e.g. <c>g.Key.Category</c>.</param>
    /// <returns><c>1</c> if <paramref name="keyMember" /> isn't part of the current row's grouping; <c>0</c> otherwise.</returns>
    public static int Grouping<T>(this DbFunctions _, T keyMember)
        => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Grouping)));
}
