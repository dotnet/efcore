// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An object that represents a column = value construct in a SET clause of UPDATE command in SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <param name="Column">A column to be updated.</param>
/// <param name="Value">A value to be assigned to the column.</param>
[DebuggerDisplay("{DebuggerDisplay(),nq}")]
public readonly record struct ColumnValueSetter(ColumnExpression Column, SqlExpression Value)
{
    private string DebuggerDisplay()
        => $"{Column.Print()} = {Value.Print()}";
}
