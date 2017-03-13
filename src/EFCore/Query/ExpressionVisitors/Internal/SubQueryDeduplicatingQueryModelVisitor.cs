// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SubQueryDeduplicatingQueryModelVisitor : QueryModelVisitorBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitQueryModel([NotNull] QueryModel queryModel)
        {
            queryModel.TransformExpressions(new TransformingQueryModelExpressionVisitor<SubQueryDeduplicatingQueryModelVisitor>(this).Visit);

            base.VisitQueryModel(queryModel);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            var duplicateSubQueryVisitor = new DuplicateSubQueryExpressionFindingExpressionVisitor();
            duplicateSubQueryVisitor.Visit(selectClause.Selector);

            var duplicateSubQueryExpressions
                = duplicateSubQueryVisitor.SubQueryExpressions
                    .Where(p => p.Value > 1)
                    .Select(p => p.Key)
                    .ToArray();

            foreach (var subQueryExpression in duplicateSubQueryExpressions)
            {
                var finalResultOperator = subQueryExpression.QueryModel.ResultOperators.LastOrDefault();

                if (finalResultOperator is FirstResultOperator firstResultOperator
                    && firstResultOperator.ReturnDefaultWhenEmpty == true)
                {
                    var newSubQueryModel = subQueryExpression.QueryModel.Clone();

                    newSubQueryModel.ResultOperators.Remove(newSubQueryModel.ResultOperators.Last());
                    newSubQueryModel.ResultOperators.Add(new TakeResultOperator(Expression.Constant(1)));
                    newSubQueryModel.ResultOperators.Add(new DefaultIfEmptyResultOperator(null));
                    newSubQueryModel.ResultTypeOverride = null;

                    var newSubQueryExpression = new SubQueryExpression(newSubQueryModel);

                    var newAdditionalFromClause
                        = new AdditionalFromClause(
                            queryModel.GetUniqueIdentfierGenerator().GetUniqueIdentifier("<range>_"),
                            subQueryExpression.Type,
                            newSubQueryExpression);

                    queryModel.BodyClauses.Add(newAdditionalFromClause);

                    var replacingVisitor 
                        = new RecursiveExpressionReplacingExpressionVisitor(
                            subQueryExpression, 
                            new QuerySourceReferenceExpression(newAdditionalFromClause));

                    queryModel.SelectClause.TransformExpressions(replacingVisitor.Visit);
                }
            }
        }

        private class DuplicateSubQueryExpressionFindingExpressionVisitor : ExpressionVisitorBase
        {
            public Dictionary<SubQueryExpression, int> SubQueryExpressions { get; }
                = new Dictionary<SubQueryExpression, int>();

            public override Expression Visit(Expression node)
            {
                return base.Visit(node);
            }

            protected override Expression VisitSubQuery(SubQueryExpression expression)
            {
                if (SubQueryExpressions.ContainsKey(expression))
                {
                    SubQueryExpressions[expression]++;
                }
                else
                {
                    SubQueryExpressions[expression] = 1;
                }

                return base.VisitSubQuery(expression);
            }
        }

        private class RecursiveExpressionReplacingExpressionVisitor : ExpressionVisitorBase
        {
            private readonly Expression _targetExpression;
            private readonly Expression _replacementExpression;

            public RecursiveExpressionReplacingExpressionVisitor(Expression targetExpression, Expression replacementExpression)
            {
                _targetExpression = targetExpression;
                _replacementExpression = replacementExpression;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _targetExpression)
                {
                    return _replacementExpression;
                }

                return base.Visit(node);
            }
        }
    }
}