// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalEntityQueryableExpressionVisitor2 : EntityQueryableExpressionVisitor2
    {
        private readonly IModel _model;

        public RelationalEntityQueryableExpressionVisitor2(IModel model)
        {
            _model = model;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                && methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSqlOnQueryable))
            {
                // TODO: Implement parameters
                var sql = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                var queryable = (IQueryable)((ConstantExpression)methodCallExpression.Arguments[0]).Value;
                return CreateShapedQueryExpression(queryable.ElementType, sql);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
            => new RelationalShapedQueryExpression(_model.FindEntityType(elementType));

        protected virtual ShapedQueryExpression CreateShapedQueryExpression(Type elementType, string sql)
            => new RelationalShapedQueryExpression(_model.FindEntityType(elementType), sql);
    }
}
