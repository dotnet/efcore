// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that contains a non-query expression. The result of a non-query expression is typically the number of rows affected.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class NonQueryExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="NonQueryExpression" /> class with associated query expression and command source.
    /// </summary>
    /// <param name="expression">The expression to affect rows on the server.</param>
    /// <param name="commandSource">The command source to use for this non-query operation.</param>
    public NonQueryExpression(Expression expression, CommandSource commandSource)
    {
        Expression = expression;
        CommandSource = commandSource;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="NonQueryExpression" /> class with associated delete expression.
    /// </summary>
    /// <param name="deleteExpression">The delete expression to delete rows on the server.</param>
    public NonQueryExpression(DeleteExpression deleteExpression)
        : this(deleteExpression, CommandSource.ExecuteDelete)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="NonQueryExpression" /> class with associated update expression.
    /// </summary>
    /// <param name="updateExpression">The update expression to update rows on the server.</param>
    public NonQueryExpression(UpdateExpression updateExpression)
        : this(updateExpression, CommandSource.ExecuteUpdate)
    {
    }

    /// <summary>
    ///     An expression representing the non-query operation to be run against server.
    /// </summary>
    public virtual Expression Expression { get; }

    /// <summary>
    ///     The command source to use for this non-query operation.
    /// </summary>
    public virtual CommandSource CommandSource { get; }

    /// <inheritdoc />
    public override Type Type
        => typeof(int);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var expression = visitor.Visit(Expression);

        return Update(expression);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="expression">The <see cref="Expression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual NonQueryExpression Update(Expression expression)
        => expression != Expression
            ? new NonQueryExpression(expression, CommandSource)
            : this;

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append($"({nameof(NonQueryExpression)}: ");
        expressionPrinter.Visit(Expression);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is NonQueryExpression nonQueryExpression
                && Equals(nonQueryExpression));

    private bool Equals(NonQueryExpression nonQueryExpression)
        => Expression == nonQueryExpression.Expression;

    /// <inheritdoc />
    public override int GetHashCode()
        => Expression.GetHashCode();
}
