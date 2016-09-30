// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SubQueryMemberPushDownExpressionVisitor : ExpressionVisitorBase, ISubQueryMemberPushDownExpressionVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = Visit(memberExpression.Expression);

            var subQueryExpression = newExpression as SubQueryExpression;
            var subSelector = subQueryExpression?.QueryModel.SelectClause.Selector;

            if (subSelector is QuerySourceReferenceExpression
                || subSelector is SubQueryExpression)
            {
                var subQueryModel = subQueryExpression.QueryModel.Clone();

                subQueryModel.SelectClause.Selector = VisitMember(memberExpression.Update(subQueryModel.SelectClause.Selector));
                subQueryModel.ResultTypeOverride = subQueryModel.SelectClause.Selector.Type;

                return new SubQueryExpression(subQueryModel);
            }

            return memberExpression.Update(newExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var newMethodCallExpression = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            if (EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
            {
                var subQueryExpression = newMethodCallExpression.Arguments[0] as SubQueryExpression;
                var subSelector = subQueryExpression?.QueryModel.SelectClause.Selector as QuerySourceReferenceExpression;

                if (subSelector != null)
                {
                    var subQueryModel = subQueryExpression.QueryModel;

                    subQueryModel.SelectClause.Selector
                        = methodCallExpression
                            .Update(
                                null,
                                new[]
                                {
                                    subSelector,
                                    methodCallExpression.Arguments[1]
                                });

                    subQueryModel.ResultTypeOverride = subQueryModel.SelectClause.Selector.Type;

                    return new SubQueryExpression(subQueryModel);
                }
            }

            return newMethodCallExpression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            subQueryExpression.QueryModel.TransformExpressions(Visit);

            return subQueryExpression;
        }
    }
}
