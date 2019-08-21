// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class NavigationExpandingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly PendingSelectorExpandingExpressionVisitor _pendingSelectorExpandingExpressionVisitor;
        private readonly SubqueryMemberPushdownExpressionVisitor _subqueryMemberPushdownExpressionVisitor;
        private readonly ReducingExpressionVisitor _reducingExpressionVisitor;
        private readonly EntityReferenceOptionalMarkingExpressionVisitor _entityReferenceOptionalMarkingExpressionVisitor;
        private readonly ISet<string> _parameterNames = new HashSet<string>();
        private readonly EnumerableToQueryableMethodConvertingExpressionVisitor _enumerableToQueryableMethodConvertingExpressionVisitor;
        private readonly ParameterExtractingExpressionVisitor _parameterExtractingExpressionVisitor;
        private readonly Dictionary<IEntityType, LambdaExpression> _parameterizedQueryFilterPredicateCache
            = new Dictionary<IEntityType, LambdaExpression>();
        private readonly Parameters _parameters = new Parameters();

        private static readonly MethodInfo _enumerableToListMethodInfo = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethods(nameof(Enumerable.ToList))
            .Single(mi => mi.GetParameters().Length == 1);

        public NavigationExpandingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            IEvaluatableExpressionFilter evaluatableExpressionFilter)
        {
            _queryCompilationContext = queryCompilationContext;
            _pendingSelectorExpandingExpressionVisitor = new PendingSelectorExpandingExpressionVisitor(this);
            _subqueryMemberPushdownExpressionVisitor = new SubqueryMemberPushdownExpressionVisitor();
            _reducingExpressionVisitor = new ReducingExpressionVisitor();
            _entityReferenceOptionalMarkingExpressionVisitor = new EntityReferenceOptionalMarkingExpressionVisitor();
            _enumerableToQueryableMethodConvertingExpressionVisitor = new EnumerableToQueryableMethodConvertingExpressionVisitor();
            _parameterExtractingExpressionVisitor = new ParameterExtractingExpressionVisitor(
                evaluatableExpressionFilter,
                _parameters,
                _queryCompilationContext.ContextType,
                _queryCompilationContext.Model,
                _queryCompilationContext.Logger,
                parameterize: false,
                generateContextAccessors: true);
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

        public virtual Expression Expand(Expression query)
        {
            var result = ExpandAndReduce(query, applyInclude: true);

            var dbContextOnQueryContextPropertyAccess =
                Expression.Convert(
                    Expression.Property(
                        QueryCompilationContext.QueryContextParameter,
                        _queryContextContextPropertyInfo),
                    _queryCompilationContext.ContextType);

            foreach (var parameterValue in _parameters.ParameterValues)
            {
                var lambda = (LambdaExpression)parameterValue.Value;
                var remappedLambdaBody = ReplacingExpressionVisitor.Replace(
                    lambda.Parameters[0],
                    dbContextOnQueryContextPropertyAccess,
                    lambda.Body);

                _queryCompilationContext.RegisterRuntimeParameter(
                    parameterValue.Key,
                    Expression.Lambda(
                        remappedLambdaBody.Type.IsValueType
                            ? Expression.Convert(remappedLambdaBody, typeof(object))
                            : remappedLambdaBody,
                        QueryCompilationContext.QueryContextParameter));
            }

            return result;
        }

        private Expression ExpandAndReduce(Expression query, bool applyInclude)
        {
            var result = Visit(query);
            result = _pendingSelectorExpandingExpressionVisitor.Visit(result);
            if (applyInclude)
            {
                result = new IncludeApplyingExpressionVisitor(this, _queryCompilationContext.IsTracking).Visit(result);
            }

            result = Reduce(result);

            return result;
        }

        private static readonly PropertyInfo _queryContextContextPropertyInfo
           = typeof(QueryContext)
               .GetTypeInfo()
               .GetDeclaredProperty(nameof(QueryContext.Context));

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.IsEntityQueryable())
            {
                var entityType = _queryCompilationContext.Model.FindEntityType(((IQueryable)constantExpression.Value).ElementType);
                var definingQuery = entityType.GetDefiningQuery();
                NavigationExpansionExpression navigationExpansionExpression;
                if (definingQuery != null)
                {
                    var processedDefiningQueryBody = _parameterExtractingExpressionVisitor.ExtractParameters(definingQuery.Body);
                    processedDefiningQueryBody = _enumerableToQueryableMethodConvertingExpressionVisitor.Visit(processedDefiningQueryBody);
                    processedDefiningQueryBody = new SelfReferenceEntityQueryableRewritingExpressionVisitor(this, entityType).Visit(processedDefiningQueryBody);

                    navigationExpansionExpression = (NavigationExpansionExpression)Visit(processedDefiningQueryBody);

                    var expanded = ExpandAndReduce(navigationExpansionExpression, applyInclude: false);
                    navigationExpansionExpression = CreateNavigationExpansionExpression(expanded, entityType);
                }
                else
                {
                    navigationExpansionExpression = CreateNavigationExpansionExpression(constantExpression, entityType);
                }

                return ApplyQueryFilter(navigationExpansionExpression);
            }

            return base.VisitConstant(constantExpression);
        }

        private NavigationExpansionExpression CreateNavigationExpansionExpression(Expression sourceExpression, IEntityType entityType)
        {
            var entityReference = new EntityReference(entityType);
            PopulateEagerLoadedNavigations(entityReference.IncludePaths);

            var currentTree = new NavigationTreeExpression(entityReference);
            var parameterName = GetParameterName(entityType.ShortName()[0].ToString().ToLower());

            return new NavigationExpansionExpression(sourceExpression, currentTree, currentTree, parameterName);
        }

        private Expression ApplyQueryFilter(NavigationExpansionExpression navigationExpansionExpression)
        {
            if (!_queryCompilationContext.IgnoreQueryFilters)
            {
                var entityType = _queryCompilationContext.Model.FindEntityType(navigationExpansionExpression.Type.GetSequenceType());
                var rootEntityType = entityType.GetRootType();
                var queryFilter = rootEntityType.GetQueryFilter();
                if (queryFilter != null)
                {
                    if (!_parameterizedQueryFilterPredicateCache.TryGetValue(rootEntityType, out var filterPredicate))
                    {
                        filterPredicate = queryFilter;
                        filterPredicate = (LambdaExpression)_parameterExtractingExpressionVisitor.ExtractParameters(filterPredicate);
                        filterPredicate = (LambdaExpression)_enumerableToQueryableMethodConvertingExpressionVisitor.Visit(filterPredicate);
                        _parameterizedQueryFilterPredicateCache[rootEntityType] = filterPredicate;
                    }

                    filterPredicate = (LambdaExpression)new SelfReferenceEntityQueryableRewritingExpressionVisitor(this, entityType).Visit(filterPredicate);
                    var sequenceType = navigationExpansionExpression.Type.GetSequenceType();

                    // if we are constructing EntityQueryable of a derived type, we need to re-map filter predicate to the correct derived type
                    var filterPredicateParameter = filterPredicate.Parameters[0];
                    if (filterPredicateParameter.Type != sequenceType)
                    {
                        var newFilterPredicateParameter = Expression.Parameter(sequenceType, filterPredicateParameter.Name);
                        var newFilterPredicateBody = ReplacingExpressionVisitor.Replace(filterPredicateParameter, newFilterPredicateParameter, filterPredicate.Body);
                        filterPredicate = Expression.Lambda(newFilterPredicateBody, newFilterPredicateParameter);
                    }

                    var filteredResult = Expression.Call(
                        QueryableMethods.Where.MakeGenericMethod(sequenceType),
                        navigationExpansionExpression,
                        filterPredicate);

                    return Visit(filteredResult);
                }
            }

            return navigationExpansionExpression;
        }

        private static void PopulateEagerLoadedNavigations(IncludeTreeNode includeTreeNode)
        {
            var entityType = includeTreeNode.EntityType;
            var outboundNavigations
                = entityType.GetNavigations()
                    .Concat(entityType.GetDerivedNavigations())
                    .Where(n => n.IsEagerLoaded());

            foreach (var navigation in outboundNavigations)
            {
                var addedIncludeTreeNode = includeTreeNode.AddNavigation(navigation);
                PopulateEagerLoadedNavigations(addedIncludeTreeNode);
            }
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            return extensionExpression is NavigationExpansionExpression
                || extensionExpression is OwnedNavigationReference
                ? extensionExpression
                : base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var innerExpression = Visit(memberExpression.Expression);

            // Convert CollectionNavigation.Count to subquery.Count()
            if (innerExpression is MaterializeCollectionNavigationExpression materializeCollectionNavigation
                && memberExpression.Member.Name == nameof(List<int>.Count))
            {
                var subquery = materializeCollectionNavigation.Subquery;
                var elementType = subquery.Type.TryGetSequenceType();
                if (subquery is OwnedNavigationReference ownedNavigationReference
                    && ownedNavigationReference.Navigation.IsCollection())
                {
                    subquery = Expression.Call(
                        QueryableMethods.AsQueryable.MakeGenericMethod(elementType),
                        subquery);
                }

                return Visit(Expression.Call(
                    QueryableMethods.CountWithoutPredicate.MakeGenericMethod(elementType),
                    subquery));
            }

            var updatedExpression = (Expression)memberExpression.Update(innerExpression);
            if (innerExpression is NavigationExpansionExpression navigationExpansionExpression
                && navigationExpansionExpression.CardinalityReducingGenericMethodInfo != null)
            {
                // This is FirstOrDefault.Member
                // due to SubqueryMemberPushdown, this may be collection navigation which was not pushed down
                var expandedExpression = new ExpandingExpressionVisitor(this, navigationExpansionExpression).Visit(updatedExpression);
                if (expandedExpression != updatedExpression)
                {
                    updatedExpression = Visit(expandedExpression);
                }
            }

            return updatedExpression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(Queryable)
                || method.DeclaringType == typeof(QueryableExtensions)
                || method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
            {
                var firstArgument = Visit(methodCallExpression.Arguments[0]);
                if (firstArgument is NavigationExpansionExpression source)
                {
                    var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;

                    if (source.PendingOrderings.Any()
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
                        case nameof(Queryable.Sum)
                        when QueryableMethods.IsSumWithoutSelector(method):
                        case nameof(Queryable.Max)
                        when genericMethod == QueryableMethods.MaxWithoutSelector:
                        case nameof(Queryable.Min)
                        when genericMethod == QueryableMethods.MinWithoutSelector:
                            return ProcessAverageMaxMinSum(
                                source,
                                genericMethod ?? method,
                                null);

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
                            return ProcessDistinctSkipTake(source, genericMethod, null);

                        case nameof(Queryable.Skip)
                        when genericMethod == QueryableMethods.Skip:
                        case nameof(Queryable.Take)
                        when genericMethod == QueryableMethods.Take:
                            return ProcessDistinctSkipTake(
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
                                null,
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
                            if (secondArgument is NavigationExpansionExpression innerSource)
                            {
                                return ProcessJoin(
                                    source,
                                    innerSource,
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                            }
                            break;
                        }

                        case nameof(QueryableExtensions.LeftJoin)
                        when genericMethod == QueryableExtensions.LeftJoinMethodInfo:
                        {
                            var secondArgument = Visit(methodCallExpression.Arguments[1]);
                            if (secondArgument is NavigationExpansionExpression innerSource)
                            {
                                return ProcessLeftJoin(
                                    source,
                                    innerSource,
                                    methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[3].UnwrapLambdaFromQuote(),
                                    methodCallExpression.Arguments[4].UnwrapLambdaFromQuote());
                            }
                            break;
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
                            if (secondArgument is NavigationExpansionExpression innerSource)
                            {
                                return ProcessSetOperation(
                                    source,
                                    genericMethod,
                                    innerSource);
                            }
                            break;
                        }

                        case nameof(Queryable.Cast)
                        when genericMethod == QueryableMethods.Cast:
                        case nameof(Queryable.OfType)
                        when genericMethod == QueryableMethods.OfType:
                            return ProcessCastOfType(
                                source,
                                genericMethod,
                                methodCallExpression.Type.TryGetSequenceType());

                        case nameof(EntityFrameworkQueryableExtensions.Include):
                        case nameof(EntityFrameworkQueryableExtensions.ThenInclude):
                            return ProcessInclude(
                                source,
                                methodCallExpression.Arguments[1],
                                string.Equals(method.Name,
                                  nameof(EntityFrameworkQueryableExtensions.ThenInclude)));

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeySelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                null,
                                null);

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyElementSelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                null);

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyElementResultSelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote(),
                                methodCallExpression.Arguments[3].UnwrapLambdaFromQuote());

                        case nameof(Queryable.GroupBy)
                        when genericMethod == QueryableMethods.GroupByWithKeyResultSelector:
                            return ProcessGroupBy(
                                source,
                                methodCallExpression.Arguments[1].UnwrapLambdaFromQuote(),
                                null,
                                methodCallExpression.Arguments[2].UnwrapLambdaFromQuote());

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
                            throw new InvalidOperationException(CoreStrings.QueryFailed(methodCallExpression.Print(), GetType().Name));
                    }
                }
                else if (firstArgument is MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression
                    && method.Name == nameof(Queryable.AsQueryable))
                {
                    var subquery = materializeCollectionNavigationExpression.Subquery;
                    return subquery is OwnedNavigationReference ownedNavigationReference
                        && ownedNavigationReference.Navigation.IsCollection()
                        ? Visit(Expression.Call(
                            QueryableMethods.AsQueryable.MakeGenericMethod(subquery.Type.TryGetSequenceType()),
                            subquery))
                        : subquery;
                }
                else if (firstArgument is OwnedNavigationReference ownedNavigationReference
                    && ownedNavigationReference.Navigation.IsCollection()
                    && method.Name == nameof(Queryable.AsQueryable))
                {
                    var parameterName = GetParameterName("o");
                    var entityReference = ownedNavigationReference.EntityReference;
                    var currentTree = new NavigationTreeExpression(entityReference);

                    return new NavigationExpansionExpression(methodCallExpression, currentTree, currentTree, parameterName);
                }

                throw new InvalidOperationException(CoreStrings.QueryFailed(methodCallExpression.Print(), GetType().Name));
            }

            if (method.IsGenericMethod
                && method.GetGenericMethodDefinition() == _enumerableToListMethodInfo)
            {
                var argument = Visit(methodCallExpression.Arguments[0]);
                if (argument is MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression)
                {
                    argument = materializeCollectionNavigationExpression.Subquery;
                }

                return methodCallExpression.Update(null, new[] { argument });
            }

            if (method.IsGenericMethod
                && method.Name == "FromSqlOnQueryable"
                && methodCallExpression.Arguments.Count == 3
                && methodCallExpression.Arguments[0] is ConstantExpression constantExpression
                && methodCallExpression.Arguments[1] is ConstantExpression
                && (methodCallExpression.Arguments[2] is ParameterExpression || methodCallExpression.Arguments[2] is ConstantExpression)
                && constantExpression.IsEntityQueryable())
            {
                var entityType = _queryCompilationContext.Model.FindEntityType(((IQueryable)constantExpression.Value).ElementType);
                var source = CreateNavigationExpansionExpression(constantExpression, entityType);
                source.UpdateSource(
                    methodCallExpression.Update(
                        null,
                        new[] { source.Source, methodCallExpression.Arguments[1], methodCallExpression.Arguments[2] }));

                return ApplyQueryFilter(source);
            }

            return ProcessUnknownMethod(methodCallExpression);
        }

        private Expression ProcessDefaultIfEmpty(NavigationExpansionExpression source)
        {
            source.UpdateSource(
                Expression.Call(
                    QueryableMethods.DefaultIfEmptyWithoutArgument.MakeGenericMethod(source.SourceElementType),
                    source.Source));

            _entityReferenceOptionalMarkingExpressionVisitor.Visit(source.PendingSelector);

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
                var firstArgumet = Visit(methodCallExpression.Arguments[0]);
                if (firstArgumet is NavigationExpansionExpression source
                    && source.Type == methodCallExpression.Type)
                {
                    source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
                    var newStructure = SnapshotExpression(source.PendingSelector);
                    var queryable = Reduce(source);

                    var result = Expression.Call(
                            methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(queryableElementType),
                            new[] { queryable }.Concat(methodCallExpression.Arguments.Skip(1).Select(e => Visit(e))));

                    var navigationTree = new NavigationTreeExpression(newStructure);
                    var parameterName = GetParameterName("e");

                    return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Expression ProcessInclude(
            NavigationExpansionExpression source, Expression expression, bool thenInclude)
        {
            if (source.PendingSelector is NavigationTreeExpression navigationTree
                && navigationTree.Value is EntityReference entityReferece)
            {
                if (entityReferece.EntityType.GetDefiningQuery() != null)
                {
                    throw new InvalidOperationException(CoreStrings.IncludeOnEntityWithDefiningQueryNotSupported(entityReferece.EntityType.DisplayName()));
                }

                if (expression is ConstantExpression includeConstant
                    && includeConstant.Value is string navigationChain)
                {
                    var navigationPaths = navigationChain.Split(new[] { "." }, StringSplitOptions.None);
                    var includeTreeNodes = new Queue<IncludeTreeNode>();
                    includeTreeNodes.Enqueue(entityReferece.IncludePaths);
                    foreach (var navigationName in navigationPaths)
                    {
                        var nodesToProcess = includeTreeNodes.Count;
                        while (nodesToProcess-- > 0)
                        {
                            var currentNode = includeTreeNodes.Dequeue();
                            foreach (var navigation in FindNavigations(currentNode.EntityType, navigationName))
                            {
                                var addedNode = currentNode.AddNavigation(navigation);
                                // This is to add eager Loaded navigations when owner type is included.
                                PopulateEagerLoadedNavigations(addedNode);
                                includeTreeNodes.Enqueue(addedNode);
                            }
                        }

                        if (includeTreeNodes.Count == 0)
                        {
                            throw new InvalidOperationException("Invalid include path: '" + navigationChain +
                                "' - couldn't find navigation for: '" + navigationName + "'");
                        }
                    }
                }
                else
                {
                    var currentIncludeTreeNode = thenInclude
                        ? entityReferece.LastIncludeTreeNode
                        : entityReferece.IncludePaths;
                    var includeLambda = expression.UnwrapLambdaFromQuote();
                    var lastIncludeTree = PopulateIncludeTree(currentIncludeTreeNode, includeLambda.Body);
                    if (lastIncludeTree == null)
                    {
                        throw new InvalidOperationException("Lambda expression used inside Include is not valid.");
                    }
                    entityReferece.SetLastInclude(lastIncludeTree);
                }

                return source;
            }

            throw new InvalidOperationException("Include has been used on non entity queryable.");
        }

        private IncludeTreeNode PopulateIncludeTree(IncludeTreeNode includeTreeNode, Expression expression)
        {
            switch (expression)
            {
                case ParameterExpression _:
                    return includeTreeNode;

                case MemberExpression memberExpression:

                    var innerExpression = memberExpression.Expression;
                    Type convertedType = null;
                    if (innerExpression is UnaryExpression unaryExpression
                        && (unaryExpression.NodeType == ExpressionType.Convert
                            || unaryExpression.NodeType == ExpressionType.ConvertChecked
                            || unaryExpression.NodeType == ExpressionType.TypeAs))
                    {
                        convertedType = unaryExpression.Type;
                        innerExpression = unaryExpression.Operand;
                    }

                    var innerIncludeTreeNode = PopulateIncludeTree(includeTreeNode, innerExpression);
                    var entityType = innerIncludeTreeNode.EntityType;
                    if (convertedType != null)
                    {
                        entityType = entityType.GetTypesInHierarchy().FirstOrDefault(et => et.ClrType == convertedType);
                        if (entityType == null)
                        {
                            throw new InvalidOperationException("Invalid include.");
                        }
                    }

                    var navigation = entityType.FindNavigation(memberExpression.Member);
                    if (navigation != null)
                    {
                        // This is to add eager Loaded navigations when owner type is included.
                        var addedNode = innerIncludeTreeNode.AddNavigation(navigation);
                        PopulateEagerLoadedNavigations(addedNode);
                        return addedNode;
                    }
                    break;
            }

            return null;
        }

        private IEnumerable<INavigation> FindNavigations(IEntityType entityType, string navigationName)
        {
            var navigation = entityType.FindNavigation(navigationName);
            if (navigation != null)
            {
                yield return navigation;
            }
            else
            {
                foreach (var derivedNavigation in entityType.GetDerivedTypes()
                    .Select(et => et.FindDeclaredNavigation(navigationName)).Where(n => n != null))
                {
                    yield return derivedNavigation;
                }
            }
        }

        private Expression ProcessSelectMany(
            NavigationExpansionExpression source,
            LambdaExpression collectionSelector,
            LambdaExpression resultSelector)
        {
            var collectionSelectorBody = ExpandNavigationsInLambdaExpression(source, collectionSelector);
            if (collectionSelectorBody is MaterializeCollectionNavigationExpression materializeCollectionNavigationExpression)
            {
                collectionSelectorBody = materializeCollectionNavigationExpression.Subquery;
            }

            if (collectionSelectorBody is NavigationExpansionExpression collectionSource)
            {
                collectionSource = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(collectionSource);
                var innerTree = new NavigationTreeExpression(SnapshotExpression(collectionSource.PendingSelector));
                collectionSelector = GenerateLambda(collectionSource, source.CurrentParameter);
                var collectionElementType = collectionSelector.ReturnType.TryGetSequenceType();

                // Collection selector body is IQueryable, we need to adjust the type to IEnumerable, to match the SelectMany signature
                // therefore the delegate type is specified explicitly
                var collectionSelectorLambdaType = typeof(Func<,>).MakeGenericType(
                    source.SourceElementType,
                    typeof(IEnumerable<>).MakeGenericType(collectionElementType));

                collectionSelector = Expression.Lambda(
                    collectionSelectorLambdaType,
                    collectionSelector.Body,
                    collectionSelector.Parameters[0]);

                var transparentIdentifierType = TransparentIdentifierFactory.Create(
                    source.SourceElementType, collectionElementType);
                var transparentIdentifierOuterMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
                var transparentIdentifierInnerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");
                var collectionElementParameter = Expression.Parameter(collectionElementType, "c");

                var newResultSelector = Expression.Lambda(
                    Expression.New(
                        transparentIdentifierType.GetTypeInfo().GetConstructors().Single(),
                        new[] { source.CurrentParameter, collectionElementParameter },
                        new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                    source.CurrentParameter,
                    collectionElementParameter);

                var newSource = Expression.Call(
                    QueryableMethods.SelectManyWithCollectionSelector.MakeGenericMethod(
                        source.SourceElementType, collectionElementType, newResultSelector.ReturnType),
                    source.Source,
                    Expression.Quote(collectionSelector),
                    Expression.Quote(newResultSelector));

                var currentTree = new NavigationTreeNode(source.CurrentTree, innerTree);
                var pendingSelector = resultSelector == null
                    ? innerTree
                    : new ReplacingExpressionVisitor(
                        new Dictionary<Expression, Expression>
                        {
                            { resultSelector.Parameters[0], source.PendingSelector },
                            { resultSelector.Parameters[1], innerTree }
                        }).Visit(resultSelector.Body);
                var parameterName = GetParameterName("ti");

                return new NavigationExpansionExpression(newSource, currentTree, pendingSelector, parameterName);
            }

            throw new InvalidOperationException(CoreStrings.QueryFailed(collectionSelector.Print(), GetType().Name));
        }

        private void ApplyPendingOrderings(NavigationExpansionExpression source)
        {
            if (source.PendingOrderings.Any())
            {
                foreach (var (orderingMethod, keySelector) in source.PendingOrderings)
                {
                    var keySelectorLambda = GenerateLambda(keySelector, source.CurrentParameter);

                    source.UpdateSource(Expression.Call(
                        orderingMethod.MakeGenericMethod(source.SourceElementType, keySelectorLambda.ReturnType),
                        source.Source,
                        keySelectorLambda));
                }

                source.ClearPendingOrderings();
            }
        }

        private bool CompareIncludes(Expression outer, Expression inner)
        {
            if (outer is EntityReference outerEntityReference
                && inner is EntityReference innerEntityReference)
            {
                return outerEntityReference.IncludePaths.Equals(innerEntityReference.IncludePaths);
            }

            if (outer is NewExpression outerNewExpression
                && inner is NewExpression innerNewExpression)
            {
                if (outerNewExpression.Arguments.Count != innerNewExpression.Arguments.Count)
                {
                    return false;
                }

                for (var i = 0; i < outerNewExpression.Arguments.Count; i++)
                {
                    if (!CompareIncludes(outerNewExpression.Arguments[i], innerNewExpression.Arguments[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return outer is DefaultExpression outerDefaultExpression
                && inner is DefaultExpression innerDefaultExpression
                && outerDefaultExpression.Type == innerDefaultExpression.Type;
        }

        private Expression ProcessGroupBy(
            NavigationExpansionExpression source,
            LambdaExpression keySelector,
            LambdaExpression elementSelector,
            LambdaExpression resultSelector)
        {
            var keySelectorBody = ExpandNavigationsInLambdaExpression(source, keySelector);
            Expression result;
            if (elementSelector == null)
            {
                source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
                // TODO: Flow include in future
                //source = (NavigationExpansionExpression)new IncludeApplyingExpressionVisitor(
                //    this, _queryCompilationContext.IsTracking).Visit(source);
                keySelector = GenerateLambda(keySelectorBody, source.CurrentParameter);
                elementSelector = GenerateLambda(source.PendingSelector, source.CurrentParameter);
                if (resultSelector == null)
                {
                    result = Expression.Call(
                        QueryableMethods.GroupByWithKeyElementSelector.MakeGenericMethod(
                            source.CurrentParameter.Type, keySelector.ReturnType, elementSelector.ReturnType),
                        source.Source,
                        Expression.Quote(keySelector),
                        Expression.Quote(elementSelector));
                }
                else
                {
                    result = Expression.Call(
                        QueryableMethods.GroupByWithKeyElementResultSelector.MakeGenericMethod(
                            source.CurrentParameter.Type, keySelector.ReturnType, elementSelector.ReturnType, resultSelector.ReturnType),
                        source.Source,
                        Expression.Quote(keySelector),
                        Expression.Quote(elementSelector),
                        Expression.Quote(resultSelector));
                }
            }
            else
            {
                source = (NavigationExpansionExpression)ProcessSelect(source, elementSelector);
                source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
                source = (NavigationExpansionExpression)new IncludeApplyingExpressionVisitor(
                    this, _queryCompilationContext.IsTracking).Visit(source);
                keySelector = GenerateLambda(keySelectorBody, source.CurrentParameter);
                elementSelector = GenerateLambda(source.PendingSelector, source.CurrentParameter);
                if (resultSelector == null)
                {
                    result = Expression.Call(
                        QueryableMethods.GroupByWithKeyElementSelector.MakeGenericMethod(
                            source.CurrentParameter.Type, keySelector.ReturnType, elementSelector.ReturnType),
                        source.Source,
                        Expression.Quote(keySelector),
                        Expression.Quote(elementSelector));
                }
                else
                {
                    result = Expression.Call(
                        QueryableMethods.GroupByWithKeyElementResultSelector.MakeGenericMethod(
                            source.CurrentParameter.Type, keySelector.ReturnType, elementSelector.ReturnType, resultSelector.ReturnType),
                        source.Source,
                        Expression.Quote(keySelector),
                        Expression.Quote(elementSelector),
                        Expression.Quote(resultSelector));
                }
            }

            var navigationTree = new NavigationTreeExpression(Expression.Default(result.Type.TryGetSequenceType()));
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private Expression ProcessSetOperation(
            NavigationExpansionExpression outerSource,
            MethodInfo genericMethod,
            NavigationExpansionExpression innerSource)
        {
            outerSource = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(outerSource);
            var outerTreeStructure = SnapshotExpression(outerSource.PendingSelector);

            innerSource = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(innerSource);
            var innerTreeStructure = SnapshotExpression(innerSource.PendingSelector);

            if (!CompareIncludes(outerTreeStructure, innerTreeStructure))
            {
                throw new InvalidOperationException(CoreStrings.SetOperationWithDifferentIncludesInOperands);
            }

            var outerQueryable = Reduce(outerSource);
            var innerQueryable = Reduce(innerSource);

            var outerType = outerQueryable.Type.TryGetSequenceType();
            var innerType = innerQueryable.Type.TryGetSequenceType();

            var result = Expression.Call(
                genericMethod.MakeGenericMethod(outerType.IsAssignableFrom(innerType) ? outerType : innerType),
                outerQueryable,
                innerQueryable);
            var navigationTree = new NavigationTreeExpression(
                outerType.IsAssignableFrom(innerType) ? outerTreeStructure : innerTreeStructure);
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private Expression ProcessJoin(
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

            var outerKey = ExpandNavigationsInLambdaExpression(outerSource, outerKeySelector);
            var innerKey = ExpandNavigationsInLambdaExpression(innerSource, innerKeySelector);

            outerKeySelector = GenerateLambda(outerKey, outerSource.CurrentParameter);
            innerKeySelector = GenerateLambda(innerKey, innerSource.CurrentParameter);

            var transparentIdentifierType = TransparentIdentifierFactory.Create(
                outerSource.SourceElementType, innerSource.SourceElementType);

            var transparentIdentifierOuterMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var transparentIdentifierInnerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");

            var newResultSelector = Expression.Lambda(
                Expression.New(
                    transparentIdentifierType.GetTypeInfo().GetConstructors().Single(),
                    new[] { outerSource.CurrentParameter, innerSource.CurrentParameter },
                    new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                outerSource.CurrentParameter,
                innerSource.CurrentParameter);

            var source = Expression.Call(
                QueryableMethods.Join.MakeGenericMethod(
                    outerSource.SourceElementType, innerSource.SourceElementType, outerKeySelector.ReturnType, newResultSelector.ReturnType),
                outerSource.Source,
                innerSource.Source,
                Expression.Quote(outerKeySelector),
                Expression.Quote(innerKeySelector),
                Expression.Quote(newResultSelector));

            var currentTree = new NavigationTreeNode(outerSource.CurrentTree, innerSource.CurrentTree);
            var pendingSelector = new ReplacingExpressionVisitor(
                new Dictionary<Expression, Expression>
                {
                    { resultSelector.Parameters[0], outerSource.PendingSelector },
                    { resultSelector.Parameters[1], innerSource.PendingSelector }
                }).Visit(resultSelector.Body);
            var parameterName = GetParameterName("ti");

            return new NavigationExpansionExpression(source, currentTree, pendingSelector, parameterName);
        }

        private Expression ProcessLeftJoin(
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

            var outerKey = ExpandNavigationsInLambdaExpression(outerSource, outerKeySelector);
            var innerKey = ExpandNavigationsInLambdaExpression(innerSource, innerKeySelector);

            outerKeySelector = GenerateLambda(outerKey, outerSource.CurrentParameter);
            innerKeySelector = GenerateLambda(innerKey, innerSource.CurrentParameter);

            var transparentIdentifierType = TransparentIdentifierFactory.Create(
                outerSource.SourceElementType, innerSource.SourceElementType);

            var transparentIdentifierOuterMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Outer");
            var transparentIdentifierInnerMemberInfo = transparentIdentifierType.GetTypeInfo().GetDeclaredField("Inner");

            var newResultSelector = Expression.Lambda(
                Expression.New(
                    transparentIdentifierType.GetTypeInfo().GetConstructors().Single(),
                    new[] { outerSource.CurrentParameter, innerSource.CurrentParameter },
                    new[] { transparentIdentifierOuterMemberInfo, transparentIdentifierInnerMemberInfo }),
                outerSource.CurrentParameter,
                innerSource.CurrentParameter);

            var source = Expression.Call(
                QueryableExtensions.LeftJoinMethodInfo.MakeGenericMethod(
                    outerSource.SourceElementType, innerSource.SourceElementType, outerKeySelector.ReturnType, newResultSelector.ReturnType),
                outerSource.Source,
                innerSource.Source,
                Expression.Quote(outerKeySelector),
                Expression.Quote(innerKeySelector),
                Expression.Quote(newResultSelector));

            var innerPendingSelector = innerSource.PendingSelector;
            innerPendingSelector = _entityReferenceOptionalMarkingExpressionVisitor.Visit(innerPendingSelector);

            var currentTree = new NavigationTreeNode(outerSource.CurrentTree, innerSource.CurrentTree);
            var pendingSelector = new ReplacingExpressionVisitor(
                new Dictionary<Expression, Expression>
                {
                    { resultSelector.Parameters[0], outerSource.PendingSelector },
                    { resultSelector.Parameters[1], innerPendingSelector }
                }).Visit(resultSelector.Body);
            var parameterName = GetParameterName("ti");

            return new NavigationExpansionExpression(source, currentTree, pendingSelector, parameterName);
        }

        private Expression ProcessCastOfType(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            Type castType)
        {
            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var newStructure = SnapshotExpression(source.PendingSelector);
            var queryable = Reduce(source);

            var result = Expression.Call(genericMethod.MakeGenericMethod(castType), queryable);

            if (newStructure is EntityReference entityReference)
            {
                var castEntityType = entityReference.EntityType.GetTypesInHierarchy().FirstOrDefault(et => et.ClrType == castType);
                if (castEntityType != null)
                {
                    var newEntityReference = new EntityReference(castEntityType);
                    if (entityReference.IsOptional)
                    {
                        newEntityReference.MarkAsOptional();
                    }
                    newEntityReference.SetIncludePaths(entityReference.IncludePaths);

                    // Prune includes for sibling types
                    var siblingNavigations = newEntityReference.IncludePaths.Keys
                        .Where(n => !castEntityType.IsAssignableFrom(n.DeclaringEntityType)
                            && !n.DeclaringEntityType.IsAssignableFrom(castEntityType)).ToList();

                    foreach (var navigation in siblingNavigations)
                    {
                        newEntityReference.IncludePaths.Remove(navigation);
                    }

                    newStructure = newEntityReference;
                }
            }
            else
            {
                newStructure = Expression.Default(castType);
            }

            var navigationTree = new NavigationTreeExpression(newStructure);
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private Expression ProcessAllAnyCountLongCount(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            LambdaExpression predicate)
        {
            if (predicate != null)
            {
                var predicateBody = ExpandNavigationsInLambdaExpression(source, predicate);

                return Expression.Call(
                    genericMethod.GetGenericMethodDefinition().MakeGenericMethod(source.SourceElementType),
                    source.Source,
                    Expression.Quote(GenerateLambda(predicateBody, source.CurrentParameter)));
            }

            return Expression.Call(
                genericMethod.MakeGenericMethod(source.SourceElementType),
                source.Source);
        }

        private Expression ProcessAverageMaxMinSum(
            NavigationExpansionExpression source,
            MethodInfo method,
            LambdaExpression selector)
        {
            if (selector != null)
            {
                source = (NavigationExpansionExpression)ProcessSelect(source, selector);
                source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);

                var selectorLambda = GenerateLambda(source.PendingSelector, source.CurrentParameter);
                if (method.GetGenericArguments().Length == 2)
                {
                    // Min/Max with selector has 2 generic parameters
                    method = method.MakeGenericMethod(source.SourceElementType, selectorLambda.ReturnType);
                }
                else
                {
                    method = method.MakeGenericMethod(source.SourceElementType);
                }

                return Expression.Call(method, source.Source, selectorLambda);
            }

            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var queryable = Reduce(source);

            if (method.GetGenericArguments().Length == 1)
            {
                // Min/Max without selector has 1 generic parameters
                method = method.MakeGenericMethod(queryable.Type.TryGetSequenceType());
            }

            return Expression.Call(method, queryable);
        }

        private Expression SnapshotExpression(Expression selector)
        {
            switch (selector)
            {
                case EntityReference entityReference:
                    return entityReference.Clone();

                case NavigationTreeExpression navigationTreeExpression:
                    return SnapshotExpression(navigationTreeExpression.Value);

                case NewExpression newExpression:
                {
                    var arguments = new Expression[newExpression.Arguments.Count];
                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        arguments[i] = newExpression.Arguments[i] is NewExpression
                            || newExpression.Arguments[i] is NavigationTreeExpression
                            ? SnapshotExpression(newExpression.Arguments[i])
                            : Expression.Default(newExpression.Arguments[i].Type);
                    }

                    return newExpression.Update(arguments);
                }

                case OwnedNavigationReference ownedNavigationReference:
                    return ownedNavigationReference.EntityReference.Clone();

                default:
                    return Expression.Default(selector.Type);
            }
        }

        private static readonly IDictionary<MethodInfo, MethodInfo> _predicateLessMethodInfo = new Dictionary<MethodInfo, MethodInfo>
        {
            { QueryableMethods.FirstWithPredicate, QueryableMethods.FirstWithoutPredicate },
            { QueryableMethods.FirstOrDefaultWithPredicate, QueryableMethods.FirstOrDefaultWithoutPredicate },
            { QueryableMethods.SingleWithPredicate, QueryableMethods.SingleWithoutPredicate },
            { QueryableMethods.SingleOrDefaultWithPredicate, QueryableMethods.SingleOrDefaultWithoutPredicate },
            { QueryableMethods.LastWithPredicate, QueryableMethods.LastWithoutPredicate },
            { QueryableMethods.LastOrDefaultWithPredicate, QueryableMethods.LastOrDefaultWithoutPredicate },
        };

        private Expression ProcessFirstSingleLastOrDefault(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            LambdaExpression predicate,
            Type returnType)
        {
            if (predicate != null)
            {
                var predicateBody = ExpandNavigationsInLambdaExpression(source, predicate);

                source.UpdateSource(Expression.Call(
                    QueryableMethods.Where.MakeGenericMethod(source.SourceElementType),
                    source.Source,
                    Expression.Quote(GenerateLambda(predicateBody, source.CurrentParameter))));

                genericMethod = _predicateLessMethodInfo[genericMethod];
            }

            if (returnType == typeof(object)
                && source.PendingSelector.Type != typeof(object))
            {
                source.ApplySelector(Expression.Convert(source.PendingSelector, returnType));
            }

            source.ConvertToSingleResult(genericMethod);

            return source;
        }

        private Expression ProcessOrderByThenBy(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            LambdaExpression keySelector,
            bool thenBy)
        {
            var keySelectorBody = ExpandNavigationsInLambdaExpression(source, keySelector);

            if (thenBy)
            {
                source.AppendPendingOrdering(genericMethod, keySelectorBody);
            }
            else
            {
                source.AddPendingOrdering(genericMethod, keySelectorBody);
            }

            return source;
        }

        private Expression ProcessSelect(
            NavigationExpansionExpression source,
            LambdaExpression selector)
        {
            // This is to apply aggregate operator on GroupBy right away rather than deferring
            if (source.SourceElementType.IsGenericType
                && source.SourceElementType.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                && !(selector.ReturnType.IsGenericType
                    && selector.ReturnType.GetGenericTypeDefinition() == typeof(IGrouping<,>)))
            {
                var selectorLambda = GenerateLambda(ExpandNavigationsInLambdaExpression(source, selector), source.CurrentParameter);

                var newSource = Expression.Call(
                    QueryableMethods.Select.MakeGenericMethod(source.SourceElementType, selectorLambda.ReturnType),
                    source.Source,
                    Expression.Quote(selectorLambda));

                var navigationTree = new NavigationTreeExpression(Expression.Default(selectorLambda.ReturnType));
                var parameterName = GetParameterName("e");

                return new NavigationExpansionExpression(newSource, navigationTree, navigationTree, parameterName);
            }

            var selectorBody = ReplacingExpressionVisitor.Replace(
                selector.Parameters[0],
                source.PendingSelector,
                selector.Body);

            source.ApplySelector(selectorBody);

            return source;
        }

        private Expression ProcessContains(
            NavigationExpansionExpression source,
            Expression item)
        {
            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var queryable = Reduce(source);

            return Expression.Call(
                QueryableMethods.Contains.MakeGenericMethod(queryable.Type.TryGetSequenceType()),
                queryable,
                item);
        }

        private Expression ProcessDistinctSkipTake(
            NavigationExpansionExpression source,
            MethodInfo genericMethod,
            Expression count)
        {
            source = (NavigationExpansionExpression)_pendingSelectorExpandingExpressionVisitor.Visit(source);
            var newStructure = SnapshotExpression(source.PendingSelector);
            var queryable = Reduce(source);

            var result = count == null
                ? Expression.Call(
                    genericMethod.MakeGenericMethod(queryable.Type.TryGetSequenceType()),
                    queryable)
                : Expression.Call(
                    genericMethod.MakeGenericMethod(queryable.Type.TryGetSequenceType()),
                    queryable,
                    count);

            var navigationTree = new NavigationTreeExpression(newStructure);
            var parameterName = GetParameterName("e");

            return new NavigationExpansionExpression(result, navigationTree, navigationTree, parameterName);
        }

        private Expression ProcessWhere(
            NavigationExpansionExpression source,
            LambdaExpression predicate)
        {
            var predicateBody = ExpandNavigationsInLambdaExpression(source, predicate);

            source.UpdateSource(Expression.Call(
                QueryableMethods.Where.MakeGenericMethod(source.SourceElementType),
                source.Source,
                Expression.Quote(GenerateLambda(predicateBody, source.CurrentParameter))));

            return source;
        }

        private LambdaExpression GenerateLambda(Expression body, ParameterExpression currentParameter)
        {
            return Expression.Lambda(Reduce(body), currentParameter);
        }

        private Expression Reduce(Expression source)
        {
            return _reducingExpressionVisitor.Visit(source);
        }

        private Expression ExpandNavigationsInExpression(NavigationExpansionExpression source, Expression expression)
        {
            expression = new ExpandingExpressionVisitor(this, source).Visit(expression);
            expression = _subqueryMemberPushdownExpressionVisitor.Visit(expression);
            expression = Visit(expression);

            return expression;
        }

        private Expression ExpandNavigationsInLambdaExpression(
            NavigationExpansionExpression source,
            LambdaExpression lambdaExpression)
        {
            var lambdaBody = ReplacingExpressionVisitor.Replace(
                lambdaExpression.Parameters[0],
                source.PendingSelector,
                lambdaExpression.Body);

            return ExpandNavigationsInExpression(source, lambdaBody);
        }

        private class Parameters : IParameterValues
        {
            private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();

            public IReadOnlyDictionary<string, object> ParameterValues => (IReadOnlyDictionary<string, object>)_parameterValues;

            public virtual void AddParameter(string name, object value)
            {
                _parameterValues.Add(name, value);
            }
        }
    }
}
