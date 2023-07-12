// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of a grouping element in <see cref="ShapedQueryExpression.ShaperExpression" />
///         for relational providers.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
public class RelationalGroupByShaperExpression : GroupByShaperExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalGroupByShaperExpression" /> class.
    /// </summary>
    /// <param name="keySelector">An expression representing key selector for the grouping result.</param>
    /// <param name="elementSelector">An expression representing element selector for the grouping result.</param>
    /// <param name="groupingEnumerable">An expression representing subquery for enumerable over the grouping result.</param>
    public RelationalGroupByShaperExpression(
        Expression keySelector,
        Expression elementSelector,
        ShapedQueryExpression groupingEnumerable)
        : base(keySelector, groupingEnumerable)
    {
        ElementSelector = elementSelector;
    }

    /// <summary>
    ///     The expression representing the element selector for this grouping result.
    /// </summary>
    public virtual Expression ElementSelector { get; }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => throw new InvalidOperationException(
            CoreStrings.VisitIsNotAllowed($"{nameof(RelationalGroupByShaperExpression)}.{nameof(VisitChildren)}"));

    /// <inheritdoc />
    public override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine($"{nameof(RelationalGroupByShaperExpression)}:");
        expressionPrinter.Append("KeySelector: ");
        expressionPrinter.Visit(KeySelector);
        expressionPrinter.AppendLine(", ");
        expressionPrinter.Append("ElementSelector: ");
        expressionPrinter.Visit(ElementSelector);
        expressionPrinter.AppendLine(", ");
        expressionPrinter.Append("GroupingEnumerable:");
        expressionPrinter.Visit(GroupingEnumerable);
        expressionPrinter.AppendLine();
    }
}
