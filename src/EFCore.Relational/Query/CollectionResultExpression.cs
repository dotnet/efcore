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
public class CollectionResultExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="CollectionResultExpression" /> class.
    /// </summary>
    /// <param name="projectionBindingExpression">An expression representing how to get the subquery from SelectExpression to get the elements.</param>
    /// <param name="navigation">A navigation associated with this collection, if any.</param>
    /// <param name="elementType">The clr type of individual elements in the collection.</param>
    public CollectionResultExpression(
        ProjectionBindingExpression projectionBindingExpression,
        INavigationBase? navigation,
        Type elementType)
    {
        ProjectionBindingExpression = projectionBindingExpression;
        Navigation = navigation;
        ElementType = elementType;
    }

    /// <summary>
    ///     The expression to get the subquery for this collection.
    /// </summary>
    public virtual ProjectionBindingExpression ProjectionBindingExpression { get; }

    /// <summary>
    ///     The navigation if associated with the collection.
    /// </summary>
    public virtual INavigationBase? Navigation { get; }

    /// <summary>
    ///     The clr type of elements of the collection.
    /// </summary>
    public virtual Type ElementType { get; }

    /// <inheritdoc />
    public override Type Type
        => ProjectionBindingExpression.Type;

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var projectionBindingExpression = (ProjectionBindingExpression)visitor.Visit(ProjectionBindingExpression);

        return Update(projectionBindingExpression);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="projectionBindingExpression">The <see cref="ProjectionBindingExpression" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual CollectionResultExpression Update(ProjectionBindingExpression projectionBindingExpression)
        => projectionBindingExpression != ProjectionBindingExpression
            ? new CollectionResultExpression(projectionBindingExpression, Navigation, ElementType)
            : this;

    /// <inheritdoc />
    public virtual void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("CollectionResultExpression:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("ProjectionBindingExpression:");
            expressionPrinter.Visit(ProjectionBindingExpression);
            expressionPrinter.AppendLine();
            if (Navigation != null)
            {
                expressionPrinter.Append("Navigation:").AppendLine(Navigation.ToString()!);
            }

            expressionPrinter.Append("ElementType:").AppendLine(ElementType.ShortDisplayName());
        }
    }
}
