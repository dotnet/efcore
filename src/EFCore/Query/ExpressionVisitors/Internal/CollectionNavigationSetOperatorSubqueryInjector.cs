// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CollectionNavigationSetOperatorSubqueryInjector : CollectionNavigationSubqueryInjector
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CollectionNavigationSetOperatorSubqueryInjector([NotNull] EntityQueryModelVisitor queryModelVisitor, bool shouldInject = false)
            : base(queryModelVisitor, shouldInject)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var shouldInject = ShouldInject;
            ShouldInject = false;

            expression.QueryModel.TransformExpressions(Visit);

            ShouldInject = shouldInject;

            foreach (var resultOperator in expression.QueryModel.ResultOperators.Where(
                ro => ro is ConcatResultOperator
                      || ro is UnionResultOperator
                      || ro is IntersectResultOperator
                      || ro is ExceptResultOperator))
            {
                shouldInject = ShouldInject;
                ShouldInject = true;

                resultOperator.TransformExpressions(Visit);

                ShouldInject = shouldInject;
            }

            return base.VisitSubQuery(expression);
        }
    }
}
