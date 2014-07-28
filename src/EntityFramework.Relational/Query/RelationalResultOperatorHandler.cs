// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalResultOperatorHandler : IResultOperatorHandler
    {
        private class HandlerContext
        {
            private readonly IResultOperatorHandler _resultOperatorHandler;
            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private readonly ResultOperatorBase _resultOperator;
            private readonly QueryModel _queryModel;
            private readonly SelectExpression _selectExpression;

            public HandlerContext(
                IResultOperatorHandler resultOperatorHandler,
                RelationalQueryModelVisitor queryModelVisitor,
                ResultOperatorBase resultOperator,
                QueryModel queryModel,
                SelectExpression selectExpression)
            {
                _resultOperatorHandler = resultOperatorHandler;
                _queryModelVisitor = queryModelVisitor;
                _resultOperator = resultOperator;
                _queryModel = queryModel;
                _selectExpression = selectExpression;
            }

            public ResultOperatorBase ResultOperator
            {
                get { return _resultOperator; }
            }

            public SelectExpression SelectExpression
            {
                get { return _selectExpression; }
            }

            public QueryModel QueryModel
            {
                get { return _queryModel; }
            }

            public RelationalQueryModelVisitor QueryModelVisitor
            {
                get { return _queryModelVisitor; }
            }

            public Expression EvalOnServer
            {
                get { return _queryModelVisitor.Expression; }
            }

            public Expression EvalOnClient
            {
                get
                {
                    return _resultOperatorHandler
                        .HandleResultOperator(_queryModelVisitor, _resultOperator, _queryModel);
                }
            }
        }

        private static readonly Dictionary<Type, Func<HandlerContext, Expression>>
            _resultHandlers = new Dictionary<Type, Func<HandlerContext, Expression>>
                {
                    { typeof(AnyResultOperator), HandleAny },
                    { typeof(TakeResultOperator), HandleTake },
                    { typeof(SingleResultOperator), HandleSingle },
                    { typeof(FirstResultOperator), HandleFirst },
                    { typeof(DistinctResultOperator), HandleDistinct }
                };

        private readonly IResultOperatorHandler _resultOperatorHandler;

        public RelationalResultOperatorHandler([NotNull] IResultOperatorHandler resultOperatorHandler)
        {
            Check.NotNull(resultOperatorHandler, "resultOperatorHandler");

            _resultOperatorHandler = resultOperatorHandler;
        }

        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, "entityQueryModelVisitor");
            Check.NotNull(resultOperator, "resultOperator");
            Check.NotNull(queryModel, "queryModel");

            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)entityQueryModelVisitor;

            var selectExpression
                = relationalQueryModelVisitor
                    .TryGetSelectExpression(queryModel.MainFromClause);

            var handlerContext
                = new HandlerContext(
                    _resultOperatorHandler,
                    relationalQueryModelVisitor,
                    resultOperator,
                    queryModel,
                    selectExpression);

            Func<HandlerContext, Expression> resultHandler;
            if (relationalQueryModelVisitor.RequiresClientFilter
                || !_resultHandlers.TryGetValue(resultOperator.GetType(), out resultHandler)
                || selectExpression == null)
            {
                return handlerContext.EvalOnClient;
            }

            return resultHandler(handlerContext);
        }

        private static Expression HandleAny(HandlerContext handlerContext)
        {
            var innerSelectExpression = new SelectExpression();

            innerSelectExpression.AddTables(handlerContext.SelectExpression.Tables);

            handlerContext.SelectExpression
                .SetProjection(new CaseExpression(new ExistsExpression(innerSelectExpression)));

            return new ResultTransformingExpressionTreeVisitor(
                handlerContext.QueryModel.MainFromClause,
                (RelationalQueryCompilationContext)handlerContext.QueryModelVisitor.QueryCompilationContext)
                .VisitExpression(handlerContext.QueryModelVisitor.Expression);
        }

        private class ResultTransformingExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly IQuerySource _outerQuerySource;
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

            public ResultTransformingExpressionTreeVisitor(
                IQuerySource outerQuerySource, RelationalQueryCompilationContext relationalQueryCompilationContext)
            {
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

                if ((MethodIsClosedFormOf(expression.Method, RelationalQueryModelVisitor.CreateEntityMethodInfo)
                     || MethodIsClosedFormOf(expression.Method, RelationalQueryModelVisitor.CreateValueReaderMethodInfo))
                    && ((ConstantExpression)expression.Arguments[0]).Value == _outerQuerySource)
                {
                    return
                        Expression.Call(
                            _getValueMethodInfo,
                            expression.Arguments[0],
                            expression.Arguments[2],
                            expression.Arguments[3]);
                }

                if (MethodIsClosedFormOf(
                    expression.Method,
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
                    if (MethodIsClosedFormOf(
                        expression.Method,
                        _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod))
                    {
                        return Expression.Call(
                            _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod
                                .MakeGenericMethod(typeof(QuerySourceScope<bool>)),
                            newArguments);
                    }

                    if (MethodIsClosedFormOf(
                        expression.Method,
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

            private static bool MethodIsClosedFormOf(MethodInfo method, MethodInfo genericMethod)
            {
                return method.IsGenericMethod
                       && ReferenceEquals(
                           method.GetGenericMethodDefinition(),
                           genericMethod);
            }
        }

        private static Expression HandleTake(HandlerContext handlerContext)
        {
            var takeResultOperator
                = (TakeResultOperator)handlerContext.ResultOperator;

            handlerContext.SelectExpression
                .AddLimit(takeResultOperator.GetConstantCount());

            return handlerContext.EvalOnServer;
        }

        private static Expression HandleSingle(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.AddLimit(2);

            return handlerContext.EvalOnClient;
        }

        private static Expression HandleFirst(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.AddLimit(1);

            return handlerContext.EvalOnClient;
        }

        private static Expression HandleDistinct(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.IsDistinct = true;
            handlerContext.SelectExpression.ClearOrderBy();

            return handlerContext.EvalOnServer;
        }
    }
}
