// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CollectionResultExpression(
    Expression queryExpression,
    IComplexProperty complexProperty)
    : Expression, IPrintableExpression
{
    /// <summary>
    ///     The query expression to get the collection.
    /// </summary>
    public virtual Expression QueryExpression { get; } = queryExpression;

    /// <summary>
    ///     The property associated with the collection. In cosmos, this can only be a complex property
    /// </summary>
    public virtual IComplexProperty ComplexProperty { get; } = complexProperty;

    /// <inheritdoc />
    public override Type Type
        => QueryExpression.Type;

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => Update(visitor.Visit(QueryExpression));

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="queryExpression">The <see cref="QueryExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual CollectionResultExpression Update(Expression queryExpression)
        => queryExpression == QueryExpression
            ? this
            : new CollectionResultExpression(queryExpression, ComplexProperty);

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("CollectionResultExpression:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("QueryExpression:");
            expressionPrinter.Visit(QueryExpression);
            expressionPrinter.AppendLine();

            expressionPrinter.Append("Complex Property:").AppendLine(ComplexProperty.ToString()!);
        }
    }
}
