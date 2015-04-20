// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class ResultTransformingExpressionTreeVisitor<TResult> : ExpressionTreeVisitorBase
    {
        private readonly IQuerySource _outerQuerySource;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        private MethodCallExpression _root;

        public ResultTransformingExpressionTreeVisitor(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(outerQuerySource, nameof(outerQuerySource));
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));

            _outerQuerySource = outerQuerySource;
            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            _root = _root ?? methodCallExpression;

            var newObject = VisitExpression(methodCallExpression.Object);

            if (newObject != methodCallExpression.Object)
            {
                return newObject;
            }

            var newArguments = VisitAndConvert(methodCallExpression.Arguments, "VisitMethodCallExpression");

            if ((methodCallExpression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo)
                 || ReferenceEquals(methodCallExpression.Method, RelationalQueryModelVisitor.CreateValueReaderMethodInfo))
                && ((ConstantExpression)methodCallExpression.Arguments[0]).Value == _outerQuerySource)
            {
                return methodCallExpression.Arguments[3];
            }

            if (newArguments != methodCallExpression.Arguments)
            {
                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod
                            .MakeGenericMethod(typeof(DbDataReader)),
                        newArguments);
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.LinqOperatorProvider.Select))
                {
                    if (methodCallExpression == _root)
                    {
                        return ResultOperatorHandler.CallWithPossibleCancellationToken(
                            _relationalQueryCompilationContext.QueryMethodProvider.GetResultMethod
                                .MakeGenericMethod(typeof(TResult)),
                            newArguments[0]);
                    }

                    return newArguments[0];
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany
                            .MakeGenericMethod(typeof(QuerySourceScope), typeof(DbDataReader)),
                        newArguments);
                }

                return Expression.Call(methodCallExpression.Method, newArguments);
            }

            return methodCallExpression;
        }

        protected override Expression VisitLambdaExpression([NotNull] LambdaExpression lambdaExpression)
        {
            Check.NotNull(lambdaExpression, nameof(lambdaExpression));

            var newBodyExpression = VisitExpression(lambdaExpression.Body);

            return newBodyExpression != lambdaExpression.Body
                ? Expression.Lambda(newBodyExpression, lambdaExpression.Parameters)
                : lambdaExpression;
        }
    }
}
