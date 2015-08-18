// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Query
{
    public class RelationalResultOperatorHandler : IResultOperatorHandler
    {
        private sealed class HandlerContext
        {
            private readonly IResultOperatorHandler _resultOperatorHandler;

            public HandlerContext(
                IResultOperatorHandler resultOperatorHandler,
                RelationalQueryModelVisitor queryModelVisitor,
                ResultOperatorBase resultOperator,
                QueryModel queryModel,
                SelectExpression selectExpression)
            {
                _resultOperatorHandler = resultOperatorHandler;
                QueryModelVisitor = queryModelVisitor;
                ResultOperator = resultOperator;
                QueryModel = queryModel;
                SelectExpression = selectExpression;
            }

            public ResultOperatorBase ResultOperator { get; }

            public SelectExpression SelectExpression { get; }

            public QueryModel QueryModel { get; }

            public RelationalQueryModelVisitor QueryModelVisitor { get; }

            public Expression EvalOnServer => QueryModelVisitor.Expression;

            public Expression EvalOnClient(bool requiresClientResultOperator = true)
            {
                QueryModelVisitor.RequiresClientResultOperator = requiresClientResultOperator;

                return _resultOperatorHandler
                    .HandleResultOperator(QueryModelVisitor, ResultOperator, QueryModel);
            }
        }

        private static readonly Dictionary<Type, Func<HandlerContext, Expression>>
            _resultHandlers = new Dictionary<Type, Func<HandlerContext, Expression>>
                {
                    { typeof(AllResultOperator), HandleAll },
                    { typeof(AnyResultOperator), HandleAny },
                    { typeof(CastResultOperator), HandleCast },
                    { typeof(ContainsResultOperator), HandleContains },
                    { typeof(CountResultOperator), HandleCount },
                    { typeof(LongCountResultOperator), HandleLongCount },
                    { typeof(DistinctResultOperator), HandleDistinct },
                    { typeof(FirstResultOperator), HandleFirst },
                    { typeof(LastResultOperator), HandleLast },
                    { typeof(MaxResultOperator), HandleMax },
                    { typeof(MinResultOperator), HandleMin },
                    { typeof(OfTypeResultOperator), HandleOfType },
                    { typeof(SingleResultOperator), HandleSingle },
                    { typeof(SkipResultOperator), HandleSkip },
                    { typeof(SumResultOperator), HandleSum },
                    { typeof(TakeResultOperator), HandleTake }
                };

        private readonly IResultOperatorHandler _resultOperatorHandler = new ResultOperatorHandler();

        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));
            Check.NotNull(resultOperator, nameof(resultOperator));
            Check.NotNull(queryModel, nameof(queryModel));

            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)entityQueryModelVisitor;

            var selectExpression
                = relationalQueryModelVisitor
                    .TryGetQuery(queryModel.MainFromClause);

            var handlerContext
                = new HandlerContext(
                    _resultOperatorHandler,
                    relationalQueryModelVisitor,
                    resultOperator,
                    queryModel,
                    selectExpression);

            Func<HandlerContext, Expression> resultHandler;
            if (relationalQueryModelVisitor.RequiresClientFilter
                || relationalQueryModelVisitor.RequiresClientResultOperator
                || relationalQueryModelVisitor.RequiresClientSelectMany
                || !_resultHandlers.TryGetValue(resultOperator.GetType(), out resultHandler)
                || selectExpression == null)
            {
                return handlerContext.EvalOnClient();
            }

            return resultHandler(handlerContext);
        }

        private static Expression HandleAll(HandlerContext handlerContext)
        {
            var filteringVisitor
                = new SqlTranslatingExpressionVisitor(
                    handlerContext.QueryModelVisitor,
                    handlerContext.SelectExpression);

            var predicate
                = filteringVisitor.Visit(
                    ((AllResultOperator)handlerContext.ResultOperator).Predicate);

            if (predicate != null)
            {
                var innerSelectExpression = new SelectExpression();

                innerSelectExpression.AddTables(handlerContext.SelectExpression.Tables);
                innerSelectExpression.Predicate = Expression.Not(predicate);

                if (handlerContext.SelectExpression.Predicate != null)
                {
                    innerSelectExpression.Predicate
                        = Expression.AndAlso(
                            handlerContext.SelectExpression.Predicate,
                            innerSelectExpression.Predicate);
                }

                SetProjectionConditionalExpression(
                    handlerContext,
                    Expression.Condition(
                        Expression.Not(new ExistsExpression(innerSelectExpression)),
                        Expression.Constant(true),
                        Expression.Constant(false),
                        typeof(bool)));

                return TransformClientExpression<bool>(handlerContext);
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleAny(HandlerContext handlerContext)
        {
            var innerSelectExpression = new SelectExpression();

            innerSelectExpression.AddTables(handlerContext.SelectExpression.Tables);
            innerSelectExpression.Predicate = handlerContext.SelectExpression.Predicate;

            SetProjectionConditionalExpression(
                handlerContext,
                Expression.Condition(
                    new ExistsExpression(innerSelectExpression),
                    Expression.Constant(true),
                    Expression.Constant(false),
                    typeof(bool)));

            return TransformClientExpression<bool>(handlerContext);
        }

        private static Expression HandleCast(HandlerContext handlerContext)
            => handlerContext.EvalOnClient(requiresClientResultOperator: false);

        private static Expression HandleContains(HandlerContext handlerContext)
            => handlerContext.EvalOnClient(requiresClientResultOperator: false);

        private static Expression HandleCount(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression
                .SetProjectionExpression(new CountExpression());

            handlerContext.SelectExpression.ClearOrderBy();

            return TransformClientExpression<int>(handlerContext);
        }

        private static Expression HandleLongCount(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression
                .SetProjectionExpression(new CountExpression(typeof(long)));

            handlerContext.SelectExpression.ClearOrderBy();

            return TransformClientExpression<long>(handlerContext);
        }

        private static Expression HandleMin(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection)
            {
                var minExpression
                    = new MinExpression(handlerContext.SelectExpression.Projection.Single());

                handlerContext.SelectExpression.SetProjectionExpression(minExpression);

                return (Expression)_transformClientExpressionMethodInfo
                    .MakeGenericMethod(minExpression.Type)
                    .Invoke(null, new object[] { handlerContext });
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleMax(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection)
            {
                var maxExpression
                    = new MaxExpression(handlerContext.SelectExpression.Projection.Single());

                handlerContext.SelectExpression.SetProjectionExpression(maxExpression);

                return (Expression)_transformClientExpressionMethodInfo
                    .MakeGenericMethod(maxExpression.Type)
                    .Invoke(null, new object[] { handlerContext });
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleSum(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection)
            {
                var sumExpression
                    = new SumExpression(handlerContext.SelectExpression.Projection.Single());

                handlerContext.SelectExpression.SetProjectionExpression(sumExpression);

                return (Expression)_transformClientExpressionMethodInfo
                    .MakeGenericMethod(sumExpression.Type)
                    .Invoke(null, new object[] { handlerContext });
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleDistinct(HandlerContext handlerContext)
        {
            var selectExpression = handlerContext.SelectExpression;

            selectExpression.IsDistinct = true;

            if (selectExpression.OrderBy.Any(o =>
                {
                    var orderByColumnExpression = o.Expression.TryGetColumnExpression();

                    if (orderByColumnExpression == null)
                    {
                        return true;
                    }

                    return !selectExpression.Projection.Any(e =>
                        {
                            var projectionColumnExpression = e.TryGetColumnExpression();

                            return projectionColumnExpression != null
                                   && projectionColumnExpression.Equals(orderByColumnExpression);
                        });
                }))
            {
                handlerContext.SelectExpression.ClearOrderBy();
            }

            return handlerContext.EvalOnServer;
        }

        private static Expression HandleFirst(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.Limit = 1;

            return handlerContext.EvalOnClient(requiresClientResultOperator: false);
        }

        private static Expression HandleLast(HandlerContext handlerContext)
        {
            if (handlerContext.SelectExpression.OrderBy.Any())
            {
                foreach (var ordering in handlerContext.SelectExpression.OrderBy)
                {
                    ordering.OrderingDirection
                        = ordering.OrderingDirection == OrderingDirection.Asc
                            ? OrderingDirection.Desc
                            : OrderingDirection.Asc;
                }

                handlerContext.SelectExpression.Limit = 1;
            }

            return handlerContext.EvalOnClient(requiresClientResultOperator: false);
        }

        private static Expression HandleOfType(HandlerContext handlerContext)
        {
            var ofTypeResultOperator
                = (OfTypeResultOperator)handlerContext.ResultOperator;

            var entityType
                = handlerContext.QueryModelVisitor.QueryCompilationContext.Model
                    .FindEntityType(ofTypeResultOperator.SearchedItemType);

            if (entityType == null)
            {
                return handlerContext.EvalOnClient();
            }

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToArray();

            if (concreteEntityTypes.Length != 1
                || concreteEntityTypes[0].RootType() != concreteEntityTypes[0])
            {
                var extensions = handlerContext.QueryModelVisitor.QueryCompilationContext.RelationalExtensions;

                var discriminatorProperty = extensions.For(concreteEntityTypes[0]).DiscriminatorProperty;

                var discriminatorColumn
                    = handlerContext.SelectExpression.Projection
                        .OfType<AliasExpression>()
                        .Single(c => c.TryGetColumnExpression()?.Property == discriminatorProperty);

                var discriminatorPredicate
                    = concreteEntityTypes
                        .Select(concreteEntityType =>
                            Expression.Equal(
                                discriminatorColumn,
                                Expression.Constant(extensions.For(concreteEntityType).DiscriminatorValue)))
                        .Aggregate((current, next) => Expression.OrElse(next, current));

                handlerContext.SelectExpression.Predicate
                    = new DiscriminatorReplacingExpressionVisitor(
                        discriminatorPredicate,
                        handlerContext.QueryModel.MainFromClause)
                        .Visit(handlerContext.SelectExpression.Predicate);
            }

            return Expression.Call(
                handlerContext.QueryModelVisitor.LinqOperatorProvider.Cast
                    .MakeGenericMethod(ofTypeResultOperator.SearchedItemType),
                handlerContext.QueryModelVisitor.Expression);
        }

        private class DiscriminatorReplacingExpressionVisitor : RelinqExpressionVisitor
        {
            private readonly Expression _discriminatorPredicate;
            private readonly IQuerySource _querySource;

            public DiscriminatorReplacingExpressionVisitor(
                Expression discriminatorPredicate, IQuerySource querySource)
            {
                _discriminatorPredicate = discriminatorPredicate;
                _querySource = querySource;
            }

            protected override Expression VisitExtension(Expression expression)
            {
                var discriminatorExpression = expression as DiscriminatorPredicateExpression;

                if (discriminatorExpression != null
                    && discriminatorExpression.QuerySource == _querySource)
                {
                    return new DiscriminatorPredicateExpression(_discriminatorPredicate, _querySource);
                }

                return expression;
            }
        }

        private static Expression HandleSingle(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.Limit = 2;

            return handlerContext.EvalOnClient(requiresClientResultOperator: false);
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

        private static void SetProjectionConditionalExpression(
            HandlerContext handlerContext, ConditionalExpression conditionalExpression)
        {
            handlerContext.SelectExpression.SetProjectionConditionalExpression(conditionalExpression);
            handlerContext.SelectExpression.ClearTables();
            handlerContext.SelectExpression.ClearOrderBy();
            handlerContext.SelectExpression.Predicate = null;
        }

        private static readonly MethodInfo _transformClientExpressionMethodInfo
            = typeof(RelationalResultOperatorHandler).GetTypeInfo()
                .GetDeclaredMethod(nameof(TransformClientExpression));

        private static Expression TransformClientExpression<TResult>(HandlerContext handlerContext)
        {
            var querySource
                = handlerContext.QueryModel.BodyClauses
                    .OfType<IQuerySource>()
                    .LastOrDefault()
                  ?? handlerContext.QueryModel.MainFromClause;

            var visitor
                = new ResultTransformingExpressionVisitor<TResult>(
                    querySource,
                    handlerContext.QueryModelVisitor.QueryCompilationContext);

            return visitor.Visit(handlerContext.QueryModelVisitor.Expression);
        }
    }
}
