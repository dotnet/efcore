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
            var innerParameter = Expression.Parameter(collection.Type.GetSequenceType(), "element");
            var rewrittenBody = new ReplacingExpressionVisitor([subquery], [innerParameter]).Visit(selectorBody);

            source = ProcessSelectMany(
                source,
                Expression.Lambda(collection, Expression.Parameter(source.SourceElementType, "outer")),
                Expression.Lambda(rewrittenBody, Expression.Parameter(source.SourceElementType, "outer"), innerParameter));
            selectorBody = source.PendingSelector;
        }

        return source;
    }

    private static Expression BuildSingleResultCollection(MethodCallExpression subqueryMethod)
    {
        var method = subqueryMethod.Method.GetGenericMethodDefinition();
        var source = subqueryMethod.Arguments[0];
        var elementType = source.Type.GetSequenceType();

        Expression Where(Expression collection)
            => Expression.Call(QueryableMethods.Where.MakeGenericMethod(elementType), collection, subqueryMethod.Arguments[1]);

        Expression Skip(Expression collection)
            => Expression.Call(QueryableMethods.Skip.MakeGenericMethod(elementType), collection, subqueryMethod.Arguments[1]);

        Expression Reverse(Expression collection)
            => Expression.Call(QueryableMethods.Reverse.MakeGenericMethod(elementType), collection);

        var oneRow = method switch
        {
            _ when method == QueryableMethods.FirstWithPredicate
                || method == QueryableMethods.FirstOrDefaultWithPredicate
                || method == QueryableMethods.SingleWithPredicate
                || method == QueryableMethods.SingleOrDefaultWithPredicate
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

        private readonly Dictionary<MethodCallExpression, int> _memberAccessCount = [with(ReferenceEqualityComparer.Instance)];

        public IEnumerable<MethodCallExpression> Liftable
            => _memberAccessCount.Where(entry => entry.Value > 1).Select(entry => entry.Key);

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