// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SplitIncludeRewritingExpressionVisitor : ExpressionVisitor
    {
        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                && methodCallExpression.Method.IsGenericMethod)
            {
                var genericMethod = methodCallExpression.Method.GetGenericMethodDefinition();
                Expression source = methodCallExpression;
                var singleResult = false;
                var reverseOrdering = false;
                if (genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate
                    || genericMethod == QueryableMethods.FirstWithoutPredicate
                    || genericMethod == QueryableMethods.SingleOrDefaultWithoutPredicate
                    || genericMethod == QueryableMethods.SingleWithoutPredicate)
                {
                    singleResult = true;
                    source = methodCallExpression.Arguments[0];
                }

                if (genericMethod == QueryableMethods.LastOrDefaultWithoutPredicate
                    || genericMethod == QueryableMethods.LastWithoutPredicate)
                {
                    singleResult = true;
                    reverseOrdering = true;
                    source = methodCallExpression.Arguments[0];
                }

                if (source is MethodCallExpression selectMethodCall
                    && selectMethodCall.Method.DeclaringType == typeof(Queryable)
                    && selectMethodCall.Method.IsGenericMethod
                    && selectMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.Select)
                {
                    var selector = RewriteCollectionInclude(
                        selectMethodCall.Arguments[0], selectMethodCall.Arguments[1].UnwrapLambdaFromQuote(), singleResult,
                        reverseOrdering);

                    source = selectMethodCall.Update(
                        selectMethodCall.Object,
                        new[] { selectMethodCall.Arguments[0], Expression.Quote(selector) });

                    if (singleResult)
                    {
                        source = methodCallExpression.Update(methodCallExpression.Object, new[] { source });
                    }

                    return source;
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private LambdaExpression RewriteCollectionInclude(
            Expression source,
            LambdaExpression selector,
            bool singleResult,
            bool reverseOrdering)
        {
            var selectorParameter = selector.Parameters[0];
            var selectorBody = selector.Body;
            var sourceElementType = source.Type.TryGetSequenceType();

            if (reverseOrdering)
            {
                source = Expression.Call(
                    QueryableMethods.Reverse.MakeGenericMethod(sourceElementType),
                    source);
            }

            if (singleResult)
            {
                source = Expression.Call(
                    QueryableMethods.Take.MakeGenericMethod(sourceElementType),
                    source,
                    Expression.Constant(1));
            }

            selectorBody = new CollectionSelectManyInjectingExpressionVisitor(
                this, source, sourceElementType, selectorParameter).Visit(selectorBody);

            return Expression.Lambda(selectorBody, selectorParameter);
        }

        private sealed class CollectionSelectManyInjectingExpressionVisitor : ExpressionVisitor
        {
            private readonly SplitIncludeRewritingExpressionVisitor _parentVisitor;
            private readonly Expression _parentQuery;
            private readonly Type _sourceElementType;
            private readonly ParameterExpression _parameterExpression;

            public CollectionSelectManyInjectingExpressionVisitor(
                SplitIncludeRewritingExpressionVisitor parentVisitor,
                Expression parentQuery,
                Type sourceElementType,
                ParameterExpression parameterExpression)
            {
                _parentQuery = new CloningExpressionVisitor().Visit(parentQuery);
                _parentVisitor = parentVisitor;
                _sourceElementType = sourceElementType;
                _parameterExpression = parameterExpression;
            }

            protected override Expression VisitExtension(Expression extensionExpression)
            {
                if (extensionExpression is MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression
                    && materializeCollectionNavigationExpression.Navigation.IsCollection)
                {
                    var subquery = materializeCollectionNavigationExpression.Subquery;
                    // Extract last select from subquery
                    if (subquery is MethodCallExpression subqueryMethodCallExpression
                        && subqueryMethodCallExpression.Method.IsGenericMethod
                        && subqueryMethodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.Select)
                    {
                        subquery = RewriteSubqueryToSelectMany(subqueryMethodCallExpression.Arguments[0]);
                        subquery = subqueryMethodCallExpression.Update(null, new[] { subquery, subqueryMethodCallExpression.Arguments[1] });
                        subquery = _parentVisitor.Visit(subquery);
                    }
                    else
                    {
                        subquery = RewriteSubqueryToSelectMany(subquery);
                    }

                    return materializeCollectionNavigationExpression.Update(subquery);
                }

                return base.VisitExtension(extensionExpression);
            }

            private Expression RewriteSubqueryToSelectMany(Expression subquery)
            {
                var collectionElementType = subquery.Type.TryGetSequenceType();
                if (subquery.Type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>))
                {
                    subquery = Expression.Call(
                        QueryableMethods.Skip.MakeGenericMethod(collectionElementType),
                        subquery,
                        Expression.Constant(0));
                }

                var newParameter = Expression.Parameter(_parameterExpression.Type);
                subquery = ReplacingExpressionVisitor.Replace(_parameterExpression, newParameter, subquery);

                // Collection selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
                // therefore the delegate type is specified explicitly
                var collectionSelectorLambdaType = typeof(Func<,>).MakeGenericType(
                    _sourceElementType,
                    typeof(IEnumerable<>).MakeGenericType(collectionElementType));

                return Expression.Call(
                    QueryableMethods.SelectManyWithoutCollectionSelector.MakeGenericMethod(_sourceElementType, collectionElementType),
                    _parentQuery,
                    Expression.Quote(Expression.Lambda(collectionSelectorLambdaType, subquery, newParameter)));
            }

            private sealed class CloningExpressionVisitor : ExpressionVisitor
            {
                protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
                {
                    var body = Visit(lambdaExpression.Body);
                    var newParameters = CopyParameters(lambdaExpression.Parameters);
                    body = new ReplacingExpressionVisitor(lambdaExpression.Parameters, newParameters).Visit(body);

                    return lambdaExpression.Update(body, newParameters);
                }

                private static IReadOnlyList<ParameterExpression> CopyParameters(IReadOnlyList<ParameterExpression> parameters)
                {
                    var newParameters = new List<ParameterExpression>();
                    foreach (var parameter in parameters)
                    {
                        newParameters.Add(Expression.Parameter(parameter.Type, parameter.Name));
                    }

                    return newParameters;
                }
            }
        }
    }
}
