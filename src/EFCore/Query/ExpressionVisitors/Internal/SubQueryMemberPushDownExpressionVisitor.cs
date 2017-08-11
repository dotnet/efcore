// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
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
    public class SubQueryMemberPushDownExpressionVisitor : ExpressionVisitorBase
    {
        private readonly QueryCompilationContext _queryCompilationContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SubQueryMemberPushDownExpressionVisitor([NotNull] QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var newExpression = Visit(memberExpression.Expression);

            if (newExpression is SubQueryExpression subQueryExpression)
            {
                var subSelector = subQueryExpression.QueryModel.SelectClause.Selector;
                if ((subSelector is QuerySourceReferenceExpression || subSelector is SubQueryExpression)
                    && !subQueryExpression.QueryModel.ResultOperators.Any(
                        ro =>
                            ro is ConcatResultOperator
                            || ro is UnionResultOperator
                            || ro is IntersectResultOperator
                            || ro is ExceptResultOperator))
                {
                    if (!subQueryExpression.QueryModel.ResultOperators.Any(ro => ro is DistinctResultOperator))
                    {
                        var querySourceMapping = new QuerySourceMapping();
                        var subQueryModel = subQueryExpression.QueryModel.Clone(querySourceMapping);
                        _queryCompilationContext.UpdateMapping(querySourceMapping);

                        subQueryModel.SelectClause.Selector = VisitMember(memberExpression.Update(subQueryModel.SelectClause.Selector));
                        subQueryModel.ResultTypeOverride = subQueryModel.SelectClause.Selector.Type;

                        return new SubQueryExpression(subQueryModel);
                    }

                    var finalResultOperator = subQueryExpression.QueryModel.ResultOperators.Last();
                    if (finalResultOperator is FirstResultOperator
                        || finalResultOperator is SingleResultOperator
                        || finalResultOperator is LastResultOperator)
                    {
                        var queryModel = subQueryExpression.QueryModel;
                        queryModel.ResultOperators.Remove(finalResultOperator);

                        queryModel.ResultTypeOverride = null;
                        var newSubQueryExpression = new SubQueryExpression(queryModel);

                        var mainFromClause = new MainFromClause(queryModel.GetNewName("subquery"), queryModel.SelectClause.Selector.Type, newSubQueryExpression);
                        var selector = Expression.MakeMemberAccess(
                            new QuerySourceReferenceExpression(mainFromClause),
                            memberExpression.Member);

                        var subqueryModel = new QueryModel(mainFromClause, new SelectClause(selector));
                        subqueryModel.ResultOperators.Add(finalResultOperator);
                        var subqueryExpression = new SubQueryExpression(subqueryModel);

                        return subqueryExpression;
                    }
                }
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

            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                var subQueryExpression = newMethodCallExpression.Arguments[0] as SubQueryExpression;
                if (subQueryExpression?.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression subSelector)
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
