// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public partial class NavigationExpandingExpressionVisitor
{
    private NavigationExpansionExpression LiftSingleResultSubqueries(NavigationExpansionExpression source)
    {
        var selectorBody = source.PendingSelector;

        var collector = new SingleResultMemberAccessCollector();
        collector.Visit(selectorBody);

        foreach (var subquery in collector.Liftable)
        {
            var collection = BuildSingleResultCollection(subquery);
            var innerParameter = Expression.Parameter(collection.Type.GetSequenceType(), "e");
            var rewrittenBody = new ReplacingExpressionVisitor([subquery], [innerParameter]).Visit(selectorBody);

            // The collection already references the outer element via source.PendingSelector; this parameter is unused.
            source = ProcessSelectMany(
                source,
                Expression.Lambda(collection, Expression.Parameter(source.SourceElementType, "o")),
                Expression.Lambda(rewrittenBody, Expression.Parameter(source.SourceElementType, "o"), innerParameter));
            selectorBody = source.PendingSelector;
        }

        return source;
    }

    private static Expression BuildSingleResultCollection(MethodCallExpression subqueryMethod)
    {
        var method = subqueryMethod.Method.GetGenericMethodDefinition();
        var source = subqueryMethod.Arguments[0];
        var elementType = source.Type.GetSequenceType();

        Expression Where(Expression c)
            => Expression.Call(QueryableMethods.Where.MakeGenericMethod(elementType), c, subqueryMethod.Arguments[1]);
        Expression Skip(Expression c)
            => Expression.Call(QueryableMethods.Skip.MakeGenericMethod(elementType), c, subqueryMethod.Arguments[1]);
        Expression Reverse(Expression c)
            => Expression.Call(QueryableMethods.Reverse.MakeGenericMethod(elementType), c);

        var oneRow = method switch
        {
            _ when method == QueryableMethods.FirstWithPredicate || method == QueryableMethods.FirstOrDefaultWithPredicate
                || method == QueryableMethods.SingleWithPredicate || method == QueryableMethods.SingleOrDefaultWithPredicate
                => Where(source),
            _ when method == QueryableMethods.LastWithPredicate || method == QueryableMethods.LastOrDefaultWithPredicate
                => Reverse(Where(source)),
            _ when method == QueryableMethods.LastWithoutPredicate || method == QueryableMethods.LastOrDefaultWithoutPredicate
                => Reverse(source),
            _ when method == QueryableMethods.ElementAt || method == QueryableMethods.ElementAtOrDefault
                => Skip(source),
            _ => source
        };

        var firstRow = Expression.Call(QueryableMethods.Take.MakeGenericMethod(elementType), oneRow, Expression.Constant(1));

        return Expression.Call(QueryableMethods.DefaultIfEmptyWithoutArgument.MakeGenericMethod(elementType), firstRow);
    }

    private sealed class SingleResultMemberAccessCollector : ExpressionVisitor
    {
        private static readonly HashSet<MethodInfo> SingleResultMethods =
        [
            QueryableMethods.FirstWithPredicate, QueryableMethods.FirstWithoutPredicate,
            QueryableMethods.FirstOrDefaultWithPredicate, QueryableMethods.FirstOrDefaultWithoutPredicate,
            QueryableMethods.SingleWithPredicate, QueryableMethods.SingleWithoutPredicate,
            QueryableMethods.SingleOrDefaultWithPredicate, QueryableMethods.SingleOrDefaultWithoutPredicate,
            QueryableMethods.LastWithPredicate, QueryableMethods.LastWithoutPredicate,
            QueryableMethods.LastOrDefaultWithPredicate, QueryableMethods.LastOrDefaultWithoutPredicate,
            QueryableMethods.ElementAt, QueryableMethods.ElementAtOrDefault
        ];

        private readonly Dictionary<MethodCallExpression, int> _memberAccessCount = new(ReferenceEqualityComparer.Instance);

        public IEnumerable<MethodCallExpression> Liftable
            => _memberAccessCount.Where(e => e.Value > 1).Select(e => e.Key);

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression is MethodCallExpression { Method.IsGenericMethod: true } subquery
                && SingleResultMethods.Contains(subquery.Method.GetGenericMethodDefinition()))
            {
                if (_memberAccessCount.TryGetValue(subquery, out var count))
                {
                    _memberAccessCount[subquery] = count + 1;
                }
                else
                {
                    // First encounter: count it and visit its arguments once (a later access duplicates the count)
                    _memberAccessCount[subquery] = 1;

                    foreach (var argument in subquery.Arguments)
                    {
                        Visit(argument);
                    }
                }

                return memberExpression;
            }

            return base.VisitMember(memberExpression);
        }
    }
}
