// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Type extension methods for <see cref="TableExpressionBase" /> and related types.
/// </summary>
public static class TableExpressionExtensions
{
    /// <summary>
    ///     If the given <paramref name="table" /> is a <see cref="JoinExpressionBase" />, returns the table it joins to. Otherwise, returns
    ///     <paramref name="table" />.
    /// </summary>
    public static TableExpressionBase UnwrapJoin(this TableExpressionBase table)
        => table is JoinExpressionBase join ? join.Table : table;
}
