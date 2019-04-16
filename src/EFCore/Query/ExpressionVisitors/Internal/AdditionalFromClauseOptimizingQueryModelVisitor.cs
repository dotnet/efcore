// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class AdditionalFromClauseOptimizingQueryModelVisitor : QueryModelVisitorBase
    {
        private QueryCompilationContext _queryCompilationContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public AdditionalFromClauseOptimizingQueryModelVisitor(QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<AdditionalFromClauseOptimizingQueryModelVisitor>(this).Visit);

            base.VisitQueryModel(queryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitAdditionalFromClause(
            AdditionalFromClause fromClause,
            QueryModel queryModel,
            int index)
        {
            if (fromClause.FromExpression is SubQueryExpression subQueryExpression
                && subQueryExpression.QueryModel.MainFromClause.FromExpression is QuerySourceReferenceExpression qsre
                && subQueryExpression.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression
                && qsre.ReferencedQuerySource is GroupJoinClause groupJoinClause
                && !(groupJoinClause.JoinClause.InnerSequence is SubQueryExpression)
                && queryModel.CountQuerySourceReferences(groupJoinClause) == 1
                && subQueryExpression.QueryModel.BodyClauses.Any()
                && subQueryExpression.QueryModel.BodyClauses.All(c => c is WhereClause))
            {
                var newMainFromClause = new MainFromClause(
                    groupJoinClause.JoinClause.ItemName,
                    groupJoinClause.JoinClause.ItemType,
                    groupJoinClause.JoinClause.InnerSequence);

                var newSelectClause = new SelectClause(
                    new QuerySourceReferenceExpression(newMainFromClause));

                var newSubQueryModel = new QueryModel(newMainFromClause, newSelectClause);

                ShiftBodyClauses(subQueryExpression.QueryModel, newSubQueryModel);

                var entityType = _queryCompilationContext.FindEntityType(subQueryExpression.QueryModel.MainFromClause);
                if (entityType != null)
                {
                    _queryCompilationContext.AddOrUpdateMapping(newMainFromClause, entityType);
                }

                var newSubQueryExpression = new SubQueryExpression(newSubQueryModel);

                groupJoinClause.JoinClause.InnerSequence = newSubQueryExpression;

                if (!subQueryExpression.QueryModel.ResultOperators.Any())
                {
                    fromClause.FromExpression = qsre;
                }
            }
        }

        private static void ShiftBodyClauses(QueryModel oldQueryModel, QueryModel newQueryModel)
        {
            var querySourceMapping = new QuerySourceMapping();

            querySourceMapping.AddMapping(
                oldQueryModel.MainFromClause,
                new QuerySourceReferenceExpression(newQueryModel.MainFromClause));

            foreach (var bodyClause in oldQueryModel.BodyClauses.ToArray())
            {
                bodyClause.TransformExpressions(
                    expression =>
                        ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                            expression,
                            querySourceMapping,
                            throwOnUnmappedReferences: false));

                oldQueryModel.BodyClauses.Remove(bodyClause);
                newQueryModel.BodyClauses.Add(bodyClause);
            }
        }
    }
}
