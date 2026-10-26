// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

// An aggregate over a grouping that the SQL translator refused to fold into the query owning the grouping, because it couldn't tell
// that SQL would evaluate it there - typically because the query it was translated for isn't the one it ends up in, as with an element
// of an inline collection that Contains later lifts into an IN (#27130). Carries both the correlated subquery fallback and the folded
// aggregate, so that GroupingAggregateFoldingExpressionVisitor can pick the folded one once the final SQL tree shows it is safe.
// Never survives query translation postprocessing.
internal sealed class GroupingAggregateFallbackExpression : SqlExpression
{
    public GroupingAggregateFallbackExpression(
        SqlExpression fallback,
        SqlExpression foldedAggregate,
        IReadOnlyList<SqlExpression> groupingKeys)
        : base(fallback.Type, fallback.TypeMapping)
    {
        Fallback = fallback;
        FoldedAggregate = foldedAggregate;
        GroupingKeys = groupingKeys;
    }

    // The translation to use unless the folded aggregate is shown safe: a correlated subquery over the grouping element.
    public SqlExpression Fallback { get; }

    // The aggregate over the owning query's GROUP BY.
    public SqlExpression FoldedAggregate { get; }

    // The owning query's GROUP BY, which identifies it: visited along with the rest, so that it stays in step with the columns of
    // the folded aggregate when a query is cloned with new table aliases.
    public IReadOnlyList<SqlExpression> GroupingKeys { get; }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var fallback = (SqlExpression)visitor.Visit(Fallback);
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
            ? new GroupingAggregateFallbackExpression(fallback, foldedAggregate, groupingKeys ?? GroupingKeys)
            : this;
    }

    public override Expression Quote()
        => throw new UnreachableException(
            $"{nameof(GroupingAggregateFallbackExpression)} should have been removed by query translation postprocessing.");

    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append("GROUPING_AGGREGATE_FALLBACK(");
        expressionPrinter.Visit(Fallback);
        expressionPrinter.Append(", ");
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
            && Fallback.Equals(groupingAggregateFallbackExpression.Fallback)
            && FoldedAggregate.Equals(groupingAggregateFallbackExpression.FoldedAggregate)
            && GroupingKeys.SequenceEqual(groupingAggregateFallbackExpression.GroupingKeys);

    public override int GetHashCode()
        => HashCode.Combine(base.GetHashCode(), Fallback, FoldedAggregate);
}
