// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LateralJoinOptimizingQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly IEnumerable<IQueryAnnotation> _queryAnnotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public LateralJoinOptimizingQueryModelVisitor([NotNull] IEnumerable<IQueryAnnotation> queryAnnotations)
        {
            _queryAnnotations = queryAnnotations;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<LateralJoinOptimizingQueryModelVisitor>(this).Visit);

            base.VisitQueryModel(queryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitAdditionalFromClause(
            [NotNull] AdditionalFromClause additionalFromClause,
            [NotNull] QueryModel queryModel,
            int index)
        {
            if (index > 0
                && additionalFromClause.FromExpression is SubQueryExpression subQueryExpression
                && subQueryExpression.QueryModel.ResultOperators.Any(r => r is SkipResultOperator || r is TakeResultOperator)
                && subQueryExpression.QueryModel.MainFromClause.FromExpression is QuerySourceReferenceExpression qsre
                && qsre.ReferencedQuerySource is GroupJoinClause groupJoinClause
                && queryModel.CountQuerySourceReferences(groupJoinClause) == 1)
            {
                subQueryExpression.QueryModel.MainFromClause.FromExpression = groupJoinClause.JoinClause.InnerSequence;
                subQueryExpression.QueryModel.MainFromClause.ItemName = groupJoinClause.JoinClause.ItemName;

                var whereClauseMapping = new QuerySourceMapping();
                whereClauseMapping.AddMapping(groupJoinClause.JoinClause,
                    new QuerySourceReferenceExpression(subQueryExpression.QueryModel.MainFromClause));

                var whereClausePredicate
                    = ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                        Expression.Equal(
                            groupJoinClause.JoinClause.OuterKeySelector,
                            groupJoinClause.JoinClause.InnerKeySelector),
                        whereClauseMapping,
                        throwOnUnmappedReferences: false);

                subQueryExpression.QueryModel.BodyClauses.Insert(0, new WhereClause(whereClausePredicate));
                    
                queryModel.BodyClauses.Remove(groupJoinClause);

                var querySourceMapping = new QuerySourceMapping();

                querySourceMapping.AddMapping(
                    groupJoinClause.JoinClause,
                    new QuerySourceReferenceExpression(additionalFromClause));

                foreach (var queryAnnotation in _queryAnnotations)
                {
                    if (queryAnnotation.QuerySource == groupJoinClause.JoinClause)
                    {
                        queryAnnotation.QuerySource = additionalFromClause;

                        if (queryAnnotation is IncludeResultOperator includeResultOperator)
                        {
                            includeResultOperator.PathFromQuerySource
                                = ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                                    includeResultOperator.PathFromQuerySource,
                                    querySourceMapping,
                                    throwOnUnmappedReferences: false);
                        }
                    }
                }
            }
        }
    }
}
