// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents a CROSS JOIN in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class CrossJoinExpression : JoinExpressionBase
{
    /// <summary>
    ///     Creates a new instance of the <see cref="CrossJoinExpression" /> class.
    /// </summary>
    /// <param name="table">A table source to CROSS JOIN with.</param>
    public CrossJoinExpression(TableExpressionBase table)
        : this(table, annotations: null)
    {
    }

    private CrossJoinExpression(TableExpressionBase table, IEnumerable<IAnnotation>? annotations)
        : base(table, annotations)
    {
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((TableExpressionBase)visitor.Visit(Table));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="table">The <see cref="JoinExpressionBase.Table" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual CrossJoinExpression Update(TableExpressionBase table)
        => table != Table
            ? new CrossJoinExpression(table, GetAnnotations())
            : this;

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("CROSS JOIN ");
        expressionPrinter.Visit(Table);
        PrintAnnotations(expressionPrinter);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is CrossJoinExpression crossJoinExpression
                && Equals(crossJoinExpression));

    private bool Equals(CrossJoinExpression crossJoinExpression)
        => base.Equals(crossJoinExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}
