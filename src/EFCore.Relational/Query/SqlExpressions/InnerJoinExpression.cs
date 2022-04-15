// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an INNER JOIN in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class InnerJoinExpression : PredicateJoinExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="InnerJoinExpression" /> class.
    /// </summary>
    /// <param name="table">A table source to INNER JOIN with.</param>
    /// <param name="joinPredicate">A predicate to use for the join.</param>
    public InnerJoinExpression(TableExpressionBase table, SqlExpression joinPredicate)
        : this(table, joinPredicate, annotations: null)
    {
    }

    private InnerJoinExpression(
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
    public virtual InnerJoinExpression Update(TableExpressionBase table, SqlExpression joinPredicate)
        => table != Table || joinPredicate != JoinPredicate
            ? new InnerJoinExpression(table, joinPredicate, GetAnnotations())
            : this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("INNER JOIN ");
        expressionPrinter.Visit(Table);
        expressionPrinter.Append(" ON ");
        expressionPrinter.Visit(JoinPredicate);
        PrintAnnotations(expressionPrinter);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is InnerJoinExpression innerJoinExpression
                && Equals(innerJoinExpression));

    private bool Equals(InnerJoinExpression innerJoinExpression)
        => base.Equals(innerJoinExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}
