// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql.Query.ExpressionVisitors.Internal
{
    public class SqlTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly SelectExpression _selectExpression;
        private readonly QueryCompilationContext _queryCompilationContext;

        public SqlTranslatingExpressionVisitor(SelectExpression selectExpression,
            QueryCompilationContext queryCompilationContext)
        {
            _selectExpression = selectExpression;
            _queryCompilationContext = queryCompilationContext;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(memberExpression,
                _queryCompilationContext, out var qsre);

            return _selectExpression.BindPropertyPath(qsre, properties) ?? base.VisitMember(memberExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var properties = MemberAccessBindingExpressionVisitor.GetPropertyPath(methodCallExpression,
                _queryCompilationContext, out var qsre);

            return _selectExpression.BindPropertyPath(qsre, properties) ?? base.VisitMethodCall(methodCallExpression);
        }
    }
}
