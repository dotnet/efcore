// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class InMemoryQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly IMaterializerFactory _materializerFactory;

        public InMemoryQueryModelVisitor(
            [NotNull] IQueryOptimizer queryOptimizer,
            [NotNull] INavigationRewritingExpressionVisitorFactory navigationRewritingExpressionVisitorFactory,
            [NotNull] ISubQueryMemberPushDownExpressionVisitor subQueryMemberPushDownExpressionVisitor,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory,
            [NotNull] IEntityResultFindingExpressionVisitorFactory entityResultFindingExpressionVisitorFactory,
            [NotNull] ITaskBlockingExpressionVisitor taskBlockingExpressionVisitor,
            [NotNull] IMemberAccessBindingExpressionVisitorFactory memberAccessBindingExpressionVisitorFactory,
            [NotNull] IOrderingExpressionVisitorFactory orderingExpressionVisitorFactory,
            [NotNull] IProjectionExpressionVisitorFactory projectionExpressionVisitorFactory,
            [NotNull] IEntityQueryableExpressionVisitorFactory entityQueryableExpressionVisitorFactory,
            [NotNull] IQueryAnnotationExtractor queryAnnotationExtractor,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IExpressionPrinter expressionPrinter,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(
                Check.NotNull(queryOptimizer, nameof(queryOptimizer)),
                Check.NotNull(navigationRewritingExpressionVisitorFactory, nameof(navigationRewritingExpressionVisitorFactory)),
                Check.NotNull(subQueryMemberPushDownExpressionVisitor, nameof(subQueryMemberPushDownExpressionVisitor)),
                Check.NotNull(querySourceTracingExpressionVisitorFactory, nameof(querySourceTracingExpressionVisitorFactory)),
                Check.NotNull(entityResultFindingExpressionVisitorFactory, nameof(entityResultFindingExpressionVisitorFactory)),
                Check.NotNull(taskBlockingExpressionVisitor, nameof(taskBlockingExpressionVisitor)),
                Check.NotNull(memberAccessBindingExpressionVisitorFactory, nameof(memberAccessBindingExpressionVisitorFactory)),
                Check.NotNull(orderingExpressionVisitorFactory, nameof(orderingExpressionVisitorFactory)),
                Check.NotNull(projectionExpressionVisitorFactory, nameof(projectionExpressionVisitorFactory)),
                Check.NotNull(entityQueryableExpressionVisitorFactory, nameof(entityQueryableExpressionVisitorFactory)),
                Check.NotNull(queryAnnotationExtractor, nameof(queryAnnotationExtractor)),
                Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(expressionPrinter, nameof(expressionPrinter)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)))
        {
            Check.NotNull(materializerFactory, nameof(materializerFactory));

            _materializerFactory = materializerFactory;
        }

        protected override void IncludeNavigations(
            IncludeSpecification includeSpecification,
            Type resultType,
            LambdaExpression accessorLambda,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));
            Check.NotNull(accessorLambda, nameof(accessorLambda));

            MethodInfo includeMethod;

            var resultItemTypeInfo = resultType.GetTypeInfo();

            if (resultItemTypeInfo.IsGenericType
                && (resultItemTypeInfo.GetGenericTypeDefinition() == typeof(IGrouping<,>)
                    || resultItemTypeInfo.GetGenericTypeDefinition() == typeof(IAsyncGrouping<,>)))
            {
                includeMethod
                    = _includeGroupedMethodInfo.MakeGenericMethod(
                        resultType.GenericTypeArguments[0],
                        resultType.GenericTypeArguments[1]);
            }
            else
            {
                includeMethod = _includeMethodInfo.MakeGenericMethod(resultType);
            }

            Expression
                = Expression.Call(
                    includeMethod,
                    QueryContextParameter,
                    Expression,
                    Expression.Constant(includeSpecification),
                    accessorLambda,
                    Expression.Constant(
                        includeSpecification.NavigationPath
                            .Select(n =>
                                {
                                    var targetType = n.GetTargetType();
                                    var materializer = _materializerFactory.CreateMaterializer(targetType);

                                    return new RelatedEntitiesLoader(targetType, materializer.Compile());
                                })
                            .ToArray()),
                    Expression.Constant(querySourceRequiresTracking));
        }

        private sealed class RelatedEntitiesLoader : IRelatedEntitiesLoader
        {
            private readonly IEntityType _targetType;
            private readonly Func<IEntityType, ValueBuffer, object> _materializer;

            public RelatedEntitiesLoader(IEntityType targetType, Func<IEntityType, ValueBuffer, object> materializer)
            {
                _targetType = targetType;
                _materializer = materializer;
            }

            public IEnumerable<EntityLoadInfo> Load(QueryContext queryContext, IIncludeKeyComparer keyComparer)
            {
                return ((InMemoryQueryContext)queryContext).Store
                    .GetTables(_targetType)
                    .SelectMany(t =>
                        t.Rows.Select(vs => new EntityLoadInfo(
                            new ValueBuffer(vs), vb => _materializer(t.EntityType, vb)))
                            .Where(eli => keyComparer.ShouldInclude(eli.ValueBuffer)));
            }

            public void Dispose()
            {
                // no-op
            }
        }

        private static readonly MethodInfo _includeMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Include));

        [UsedImplicitly]
        private static IEnumerable<TResult> Include<TResult>(
            QueryContext queryContext,
            IEnumerable<TResult> source,
            IncludeSpecification includeSpecification,
            Func<TResult, object> accessorLambda,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            foreach (var result in source)
            {
                var entityOrCollection = accessorLambda.Invoke(result);

                if (includeSpecification.IsEnumerableTarget)
                {
                    foreach (var entity in (IEnumerable)entityOrCollection)
                    {
                        queryContext.QueryBuffer
                            .Include(
                                queryContext,
                                entity,
                                includeSpecification.NavigationPath,
                                relatedEntitiesLoaders,
                                querySourceRequiresTracking);
                    }
                }
                else
                {
                    queryContext.QueryBuffer
                        .Include(
                            queryContext,
                            entityOrCollection,
                            includeSpecification.NavigationPath,
                            relatedEntitiesLoaders,
                            querySourceRequiresTracking);
                }

                yield return result;
            }
        }

        private static readonly MethodInfo _includeGroupedMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IncludeGrouped));

        [UsedImplicitly]
        private static IEnumerable<IGrouping<TKey, TOut>> IncludeGrouped<TKey, TOut>(
            QueryContext queryContext,
            IEnumerable<IGrouping<TKey, TOut>> groupings,
            IncludeSpecification includeSpecification,
            Func<TOut, object> accessorLambda,
            IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            return groupings.Select(g =>
                new IncludeGrouping<TKey, TOut>(
                    queryContext,
                    g,
                    includeSpecification.NavigationPath,
                    accessorLambda,
                    relatedEntitiesLoaders,
                    querySourceRequiresTracking));
        }

        private class IncludeGrouping<TKey, TOut> : IGrouping<TKey, TOut>
        {
            private readonly QueryContext _queryContext;
            private readonly IGrouping<TKey, TOut> _grouping;
            private readonly IReadOnlyList<INavigation> _navigationPath;
            private readonly Func<TOut, object> _accessorLambda;
            private readonly IReadOnlyList<RelatedEntitiesLoader> _relatedEntitiesLoaders;
            private readonly bool _querySourceRequiresTracking;

            public IncludeGrouping(
                QueryContext queryContext,
                IGrouping<TKey, TOut> grouping,
                IReadOnlyList<INavigation> navigationPath,
                Func<TOut, object> accessorLambda,
                IReadOnlyList<RelatedEntitiesLoader> relatedEntitiesLoaders,
                bool querySourceRequiresTracking)
            {
                _queryContext = queryContext;
                _grouping = grouping;
                _navigationPath = navigationPath;
                _accessorLambda = accessorLambda;
                _relatedEntitiesLoaders = relatedEntitiesLoaders;
                _querySourceRequiresTracking = querySourceRequiresTracking;
            }

            public TKey Key => _grouping.Key;

            public IEnumerator<TOut> GetEnumerator()
            {
                foreach (var result in _grouping)
                {
                    _queryContext.QueryBuffer
                        .Include(
                            _queryContext,
                            _accessorLambda.Invoke(result),
                            _navigationPath,
                            _relatedEntitiesLoaders,
                            _querySourceRequiresTracking);

                    yield return result;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static readonly MethodInfo EntityQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(EntityQuery));

        [UsedImplicitly]
        private static IEnumerable<TEntity> EntityQuery<TEntity>(
            QueryContext queryContext,
            IEntityType entityType,
            IKey key,
            Func<IEntityType, ValueBuffer, object> materializer,
            bool queryStateManager)
            where TEntity : class
        {
            return ((InMemoryQueryContext)queryContext).Store
                .GetTables(entityType)
                .SelectMany(t =>
                    t.Rows.Select(vs =>
                        {
                            var valueBuffer = new ValueBuffer(vs);

                            return (TEntity)queryContext
                                .QueryBuffer
                                .GetEntity(
                                    key,
                                    new EntityLoadInfo(
                                        valueBuffer,
                                        vr => materializer(t.EntityType, vr)),
                                    queryStateManager,
                                    throwOnNullKey: false);
                        }));
        }

        public static readonly MethodInfo ProjectionQueryMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(ProjectionQuery));

        [UsedImplicitly]
        private static IEnumerable<ValueBuffer> ProjectionQuery(
            QueryContext queryContext,
            IEntityType entityType)
        {
            return ((InMemoryQueryContext)queryContext).Store
                .GetTables(entityType)
                .SelectMany(t => t.Rows.Select(vs => new ValueBuffer(vs)));
        }
    }
}
