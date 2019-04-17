// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Microsoft.Extensions.DependencyInjection;
using Remotion.Linq;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class QueryAnnotationExtractor : IQueryAnnotationExtractor
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
