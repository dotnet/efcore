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
public class ColumnValueSetter
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ColumnValueSetter" /> class.
    /// </summary>
    /// <param name="column">A column to be updated.</param>
    /// <param name="value">A value to be assigned to the column.</param>
    public ColumnValueSetter(ColumnExpression column, SqlExpression value)
    {
        Column = column;
        Value = value;
    }

    /// <summary>
    ///     The column to update value of.
    /// </summary>
    public virtual ColumnExpression Column { get; }

    /// <summary>
    ///     The value to be assigned to the column.
    /// </summary>
    public virtual SqlExpression Value { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ColumnValueSetter columnValueSetter
                && Equals(columnValueSetter));

    private bool Equals(ColumnValueSetter columnValueSetter)
        => Column == columnValueSetter.Column
        && Value == columnValueSetter.Value;

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Column, Value);
}

