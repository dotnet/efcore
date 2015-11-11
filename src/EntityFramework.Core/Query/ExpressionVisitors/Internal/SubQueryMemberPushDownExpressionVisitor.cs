// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class SubQueryMemberPushDownExpressionVisitor : ExpressionVisitorBase, ISubQueryMemberPushDownExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var newExpression = Visit(node.Expression);

            var subQueryExpression = newExpression as SubQueryExpression;
            var subSelector = subQueryExpression?.QueryModel.SelectClause.Selector;

            if (subSelector is QuerySourceReferenceExpression
                || subSelector is SubQueryExpression)
            {
                var subQueryModel = subQueryExpression.QueryModel;

                subQueryModel.SelectClause.Selector = VisitMember(node.Update(subSelector));
                subQueryModel.ResultTypeOverride = subQueryModel.SelectClause.Selector.Type;

                return new SubQueryExpression(subQueryModel);
            }

            return node.Update(newExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var newMethodCallExpression = (MethodCallExpression)base.VisitMethodCall(node);

            if (node.Method.IsGenericMethod
                && (node.Method.GetGenericMethodDefinition() == EntityQueryModelVisitor.PropertyMethodInfo))
            {
                var subQueryExpression = newMethodCallExpression.Arguments[0] as SubQueryExpression;
                var subSelector = subQueryExpression?.QueryModel.SelectClause.Selector as QuerySourceReferenceExpression;

                if (subSelector != null)
                {
                    var subQueryModel = subQueryExpression.QueryModel;

                    subQueryModel.SelectClause.Selector
                        = node
                            .Update(
                                null,
                                new[]
                                {
                                    subSelector,
                                    node.Arguments[1]
                                });

                    subQueryModel.ResultTypeOverride = subQueryModel.SelectClause.Selector.Type;

                    return new SubQueryExpression(subQueryModel);
                }
            }

            return newMethodCallExpression;
        }
    }
}
