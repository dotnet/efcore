// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(expression.QueryModel);

            return queryModelVisitor.Expression;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual EntityQueryModelVisitor CreateQueryModelVisitor()
            => QueryModelVisitor.QueryCompilationContext
                .CreateQueryModelVisitor(_entityQueryModelVisitor);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node.Name.StartsWith(CompiledQueryCache.CompiledQueryParameterPrefix, StringComparison.Ordinal)
                ? Expression.Call(
                    GetParameterValueMethodInfo.MakeGenericMethod(node.Type),
                    EntityQueryModelVisitor.QueryContextParameter,
                    Expression.Constant(node.Name))
                : (Expression)node;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression node)
        {
            if (node is NullConditionalExpression nullConditionalExpression)
            {
                var newCaller = Visit(nullConditionalExpression.Caller);
                var newAccessOperation = Visit(nullConditionalExpression.AccessOperation);

                return newCaller != nullConditionalExpression.Caller
                       || newAccessOperation != nullConditionalExpression.AccessOperation
                    ? new NullConditionalExpression(newCaller, newAccessOperation)
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
