// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of a collection during split query for relational provider in
///         <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalSplitCollectionShaperExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalCollectionShaperExpression" /> class.
    /// </summary>
    /// <param name="parentIdentifier">An identifier for the parent element.</param>
    /// <param name="childIdentifier">An identifier for the child element.</param>
    /// <param name="identifierValueComparers">A list of value comparers to compare identifiers.</param>
    /// <param name="selectExpression">A SQL query to get values for this collection from database.</param>
    /// <param name="innerShaper">An expression used to create individual elements of the collection.</param>
    /// <param name="navigation">A navigation associated with this collection, if any.</param>
    /// <param name="elementType">The clr type of individual elements in the collection.</param>
    public RelationalSplitCollectionShaperExpression(
        Expression parentIdentifier,
        Expression childIdentifier,
        IReadOnlyList<ValueComparer> identifierValueComparers,
        SelectExpression selectExpression,
        Expression innerShaper,
        INavigationBase? navigation,
        Type elementType)
    {
        ParentIdentifier = parentIdentifier;
        ChildIdentifier = childIdentifier;
        IdentifierValueComparers = identifierValueComparers;
        SelectExpression = selectExpression;
        InnerShaper = innerShaper;
        Navigation = navigation;
        ElementType = elementType;
    }

    /// <summary>
    ///     The identifier for the parent element.
    /// </summary>
    public virtual Expression ParentIdentifier { get; }

    /// <summary>
    ///     The identifier for the child element.
    /// </summary>
    public virtual Expression ChildIdentifier { get; }

    /// <summary>
    ///     The list of value comparers to compare identifiers.
    /// </summary>
    public virtual IReadOnlyList<ValueComparer> IdentifierValueComparers { get; }

    /// <summary>
    ///     The SQL query to get values for this collection from database.
    /// </summary>
    public virtual SelectExpression SelectExpression { get; }

    /// <summary>
    ///     The expression to create inner elements.
    /// </summary>
    public virtual Expression InnerShaper { get; }

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
        => Navigation?.ClrType ?? typeof(List<>).MakeGenericType(ElementType);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var parentIdentifier = visitor.Visit(ParentIdentifier);
        var childIdentifier = visitor.Visit(ChildIdentifier);
        var selectExpression = (SelectExpression)visitor.Visit(SelectExpression);
        var innerShaper = visitor.Visit(InnerShaper);

        return Update(parentIdentifier, childIdentifier, selectExpression, innerShaper);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="parentIdentifier">The <see cref="ParentIdentifier" /> property of the result.</param>
    /// <param name="childIdentifier">The <see cref="ChildIdentifier" /> property of the result.</param>
    /// <param name="selectExpression">The <see cref="SelectExpression" /> property of the result.</param>
    /// <param name="innerShaper">The <see cref="InnerShaper" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual RelationalSplitCollectionShaperExpression Update(
        Expression parentIdentifier,
        Expression childIdentifier,
        SelectExpression selectExpression,
        Expression innerShaper)
        => parentIdentifier != ParentIdentifier
            || childIdentifier != ChildIdentifier
            || selectExpression != SelectExpression
            || innerShaper != InnerShaper
                ? new RelationalSplitCollectionShaperExpression(
                    parentIdentifier, childIdentifier, IdentifierValueComparers, selectExpression, innerShaper, Navigation, ElementType)
                : this;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("RelationalSplitCollectionShaperExpression:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("ParentIdentifier:");
            expressionPrinter.Visit(ParentIdentifier);
            expressionPrinter.AppendLine();
            expressionPrinter.Append("ChildIdentifier:");
            expressionPrinter.Visit(ChildIdentifier);
            expressionPrinter.AppendLine();
            expressionPrinter.Append("SelectExpression:");
            expressionPrinter.Visit(SelectExpression);
            expressionPrinter.AppendLine();
            expressionPrinter.Append("InnerShaper:");
            expressionPrinter.Visit(InnerShaper);
            expressionPrinter.AppendLine();
            expressionPrinter.AppendLine($"Navigation: {Navigation?.Name}");
        }
    }
}
