// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     The kind of multiple grouping represented by a <see cref="GroupingSetsExpression" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public enum GroupingSetsKind
{
    /// <summary>
    ///     <c>ROLLUP (a, b)</c>: groups by <c>(a, b)</c>, <c>(a)</c> and <c>()</c>.
    /// </summary>
    Rollup,

    /// <summary>
    ///     <c>CUBE (a, b)</c>: groups by all combinations of the terms: <c>(a, b)</c>, <c>(a)</c>, <c>(b)</c> and <c>()</c>.
    /// </summary>
    Cube,

    /// <summary>
    ///     <c>GROUPING SETS ((a, b), (a), ())</c>: groups by each of the explicitly specified sets.
    /// </summary>
    GroupingSets
}
