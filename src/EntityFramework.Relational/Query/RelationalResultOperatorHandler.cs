// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

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
                    { typeof(AllResultOperator), HandleAll },
                    { typeof(AnyResultOperator), HandleAny },
                    { typeof(CountResultOperator), HandleCount },
                    { typeof(TakeResultOperator), HandleTake },
                    { typeof(SingleResultOperator), HandleSingle },
                    { typeof(FirstResultOperator), HandleFirst },
                    { typeof(DistinctResultOperator), HandleDistinct },
                    { typeof(SkipResultOperator), HandleSkip }
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

        private static Expression HandleAll(HandlerContext handlerContext)
        {
            var filteringVisitor
                = new RelationalQueryModelVisitor.FilteringExpressionTreeVisitor(handlerContext.QueryModelVisitor);

            var predicate
                = filteringVisitor.VisitExpression(
                    ((AllResultOperator)handlerContext.ResultOperator).Predicate);

            if (!filteringVisitor.RequiresClientEval)
            {
                var innerSelectExpression = new SelectExpression();

                innerSelectExpression.AddTables(handlerContext.SelectExpression.Tables);
                innerSelectExpression.Predicate = Expression.Not(predicate);

                SetProjectionCaseExpression(
                    handlerContext,
                    new CaseExpression(Expression.Not(new ExistsExpression(innerSelectExpression))));

                return TransformClientExpression<bool>(handlerContext);
            }

            return handlerContext.EvalOnClient;
        }

        private static Expression HandleAny(HandlerContext handlerContext)
        {
            var innerSelectExpression = new SelectExpression();

            innerSelectExpression.AddTables(handlerContext.SelectExpression.Tables);
            innerSelectExpression.Predicate = handlerContext.SelectExpression.Predicate;

            SetProjectionCaseExpression(
                handlerContext,
                new CaseExpression(new ExistsExpression(innerSelectExpression)));

            return TransformClientExpression<bool>(handlerContext);
        }

        private static Expression HandleCount(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression
                .SetProjectionCountExpression(new CountExpression());

            return TransformClientExpression<int>(handlerContext);
        }

        private static Expression HandleDistinct(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.IsDistinct = true;
            handlerContext.SelectExpression.ClearOrderBy();

            return handlerContext.EvalOnServer;
        }

        private static Expression HandleFirst(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.Limit = 1;

            return handlerContext.EvalOnClient;
        }

        private static Expression HandleSingle(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.Limit = 2;

            return handlerContext.EvalOnClient;
        }

        private static Expression HandleSkip(HandlerContext handlerContext)
        {
            var skipResultOperator = (SkipResultOperator)handlerContext.ResultOperator;

            handlerContext.SelectExpression.Offset = skipResultOperator.GetConstantCount();

            return handlerContext.EvalOnServer;
        }

        private static Expression HandleTake(HandlerContext handlerContext)
        {
            var takeResultOperator = (TakeResultOperator)handlerContext.ResultOperator;

            handlerContext.SelectExpression.Limit = takeResultOperator.GetConstantCount();

            return handlerContext.EvalOnServer;
        }

        private static void SetProjectionCaseExpression(HandlerContext handlerContext, CaseExpression caseExpression)
        {
            handlerContext.SelectExpression.SetProjectionCaseExpression(caseExpression);
            handlerContext.SelectExpression.ClearTables();
            handlerContext.SelectExpression.ClearOrderBy();
            handlerContext.SelectExpression.Predicate = null;
        }

        private static Expression TransformClientExpression<TResult>(HandlerContext handlerContext)
        {
            return new ResultTransformingExpressionTreeVisitor<TResult>(
                handlerContext.QueryModel.MainFromClause,
                (RelationalQueryCompilationContext)handlerContext.QueryModelVisitor.QueryCompilationContext)
                .VisitExpression(handlerContext.QueryModelVisitor.Expression);
        }
    }
}
