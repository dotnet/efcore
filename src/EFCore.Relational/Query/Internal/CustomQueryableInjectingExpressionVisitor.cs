// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class CustomQueryableInjectingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                && methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSqlOnQueryable))
            {
                var sql = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                var entityType = ((IEntityQueryable)((ConstantExpression)methodCallExpression.Arguments[0]).Value).EntityType;

                return CreateFromSqlQueryableExpression(entityType, sql, methodCallExpression.Arguments[2]);
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private static ConstantExpression CreateFromSqlQueryableExpression(IEntityType entityType, string sql, Expression argument)
        {
            return Expression.Constant(
                _createFromSqlQueryableMethod
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(
                        null, new object[] { NullAsyncQueryProvider.Instance, entityType, sql, argument }));
        }

        private static readonly MethodInfo _createFromSqlQueryableMethod
            = typeof(CustomQueryableInjectingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(CreateFromSqlQueryable));

        [UsedImplicitly]
        private static FromSqlQueryable<TResult> CreateFromSqlQueryable<TResult>(
            IAsyncQueryProvider entityQueryProvider, IEntityType entityType, string sql, Expression argument)
            => new FromSqlQueryable<TResult>(entityQueryProvider, entityType, sql, argument);
    }
}
