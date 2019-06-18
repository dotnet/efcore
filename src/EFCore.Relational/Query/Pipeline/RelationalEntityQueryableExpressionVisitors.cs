// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class RelationalEntityQueryableTranslator : EntityQueryableTranslator
    {
        private readonly IModel _model;
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        public RelationalEntityQueryableTranslator(IModel model, ISqlExpressionFactory sqlExpressionFactory)
        {
            _model = model;
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                && methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSqlOnQueryable))
            {
                var sql = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                var queryable = (IQueryable)((ConstantExpression)methodCallExpression.Arguments[0]).Value;
                return CreateShapedQueryExpression(queryable.ElementType, sql, methodCallExpression.Arguments[2]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override ShapedQueryExpression CreateShapedQueryExpression(Type elementType)
        {
            var entityType = _model.FindEntityType(elementType);
            var queryExpression = _sqlExpressionFactory.Select(entityType);

            return CreateShapedQueryExpression(entityType, queryExpression);
        }

        protected virtual ShapedQueryExpression CreateShapedQueryExpression(Type elementType, string sql, Expression arguments)
        {
            var entityType = _model.FindEntityType(elementType);
            var queryExpression = _sqlExpressionFactory.Select(entityType, sql, arguments);

            return CreateShapedQueryExpression(entityType, queryExpression);
        }

        private ShapedQueryExpression CreateShapedQueryExpression(IEntityType entityType, SelectExpression selectExpression)
        {
            return new ShapedQueryExpression(
                selectExpression,
                new EntityShaperExpression(
                entityType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));
        }
    }
}
