// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of a collection for relational provider in
///         <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalCollectionShaperExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalCollectionShaperExpression" /> class.
    /// </summary>
    /// <param name="parentIdentifier">An identifier for the parent element.</param>
    /// <param name="outerIdentifier">An identifier for the outer element.</param>
    /// <param name="selfIdentifier">An identifier for the element in the collection.</param>
    /// <param name="parentIdentifierValueComparers">A list of value comparers to compare parent identifier.</param>
    /// <param name="outerIdentifierValueComparers">A list of value comparers to compare outer identifier.</param>
    /// <param name="selfIdentifierValueComparers">A list of value comparers to compare self identifier.</param>
    /// <param name="innerShaper">An expression used to create individual elements of the collection.</param>
    /// <param name="navigation">A navigation associated with this collection, if any.</param>
    /// <param name="elementType">The clr type of individual elements in the collection.</param>
    public RelationalCollectionShaperExpression(
        Expression parentIdentifier,
        Expression outerIdentifier,
        Expression selfIdentifier,
        IReadOnlyList<ValueComparer> parentIdentifierValueComparers,
        IReadOnlyList<ValueComparer> outerIdentifierValueComparers,
        IReadOnlyList<ValueComparer> selfIdentifierValueComparers,
        Expression innerShaper,
        INavigationBase? navigation,
        Type elementType)
    {
        ParentIdentifier = parentIdentifier;
        OuterIdentifier = outerIdentifier;
        SelfIdentifier = selfIdentifier;
        ParentIdentifierValueComparers = parentIdentifierValueComparers;
        OuterIdentifierValueComparers = outerIdentifierValueComparers;
        SelfIdentifierValueComparers = selfIdentifierValueComparers;
        InnerShaper = innerShaper;
        Navigation = navigation;
        ElementType = elementType;
    }

    /// <summary>
    ///     The identifier for the parent element.
    /// </summary>
    public virtual Expression ParentIdentifier { get; }

    /// <summary>
    ///     The identifier for the outer element.
    /// </summary>
    public virtual Expression OuterIdentifier { get; }

    /// <summary>
    ///     The identifier for the element in the collection.
    /// </summary>
    public virtual Expression SelfIdentifier { get; }

    /// <summary>
    ///     The list of value comparers to compare parent identifier.
    /// </summary>
    public virtual IReadOnlyList<ValueComparer> ParentIdentifierValueComparers { get; }

    /// <summary>
    ///     The list of value comparers to compare outer identifier.
    /// </summary>
    public virtual IReadOnlyList<ValueComparer> OuterIdentifierValueComparers { get; }

    /// <summary>
    ///     The list of value comparers to compare self identifier.
    /// </summary>
    public virtual IReadOnlyList<ValueComparer> SelfIdentifierValueComparers { get; }

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
        var outerIdentifier = visitor.Visit(OuterIdentifier);
        var selfIdentifier = visitor.Visit(SelfIdentifier);
        var innerShaper = visitor.Visit(InnerShaper);

        return Update(parentIdentifier, outerIdentifier, selfIdentifier, innerShaper);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="parentIdentifier">The <see cref="ParentIdentifier" /> property of the result.</param>
    /// <param name="outerIdentifier">The <see cref="OuterIdentifier" /> property of the result.</param>
    /// <param name="selfIdentifier">The <see cref="SelfIdentifier" /> property of the result.</param>
    /// <param name="innerShaper">The <see cref="InnerShaper" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual RelationalCollectionShaperExpression Update(
        Expression parentIdentifier,
        Expression outerIdentifier,
        Expression selfIdentifier,
        Expression innerShaper)
        => parentIdentifier != ParentIdentifier
            || outerIdentifier != OuterIdentifier
            || selfIdentifier != SelfIdentifier
            || innerShaper != InnerShaper
                ? new RelationalCollectionShaperExpression(
                    parentIdentifier, outerIdentifier, selfIdentifier,
                    ParentIdentifierValueComparers, OuterIdentifierValueComparers, SelfIdentifierValueComparers,
                    innerShaper, Navigation, ElementType)
                : this;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("RelationalCollectionShaper:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("ParentIdentifier:");
            expressionPrinter.Visit(ParentIdentifier);
            expressionPrinter.AppendLine();
            expressionPrinter.Append("OuterIdentifier:");
            expressionPrinter.Visit(OuterIdentifier);
            expressionPrinter.AppendLine();
            expressionPrinter.Append("SelfIdentifier:");
            expressionPrinter.Visit(SelfIdentifier);
            expressionPrinter.AppendLine();
            expressionPrinter.Append("InnerShaper:");
            expressionPrinter.Visit(InnerShaper);
            expressionPrinter.AppendLine();
            expressionPrinter.AppendLine($"Navigation: {Navigation?.Name}");
        }
    }
}
