// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

                        var selector = subQueryModel.SelectClause.Selector;
                        if (IsFirstSingleLastOrDefault(subQueryExpression.QueryModel.ResultOperators.LastOrDefault())
                            && !selector.Type.IsNullableType())
                        {
                            var oldType = selector.Type;
                            subQueryModel.SelectClause.Selector
                                = Expression.Convert(
                                    selector,
                                    selector.Type.MakeNullable());

                            subQueryModel.ResultTypeOverride = subQueryModel.SelectClause.Selector.Type;

                            return Expression.Convert(
                                new SubQueryExpression(subQueryModel),
                                oldType);
                        }

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
                        Expression selector = Expression.MakeMemberAccess(
                            new QuerySourceReferenceExpression(mainFromClause),
                            memberExpression.Member);

                        if (IsFirstSingleLastOrDefault(finalResultOperator)
                            && !selector.Type.IsNullableType())
                        {
                            var oldType = selector.Type;
                            selector = Expression.Convert(
                                selector,
                                selector.Type.MakeNullable());

                            var subqueryModel = new QueryModel(mainFromClause, new SelectClause(selector));
                            subqueryModel.ResultOperators.Add(finalResultOperator);

                            return Expression.Convert(
                                new SubQueryExpression(subqueryModel),
                                oldType);
                        }
                        else
                        {
                            var subqueryModel = new QueryModel(mainFromClause, new SelectClause(selector));
                            subqueryModel.ResultOperators.Add(finalResultOperator);
                            var subqueryExpression = new SubQueryExpression(subqueryModel);

                            return subqueryExpression;
                        }
                    }
                }
            }

            return memberExpression.Update(newExpression);
        }

        private static bool IsFirstSingleLastOrDefault(ResultOperatorBase resultOperator)
            => (resultOperator is FirstResultOperator first && first.ReturnDefaultWhenEmpty)
               || (resultOperator is SingleResultOperator single && single.ReturnDefaultWhenEmpty)
               || (resultOperator is LastResultOperator last && last.ReturnDefaultWhenEmpty);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var newMethodCallExpression = (MethodCallExpression)base.VisitMethodCall(methodCallExpression);

            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                if (newMethodCallExpression.Arguments[0] is SubQueryExpression subQueryExpression
                    && subQueryExpression.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression subSelector)
                {
                    var querySourceMapping = new QuerySourceMapping();
                    var subQueryModel = subQueryExpression.QueryModel.Clone(querySourceMapping);
                    _queryCompilationContext.UpdateMapping(querySourceMapping);
                    var clonedSubSelector = querySourceMapping.GetExpression(subSelector.ReferencedQuerySource);

                    subQueryModel.SelectClause.Selector
                        = methodCallExpression
                            .Update(
                                null,
                                new[]
                                {
                                    clonedSubSelector,
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
        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            // remove double convert that could be introduced in the member pushdown (when we introduce cast to nullable for FirstOrDefault cases)
            if (unaryExpression.NodeType == ExpressionType.Convert
                || unaryExpression.NodeType == ExpressionType.ConvertChecked)
            {
                var newOperand = Visit(unaryExpression.Operand);
                return newOperand is UnaryExpression innerUnaryExpression
                    && (innerUnaryExpression.NodeType == ExpressionType.Convert
                        || innerUnaryExpression.NodeType == ExpressionType.ConvertChecked)
                    && innerUnaryExpression.Operand.Type == unaryExpression.Type
                    && innerUnaryExpression.Operand.Type != typeof(object)
                    ? innerUnaryExpression.Operand
                    : unaryExpression.Update(newOperand);
            }

            return base.VisitUnary(unaryExpression);
        }
    }
}
