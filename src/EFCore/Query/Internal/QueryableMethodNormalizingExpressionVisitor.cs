// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class QueryableMethodNormalizingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;

        private readonly SelectManyVerifyingExpressionVisitor _selectManyVerifyingExpressionVisitor
            = new SelectManyVerifyingExpressionVisitor();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public QueryableMethodNormalizingExpressionVisitor([NotNull] QueryCompilationContext queryCompilationContext)
        {
            Check.NotNull(queryCompilationContext, nameof(Query));

            _queryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var method = methodCallExpression.Method;

            // Extract information from query metadata method and prune them
            if (method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && method.IsGenericMethod
                && ExtractQueryMetadata(methodCallExpression) is Expression expression)
            {
                return expression;
            }

            Expression visitedExpression = null;
            if (method.DeclaringType == typeof(Enumerable))
            {
                visitedExpression = TryConvertEnumerableToQueryable(methodCallExpression);
            }

            if (methodCallExpression.Method.DeclaringType.IsGenericType
                && (methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(ICollection<>)
                    || methodCallExpression.Method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
                && string.Equals(nameof(List<int>.Contains), methodCallExpression.Method.Name))
            {
                visitedExpression = TryConvertListContainsToQueryableContains(methodCallExpression);
            }

            if (method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && method.IsGenericMethod
                && method.GetGenericMethodDefinition() is MethodInfo genericMethod
                && (genericMethod == EntityFrameworkQueryableExtensions.IncludeMethodInfo
                    || genericMethod == EntityFrameworkQueryableExtensions.ThenIncludeAfterEnumerableMethodInfo
                    || genericMethod == EntityFrameworkQueryableExtensions.ThenIncludeAfterReferenceMethodInfo))
            {
                var includeLambda = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                if (includeLambda.ReturnType.IsGenericType
                    && includeLambda.ReturnType.GetGenericTypeDefinition() == typeof(IOrderedEnumerable<>))
                {
                    var source = Visit(methodCallExpression.Arguments[0]);
                    var body = Visit(includeLambda.Body);

                    // we have to rewrite the lambda to accommodate for IOrderedEnumerable<> into IOrderedQueryable<> conversion
                    var lambda = (Expression)Expression.Lambda(body, includeLambda.Parameters);
                    if (methodCallExpression.Arguments[1].NodeType == ExpressionType.Quote)
                    {
                        lambda = Expression.Quote(lambda);
                    }

                    var genericArguments = methodCallExpression.Method.GetGenericArguments();
                    var lastGenericArgument = genericArguments[^1];

                    if (body.Type.IsGenericType
                        && body.Type.GetGenericTypeDefinition() == typeof(IOrderedQueryable<>))
                    {
                        genericArguments[^1] = body.Type;
                        var newIncludeMethod = methodCallExpression.Method.GetGenericMethodDefinition()
                            .MakeGenericMethod(genericArguments);

                        return Expression.Call(newIncludeMethod, source, lambda);
                    }

                    return methodCallExpression.Update(null, new[] { source, lambda });
                }
            }

            if (visitedExpression == null)
            {
                if (method.IsGenericMethod
                    && method.GetGenericMethodDefinition() == QueryableMethods.Select)
                {
                    var selector = methodCallExpression.Arguments[1].UnwrapLambdaFromQuote();
                    VerifyReturnType(selector.Body, selector.Parameters[0]);
                }

                visitedExpression = base.VisitMethodCall(methodCallExpression);
            }

            if (visitedExpression is MethodCallExpression visitedMethodcall
                && visitedMethodcall.Method.DeclaringType == typeof(Queryable)
                && visitedMethodcall.Method.IsGenericMethod)
            {
                return TryFlattenGroupJoinSelectMany(visitedMethodcall);
            }

            return visitedExpression;
        }

        private void VerifyReturnType(Expression expression, ParameterExpression lambdaParameter)
        {
            switch (expression)
            {
                case NewExpression newExpression:
                    foreach (var argument in newExpression.Arguments)
                    {
                        VerifyReturnType(argument, lambdaParameter);
                    }

                    break;

                case MemberInitExpression memberInitExpression:
                    VerifyReturnType(memberInitExpression.NewExpression, lambdaParameter);
                    foreach (var memberBinding in memberInitExpression.Bindings)
                    {
                        if (memberBinding is MemberAssignment memberAssignment)
                        {
                            VerifyReturnType(memberAssignment.Expression, lambdaParameter);
                        }
                    }

                    break;

                default:
                    if (expression.Type.TryGetElementType(typeof(IOrderedEnumerable<>)) != null
                        || expression.Type.TryGetElementType(typeof(IQueryable<>)) != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.QueryInvalidMaterializationType(
                                new ExpressionPrinter().Print(Expression.Lambda(expression, lambdaParameter)),
                                expression.Type.ShortDisplayName()));
                    }

                    break;
            }
        }

        private Expression ExtractQueryMetadata(MethodCallExpression methodCallExpression)
        {
            // We visit innerQueryable first so that we can get information in the same order operators are applied.
            var genericMethodDefinition = methodCallExpression.Method.GetGenericMethodDefinition();

            if (genericMethodDefinition == EntityFrameworkQueryableExtensions.AsTrackingMethodInfo)
            {
                var visitedExpression = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                return visitedExpression;
            }

            if (genericMethodDefinition == EntityFrameworkQueryableExtensions.AsNoTrackingMethodInfo)
            {
                var visitedExpression = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                return visitedExpression;
            }

            if (genericMethodDefinition == EntityFrameworkQueryableExtensions.AsNoTrackingWithIdentityResolutionMethodInfo)
            {
                var visitedExpression = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.QueryTrackingBehavior = QueryTrackingBehavior.NoTrackingWithIdentityResolution;

                return visitedExpression;
            }

            if (genericMethodDefinition == EntityFrameworkQueryableExtensions.TagWithMethodInfo)
            {
                var visitedExpression = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.AddTag((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value);

                return visitedExpression;
            }

            if (genericMethodDefinition == EntityFrameworkQueryableExtensions.IgnoreQueryFiltersMethodInfo)
            {
                var visitedExpression = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.IgnoreQueryFilters = true;

                return visitedExpression;
            }

            if (genericMethodDefinition == EntityFrameworkQueryableExtensions.IgnoreAutoIncludesMethodInfo)
            {
                var visitedExpression = Visit(methodCallExpression.Arguments[0]);
                _queryCompilationContext.IgnoreAutoIncludes = true;

                return visitedExpression;
            }

            return null;
        }

        private Expression TryConvertEnumerableToQueryable(MethodCallExpression methodCallExpression)
        {
            // TODO : CHECK if this is still needed
            if (methodCallExpression.Method.Name == nameof(Enumerable.SequenceEqual))
            {
                // Skip SequenceEqual over enumerable since it could be over byte[] or other array properties
                // Ideally we could make check in nav expansion about it (since it can bind to property)
                // But since we don't translate SequenceEqual anyway, this is fine for now.
                return base.VisitMethodCall(methodCallExpression);
            }

            if (methodCallExpression.Arguments.Count > 0
                && ClientSource(methodCallExpression.Arguments[0]))
            {
                // this is methodCall over closure variable or constant
                return base.VisitMethodCall(methodCallExpression);
            }

            var arguments = VisitAndConvert(methodCallExpression.Arguments, nameof(VisitMethodCall)).ToArray();

            var enumerableMethod = methodCallExpression.Method;
            var enumerableParameters = enumerableMethod.GetParameters();
            Type[] genericTypeArguments = null;
            if (enumerableMethod.Name == nameof(Enumerable.Min)
                || enumerableMethod.Name == nameof(Enumerable.Max))
            {
                genericTypeArguments = new Type[methodCallExpression.Arguments.Count];

                if (!enumerableMethod.IsGenericMethod)
                {
                    genericTypeArguments[0] = enumerableMethod.ReturnType;
                }
                else
                {
                    var argumentTypes = enumerableMethod.GetGenericArguments();
                    if (argumentTypes.Length == genericTypeArguments.Length)
                    {
                        genericTypeArguments = argumentTypes;
                    }
                    else
                    {
                        genericTypeArguments[0] = argumentTypes[0];
                        genericTypeArguments[1] = enumerableMethod.ReturnType;
                    }
                }
            }
            else if (enumerableMethod.IsGenericMethod)
            {
                genericTypeArguments = enumerableMethod.GetGenericArguments();
            }

            foreach (var method in typeof(Queryable).GetTypeInfo().GetDeclaredMethods(methodCallExpression.Method.Name))
            {
                var queryableMethod = method;
                if (queryableMethod.IsGenericMethod)
                {
                    if (genericTypeArguments != null
                        && queryableMethod.GetGenericArguments().Length == genericTypeArguments.Length)
                    {
                        queryableMethod = queryableMethod.MakeGenericMethod(genericTypeArguments);
                    }
                    else
                    {
                        continue;
                    }
                }

                var queryableParameters = queryableMethod.GetParameters();
                if (enumerableParameters.Length != queryableParameters.Length)
                {
                    continue;
                }

                var validMapping = true;
                for (var i = 0; i < enumerableParameters.Length; i++)
                {
                    var enumerableParameterType = enumerableParameters[i].ParameterType;
                    var queryableParameterType = queryableParameters[i].ParameterType;

                    if (enumerableParameterType == queryableParameterType)
                    {
                        continue;
                    }

                    if (CanConvertEnumerableToQueryable(enumerableParameterType, queryableParameterType))
                    {
                        var innerArgument = arguments[i];
                        var genericType = innerArgument.Type.TryGetSequenceType();

                        // If innerArgument has ToList applied to it then unwrap it.
                        // Also preserve generic argument of ToList is applied to different type
                        if (arguments[i].Type.TryGetElementType(typeof(List<>)) != null
                            && arguments[i] is MethodCallExpression toListMethodCallExpression
                            && toListMethodCallExpression.Method.IsGenericMethod
                            && toListMethodCallExpression.Method.GetGenericMethodDefinition() == EnumerableMethods.ToList)
                        {
                            genericType = toListMethodCallExpression.Method.GetGenericArguments()[0];
                            innerArgument = toListMethodCallExpression.Arguments[0];
                        }

                        var innerQueryableElementType = innerArgument.Type.TryGetElementType(typeof(IQueryable<>));
                        if (innerQueryableElementType == null
                            || innerQueryableElementType != genericType)
                        {
                            arguments[i] = Expression.Call(
                                QueryableMethods.AsQueryable.MakeGenericMethod(genericType),
                                innerArgument);
                        }

                        continue;
                    }

                    if (queryableParameterType.IsGenericType
                        && queryableParameterType.GetGenericTypeDefinition() == typeof(Expression<>)
                        && queryableParameterType.GetGenericArguments()[0] == enumerableParameterType)
                    {
                        continue;
                    }

                    validMapping = false;
                    break;
                }

                if (validMapping)
                {
                    return Expression.Call(
                        queryableMethod,
                        arguments.Select(
                            arg => arg is LambdaExpression lambda ? Expression.Quote(lambda) : arg));
                }
            }

            return methodCallExpression.Update(Visit(methodCallExpression.Object), arguments);
        }

        private Expression TryConvertListContainsToQueryableContains(MethodCallExpression methodCallExpression)
        {
            if (ClientSource(methodCallExpression.Object))
            {
                // this is methodCall over closure variable or constant
                return base.VisitMethodCall(methodCallExpression);
            }

            var sourceType = methodCallExpression.Method.DeclaringType.GetGenericArguments()[0];

            return Expression.Call(
                QueryableMethods.Contains.MakeGenericMethod(sourceType),
                Expression.Call(
                    QueryableMethods.AsQueryable.MakeGenericMethod(sourceType),
                    methodCallExpression.Object),
                methodCallExpression.Arguments[0]);
        }

        private static bool ClientSource(Expression expression)
            => expression is ConstantExpression
                || expression is MemberInitExpression
                || expression is NewExpression
                || expression is ParameterExpression parameter
                && parameter.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true;

        private static bool CanConvertEnumerableToQueryable(Type enumerableType, Type queryableType)
        {
            if (enumerableType == typeof(IEnumerable)
                && queryableType == typeof(IQueryable))
            {
                return true;
            }

            if (!enumerableType.IsGenericType
                || !queryableType.IsGenericType
                || !enumerableType.GetGenericArguments().SequenceEqual(queryableType.GetGenericArguments()))
            {
                return false;
            }

            enumerableType = enumerableType.GetGenericTypeDefinition();
            queryableType = queryableType.GetGenericTypeDefinition();

            return enumerableType == typeof(IEnumerable<>) && queryableType == typeof(IQueryable<>)
                || enumerableType == typeof(IOrderedEnumerable<>) && queryableType == typeof(IOrderedQueryable<>);
        }

        private Expression TryFlattenGroupJoinSelectMany(MethodCallExpression methodCallExpression)
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
                    var outer = groupJoinMethod.Arguments[0];
                    var inner = groupJoinMethod.Arguments[1];
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

                    if (!correlatedCollectionSelector)
                    {
                        inner = Visit(
                            ReplacingExpressionVisitor.Replace(
                                groupJoinResultSelector.Parameters[1], inner, collectionSelectorBody));

                        if (inner is MethodCallExpression innerMethodCall
                            && innerMethodCall.Method.IsGenericMethod
                            && innerMethodCall.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable
                            && innerMethodCall.Type == innerMethodCall.Arguments[0].Type)
                        {
                            // Remove redundant AsQueryable.
                            // It is fine to leave it in the tree since it is no-op
                            inner = innerMethodCall.Arguments[0];
                        }

                        var resultSelectorBody = ReplacingExpressionVisitor.Replace(
                            selectManyResultSelector.Parameters[0],
                            groupJoinResultSelector.Body,
                            selectManyResultSelector.Body);

                        var resultSelector = Expression.Lambda(
                            resultSelectorBody,
                            groupJoinResultSelector.Parameters[0],
                            selectManyResultSelector.Parameters[1]);

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
                    // TODO: Convert correlated patterns to SelectMany
                    //else
                    //{
                    //    var outerParameter = outerKeySelector.Parameters[0];
                    //    var innerParameter = innerKeySelector.Parameters[0];
                    //    var correlationPredicate = Expression.Equal(
                    //        outerKeySelector.Body,
                    //        innerKeySelector.Body);

                    //    inner = Expression.Call(
                    //        QueryableMethods.Where.MakeGenericMethod(inner.Type.TryGetSequenceType()),
                    //        inner,
                    //        Expression.Quote(Expression.Lambda(correlationPredicate, innerParameter)));

                    //    inner = ReplacingExpressionVisitor.Replace(
                    //        groupJoinResultSelector.Parameters[1],
                    //        inner,
                    //        collectionSelectorBody);

                    //    inner = Expression.Quote(Expression.Lambda(inner, outerParameter));

                    //    var resultSelectorBody = ReplacingExpressionVisitor.Replace(
                    //        selectManyResultSelector.Parameters[0],
                    //        groupJoinResultSelector.Body,
                    //        selectManyResultSelector.Body);

                    //    var resultSelector = Expression.Lambda(
                    //        resultSelectorBody,
                    //        groupJoinResultSelector.Parameters[0],
                    //        selectManyResultSelector.Parameters[1]);
                    //}
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
                    var outer = groupJoinMethod.Arguments[0];
                    var inner = groupJoinMethod.Arguments[1];
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

                        inner = Visit(inner);

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

            return methodCallExpression;
        }

        private sealed class SelectManyVerifyingExpressionVisitor : ExpressionVisitor
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
                Check.NotNull(lambdaExpression, nameof(lambdaExpression));

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
                Check.NotNull(methodCallExpression, nameof(methodCallExpression));

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
                Check.NotNull(parameterExpression, nameof(parameterExpression));

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
