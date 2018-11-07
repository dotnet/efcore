// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Transformations;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryOptimizer : SubQueryFromClauseFlattener, IQueryOptimizer
    {
        private class FromClauseData : IFromClause
        {
            public string ItemName { get; }

            public Type ItemType { get; }

            public Expression FromExpression { get; }

            public FromClauseData(string itemName, Type itemType, Expression fromExpression)
            {
                ItemName = itemName;
                ItemType = itemType;
                FromExpression = fromExpression;
            }

            void IClause.TransformExpressions(Func<Expression, Expression> transformation)
            {
                throw new NotSupportedException();
            }

            void IFromClause.CopyFromSource(IFromClause source)
            {
                throw new NotSupportedException();
            }
        }

        private QueryCompilationContext _queryCompilationContext;

        private readonly TransformingQueryModelExpressionVisitor<QueryOptimizer> _transformingExpressionVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryOptimizer()
        {
            _transformingExpressionVisitor = new TransformingQueryModelExpressionVisitor<QueryOptimizer>(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Optimize(
            QueryCompilationContext queryCompilationContext,
            QueryModel queryModel)
        {
            _queryCompilationContext = queryCompilationContext;

            new AdditionalFromClauseOptimizingQueryModelVisitor(_queryCompilationContext).VisitQueryModel(queryModel);

            VisitQueryModel(queryModel);

            queryModel.TransformExpressions(_transformingExpressionVisitor.Visit);
            queryModel.TransformExpressions(new ConditionalOptimizingExpressionVisitor().Visit);
            queryModel.TransformExpressions(new EntityEqualityRewritingExpressionVisitor(queryCompilationContext).Visit);
            queryModel.TransformExpressions(new SubQueryMemberPushDownExpressionVisitor(queryCompilationContext).Visit);
            queryModel.TransformExpressions(new ExistsToAnyRewritingExpressionVisitor().Visit);
            queryModel.TransformExpressions(new AllAnyToContainsRewritingExpressionVisitor().Visit);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            TryFlattenJoin(joinClause, queryModel);

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            TryFlattenJoin(joinClause, queryModel);

            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }

        private void TryFlattenJoin(JoinClause joinClause, QueryModel queryModel)
        {
            if (joinClause.InnerSequence is SubQueryExpression subQueryExpression)
            {
                VisitQueryModel(subQueryExpression.QueryModel);

                if (subQueryExpression.QueryModel.IsIdentityQuery()
                    && !subQueryExpression.QueryModel.ResultOperators.Any())
                {
                    joinClause.InnerSequence
                        = subQueryExpression.QueryModel.MainFromClause.FromExpression;

                    foreach (var queryAnnotation
                        in _queryCompilationContext.QueryAnnotations
                            .Where(qa => qa.QuerySource == subQueryExpression.QueryModel.MainFromClause))
                    {
                        queryAnnotation.QuerySource = joinClause;
                        queryAnnotation.QueryModel = queryModel;
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);

            // Attempts to rewrite GroupJoin/SelectMany to regular join

            var additionalFromClause
                = queryModel.BodyClauses.ElementAtOrDefault(index + 1)
                    as AdditionalFromClause;

            if (additionalFromClause?.FromExpression is QuerySourceReferenceExpression querySourceReferenceExpression
                && querySourceReferenceExpression.ReferencedQuerySource == groupJoinClause)
            {
                if (queryModel.CountQuerySourceReferences(groupJoinClause) == 1)
                {
                    // GroupJoin/SelectMany can be rewritten to regular Join.

                    queryModel.BodyClauses.RemoveAt(index + 1);
                    queryModel.BodyClauses.RemoveAt(index);
                    queryModel.BodyClauses.Insert(index, groupJoinClause.JoinClause);

                    UpdateQuerySourceMapping(
                        queryModel,
                        additionalFromClause,
                        new QuerySourceReferenceExpression(groupJoinClause.JoinClause));
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void FlattenSubQuery(
            SubQueryExpression subQueryExpression,
            IFromClause fromClause,
            QueryModel queryModel,
            int destinationIndex)
        {
            var subQueryModel = subQueryExpression.QueryModel;

            VisitQueryModel(subQueryModel);

            // no groupby and no distinct
            var emptyQueryModelWithFlattenableResultOperatorInSubquery
                = !queryModel.BodyClauses.Any()
                  && subQueryModel.ResultOperators.All(
                      ro => ro is CastResultOperator
                            || ro is DefaultIfEmptyResultOperator
                            || ro is ExceptResultOperator
                            || ro is OfTypeResultOperator
                            || ro is ReverseResultOperator
                            || ro is SkipResultOperator
                            || ro is TakeResultOperator);

            // we can lift distinct however if the outer query has result operator that doesn't care about having correct element count
            var emptyQueryModelWithResultOperatorThatIgnoresElementCountAndDistinctInSubquery
                = !queryModel.BodyClauses.Any()
                  && subQueryModel.ResultOperators.Any(ro => ro is DistinctResultOperator)
                  && queryModel.ResultOperators.Any(
                      ro => ro is ContainsResultOperator
                            || ro is AnyResultOperator
                            || ro is AllResultOperator
                            || ro is MinResultOperator
                            || ro is MaxResultOperator);

            var subqueryInMainClauseWithoutResultOperatorsProjectingItsMainClause
                = fromClause is MainFromClause
                  && !subQueryModel.ResultOperators.Any()
                  && subQueryModel.SelectClause.Selector is QuerySourceReferenceExpression subquerySelectorsQsre
                  && subquerySelectorsQsre.ReferencedQuerySource == subQueryModel.MainFromClause;

            if (subQueryModel.ResultOperators.All(ro => ro is CastResultOperator)
                && !subQueryModel.BodyClauses.Any(bc => bc is OrderByClause)
                && subQueryModel.SelectClause.Selector.NodeType != ExpressionType.MemberInit
                && subQueryModel.SelectClause.Selector.NodeType != ExpressionType.New
                || queryModel.IsIdentityQuery()
                && !queryModel.ResultOperators.Any()
                || emptyQueryModelWithFlattenableResultOperatorInSubquery
                || emptyQueryModelWithResultOperatorThatIgnoresElementCountAndDistinctInSubquery
                || subqueryInMainClauseWithoutResultOperatorsProjectingItsMainClause)
            {
                var querySourceMapping = new QuerySourceMapping();
                var clonedSubQueryModel = subQueryModel.Clone(querySourceMapping);
                UpdateQueryAnnotations(subQueryModel, querySourceMapping);
                _queryCompilationContext.UpdateMapping(querySourceMapping);

                var innerMainFromClause = clonedSubQueryModel.MainFromClause;
                var isGeneratedNameOuter = fromClause.HasGeneratedItemName();

                var itemName = innerMainFromClause.HasGeneratedItemName()
                                  && !isGeneratedNameOuter
                    ? fromClause.ItemName
                    : innerMainFromClause.ItemName;

                var fromClauseData
                    = new FromClauseData(
                        itemName, innerMainFromClause.ItemType, innerMainFromClause.FromExpression);

                fromClause.CopyFromSource(fromClauseData);

                var newExpression = clonedSubQueryModel.SelectClause.Selector;
                var newExpressionTypeInfo = newExpression.Type.GetTypeInfo();
                var castResultOperatorTypes = clonedSubQueryModel.ResultOperators.OfType<CastResultOperator>().Select(cre => cre.CastItemType).ToList();
                var type = castResultOperatorTypes.LastOrDefault(t => newExpressionTypeInfo.IsAssignableFrom(t.GetTypeInfo()));

                if (type != null
                    && type != newExpression.Type)
                {
                    newExpression = Expression.Convert(newExpression, type);
                }

                UpdateQuerySourceMapping(
                    queryModel,
                    fromClause,
                    newExpression);

                InsertBodyClauses(clonedSubQueryModel.BodyClauses, queryModel, destinationIndex);

                foreach (var resultOperator in clonedSubQueryModel.ResultOperators.Where(ro => !(ro is CastResultOperator)).Reverse())
                {
                    queryModel.ResultOperators.Insert(0, resultOperator);
                }

                UpdateQuerySourceMapping(
                    queryModel,
                    innerMainFromClause,
                    new QuerySourceReferenceExpression(fromClause));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            if (resultOperator is ValueFromSequenceResultOperatorBase
                && !(resultOperator is FirstResultOperator)
                && !(resultOperator is SingleResultOperator)
                && !(resultOperator is LastResultOperator)
                && !queryModel.ResultOperators
                    .Any(r => r is TakeResultOperator || r is SkipResultOperator))
            {
                for (var i = queryModel.BodyClauses.Count - 1; i >= 0; i--)
                {
                    if (queryModel.BodyClauses[i] is OrderByClause)
                    {
                        queryModel.BodyClauses.RemoveAt(i);
                    }
                }
            }

            if (resultOperator is OfTypeResultOperator ofTypeOperator)
            {
                var searchedItemType = ofTypeOperator.SearchedItemType;
                if (searchedItemType == queryModel.MainFromClause.ItemType)
                {
                    queryModel.ResultOperators.RemoveAt(index);
                }
                else
                {
                    var entityType = _queryCompilationContext.Model.FindEntityType(searchedItemType)
                        ?? _queryCompilationContext.FindEntityType(queryModel.MainFromClause);

                    if (entityType != null)
                    {
                        var oldQuerySource = queryModel.MainFromClause;

                        if (((oldQuerySource.FromExpression as ConstantExpression)?.Value as IQueryable)?.Provider
                            is IAsyncQueryProvider entityQueryProvider)
                        {
                            queryModel.ResultOperators.RemoveAt(index);

                            var newMainFromClause
                                = new MainFromClause(
                                    oldQuerySource.ItemName,
                                    entityType.ClrType,
                                    entityQueryProvider.CreateEntityQueryableExpression(entityType.ClrType));

                            queryModel.MainFromClause = newMainFromClause;

                            _queryCompilationContext.AddOrUpdateMapping(newMainFromClause, entityType);

                            UpdateQuerySourceMapping(
                                queryModel,
                                oldQuerySource,
                                new QuerySourceReferenceExpression(newMainFromClause));
                        }
                    }
                }
            }

            ProcessSetResultOperator(resultOperator);

            TryOptimizeContains(resultOperator, queryModel);

            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        private void TryOptimizeContains(ResultOperatorBase resultOperator, QueryModel queryModel)
        {
            if (resultOperator is ContainsResultOperator containsResultOperator
                && queryModel.SelectClause.Selector is QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                // Contains with entity instance to simple key contains expansion

                var projectedEntityType
                    = _queryCompilationContext.Model
                        .FindEntityType(querySourceReferenceExpression.Type);

                if (projectedEntityType != null)
                {
                    var valueEntityType
                        = _queryCompilationContext.Model
                            .FindEntityType(containsResultOperator.Item.Type);

                    if (valueEntityType != null
                        && valueEntityType.BaseType == projectedEntityType.BaseType)
                    {
                        var primaryKey = projectedEntityType.FindPrimaryKey();

                        if (primaryKey.Properties.Count == 1)
                        {
                            queryModel.SelectClause.Selector
                                = querySourceReferenceExpression
                                    .CreateEFPropertyExpression(primaryKey.Properties[0], makeNullable: false);

                            containsResultOperator.Item
                                = containsResultOperator.Item
                                    .CreateEFPropertyExpression(primaryKey.Properties[0], makeNullable: false);
                        }
                    }
                }
            }
        }

        private void UpdateQueryAnnotations(QueryModel queryModel, QuerySourceMapping querySourceMapping)
        {
            foreach (var queryAnnotation in _queryCompilationContext.QueryAnnotations)
            {
                if (querySourceMapping.ContainsMapping(queryAnnotation.QuerySource))
                {
                    queryAnnotation.QuerySource = querySourceMapping.GetExpression(queryAnnotation.QuerySource)
                        .TryGetReferencedQuerySource();
                    queryAnnotation.QueryModel = queryModel;

                    if (queryAnnotation is IncludeResultOperator includeAnnotation
                        && includeAnnotation.PathFromQuerySource != null)
                    {
                        includeAnnotation.PathFromQuerySource
                            = ReferenceReplacingExpressionVisitor
                                .ReplaceClauseReferences(includeAnnotation.PathFromQuerySource, querySourceMapping, throwOnUnmappedReferences: false);
                    }
                }
            }
        }

        private void UpdateQuerySourceMapping(
            QueryModel queryModel,
            IQuerySource oldQuerySource,
            Expression newExpression)
        {
            var querySourceMapping = new QuerySourceMapping();
            querySourceMapping.AddMapping(oldQuerySource, newExpression);

            queryModel.TransformExpressions(
                e => ReferenceReplacingExpressionVisitor
                    .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

            if (newExpression is QuerySourceReferenceExpression qsre)
            {
                var newQuerySource = qsre.ReferencedQuerySource;
                var entityType = _queryCompilationContext.FindEntityType(oldQuerySource);
                if (entityType != null)
                {
                    _queryCompilationContext.AddOrUpdateMapping(newQuerySource, entityType);
                }
                foreach (var queryAnnotation in _queryCompilationContext.QueryAnnotations.Where(qa => qa.QuerySource == oldQuerySource))
                {
                    queryAnnotation.QuerySource = newQuerySource;
                    queryAnnotation.QueryModel = queryModel;

                    if (queryAnnotation is IncludeResultOperator includeAnnotation
                        && includeAnnotation.PathFromQuerySource != null)
                    {
                        includeAnnotation.PathFromQuerySource
                            = ReferenceReplacingExpressionVisitor
                                .ReplaceClauseReferences(includeAnnotation.PathFromQuerySource, querySourceMapping, throwOnUnmappedReferences: false);
                    }
                }
            }
        }

        private static void ProcessSetResultOperator(ResultOperatorBase resultOperator)
        {
            switch (resultOperator)
            {
                case ExceptResultOperator _:
                case ConcatResultOperator _:
                case IntersectResultOperator _:
                case UnionResultOperator _:
                    resultOperator.TransformExpressions(ConvertEntityQueryableToSubQuery);
                    break;
            }
        }

        private static Expression ConvertEntityQueryableToSubQuery(Expression expression)
        {
            if ((expression as ConstantExpression)?.IsEntityQueryable() ?? false)
            {
                var mainFromClause = new MainFromClause(
                    "<generated>_",
                    expression.Type.GenericTypeArguments[0],
                    expression);

                var queryModel = new QueryModel(
                    mainFromClause,
                    new SelectClause(new QuerySourceReferenceExpression(mainFromClause)));

                mainFromClause.ItemName = queryModel.GetNewName(mainFromClause.ItemName);

                return new SubQueryExpression(queryModel);
            }

            return expression;
        }
    }
}
