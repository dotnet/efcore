// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of a collection in <see cref="ShapedQueryExpression.ShaperExpression" /> for relational
///         providers.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <param name="queryExpression">Represents the server-side query expression for the collection.</param>
/// <param name="relationship">A navigation associated with this collection, if any.</param>
/// <param name="elementType">The clr type of individual elements in the collection.</param>
public class CollectionResultExpression(
    Expression queryExpression,
    IStructuralProperty? relationship,
    Type elementType)
    : Expression, IPrintableExpression
{
    /// <summary>
    ///     The expression to get the subquery for this collection.
    /// </summary>
    public virtual Expression QueryExpression { get; } = queryExpression;

    /// <summary>
    ///     The relationship associated with the collection, if any.
    /// </summary>
    public virtual IStructuralProperty? Relationship { get; } = relationship;

    /// <summary>
    ///     The clr type of elements of the collection.
    /// </summary>
    public virtual Type ElementType { get; } = elementType;

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
    /// <param name="queryExpression">The <see cref="ProjectionBindingExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual CollectionResultExpression Update(Expression queryExpression)
        => queryExpression == QueryExpression
            ? this
            : new CollectionResultExpression(queryExpression, Relationship, ElementType);

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("CollectionResultExpression:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("QueryExpression:");
            expressionPrinter.Visit(QueryExpression);
            expressionPrinter.AppendLine();

            if (Relationship is not null)
            {
                expressionPrinter.Append("Relationship:").AppendLine(Relationship.ToString()!);
            }

            expressionPrinter.Append("ElementType:").AppendLine(ElementType.ShortDisplayName());
        }
    }

    /// <summary>
    ///     The expression to get the subquery for this collection.
    /// </summary>
    [Obsolete("Use QueryExpression instead.", error: true)]
    public virtual ProjectionBindingExpression ProjectionBindingExpression { get; } = null!;

    /// <summary>
    ///     The navigation if associated with the collection.
    /// </summary>
    [Obsolete("Use Relationship instead.", error: true)]
    public virtual INavigationBase? Navigation { get; }
}
