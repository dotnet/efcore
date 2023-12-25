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
    /// <param name="prunable">Whether this join expression may be pruned if nothing references a column on it.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    protected JoinExpressionBase(TableExpressionBase table, bool prunable, IEnumerable<IAnnotation>? annotations = null)
        : base(alias: null, annotations)
    {
        Table = table;
        IsPrunable = prunable;
    }

    /// <summary>
    ///     Gets the underlying table source to join with.
    /// </summary>
    public virtual TableExpressionBase Table { get; }

    /// <summary>
    ///     Whether this join expression may be pruned if nothing references a column on it. This isn't the case, for example, when an
    ///     INNER JOIN is used to filter out rows.
    /// </summary>
    public virtual bool IsPrunable { get; }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="table">The <see cref="JoinExpressionBase.Table" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public abstract JoinExpressionBase Update(TableExpressionBase table);

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
