// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class RelationalResultOperatorHandler : IRelationalResultOperatorHandler
    {
        private sealed class HandlerContext
        {
            private readonly IResultOperatorHandler _resultOperatorHandler;
            private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;

            public HandlerContext(
                IResultOperatorHandler resultOperatorHandler,
                IModel model,
                ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
                ISelectExpressionFactory selectExpressionFactory,
                RelationalQueryModelVisitor queryModelVisitor,
                ResultOperatorBase resultOperator,
                QueryModel queryModel,
                SelectExpression selectExpression)
            {
                _resultOperatorHandler = resultOperatorHandler;
                _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;

                Model = model;
                SelectExpressionFactory = selectExpressionFactory;
                QueryModelVisitor = queryModelVisitor;
                ResultOperator = resultOperator;
                QueryModel = queryModel;
                SelectExpression = selectExpression;
            }

            public IModel Model { get; }
            public ISelectExpressionFactory SelectExpressionFactory { get; }
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

            public SqlTranslatingExpressionVisitor CreateSqlTranslatingVisitor()
                => _sqlTranslatingExpressionVisitorFactory.Create(QueryModelVisitor, SelectExpression);
        }

        private static readonly Dictionary<Type, Func<HandlerContext, Expression>>
            _resultHandlers = new Dictionary<Type, Func<HandlerContext, Expression>>
            {
                { typeof(AllResultOperator), HandleAll },
                { typeof(AnyResultOperator), HandleAny },
                { typeof(AverageResultOperator), HandleAverage },
                { typeof(CastResultOperator), HandleCast },
                { typeof(ContainsResultOperator), HandleContains },
                { typeof(CountResultOperator), HandleCount },
                { typeof(LongCountResultOperator), HandleLongCount },
                { typeof(DefaultIfEmptyResultOperator), HandleDefaultIfEmpty },
                { typeof(DistinctResultOperator), HandleDistinct },
                { typeof(FirstResultOperator), HandleFirst },
                { typeof(GroupResultOperator), HandleGroup },
                { typeof(LastResultOperator), HandleLast },
                { typeof(MaxResultOperator), HandleMax },
                { typeof(MinResultOperator), HandleMin },
                { typeof(SingleResultOperator), HandleSingle },
                { typeof(SkipResultOperator), HandleSkip },
                { typeof(SumResultOperator), HandleSum },
                { typeof(TakeResultOperator), HandleTake }
            };

        private readonly IModel _model;
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IResultOperatorHandler _resultOperatorHandler;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationalResultOperatorHandler(
            [NotNull] IModel model,
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IResultOperatorHandler resultOperatorHandler)
        {
            _model = model;
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _selectExpressionFactory = selectExpressionFactory;
            _resultOperatorHandler = resultOperatorHandler;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            var relationalQueryModelVisitor
                = (RelationalQueryModelVisitor)entityQueryModelVisitor;

            var selectExpression
                = relationalQueryModelVisitor
                    .TryGetQuery(queryModel.MainFromClause);

            var handlerContext
                = new HandlerContext(
                    _resultOperatorHandler,
                    _model,
                    _sqlTranslatingExpressionVisitorFactory,
                    _selectExpressionFactory,
                    relationalQueryModelVisitor,
                    resultOperator,
                    queryModel,
                    selectExpression);

            return relationalQueryModelVisitor.RequiresClientEval
                || relationalQueryModelVisitor.RequiresClientSelectMany
                || relationalQueryModelVisitor.RequiresClientJoin
                || relationalQueryModelVisitor.RequiresClientFilter
                || relationalQueryModelVisitor.RequiresClientOrderBy
                || relationalQueryModelVisitor.RequiresClientResultOperator
                || relationalQueryModelVisitor.RequiresStreamingGroupResultOperator
                || !_resultHandlers.TryGetValue(resultOperator.GetType(), out var resultHandler)
                || selectExpression == null
                ? handlerContext.EvalOnClient()
                : resultHandler(handlerContext);
        }

        private static Expression HandleAll(HandlerContext handlerContext)
        {
            var sqlTranslatingVisitor
                = handlerContext.CreateSqlTranslatingVisitor();

            PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);

            var predicate
                = sqlTranslatingVisitor.Visit(
                    ((AllResultOperator)handlerContext.ResultOperator).Predicate);

            if (predicate != null)
            {
                var innerSelectExpression = handlerContext.SelectExpression.Clone();

                innerSelectExpression.ClearProjection();
                innerSelectExpression.AddToProjection(Expression.Constant(1));
                innerSelectExpression.AddToPredicate(Expression.Not(predicate));

                if (innerSelectExpression.Limit == null
                    && innerSelectExpression.Offset == null)
                {
                    innerSelectExpression.ClearOrderBy();
                }

                SetConditionAsProjection(
                    handlerContext,
                    Expression.Not(new ExistsExpression(innerSelectExpression)));

                return TransformClientExpression<bool>(handlerContext);
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleAny(HandlerContext handlerContext)
        {
            var innerSelectExpression = handlerContext.SelectExpression.Clone();

            innerSelectExpression.ClearProjection();
            innerSelectExpression.AddToProjection(Expression.Constant(1));

            if (innerSelectExpression.Limit == null
                && innerSelectExpression.Offset == null)
            {
                innerSelectExpression.ClearOrderBy();
            }

            SetConditionAsProjection(
                handlerContext,
                new ExistsExpression(innerSelectExpression));

            return TransformClientExpression<bool>(handlerContext);
        }

        private static Expression HandleAverage(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection
                && handlerContext.SelectExpression.Projection.Count == 1)
            {
                PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);
                var expression = handlerContext.SelectExpression.Projection[0];

                if (!ContainsSelect(expression))
                {
                    expression = expression.UnwrapAliasExpression();

                    var inputType = handlerContext.QueryModel.SelectClause.Selector.Type;
                    var outputType = inputType;
                    var nonNullableInputType = inputType.UnwrapNullableType();
                    if (nonNullableInputType == typeof(int)
                        || nonNullableInputType == typeof(long))
                    {
                        outputType = inputType.IsNullableType() ? typeof(double?) : typeof(double);
                        expression = new ExplicitCastExpression(expression, outputType);
                    }

                    Expression averageExpression = new SqlFunctionExpression(
                        "AVG",
                        outputType,
                        new[] { expression });

                    if (nonNullableInputType == typeof(float))
                    {
                        averageExpression = new ExplicitCastExpression(averageExpression, inputType);
                    }

                    handlerContext.SelectExpression.SetProjectionExpression(averageExpression);

                    var averageExpressionType = averageExpression.Type;
                    var throwOnNullResult = DetermineAggregateThrowingBehavior(handlerContext, averageExpressionType);

                    return (Expression)_transformClientExpressionMethodInfo
                        .MakeGenericMethod(averageExpressionType)
                        .Invoke(null, new object[] { handlerContext, throwOnNullResult });
                }
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleCast(HandlerContext handlerContext)
            => handlerContext.EvalOnClient(requiresClientResultOperator: false);

        private static Expression HandleContains(HandlerContext handlerContext)
        {
            var filteringVisitor = handlerContext.CreateSqlTranslatingVisitor();

            var itemResultOperator = (ContainsResultOperator)handlerContext.ResultOperator;

            var item = filteringVisitor.Visit(itemResultOperator.Item);
            if (item != null)
            {
                if (item is SelectExpression itemSelectExpression)
                {
                    var queryCompilationContext = handlerContext.QueryModelVisitor.QueryCompilationContext;
                    var entityType = queryCompilationContext.FindEntityType(handlerContext.QueryModel.MainFromClause)
                                     ?? handlerContext.Model.FindEntityType(handlerContext.QueryModel.MainFromClause.ItemType);

                    if (entityType != null)
                    {
                        var outerSelectExpression = handlerContext.SelectExpressionFactory.Create(queryCompilationContext);

                        var collectionSelectExpression
                            = handlerContext.SelectExpression.Clone(queryCompilationContext.CreateUniqueTableAlias());
                        outerSelectExpression.AddTable(collectionSelectExpression);

                        itemSelectExpression.Alias = queryCompilationContext.CreateUniqueTableAlias();
                        var joinExpression = outerSelectExpression.AddInnerJoin(itemSelectExpression);

                        foreach (var property in entityType.FindPrimaryKey().Properties)
                        {
                            var itemProperty = itemSelectExpression.BindProperty(
                                property,
                                itemSelectExpression.ProjectStarTable.QuerySource);

                            itemSelectExpression.AddToProjection(itemProperty);

                            var collectionProperty = collectionSelectExpression.BindProperty(
                                property,
                                collectionSelectExpression.ProjectStarTable.QuerySource);

                            collectionSelectExpression.AddToProjection(collectionProperty);

                            var predicate = Expression.Equal(
                                collectionProperty.LiftExpressionFromSubquery(collectionSelectExpression),
                                itemProperty.LiftExpressionFromSubquery(itemSelectExpression));

                            joinExpression.Predicate
                                = joinExpression.Predicate == null
                                    ? predicate
                                    : Expression.AndAlso(
                                        joinExpression.Predicate,
                                        predicate);
                        }

                        SetConditionAsProjection(
                            handlerContext,
                            new ExistsExpression(outerSelectExpression));

                        return TransformClientExpression<bool>(handlerContext);
                    }
                }

                SetConditionAsProjection(
                    handlerContext,
                    new InExpression(
                        item,
                        handlerContext.SelectExpression.Clone("")));

                return TransformClientExpression<bool>(handlerContext);
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleCount(HandlerContext handlerContext)
        {
            PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);

            handlerContext.SelectExpression
                .SetProjectionExpression(
                    new SqlFunctionExpression(
                        "COUNT",
                        typeof(int),
                        new[] { new SqlFragmentExpression("*") }));

            handlerContext.SelectExpression.ClearOrderBy();

            return TransformClientExpression<int>(handlerContext);
        }

        private static Expression HandleDefaultIfEmpty(HandlerContext handlerContext)
        {
            var defaultIfEmptyResultOperator = (DefaultIfEmptyResultOperator)handlerContext.ResultOperator;

            if (defaultIfEmptyResultOperator.OptionalDefaultValue != null)
            {
                return handlerContext.EvalOnClient();
            }

            var selectExpression = handlerContext.SelectExpression;

            selectExpression.PushDownSubquery();
            selectExpression.ExplodeStarProjection();

            var subquery = selectExpression.Tables.Single();

            selectExpression.ClearTables();

            var emptySelectExpression = handlerContext.SelectExpressionFactory.Create(handlerContext.QueryModelVisitor.QueryCompilationContext, "empty");
            emptySelectExpression.AddToProjection(new AliasExpression("empty", Expression.Constant(null)));

            selectExpression.AddTable(emptySelectExpression);

            var leftOuterJoinExpression = new LeftOuterJoinExpression(subquery);
            var constant1 = Expression.Constant(1);

            leftOuterJoinExpression.Predicate = Expression.Equal(constant1, constant1);

            selectExpression.AddTable(leftOuterJoinExpression);

            selectExpression.ProjectStarTable = subquery;

            handlerContext.QueryModelVisitor.Expression
                = new DefaultIfEmptyExpressionVisitor(
                        handlerContext.QueryModelVisitor.QueryCompilationContext)
                    .Visit(handlerContext.QueryModelVisitor.Expression);

            return handlerContext.EvalOnClient(requiresClientResultOperator: false);
        }

        private sealed class DefaultIfEmptyExpressionVisitor : ExpressionVisitor
        {
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

            public DefaultIfEmptyExpressionVisitor(RelationalQueryCompilationContext relationalQueryCompilationContext)
                => _relationalQueryCompilationContext = relationalQueryCompilationContext;

            protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            {
                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
                {
                    return Expression.Call(
                        _relationalQueryCompilationContext.QueryMethodProvider.DefaultIfEmptyShapedQueryMethod
                            .MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()),
                        methodCallExpression.Arguments);
                }

                if (methodCallExpression.Method.MethodIsClosedFormOf(
                    _relationalQueryCompilationContext.QueryMethodProvider.InjectParametersMethod))
                {
                    var newSource = VisitMethodCall((MethodCallExpression)methodCallExpression.Arguments[1]);

                    return Expression.Call(
                        methodCallExpression.Method,
                        methodCallExpression.Arguments[0],
                        newSource,
                        methodCallExpression.Arguments[2],
                        methodCallExpression.Arguments[3]);
                }

                return base.VisitMethodCall(methodCallExpression);
            }
        }

        private static Expression HandleDistinct(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection)
            {
                handlerContext.SelectExpression.IsDistinct = true;

                return handlerContext.EvalOnServer;
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleFirst(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.Limit = Expression.Constant(1);

            var requiresClientResultOperator = !((FirstResultOperator)handlerContext.ResultOperator).ReturnDefaultWhenEmpty
                                               && handlerContext.QueryModelVisitor.ParentQueryModelVisitor != null;

            return handlerContext.EvalOnClient(requiresClientResultOperator);
        }

        private static Expression HandleGroup(HandlerContext handlerContext)
        {
            if (handlerContext.QueryModelVisitor.QueryCompilationContext.IsIncludeQuery
                && handlerContext.QueryModel.ResultOperators.Any(
                    o => o is SkipResultOperator || o is TakeResultOperator || o is ChoiceResultOperatorBase || o is DistinctResultOperator))
            {
                return handlerContext.EvalOnClient();
            }

            var sqlTranslatingExpressionVisitor = handlerContext.CreateSqlTranslatingVisitor();

            var groupResultOperator = (GroupResultOperator)handlerContext.ResultOperator;

            var sqlExpression
                = sqlTranslatingExpressionVisitor.Visit(groupResultOperator.KeySelector);

            if ((sqlExpression != null || groupResultOperator.KeySelector is ConstantExpression)
                && !handlerContext.QueryModelVisitor.QueryCompilationContext
                    .QuerySourceRequiresMaterialization(groupResultOperator)
                && (groupResultOperator.ElementSelector is QuerySourceReferenceExpression elementQsre
                    || sqlTranslatingExpressionVisitor.Visit(groupResultOperator.ElementSelector) != null)
                && handlerContext.QueryModelVisitor.Expression is MethodCallExpression shapedQueryMethod
                && shapedQueryMethod.Method.MethodIsClosedFormOf(
                    handlerContext.QueryModelVisitor.QueryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
            {
                PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);

                // GroupBy Aggregate
                // TODO: InjectParameters type Expression.
                Expression[] key = null;
                var groupByKeyMemberInfo = groupResultOperator.ItemType.GetMember("Key")[0];
                var memberInfoMappings = new Dictionary<MemberInfo, Expression>();

                switch (groupResultOperator.KeySelector)
                {
                    case MemberExpression memberExpression:
                        var sql = sqlTranslatingExpressionVisitor.Visit(memberExpression);
                        memberInfoMappings[groupByKeyMemberInfo] = sql;
                        key = new[] { sql };
                        break;

                    case MethodCallExpression methodCallExpression
                    when methodCallExpression.IsEFProperty()
                        || methodCallExpression.Method.IsEFIndexer():
                        sql = sqlTranslatingExpressionVisitor.Visit(methodCallExpression);
                        memberInfoMappings[groupByKeyMemberInfo] = sql;
                        key = new[] { sql };
                        break;

                    case UnaryExpression unaryExpression
                    when unaryExpression.RemoveConvert() is MethodCallExpression methodCallExpression
                        && methodCallExpression.Method.IsEFIndexer():
                        sql = sqlTranslatingExpressionVisitor.Visit(methodCallExpression);
                        memberInfoMappings[groupByKeyMemberInfo] = sql;
                        key = new[] { sql };
                        break;

                    case NullConditionalExpression nullConditionalExpression:
                        sql = sqlTranslatingExpressionVisitor.Visit(nullConditionalExpression);
                        memberInfoMappings[groupByKeyMemberInfo] = sql;
                        key = new[] { sql };
                        break;

                    case NewExpression newExpression:
                        key = VisitAndSaveMapping(
                            sqlTranslatingExpressionVisitor,
                            memberInfoMappings,
                            newExpression);
                        break;

                    case MemberInitExpression memberInitExpression:
                        key = VisitAndSaveMapping(
                            sqlTranslatingExpressionVisitor,
                            memberInfoMappings,
                            memberInitExpression);
                        break;
                }

                var selectExpression = handlerContext.SelectExpression;

                if (key != null
                    || groupResultOperator.KeySelector is ConstantExpression
                    || groupResultOperator.KeySelector is ParameterExpression)
                {
                    if (!(groupResultOperator.ElementSelector is QuerySourceReferenceExpression))
                    {
                        handlerContext.QueryModelVisitor.VisitSelectClause(
                            new SelectClause(groupResultOperator.ElementSelector),
                            handlerContext.QueryModel);
                    }

                    foreach (var keyValue in memberInfoMappings)
                    {
                        selectExpression.SetProjectionForMemberInfo(keyValue.Key, keyValue.Value);
                    }

                    if (key == null)
                    {
                        var constantOrParameter = sqlTranslatingExpressionVisitor.Visit(groupResultOperator.KeySelector);
                        var saveMapping = constantOrParameter != null;
                        if (!saveMapping)
                        {
                            constantOrParameter = Expression.Constant(1);
                        }

                        var subquery = selectExpression.PushDownSubquery();

                        if (subquery.Projection.Count == 0)
                        {
                            selectExpression.IsProjectStar = false;
                        }
                        else
                        {
                            selectExpression.ExplodeStarProjection();
                        }

                        var projectionIndex = subquery.AddToProjection(constantOrParameter, resetProjectStar: false);

                        subquery.SetProjectionForMemberInfo(groupByKeyMemberInfo, constantOrParameter);

                        var projection = subquery.Projection[projectionIndex].LiftExpressionFromSubquery(subquery);
                        if (saveMapping)
                        {
                            selectExpression.SetProjectionForMemberInfo(groupByKeyMemberInfo, projection);
                        }

                        key = new[] { projection };
                    }

                    if (groupResultOperator.ElementSelector.NodeType == ExpressionType.New
                        || groupResultOperator.ElementSelector.NodeType == ExpressionType.MemberInit)
                    {
                        selectExpression.ClearProjection();
                    }

                    selectExpression.AddToGroupBy(key);

                    return Expression.Call(
                        handlerContext.QueryModelVisitor.QueryCompilationContext.QueryMethodProvider
                            .ShapedQueryMethod.MakeGenericMethod(typeof(ValueBuffer)),
                        shapedQueryMethod.Arguments[0],
                        shapedQueryMethod.Arguments[1],
                        Expression.Constant(new ValueBufferShaper(groupResultOperator)));
                }
            }

            if (sqlExpression != null)
            {
                PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);

                sqlExpression
                    = sqlTranslatingExpressionVisitor.Visit(groupResultOperator.KeySelector);

                var columns = (sqlExpression as ConstantExpression)?.Value as Expression[] ?? new[] { sqlExpression };

                handlerContext.SelectExpression.PrependToOrderBy(columns.Select(c => new Ordering(c, OrderingDirection.Asc)));

                handlerContext.QueryModelVisitor.RequiresStreamingGroupResultOperator = true;
            }

            var oldGroupByCall = (MethodCallExpression)handlerContext.EvalOnClient(requiresClientResultOperator: sqlExpression == null);

            var newGroupByCall
                = handlerContext.QueryModelVisitor.QueryCompilationContext.QueryMethodProvider.GroupByMethod;

            if (oldGroupByCall.Method.Name == "_GroupByAsync")
            {
                newGroupByCall = _groupByAsync;
            }

            return sqlExpression != null
                ? Expression.Call(
                    newGroupByCall
                        .MakeGenericMethod(oldGroupByCall.Method.GetGenericArguments()),
                    oldGroupByCall.Arguments)
                : oldGroupByCall;
        }

        private static Expression[] VisitAndSaveMapping(
            SqlTranslatingExpressionVisitor sqlTranslator,
            Dictionary<MemberInfo, Expression> memberInfoMappings,
            NewExpression newExpression)
        {
            var translation = new Expression[newExpression.Arguments.Count];

            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                var sourceExpression = newExpression.Arguments[i];

                var sqlExpression = sqlTranslator.Visit(sourceExpression);

                Debug.Assert(sqlExpression != null);

                if (sqlExpression is ConstantExpression constantExpression
                    && constantExpression.Value is Expression[])
                {
                    // Encountered anonymous object inside anonymous object so abort
                    return null;
                }

                var memberInfo = newExpression.Members?[i];

                if (memberInfo != null)
                {
                    memberInfoMappings[memberInfo] = sqlExpression;
                }

                translation[i] = sqlExpression;
            }

            return translation;
        }

        private static Expression[] VisitAndSaveMapping(
            SqlTranslatingExpressionVisitor sqlTranslator,
            Dictionary<MemberInfo, Expression> memberInfoMappings,
            MemberInitExpression memberInitExpression)
        {
            var translation = new Expression[memberInitExpression.Bindings.Count];

            for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
            {
                if (memberInitExpression.Bindings[i] is MemberAssignment sourceExpression)
                {
                    var sqlExpression = sqlTranslator.Visit(sourceExpression.Expression);

                    Debug.Assert(sqlExpression != null);

                    if (sqlExpression is ConstantExpression constantExpression
                        && constantExpression.Value is Expression[])
                    {
                        // Encountered anonymous object inside DTO so abort
                        return null;
                    }

                    var memberInfo = sourceExpression.Member;

                    if (memberInfo != null)
                    {
                        memberInfoMappings[memberInfo] = sqlExpression;
                    }

                    translation[i] = sqlExpression;
                }
                else
                {
                    // Unknown binding
                    return null;
                }
            }

            return translation;
        }

        private static readonly MethodInfo _groupByAsync
            = typeof(RelationalResultOperatorHandler)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_GroupByAsync));

        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<IGrouping<TKey, TElement>> _GroupByAsync<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, CancellationToken, Task<TElement>> elementSelector)
            => new AsyncGroupByAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector);

        private sealed class AsyncGroupByAsyncEnumerable<TSource, TKey, TElement>
            : IAsyncEnumerable<IGrouping<TKey, TElement>>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, TKey> _keySelector;
            private readonly Func<TSource, CancellationToken, Task<TElement>> _elementSelector;

            public AsyncGroupByAsyncEnumerable(
                IAsyncEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, CancellationToken, Task<TElement>> elementSelector)
            {
                _source = source;
                _keySelector = keySelector;
                _elementSelector = elementSelector;
            }

            public IAsyncEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
                => new GroupByAsyncEnumerator(this);

            private sealed class GroupByAsyncEnumerator : IAsyncEnumerator<IGrouping<TKey, TElement>>
            {
                private readonly AsyncGroupByAsyncEnumerable<TSource, TKey, TElement> _groupByAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IAsyncEnumerator<TSource> _sourceEnumerator;
                private bool _hasNext;

                public GroupByAsyncEnumerator(
                    AsyncGroupByAsyncEnumerable<TSource, TKey, TElement> groupByAsyncEnumerable)
                {
                    _groupByAsyncEnumerable = groupByAsyncEnumerable;
                    _comparer = EqualityComparer<TKey>.Default;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_sourceEnumerator == null)
                    {
                        _sourceEnumerator = _groupByAsyncEnumerable._source.GetEnumerator();
                        _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);
                    }

                    if (_hasNext)
                    {
                        var currentKey = _groupByAsyncEnumerable._keySelector(_sourceEnumerator.Current);
                        var element = await _groupByAsyncEnumerable._elementSelector(_sourceEnumerator.Current, cancellationToken);
                        var grouping = new Grouping<TKey, TElement>(currentKey)
                        {
                            element
                        };

                        while (true)
                        {
                            _hasNext = await _sourceEnumerator.MoveNext(cancellationToken);

                            if (!_hasNext)
                            {
                                break;
                            }

                            if (!_comparer.Equals(
                                currentKey,
                                _groupByAsyncEnumerable._keySelector(_sourceEnumerator.Current)))
                            {
                                break;
                            }

                            grouping.Add(await _groupByAsyncEnumerable._elementSelector(_sourceEnumerator.Current, cancellationToken));
                        }

                        Current = grouping;

                        return true;
                    }

                    return false;
                }

                public IGrouping<TKey, TElement> Current { get; private set; }

                public void Dispose() => _sourceEnumerator?.Dispose();
            }
        }

        private static Expression HandleLast(HandlerContext handlerContext)
        {
            var requiresClientResultOperator = true;
            if (handlerContext.SelectExpression.OrderBy.Count > 0)
            {
                if (handlerContext.SelectExpression.Limit != null
                    || handlerContext.SelectExpression.Offset != null)
                {
                    handlerContext.SelectExpression.PushDownSubquery();
                    handlerContext.SelectExpression.LiftOrderBy();
                }

                foreach (var ordering in handlerContext.SelectExpression.OrderBy)
                {
                    ordering.OrderingDirection
                        = ordering.OrderingDirection == OrderingDirection.Asc
                            ? OrderingDirection.Desc
                            : OrderingDirection.Asc;
                }

                handlerContext.SelectExpression.Limit = Expression.Constant(1);
                requiresClientResultOperator = false;
            }

            requiresClientResultOperator
                = requiresClientResultOperator
                  || !((LastResultOperator)handlerContext.ResultOperator).ReturnDefaultWhenEmpty
                  && handlerContext.QueryModelVisitor.ParentQueryModelVisitor != null;

            return handlerContext.EvalOnClient(requiresClientResultOperator);
        }

        private static Expression HandleLongCount(HandlerContext handlerContext)
        {
            PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);

            handlerContext.SelectExpression
                .SetProjectionExpression(
                    new SqlFunctionExpression(
                        "COUNT",
                        typeof(long),
                        new[] { new SqlFragmentExpression("*") }));

            handlerContext.SelectExpression.ClearOrderBy();

            return TransformClientExpression<long>(handlerContext);
        }

        private static Expression HandleMin(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection
                && handlerContext.SelectExpression.Projection.Count == 1)
            {
                PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);
                var expression = handlerContext.SelectExpression.Projection[0];

                if (!ContainsSelect(expression))
                {
                    var minExpression = new SqlFunctionExpression(
                        "MIN",
                        handlerContext.QueryModel.SelectClause.Selector.Type,
                        new[] { expression.UnwrapAliasExpression() });

                    handlerContext.SelectExpression.SetProjectionExpression(minExpression);

                    var minExpressionType = minExpression.Type;
                    var throwOnNullResult = DetermineAggregateThrowingBehavior(handlerContext, minExpressionType);

                    return (Expression)_transformClientExpressionMethodInfo
                        .MakeGenericMethod(minExpressionType)
                        .Invoke(null, new object[] { handlerContext, throwOnNullResult });
                }
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleMax(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection
                && handlerContext.SelectExpression.Projection.Count == 1)
            {
                PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);
                var expression = handlerContext.SelectExpression.Projection[0];

                if (!ContainsSelect(expression))
                {
                    var maxExpression = new SqlFunctionExpression(
                        "MAX",
                        handlerContext.QueryModel.SelectClause.Selector.Type,
                        new[] { expression.UnwrapAliasExpression() });

                    handlerContext.SelectExpression.SetProjectionExpression(maxExpression);

                    var maxExpressionType = maxExpression.Type;
                    var throwOnNullResult = DetermineAggregateThrowingBehavior(handlerContext, maxExpressionType);

                    return (Expression)_transformClientExpressionMethodInfo
                        .MakeGenericMethod(maxExpressionType)
                        .Invoke(null, new object[] { handlerContext, throwOnNullResult });
                }
            }

            return handlerContext.EvalOnClient();
        }

        private static bool DetermineAggregateThrowingBehavior(HandlerContext handlerContext, Type maxExpressionType)
        {
            if (handlerContext.QueryModel.MainFromClause.FromExpression.Type.IsGrouping())
            {
                return false;
            }

            var throwOnNullResult = !maxExpressionType.IsNullableType();

            if (throwOnNullResult
                && handlerContext.QueryModelVisitor.ParentQueryModelVisitor != null)
            {
                handlerContext.QueryModelVisitor.QueryCompilationContext.Logger
                    .QueryPossibleExceptionWithAggregateOperator();
            }

            if (handlerContext.QueryModelVisitor.ParentQueryModelVisitor != null)
            {
                handlerContext.QueryModelVisitor.RequiresClientResultOperator = throwOnNullResult;
            }

            return throwOnNullResult;
        }

        private static Expression HandleSingle(HandlerContext handlerContext)
        {
            handlerContext.SelectExpression.Limit = Expression.Constant(2);

            var returnExpression = handlerContext.EvalOnClient();

            // For top level single, we do not require client eval
            if (handlerContext.QueryModelVisitor.ParentQueryModelVisitor == null)
            {
                handlerContext.QueryModelVisitor.RequiresClientResultOperator = false;
            }

            return returnExpression;
        }

        private static Expression HandleSkip(HandlerContext handlerContext)
        {
            var skipResultOperator = (SkipResultOperator)handlerContext.ResultOperator;

            var sqlTranslatingExpressionVisitor
                = handlerContext.CreateSqlTranslatingVisitor();

            var offset = sqlTranslatingExpressionVisitor.Visit(skipResultOperator.Count);
            if (offset != null)
            {
                handlerContext.SelectExpression.Offset = offset;

                return handlerContext.EvalOnServer;
            }

            return handlerContext.EvalOnClient();
        }

        private static readonly MethodInfo _castToNullableMethodInfo
            = typeof(RelationalTaskExtensions).GetRuntimeMethods().Where(m => m.Name == nameof(RelationalTaskExtensions.CastToNullable)).Single();

        private static Expression HandleSum(HandlerContext handlerContext)
        {
            if (!handlerContext.QueryModelVisitor.RequiresClientProjection
                && handlerContext.SelectExpression.Projection.Count == 1)
            {
                PrepareSelectExpressionForAggregate(handlerContext.SelectExpression, handlerContext.QueryModel);
                var expression = handlerContext.SelectExpression.Projection[0];

                if (!ContainsSelect(expression))
                {
                    var inputType = handlerContext.QueryModel.SelectClause.Selector.Type;

                    Expression sumExpression = new SqlFunctionExpression(
                        "SUM", inputType, new[] { expression.UnwrapAliasExpression() });
                    if (inputType.UnwrapNullableType() == typeof(float))
                    {
                        sumExpression = new ExplicitCastExpression(sumExpression, inputType);
                    }

                    handlerContext.SelectExpression.SetProjectionExpression(sumExpression);

                    var clientExpression
                        = (Expression)_transformClientExpressionMethodInfo
                            .MakeGenericMethod(sumExpression.Type.UnwrapNullableType())
                            .Invoke(null, new object[] { handlerContext, /*throwOnNullResult:*/ false });

                    if (typeof(Task).IsAssignableFrom(clientExpression.Type))
                    {
                        return sumExpression.Type.IsNullableType()
                            ? Expression.Call(
                                _castToNullableMethodInfo.MakeGenericMethod(sumExpression.Type.UnwrapNullableType()),
                                clientExpression)
                            : clientExpression;
                    }
                    else
                    {
                        return sumExpression.Type.IsNullableType()
                            ? Expression.Convert(clientExpression, sumExpression.Type)
                            : clientExpression;
                    }
                }
            }

            return handlerContext.EvalOnClient();
        }

        private static Expression HandleTake(HandlerContext handlerContext)
        {
            var takeResultOperator = (TakeResultOperator)handlerContext.ResultOperator;

            var sqlTranslatingExpressionVisitor
                = handlerContext.CreateSqlTranslatingVisitor();

            var limit = sqlTranslatingExpressionVisitor.Visit(takeResultOperator.Count);

            if (limit != null)
            {
                handlerContext.SelectExpression.Limit = limit;

                return handlerContext.EvalOnServer;
            }

            return handlerContext.EvalOnClient();
        }

        private static void SetConditionAsProjection(
            HandlerContext handlerContext, Expression condition)
        {
            handlerContext.SelectExpression.Clear();

            handlerContext.SelectExpression.AddToProjection(
                Expression.Condition(
                    condition,
                    Expression.Constant(true),
                    Expression.Constant(false),
                    typeof(bool)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected static void PrepareSelectExpressionForAggregate(SelectExpression selectExpression, QueryModel queryModel)
        {
            if (selectExpression.IsDistinct
                || selectExpression.Limit != null
                || selectExpression.Offset != null
                || (selectExpression.GroupBy.Count > 0
                    && !IsGroupByAggregate(queryModel)))
            {
                selectExpression.PushDownSubquery();
                selectExpression.ExplodeStarProjection();
            }
        }

        private static bool IsGroupByAggregate(QueryModel queryModel)
            => queryModel.MainFromClause.FromExpression is QuerySourceReferenceExpression mainFromClauseQsre
                && mainFromClauseQsre.ReferencedQuerySource.ItemType.IsGrouping();

        private static readonly MethodInfo _transformClientExpressionMethodInfo
            = typeof(RelationalResultOperatorHandler).GetTypeInfo()
                .GetDeclaredMethod(nameof(TransformClientExpression));

        private static Expression TransformClientExpression<TResult>(
            HandlerContext handlerContext, bool throwOnNullResult = false)
            => new ResultTransformingExpressionVisitor<TResult>(
                    handlerContext.QueryModelVisitor.QueryCompilationContext,
                    throwOnNullResult)
                .Visit(handlerContext.QueryModelVisitor.Expression);

        private sealed class ContainsSelectExpressionVisitor : ExpressionVisitor
        {
            public bool ContainsExpressionType { get; private set; }
            public override Expression Visit(Expression node)
            {
                if (node is SelectExpression)
                {
                    ContainsExpressionType = true;
                    return node;
                }
                return base.Visit(node);
            }
        }
        private static bool ContainsSelect(Expression expression)
        {
            var visitor = new ContainsSelectExpressionVisitor();
            visitor.Visit(expression);
            return visitor.ContainsExpressionType;
        }
    }
}
