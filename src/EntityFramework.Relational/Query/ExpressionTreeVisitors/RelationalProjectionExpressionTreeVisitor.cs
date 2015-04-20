// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalProjectionExpressionTreeVisitor : ProjectionExpressionTreeVisitor
    {
        private readonly SqlTranslatingExpressionTreeVisitor _sqlTranslatingExpressionTreeVisitor;

        private bool _requiresClientEval;

        public RelationalProjectionExpressionTreeVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                Check.NotNull(querySource, nameof(querySource)))
        {
            _sqlTranslatingExpressionTreeVisitor
                = new SqlTranslatingExpressionTreeVisitor(queryModelVisitor);
        }

        private new RelationalQueryModelVisitor QueryModelVisitor
            => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        public virtual bool RequiresClientEval => _requiresClientEval;

        protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && ReferenceEquals(
                    methodCallExpression.Method.GetGenericMethodDefinition(),
                    QueryExtensions.PropertyMethodInfo))
            {
                var newArg0 = VisitExpression(methodCallExpression.Arguments[0]);

                if (newArg0 != methodCallExpression.Arguments[0])
                {
                    return Expression.Call(
                        methodCallExpression.Method,
                        newArg0,
                        methodCallExpression.Arguments[1]);
                }

                return methodCallExpression;
            }

            return base.VisitMethodCallExpression(methodCallExpression);
        }

        public override Expression VisitExpression(Expression expression)
        {
            if (expression != null
                && !(expression is QuerySourceReferenceExpression))
            {
                var sqlExpression
                    = _sqlTranslatingExpressionTreeVisitor.VisitExpression(expression);

                if (sqlExpression == null)
                {
                    _requiresClientEval = true;
                }
                else
                {
                    var selectExpression
                        = QueryModelVisitor.TryGetQuery(QuerySource);

                    Debug.Assert(selectExpression != null);

                    if (!(expression is NewExpression))
                    {
                        //var aliasExpression = sqlExpression as AliasExpression;
                        var columnExpression = sqlExpression.GetColumnExpression();

                        if (columnExpression != null)
                        {
                            selectExpression.AddToProjection(columnExpression);

                            return expression;
                        }

                        var index = selectExpression.AddToProjection(sqlExpression);

                        return
                            QueryModelVisitor.BindReadValueMethod(
                                expression.Type,
                                QuerySourceScope.GetResult(
                                    EntityQueryModelVisitor.QuerySourceScopeParameter,
                                    QuerySource,
                                    typeof(IValueReader)),
                                index);
                    }
                }
            }

            return base.VisitExpression(expression);
        }
    }
}
