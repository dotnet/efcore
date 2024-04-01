// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions;

/// <summary>
///     <para>
///         An expression that represents an ordering in a SQL tree.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public class OrderingExpression : Expression, IRelationalQuotableExpression, IPrintableExpression
{
    private static ConstructorInfo? _quotingConstructor;

    /// <summary>
    ///     Creates a new instance of the <see cref="OrderingExpression" /> class.
    /// </summary>
    /// <param name="expression">An expression used for ordering.</param>
    /// <param name="ascending">A value indicating if the ordering is ascending.</param>
    public OrderingExpression(SqlExpression expression, bool ascending)
    {
        Expression = expression;
        IsAscending = ascending;
    }

    /// <summary>
    ///     The expression used for ordering.
    /// </summary>
    public virtual SqlExpression Expression { get; }

    /// <summary>
    ///     The value indicating if the ordering is ascending.
    /// </summary>
    public virtual bool IsAscending { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    public override Type Type
        => Expression.Type;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update((SqlExpression)visitor.Visit(Expression));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="expression">The <see cref="Expression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual OrderingExpression Update(SqlExpression expression)
        => expression != Expression
            ? new OrderingExpression(expression, IsAscending)
            : this;


    /// <inheritdoc />
    public Expression Quote()
        => New(
            _quotingConstructor ??= typeof(OrderingExpression).GetConstructor([typeof(SqlExpression), typeof(bool)])!,
            Expression.Quote(),
            Constant(IsAscending));

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Visit(Expression);

        expressionPrinter.Append(IsAscending ? " ASC" : " DESC");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is OrderingExpression orderingExpression
                && Equals(orderingExpression));

    private bool Equals(OrderingExpression orderingExpression)
        => Expression.Equals(orderingExpression.Expression)
            && IsAscending == orderingExpression.IsAscending;

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Expression, IsAscending);
}
