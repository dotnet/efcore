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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CollectionNavigationSetOperatorSubqueryInjector : CollectionNavigationSubqueryInjector
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CollectionNavigationSetOperatorSubqueryInjector([NotNull] EntityQueryModelVisitor queryModelVisitor, bool shouldInject = false)
            : base(queryModelVisitor, shouldInject)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
