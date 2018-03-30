// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SubqueryProjectingSingleValueOptimizingExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            if (subQueryExpression.QueryModel.ResultOperators.LastOrDefault() is FirstResultOperator first
                && first.ReturnDefaultWhenEmpty
                && !subQueryExpression.Type.IsNullableType())
            {
                var result = Expression.Coalesce(
                    Expression.Convert(subQueryExpression, subQueryExpression.Type.MakeNullable()),
                    Expression.Constant(subQueryExpression.Type.GetDefaultValue()));

                return result;
            }

            return base.VisitSubQuery(subQueryExpression);
        }
    }
}
