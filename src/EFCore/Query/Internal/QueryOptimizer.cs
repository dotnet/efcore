// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Utilities;
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

        private IReadOnlyCollection<IQueryAnnotation> _queryAnnotations;
        private readonly IModel _model;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public QueryOptimizer([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));

            _model = model;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Optimize(
            IReadOnlyCollection<IQueryAnnotation> queryAnnotations,
            QueryModel queryModel)
        {
            _queryAnnotations = queryAnnotations;

            VisitQueryModel(queryModel);
            queryModel.TransformExpressions(e => new TransformingQueryModelExpressionVisitor<QueryOptimizer>(this).Visit(e));
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
                        in _queryAnnotations
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
            [NotNull] SubQueryExpression subQueryExpression,
            [NotNull] IFromClause fromClause,
            [NotNull] QueryModel queryModel,
            int destinationIndex)
        {
            var subQueryModel = subQueryExpression.QueryModel;

            VisitQueryModel(subQueryModel);

            var subqueryInMainClauseWithoutResultOperatorsProjectingItsMainClause
                = fromClause is MainFromClause
                    && !subQueryModel.ResultOperators.Any()
                    && subQueryModel.SelectClause.Selector is QuerySourceReferenceExpression subquerySelectorsQsre
                    && subquerySelectorsQsre.ReferencedQuerySource == subQueryModel.MainFromClause;

            if (subQueryModel.ResultOperators.All(ro => ro is CastResultOperator)
                && !subQueryModel.BodyClauses.Any(bc => bc is OrderByClause)
                || queryModel.IsIdentityQuery()
                && !queryModel.ResultOperators.Any()
                || !queryModel.BodyClauses.Any()
                && !subQueryModel.ResultOperators.Any(ro => ro is GroupResultOperator)
                || subqueryInMainClauseWithoutResultOperatorsProjectingItsMainClause)
            {
                string itemName;

                var innerMainFromClause = subQueryExpression.QueryModel.MainFromClause;
                var isGeneratedNameOuter = fromClause.HasGeneratedItemName();

                if (innerMainFromClause.HasGeneratedItemName()
                    && !isGeneratedNameOuter)
                {
                    itemName = fromClause.ItemName;
                }
                else
                {
                    itemName = innerMainFromClause.ItemName;
                }

                var fromClauseData
                    = new FromClauseData(
                        itemName, innerMainFromClause.ItemType, innerMainFromClause.FromExpression);

                fromClause.CopyFromSource(fromClauseData);

                UpdateQuerySourceMapping(
                    queryModel,
                    fromClause,
                    subQueryExpression.QueryModel.SelectClause.Selector);

                InsertBodyClauses(subQueryExpression.QueryModel.BodyClauses, queryModel, destinationIndex);

                foreach (var resultOperator in subQueryModel.ResultOperators.Reverse())
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
                && !(resultOperator is ChoiceResultOperatorBase)
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
                    var entityType = _model.FindEntityType(searchedItemType);

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

                            UpdateQuerySourceMapping(queryModel,
                                oldQuerySource,
                                new QuerySourceReferenceExpression(newMainFromClause));
                        }
                    }
                }
            }

            ProcessSetResultOperator(resultOperator);

            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        private void UpdateQuerySourceMapping(
            QueryModel queryModel,
            IQuerySource oldQuerySource,
            Expression newExpression)
        {
            var querySourceMapping = new QuerySourceMapping();
            querySourceMapping.AddMapping(oldQuerySource, newExpression);

            queryModel.TransformExpressions(e =>
                ReferenceReplacingExpressionVisitor
                    .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

            if (newExpression is QuerySourceReferenceExpression qsre)
            {
                var newQuerySource = qsre.ReferencedQuerySource;
                foreach (var queryAnnotation in _queryAnnotations.Where(qa => qa.QuerySource == oldQuerySource))
                {
                    queryAnnotation.QuerySource = newQuerySource;
                    queryAnnotation.QueryModel = queryModel;
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
