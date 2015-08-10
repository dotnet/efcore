// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Transformations;

namespace Microsoft.Data.Entity.Query
{
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

        private IReadOnlyCollection<QueryAnnotationBase> _queryAnnotations;

        public virtual void Optimize(
            [NotNull] IReadOnlyCollection<QueryAnnotationBase> queryAnnotations,
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryAnnotations, nameof(queryAnnotations));
            Check.NotNull(queryModel, nameof(queryModel));

            _queryAnnotations = queryAnnotations;

            VisitQueryModel(queryModel);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var subQueryExpression = joinClause.InnerSequence as SubQueryExpression;

            if (subQueryExpression != null)
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
                    }
                }
            }

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        protected override void FlattenSubQuery(
            [NotNull] SubQueryExpression subQueryExpression,
            [NotNull] IFromClause fromClause,
            [NotNull] QueryModel queryModel,
            int destinationIndex)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            var subQueryModel = subQueryExpression.QueryModel;

            VisitQueryModel(subQueryModel);

            if ((subQueryModel.ResultOperators
                .All(ro => (ro is OfTypeResultOperator || ro is CastResultOperator))
                 && !subQueryModel.BodyClauses.Any(bc => bc is OrderByClause))
                || (queryModel.IsIdentityQuery()
                    && !queryModel.ResultOperators.Any()))
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

                var fromClauseData = new FromClauseData(
                    itemName,
                    innerMainFromClause.ItemType,
                    innerMainFromClause.FromExpression);
                fromClause.CopyFromSource(fromClauseData);

                var innerSelectorMapping = new QuerySourceMapping();

                innerSelectorMapping.AddMapping(fromClause, subQueryExpression.QueryModel.SelectClause.Selector);

                queryModel.TransformExpressions(
                    ex => ReferenceReplacingExpressionVisitor
                        .ReplaceClauseReferences(ex, innerSelectorMapping, false));

                InsertBodyClauses(subQueryExpression.QueryModel.BodyClauses, queryModel, destinationIndex);

                foreach (var resultOperator in subQueryModel.ResultOperators.Reverse())
                {
                    queryModel.ResultOperators.Insert(0, resultOperator);
                }

                var innerBodyClauseMapping = new QuerySourceMapping();

                innerBodyClauseMapping
                    .AddMapping(innerMainFromClause, new QuerySourceReferenceExpression(fromClause));

                queryModel.TransformExpressions(ex =>
                    ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(ex, innerBodyClauseMapping, false));

                foreach (var queryAnnotation
                    in _queryAnnotations
                        .Where(qa => qa.QuerySource == subQueryExpression.QueryModel.MainFromClause))
                {
                    queryAnnotation.QuerySource = fromClause;
                    queryAnnotation.QueryModel = queryModel;
                }
            }
        }

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

            base.VisitResultOperator(resultOperator, queryModel, index);
        }
    }
}
