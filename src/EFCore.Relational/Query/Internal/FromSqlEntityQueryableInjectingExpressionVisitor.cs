// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class FromSqlEntityQueryableInjectingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(RelationalQueryableExtensions)
                && methodCallExpression.Method.Name == nameof(RelationalQueryableExtensions.FromSqlOnQueryable))
            {
                var sql = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;
                var queryable = (IQueryable)((ConstantExpression)methodCallExpression.Arguments[0]).Value;

                return Expression.Constant(
                    _createFromSqlEntityQueryableMethod
                        .MakeGenericMethod(queryable.ElementType)
                        .Invoke(null, new object[] { queryable.Provider, sql, methodCallExpression.Arguments[2] }));
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private static readonly MethodInfo _createFromSqlEntityQueryableMethod
            = typeof(FromSqlEntityQueryableInjectingExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(_CreateFromSqlEntityQueryable));

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static FromSqlEntityQueryable<TResult> _CreateFromSqlEntityQueryable<TResult>(
            IAsyncQueryProvider entityQueryProvider,
            string sql,
            Expression arguments)
            => new FromSqlEntityQueryable<TResult>(entityQueryProvider, sql, arguments);
    }
}
