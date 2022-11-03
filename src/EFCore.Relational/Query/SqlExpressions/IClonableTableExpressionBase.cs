// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An interface that represents a table source in a SQL tree which can be cloned.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public interface IClonableTableExpressionBase
{
    /// <summary>
    ///     Creates a new object that is a copy of the current instance.
    /// </summary>
    /// <returns>A new object that is a copy of this instance.</returns>
    TableExpressionBase Clone();
}
