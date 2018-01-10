// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class QueryAnnotationExtractor : IQueryAnnotationExtractor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyCollection<IQueryAnnotation> ExtractQueryAnnotations(QueryModel queryModel)
        {
            var queryAnnotations = new List<IQueryAnnotation>();

            ExtractQueryAnnotations(queryModel, queryAnnotations);

            return queryAnnotations;
        }

        private static void ExtractQueryAnnotations(
            QueryModel queryModel, ICollection<IQueryAnnotation> queryAnnotations)
        {
            queryModel
                .TransformExpressions(
                    e => ExtractQueryAnnotations(e, queryAnnotations));

            foreach (var resultOperator in queryModel.ResultOperators.ToList())
            {
                if (resultOperator is IQueryAnnotation queryAnnotation)
                {
                    queryAnnotations.Add(queryAnnotation);

                    queryAnnotation.QueryModel = queryModel;

                    if (queryAnnotation.QuerySource == null)
                    {
                        queryAnnotation.QuerySource = queryModel.MainFromClause;
                    }

                    queryModel.ResultOperators.Remove(resultOperator);
                }
            }
        }

        private static Expression ExtractQueryAnnotations(
            Expression expression, ICollection<IQueryAnnotation> queryAnnotations)
        {
            new QueryAnnotationExtractingVisitor(queryAnnotations).Visit(expression);

            return expression;
        }

        private class QueryAnnotationExtractingVisitor : ExpressionVisitorBase
        {
            private readonly ICollection<IQueryAnnotation> _queryAnnotations;

            public QueryAnnotationExtractingVisitor(ICollection<IQueryAnnotation> queryAnnotations)
                => _queryAnnotations = queryAnnotations;

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                ExtractQueryAnnotations(expression.QueryModel, _queryAnnotations);

                return expression;
            }
        }
    }
}
