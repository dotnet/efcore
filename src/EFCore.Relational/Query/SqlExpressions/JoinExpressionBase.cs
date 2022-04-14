// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a JOIN in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public abstract class JoinExpressionBase : TableExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="JoinExpressionBase" /> class.
    /// </summary>
    /// <param name="table">A table source to join with.</param>
    protected JoinExpressionBase(TableExpressionBase table)
        : this(table, annotations: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="JoinExpressionBase" /> class.
    /// </summary>
    /// <param name="table">A table source to join with.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    protected JoinExpressionBase(TableExpressionBase table, IEnumerable<IAnnotation>? annotations)
        : base(alias: null, annotations)
    {
        Table = table;
    }

    /// <summary>
    ///     Gets the underlying table source to join with.
    /// </summary>
    public virtual TableExpressionBase Table { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is JoinExpressionBase joinExpressionBase
                && Equals(joinExpressionBase));

    private bool Equals(JoinExpressionBase joinExpressionBase)
        => base.Equals(joinExpressionBase)
            && Table.Equals(joinExpressionBase.Table);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Table);
}
