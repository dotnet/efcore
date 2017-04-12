// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ResultTransformingExpressionVisitor<TResult> : ExpressionVisitorBase
    {
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ResultTransformingExpressionVisitor(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(outerQuerySource, nameof(outerQuerySource));
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));

            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
                return ResultOperatorHandler.CallWithPossibleCancellationToken(
                    _relationalQueryCompilationContext.QueryMethodProvider
                        .GetResultMethod.MakeGenericMethod(typeof(TResult)),
                    Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod,
                        EntityQueryModelVisitor.QueryContextParameter,
                        Expression.Constant(shapedQueryExpression.ShaperCommandContext),
                        Expression.Default(typeof(int?))));
            }

            return base.VisitExtension(extensionExpression);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.MethodIsClosedFormOf(
                _relationalQueryCompilationContext.QueryMethodProvider.InjectParametersMethod))
            {
                var sourceArgument = (MethodCallExpression)Visit(methodCallExpression.Arguments[1]);
                if (sourceArgument.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.GetResultMethod))
                {
                    var getResultArgument = sourceArgument.Arguments[0];
                    var newGetResultArgument = Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.InjectParametersMethod.MakeGenericMethod(typeof(ValueBuffer)),
                        methodCallExpression.Arguments[0], getResultArgument, methodCallExpression.Arguments[2], methodCallExpression.Arguments[3]);

                    return ResultOperatorHandler.CallWithPossibleCancellationToken(sourceArgument.Method, newGetResultArgument);
                }

                return sourceArgument;
            }

            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var expression in methodCallExpression.Arguments)
            {
                var newExpression = Visit(expression);

                if (newExpression != expression)
                {
                    return newExpression;
                }
            }

            return methodCallExpression;
        }
    }
}
