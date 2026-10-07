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
        : this(keySelector, elementSelector, groupingEnumerable, resultSelector: null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="RelationalGroupByShaperExpression" /> class.
    /// </summary>
    /// <param name="keySelector">An expression representing key selector for the grouping result.</param>
    /// <param name="elementSelector">An expression representing element selector for the grouping result.</param>
    /// <param name="groupingEnumerable">An expression representing subquery for enumerable over the grouping result.</param>
    /// <param name="resultSelector">
    ///     A client-side projection applied to each materialized grouping, or <see langword="null" /> if the groupings themselves are
    ///     the result of the query.
    /// </param>
    public RelationalGroupByShaperExpression(
        Expression keySelector,
        Expression elementSelector,
        ShapedQueryExpression groupingEnumerable,
        LambdaExpression? resultSelector)
        : base(keySelector, groupingEnumerable)
    {
        ElementSelector = elementSelector;
        ResultSelector = resultSelector;
    }

    /// <summary>
    ///     The expression representing the element selector for this grouping result.
    /// </summary>
    public virtual Expression ElementSelector { get; }

    /// <summary>
    ///     <para>
    ///         A projection applied on the client to each grouping produced by this expression, or <see langword="null" /> when the
    ///         groupings are themselves the result of the query.
    ///     </para>
    ///     <para>
    ///         This is used when the query projects out of the grouping without aggregating it (e.g. <c>g => g.Select(e => e.Id)</c>);
    ///         such a projection needs no aggregation on the server, so the elements are streamed in key order and the projection is
    ///         applied to each grouping once it has been materialized.
    ///     </para>
    /// </summary>
    public virtual LambdaExpression? ResultSelector { get; }

    /// <inheritdoc />
    public override Type Type
        => ResultSelector?.ReturnType ?? base.Type;

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
        if (ResultSelector != null)
        {
            expressionPrinter.AppendLine(", ");
            expressionPrinter.Append("ResultSelector: ");
            expressionPrinter.Visit(ResultSelector);
        }

        expressionPrinter.AppendLine();
    }
}
