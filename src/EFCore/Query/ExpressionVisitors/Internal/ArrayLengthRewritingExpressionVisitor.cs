// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ArrayLengthRewritingExpressionVisitor : ExpressionVisitorBase
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var subQueryModel = expression.QueryModel;
            var fromExpression = subQueryModel.MainFromClause.FromExpression;

            if (fromExpression.NodeType == ExpressionType.MemberAccess
                && subQueryModel.BodyClauses.Count == 0
                && subQueryModel.ResultOperators.Count == 1
                && fromExpression is MemberExpression memberExpression
                && memberExpression.Type == typeof(byte[])
                && subQueryModel.ResultTypeOverride == typeof(int))
            {
                subQueryModel.ResultOperators.Clear();

                var memberInfo = memberExpression.Type.GetMember(nameof(string.Length))[0];

                return Expression.MakeMemberAccess(memberExpression, memberInfo);
            }

            return expression;
        }
    }
}
