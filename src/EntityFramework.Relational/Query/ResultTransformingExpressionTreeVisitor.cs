// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class ResultTransformingExpressionTreeVisitor : ExpressionTreeVisitor
    {
        private readonly IQuerySource _outerQuerySource;
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        public ResultTransformingExpressionTreeVisitor(
            [NotNull] IQuerySource outerQuerySource,
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(outerQuerySource, "outerQuerySource");
            Check.NotNull(relationalQueryCompilationContext, "relationalQueryCompilationContext");

            _outerQuerySource = outerQuerySource;
            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        private static readonly MethodInfo _getValueMethodInfo
            = typeof(ResultTransformingExpressionTreeVisitor).GetTypeInfo()
                .GetDeclaredMethod("GetValue");

        [UsedImplicitly]
        private static QuerySourceScope<bool> GetValue(
            IQuerySource querySource,
            QuerySourceScope parentQuerySourceScope,
            DbDataReader dataReader)
        {
            return new QuerySourceScope<bool>(
                querySource,
                dataReader.GetBoolean(0),
                parentQuerySourceScope);
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
        {
            var newArguments = VisitAndConvert(expression.Arguments, "VisitMethodCallExpression");

            if ((expression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo)
                 || expression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateValueReaderMethodInfo))
                && ((ConstantExpression)expression.Arguments[0]).Value == _outerQuerySource)
            {
                return
                    Expression.Call(
                        _getValueMethodInfo,
                        expression.Arguments[0],
                        expression.Arguments[2],
                        expression.Arguments[3]);
            }

            if (expression.Method.MethodIsClosedFormOf(
                QuerySourceScope.GetResultMethodInfo)
                && ((ConstantExpression)expression.Arguments[0]).Value == _outerQuerySource)
            {
                return
                    QuerySourceScope.GetResult(
                        expression.Object,
                        _outerQuerySource,
                        typeof(bool));
            }

            if (newArguments != expression.Arguments)
            {
                if (expression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod
                            .MakeGenericMethod(typeof(QuerySourceScope<bool>)),
                        newArguments);
                }

                if (expression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.LinqOperatorProvider.Select))
                {
                    return
                        Expression.Call(
                            _relationalQueryCompilationContext.LinqOperatorProvider.First
                                .MakeGenericMethod(typeof(bool)),
                            Expression.Call(
                                _relationalQueryCompilationContext.LinqOperatorProvider.Select
                                    .MakeGenericMethod(
                                        typeof(QuerySourceScope),
                                        typeof(bool)),
                                newArguments));
                }

                return Expression.Call(expression.Method, newArguments);
            }

            return expression;
        }

        protected override Expression VisitLambdaExpression(LambdaExpression expression)
        {
            var newBodyExpression = VisitExpression(expression.Body);

            return newBodyExpression != expression.Body
                ? Expression.Lambda(newBodyExpression, expression.Parameters)
                : expression;
        }
    }
}
