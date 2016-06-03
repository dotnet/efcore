// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
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
            Expression accessorExpression,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(includeSpecification, nameof(includeSpecification));
            Check.NotNull(resultType, nameof(resultType));

            var includeExpressionVisitor = new InMemoryIncludeExpressionVisitor(
                _materializerFactory,
                QueryCompilationContext,
                LinqOperatorProvider,
                includeSpecification,
                accessorExpression,
                querySourceRequiresTracking);

            Expression = includeExpressionVisitor.Visit(Expression);
        }

        private sealed class InMemoryIncludeExpressionVisitor : ExpressionVisitorBase
        {
            private readonly IncludeSpecification _includeSpecification;
            private readonly IMaterializerFactory _materializerFactory;
            private readonly QueryCompilationContext _queryCompilationContext;
            private readonly ILinqOperatorProvider _linqOperatorProvider;
            private readonly Expression _accessorExpression;
            private readonly bool _querySourceRequiresTracking;

            public InMemoryIncludeExpressionVisitor(
                [NotNull] IMaterializerFactory materializerFactory,
                [NotNull] QueryCompilationContext queryCompilationContext,
                [NotNull] ILinqOperatorProvider linqOperatorProvider,
                [NotNull] IncludeSpecification includeSpecification,
                [NotNull] Expression accessorExpression,
                bool querySourceRequiresTracking)
            {
                _materializerFactory = materializerFactory;
                _queryCompilationContext = queryCompilationContext;
                _linqOperatorProvider = linqOperatorProvider;
                _includeSpecification = includeSpecification;
                _accessorExpression = accessorExpression;
                _querySourceRequiresTracking = querySourceRequiresTracking;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.MethodIsClosedFormOf(_linqOperatorProvider.Select))
                {
                    var selectorIncludeInjectingVisitor = new SelectorIncludeInjectingExpressionVisitor(
                        _includeSpecification, 
                        _materializerFactory, 
                        _queryCompilationContext, 
                        _accessorExpression, 
                        _querySourceRequiresTracking);

                    var newSelector = selectorIncludeInjectingVisitor.Visit(node.Arguments[1]);

                    return node.Update(node.Object, new[] { node.Arguments[0], newSelector });
                }

                if (node.Method.MethodIsClosedFormOf(EntityQueryMethodInfo) || node.Method.MethodIsClosedFormOf(OfTypeMethodInfo))
                {
                    return ApplyTopLevelInclude(node);
                }

                return base.VisitMethodCall(node);
            }

            private Expression ApplyTopLevelInclude(MethodCallExpression methodCallExpression)
            {
                var elementType = methodCallExpression.Type.GetGenericArguments().First();
                var includeMethod = _includeMethodInfo.MakeGenericMethod(elementType);

                var result = Expression.Call(
                    includeMethod,
                    QueryContextParameter,
                    methodCallExpression,
                    Expression.Constant(_includeSpecification),
                    Expression.Constant(
                        _includeSpecification.NavigationPath
                            .Select(n =>
                            {
                                var targetType = n.GetTargetType();
                                var materializer = _materializerFactory.CreateMaterializer(targetType);

                                return new RelatedEntitiesLoader(targetType, materializer.Compile());
                            })
                            .ToArray()),
                    Expression.Constant(_querySourceRequiresTracking));

                return result;
            }

            private class SelectorIncludeInjectingExpressionVisitor : ExpressionVisitorBase
            {
                private readonly IncludeSpecification _includeSpecification;
                private readonly IMaterializerFactory _materializerFactory;
                private readonly QueryCompilationContext _queryCompilationContext;
                private readonly Expression _accessorExpression;
                private readonly bool _querySourceRequiresTracking;

                public SelectorIncludeInjectingExpressionVisitor(
                    [NotNull] IncludeSpecification includeSpecification,
                    [NotNull] IMaterializerFactory materializerFactory,
                    [NotNull] QueryCompilationContext queryCompilationContext,
                    [NotNull] Expression accessorExpression,
                    bool querySourceRequiresTracking)
                {
                    _includeSpecification = includeSpecification;
                    _materializerFactory = materializerFactory;
                    _queryCompilationContext = queryCompilationContext;
                    _accessorExpression = accessorExpression;
                    _querySourceRequiresTracking = querySourceRequiresTracking;
                }

                public override Expression Visit([CanBeNull] Expression node)
                {
                    if (node == _accessorExpression)
                    {
                        var includeMethod = _includeSpecification.IsEnumerableTarget
                            ? _includeCollectionMethodInfo.MakeGenericMethod(node.Type)
                            : _includeEntityMethodInfo.MakeGenericMethod(node.Type);

                        var result = Expression.Call(
                            includeMethod,
                            QueryContextParameter,
                            node,
                            Expression.Constant(_includeSpecification),
                            Expression.Constant(
                                _includeSpecification.NavigationPath
                                    .Select(n =>
                                    {
                                        var targetType = n.GetTargetType();
                                        var materializer = _materializerFactory.CreateMaterializer(targetType);

                                        return new RelatedEntitiesLoader(targetType, materializer.Compile());
                                    })
                                    .ToArray()),
                            Expression.Constant(_querySourceRequiresTracking));

                        return result;
                    }

                    return base.Visit(node);
                }

                protected override Expression VisitLambda<T>(Expression<T> node)
                {
                    var newBody = Visit(node.Body);

                    return node.Update(newBody, node.Parameters);
                }
            }
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

        private static readonly MethodInfo _includeEntityMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IncludeEntity));

        private static readonly MethodInfo _includeCollectionMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(IncludeCollection));

        private static TEntity IncludeEntity<TEntity>(
            QueryContext queryContext,
            TEntity source,
            IncludeSpecification includeSpecification,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            queryContext.QueryBuffer
                .Include(
                    queryContext,
                    source,
                    includeSpecification.NavigationPath,
                    relatedEntitiesLoaders,
                    querySourceRequiresTracking);

            return source;
        }

        [UsedImplicitly]
        private static TEntity IncludeCollection<TEntity>(
            QueryContext queryContext,
            TEntity source,
            IncludeSpecification includeSpecification,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            foreach (var entity in (IEnumerable)source)
            {
                queryContext.QueryBuffer
                    .Include(
                        queryContext,
                        entity,
                        includeSpecification.NavigationPath,
                        relatedEntitiesLoaders,
                        querySourceRequiresTracking);
            }

            return source;
        }

        private static readonly MethodInfo _includeMethodInfo
            = typeof(InMemoryQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(Include));

        [UsedImplicitly]
        private static IEnumerable<TResult> Include<TResult>(
            QueryContext queryContext,
            IEnumerable<TResult> source,
            IncludeSpecification includeSpecification,
            IReadOnlyList<IRelatedEntitiesLoader> relatedEntitiesLoaders,
            bool querySourceRequiresTracking)
        {
            foreach (var result in source)
            {
                if (includeSpecification.IsEnumerableTarget)
                {
                    foreach (var entity in (IEnumerable)result)
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
                            result,
                            includeSpecification.NavigationPath,
                            relatedEntitiesLoaders,
                            querySourceRequiresTracking);
                }

                yield return result;
            }
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

        public static readonly MethodInfo OfTypeMethodInfo
            = typeof(Enumerable).GetTypeInfo()
            .GetDeclaredMethod(nameof(Enumerable.OfType));

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
