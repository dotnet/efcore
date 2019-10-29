// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class GroupJoinFlatteningExpressionVisitor : ExpressionVisitor
    {
        private static readonly EnumerableToQueryableMethodConvertingExpressionVisitor _enumerableToQueryableReMappingExpressionVisitor
            = new EnumerableToQueryableMethodConvertingExpressionVisitor();

        private readonly SelectManyVerifyingExpressionVisitor _selectManyVerifyingExpressionVisitor
            = new SelectManyVerifyingExpressionVisitor();

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                && methodCallExpression.Method.IsGenericMethod)
            {
                var genericMethod = methodCallExpression.Method.GetGenericMethodDefinition();
                if (genericMethod == QueryableMethods.SelectManyWithCollectionSelector)
                {
                    // SelectMany
                    var selectManySource = methodCallExpression.Arguments[0];
                    if (selectManySource is MethodCallExpression groupJoinMethod
                        && groupJoinMethod.Method.IsGenericMethod
                        && groupJoinMethod.Method.GetGenericMethodDefinition() == QueryableMethods.GroupJoin)
                    {
                        // GroupJoin
                        var outer = Visit(groupJoinMethod.Arguments[0]);
                        var inner = Visit(groupJoinMethod.Arguments[1]);
                        var outerKeySelector = groupJoinMethod.Arguments[2].UnwrapLambdaFromQuote();
                        var innerKeySelector = groupJoinMethod.Arguments[3].UnwrapLambdaFromQuote();
                        var groupJoinResultSelector = groupJoinMethod.Arguments[4].UnwrapLambdaFromQuote();

                        var selectManyCollectionSelector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                        var selectManyResultSelector = methodCallExpression.Arguments[2].UnwrapLambdaFromQuote();

                        var collectionSelectorBody = selectManyCollectionSelector.Body;
                        var defaultIfEmpty = false;

                        if (collectionSelectorBody is MethodCallExpression collectionEndingMethod
                            && collectionEndingMethod.Method.IsGenericMethod
                            && collectionEndingMethod.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
                        {
                            defaultIfEmpty = true;
                            collectionSelectorBody = collectionEndingMethod.Arguments[0];
                        }

                        collectionSelectorBody = ReplacingExpressionVisitor.Replace(
                            selectManyCollectionSelector.Parameters[0],
                            groupJoinResultSelector.Body,
                            collectionSelectorBody);

                        var correlatedCollectionSelector = _selectManyVerifyingExpressionVisitor
                            .VerifyCollectionSelector(
                                collectionSelectorBody, groupJoinResultSelector.Parameters[1]);

                        if (correlatedCollectionSelector)
                        {
                            var outerParameter = outerKeySelector.Parameters[0];
                            var innerParameter = innerKeySelector.Parameters[0];
                            var correlationPredicate = Expression.Equal(
                                outerKeySelector.Body,
                                innerKeySelector.Body);

                            inner = Expression.Call(
                                QueryableMethods.Where.MakeGenericMethod(inner.Type.TryGetSequenceType()),
                                inner,
                                Expression.Quote(Expression.Lambda(correlationPredicate, innerParameter)));

                            inner = ReplacingExpressionVisitor.Replace(
                                groupJoinResultSelector.Parameters[1],
                                inner,
                                collectionSelectorBody);

                            inner = Expression.Quote(Expression.Lambda(inner, outerParameter));
                        }
                        else
                        {
                            inner = _enumerableToQueryableReMappingExpressionVisitor.Visit(
                                ReplacingExpressionVisitor.Replace(
                                    groupJoinResultSelector.Parameters[1],
                                    inner,
                                    collectionSelectorBody));

                            if (inner is MethodCallExpression innerMethodCall
                                && innerMethodCall.Method.IsGenericMethod
                                && innerMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable
                                && innerMethodCall.Type == innerMethodCall.Arguments[0].Type)
                            {
                                // Remove redundant AsQueryable.
                                // It is fine to leave it in the tree since it is no-op
                                inner = innerMethodCall.Arguments[0];
                            }
                        }

                        var resultSelectorBody = ReplacingExpressionVisitor.Replace(
                            selectManyResultSelector.Parameters[0],
                            groupJoinResultSelector.Body,
                            selectManyResultSelector.Body);

                        var resultSelector = Expression.Lambda(
                            resultSelectorBody,
                            groupJoinResultSelector.Parameters[0],
                            selectManyResultSelector.Parameters[1]);

                        if (correlatedCollectionSelector)
                        {
                            // select many case
                        }
                        else
                        {
                            // join case
                            if (defaultIfEmpty)
                            {
                                // left join
                                return Expression.Call(
                                    QueryableExtensions.LeftJoinMethodInfo.MakeGenericMethod(
                                        outer.Type.TryGetSequenceType(),
                                        inner.Type.TryGetSequenceType(),
                                        outerKeySelector.ReturnType,
                                        resultSelector.ReturnType),
                                    outer,
                                    inner,
                                    outerKeySelector,
                                    innerKeySelector,
                                    resultSelector);
                            }

                            // inner join
                            return Expression.Call(
                                QueryableMethods.Join.MakeGenericMethod(
                                    outer.Type.TryGetSequenceType(),
                                    inner.Type.TryGetSequenceType(),
                                    outerKeySelector.ReturnType,
                                    resultSelector.ReturnType),
                                outer,
                                inner,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector);
                        }
                    }
                }
                else if (genericMethod == QueryableMethods.SelectManyWithoutCollectionSelector)
                {
                    // SelectMany
                    var selectManySource = methodCallExpression.Arguments[0];
                    if (selectManySource is MethodCallExpression groupJoinMethod
                        && groupJoinMethod.Method.IsGenericMethod
                        && groupJoinMethod.Method.GetGenericMethodDefinition() == QueryableMethods.GroupJoin)
                    {
                        // GroupJoin
                        var outer = Visit(groupJoinMethod.Arguments[0]);
                        var inner = Visit(groupJoinMethod.Arguments[1]);
                        var outerKeySelector = groupJoinMethod.Arguments[2].UnwrapLambdaFromQuote();
                        var innerKeySelector = groupJoinMethod.Arguments[3].UnwrapLambdaFromQuote();
                        var groupJoinResultSelector = groupJoinMethod.Arguments[4].UnwrapLambdaFromQuote();

                        var selectManyResultSelector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();

                        var groupJoinResultSelectorBody = groupJoinResultSelector.Body;
                        var defaultIfEmpty = false;

                        if (groupJoinResultSelectorBody is MethodCallExpression collectionEndingMethod
                            && collectionEndingMethod.Method.IsGenericMethod
                            && collectionEndingMethod.Method.GetGenericMethodDefinition() == QueryableMethods.DefaultIfEmptyWithoutArgument)
                        {
                            defaultIfEmpty = true;
                            groupJoinResultSelectorBody = collectionEndingMethod.Arguments[0];
                        }

                        var correlatedCollectionSelector = _selectManyVerifyingExpressionVisitor
                            .VerifyCollectionSelector(
                                groupJoinResultSelectorBody, groupJoinResultSelector.Parameters[1]);

                        if (!correlatedCollectionSelector)
                        {
                            inner = ReplacingExpressionVisitor.Replace(
                                groupJoinResultSelector.Parameters[1],
                                inner,
                                groupJoinResultSelectorBody);

                            inner = ReplacingExpressionVisitor.Replace(
                                selectManyResultSelector.Parameters[0],
                                inner,
                                selectManyResultSelector.Body);

                            inner = _enumerableToQueryableReMappingExpressionVisitor.Visit(inner);

                            var resultSelector = Expression.Lambda(
                                innerKeySelector.Parameters[0],
                                groupJoinResultSelector.Parameters[0],
                                innerKeySelector.Parameters[0]);

                            // join case
                            if (defaultIfEmpty)
                            {
                                // left join
                                return Expression.Call(
                                    QueryableExtensions.LeftJoinMethodInfo.MakeGenericMethod(
                                        outer.Type.TryGetSequenceType(),
                                        inner.Type.TryGetSequenceType(),
                                        outerKeySelector.ReturnType,
                                        resultSelector.ReturnType),
                                    outer,
                                    inner,
                                    outerKeySelector,
                                    innerKeySelector,
                                    resultSelector);
                            }

                            // inner join
                            return Expression.Call(
                                QueryableMethods.Join.MakeGenericMethod(
                                    outer.Type.TryGetSequenceType(),
                                    inner.Type.TryGetSequenceType(),
                                    outerKeySelector.ReturnType,
                                    resultSelector.ReturnType),
                                outer,
                                inner,
                                outerKeySelector,
                                innerKeySelector,
                                resultSelector);
                        }
                    }
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private class SelectManyVerifyingExpressionVisitor : ExpressionVisitor
        {
            private readonly List<ParameterExpression> _allowedParameters = new List<ParameterExpression>();
            private readonly ISet<string> _allowedMethods = new HashSet<string> { nameof(Queryable.Where), nameof(Queryable.AsQueryable) };

            private ParameterExpression _rootParameter;
            private int _rootParameterCount;
            private bool _correlated;

            public bool VerifyCollectionSelector(Expression body, ParameterExpression rootParameter)
            {
                _correlated = false;
                _rootParameterCount = 0;
                _rootParameter = rootParameter;

                Visit(body);

                if (_rootParameterCount == 1)
                {
                    var expression = body;
                    while (expression != null)
                    {
                        if (expression is MemberExpression memberExpression)
                        {
                            expression = memberExpression.Expression;
                        }
                        else if (expression is MethodCallExpression methodCallExpression
                            && methodCallExpression.Method.DeclaringType == typeof(Queryable))
                        {
                            expression = methodCallExpression.Arguments[0];
                        }
                        else if (expression is ParameterExpression)
                        {
                            if (expression != _rootParameter)
                            {
                                _correlated = true;
                            }

                            break;
                        }
                        else
                        {
                            _correlated = true;
                            break;
                        }
                    }
                }

                _rootParameter = null;

                return _correlated;
            }

            protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
            {
                try
                {
                    _allowedParameters.AddRange(lambdaExpression.Parameters);

                    return base.VisitLambda(lambdaExpression);
                }
                finally
                {
                    foreach (var parameter in lambdaExpression.Parameters)
                    {
                        _allowedParameters.Remove(parameter);
                    }
                }
            }

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (_correlated)
                {
                    return methodCallExpression;
                }

                if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                    && !_allowedMethods.Contains(methodCallExpression.Method.Name))
                {
                    if (methodCallExpression.Method.IsGenericMethod
                        && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.Select)
                    {
                        var selector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                        if (selector.Body == selector.Parameters[0])
                        {
                            // identity projection is allowed
                            return methodCallExpression;
                        }
                    }

                    _correlated = true;

                    return methodCallExpression;
                }

                return base.VisitMethodCall(methodCallExpression);
            }

            protected override Expression VisitParameter(ParameterExpression parameterExpression)
            {
                if (_allowedParameters.Contains(parameterExpression))
                {
                    return parameterExpression;
                }

                if (parameterExpression == _rootParameter)
                {
                    _rootParameterCount++;

                    return parameterExpression;
                }

                _correlated = true;

                return base.VisitParameter(parameterExpression);
            }
        }
    }
}
