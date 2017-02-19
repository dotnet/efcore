// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DefaultQueryExpressionVisitor : ExpressionVisitorBase
    {
        private readonly EntityQueryModelVisitor _entityQueryModelVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DefaultQueryExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
        {
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));

            _entityQueryModelVisitor = entityQueryModelVisitor;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityQueryModelVisitor QueryModelVisitor => _entityQueryModelVisitor;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var compilationContext = QueryModelVisitor.QueryCompilationContext;

            var subQueryModelVisitor = compilationContext.GetQueryModelVisitor(expression.QueryModel);

            if (subQueryModelVisitor == null)
            {
                subQueryModelVisitor
                    = compilationContext.CreateQueryModelVisitor(
                        expression.QueryModel,
                        QueryModelVisitor);

                subQueryModelVisitor.VisitQueryModel(expression.QueryModel);
            }

            return subQueryModelVisitor.Expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name
                .StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix, StringComparison.Ordinal))
            {
                return Expression.Call(
                    GetParameterValueMethodInfo.MakeGenericMethod(node.Type),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(node.Name));
            }

            return node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            var nullConditionalExpression = node as NullConditionalExpression;
            if (nullConditionalExpression != null)
            {
                var newNullableCaller = Visit(nullConditionalExpression.NullableCaller);
                var newCaller = Visit(nullConditionalExpression.Caller);
                var newAccessOperation = Visit(nullConditionalExpression.AccessOperation);

                return newNullableCaller != nullConditionalExpression.NullableCaller
                    || newCaller != nullConditionalExpression.Caller
                    || newAccessOperation != nullConditionalExpression.AccessOperation
                    ? new NullConditionalExpression(newNullableCaller, newCaller, newAccessOperation)
                    : node;
            }

            return base.VisitExtension(node);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo GetParameterValueMethodInfo
            = typeof(DefaultQueryExpressionVisitor)
                .GetTypeInfo().GetDeclaredMethod(nameof(GetParameterValue));

        [UsedImplicitly]
        private static T GetParameterValue<T>(QueryContext queryContext, string parameterName)
            => (T)queryContext.ParameterValues[parameterName];
    }
}
