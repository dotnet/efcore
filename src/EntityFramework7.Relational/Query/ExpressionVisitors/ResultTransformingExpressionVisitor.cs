// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class ResultTransformingExpressionVisitor<TResult> : ExpressionVisitorBase
    {
        private readonly IQuerySource _outerQuerySource;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        public ResultTransformingExpressionVisitor(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(outerQuerySource, nameof(outerQuerySource));
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));

            _outerQuerySource = outerQuerySource;
            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            var newObject = Visit(methodCallExpression.Object);

            if (newObject != methodCallExpression.Object)
            {
                return newObject;
            }

            var newArguments = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCall");

            if ((methodCallExpression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo)
                 || ReferenceEquals(methodCallExpression.Method, RelationalQueryModelVisitor.CreateValueBufferMethodInfo))
                && ((ConstantExpression)methodCallExpression.Arguments[0]).Value == _outerQuerySource)
            {
                return methodCallExpression.Arguments[3]; // valueBuffer
            }

            if (newArguments != methodCallExpression.Arguments)
            {
                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod
                            .MakeGenericMethod(typeof(ValueBuffer)),
                        newArguments);
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.LinqOperatorProvider.Select))
                {
                    return ResultOperatorHandler.CallWithPossibleCancellationToken(
                        _relationalQueryCompilationContext.QueryMethodProvider.GetResultMethod
                            .MakeGenericMethod(typeof(TResult)),
                        newArguments[0]);
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany
                            .MakeGenericMethod(typeof(QueryResultScope), typeof(ValueBuffer)),
                        newArguments);
                }

                return Expression.Call(methodCallExpression.Method, newArguments);
            }

            return methodCallExpression;
        }

        protected override Expression VisitLambda<T>(Expression<T> lambdaExpression)
        {
            Check.NotNull(lambdaExpression, nameof(lambdaExpression));

            var newBodyExpression = Visit(lambdaExpression.Body);

            return newBodyExpression != lambdaExpression.Body
                ? Expression.Lambda(newBodyExpression, lambdaExpression.Parameters)
                : lambdaExpression;
        }
    }
}
