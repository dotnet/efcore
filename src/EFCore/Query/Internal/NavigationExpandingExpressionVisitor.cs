// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public partial class NavigationExpandingExpressionVisitor : ExpressionVisitor
{
    private static readonly PropertyInfo QueryContextContextPropertyInfo
        = typeof(QueryContext).GetTypeInfo().GetDeclaredProperty(nameof(QueryContext.Context))!;

    private static readonly Dictionary<MethodInfo, MethodInfo> PredicateLessMethodInfo = new()
    {
        { QueryableMethods.FirstWithPredicate, QueryableMethods.FirstWithoutPredicate },
        { QueryableMethods.FirstOrDefaultWithPredicate, QueryableMethods.FirstOrDefaultWithoutPredicate },
        { QueryableMethods.SingleWithPredicate, QueryableMethods.SingleWithoutPredicate },
        { QueryableMethods.SingleOrDefaultWithPredicate, QueryableMethods.SingleOrDefaultWithoutPredicate },
        { QueryableMethods.LastWithPredicate, QueryableMethods.LastWithoutPredicate },
        { QueryableMethods.LastOrDefaultWithPredicate, QueryableMethods.LastOrDefaultWithoutPredicate }
    };

    private static readonly List<MethodInfo> SupportedFilteredIncludeOperations = new()
    {
        QueryableMethods.Where,
        QueryableMethods.OrderBy,
        QueryableMethods.OrderByDescending,
        QueryableMethods.ThenBy,
        QueryableMethods.ThenByDescending,
        QueryableMethods.Skip,
        QueryableMethods.Take,
        QueryableMethods.AsQueryable
    };

    private readonly QueryTranslationPreprocessor _queryTranslationPreprocessor;
    private readonly QueryCompilationContext _queryCompilationContext;
    //private readonly PendingSelectorExpandingExpressionVisitor _pendingSelectorExpandingExpressionVisitor;
    private readonly SubqueryMemberPushdownExpressionVisitor _subqueryMemberPushdownExpressionVisitor;
    private readonly NullCheckRemovingExpressionVisitor _nullCheckRemovingExpressionVisitor;
    private readonly ReducingExpressionVisitor _reducingExpressionVisitor;
    //private readonly EntityReferenceOptionalMarkingExpressionVisitor _entityReferenceOptionalMarkingExpressionVisitor;
    private readonly RemoveRedundantNavigationComparisonExpressionVisitor _removeRedundantNavigationComparisonExpressionVisitor;
    private readonly HashSet<string> _parameterNames = new();
    //private readonly ParameterExtractingExpressionVisitor _parameterExtractingExpressionVisitor;
    private readonly INavigationExpansionExtensibilityHelper _extensibilityHelper;
    private readonly HashSet<IEntityType> _nonCyclicAutoIncludeEntityTypes;

    private readonly Dictionary<IEntityType, LambdaExpression> _parameterizedQueryFilterPredicateCache
        = new();

    //private readonly Parameters _parameters = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NavigationExpandingExpressionVisitor(
        QueryTranslationPreprocessor queryTranslationPreprocessor,
        QueryCompilationContext queryCompilationContext,
        IEvaluatableExpressionFilter evaluatableExpressionFilter,
        INavigationExpansionExtensibilityHelper extensibilityHelper)
    {
        _queryTranslationPreprocessor = queryTranslationPreprocessor;
        _queryCompilationContext = queryCompilationContext;
        _extensibilityHelper = extensibilityHelper;
        //_pendingSelectorExpandingExpressionVisitor = new PendingSelectorExpandingExpressionVisitor(this, extensibilityHelper);
        _subqueryMemberPushdownExpressionVisitor = new SubqueryMemberPushdownExpressionVisitor(queryCompilationContext.Model);
        _nullCheckRemovingExpressionVisitor = new NullCheckRemovingExpressionVisitor();
        _reducingExpressionVisitor = new ReducingExpressionVisitor();
        //_entityReferenceOptionalMarkingExpressionVisitor = new EntityReferenceOptionalMarkingExpressionVisitor();
        _removeRedundantNavigationComparisonExpressionVisitor = new RemoveRedundantNavigationComparisonExpressionVisitor(
            queryCompilationContext.Logger);
        //_parameterExtractingExpressionVisitor = new ParameterExtractingExpressionVisitor(
        //    evaluatableExpressionFilter,
        //    _parameters,
        //    _queryCompilationContext.ContextType,
        //    _queryCompilationContext.Model,
        //    _queryCompilationContext.Logger,
        //    parameterize: false,
        //    generateContextAccessors: true);

        _nonCyclicAutoIncludeEntityTypes = !_queryCompilationContext.IgnoreAutoIncludes ? new HashSet<IEntityType>() : null!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Expand(Expression query)
    {
        var result = Visit(query);

        ////if (result is GroupByNavigationExpansionExpression)
        ////{
        ////    // This indicates that GroupBy was not condensed out of grouping operator.
        ////    throw new InvalidOperationException(CoreStrings.TranslationFailed(query.Print()));
        ////}

        ////result = new PendingSelectorExpandingExpressionVisitor(this, _extensibilityHelper, applyIncludes: true).Visit(result);
        // TODO: trim additional navigations
        if (result is NavigationExpansionExpression navigationExpansionExpression
            && navigationExpansionExpression.PendingOrderings.Count > 0)
        {
            ApplyPendingOrderings(navigationExpansionExpression);
        }
        result = new IncludeApplyingExpressionVisitor(this, _extensibilityHelper).Visit(result);
        result = Reduce(result);

        ////var dbContextOnQueryContextPropertyAccess =
        ////    Expression.Convert(
        ////        Expression.Property(
        ////            QueryCompilationContext.QueryContextParameter,
        ////            QueryContextContextPropertyInfo),
        ////        _queryCompilationContext.ContextType);

        ////foreach (var (key, value) in _parameters.ParameterValues)
        ////{
        ////    var lambda = (LambdaExpression)value!;
        ////    var remappedLambdaBody = ReplacingExpressionVisitor.Replace(
        ////        lambda.Parameters[0],
        ////        dbContextOnQueryContextPropertyAccess,
        ////        lambda.Body);

        ////    _queryCompilationContext.RegisterRuntimeParameter(
        ////        key,
        ////        Expression.Lambda(
        ////            remappedLambdaBody.Type.IsValueType
        ////                ? Expression.Convert(remappedLambdaBody, typeof(object))
        ////                : remappedLambdaBody,
        ////            QueryCompilationContext.QueryContextParameter));
        ////}

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case QueryRootExpression queryRootExpression:
                var entityType = queryRootExpression.EntityType;
                var parameterName = GetParameterName(entityType.ShortName()[0].ToString().ToLowerInvariant());
                var node = new LeafNode(entityType.ClrType);
                var element = new Element(node);
                var entityExpression = new EntityExpression(element, entityType);
                var navigationExpansionExpression = new NavigationExpansionExpression(
                    queryRootExpression,
                    node,
                    entityExpression,
                    parameterName);

                ((INode)node).AttachOwner(navigationExpansionExpression);
                // TODO Apply query filter
                return navigationExpansionExpression;

            case NavigationExpansionExpression _:
            case EntityExpression _:
            case Element _:
                return extensionExpression;

            // Skip owned navigation reference

            case MaterializeCollectionNavigationExpression _:
            case IncludeExpression _:
                return base.VisitExtension(extensionExpression);

            default:
                throw new InvalidFilterCriteriaException();
                //return base.VisitExtension(extensionExpression);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMember(MemberExpression memberExpression)
    {
        var innerExpression = Visit(memberExpression.Expression);

        // Convert ICollection<T>.Count to Count<T>()
        if (memberExpression.Expression != null
            && innerExpression != null
            && memberExpression.Member.Name == nameof(ICollection<int>.Count)
            && memberExpression.Expression.Type.GetInterfaces().Append(memberExpression.Expression.Type)
                .Any(
                    e => e.IsGenericType
                        && (e.GetGenericTypeDefinition() is Type genericTypeDefinition
                            && (genericTypeDefinition == typeof(ICollection<>)
                                || genericTypeDefinition == typeof(IReadOnlyCollection<>)))))
        {
            var innerQueryable = UnwrapCollectionMaterialization(innerExpression);

            if (innerQueryable.Type.TryGetElementType(typeof(IQueryable<>)) != null)
            {
                return Visit(
                    Expression.Call(
                        QueryableMethods.CountWithoutPredicate.MakeGenericMethod(innerQueryable.Type.GetSequenceType()),
                        innerQueryable));
            }
        }

        var updatedExpression = (Expression)memberExpression.Update(innerExpression);
        if (innerExpression is NavigationExpansionExpression navigationExpansionExpression
            && navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null)
        {
            // This is FirstOrDefault.Member
            // Subquery member pushdown doesn't push down collection navigations which follows queryable chain
            var expandedExpression =
                new ExpandingExpressionVisitor(this, navigationExpansionExpression, _extensibilityHelper).Visit(updatedExpression);
            if (expandedExpression != updatedExpression)
            {
                updatedExpression = Visit(expandedExpression);
            }
        }

        return updatedExpression;
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
        if (method.DeclaringType == typeof(Queryable)
            || method.DeclaringType == typeof(QueryableExtensions)
            || method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
        {
            var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;
            // First argument is source
            var firstArgument = Visit(methodCallExpression.Arguments[0]);
            if (firstArgument is NavigationExpansionExpression source)
            {
                if (source.PendingOrderings.Count > 0
                    && genericMethod != QueryableMethods.ThenBy
                    && genericMethod != QueryableMethods.ThenByDescending)
                {
                    ApplyPendingOrderings(source);
                }

                switch (method.Name)
                {
                    case nameof(Queryable.AsQueryable)
                        when genericMethod == QueryableMethods.AsQueryable:
                        return source;

                    case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithoutPredicate:
                    case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithoutPredicate:
                    case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithoutPredicate:
                        return ProcessAllAnyCountLongCount(
                            source,
                            genericMethod,
                            predicate: null);

                    case nameof(Queryable.All)
                        when genericMethod == QueryableMethods.All:
                    case nameof(Queryable.Any)
                        when genericMethod == QueryableMethods.AnyWithPredicate:
                    case nameof(Queryable.Count)
                        when genericMethod == QueryableMethods.CountWithPredicate:
                    case nameof(Queryable.LongCount)
                        when genericMethod == QueryableMethods.LongCountWithPredicate:
                        return ProcessAllAnyCountLongCount(
                            source,
                            genericMethod,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                    case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithoutSelector(method):
                    case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithoutSelector:
                    case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithoutSelector:
                    case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithoutSelector(method):
                        return ProcessAverageMaxMinSum(
                            source,
                            genericMethod ?? method,
                            selector: null);

                    case nameof(Queryable.Average)
                        when QueryableMethods.IsAverageWithSelector(method):
                    case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithSelector(method):
                    case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithSelector:
                    case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithSelector:
                        return ProcessAverageMaxMinSum(
                            source,
                            genericMethod ?? method,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                    case nameof(Queryable.Distinct)
                        when genericMethod == QueryableMethods.Distinct:
                        return ProcessDistinct(source, genericMethod);

                    case nameof(Queryable.Skip)
                        when genericMethod == QueryableMethods.Skip:
                    case nameof(Queryable.Take)
                        when genericMethod == QueryableMethods.Take:
                        return ProcessSkipTake(
                            source,
                            genericMethod,
                            methodCallExpression.Arguments[1]);

                    case nameof(Queryable.Contains)
                        when genericMethod == QueryableMethods.Contains:
                        return ProcessContains(
                            source,
                            methodCallExpression.Arguments[1]);

                    case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithoutPredicate:
                    case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithoutPredicate:
                    case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithoutPredicate:
                    case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithoutPredicate:
                    case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithoutPredicate:
                    case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithoutPredicate:
                        return ProcessFirstSingleLastOrDefault(
                            source,
                            genericMethod,
                            predicate: null,
                            methodCallExpression.Type);

                    case nameof(Queryable.First)
                        when genericMethod == QueryableMethods.FirstWithPredicate:
                    case nameof(Queryable.FirstOrDefault)
                        when genericMethod == QueryableMethods.FirstOrDefaultWithPredicate:
                    case nameof(Queryable.Single)
                        when genericMethod == QueryableMethods.SingleWithPredicate:
                    case nameof(Queryable.SingleOrDefault)
                        when genericMethod == QueryableMethods.SingleOrDefaultWithPredicate:
                    case nameof(Queryable.Last)
                        when genericMethod == QueryableMethods.LastWithPredicate:
                    case nameof(Queryable.LastOrDefault)
                        when genericMethod == QueryableMethods.LastOrDefaultWithPredicate:
                        return ProcessFirstSingleLastOrDefault(
                            source,
                            genericMethod,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                            methodCallExpression.Type);

                    case nameof(Queryable.Join)
                        when genericMethod == QueryableMethods.Join:
                    {
                        var secondArgument = Visit(methodCallExpression.Arguments[1]);
                        secondArgument = UnwrapCollectionMaterialization(secondArgument);
                        if (secondArgument is NavigationExpansionExpression innerSource)
                        {
                            return ProcessJoin(
                                source,
                                innerSource,
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                        }

                        goto default;
                    }

                    case nameof(QueryableExtensions.LeftJoin)
                        when genericMethod == QueryableExtensions.LeftJoinMethodInfo:
                    {
                        var secondArgument = Visit(methodCallExpression.Arguments[1]);
                        secondArgument = UnwrapCollectionMaterialization(secondArgument);
                        if (secondArgument is NavigationExpansionExpression innerSource)
                        {
                            return ProcessLeftJoin(
                                source,
                                innerSource,
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                        }

                        goto default;
                    }

                    case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithoutCollectionSelector:
                        return ProcessSelectMany(
                            source,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                            null);

                    case nameof(Queryable.SelectMany)
                        when genericMethod == QueryableMethods.SelectManyWithCollectionSelector:
                        return ProcessSelectMany(
                            source,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                            methodCallExpression.Arguments[2].UnwrapLambdaFromQuote());

                    case nameof(Queryable.Concat)
                        when genericMethod == QueryableMethods.Concat:
                    case nameof(Queryable.Except)
                        when genericMethod == QueryableMethods.Except:
                    case nameof(Queryable.Intersect)
                        when genericMethod == QueryableMethods.Intersect:
                    case nameof(Queryable.Union)
                        when genericMethod == QueryableMethods.Union:
                    {
                        var secondArgument = Visit(methodCallExpression.Arguments[1]);
                        secondArgument = UnwrapCollectionMaterialization(secondArgument);
                        if (secondArgument is NavigationExpansionExpression innerSource)
                        {
                            return ProcessSetOperation(source, genericMethod, innerSource);
                        }

                        goto default;
                    }

                    case nameof(Queryable.Cast)
                        when genericMethod == QueryableMethods.Cast:
                    case nameof(Queryable.OfType)
                        when genericMethod == QueryableMethods.OfType:
                        return ProcessCastOfType(
                            source,
                            genericMethod,
                            methodCallExpression.Type.GetSequenceType());

                    case nameof(EntityFrameworkQueryableExtensions.Include):
                        return ProcessInclude(
                            source,
                            methodCallExpression.Arguments[1],
                            thenInclude: false,
                            setLoaded: true);

                    case nameof(EntityFrameworkQueryableExtensions.ThenInclude):
                        return ProcessInclude(
                            source,
                            methodCallExpression.Arguments[1],
                            thenInclude: true,
                            setLoaded: true);

                    case nameof(EntityFrameworkQueryableExtensions.NotQuiteInclude):
                        return ProcessInclude(
                            source,
                            methodCallExpression.Arguments[1],
                            thenInclude: false,
                            setLoaded: false);

                    //case nameof(Queryable.GroupBy)
                    //    when genericMethod == QueryableMethods.GroupByWithKeySelector:
                    //    return ProcessGroupBy(
                    //        source,
                    //        methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                    //        null,
                    //        null);

                    //case nameof(Queryable.GroupBy)
                    //    when genericMethod == QueryableMethods.GroupByWithKeyElementSelector:
                    //    return ProcessGroupBy(
                    //        source,
                    //        methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                    //        methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                    //        null);

                    //case nameof(Queryable.GroupBy)
                    //    when genericMethod == QueryableMethods.GroupByWithKeyElementResultSelector:
                    //    return ProcessGroupBy(
                    //        source,
                    //        methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                    //        methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                    //        methodCallExpression.Arguments[3].UnwrapLambdaFromQuote());

                    //case nameof(Queryable.GroupBy)
                    //    when genericMethod == QueryableMethods.GroupByWithKeyResultSelector:
                    //    return ProcessGroupBy(
                    //        source,
                    //        methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                    //        null,
                    //        methodCallExpression.Arguments[2].UnwrapLambdaFromQuote());

                    case nameof(Queryable.OrderBy)
                        when genericMethod == QueryableMethods.OrderBy:
                    case nameof(Queryable.OrderByDescending)
                        when genericMethod == QueryableMethods.OrderByDescending:
                        return ProcessOrderByThenBy(
                            source,
                            genericMethod,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                            thenBy: false);

                    case nameof(Queryable.ThenBy)
                        when genericMethod == QueryableMethods.ThenBy:
                    case nameof(Queryable.ThenByDescending)
                        when genericMethod == QueryableMethods.ThenByDescending:
                        return ProcessOrderByThenBy(
                            source,
                            genericMethod,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                            thenBy: true);

                    case nameof(Queryable.Reverse)
                        when genericMethod == QueryableMethods.Reverse:
                        return ProcessReverse(source);

                    case nameof(Queryable.Select)
                        when genericMethod == QueryableMethods.Select:
                        return ProcessSelect(
                            source,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                    case nameof(Queryable.Where)
                        when genericMethod == QueryableMethods.Where:
                        return ProcessWhere(
                            source,
                            methodCallExpression.Arguments[1].UnwrapLambdaFromQuote());

                    case nameof(Queryable.DefaultIfEmpty)
                        when genericMethod == QueryableMethods.DefaultIfEmptyWithoutArgument:
                        return ProcessDefaultIfEmpty(source);

                    default:
                        // Aggregate overloads
                        // GroupJoin overloads
                        // Zip
                        // SequenceEqual overloads
                        // ElementAt
                        // ElementAtOrDefault
                        // SkipWhile
                        // TakeWhile
                        // DefaultIfEmpty with argument
                        // Index based lambda overloads of Where, SkipWhile, TakeWhile, Select, SelectMany
                        // IEqualityComparer overloads of Distinct, Contains, Join, Except, Intersect, Union, OrderBy, ThenBy, OrderByDescending, ThenByDescending, GroupBy
                        throw new InvalidOperationException(
                            CoreStrings.TranslationFailed(
                                _reducingExpressionVisitor.Visit(methodCallExpression).Print()));
                }
            }



            if (genericMethod == QueryableMethods.AsQueryable)
            {
                //if (firstArgument is NavigationTreeExpression navigationTreeExpression
                //    && navigationTreeExpression.Type.IsGenericType
                //    && navigationTreeExpression.Type.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                //{
                //    // This is groupingElement.AsQueryable so we preserve it
                //    return Expression.Call(
                //        QueryableMethods.AsQueryable.MakeGenericMethod(navigationTreeExpression.Type.GetSequenceType()),
                //        navigationTreeExpression);
                //}

                return UnwrapCollectionMaterialization(firstArgument);
            }

            if (firstArgument.Type.TryGetElementType(typeof(IQueryable<>)) == null)
            {
                // firstArgument was not an queryable
                var visitedArguments = new[] { firstArgument }
                    .Concat(methodCallExpression.Arguments.Skip(1).Select(e => Visit(e)));

                return ConvertToEnumerable(method, visitedArguments);
            }

            throw new InvalidOperationException(CoreStrings.TranslationFailed(methodCallExpression.Print()));
        }

        // Remove MaterializeCollectionNavigationExpression when applying ToList/ToArray
        //if (method.IsGenericMethod
        //    && (method.GetGenericMethodDefinition() == EnumerableMethods.ToList
        //        || method.GetGenericMethodDefinition() == EnumerableMethods.ToArray))
        //{
        //    return methodCallExpression.Update(
        //        null, new[] { UnwrapCollectionMaterialization(Visit(methodCallExpression.Arguments[0])) });
        //}

        return ProcessUnknownMethod(methodCallExpression);
    }

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //protected override Expression VisitUnary(UnaryExpression unaryExpression)
    //{
    //    var operand = Visit(unaryExpression.Operand);
    //    // Convert Array.Length to Count()
    //    if (unaryExpression.Operand.Type.IsArray
    //        && unaryExpression.NodeType == ExpressionType.ArrayLength)
    //    {
    //        var innerQueryable = UnwrapCollectionMaterialization(operand);
    //        // Only if inner is queryable as array properties could also have Length access
    //        if (innerQueryable.Type.TryGetElementType(typeof(IQueryable<>)) is Type elementType)
    //        {
    //            return Visit(Expression.Call(QueryableMethods.CountWithoutPredicate.MakeGenericMethod(elementType), innerQueryable));
    //        }
    //    }

    //    return unaryExpression.Update(operand);
    //}

    private Expression ProcessAllAnyCountLongCount(
        NavigationExpansionExpression source,
        MethodInfo genericMethod,
        LambdaExpression? predicate)
    {
        if (predicate != null)
        {
            predicate = ProcessLambdaExpression(source, predicate);

            return Expression.Call(
                genericMethod.MakeGenericMethod(source.ElementType), source.Source, Expression.Quote(predicate));
        }

        return Expression.Call(genericMethod.MakeGenericMethod(source.ElementType), source.Source);
    }

    private Expression ProcessAverageMaxMinSum(NavigationExpansionExpression source, MethodInfo method, LambdaExpression? selector)
    {
        if (selector != null)
        {
            selector = ProcessLambdaExpression(source, selector);

            method = method.GetGenericArguments().Length == 2
                ? method.MakeGenericMethod(source.ElementType, selector.ReturnType)
                : method.MakeGenericMethod(source.ElementType);

            return Expression.Call(method, source.Source, selector);
        }

        var queryable = Reduce(source);
        if (method.GetGenericArguments().Length == 1)
        {
            // Min/Max without selector has 1 generic parameters
            method = method.MakeGenericMethod(queryable.Type.GetSequenceType());
        }

        return Expression.Call(method, queryable);
    }

    private NavigationExpansionExpression ProcessCastOfType(
        NavigationExpansionExpression source, MethodInfo genericMethod, Type castType)
    {
        if (castType.IsAssignableFrom(source.SelectorStructure.Type)
            || castType == typeof(object))
        {
            // Casting to base/implementing interface is redundant
            return source;
        }

        var queryable = Reduce(source);
        var result = Expression.Call(genericMethod.MakeGenericMethod(castType), queryable);
        var newTree = new LeafNode(castType);
        var element = new Element(newTree);
        Expression selectorStructure;

        if (source.SelectorStructure is EntityExpression entityExpression
            && entityExpression.EntityType.GetAllBaseTypes().Concat(entityExpression.EntityType.GetDerivedTypesInclusive())
                .FirstOrDefault(et => et.ClrType == castType) is IEntityType castEntityType)
        {
            // We stripped off all expanded navigations but this preserves the include nodes and other info
            var newEntityExpression = new EntityExpression(element, castEntityType);
            //if (entityExpression.IsOptional)
            //{
            //    newEntityReference.MarkAsOptional();
            //}

            //newEntityReference.IncludePaths.Merge(entityExpression.IncludePaths);

            //// Prune includes for sibling types
            //var siblingNavigations = newEntityReference.IncludePaths.Keys
            //    .Where(
            //        n => !castEntityType.IsAssignableFrom(n.DeclaringEntityType)
            //            && !n.DeclaringEntityType.IsAssignableFrom(castEntityType));

            //foreach (var navigation in siblingNavigations)
            //{
            //    newEntityReference.IncludePaths.Remove(navigation);
            //}
            selectorStructure = newEntityExpression;
        }
        else
        {
            selectorStructure = element;
        }

        source = source.UpdateSelector(result, newTree, selectorStructure);

        ((INode)newTree).AttachOwner(source);

        return source;
    }

    private Expression ProcessContains(NavigationExpansionExpression source, Expression item)
    {
        var queryable = Reduce(source);

        return Expression.Call(QueryableMethods.Contains.MakeGenericMethod(queryable.Type.GetSequenceType()), queryable, item);
    }

    private NavigationExpansionExpression ProcessDefaultIfEmpty(NavigationExpansionExpression source)
    {
        source.UpdateSource(
            Expression.Call(
                QueryableMethods.DefaultIfEmptyWithoutArgument.MakeGenericMethod(source.ElementType),
                source.Source));

        if (source.CurrentTree is LeafNode
            && !source.CurrentTree.Type.IsNullableType())
        {
            // If the inner type is non-nullable type then it wouldn't have expanded navigations so we just need to look at the current tree.
            var parameter = Expression.Parameter(source.SelectorStructure.Type);
            var selector = Expression.Lambda(
                Expression.Coalesce(
                    Expression.Convert(parameter, parameter.Type.MakeNullable()),
                    parameter.Type.GetDefaultValueConstant()),
                parameter);

            return ProcessSelect(source, selector);
        }
        else
        {
            //_entityReferenceOptionalMarkingExpressionVisitor.Visit(pendingSelector);
        }

        return source;
    }

    private NavigationExpansionExpression ProcessDistinct(NavigationExpansionExpression source, MethodInfo genericMethod)
    {
        var queryable = Reduce(source);
        var elementType = queryable.Type.GetSequenceType();
        var result = Expression.Call(genericMethod.MakeGenericMethod(elementType), queryable);

        var newTree = new LeafNode(elementType);
        var element = new Element(newTree);
        var selectorStructure = RegenerateSelectorStructure(source.SelectorStructure, element);

        source = source.UpdateSelector(result, newTree, selectorStructure);

        ((INode)newTree).AttachOwner(source);

        return source;
    }

    private Expression ProcessFirstSingleLastOrDefault(
        NavigationExpansionExpression source, MethodInfo genericMethod, LambdaExpression? predicate, Type returnType)
    {
        if (predicate != null)
        {
            source = ProcessWhere(source, predicate);
            genericMethod = PredicateLessMethodInfo[genericMethod];
        }

        if (source.SelectorStructure.Type != returnType)
        {
            var parameter = Expression.Parameter(source.SelectorStructure.Type);
            source = ProcessSelect(source, Expression.Lambda(Expression.Convert(parameter, returnType), parameter));
        }

        var result = Reduce(source);
        var elementType = result.Type.GetSequenceType();

        var newTree = new LeafNode(elementType);
        var element = new Element(newTree);
        var selectorStructure = RegenerateSelectorStructure(source.SelectorStructure, element);

        source = source.UpdateSelector(result, newTree, selectorStructure);

        ((INode)newTree).AttachOwner(source);

        source.ConvertToSingleResult(genericMethod);

        return source;
    }

    private NavigationExpansionExpression ProcessInclude(
        NavigationExpansionExpression source,
        Expression expression,
        bool thenInclude,
        bool setLoaded)
    {
        if (UnwrapInnerEntityExpression(source.SelectorStructure) is EntityExpression entityExpression)
        {
            if (expression is ConstantExpression includeConstant
                && includeConstant.Value is string navigationChain)
            {
                var navigationPaths = navigationChain.Split(new[] { "." }, StringSplitOptions.None);
                var includeTreeNodes = new Queue<IncludeTreeNode>();
                includeTreeNodes.Enqueue(entityExpression.IncludePaths);
                foreach (var navigationName in navigationPaths)
                {
                    var nodesToProcess = includeTreeNodes.Count;
                    while (nodesToProcess-- > 0)
                    {
                        var currentNode = includeTreeNodes.Dequeue();
                        foreach (var navigation in FindNavigations(currentNode.EntityType, navigationName))
                        {
                            var addedNode = currentNode.AddNavigation(navigation, setLoaded);

                            // This is to add eager Loaded navigations when owner type is included.
                            //PopulateEagerLoadedNavigations(addedNode);
                            includeTreeNodes.Enqueue(addedNode);
                        }
                    }

                    if (includeTreeNodes.Count == 0)
                    {
                        _queryCompilationContext.Logger.InvalidIncludePathError(navigationChain, navigationName);
                    }
                }
            }
            else
            {
                //var currentIncludeTreeNode = thenInclude
                //    // LastIncludeTreeNode would be non-null for ThenInclude
                //    ? entityReference.LastIncludeTreeNode!
                //    : entityReference.IncludePaths;
                //var includeLambda = expression.UnwrapLambdaFromQuote();

                //var (result, filterExpression) = ExtractIncludeFilter(includeLambda.Body, includeLambda.Body);
                //var lastIncludeTree = PopulateIncludeTree(currentIncludeTreeNode, result, setLoaded);
                //if (filterExpression != null)
                //{
                //    if (lastIncludeTree.FilterExpression != null
                //        && !ExpressionEqualityComparer.Instance.Equals(filterExpression, lastIncludeTree.FilterExpression))
                //    {
                //        throw new InvalidOperationException(
                //            CoreStrings.MultipleFilteredIncludesOnSameNavigation(
                //                FormatFilter(filterExpression.Body).Print(),
                //                FormatFilter(lastIncludeTree.FilterExpression.Body).Print()));
                //    }

                //    lastIncludeTree.ApplyFilter(filterExpression);
                //}

                //entityReference.SetLastInclude(lastIncludeTree);
            }

            return source;
        }

        throw new InvalidOperationException(CoreStrings.IncludeOnNonEntity(expression.Print()));

        static (Expression result, LambdaExpression? filterExpression) ExtractIncludeFilter(
            Expression currentExpression,
            Expression includeExpression)
        {
            if (currentExpression is MemberExpression)
            {
                return (currentExpression, default);
            }

            if (currentExpression is MethodCallExpression methodCallExpression)
            {
                if (!methodCallExpression.Method.IsGenericMethod
                    || !SupportedFilteredIncludeOperations.Contains(methodCallExpression.Method.GetGenericMethodDefinition()))
                {
                    throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(includeExpression));
                }

                var (result, filterExpression) = ExtractIncludeFilter(methodCallExpression.Arguments[0], includeExpression);
                if (filterExpression == null)
                {
                    var prm = Expression.Parameter(result.Type);
                    filterExpression = Expression.Lambda(prm, prm);
                }

                var arguments = new List<Expression> { filterExpression.Body };
                arguments.AddRange(methodCallExpression.Arguments.Skip(1));
                filterExpression = Expression.Lambda(
                    methodCallExpression.Update(methodCallExpression.Object, arguments),
                    filterExpression.Parameters);

                return (result, filterExpression);
            }

            throw new InvalidOperationException(CoreStrings.InvalidIncludeExpression(includeExpression));
        }

        static Expression FormatFilter(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsGenericMethod
                && SupportedFilteredIncludeOperations.Contains(methodCallExpression.Method.GetGenericMethodDefinition()))
            {
                if (methodCallExpression.Method.GetGenericMethodDefinition() == QueryableMethods.AsQueryable)
                {
                    return Expression.Parameter(expression.Type, "navigation");
                }

                var arguments = new List<Expression>();
                var source = FormatFilter(methodCallExpression.Arguments[0]);
                arguments.Add(source);
                arguments.AddRange(methodCallExpression.Arguments.Skip(1));

                return methodCallExpression.Update(methodCallExpression.Object, arguments);
            }

            return expression;
        }
    }

    private NavigationExpansionExpression ProcessJoin(
        NavigationExpansionExpression outerSource,
        NavigationExpansionExpression innerSource,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        if (innerSource.PendingOrderings.Count > 0)
        {
            ApplyPendingOrderings(innerSource);
        }

        (outerKeySelector, innerKeySelector) = ProcessJoinConditions(outerSource, innerSource, outerKeySelector, innerKeySelector);
        var resultSelectorBody = new ReplacingExpressionVisitor(
                new Expression[] { resultSelector.Parameters[0], resultSelector.Parameters[1] },
                new[] { outerSource.SelectorStructure, innerSource.SelectorStructure })
            .Visit(resultSelector.Body);

        resultSelectorBody = new ExpandingExpressionVisitor(this, outerSource, _extensibilityHelper).Visit(resultSelectorBody);
        resultSelectorBody = new ExpandingExpressionVisitor(this, innerSource, _extensibilityHelper).Visit(resultSelectorBody);
        resultSelectorBody = Visit(resultSelectorBody);
        var parameterName = GetParameterName("ti");
        var newTree = new LeafNode(resultSelectorBody.Type);
        var element = new Element(newTree);
        var selectorStructure = SnapshotSelector(resultSelectorBody, newTree, element);

        var newSource = new NavigationExpansionExpression(
            Expression.Call(
                QueryableMethods.Join.MakeGenericMethod(
                    outerSource.ElementType, innerSource.ElementType, outerKeySelector.ReturnType, resultSelectorBody.Type),
                outerSource.Source,
                innerSource.Source,
                Expression.Quote(outerKeySelector),
                Expression.Quote(innerKeySelector),
                Expression.Quote(Expression.Lambda(Reduce(resultSelectorBody), outerSource.CurrentParameter, innerSource.CurrentParameter))),
            newTree,
            selectorStructure,
            parameterName);

        ((INode)newTree).AttachOwner(newSource);

        return newSource;
    }

    private NavigationExpansionExpression ProcessLeftJoin(
        NavigationExpansionExpression outerSource,
        NavigationExpansionExpression innerSource,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        if (innerSource.PendingOrderings.Any())
        {
            ApplyPendingOrderings(innerSource);
        }

        (outerKeySelector, innerKeySelector) = ProcessJoinConditions(outerSource, innerSource, outerKeySelector, innerKeySelector);
        var resultSelectorBody = new ReplacingExpressionVisitor(
            new Expression[] { resultSelector.Parameters[0], resultSelector.Parameters[1] },
            new[] { outerSource.SelectorStructure, innerSource.SelectorStructure })
                .Visit(resultSelector.Body);

        resultSelectorBody = ExpandNavigationsForSource(outerSource, resultSelectorBody);
        resultSelectorBody = ExpandNavigationsForSource(innerSource, resultSelectorBody);
        resultSelectorBody = Visit(resultSelectorBody);
        var parameterName = GetParameterName("ti");
        var newTree = new LeafNode(resultSelectorBody.Type);
        var element = new Element(newTree);
        var selectorStructure = SnapshotSelector(resultSelectorBody, newTree, element);

        var newSource = new NavigationExpansionExpression(
            Expression.Call(
                QueryableExtensions.LeftJoinMethodInfo.MakeGenericMethod(
                    outerSource.ElementType, innerSource.ElementType, outerKeySelector.ReturnType, resultSelectorBody.Type),
                outerSource.Source,
                innerSource.Source,
                Expression.Quote(outerKeySelector),
                Expression.Quote(innerKeySelector),
                Expression.Quote(Expression.Lambda(Reduce(resultSelectorBody), outerSource.CurrentParameter, innerSource.CurrentParameter))),
            newTree,
            selectorStructure,
            parameterName);

        ((INode)newTree).AttachOwner(newSource);

        return newSource;
    }

    private (LambdaExpression, LambdaExpression) ProcessJoinConditions(
        NavigationExpansionExpression outerSource,
        NavigationExpansionExpression innerSource,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector)
    {
        var outerKeyLambda = RemapLambdaExpression(outerSource, outerKeySelector);
        var innerKeyLambda = RemapLambdaExpression(innerSource, innerKeySelector);

        var keyComparison = (BinaryExpression)_removeRedundantNavigationComparisonExpressionVisitor
            .Visit(Expression.Equal(outerKeyLambda, innerKeyLambda));

        outerKeySelector = GenerateLambda(ExpandNavigationsForSource(outerSource, keyComparison.Left), outerSource.CurrentParameter);
        innerKeySelector = GenerateLambda(ExpandNavigationsForSource(innerSource, keyComparison.Right), innerSource.CurrentParameter);

        if (outerKeySelector.ReturnType != innerKeySelector.ReturnType)
        {
            var baseType = outerKeySelector.ReturnType.IsAssignableFrom(innerKeySelector.ReturnType)
                ? outerKeySelector.ReturnType
                : innerKeySelector.ReturnType;

            outerKeySelector = ChangeReturnType(outerKeySelector, baseType);
            innerKeySelector = ChangeReturnType(innerKeySelector, baseType);
        }

        return (outerKeySelector, innerKeySelector);

        static LambdaExpression ChangeReturnType(LambdaExpression lambdaExpression, Type type)
        {
            var delegateType = typeof(Func<,>).MakeGenericType(lambdaExpression.Parameters[0].Type, type);
            return Expression.Lambda(delegateType, lambdaExpression.Body, lambdaExpression.Parameters);
        }
    }

    private NavigationExpansionExpression ProcessOrderByThenBy(
        NavigationExpansionExpression source,
        MethodInfo genericMethod,
        LambdaExpression keySelector,
        bool thenBy)
    {
        // TODO: Align processing with expanding navigations for source
        var lambdaBody = RemapLambdaExpression(source, keySelector);
        lambdaBody = ExpandNavigationsForSource(source, lambdaBody);

        if (thenBy)
        {
            source.AppendPendingOrdering(genericMethod, lambdaBody);
        }
        else
        {
            source.AddPendingOrdering(genericMethod, lambdaBody);
        }

        return source;
    }

    private static Expression ProcessReverse(NavigationExpansionExpression source)
    {
        source.UpdateSource(
            Expression.Call(
                QueryableMethods.Reverse.MakeGenericMethod(source.ElementType),
                source.Source));

        return source;
    }

    private NavigationExpansionExpression ProcessSelect(NavigationExpansionExpression source, LambdaExpression selector)
    {
        if (selector.Body == selector.Parameters[0])
        {
            return source;
        }

        var selectorBody = RemapLambdaExpression(source, selector);

        selectorBody = new ExpandingExpressionVisitor(this, source, _extensibilityHelper).Visit(selectorBody);
        selectorBody = Visit(selectorBody);

        var newTree = new LeafNode(selectorBody.Type);
        var element = new Element(newTree);
        var selectorStructure = SnapshotSelector(selectorBody, newTree, element);

        source = source.UpdateSelector(
            Expression.Call(
                QueryableMethods.Select.MakeGenericMethod(source.ElementType, selectorBody.Type),
                source.Source,
                Expression.Quote(GenerateLambda(selectorBody, source.CurrentParameter))),
            newTree,
            selectorStructure);

        ((INode)newTree).AttachOwner(source);

        return source;
    }

    private NavigationExpansionExpression ProcessSelectMany(
        NavigationExpansionExpression source,
        LambdaExpression collectionSelector,
        LambdaExpression? resultSelector)
    {
        var collectionSelectorBody = ExpandNavigationsForSource(source, RemapLambdaExpression(source, collectionSelector));
        collectionSelectorBody = UnwrapCollectionMaterialization(collectionSelectorBody);

        if (collectionSelectorBody is NavigationExpansionExpression collectionSource)
        {
            if (collectionSource.PendingOrderings.Count > 0)
            {
                ApplyPendingOrderings(collectionSource);
            }

            collectionSelector = GenerateLambda(collectionSource, source.CurrentParameter);
            var collectionElementType = collectionSelector.ReturnType.GetSequenceType();

            // Collection selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
            // therefore the delegate type is specified explicitly
            var collectionSelectorLambdaType = typeof(Func<,>).MakeGenericType(
                source.ElementType,
                typeof(IEnumerable<>).MakeGenericType(collectionElementType));

            collectionSelector = Expression.Lambda(collectionSelectorLambdaType, collectionSelector.Body, collectionSelector.Parameters[0]);
            var parameterName = GetParameterName("ti");
            NavigationExpansionExpression newSource;
            LeafNode newTree;

            if (resultSelector == null)
            {
                newTree = new LeafNode(collectionSource.SelectorStructure.Type);
                var element = new Element(newTree);
                var selectorStructure = SnapshotSelector(collectionSource.SelectorStructure, newTree, element);
                newSource = new NavigationExpansionExpression(
                    Expression.Call(
                        QueryableMethods.SelectManyWithoutCollectionSelector.MakeGenericMethod(source.ElementType, collectionElementType),
                        source.Source,
                        Expression.Quote(collectionSelector)),
                    newTree,
                    selectorStructure,
                    parameterName);
            }
            else
            {
                var resultSelectorBody = new ReplacingExpressionVisitor(
                    new Expression[] { resultSelector.Parameters[0], resultSelector.Parameters[1] },
                    new[] { source.SelectorStructure, collectionSource.SelectorStructure })
                        .Visit(resultSelector.Body);

                resultSelectorBody = ExpandNavigationsForSource(source, resultSelectorBody);
                resultSelectorBody = ExpandNavigationsForSource(collectionSource, resultSelectorBody);
                resultSelectorBody = Visit(resultSelectorBody);
                newTree = new LeafNode(resultSelectorBody.Type);
                var element = new Element(newTree);
                var selectorStructure = SnapshotSelector(resultSelectorBody, newTree, element);

                newSource = new NavigationExpansionExpression(
                    Expression.Call(
                        QueryableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                            source.ElementType, collectionElementType, resultSelectorBody.Type),
                        source.Source,
                        Expression.Quote(collectionSelector),
                        Expression.Quote(
                            Expression.Lambda(Reduce(resultSelectorBody), source.CurrentParameter, collectionSource.CurrentParameter))),
                    newTree,
                    selectorStructure,
                    parameterName);
            }

            ((INode)newTree).AttachOwner(newSource);

            return newSource;
        }

        // TODO: Improve this exception message
        throw new InvalidOperationException(CoreStrings.TranslationFailed(collectionSelector.Print()));
    }

    private NavigationExpansionExpression ProcessSetOperation(
        NavigationExpansionExpression outerSource,
        MethodInfo genericMethod,
        NavigationExpansionExpression innerSource)
    {
        ValidateExpressionCompatibility(outerSource.SelectorStructure, innerSource.SelectorStructure);

        var outerQueryable = Reduce(outerSource);
        var innerQueryable = Reduce(innerSource);

        var outerType = outerQueryable.Type.GetSequenceType();
        var innerType = innerQueryable.Type.GetSequenceType();
        var resultType = outerType.IsAssignableFrom(innerType) ? outerType : innerType;
        var resultSelector = outerType.IsAssignableFrom(innerType) ? outerSource.SelectorStructure : innerSource.SelectorStructure;

        var newTree = new LeafNode(resultSelector.Type);
        var element = new Element(newTree);
        var selectorStructure = SnapshotSelector(resultSelector, newTree, element);

        outerSource = outerSource.UpdateSelector(
            Expression.Call(genericMethod.MakeGenericMethod(resultType), outerQueryable, innerQueryable),
            newTree,
            selectorStructure);

        ((INode)newTree).AttachOwner(outerSource);

        return outerSource;
    }

    private static NavigationExpansionExpression ProcessSkipTake(
        NavigationExpansionExpression source,
        MethodInfo genericMethod,
        Expression count)
    {
        source.UpdateSource(Expression.Call(genericMethod.MakeGenericMethod(source.ElementType), source.Source, count));

        return source;
    }

    private Expression ProcessUnknownMethod(MethodCallExpression methodCallExpression)
    {
        var queryableElementType = methodCallExpression.Type.TryGetElementType(typeof(IQueryable<>));
        if (queryableElementType != null
            && methodCallExpression.Object == null
            && methodCallExpression.Arguments.All(a => a.GetLambdaOrNull() == null)
            && methodCallExpression.Method.IsGenericMethod
            && methodCallExpression.Method.GetGenericArguments().Length == 1
            && methodCallExpression.Method.GetGenericArguments()[0] == queryableElementType
            && methodCallExpression.Arguments.Count > 0
            && methodCallExpression.Arguments.Skip(1).All(e => e.Type.TryGetElementType(typeof(IQueryable<>)) == null))
        {
            throw new InvalidFilterCriteriaException();
            //var firstArgument = Visit(methodCallExpression.Arguments[0]);
            //if (firstArgument is NavigationExpansionExpression1 source
            //    && source.Type == methodCallExpression.Type)
            //{
            //    source = (NavigationExpansionExpression1)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            //    var newStructure = SnapshotExpression(source.PendingSelector);
            //    var queryable = Reduce(source);

            //    var result = Expression.Call(
            //        methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(queryableElementType),
            //        new[] { queryable }.Concat(methodCallExpression.Arguments.Skip(1).Select(e => Visit(e))));

            //    var navigationTree = new NavigationTreeExpression(newStructure);
            //    var parameterName = GetParameterName("e");

            //    return new NavigationExpansionExpression1(result, navigationTree, navigationTree, parameterName);
            //}
        }

        return base.VisitMethodCall(methodCallExpression);
    }

    private NavigationExpansionExpression ProcessWhere(NavigationExpansionExpression source, LambdaExpression predicate)
    {
        predicate = ProcessLambdaExpression(source, predicate);

        source.UpdateSource(
            Expression.Call(
                QueryableMethods.Where.MakeGenericMethod(source.ElementType),
                source.Source,
                Expression.Quote(predicate)));

        return source;
    }

    private static Expression RegenerateSelectorStructure(Expression expression, Element element)
    {
        switch (expression)
        {
            case NewExpression newExpression:
                var newArguments = new Expression[newExpression.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    newArguments[i] = newExpression.Members == null
                        ? Expression.Default(newExpression.Arguments[i].Type)
                        : RegenerateSelectorStructure(newExpression.Arguments[i], element.AddMember(newExpression.Members[i]));
                }

                return newExpression.Update(newArguments);

            case MemberInitExpression memberInitExpression
                when memberInitExpression.Bindings.Any(e => e.BindingType == MemberBindingType.Assignment):
                var newNewExpression = RegenerateSelectorStructure(memberInitExpression.NewExpression, element);
                var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
                for (var i = 0; i < newBindings.Length; i++)
                {
                    var binding = (MemberAssignment)memberInitExpression.Bindings[i];
                    var newElement = element.AddMember(binding.Member);
                    newBindings[i] = binding.Update(RegenerateSelectorStructure(binding.Expression, newElement));
                }

                return memberInitExpression.Update((NewExpression)newNewExpression, newBindings);

            case EntityExpression entityExpression:
                return entityExpression.Clone(element);

            case Element _:
                return element;

            default:
                throw new InvalidFilterCriteriaException();
        }
    }

    private Expression SnapshotSelector(
        Expression currentExpression,
        //Expression selectorBody,
        INode tree,
        Element element//,
                       //out Expression updatedSelector,
                       //out INode updatedTree
        )
    {
        switch (currentExpression)
        {
            case NewExpression newExpression:
                var newArguments = new Expression[newExpression.Arguments.Count];
                for (var i = 0; i < newArguments.Length; i++)
                {
                    newArguments[i] = newExpression.Members == null
                        ? Expression.Default(newExpression.Arguments[i].Type)
                        : SnapshotSelector(newExpression.Arguments[i], tree, element.AddMember(newExpression.Members[i]));
                }

                return newExpression.Update(newArguments);

            case MemberInitExpression memberInitExpression
            when memberInitExpression.Bindings.Any(e => e.BindingType == MemberBindingType.Assignment):
                var newNewExpression = SnapshotSelector(memberInitExpression.NewExpression, tree, element);
                var newBindings = new MemberBinding[memberInitExpression.Bindings.Count];
                for (var i = 0; i < newBindings.Length; i++)
                {
                    var binding = (MemberAssignment)memberInitExpression.Bindings[i];
                    var newElement = element.AddMember(binding.Member);
                    newBindings[i] = binding.Update(SnapshotSelector(binding.Expression, tree, newElement));
                }

                return memberInitExpression.Update((NewExpression)newNewExpression, newBindings);

            case EntityExpression entityExpression:
                return entityExpression.Clone(element);

            case NavigationExpansionExpression navigationExpansionExpression
            when navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null
            && navigationExpansionExpression.SelectorStructure is EntityExpression innerEntityExpression:
                return innerEntityExpression.Clone(element);

            default:
                return element;
        }
    }

    private void ValidateExpressionCompatibility(Expression outer, Expression inner)
    {
        //if (outer is EntityExpression outerEntityExpression
        //    && inner is EntityExpression innerEntityExpression)
        //{
        //    if (!outerEntityExpression.IncludePaths.Equals(innerEntityExpression.IncludePaths))
        //    {
        //        throw new InvalidOperationException(CoreStrings.SetOperationWithDifferentIncludesInOperands);
        //    }

        //    if (!_extensibilityHelper.AreQueryRootsCompatible(
        //            outerEntityExpression.QueryRootExpression, innerEntityExpression.QueryRootExpression))
        //    {
        //        throw new InvalidOperationException(CoreStrings.IncompatibleSourcesForSetOperation);
        //    }
        //}

        //if (outer is NewExpression outerNewExpression
        //    && inner is NewExpression innerNewExpression)
        //{
        //    if (outerNewExpression.Arguments.Count != innerNewExpression.Arguments.Count)
        //    {
        //        throw new InvalidOperationException(CoreStrings.SetOperationWithDifferentIncludesInOperands);
        //    }

        //    for (var i = 0; i < outerNewExpression.Arguments.Count; i++)
        //    {
        //        ValidateExpressionCompatibility(outerNewExpression.Arguments[i], innerNewExpression.Arguments[i]);
        //    }
        //}

        //if (outer is DefaultExpression outerDefaultExpression
        //    && inner is DefaultExpression innerDefaultExpression
        //    && outerDefaultExpression.Type != innerDefaultExpression.Type)
        //{
        //    throw new InvalidOperationException(CoreStrings.SetOperationWithDifferentIncludesInOperands);
        //}
    }

    private void ApplyPendingOrderings(NavigationExpansionExpression source)
    {
        Check.DebugAssert(source.PendingOrderings.Count > 0, "There was no pending ordering.");
        foreach (var (orderingMethod, keySelector) in source.PendingOrderings)
        {
            if (UnwrapInnerEntityExpression(keySelector) is EntityExpression entityExpression)
            {
                var primaryKeyProperties = entityExpression.EntityType.FindPrimaryKey()?.Properties;
                if (primaryKeyProperties != null)
                {
                    for (var i = 0; i < primaryKeyProperties.Count; i++)
                    {
                        var genericMethod = i > 0
                            ? GetThenByMethod(orderingMethod)
                            : orderingMethod;

                        var keyPropertyLambda = GenerateLambda(
                            keySelector.CreateEFPropertyExpression(
                                primaryKeyProperties[i], false/*entityReference.IsOptional*/),
                            source.CurrentParameter);

                        source.UpdateSource(
                            Expression.Call(
                                genericMethod.MakeGenericMethod(source.ElementType, keyPropertyLambda.ReturnType),
                                source.Source,
                                keyPropertyLambda));
                    }

                    continue;
                }
            }

            var keySelectorLambda = GenerateLambda(keySelector, source.CurrentParameter);

            source.UpdateSource(
                Expression.Call(
                    orderingMethod.MakeGenericMethod(source.ElementType, keySelectorLambda.ReturnType),
                    source.Source,
                    keySelectorLambda));
        }

        source.ClearPendingOrderings();

        static MethodInfo GetThenByMethod(MethodInfo currentGenericMethod)
            => currentGenericMethod == QueryableMethods.OrderBy
                ? QueryableMethods.ThenBy
                : currentGenericMethod == QueryableMethods.OrderByDescending
                    ? QueryableMethods.ThenByDescending
                    : currentGenericMethod;
    }

    private static Expression RemapLambdaExpression(NavigationExpansionExpression source, LambdaExpression lambdaExpression)
        => ReplacingExpressionVisitor.Replace(lambdaExpression.Parameters[0], source.SelectorStructure, lambdaExpression.Body);

    private LambdaExpression ProcessLambdaExpression(NavigationExpansionExpression source, LambdaExpression lambdaExpression)
        => GenerateLambda(ExpandNavigationsForSource(source, RemapLambdaExpression(source, lambdaExpression)), source.CurrentParameter);

    private Expression ExpandNavigationsForSource(NavigationExpansionExpression source, Expression expression)
    {
        expression = _removeRedundantNavigationComparisonExpressionVisitor.Visit(expression);
        expression = new ExpandingExpressionVisitor(this, source, _extensibilityHelper).Visit(expression);
        //expression = _subqueryMemberPushdownExpressionVisitor.Visit(expression);
        expression = Visit(expression);
        //expression = _pendingSelectorExpandingExpressionVisitor.Visit(expression);

        return expression;
    }

    private LambdaExpression GenerateLambda(Expression body, ParameterExpression currentParameter)
        => Expression.Lambda(Reduce(body), currentParameter);

    private Expression UnwrapCollectionMaterialization(Expression expression)
    {
        if (expression is MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression)
        {
            expression = materializeCollectionNavigationExpression.Subquery;
        }

        return expression;
    }

    private static MethodCallExpression ConvertToEnumerable(MethodInfo queryableMethod, IEnumerable<Expression> arguments)
    {
        var genericTypeArguments = queryableMethod.IsGenericMethod
            ? queryableMethod.GetGenericArguments()
            : Array.Empty<Type>();

        var enumerableArguments = arguments.Select(
            arg => arg is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Quote
                && unaryExpression.Operand is LambdaExpression
                    ? unaryExpression.Operand
                    : arg).ToList();

        if (queryableMethod.Name == nameof(Enumerable.Min))
        {
            if (genericTypeArguments.Length == 1)
            {
                var resultType = genericTypeArguments[0];
                var enumerableMethod = EnumerableMethods.GetMinWithoutSelector(resultType);

                if (!IsNumericType(resultType))
                {
                    enumerableMethod = enumerableMethod.MakeGenericMethod(resultType);
                }

                return Expression.Call(enumerableMethod, enumerableArguments);
            }

            if (genericTypeArguments.Length == 2)
            {
                var resultType = genericTypeArguments[1];
                var enumerableMethod = EnumerableMethods.GetMinWithSelector(resultType);

                enumerableMethod = IsNumericType(resultType)
                    ? enumerableMethod.MakeGenericMethod(genericTypeArguments[0])
                    : enumerableMethod.MakeGenericMethod(genericTypeArguments);

                return Expression.Call(enumerableMethod, enumerableArguments);
            }
        }

        if (queryableMethod.Name == nameof(Enumerable.Max))
        {
            if (genericTypeArguments.Length == 1)
            {
                var resultType = genericTypeArguments[0];
                var enumerableMethod = EnumerableMethods.GetMaxWithoutSelector(resultType);

                if (!IsNumericType(resultType))
                {
                    enumerableMethod = enumerableMethod.MakeGenericMethod(resultType);
                }

                return Expression.Call(enumerableMethod, enumerableArguments);
            }

            if (genericTypeArguments.Length == 2)
            {
                var resultType = genericTypeArguments[1];
                var enumerableMethod = EnumerableMethods.GetMaxWithSelector(resultType);

                enumerableMethod = IsNumericType(resultType)
                    ? enumerableMethod.MakeGenericMethod(genericTypeArguments[0])
                    : enumerableMethod.MakeGenericMethod(genericTypeArguments);

                return Expression.Call(enumerableMethod, enumerableArguments);
            }
        }

        foreach (var method in typeof(Enumerable).GetTypeInfo().GetDeclaredMethods(queryableMethod.Name))
        {
            var enumerableMethod = method;
            if (enumerableMethod.IsGenericMethod)
            {
                if (genericTypeArguments != null
                    && enumerableMethod.GetGenericArguments().Length == genericTypeArguments.Length)
                {
                    enumerableMethod = enumerableMethod.MakeGenericMethod(genericTypeArguments);
                }
                else
                {
                    continue;
                }
            }

            var enumerableMethodParameters = enumerableMethod.GetParameters();
            if (enumerableMethodParameters.Length != enumerableArguments.Count)
            {
                continue;
            }

            var validMapping = true;
            for (var i = 0; i < enumerableMethodParameters.Length; i++)
            {
                if (!enumerableMethodParameters[i].ParameterType.IsAssignableFrom(enumerableArguments[i].Type))
                {
                    validMapping = false;
                    break;
                }
            }

            if (validMapping)
            {
                return Expression.Call(enumerableMethod, enumerableArguments);
            }
        }

        throw new InvalidOperationException(CoreStrings.CannotConvertQueryableToEnumerableMethod);

        static bool IsNumericType(Type type)
        {
            type = type.UnwrapNullableType();

            return type == typeof(int)
                || type == typeof(long)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal);
        }
    }
    private string GetParameterName(string prefix)
    {
        var uniqueName = prefix;
        var index = 0;
        while (_parameterNames.Contains(uniqueName))
        {
            uniqueName = $"{prefix}{index++}";
        }

        _parameterNames.Add(uniqueName);
        return uniqueName;
    }

    private Expression Reduce(Expression source)
        => _reducingExpressionVisitor.Visit(source);

    private static Expression UnwrapInnerEntityExpression(Expression expression)
    {
        return expression is NavigationExpansionExpression navigationExpansionExpression
            && navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null
            ? navigationExpansionExpression.SelectorStructure
            : expression;
    }

    private static IEnumerable<INavigationBase> FindNavigations(IEntityType entityType, string navigationName)
    {
        var navigation = entityType.FindNavigation(navigationName);
        if (navigation != null)
        {
            yield return navigation;
        }
        else
        {
            foreach (var derivedNavigation in entityType.GetDerivedTypes()
                         .Select(et => et.FindDeclaredNavigation(navigationName)))
            {
                if (derivedNavigation != null)
                {
                    yield return derivedNavigation;
                }
            }
        }

        var skipNavigation = entityType.FindSkipNavigation(navigationName);
        if (skipNavigation != null)
        {
            yield return skipNavigation;
        }
        else
        {
            foreach (var derivedSkipNavigation in entityType.GetDerivedTypes()
                         .Select(et => et.FindDeclaredSkipNavigation(navigationName)))
            {
                if (derivedSkipNavigation != null)
                {
                    yield return derivedSkipNavigation;
                }
            }
        }
    }

}
