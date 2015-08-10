// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class SubQueryMemberPushDownExpressionVisitor : ExpressionVisitorBase, ISubQueryMemberPushDownExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = Visit(memberExpression.Expression);

            var subQueryExpression = newExpression as SubQueryExpression;
            var subSelector = subQueryExpression?.QueryModel.SelectClause.Selector;

            if (subSelector is QuerySourceReferenceExpression
                || subSelector is SubQueryExpression)
            {
                var subQueryModel = subQueryExpression.QueryModel;

                subQueryModel.SelectClause.Selector = VisitMember(memberExpression.Update(subSelector));
                subQueryModel.ResultTypeOverride = subQueryModel.SelectClause.Selector.Type;

                return new SubQueryExpression(subQueryModel);
            }

            return memberExpression.Update(newExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var newMethodCallExpression = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == EntityQueryModelVisitor.PropertyMethodInfo)
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
    }
}
