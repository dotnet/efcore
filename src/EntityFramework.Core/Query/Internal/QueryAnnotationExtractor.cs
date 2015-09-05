// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Query.ResultOperators.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class QueryAnnotationExtractor : IQueryAnnotationExtractor
    {
        public virtual IReadOnlyCollection<QueryAnnotationBase> ExtractQueryAnnotations(QueryModel queryModel)
        {
            var queryAnnotations = new List<QueryAnnotationBase>();

            ExtractQueryAnnotations(queryModel, queryAnnotations);

            return queryAnnotations;
        }

        private static void ExtractQueryAnnotations(
            QueryModel queryModel, ICollection<QueryAnnotationBase> queryAnnotations)
        {
            foreach (var resultOperator
                in queryModel.ResultOperators
                    .OfType<QueryAnnotationResultOperator>()
                    .ToList())
            {
                resultOperator.Annotation.QueryModel = queryModel;
                resultOperator.Annotation.QuerySource = queryModel.MainFromClause;
                queryAnnotations.Add(resultOperator.Annotation);
                queryModel.ResultOperators.Remove(resultOperator);
            }

            queryModel.MainFromClause
                .TransformExpressions(e =>
                    ExtractQueryAnnotations(e, queryAnnotations));

            foreach (var bodyClause in queryModel.BodyClauses)
            {
                bodyClause
                    .TransformExpressions(e =>
                        ExtractQueryAnnotations(e, queryAnnotations));
            }
        }

        private static Expression ExtractQueryAnnotations(
            Expression expression, ICollection<QueryAnnotationBase> queryAnnotations)
        {
            var subQueryExpression = expression as SubQueryExpression;

            if (subQueryExpression != null)
            {
                ExtractQueryAnnotations(subQueryExpression.QueryModel, queryAnnotations);
            }

            return expression;
        }
    }
}
