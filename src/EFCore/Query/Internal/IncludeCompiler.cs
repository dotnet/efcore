// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public partial class IncludeCompiler
    {
        private static readonly MethodInfo _queryBufferStartTrackingMethodInfo
            = typeof(IQueryBuffer).GetTypeInfo()
                .GetDeclaredMethods(nameof(IQueryBuffer.StartTracking))
                .Single(mi => mi.GetParameters()[1].ParameterType == typeof(IEntityType));

        private static readonly ParameterExpression _includedParameter
            = Expression.Parameter(typeof(object[]), name: "included");

        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;
        private readonly List<IncludeResultOperator> _includeResultOperators;

        private QueryModel _targetQueryModel;

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

            _includeResultOperators
                = _queryCompilationContext.QueryAnnotations
                    .OfType<IncludeResultOperator>()
                    .ToList();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void CompileIncludes(
            [NotNull] QueryModel queryModel,
            bool trackingQuery,
            bool asyncQuery,
            bool shouldThrow)
        {
            if (queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                return;
            }

            _targetQueryModel = _targetQueryModel ?? queryModel;

            foreach (var includeLoadTree in CreateIncludeLoadTrees(queryModel, shouldThrow))
            {
                includeLoadTree.Compile(
                    _queryCompilationContext,
                    _targetQueryModel,
                    trackingQuery,
                    asyncQuery,
                    ref _collectionIncludeId);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RewriteCollectionQueries()
        {
            if (_targetQueryModel != null)
            {
                RewriteCollectionQueries(_targetQueryModel);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void RewriteCollectionQueries([NotNull] QueryModel queryModel)
        {
            var collectionQueryModelRewritingExpressionVisitor
                = new CollectionQueryModelRewritingExpressionVisitor(_queryCompilationContext, queryModel, this);

            queryModel.TransformExpressions(collectionQueryModelRewritingExpressionVisitor.Visit);

            ApplyParentOrderings(queryModel, collectionQueryModelRewritingExpressionVisitor.ParentOrderings);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void LogIgnoredIncludes()
        {
            foreach (var includeResultOperator in _includeResultOperators.Where(iro => !iro.IsImplicitLoad))
            {
                _queryCompilationContext.Logger.IncludeIgnoredWarning(includeResultOperator);
            }
        }

        private IEnumerable<IncludeLoadTree> CreateIncludeLoadTrees(QueryModel queryModel, bool shouldThrow)
        {
            var querySourceTracingExpressionVisitor
                = _querySourceTracingExpressionVisitorFactory.Create();

            var includeLoadTrees = new List<IncludeLoadTree>();

            foreach (var includeResultOperator in _includeResultOperators.ToArray())
            {
                var querySourceReferenceExpression
                    = querySourceTracingExpressionVisitor
                        .FindResultQuerySourceReferenceExpression(
                            queryModel.GetOutputExpression(),
                            includeResultOperator.QuerySource);

                if (querySourceReferenceExpression == null)
                {
                    continue;
                }

                if (querySourceReferenceExpression.Type.IsGrouping()
                    && querySourceTracingExpressionVisitor.OriginGroupByQueryModel != null)
                {
                    querySourceReferenceExpression
                        = querySourceTracingExpressionVisitor
                            .FindResultQuerySourceReferenceExpression(
                                querySourceTracingExpressionVisitor.OriginGroupByQueryModel.GetOutputExpression(),
                                includeResultOperator.QuerySource);

                    _targetQueryModel = querySourceTracingExpressionVisitor.OriginGroupByQueryModel;
                }

                if (querySourceReferenceExpression?.Type.IsGrouping() != false)
                {
                    continue;
                }

                var includeLoadTree
                    = includeLoadTrees
                        .SingleOrDefault(
                            t => ReferenceEquals(
                                t.QuerySourceReferenceExpression, querySourceReferenceExpression));

                if (includeLoadTree == null)
                {
                    includeLoadTrees.Add(includeLoadTree = new IncludeLoadTree(querySourceReferenceExpression));
                }

                if (!TryPopulateIncludeLoadTree(includeResultOperator, includeLoadTree, shouldThrow))
                {
                    includeLoadTrees.Remove(includeLoadTree);
                    continue;
                }

                _queryCompilationContext.Logger.NavigationIncluded(includeResultOperator);
                _includeResultOperators.Remove(includeResultOperator);
            }

            return includeLoadTrees;
        }

        private bool TryPopulateIncludeLoadTree(
            IncludeResultOperator includeResultOperator,
            IncludeLoadTree includeLoadTree,
            bool shouldThrow)
        {
            if (includeResultOperator.NavigationPaths != null)
            {
                foreach (var navigationPath in includeResultOperator.NavigationPaths)
                {
                    includeLoadTree.AddLoadPath(navigationPath);
                }

                return true;
            }

            IEntityType entityType = null;
            if (includeResultOperator.PathFromQuerySource is QuerySourceReferenceExpression qsre)
            {
                entityType = _queryCompilationContext.FindEntityType(qsre.ReferencedQuerySource);
            }

            if (entityType == null)
            {
                entityType = _queryCompilationContext.Model.FindEntityType(includeResultOperator.PathFromQuerySource.Type);

                if (entityType == null)
                {
                    var pathFromSource = MemberAccessBindingExpressionVisitor.GetPropertyPath(
                        includeResultOperator.PathFromQuerySource, _queryCompilationContext, out qsre);

                    if (pathFromSource.Count > 0
                        && pathFromSource[pathFromSource.Count - 1] is INavigation navigation)
                    {
                        entityType = navigation.GetTargetType();
                    }
                }
            }

            if (entityType == null)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                            includeResultOperator.ToString(),
                            includeResultOperator.NavigationPropertyPaths.FirstOrDefault()));
                }

                return false;
            }

            return WalkNavigations(entityType, includeResultOperator.NavigationPropertyPaths, includeLoadTree, shouldThrow);
        }

        private static bool WalkNavigations(
            IEntityType entityType,
            IReadOnlyList<string> navigationPropertyPaths,
            IncludeLoadTree includeLoadTree,
            bool shouldThrow)
        {
            var longestMatchFound
                = WalkNavigationsInternal(
                    entityType,
                    navigationPropertyPaths,
                    includeLoadTree,
                    new Stack<INavigation>(),
                    (0, entityType));

            if (longestMatchFound.Depth < navigationPropertyPaths.Count)
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncludeBadNavigation(
                            navigationPropertyPaths[longestMatchFound.Depth],
                            longestMatchFound.EntityType.DisplayName()));
                }

                return false;
            }

            return true;
        }

        private static (int Depth, IEntityType EntityType) WalkNavigationsInternal(
            IEntityType entityType,
            IReadOnlyList<string> navigationPropertyPaths,
            IncludeLoadTree includeLoadTree,
            Stack<INavigation> stack,
            (int Depth, IEntityType EntityType) longestMatchFound)
        {
            var outboundNavigations
                = entityType.GetNavigations()
                    .Concat(entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations()))
                    .Where(n => navigationPropertyPaths.Count > stack.Count && n.Name == navigationPropertyPaths[stack.Count])
                    .ToList();

            if (outboundNavigations.Count == 0
                && stack.Count > 0)
            {
                includeLoadTree.AddLoadPath(stack.Reverse().ToArray());

                if (stack.Count > longestMatchFound.Depth)
                {
                    longestMatchFound = (stack.Count, entityType);
                }
            }
            else
            {
                foreach (var navigation in outboundNavigations)
                {
                    stack.Push(navigation);

                    longestMatchFound
                        = WalkNavigationsInternal(
                            navigation.GetTargetType(),
                            navigationPropertyPaths,
                            includeLoadTree,
                            stack,
                            longestMatchFound);

                    stack.Pop();
                }
            }

            return longestMatchFound;
        }

        private static void ApplyParentOrderings(
            QueryModel queryModel,
            IReadOnlyCollection<Ordering> parentOrderings)
        {
            if (parentOrderings.Count > 0)
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    }
}
