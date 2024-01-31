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
    /// <param name="prunable">Whether this join expression may be pruned if nothing references a column on it.</param>
    /// <param name="annotations">A collection of annotations associated with this expression.</param>
    protected PredicateJoinExpressionBase(
        TableExpressionBase table,
        SqlExpression joinPredicate,
        bool prunable,
        IReadOnlyDictionary<string, IAnnotation>? annotations = null)
        : base(table, prunable, annotations)
    {
        JoinPredicate = joinPredicate;
    }

    /// <summary>
    ///     The predicate used in join.
    /// </summary>
    public virtual SqlExpression JoinPredicate { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var table = (TableExpressionBase)visitor.Visit(Table);
        var joinPredicate = (SqlExpression)visitor.Visit(JoinPredicate);

        return Update(table, joinPredicate);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="table">The <see cref="JoinExpressionBase.Table" /> property of the result.</param>
    /// <param name="joinPredicate">The <see cref="PredicateJoinExpressionBase.JoinPredicate" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public abstract PredicateJoinExpressionBase Update(TableExpressionBase table, SqlExpression joinPredicate);

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
