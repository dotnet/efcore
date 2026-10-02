// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public partial class NavigationExpandingExpressionVisitor
{
    private NavigationExpansionExpression LiftSingleResultSubqueries(NavigationExpansionExpression source)
    {
        // The lifted subquery may require OUTER APPLY when its correlation cannot be converted to a regular join.
        if (!_queryCompilationContext.SupportsOuterApply)
        {
            return source;
        }

        // Turns member reads off a subquery (e.g. through a let) into the Select(...).First() form written copies already have.
        var selectorBody = _subqueryMemberPushdownExpressionVisitor.Visit(source.PendingSelector);

        var collector = new SingleResultSubqueryReadCollector(this);
        collector.Visit(selectorBody);

        // Existence checks alone don't justify a join: EF already translates those to EXISTS.
        foreach (var (collection, reads) in collector.Reads.Where(e => e.Value.Count > 1 && e.Value.Exists(r => !r.IsExistenceCheck)))
        {
            var innerParameter = Expression.Parameter(collection.Type.GetSequenceType(), "e");
            var rewrittenBody = ReplacingExpressionVisitor.Replace(
                reads.Select(r => r.Node).ToList(),
                reads.Select(r => r.CreateReplacement(innerParameter)).ToList(),
                selectorBody);

            // The collection already references the outer element via source.PendingSelector; this parameter is unused.
            source = ProcessSelectMany(
                source,
                Expression.Lambda(collection, Expression.Parameter(source.SourceElementType, "o")),
                Expression.Lambda(rewrittenBody, Expression.Parameter(source.SourceElementType, "o"), innerParameter));
            selectorBody = source.PendingSelector;
        }

        return source;
    }

    private static Expression BuildSingleResultCollection(MethodCallExpression subqueryMethod, Expression source)
    {
        var method = subqueryMethod.Method.GetGenericMethodDefinition();
        var elementType = source.Type.GetSequenceType();

        if (PredicateLessMethodInfo.TryGetValue(method, out var predicateLessMethod))
        {
            source = Expression.Call(QueryableMethods.Where.MakeGenericMethod(elementType), source, subqueryMethod.Arguments[1]);
            method = predicateLessMethod;
        }

        var oneRow = method switch
        {
            _ when method == QueryableMethods.LastWithoutPredicate || method == QueryableMethods.LastOrDefaultWithoutPredicate
                => Expression.Call(QueryableMethods.Reverse.MakeGenericMethod(elementType), source),
            _ when method == QueryableMethods.ElementAt || method == QueryableMethods.ElementAtOrDefault
                => Expression.Call(QueryableMethods.Skip.MakeGenericMethod(elementType), source, subqueryMethod.Arguments[1]),
            _ => source
        };

        var firstRow = Expression.Call(QueryableMethods.Take.MakeGenericMethod(elementType), oneRow, Expression.Constant(1));

        return Expression.Call(QueryableMethods.DefaultIfEmptyWithoutArgument.MakeGenericMethod(elementType), firstRow);
    }

    private sealed record SingleResultSubqueryRead(Expression Node, LambdaExpression? Projector, bool IsExistenceCheck)
    {
        public Expression CreateReplacement(Expression element)
            => IsExistenceCheck
                ? Expression.MakeBinary(Node.NodeType, element, Expression.Constant(null, element.Type))
                : Projector == null
                    ? element
                    : ReplacingExpressionVisitor.Replace(Projector.Parameters[0], element, Projector.Body);
    }

    private sealed class SingleResultSubqueryReadCollector(NavigationExpandingExpressionVisitor visitor) : ExpressionVisitor
    {
        public Dictionary<Expression, List<SingleResultSubqueryRead>> Reads { get; } = [with(ExpressionEqualityComparer.Instance)];

        // A subquery inside a nested lambda is correlated to that lambda's parameter, not to the element being processed.
        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
            => lambdaExpression;

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual
                && (binaryExpression.Left.IsNullConstantExpression() ? binaryExpression.Right
                        : binaryExpression.Right.IsNullConstantExpression() ? binaryExpression.Left
                        : null).UnwrapTypeConversion(out var convertedType) is MethodCallExpression subquery
                && convertedType == null
                && Decompose(subquery) is (var collection, var projector)
                && visitor._queryCompilationContext.Model.FindEntityType(collection.Type.GetSequenceType())!.FindPrimaryKey() != null
                && (projector == null
                    || (projector.Body is NewExpression or MemberInitExpression
                        && IsRowPreservingProjection(projector.Body, projector.Parameters[0]))))
            {
                // An entity, or an object created by the projection, is null exactly when the row is missing.
                Reads.GetOrAddNew(collection).Add(new SingleResultSubqueryRead(binaryExpression, null, IsExistenceCheck: true));

                return binaryExpression;
            }

            return base.VisitBinary(binaryExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (Decompose(methodCallExpression) is not (var collection, var projector))
            {
                return base.VisitMethodCall(methodCallExpression);
            }

            if (projector == null || IsPropertyChain(projector.Body, projector.Parameters[0]))
            {
                Reads.GetOrAddNew(collection).Add(new SingleResultSubqueryRead(methodCallExpression, projector, IsExistenceCheck: false));
            }

            return methodCallExpression;
        }

        private (Expression Collection, LambdaExpression? Projector)? Decompose(MethodCallExpression subquery)
        {
            // A missing row would read as null rather than as the default value that EF otherwise coalesces to.
            if (!subquery.Method.IsGenericMethod
                || !SubqueryMemberPushdownExpressionVisitor.SupportedMethods.Contains(subquery.Method.GetGenericMethodDefinition())
                || !subquery.Type.IsNullableType())
            {
                return null;
            }

            var rows = subquery.Arguments[0];
            LambdaExpression? projector = null;
            if (rows is MethodCallExpression { Method.IsGenericMethod: true } selectMethodCall
                && selectMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.Select
                && !PredicateLessMethodInfo.ContainsKey(subquery.Method.GetGenericMethodDefinition()))
            {
                // Fold member reads off constructed projections left by member pushdown.
                projector = (LambdaExpression)new ReplacingExpressionVisitor([], [])
                    .Visit(selectMethodCall.Arguments[1].UnwrapLambdaFromQuote());
                rows = selectMethodCall.Arguments[0];
            }

            return visitor._queryCompilationContext.Model.FindEntityType(rows.Type.GetSequenceType()) == null
                ? null
                : (BuildSingleResultCollection(subquery, rows), projector);
        }

        // A constructed projection can still remove rows if one of its members joins to a filtered required navigation.
        private bool IsRowPreservingProjection(Expression expression, ParameterExpression parameter)
            => expression switch
            {
                NewExpression newExpression => newExpression.Arguments.All(a => IsRowPreservingProjection(a, parameter)),
                MemberInitExpression memberInitExpression => IsRowPreservingProjection(memberInitExpression.NewExpression, parameter)
                    && memberInitExpression.Bindings.All(b => b is MemberAssignment assignment
                        && IsRowPreservingProjection(assignment.Expression, parameter)),
                ConstantExpression => true,
                _ => IsPropertyChain(expression, parameter)
            };

        // Properties and navigations read off a missing row are null, as the subquery was, unlike e.g. x.Name ?? "" or x.Id.HasValue.
        private bool IsPropertyChain(Expression expression, ParameterExpression parameter)
            => FindChainType(expression, parameter) != null
                || (expression.UnwrapTypeConversion(out _) is MemberExpression { Expression: { } target } member
                    && FindChainType(target, parameter)?.FindMember(member.Member.Name) is IProperty);

        // A navigation to a filtered entity is excluded: inside the subquery its filter could skip to the next row instead.
        private ITypeBase? FindChainType(Expression expression, ParameterExpression parameter)
            => expression.UnwrapTypeConversion(out _) switch
            {
                var root when root == parameter => visitor._queryCompilationContext.Model.FindEntityType(root.Type),
                MemberExpression { Expression: { } target } member
                    => FindChainType(target, parameter)?.FindMember(member.Member.Name) switch
                    {
                        IComplexProperty { IsCollection: false } complexProperty => complexProperty.ComplexType,
                        INavigation { IsCollection: false } navigation
                            when !visitor.HasApplicableQueryFilters(navigation.TargetEntityType) => navigation.TargetEntityType,
                        _ => null
                    },
                _ => null
            };
    }
}
