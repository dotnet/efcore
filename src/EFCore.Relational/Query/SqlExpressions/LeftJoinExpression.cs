// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a LEFT JOIN in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class LeftJoinExpression : PredicateJoinExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="LeftJoinExpression" /> class.
    /// </summary>
    /// <param name="table">A table source to LEFT JOIN with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    public LeftJoinExpression(TableExpressionBase table, SqlExpression joinPredicate)
        : this(table, joinPredicate, annotations: null)
    {
    }

    private LeftJoinExpression(
        TableExpressionBase table,
        SqlExpression joinPredicate,
        IEnumerable<IAnnotation>? annotations)
        : base(table, joinPredicate, annotations)
    {
    }

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
    public virtual LeftJoinExpression Update(TableExpressionBase table, SqlExpression joinPredicate)
        => table != Table || joinPredicate != JoinPredicate
            ? new LeftJoinExpression(table, joinPredicate, GetAnnotations())
            : this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("LEFT JOIN ");
        expressionPrinter.Visit(Table);
        expressionPrinter.Append(" ON ");
        expressionPrinter.Visit(JoinPredicate);
        PrintAnnotations(expressionPrinter);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is LeftJoinExpression leftJoinExpression
                && Equals(leftJoinExpression));

    private bool Equals(LeftJoinExpression leftJoinExpression)
        => base.Equals(leftJoinExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}
