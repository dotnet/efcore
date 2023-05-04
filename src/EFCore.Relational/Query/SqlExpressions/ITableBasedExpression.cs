// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An interface that gives access to an optional <see cref="ITableBase" /> associated with given table source.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public interface ITableBasedExpression
{
    /// <summary>
    ///     The <see cref="ITableBase" /> associated with given table source, if any.
    /// </summary>
    ITableBase? Table { get; }
}
