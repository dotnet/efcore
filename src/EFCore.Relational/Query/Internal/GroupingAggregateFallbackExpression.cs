// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// An aggregate over a grouping that the SQL translator refused to fold into the query owning the grouping, because it couldn't tell
// that SQL would evaluate it there - typically because the query it was translated for isn't the one it ends up in, as with an element
// of an inline collection that Contains later lifts into an IN (#27130). Carries the folded aggregate and, when there is one, the
// correlated subquery fallback, so that GroupingAggregateFoldingExpressionVisitor can pick the folded one once the final SQL tree shows
// it is safe. A provider aggregate such as STDEVP or STRING_AGG has no subquery fallback; if it can't be folded either, it fails to
// translate then. Never survives query translation postprocessing.
internal sealed class GroupingAggregateFallbackExpression : SqlExpression
{
    public GroupingAggregateFallbackExpression(
        SqlExpression? fallback,
        SqlExpression foldedAggregate,
        IReadOnlyList<SqlExpression> groupingKeys,
        Expression aggregateCall)
        : base(fallback?.Type ?? foldedAggregate.Type, fallback?.TypeMapping ?? foldedAggregate.TypeMapping)
    {
        Fallback = fallback;
        FoldedAggregate = foldedAggregate;
        GroupingKeys = groupingKeys;
        AggregateCall = aggregateCall;
    }

    // The translation to use unless the folded aggregate is shown safe: a correlated subquery over the grouping element, or null when
    // the aggregate can't be translated as one.
    public SqlExpression? Fallback { get; }

    // The aggregate over the owning query's GROUP BY.
    public SqlExpression FoldedAggregate { get; }

    // The owning query's GROUP BY, which identifies it: visited along with the rest, so that it stays in step with the columns of
    // the folded aggregate when a query is cloned with new table aliases.
    public IReadOnlyList<SqlExpression> GroupingKeys { get; }

    // The LINQ aggregate call, for the translation failure reported when there's no fallback and the aggregate can't be folded.
    public Expression AggregateCall { get; }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var fallback = (SqlExpression?)visitor.Visit(Fallback);
        var foldedAggregate = (SqlExpression)visitor.Visit(FoldedAggregate);

        var groupingKeys = (List<SqlExpression>?)null;
        for (var i = 0; i < GroupingKeys.Count; i++)
        {
            var groupingKey = (SqlExpression)visitor.Visit(GroupingKeys[i]);
            if (groupingKey != GroupingKeys[i] && groupingKeys is null)
            {
                groupingKeys = [.. GroupingKeys.Take(i)];
            }

            groupingKeys?.Add(groupingKey);
        }

        return fallback != Fallback || foldedAggregate != FoldedAggregate || groupingKeys is not null
            ? new GroupingAggregateFallbackExpression(fallback, foldedAggregate, groupingKeys ?? GroupingKeys, AggregateCall)
            : this;
    }

    public override Expression Quote()
        => throw new UnreachableException(
            $"{nameof(GroupingAggregateFallbackExpression)} should have been removed by query translation postprocessing.");

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("GROUPING_AGGREGATE_FALLBACK(");
        if (Fallback is not null)
        {
            expressionPrinter.Visit(Fallback);
            expressionPrinter.Append(", ");
        }

        expressionPrinter.Visit(FoldedAggregate);
        expressionPrinter.Append(")");
    }

    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is GroupingAggregateFallbackExpression groupingAggregateFallbackExpression
                && Equals(groupingAggregateFallbackExpression));

    private bool Equals(GroupingAggregateFallbackExpression groupingAggregateFallbackExpression)
        => base.Equals(groupingAggregateFallbackExpression)
            && Equals(Fallback, groupingAggregateFallbackExpression.Fallback)
            && FoldedAggregate.Equals(groupingAggregateFallbackExpression.FoldedAggregate)
            && GroupingKeys.SequenceEqual(groupingAggregateFallbackExpression.GroupingKeys);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Fallback, FoldedAggregate);
}
