// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IncludeCompiler
    {
        private static readonly ExpressionEqualityComparer _expressionEqualityComparer
            = new ExpressionEqualityComparer();

        private static readonly MethodInfo _referenceEqualsMethodInfo
            = typeof(object).GetTypeInfo()
                .GetDeclaredMethod(nameof(ReferenceEquals));

        private static readonly MethodInfo _collectionAccessorAddMethodInfo
            = typeof(IClrCollectionAccessor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IClrCollectionAccessor.Add));

        private static readonly MethodInfo _queryBufferStartTrackingMethodInfo
            = typeof(IQueryBuffer).GetTypeInfo()
                .GetDeclaredMethods(nameof(IQueryBuffer.StartTracking))
                .Single(mi => mi.GetParameters()[1].ParameterType == typeof(IEntityType));

        private static readonly MethodInfo _queryBufferIncludeCollectionMethodInfo
            = typeof(IQueryBuffer).GetTypeInfo()
                .GetDeclaredMethod(nameof(IQueryBuffer.IncludeCollection));

        private static readonly MethodInfo _queryBufferIncludeCollectionAsyncMethodInfo
            = typeof(IQueryBuffer).GetTypeInfo()
                .GetDeclaredMethod(nameof(IQueryBuffer.IncludeCollectionAsync));

        private static readonly ParameterExpression _includedParameter
            = Expression.Parameter(typeof(object[]), name: "included");

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        private static readonly ParameterExpression _cancellationTokenParameter
            = Expression.Parameter(typeof(CancellationToken), name: "ct");

        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;

        private int _collectionIncludeId;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IncludeCompiler(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
        {
            _queryCompilationContext = queryCompilationContext;
            _querySourceTracingExpressionVisitorFactory = querySourceTracingExpressionVisitorFactory;
        }

        private struct IncludeSpecification
        {
            public IncludeSpecification(
                IncludeResultOperator includeResultOperator,
                QuerySourceReferenceExpression querySourceReferenceExpression,
                INavigation[] navigationPath)
            {
                IncludeResultOperator = includeResultOperator;
                QuerySourceReferenceExpression = querySourceReferenceExpression;
                NavigationPath = navigationPath;
            }

            public IncludeResultOperator IncludeResultOperator { get; }
            public QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }
            public INavigation[] NavigationPath { get; }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void CompileIncludes(
            [NotNull] QueryModel queryModel,
            [NotNull] ICollection<IncludeResultOperator> includeResultOperators,
            bool trackingQuery,
            bool asyncQuery)
        {
            if (queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                return;
            }

            var includeGroupings
                = CreateIncludeSpecifications(queryModel, includeResultOperators)
                    .GroupBy(a => a.QuerySourceReferenceExpression);

            var parentOrderings = new List<Ordering>();

            var compiledIncludes
                = new List<(
                    QuerySourceReferenceExpression querySourceReferenceExpression,
                    Expression expression)>();

            foreach (var includeGrouping in includeGroupings)
            {
                var entityParameter = Expression.Parameter(includeGrouping.Key.Type, name: "entity");

                var propertyExpressions = new List<Expression>();
                var blockExpressions = new List<Expression>();

                if (trackingQuery)
                {
                    blockExpressions.Add(
                        Expression.Call(
                            Expression.Property(
                                EntityQueryModelVisitor.QueryContextParameter,
                                nameof(QueryContext.QueryBuffer)),
                            _queryBufferStartTrackingMethodInfo,
                            entityParameter,
                            Expression.Constant(
                                _queryCompilationContext.Model
                                    .FindEntityType(entityParameter.Type))));
                }

                var includedIndex = 0;

                foreach (var includeSpecification in includeGrouping)
                {
                    _queryCompilationContext.Logger
                        .LogDebug(
                            CoreEventId.IncludingNavigation,
                            () => CoreStrings.LogIncludingNavigation(
                                $"{includeSpecification.IncludeResultOperator.PathFromQuerySource}.{includeSpecification.IncludeResultOperator.NavigationPropertyPaths.Join(".")}"));

                    var navigation = includeSpecification.NavigationPath[0];

                    if (navigation.IsCollection())
                    {
                        var collectionIncludeQueryModel
                            = BuildCollectionIncludeQueryModel(
                                queryModel,
                                navigation,
                                includeSpecification.QuerySourceReferenceExpression,
                                parentOrderings);

                        _queryCompilationContext.AddQuerySourceRequiringMaterialization(
                            ((QuerySourceReferenceExpression)collectionIncludeQueryModel.SelectClause.Selector).ReferencedQuerySource);

                        Expression collectionLambdaExpression 
                            = Expression.Lambda<Func<IEnumerable<object>>>(
                                new SubQueryExpression(collectionIncludeQueryModel));

                        var includeCollectionMethodInfo = _queryBufferIncludeCollectionMethodInfo;

                        Expression cancellationTokenExpression = null;

                        if (asyncQuery)
                        {
                            collectionLambdaExpression
                                = Expression.Convert(
                                    collectionLambdaExpression,
                                    typeof(Func<IAsyncEnumerable<object>>));

                            includeCollectionMethodInfo = _queryBufferIncludeCollectionAsyncMethodInfo;
                            cancellationTokenExpression = _cancellationTokenParameter;
                        }

                        blockExpressions.Add(
                            BuildCollectionIncludeExpressions(
                                navigation,
                                entityParameter,
                                trackingQuery,
                                collectionLambdaExpression,
                                includeCollectionMethodInfo,
                                cancellationTokenExpression));
                    }
                    else
                    {
                        propertyExpressions.AddRange(
                            includeSpecification.NavigationPath
                                .Select(
                                    (t, i) =>
                                        includeSpecification.NavigationPath
                                            .Take(i + 1)
                                            .Aggregate(
                                                (Expression)includeSpecification.QuerySourceReferenceExpression,
                                                EntityQueryModelVisitor.CreatePropertyExpression)));

                        blockExpressions.Add(
                            BuildIncludeExpressions(
                                includeSpecification.NavigationPath,
                                entityParameter,
                                trackingQuery,
                                ref includedIndex,
                                navigationIndex: 0));
                    }

                    // TODO: Hack until new Include fully implemented
                    includeResultOperators.Remove(includeSpecification.IncludeResultOperator);
                }

                Expression includeExpression = null;

                if (asyncQuery)
                {
                    var taskExpression = new List<Expression>();

                    foreach (var expression in blockExpressions.ToArray())
                    {
                        if (expression.Type == typeof(Task))
                        {
                            blockExpressions.Remove(expression);
                            taskExpression.Add(expression);
                        }
                    }

                    if (taskExpression.Count > 0)
                    {
                        blockExpressions.Add(
                            Expression.Call(
                                _awaitIncludesMethodInfo,
                                Expression.NewArrayInit(
                                    typeof(Func<Task>),
                                    taskExpression.Select(e => Expression.Lambda(e)))));

                        includeExpression
                            = Expression.Property(
                                Expression.Call(
                                    _includeAsyncMethodInfo.MakeGenericMethod(includeGrouping.Key.Type),
                                    EntityQueryModelVisitor.QueryContextParameter,
                                    includeGrouping.Key,
                                    Expression.NewArrayInit(typeof(object), propertyExpressions),
                                    Expression.Lambda(
                                        Expression.Block(blockExpressions),
                                        EntityQueryModelVisitor.QueryContextParameter,
                                        entityParameter,
                                        _includedParameter,
                                        _cancellationTokenParameter),
                                    _cancellationTokenParameter),
                                nameof(Task<object>.Result));
                    }
                }

                if (includeExpression == null)
                {
                    includeExpression
                        = Expression.Call(
                            _includeMethodInfo.MakeGenericMethod(includeGrouping.Key.Type),
                            EntityQueryModelVisitor.QueryContextParameter,
                            includeGrouping.Key,
                            Expression.NewArrayInit(typeof(object), propertyExpressions),
                            Expression.Lambda(
                                Expression.Block(typeof(void), blockExpressions),
                                EntityQueryModelVisitor.QueryContextParameter,
                                entityParameter,
                                _includedParameter));
                }

                compiledIncludes.Add((includeGrouping.Key, includeExpression));
            }

            ApplyParentOrderings(queryModel, parentOrderings);
            ApplyIncludeExpressionsToQueryModel(queryModel, compiledIncludes);
        }

        private static void ApplyParentOrderings(
            QueryModel queryModel, IReadOnlyCollection<Ordering> parentOrderings)
        {
            if (parentOrderings.Any())
            {
                var orderByClause
                    = queryModel.BodyClauses
                        .OfType<OrderByClause>()
                        .LastOrDefault();

                if (orderByClause == null)
                {
                    orderByClause = new OrderByClause();
                    queryModel.BodyClauses.Add(orderByClause);
                }

                foreach (var ordering in parentOrderings)
                {
                    orderByClause.Orderings.Add(ordering);
                }
            }
        }

        private static void ApplyIncludeExpressionsToQueryModel(
            QueryModel queryModel,
            IEnumerable<(
                QuerySourceReferenceExpression querySourceReferenceExpression,
                Expression expression)> compiledIncludes)
        {
            var includeReplacingExpressionVisitor = new IncludeReplacingExpressionVisitor();

            foreach (var include in compiledIncludes)
            {
                queryModel.SelectClause.TransformExpressions(
                    e => includeReplacingExpressionVisitor.Replace(
                        include.querySourceReferenceExpression,
                        include.expression,
                        e));

                foreach (var groupResultOperator
                    in queryModel.ResultOperators.OfType<GroupResultOperator>())
                {
                    groupResultOperator.ElementSelector
                        = includeReplacingExpressionVisitor.Replace(
                            include.querySourceReferenceExpression,
                            include.expression,
                            groupResultOperator.ElementSelector);
                }
            }
        }

        private static readonly MethodInfo _awaitIncludesMethodInfo
            = typeof(IncludeCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(_AwaitIncludes));

        // ReSharper disable once InconsistentNaming
        private static async Task _AwaitIncludes(IReadOnlyList<Func<Task>> taskFactories)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < taskFactories.Count; i++)
            {
                await taskFactories[i]();
            }
        }

        private class IncludeReplacingExpressionVisitor : RelinqExpressionVisitor
        {
            private QuerySourceReferenceExpression _querySourceReferenceExpression;
            private Expression _includeExpression;

            public Expression Replace(
                QuerySourceReferenceExpression querySourceReferenceExpression,
                Expression includeExpression,
                Expression searchedExpression)
            {
                _querySourceReferenceExpression = querySourceReferenceExpression;
                _includeExpression = includeExpression;

                return Visit(searchedExpression);
            }

            protected override Expression VisitQuerySourceReference(
                QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                if (ReferenceEquals(querySourceReferenceExpression, _querySourceReferenceExpression))
                {
                    _querySourceReferenceExpression = null;

                    return _includeExpression;
                }

                return querySourceReferenceExpression;
            }
        }

        private QueryModel BuildCollectionIncludeQueryModel(
            QueryModel parentQueryModel,
            INavigation navigation,
            QuerySourceReferenceExpression parentQuerySourceReferenceExpression,
            ICollection<Ordering> parentOrderings)
        {
            BuildParentOrderings(
                parentQueryModel,
                navigation,
                parentQuerySourceReferenceExpression,
                parentOrderings);

            var parentQuerySource = parentQuerySourceReferenceExpression.ReferencedQuerySource;

            var parentItemName
                = parentQuerySource.HasGeneratedItemName()
                    ? navigation.DeclaringEntityType.DisplayName()[0].ToString().ToLowerInvariant()
                    : parentQuerySource.ItemName;

            var collectionMainFromClause
                = new MainFromClause(
                    $"{parentItemName}.{navigation.Name}",
                    navigation.GetTargetType().ClrType,
                    NullAsyncQueryProvider.Instance
                        .CreateEntityQueryableExpression(navigation.GetTargetType().ClrType));

            var collectionQuerySourceReferenceExpression
                = new QuerySourceReferenceExpression(collectionMainFromClause);

            var collectionQueryModel
                = new QueryModel(
                    collectionMainFromClause,
                    new SelectClause(collectionQuerySourceReferenceExpression));

            var querySourceMapping = new QuerySourceMapping();
            var clonedParentQueryModel = parentQueryModel.Clone(querySourceMapping);

            CloneAnnotations(querySourceMapping, clonedParentQueryModel);

            var clonedParentQuerySourceReferenceExpression
                = (QuerySourceReferenceExpression)querySourceMapping.GetExpression(parentQuerySource);

            var clonedParentQuerySource
                = clonedParentQuerySourceReferenceExpression.ReferencedQuerySource;

            AdjustPredicate(
                clonedParentQueryModel,
                clonedParentQuerySource,
                clonedParentQuerySourceReferenceExpression);

            clonedParentQueryModel.SelectClause
                = new SelectClause(Expression.Default(typeof(AnonymousObject)));

            var subQueryProjection = new List<Expression>();

            var lastResultOperator = ProcessResultOperators(clonedParentQueryModel);

            clonedParentQueryModel.ResultTypeOverride
                = typeof(IQueryable<>).MakeGenericType(clonedParentQueryModel.SelectClause.Selector.Type);

            var joinQuerySourceReferenceExpression
                = CreateJoinToParentQuery(
                    clonedParentQueryModel,
                    clonedParentQuerySourceReferenceExpression,
                    collectionQuerySourceReferenceExpression,
                    navigation.ForeignKey,
                    collectionQueryModel,
                    subQueryProjection);

            ApplyParentOrderings(
                parentOrderings,
                clonedParentQueryModel,
                querySourceMapping,
                lastResultOperator);

            LiftOrderBy(
                clonedParentQuerySource,
                joinQuerySourceReferenceExpression,
                clonedParentQueryModel,
                collectionQueryModel,
                subQueryProjection);
            
            clonedParentQueryModel.SelectClause.Selector
                = Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        subQueryProjection));

            return collectionQueryModel;
        }

        private static void BuildParentOrderings(
            QueryModel queryModel,
            INavigation navigation,
            Expression querySourceReferenceExpression,
            ICollection<Ordering> parentOrderings)
        {
            var orderings = parentOrderings;

            var orderByClause
                = queryModel.BodyClauses.OfType<OrderByClause>().LastOrDefault();

            if (orderByClause != null)
            {
                orderings = orderings.Concat(orderByClause.Orderings).ToArray();
            }

            foreach (var property in navigation.ForeignKey.PrincipalKey.Properties)
            {
                var propertyExpression
                    = EntityQueryModelVisitor
                        .CreatePropertyExpression(querySourceReferenceExpression, property);

                if (!orderings.Any(o =>
                    _expressionEqualityComparer.Equals(o.Expression, propertyExpression)
                    || o.Expression is MemberExpression memberExpression
                    && memberExpression.Expression == querySourceReferenceExpression
                    && memberExpression.Member.Equals(property.PropertyInfo)))
                {
                    parentOrderings.Add(new Ordering(propertyExpression, OrderingDirection.Asc));
                }
            }
        }

        private void CloneAnnotations(QuerySourceMapping querySourceMapping, QueryModel queryModel)
        {
            var clonedAnnotations = new List<IQueryAnnotation>();

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var annotation
                in _queryCompilationContext.QueryAnnotations.OfType<ICloneableQueryAnnotation>())
            {
                if (querySourceMapping.GetExpression(annotation.QuerySource)
                    is QuerySourceReferenceExpression querySourceReferenceExpression)
                {
                    clonedAnnotations.Add(
                        annotation.Clone(
                            querySourceReferenceExpression.ReferencedQuerySource, queryModel));
                }
            }

            var newAnnotations = _queryCompilationContext.QueryAnnotations.ToList();

            newAnnotations.AddRange(clonedAnnotations);

            _queryCompilationContext.QueryAnnotations = newAnnotations;
        }

        private static void AdjustPredicate(
            QueryModel queryModel,
            IQuerySource parentQuerySource,
            Expression targetParentExpression)
        {
            var querySourcePriorityAnalyzer
                = new QuerySourcePriorityAnalyzer(queryModel.SelectClause.Selector);

            Expression predicate = null;

            if (querySourcePriorityAnalyzer.AreLowerPriorityQuerySources(parentQuerySource))
            {
                predicate
                    = Expression.NotEqual(
                        targetParentExpression,
                        Expression.Constant(null, targetParentExpression.Type));
            }

            predicate
                = querySourcePriorityAnalyzer.GetHigherPriorityQuerySources(parentQuerySource)
                    .Select(qs => new QuerySourceReferenceExpression(qs))
                    .Select(qsre => Expression.Equal(qsre, Expression.Constant(null, qsre.Type)))
                    .Aggregate(
                        predicate,
                        (current, nullCheck)
                            => current == null
                                ? nullCheck
                                : Expression.AndAlso(current, nullCheck));

            if (predicate != null)
            {
                var whereClause = queryModel.BodyClauses.OfType<WhereClause>().LastOrDefault();

                if (whereClause == null)
                {
                    queryModel.BodyClauses.Add(new WhereClause(predicate));
                }
                else
                {
                    whereClause.Predicate = Expression.AndAlso(whereClause.Predicate, predicate);
                }
            }
        }

        private sealed class QuerySourcePriorityAnalyzer : RelinqExpressionVisitor
        {
            private readonly List<IQuerySource> _querySources = new List<IQuerySource>();

            public QuerySourcePriorityAnalyzer(Expression expression)
            {
                Visit(expression);
            }

            public bool AreLowerPriorityQuerySources(IQuerySource querySource)
            {
                var index = _querySources.IndexOf(querySource);

                return index != -1 && index < _querySources.Count - 1;
            }

            public IEnumerable<IQuerySource> GetHigherPriorityQuerySources(IQuerySource querySource)
            {
                var index = _querySources.IndexOf(querySource);

                if (index != -1)
                {
                    for (var i = 0; i < index; i++)
                    {
                        yield return _querySources[i];
                    }
                }
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                IQuerySource querySource;

                if (node.NodeType == ExpressionType.Coalesce
                    && (querySource = ExtractQuerySource(node.Left)) != null)
                {
                    _querySources.Add(querySource);

                    if ((querySource = ExtractQuerySource(node.Right)) != null)
                    {
                        _querySources.Add(querySource);
                    }
                    else
                    {
                        Visit(node.Right);

                        return node;
                    }
                }

                return base.VisitBinary(node);
            }

            private static IQuerySource ExtractQuerySource(Expression expression)
            {
                switch (expression)
                {
                    case QuerySourceReferenceExpression querySourceReferenceExpression:
                        return querySourceReferenceExpression.ReferencedQuerySource;
                    case MethodCallExpression methodCallExpression
                    when IsIncludeMethod(methodCallExpression):
                        return ((QuerySourceReferenceExpression)methodCallExpression.Arguments[1]).ReferencedQuerySource;
                }

                return null;
            }
        }

        private static bool ProcessResultOperators(QueryModel queryModel)
        {
            var choiceResultOperator
                = queryModel.ResultOperators.LastOrDefault() as ChoiceResultOperatorBase;

            var lastResultOperator = false;

            if (choiceResultOperator != null)
            {
                queryModel.ResultOperators.Remove(choiceResultOperator);
                queryModel.ResultOperators.Add(new TakeResultOperator(Expression.Constant(1)));

                lastResultOperator = choiceResultOperator is LastResultOperator;
            }

            foreach (var groupResultOperator
                in queryModel.ResultOperators.OfType<GroupResultOperator>()
                    .ToArray())
            {
                queryModel.ResultOperators.Remove(groupResultOperator);

                var orderByClause1 = queryModel.BodyClauses.OfType<OrderByClause>().LastOrDefault();

                if (orderByClause1 == null)
                {
                    queryModel.BodyClauses.Add(orderByClause1 = new OrderByClause());
                }

                orderByClause1.Orderings.Add(new Ordering(groupResultOperator.KeySelector, OrderingDirection.Asc));
            }

            if (queryModel.BodyClauses
                    .Count(
                        bc => bc is AdditionalFromClause
                              || bc is JoinClause
                              || bc is GroupJoinClause) > 0)
            {
                queryModel.ResultOperators.Add(new DistinctResultOperator());
            }

            return lastResultOperator;
        }

        private static QuerySourceReferenceExpression CreateJoinToParentQuery(
            QueryModel parentQueryModel,
            QuerySourceReferenceExpression parentQuerySourceReferenceExpression,
            Expression outerTargetExpression,
            IForeignKey foreignKey,
            QueryModel targetQueryModel,
            ICollection<Expression> subQueryProjection)
        {
            var subQueryExpression = new SubQueryExpression(parentQueryModel);
            var parentQuerySource = parentQuerySourceReferenceExpression.ReferencedQuerySource;

            var joinClause
                = new JoinClause(
                    "_" + parentQuerySource.ItemName,
                    typeof(AnonymousObject),
                    subQueryExpression,
                    CreateKeyAccessExpression(
                        outerTargetExpression,
                        foreignKey.Properties),
                    Expression.Constant(null));

            var joinQuerySourceReferenceExpression = new QuerySourceReferenceExpression(joinClause);
            var innerKeyExpressions = new List<Expression>();

            foreach (var principalKeyProperty in foreignKey.PrincipalKey.Properties)
            {
                innerKeyExpressions.Add(
                    Expression.Convert(
                        Expression.Call(
                            joinQuerySourceReferenceExpression,
                            AnonymousObject.GetValueMethodInfo,
                            Expression.Constant(subQueryProjection.Count)),
                        principalKeyProperty.ClrType.MakeNullable()));

                subQueryProjection.Add(
                    Expression.Convert(
                        EntityQueryModelVisitor
                            .CreatePropertyExpression(
                                parentQuerySourceReferenceExpression,
                                principalKeyProperty),
                        typeof(object)));
            }

            joinClause.InnerKeySelector
                = innerKeyExpressions.Count == 1
                    ? innerKeyExpressions[0]
                    : Expression.New(
                        AnonymousObject.AnonymousObjectCtor,
                        Expression.NewArrayInit(
                            typeof(object),
                            innerKeyExpressions.Select(e => Expression.Convert(e, typeof(object)))));

            targetQueryModel.BodyClauses.Add(joinClause);

            return joinQuerySourceReferenceExpression;
        }

        // TODO: Unify this with other versions
        private static Expression CreateKeyAccessExpression(Expression target, IReadOnlyList<IProperty> properties)
            => properties.Count == 1
                ? EntityQueryModelVisitor
                    .CreatePropertyExpression(target, properties[0])
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(
                                p =>
                                    Expression.Convert(
                                        EntityQueryModelVisitor.CreatePropertyExpression(target, p),
                                        typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));

        private static void ApplyParentOrderings(
            IEnumerable<Ordering> parentOrderings,
            QueryModel queryModel,
            QuerySourceMapping querySourceMapping,
            bool reverseOrdering)
        {
            var orderByClause = queryModel.BodyClauses.OfType<OrderByClause>().LastOrDefault();

            if (orderByClause == null)
            {
                queryModel.BodyClauses.Add(orderByClause = new OrderByClause());
            }

            foreach (var ordering in parentOrderings)
            {
                var newExpression
                    = CloningExpressionVisitor
                        .AdjustExpressionAfterCloning(ordering.Expression, querySourceMapping);

                orderByClause.Orderings
                    .Add(new Ordering(newExpression, ordering.OrderingDirection));
            }

            if (reverseOrdering)
            {
                foreach (var ordering in orderByClause.Orderings)
                {
                    ordering.OrderingDirection
                        = ordering.OrderingDirection == OrderingDirection.Asc
                            ? OrderingDirection.Desc
                            : OrderingDirection.Asc;
                }
            }
        }

        private static void LiftOrderBy(
            IQuerySource querySource,
            Expression targetExpression,
            QueryModel fromQueryModel,
            QueryModel toQueryModel,
            List<Expression> subQueryProjection)
        {
            var canRemove
                = !fromQueryModel.ResultOperators
                    .Any(r => r is SkipResultOperator || r is TakeResultOperator);

            foreach (var orderByClause
                in fromQueryModel.BodyClauses.OfType<OrderByClause>().ToArray())
            {
                var outerOrderByClause = new OrderByClause();
                
                foreach (var ordering in orderByClause.Orderings)
                {
                    int projectionIndex;

                    if (ordering.Expression is MemberExpression memberExpression
                        && memberExpression.Expression is QuerySourceReferenceExpression memberQsre
                        && memberQsre.ReferencedQuerySource == querySource)
                    {
                        projectionIndex
                            = subQueryProjection
                                .FindIndex(
                                    e =>
                                        {
                                            var propertyExpression = (MethodCallExpression)e.RemoveConvert();
                                            var properyQsre = (QuerySourceReferenceExpression)propertyExpression.Arguments[0];
                                            var propertyName = (string)((ConstantExpression)propertyExpression.Arguments[1]).Value;

                                            return properyQsre.ReferencedQuerySource == memberQsre.ReferencedQuerySource
                                                   && propertyName == memberExpression.Member.Name;
                                        });
                    }
                    else 
                    {
                        projectionIndex
                            = subQueryProjection
                                .FindIndex(e => _expressionEqualityComparer.Equals(e.RemoveConvert(), ordering.Expression));
                    }

                    if (projectionIndex == -1)
                    {
                        projectionIndex = subQueryProjection.Count;

                        subQueryProjection.Add(Expression.Convert(ordering.Expression, typeof(object)));
                    }
                    
                    var newExpression
                        = Expression.Call(
                            targetExpression,
                            AnonymousObject.GetValueMethodInfo,
                            Expression.Constant(projectionIndex));

                    outerOrderByClause.Orderings
                        .Add(new Ordering(newExpression, ordering.OrderingDirection));
                }

                toQueryModel.BodyClauses.Add(outerOrderByClause);

                if (canRemove)
                {
                    fromQueryModel.BodyClauses.Remove(orderByClause);
                }
            }
        }

        private Expression BuildCollectionIncludeExpressions(
            INavigation navigation,
            Expression targetEntityExpression,
            bool trackingQuery,
            Expression relatedCollectionFuncExpression,
            MethodInfo includeCollectionMethodInfo,
            Expression cancellationTokenExpression)
        {
            var inverseNavigation = navigation.FindInverse();

            var arguments = new List<Expression>
            {
                Expression.Constant(_collectionIncludeId++),
                Expression.Constant(navigation),
                Expression.Constant(inverseNavigation, typeof(INavigation)),
                Expression.Constant(navigation.GetTargetType()),
                Expression.Constant(navigation.GetCollectionAccessor()),
                Expression.Constant(inverseNavigation?.GetSetter(), typeof(IClrPropertySetter)),
                Expression.Constant(trackingQuery),
                targetEntityExpression,
                relatedCollectionFuncExpression
            };

            if (cancellationTokenExpression != null)
            {
                arguments.Add(cancellationTokenExpression);
            }

            return Expression.Call(
                Expression.Property(
                    EntityQueryModelVisitor.QueryContextParameter,
                    nameof(QueryContext.QueryBuffer)),
                includeCollectionMethodInfo,
                arguments);
        }

        private IEnumerable<IncludeSpecification> CreateIncludeSpecifications(
            QueryModel queryModel,
            IEnumerable<IncludeResultOperator> includeResultOperators)
        {
            var querySourceTracingExpressionVisitor
                = _querySourceTracingExpressionVisitorFactory.Create();

            return includeResultOperators
                .Select(
                    includeResultOperator =>
                        {
                            var entityType
                                = _queryCompilationContext.Model
                                    .FindEntityType(includeResultOperator.PathFromQuerySource.Type);

                            var parts = includeResultOperator.NavigationPropertyPaths.ToArray();
                            var navigationPath = new INavigation[parts.Length];

                            for (var i = 0; i < parts.Length; i++)
                            {
                                navigationPath[i] = entityType.FindNavigation(parts[i]);

                                if (navigationPath[i] == null)
                                {
                                    throw new InvalidOperationException(
                                        CoreStrings.IncludeBadNavigation(parts[i], entityType.DisplayName()));
                                }

                                entityType = navigationPath[i].GetTargetType();
                            }

                            var querySourceReferenceExpression
                                = querySourceTracingExpressionVisitor
                                    .FindResultQuerySourceReferenceExpression(
                                        queryModel.SelectClause.Selector,
                                        includeResultOperator.QuerySource);

                            if (querySourceReferenceExpression == null)
                            {
                                _queryCompilationContext.Logger
                                    .LogWarning(
                                        CoreEventId.IncludeIgnoredWarning,
                                        () => CoreStrings.LogIgnoredInclude(
                                            $"{includeResultOperator.QuerySource.ItemName}.{navigationPath.Select(n => n.Name).Join(".")}"));
                            }

                            return new IncludeSpecification(
                                includeResultOperator,
                                querySourceReferenceExpression,
                                navigationPath);
                        })
                .Where(
                    a =>
                        {
                            if (a.QuerySourceReferenceExpression == null)
                            {
                                return false;
                            }

                            var sequenceType = a.QuerySourceReferenceExpression.Type.TryGetSequenceType();

                            if (sequenceType != null
                                && _queryCompilationContext.Model.FindEntityType(sequenceType) != null)
                            {
                                return false;
                            }

                            return !a.NavigationPath.Any(n => n.IsCollection())
                                   || a.NavigationPath.Length == 1;
                        })
                .ToArray();
        }

        private static Expression BuildIncludeExpressions(
            IReadOnlyList<INavigation> navigationPath,
            Expression targetEntityExpression,
            bool trackingQuery,
            ref int includedIndex,
            int navigationIndex)
        {
            var navigation = navigationPath[navigationIndex];

            var relatedArrayAccessExpression
                = Expression.ArrayAccess(_includedParameter, Expression.Constant(includedIndex++));

            var relatedEntityExpression
                = Expression.Convert(relatedArrayAccessExpression, navigation.ClrType);

            var stateManagerProperty
                = Expression.Property(
                    Expression.Property(
                        EntityQueryModelVisitor.QueryContextParameter,
                        nameof(QueryContext.StateManager)),
                    nameof(Lazy<object>.Value));

            var blockExpressions = new List<Expression>();

            if (trackingQuery)
            {
                blockExpressions.Add(
                    Expression.Call(
                        Expression.Property(
                            EntityQueryModelVisitor.QueryContextParameter,
                            nameof(QueryContext.QueryBuffer)),
                        _queryBufferStartTrackingMethodInfo,
                        relatedArrayAccessExpression,
                        Expression.Constant(navigation.GetTargetType())));

                blockExpressions.Add(
                    Expression.Call(
                        _setRelationshipSnapshotValueMethodInfo,
                        stateManagerProperty,
                        Expression.Constant(navigation),
                        targetEntityExpression,
                        relatedArrayAccessExpression));
            }
            else
            {
                blockExpressions.Add(
                    Expression.Assign(
                        Expression.MakeMemberAccess(
                            targetEntityExpression,
                            navigation.GetMemberInfo(false, true)),
                        relatedEntityExpression));
            }

            var inverseNavigation = navigation.FindInverse();

            if (inverseNavigation != null)
            {
                var collection = inverseNavigation.IsCollection();

                if (trackingQuery)
                {
                    blockExpressions.Add(
                        Expression.Call(
                            collection
                                ? _addToCollectionSnapshotMethodInfo
                                : _setRelationshipSnapshotValueMethodInfo,
                            stateManagerProperty,
                            Expression.Constant(inverseNavigation),
                            relatedArrayAccessExpression,
                            targetEntityExpression));
                }
                else
                {
                    blockExpressions.Add(
                        collection
                            ? (Expression)Expression.Call(
                                Expression.Constant(inverseNavigation.GetCollectionAccessor()),
                                _collectionAccessorAddMethodInfo,
                                relatedArrayAccessExpression,
                                targetEntityExpression)
                            : Expression.Assign(
                                Expression.MakeMemberAccess(
                                    relatedEntityExpression,
                                    inverseNavigation
                                        .GetMemberInfo(forConstruction: false, forSet: true)),
                                targetEntityExpression));
                }
            }

            if (navigationIndex < navigationPath.Count - 1)
            {
                blockExpressions.Add(
                    BuildIncludeExpressions(
                        navigationPath,
                        relatedEntityExpression,
                        trackingQuery,
                        ref includedIndex,
                        navigationIndex + 1));
            }

            return
                Expression.IfThen(
                    Expression.Not(
                        Expression.Call(
                            _referenceEqualsMethodInfo,
                            relatedArrayAccessExpression,
                            Expression.Constant(null, typeof(object)))),
                    Expression.Block(typeof(void), blockExpressions));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsIncludeMethod([NotNull] MethodCallExpression methodCallExpression)
            => methodCallExpression.Method.MethodIsClosedFormOf(_includeMethodInfo)
              || methodCallExpression.Method.MethodIsClosedFormOf(_includeAsyncMethodInfo);

        private static readonly MethodInfo _includeMethodInfo
            = typeof(IncludeCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(_Include));

        // ReSharper disable once InconsistentNaming
        private static TEntity _Include<TEntity>(
            QueryContext queryContext,
            TEntity entity,
            object[] included,
            Action<QueryContext, TEntity, object[]> fixup)
        {
            if (entity != null)
            {
                fixup(queryContext, entity, included);
            }

            return entity;
        }

        private static readonly MethodInfo _includeAsyncMethodInfo
            = typeof(IncludeCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(_IncludeAsync));

        // ReSharper disable once InconsistentNaming
        private static async Task<TEntity> _IncludeAsync<TEntity>(
            QueryContext queryContext,
            TEntity entity,
            object[] included,
            Func<QueryContext, TEntity, object[], CancellationToken, Task> fixup,
            CancellationToken cancellationToken)
        {
            if (entity != null)
            {
                await fixup(queryContext, entity, included, cancellationToken);
            }

            return entity;
        }

        private static readonly MethodInfo _setRelationshipSnapshotValueMethodInfo
            = typeof(IncludeCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(SetRelationshipSnapshotValue));

        private static void SetRelationshipSnapshotValue(
            IStateManager stateManager,
            IPropertyBase navigation,
            object entity,
            object value)
        {
            var internalEntityEntry = stateManager.TryGetEntry(entity);

            Debug.Assert(internalEntityEntry != null);

            internalEntityEntry.SetRelationshipSnapshotValue(navigation, value);
        }

        private static readonly MethodInfo _addToCollectionSnapshotMethodInfo
            = typeof(IncludeCompiler).GetTypeInfo()
                .GetDeclaredMethod(nameof(AddToCollectionSnapshot));

        private static void AddToCollectionSnapshot(
            IStateManager stateManager,
            IPropertyBase navigation,
            object entity,
            object value)
        {
            var internalEntityEntry = stateManager.TryGetEntry(entity);

            Debug.Assert(internalEntityEntry != null);

            internalEntityEntry.AddToCollectionSnapshot(navigation, value);
        }
    }
}
