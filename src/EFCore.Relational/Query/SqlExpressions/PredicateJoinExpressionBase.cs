// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a JOIN with a search condition in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public abstract class PredicateJoinExpressionBase : JoinExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="PredicateJoinExpressionBase" /> class.
    /// </summary>
    /// <param name="table">A table source to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    protected PredicateJoinExpressionBase(TableExpressionBase table, SqlExpression joinPredicate)
        : this(table, joinPredicate, annotations: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="PredicateJoinExpressionBase" /> class.
    /// </summary>
    /// <param name="table">A table source to join with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    protected PredicateJoinExpressionBase(
        TableExpressionBase table,
        SqlExpression joinPredicate,
        IEnumerable<IAnnotation>? annotations)
        : base(table, annotations)
    {
        JoinPredicate = joinPredicate;
    }

    /// <summary>
    ///     The predicate used in join.
    /// </summary>
    public virtual SqlExpression JoinPredicate { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is PredicateJoinExpressionBase predicateJoinExpressionBase
                && Equals(predicateJoinExpressionBase));

    private bool Equals(PredicateJoinExpressionBase predicateJoinExpressionBase)
        => base.Equals(predicateJoinExpressionBase)
            && JoinPredicate.Equals(predicateJoinExpressionBase.JoinPredicate);

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), JoinPredicate);
}
