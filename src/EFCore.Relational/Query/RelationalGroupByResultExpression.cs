// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of a grouping for relational provider in
///         <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalGroupByResultExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalGroupByResultExpression" /> class.
    /// </summary>
    /// <param name="keyIdentifier">An identifier for the parent element.</param>
    /// <param name="keyIdentifierValueComparers">A list of value comparers to compare parent identifier.</param>
    /// <param name="keyShaper">An expression used to create individual elements of the collection.</param>
    /// <param name="elementShaper">An expression used to create individual elements of the collection.</param>
    public RelationalGroupByResultExpression(
        Expression keyIdentifier,
        IReadOnlyList<ValueComparer> keyIdentifierValueComparers,
        Expression keyShaper,
        Expression elementShaper)
    {
        KeyIdentifier = keyIdentifier;
        KeyIdentifierValueComparers = keyIdentifierValueComparers;
        KeyShaper = keyShaper;
        ElementShaper = elementShaper;
        Type = typeof(IGrouping<,>).MakeGenericType(keyShaper.Type, elementShaper.Type);
    }

    /// <summary>
    ///     The identifier for the grouping key.
    /// </summary>
    public virtual Expression KeyIdentifier { get; }

    /// <summary>
    ///     The list of value comparers to compare key identifier.
    /// </summary>
    public virtual IReadOnlyList<ValueComparer> KeyIdentifierValueComparers { get; }

    /// <summary>
    ///     The expression to create the grouping key.
    /// </summary>
    public virtual Expression KeyShaper { get; }

    /// <summary>
    ///     The expression to create elements in the group.
    /// </summary>
    public virtual Expression ElementShaper { get; }

    /// <inheritdoc />
    public override Type Type { get; }

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var keyIdentifier = visitor.Visit(KeyIdentifier);
        var keyShaper = visitor.Visit(KeyShaper);
        var elementShaper = visitor.Visit(ElementShaper);

        return Update(keyIdentifier, keyShaper, elementShaper);
    }

    /// <summary>
    ///     Creates a new expression that is like this one, but using the supplied children. If all of the children are the same, it will
    ///     return this expression.
    /// </summary>
    /// <param name="keyIdentifier">The <see cref="KeyIdentifier" /> property of the result.</param>
    /// <param name="keyShaper">The <see cref="KeyShaper" /> property of the result.</param>
    /// <param name="elementShaper">The <see cref="ElementShaper" /> property of the result.</param>
    /// <returns>This expression if no children changed, or an expression with the updated children.</returns>
    public virtual RelationalGroupByResultExpression Update(
        Expression keyIdentifier,
        Expression keyShaper,
        Expression elementShaper)
        => keyIdentifier != KeyIdentifier
            || keyShaper != KeyShaper
            || elementShaper != ElementShaper
                ? new RelationalGroupByResultExpression(keyIdentifier, KeyIdentifierValueComparers, keyShaper, elementShaper)
                : this;

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine("RelationalGroupByResultExpression:");
        using (expressionPrinter.Indent())
        {
            expressionPrinter.Append("KeyIdentifier:");
            expressionPrinter.Visit(KeyIdentifier);
            expressionPrinter.AppendLine(",");
            expressionPrinter.Append("KeyShaper:");
            expressionPrinter.Visit(KeyShaper);
            expressionPrinter.AppendLine(",");
            expressionPrinter.Append("ElementShaper:");
            expressionPrinter.Visit(ElementShaper);
            expressionPrinter.AppendLine();
        }
    }
}
