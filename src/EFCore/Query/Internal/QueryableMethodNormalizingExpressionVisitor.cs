// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Infrastructure.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class QueryableMethodNormalizingExpressionVisitor : ExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly SelectManyVerifyingExpressionVisitor _selectManyVerifyingExpressionVisitor = new();
    private readonly GroupJoinConvertingExpressionVisitor _groupJoinConvertingExpressionVisitor = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public QueryableMethodNormalizingExpressionVisitor(QueryCompilationContext queryCompilationContext)
    {
        _queryCompilationContext = queryCompilationContext;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Normalize(Expression expression)
    {
        var result = Visit(expression);

        return _groupJoinConvertingExpressionVisitor.Visit(result);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        // Convert array[x] to array.ElementAt(x)
        if (binaryExpression is
            {
                NodeType: ExpressionType.ArrayIndex,
                Left: var source,
                Right: var index
            })
        {
            return VisitMethodCall(
                Expression.Call(
                    EnumerableMethods.ElementAt.MakeGenericMethod(source.Type.GetSequenceType()), source, index));
        }

        return base.VisitBinary(binaryExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        // Extract information from query metadata method and prune them
        if (method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
            && method.IsGenericMethod
            && ExtractQueryMetadata(methodCallExpression) is Expression expression)
        {
            return expression;
        }

        // Normalize list[x] to list.ElementAt(x)
        if (methodCallExpression is
            {
                Method:
                {
                    Name: "get_Item",
                    IsStatic: false,
                    DeclaringType: Type declaringType
                },
                Object: Expression indexerSource,
                Arguments: [var index]
            }
            && declaringType.GetInterface("IReadOnlyList`1") is not null)
        {
            return VisitMethodCall(
                Expression.Call(
                    EnumerableMethods.ElementAt.MakeGenericMethod(indexerSource.Type.GetSequenceType()),
                    indexerSource,
                    index));
        }

        Expression? visitedExpression = null;
        if (method.DeclaringType == typeof(Enumerable))
        {
            visitedExpression = TryConvertEnumerableToQueryable(methodCallExpression);
        }

        if (method.DeclaringType is { IsGenericType: true }
            && (method.DeclaringType.GetGenericTypeDefinition() == typeof(ICollection<>)
                || method.DeclaringType.GetGenericTypeDefinition() == typeof(List<>))
            && method.Name == nameof(List<int>.Contains))
        {
            visitedExpression = TryConvertListContainsToQueryableContains(methodCallExpression);
        }

        if (method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() is MethodInfo genericMethod
            && (genericMethod == EntityFrameworkQueryableExtensions.IncludeMethodInfo
                || genericMethod == EntityFrameworkQueryableExtensions.ThenIncludeAfterEnumerableMethodInfo
                || genericMethod == EntityFrameworkQueryableExtensions.ThenIncludeAfterReferenceMethodInfo
                || genericMethod == EntityFrameworkQueryableExtensions.NotQuiteIncludeMethodInfo))
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

        if (visitedExpression is MethodCallExpression visitedMethodCall
            && visitedMethodCall.Method.DeclaringType == typeof(Queryable)
            && visitedMethodCall.Method.IsGenericMethod)
        {
            return TryFlattenGroupJoinSelectMany(visitedMethodCall);
        }

        return visitedExpression;
    }

    private static void VerifyReturnType(Expression expression, ParameterExpression lambdaParameter)
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
                            new ExpressionPrinter().PrintExpression(Expression.Lambda(expression, lambdaParameter)),
                            expression.Type.ShortDisplayName()));
                }

                break;
        }
    }

    private Expression? ExtractQueryMetadata(MethodCallExpression methodCallExpression)
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
            _queryCompilationContext.AddTag(methodCallExpression.Arguments[1].GetConstantValue<string>());

            return visitedExpression;
        }

        if (genericMethodDefinition == EntityFrameworkQueryableExtensions.TagWithCallSiteMethodInfo)
        {
            var visitedExpression = Visit(methodCallExpression.Arguments[0]);

            var filePath = methodCallExpression.Arguments[1].GetConstantValue<string>();
            var lineNumber = methodCallExpression.Arguments[2].GetConstantValue<int>();
            _queryCompilationContext.AddTag($"File: {filePath}:{lineNumber}");

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
            && methodCallExpression.Arguments[0] is MemberInitExpression or NewExpression)
        {
            return base.VisitMethodCall(methodCallExpression);
        }

        var arguments = VisitAndConvert(methodCallExpression.Arguments, nameof(VisitMethodCall)).ToArray();

        var enumerableMethod = methodCallExpression.Method;
        var enumerableParameters = enumerableMethod.GetParameters();
        Type[] genericTypeArguments = [];
        if (enumerableMethod.Name is nameof(Enumerable.Min) or nameof(Enumerable.Max))
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
                    var genericType = innerArgument.Type.GetSequenceType();

                    // If innerArgument has ToList applied to it then unwrap it.
                    // Also preserve generic argument of ToList is applied to different type
                    if (arguments[i].Type.TryGetElementType(typeof(List<>)) != null
                        && arguments[i] is MethodCallExpression { Method.IsGenericMethod: true } toListMethodCallExpression
                        && toListMethodCallExpression.Method.GetGenericMethodDefinition() == EnumerableMethods.ToList)
                    {
                        genericType = toListMethodCallExpression.Method.GetGenericArguments()[0];
                        innerArgument = toListMethodCallExpression.Arguments[0];
                    }

                    var innerQueryableElementType = innerArgument.Type.TryGetElementType(typeof(IQueryable<>));
                    if (innerQueryableElementType == null
                        || innerQueryableElementType != genericType)
                    {
                        while (innerArgument is UnaryExpression
                               {
                                   NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs
                               } unaryExpression
                               && unaryExpression.Type.TryGetElementType(typeof(IEnumerable<>)) != null)
                        {
                            innerArgument = unaryExpression.Operand;
                        }

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
        if (methodCallExpression.Object is MemberInitExpression or NewExpression)
        {
            return base.VisitMethodCall(methodCallExpression);
        }

        var sourceType = methodCallExpression.Method.DeclaringType!.GetGenericArguments()[0];

        return VisitMethodCall(
            Expression.Call(
                QueryableMethods.Contains.MakeGenericMethod(sourceType),
                Expression.Call(
                    QueryableMethods.AsQueryable.MakeGenericMethod(sourceType),
                    methodCallExpression.Object!),
                methodCallExpression.Arguments[0]));
    }

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

    private MethodCallExpression TryFlattenGroupJoinSelectMany(MethodCallExpression methodCallExpression)
    {
        var genericMethod = methodCallExpression.Method.GetGenericMethodDefinition();
        if (genericMethod == QueryableMethods.SelectManyWithCollectionSelector)
        {
            // SelectMany
            var selectManySource = methodCallExpression.Arguments[0];
            if (selectManySource is MethodCallExpression { Method.IsGenericMethod: true } groupJoinMethod
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

                if (collectionSelectorBody is MethodCallExpression { Method.IsGenericMethod: true } collectionEndingMethod
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

                    if (inner is MethodCallExpression { Method.IsGenericMethod: true } innerMethodCall
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
                    var genericArguments = groupJoinMethod.Method.GetGenericArguments();
                    genericArguments[^1] = resultSelector.ReturnType;

                    return Expression.Call(
                        (defaultIfEmpty ? QueryableExtensions.LeftJoinMethodInfo : QueryableMethods.Join).MakeGenericMethod(
                            genericArguments),
                        outer, inner, outerKeySelector, innerKeySelector, resultSelector);
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
                //        QueryableMethods.Where.MakeGenericMethod(inner.Type.GetSequenceType()),
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
            if (selectManySource is MethodCallExpression { Method.IsGenericMethod: true } groupJoinMethod
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

                if (groupJoinResultSelectorBody is MethodCallExpression { Method.IsGenericMethod: true } collectionEndingMethod
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

                    var genericArguments = groupJoinMethod.Method.GetGenericArguments();
                    genericArguments[^1] = resultSelector.ReturnType;

                    return Expression.Call(
                        (defaultIfEmpty ? QueryableExtensions.LeftJoinMethodInfo : QueryableMethods.Join).MakeGenericMethod(
                            genericArguments),
                        outer, inner, outerKeySelector, innerKeySelector, resultSelector);
                }
            }
        }

        return methodCallExpression;
    }

    private sealed class GroupJoinConvertingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                && methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.GroupJoin)
            {
                var genericArguments = methodCallExpression.Method.GetGenericArguments();
                var outerSource = methodCallExpression.Arguments[0];
                var innerSource = methodCallExpression.Arguments[1];
                var outerKeySelector = methodCallExpression.Arguments[2].UnwrapLambdaFromQuote();
                var innerKeySelector = methodCallExpression.Arguments[3].UnwrapLambdaFromQuote();
                var resultSelector = methodCallExpression.Arguments[4].UnwrapLambdaFromQuote();

                if (innerSource.Type.IsGenericType
                    && innerSource.Type.GetGenericTypeDefinition() != typeof(IQueryable<>))
                {
                    // In case of collection navigation it can be of enumerable or other type.
                    innerSource = Expression.Call(
                        QueryableMethods.AsQueryable.MakeGenericMethod(innerSource.Type.GetSequenceType()),
                        innerSource);
                }

                var correlationPredicate = ReplacingExpressionVisitor.Replace(
                    outerKeySelector.Parameters[0],
                    resultSelector.Parameters[0],
                    Expression.AndAlso(
                        ExpressionExtensions.CreateEqualsExpression(
                            outerKeySelector.Body,
                            Expression.Constant(null),
                            negated: true),
                        ExpressionExtensions.CreateEqualsExpression(
                            outerKeySelector.Body,
                            innerKeySelector.Body)));

                innerSource = Expression.Call(
                    QueryableMethods.Where.MakeGenericMethod(genericArguments[1]),
                    innerSource,
                    Expression.Quote(
                        Expression.Lambda(
                            correlationPredicate,
                            innerKeySelector.Parameters)));

                var selector = ReplacingExpressionVisitor.Replace(
                    resultSelector.Parameters[1],
                    innerSource,
                    resultSelector.Body);

                if (genericArguments[3].IsGenericType
                    && genericArguments[3].GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    selector = Expression.Call(
                        EnumerableMethods.AsEnumerable.MakeGenericMethod(genericArguments[3].GetSequenceType()),
                        selector);
                }

                return Expression.Call(
                    QueryableMethods.Select.MakeGenericMethod(genericArguments[0], genericArguments[3]),
                    outerSource,
                    Expression.Quote(
                        Expression.Lambda(
                            selector,
                            resultSelector.Parameters[0])));
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }

    private sealed class SelectManyVerifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly List<ParameterExpression> _allowedParameters = [];
        private readonly ISet<string> _allowedMethods = new HashSet<string> { nameof(Queryable.Where), nameof(Queryable.AsQueryable) };

        private ParameterExpression? _rootParameter;
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
            if (_allowedParameters.Contains(parameterExpression)
                || parameterExpression.Name?.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal) == true)
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
