// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LeftOuterJoinClauseQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly IEnumerable<IQueryAnnotation> _queryAnnotations;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public LeftOuterJoinClauseQueryModelVisitor([NotNull] IEnumerable<IQueryAnnotation> queryAnnotations)
        {
            _queryAnnotations = queryAnnotations;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<LeftOuterJoinClauseQueryModelVisitor>(this).Visit);

            base.VisitQueryModel(queryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitGroupJoinClause(
            [NotNull] GroupJoinClause groupJoinClause,
            [NotNull] QueryModel queryModel, 
            int index)
        {
            if (queryModel.CountQuerySourceReferences(groupJoinClause) == 1
                && queryModel.BodyClauses.ElementAtOrDefault(index + 1) is AdditionalFromClause additionalFromClause 
                && additionalFromClause.FromExpression is SubQueryExpression subQueryExpression
                && subQueryExpression.QueryModel.MainFromClause.FromExpression is QuerySourceReferenceExpression subQsre
                && subQsre.ReferencedQuerySource == groupJoinClause
                && !subQueryExpression.QueryModel.BodyClauses.Any()
                && subQueryExpression.QueryModel.ResultOperators.Count == 1
                && subQueryExpression.QueryModel.ResultOperators[0] is DefaultIfEmptyResultOperator defaultIfEmptyResultOperator
                && defaultIfEmptyResultOperator.OptionalDefaultValue == null)
            {
                var leftOuterJoinClause = new LeftOuterJoinClause(groupJoinClause, additionalFromClause);

                queryModel.BodyClauses.Insert(index, leftOuterJoinClause);
                queryModel.BodyClauses.Remove(groupJoinClause);
                queryModel.BodyClauses.Remove(additionalFromClause);

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
