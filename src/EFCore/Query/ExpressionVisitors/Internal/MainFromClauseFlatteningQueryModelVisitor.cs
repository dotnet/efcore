// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MainFromClauseFlatteningQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly IEnumerable<IQueryAnnotation> _queryAnnotations;
        private readonly SubQueryExpressionVisitor _subQueryExpressionVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MainFromClauseFlatteningQueryModelVisitor([NotNull] IEnumerable<IQueryAnnotation> queryAnnotations)
        {
            _queryAnnotations = queryAnnotations;
            _subQueryExpressionVisitor = new SubQueryExpressionVisitor(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            queryModel.TransformExpressions(_subQueryExpressionVisitor.Visit);

            base.VisitQueryModel(queryModel);
        }
        
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitMainFromClause([NotNull] MainFromClause fromClause, [NotNull] QueryModel queryModel)
        {
            if (fromClause.FromExpression is SubQueryExpression subQueryExpression)
            {
                var subQueryModel = subQueryExpression.QueryModel;

                if (subQueryModel.SelectClause.Selector is QuerySourceReferenceExpression
                    && !subQueryModel.ResultOperators.OfType<GroupResultOperator>().Any()
                    && !queryModel.BodyClauses.Any())
                {
                    queryModel.UpdateQuerySourceMapping(
                        queryModel.MainFromClause,
                        subQueryModel.SelectClause.Selector,
                        _queryAnnotations);

                    queryModel.MainFromClause = subQueryModel.MainFromClause;

                    foreach (var bodyClause in subQueryModel.BodyClauses)
                    {
                        queryModel.BodyClauses.Add(bodyClause);
                    }

                    foreach (var resultOperator in subQueryModel.ResultOperators.Reverse())
                    {
                        queryModel.ResultOperators.Insert(0, resultOperator);
                    }

                    return;
                }
            }
        }

        private class SubQueryExpressionVisitor : RelinqExpressionVisitor
        {
            private readonly MainFromClauseFlatteningQueryModelVisitor _queryModelVisitor;

            public SubQueryExpressionVisitor(MainFromClauseFlatteningQueryModelVisitor queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                _queryModelVisitor.VisitQueryModel(expression.QueryModel);

                return expression;
            }
        }
    }
}
