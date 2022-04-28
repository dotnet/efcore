// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         An expression that represents creation of a grouping element in <see cref="ShapedQueryExpression.ShaperExpression" />.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
/// </remarks>
public class GroupByShaperExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="GroupByShaperExpression" /> class.
    /// </summary>
    /// <param name="keySelector">An expression representing key selector for the grouping result.</param>
    /// <param name="elementSelector">An expression representing element selector for the grouping result.</param>
    /// <param name="groupingEnumerable">An expression representing subquery for enumerable over the grouping result.</param>
    public GroupByShaperExpression(
        Expression keySelector,
        Expression elementSelector,
        ShapedQueryExpression groupingEnumerable)
    {
        KeySelector = keySelector;
        ElementSelector = elementSelector;
        GroupingEnumerable = groupingEnumerable;
    }

    /// <summary>
    ///     The expression representing the key selector for this grouping result.
    /// </summary>
    public virtual Expression KeySelector { get; }

    /// <summary>
    ///     The expression representing the element selector for this grouping result.
    /// </summary>
    public virtual Expression ElementSelector { get; }

    /// <summary>
    ///     The expression representing the subquery for the enumerable over this grouping result.
    /// </summary>
    public virtual ShapedQueryExpression GroupingEnumerable { get; }

    /// <inheritdoc />
    public override Type Type
        => typeof(IGrouping<,>).MakeGenericType(KeySelector.Type, GroupingEnumerable.ShaperExpression.Type);

    /// <inheritdoc />
    public sealed override ExpressionType NodeType
        => ExpressionType.Extension;

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => throw new InvalidOperationException(
            CoreStrings.VisitIsNotAllowed($"{nameof(GroupByShaperExpression)}.{nameof(VisitChildren)}"));

    /// <inheritdoc />
    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.AppendLine($"{nameof(GroupByShaperExpression)}:");
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
