// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query
{
    public class RelationalQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly RelationalQueryModelVisitor _parentQueryModelVisitor;

        private readonly IMethodCallTranslator _methodCallTranslator = new CompositeMethodCallTranslator();

        private readonly Dictionary<IQuerySource, SelectExpression> _queriesBySource
            = new Dictionary<IQuerySource, SelectExpression>();

        private Expression _preOrderingExpression;

        private bool _requiresClientFilter;

        public RelationalQueryModelVisitor(
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [CanBeNull] RelationalQueryModelVisitor parentQueryModelVisitor)
            : base(Check.NotNull(queryCompilationContext, "queryCompilationContext"))
        {
            _parentQueryModelVisitor = parentQueryModelVisitor;
        }

        public virtual bool RequiresClientFilter
        {
            get { return _requiresClientFilter; }
        }

        public virtual new RelationalQueryCompilationContext QueryCompilationContext
        {
            get { return (RelationalQueryCompilationContext)base.QueryCompilationContext; }
        }

        public virtual SelectExpression TryGetSelectExpression([NotNull] IQuerySource querySource)
        {
            SelectExpression selectExpression;
            return (_queriesBySource.TryGetValue(querySource, out selectExpression)
                ? selectExpression
                : _queriesBySource.Values.SingleOrDefault(se => se.HandlesQuerySource(querySource)));
        }

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            return new RelationalEntityQueryableExpressionTreeVisitor(this, querySource);
        }

        protected override ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor()
        {
            return new RelationalProjectionSubQueryExpressionTreeVisitor(this);
        }

        protected override ExpressionTreeVisitor CreateOrderingExpressionTreeVisitor(Ordering ordering)
        {
            return new RelationalOrderingExpressionTreeVisitor(this, ordering);
        }

        protected override void IncludeNavigation(
            IQuerySource querySource,
            Type resultType,
            LambdaExpression accessorLambda,
            Expression navigationPropertyPath)
        {
            var navigation
                = BindNavigationMemberExpression(
                    (MemberExpression)navigationPropertyPath,
                    (n, _) => n);

            if (navigation == null)
            {
                return;
            }

            if (!navigation.PointsToPrincipal)
            {
                IncludeCollection(querySource, resultType, accessorLambda, navigation);
            }
            else
            {
                IncludeReference(querySource, navigation);
            }
        }

        private void IncludeReference(IQuerySource querySource, INavigation navigation)
        {
            var selectExpression = QueryCompilationContext.FindSelectExpression(querySource);
            var dependentTableExpression = selectExpression.FindTableForQuerySource(querySource);
            var targetEntityType = navigation.GetTargetType();
            var targetTableName = targetEntityType.TableName();

            var targetTableExpression
                = new TableExpression(
                    targetTableName,
                    targetEntityType.Schema(),
                    targetTableName.First().ToString().ToLower(),
                    querySource);

            var readerOffset = selectExpression.Projection.Count;

            var columnExpressions
                = targetEntityType.Properties
                    .Select(p => new ColumnExpression(p, targetTableExpression));

            var innerJoinExpression
                = navigation.ForeignKey.IsRequired
                    ? selectExpression
                        .AddInnerJoin(targetTableExpression, columnExpressions)
                    : selectExpression
                        .AddOuterJoin(targetTableExpression, columnExpressions);

            Expression joinPredicateExpression = null;

            var primaryKeyProperties = targetEntityType.GetPrimaryKey().Properties;

            for (var i = 0; i < navigation.ForeignKey.Properties.Count; i++)
            {
                var primaryKeyProperty = primaryKeyProperties[i];
                var foreignKeyProperty = navigation.ForeignKey.Properties[i];

                var equalExpression
                    = Expression.Equal(
                        new ColumnExpression(primaryKeyProperty, innerJoinExpression),
                        new ColumnExpression(foreignKeyProperty, dependentTableExpression));

                joinPredicateExpression
                    = joinPredicateExpression == null
                        ? equalExpression
                        : Expression.And(joinPredicateExpression, equalExpression);
            }

            innerJoinExpression.Predicate = joinPredicateExpression;

            Expression
                = new IncludeReferenceExpressionTreeVisitor(querySource, navigation, readerOffset)
                    .VisitExpression(Expression);
        }

        private class IncludeReferenceExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly IQuerySource _querySource;
            private readonly INavigation _navigation;
            private readonly int _readerOffset;

            public IncludeReferenceExpressionTreeVisitor(
                IQuerySource querySource, INavigation navigation, int readerOffset)
            {
                _querySource = querySource;
                _navigation = navigation;
                _readerOffset = readerOffset;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression expression)
            {
                if (expression.Method.MethodIsClosedFormOf(CreateEntityMethodInfo))
                {
                    var querySource = ((ConstantExpression)expression.Arguments[0]).Value;

                    if (querySource == _querySource)
                    {
                        return Expression
                            .Call(
                                _includeReferenceMethodInfo
                                    .MakeGenericMethod(expression.Method.GetGenericArguments()[0]),
                                expression.Arguments[1],
                                expression,
                                Expression.Constant(_navigation),
                                expression.Arguments[3],
                                Expression.Constant(_readerOffset)
                            );
                    }
                }

                return base.VisitMethodCallExpression(expression);
            }
        }

        private static readonly MethodInfo _includeReferenceMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("_IncludeReference");

        [UsedImplicitly]
        private static QuerySourceScope<TEntity> _IncludeReference<TEntity>(
            QueryContext queryContext,
            QuerySourceScope<TEntity> querySourceScope,
            INavigation navigation,
            DbDataReader dataReader,
            int readerOffset)
        {
            var valueReader
                = ((RelationalQueryContext)queryContext).ValueReaderFactory
                    .Create(dataReader);

            queryContext.QueryBuffer
                .Include(
                    querySourceScope._result,
                    navigation,
                    (_, __) => new[] { new OffsetValueReaderDecorator(valueReader, readerOffset) });

            return querySourceScope;
        }

        private void IncludeCollection(IQuerySource querySource, Type resultType, LambdaExpression accessorLambda, INavigation navigation)
        {
            var selectExpression = QueryCompilationContext.FindSelectExpression(querySource);

            var primaryKeyProperties = navigation.EntityType.GetPrimaryKey().Properties;

            foreach (var property in primaryKeyProperties)
            {
                selectExpression.AddToOrderBy(property, querySource, OrderingDirection.Asc);
            }

            var targetEntityType = navigation.GetTargetType();
            var targetTableName = targetEntityType.TableName();

            var targetSelectExpression = new SelectExpression();

            var targetTableExpression
                = new TableExpression(
                    targetTableName,
                    targetEntityType.Schema(),
                    targetTableName.First().ToString().ToLower(),
                    querySource);

            targetSelectExpression.AddTable(targetTableExpression);

            foreach (var property in targetEntityType.Properties)
            {
                targetSelectExpression.AddToProjection(property, querySource);
            }

            var innerJoinSelectExpression
                = selectExpression.Clone(
                    ((ColumnExpression)selectExpression.OrderBy.First().Expression).TableAlias);

            innerJoinSelectExpression.IsDistinct = true;
            innerJoinSelectExpression.ClearProjection();

            foreach (var columnExpression 
                in innerJoinSelectExpression.OrderBy
                    .Select(o => o.Expression)
                    .Cast<ColumnExpression>())
            {
                innerJoinSelectExpression.AddToProjection(columnExpression);
            }

            innerJoinSelectExpression.ClearOrderBy();

            var innerJoinExpression
                = targetSelectExpression.AddInnerJoin(innerJoinSelectExpression);

            targetSelectExpression.AddToOrderBy(selectExpression.OrderBy);

            Expression joinPredicateExpression = null;

            for (var i = 0; i < navigation.ForeignKey.Properties.Count; i++)
            {
                var primaryKeyProperty = primaryKeyProperties[i];
                var foreignKeyProperty = navigation.ForeignKey.Properties[i];

                var equalExpression
                    = Expression.Equal(
                        new ColumnExpression(primaryKeyProperty, targetTableExpression),
                        new ColumnExpression(foreignKeyProperty, innerJoinExpression));

                joinPredicateExpression
                    = joinPredicateExpression == null
                        ? equalExpression
                        : Expression.And(joinPredicateExpression, equalExpression);
            }

            innerJoinExpression.Predicate = joinPredicateExpression;

            var readerParameter = Expression.Parameter(typeof(DbDataReader));

            Expression
                = Expression.Call(
                    _includeCollectionMethodInfo
                        .MakeGenericMethod(resultType),
                    QueryContextParameter,
                    Expression,
                    Expression.Constant(navigation),
                    Expression.Call(
                        QueryCompilationContext.QueryMethodProvider.QueryMethod
                            .MakeGenericMethod(typeof(IValueReader)),
                        QueryContextParameter,
                        Expression.Constant(new CommandBuilder(targetSelectExpression, QueryCompilationContext)),
                        Expression.Lambda(
                            Expression.Call(
                                _createValueReaderForIncludeMethodInfo,
                                QueryContextParameter,
                                readerParameter,
                                Expression.Constant(targetEntityType)),
                            readerParameter)),
                    accessorLambda);
        }

        private static readonly MethodInfo _includeCollectionMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("_IncludeCollection");

        [UsedImplicitly]
        private static IEnumerable<TResult> _IncludeCollection<TResult>(
            QueryContext queryContext,
            IEnumerable<TResult> source,
            INavigation navigation,
            IEnumerable<IValueReader> relatedValueReaders,
            Func<TResult, object> accessorLambda)
        {
            using (var relatedValuesIterator = new IncludeCollectionIterator(relatedValueReaders.GetEnumerator()))
            {
                foreach (var result in source)
                {
                    queryContext.QueryBuffer
                        .Include(
                            accessorLambda.Invoke(result),
                            navigation,
                            relatedValuesIterator.GetRelatedValues);

                    yield return result;
                }
            }
        }

        private static readonly MethodInfo _createValueReaderForIncludeMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("CreateValueReaderForInclude");

        [UsedImplicitly]
        private static IValueReader CreateValueReaderForInclude(
            QueryContext queryContext, DbDataReader dataReader, IEntityType entityType)
        {
            return ((RelationalQueryContext)queryContext).ValueReaderFactory.Create(dataReader);
        }

        private class IncludeCollectionIterator : IDisposable
        {
            private readonly IEnumerator<IValueReader> _relatedValuesEnumerator;

            private bool _hasRemainingRows;
            private bool _initialized;

            public IncludeCollectionIterator(IEnumerator<IValueReader> relatedValuesEnumerator)
            {
                _relatedValuesEnumerator = relatedValuesEnumerator;
            }

            public IEnumerable<IValueReader> GetRelatedValues(
                EntityKey primaryKey, Func<IValueReader, EntityKey> relatedKeyFactory)
            {
                if (!_initialized)
                {
                    _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
                    _initialized = true;
                }

                while (_hasRemainingRows
                       && relatedKeyFactory(_relatedValuesEnumerator.Current).Equals(primaryKey))
                {
                    yield return _relatedValuesEnumerator.Current;

                    _hasRemainingRows = _relatedValuesEnumerator.MoveNext();
                }
            }

            public void Dispose()
            {
                _relatedValuesEnumerator.Dispose();
            }
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);

            var selectExpression = TryGetSelectExpression(fromClause);

            if (selectExpression != null)
            {
                var previousQuerySource
                    = index == 0
                        ? queryModel.MainFromClause
                        : queryModel.BodyClauses[index - 1] as IQuerySource;

                if (previousQuerySource != null)
                {
                    var previousSelectExpression = TryGetSelectExpression(previousQuerySource);

                    if (previousSelectExpression != null)
                    {
                        var readerOffset = previousSelectExpression.Projection.Count;

                        previousSelectExpression
                            .AddCrossJoin(selectExpression.Tables.Single(), selectExpression.Projection);

                        _queriesBySource.Remove(fromClause);

                        Expression
                            = new QueryFlatteningExpressionTreeVisitor(
                                previousQuerySource,
                                fromClause,
                                QueryCompilationContext,
                                readerOffset,
                                LinqOperatorProvider.SelectMany)
                                .VisitExpression(Expression);
                    }
                }
            }
        }

        private class QueryFlatteningExpressionTreeVisitor : ExpressionTreeVisitor
        {
            private readonly IQuerySource _outerQuerySource;
            private readonly IQuerySource _innerQuerySource;
            private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;
            private readonly int _readerOffset;
            private readonly MethodInfo _operatorToFlatten;

            private MethodCallExpression _outerSelectManyExpression;
            private Expression _outerShaperExpression;
            private Expression _outerCommandBuilder;

            public QueryFlatteningExpressionTreeVisitor(
                IQuerySource outerQuerySource,
                IQuerySource innerQuerySource,
                RelationalQueryCompilationContext relationalQueryCompilationContext,
                int readerOffset,
                MethodInfo operatorToFlatten)
            {
                _outerQuerySource = outerQuerySource;
                _innerQuerySource = innerQuerySource;
                _relationalQueryCompilationContext = relationalQueryCompilationContext;
                _readerOffset = readerOffset;
                _operatorToFlatten = operatorToFlatten;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                var newExpression
                    = (MethodCallExpression)base.VisitMethodCallExpression(methodCallExpression);

                if (_outerShaperExpression != null)
                {
                    if (_outerCommandBuilder == null)
                    {
                        _outerCommandBuilder = methodCallExpression.Arguments[1];
                    }
                    else if (newExpression.Method.MethodIsClosedFormOf(
                        _relationalQueryCompilationContext.QueryMethodProvider.QueryMethod))
                    {
                        newExpression
                            = Expression.Call(
                                newExpression.Method,
                                newExpression.Arguments[0],
                                _outerCommandBuilder,
                                newExpression.Arguments[2]);
                    }
                }

                if (ReferenceEquals(newExpression.Method, CreateValueReaderMethodInfo)
                    || newExpression.Method.MethodIsClosedFormOf(CreateEntityMethodInfo))
                {
                    var constantExpression = (ConstantExpression)newExpression.Arguments[0];

                    if (constantExpression.Value == _outerQuerySource)
                    {
                        _outerShaperExpression = newExpression;
                    }
                    else if (constantExpression.Value == _innerQuerySource)
                    {
                        var newArguments = new List<Expression>(newExpression.Arguments);
                        newArguments[2] = _outerShaperExpression;

                        if (newArguments.Count == 6)
                        {
                            newArguments[4]
                                = Expression.Constant(
                                    _readerOffset
                                    + (int)((ConstantExpression)newArguments[4]).Value);
                        }

                        newExpression
                            = Expression.Call(newExpression.Method, newArguments);
                    }
                }
                else if (_outerShaperExpression != null
                         && _outerSelectManyExpression == null
                         && newExpression.Method.MethodIsClosedFormOf(
                             _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany))
                {
                    _outerSelectManyExpression = newExpression;
                }
                else if (_outerSelectManyExpression != null
                         && newExpression.Method.MethodIsClosedFormOf(_operatorToFlatten))
                {
                    newExpression
                        = Expression.Call(
                            _relationalQueryCompilationContext.LinqOperatorProvider.SelectMany
                                .MakeGenericMethod(
                                    typeof(QuerySourceScope),
                                    typeof(QuerySourceScope)),
                            _outerSelectManyExpression.Arguments[0],
                            newExpression.Arguments[1] is LambdaExpression
                                ? newExpression.Arguments[1]
                                : Expression.Lambda(
                                    newExpression.Arguments[1],
                                    QuerySourceScopeParameter));
                }

                return newExpression;
            }
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            var previousQuerySource
                = index == 0
                    ? queryModel.MainFromClause
                    : queryModel.BodyClauses[index - 1] as IQuerySource;

            var previousSelectExpression
                = previousQuerySource != null
                    ? TryGetSelectExpression(previousQuerySource)
                    : null;

            var previousSelectProjectionCount
                = previousSelectExpression != null
                    ? previousSelectExpression.Projection.Count
                    : -1;

            base.VisitJoinClause(joinClause, queryModel, index);

            if (previousSelectExpression != null)
            {
                var selectExpression = TryGetSelectExpression(joinClause);

                if (selectExpression != null)
                {
                    var filteringExpressionTreeVisitor
                        = new FilteringExpressionTreeVisitor(this);

                    var predicate
                        = filteringExpressionTreeVisitor
                            .VisitExpression(
                                Expression.Equal(
                                    joinClause.OuterKeySelector,
                                    joinClause.InnerKeySelector));

                    if (predicate != null)
                    {
                        _queriesBySource.Remove(joinClause);

                        previousSelectExpression.RemoveRangeFromProjection(previousSelectProjectionCount);

                        var innerJoinExpression
                            = previousSelectExpression
                                .AddInnerJoin(
                                    selectExpression.Tables.Single(),
                                    QuerySourceRequiresMaterialization(joinClause)
                                        ? selectExpression.Projection
                                        : Enumerable.Empty<ColumnExpression>());

                        innerJoinExpression.Predicate = predicate;

                        Expression
                            = new QueryFlatteningExpressionTreeVisitor(
                                previousQuerySource,
                                joinClause,
                                QueryCompilationContext,
                                previousSelectProjectionCount,
                                LinqOperatorProvider.Join)
                                .VisitExpression(Expression);
                    }
                    else
                    {
                        previousSelectExpression.RemoveRangeFromProjection(previousSelectProjectionCount);
                    }
                }
            }
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            var previousExpression = Expression;

            var projectionCounts
                = _queriesBySource
                    .Select(kv => new
                        {
                            SelectExpression = kv.Value,
                            kv.Value.Projection.Count
                        })
                    .ToList();

            _requiresClientFilter = !_queriesBySource.Any();

            base.VisitWhereClause(whereClause, queryModel, index);

            foreach (var selectExpression in _queriesBySource.Values)
            {
                var filteringVisitor = new FilteringExpressionTreeVisitor(this);

                selectExpression.Predicate = filteringVisitor.VisitExpression(whereClause.Predicate);

                _requiresClientFilter |= filteringVisitor.RequiresClientEval;
            }

            if (!_requiresClientFilter)
            {
                foreach (var projectionCount in projectionCounts)
                {
                    projectionCount.SelectExpression
                        .RemoveRangeFromProjection(projectionCount.Count);
                }

                Expression = previousExpression;
            }
        }

        internal class FilteringExpressionTreeVisitor : ThrowingExpressionTreeVisitor
        {
            private readonly RelationalQueryModelVisitor _queryModelVisitor;

            private bool _requiresClientEval;

            public FilteringExpressionTreeVisitor(RelationalQueryModelVisitor queryModelVisitor)
            {
                _queryModelVisitor = queryModelVisitor;
            }

            public bool RequiresClientEval
            {
                get { return _requiresClientEval; }
            }

            protected override Expression VisitBinaryExpression(BinaryExpression binaryExpression)
            {
                switch (binaryExpression.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    {
                        return UnfoldStructuralComparison(binaryExpression.NodeType, ProcessComparisonExpression(binaryExpression));
                    }
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    {
                        return ProcessComparisonExpression(binaryExpression);
                    }

                    case ExpressionType.AndAlso:
                    {
                        var left = VisitExpression(binaryExpression.Left);
                        var right = VisitExpression(binaryExpression.Right);

                        return left != null
                               && right != null
                            ? Expression.AndAlso(left, right)
                            : (left ?? right);
                    }

                    case ExpressionType.OrElse:
                    {
                        var left = VisitExpression(binaryExpression.Left);
                        var right = VisitExpression(binaryExpression.Right);

                        return left != null
                               && right != null
                            ? Expression.OrElse(left, right)
                            : null;
                    }
                }

                _requiresClientEval = true;

                return null;
            }

            private Expression UnfoldStructuralComparison(ExpressionType expressionType, Expression expression)
            {
                var binaryExpression = expression as BinaryExpression;

                if (binaryExpression != null)
                {
                    var leftConstantExpression = binaryExpression.Left as ConstantExpression;

                    if (leftConstantExpression != null)
                    {
                        var leftExpressions = leftConstantExpression.Value as Expression[];

                        if (leftExpressions != null)
                        {
                            var rightConstantExpression = binaryExpression.Right as ConstantExpression;

                            if (rightConstantExpression != null)
                            {
                                var rightExpressions = rightConstantExpression.Value as Expression[];

                                if (rightExpressions != null
                                    && leftExpressions.Length == rightExpressions.Length)
                                {
                                    return leftExpressions
                                        .Zip(rightExpressions, (l, r) =>
                                            Expression.MakeBinary(expressionType, l, r))
                                        .Aggregate((e1, e2) =>
                                            expressionType == ExpressionType.Equal
                                                ? Expression.AndAlso(e1, e2)
                                                : Expression.OrElse(e1, e2));
                                }
                            }
                        }
                    }
                }

                return expression;
            }

            private Expression ProcessComparisonExpression(BinaryExpression binaryExpression)
            {
                var leftExpression = VisitExpression(binaryExpression.Left);
                var rightExpression = VisitExpression(binaryExpression.Right);

                if (leftExpression == null
                    || rightExpression == null)
                {
                    return null;
                }

                var nullExpression
                    = TransformNullComparison(leftExpression, rightExpression, binaryExpression.NodeType);

                if (nullExpression != null)
                {
                    return nullExpression;
                }

                return Expression
                    .MakeBinary(binaryExpression.NodeType, leftExpression, rightExpression);
            }

            private Expression TransformNullComparison(
                Expression left, Expression right, ExpressionType expressionType)
            {
                if (expressionType == ExpressionType.Equal
                    || expressionType == ExpressionType.NotEqual)
                {
                    var constant
                        = right as ConstantExpression
                          ?? left as ConstantExpression;

                    if (constant != null
                        && constant.Value == null)
                    {
                        var propertyAccess
                            = left as ColumnExpression
                              ?? right as ColumnExpression;

                        if (propertyAccess != null)
                        {
                            return expressionType == ExpressionType.Equal
                                ? (Expression)new IsNullExpression(propertyAccess)
                                : new IsNotNullExpression(propertyAccess);
                        }
                    }
                }

                return null;
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                var operand = VisitExpression(methodCallExpression.Object);

                if (operand != null)
                {
                    var arguments
                        = methodCallExpression.Arguments
                            .Select(VisitExpression)
                            .Where(e => e != null)
                            .ToArray();

                    if (arguments.Length == methodCallExpression.Arguments.Count)
                    {
                        var boundExpression
                            = Expression.Call(
                                operand,
                                methodCallExpression.Method,
                                arguments);

                        var translatedMethodExpression
                            = _queryModelVisitor._methodCallTranslator
                                .Translate(boundExpression);

                        if (translatedMethodExpression != null)
                        {
                            return translatedMethodExpression;
                        }
                    }
                }
                else
                {
                    var columnExpression
                        = _queryModelVisitor
                            .BindMethodCallExpression(
                                methodCallExpression,
                                (property, querySource, selectExpression)
                                    => new ColumnExpression(
                                        property,
                                        selectExpression.FindTableForQuerySource(querySource)));

                    if (columnExpression != null)
                    {
                        return columnExpression;
                    }
                }

                _requiresClientEval = true;

                return null;
            }

            protected override Expression VisitMemberExpression(MemberExpression memberExpression)
            {
                var columnExpression
                    = _queryModelVisitor
                        .BindMemberExpression(
                            memberExpression,
                            (property, querySource, selectExpression)
                                => new ColumnExpression(
                                    property,
                                    selectExpression.FindTableForQuerySource(querySource)));

                if (columnExpression != null)
                {
                    return columnExpression;
                }

                _requiresClientEval = true;

                return null;
            }

            protected override Expression VisitNewExpression(NewExpression newExpression)
            {
                if (newExpression.Members != null
                    && newExpression.Arguments.Any()
                    && newExpression.Arguments.Count == newExpression.Members.Count)
                {
                    var memberBindings
                        = newExpression.Arguments
                            .Select(VisitExpression)
                            .Where(e => e != null)
                            .ToArray();

                    if (memberBindings.Length == newExpression.Arguments.Count)
                    {
                        return Expression.Constant(memberBindings);
                    }
                }

                _requiresClientEval = true;

                return null;
            }

            private static readonly Type[] _supportedConstantTypes =
                {
                    typeof(bool),
                    typeof(byte),
                    typeof(byte[]),
                    typeof(char),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(double),
                    typeof(float),
                    typeof(Guid),
                    typeof(int),
                    typeof(long),
                    typeof(sbyte),
                    typeof(short),
                    typeof(string),
                    typeof(uint),
                    typeof(ulong),
                    typeof(ushort)
                };

            protected override Expression VisitConstantExpression(ConstantExpression constantExpression)
            {
                if (constantExpression.Value == null)
                {
                    return constantExpression;
                }

                var underlyingType = constantExpression.Type.UnwrapNullableType();

                if (underlyingType.GetTypeInfo().IsEnum)
                {
                    underlyingType = Enum.GetUnderlyingType(underlyingType);
                }

                if (_supportedConstantTypes.Contains(underlyingType))
                {
                    return constantExpression;
                }

                _requiresClientEval = true;

                return null;
            }

            protected override TResult VisitUnhandledItem<TItem, TResult>(
                TItem unhandledItem, string visitMethod, Func<TItem, TResult> baseBehavior)
            {
                _requiresClientEval = true;

                return default(TResult);
            }

            protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
            {
                return null; // never called
            }
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            if (_preOrderingExpression == null)
            {
                _preOrderingExpression = Expression;
            }

            var orderingCounts
                = _queriesBySource
                    .Where(kv => kv.Value.OrderBy.Any())
                    .Select(kv => new { kv.Key, kv.Value.OrderBy.Count })
                    .ToList();

            base.VisitOrderByClause(orderByClause, queryModel, index);

            foreach (var querySourceOrdering in orderingCounts)
            {
                var selectExpression = _queriesBySource[querySourceOrdering.Key];

                if (querySourceOrdering.Count != selectExpression.OrderBy.Count)
                {
                    var orderBy = selectExpression.OrderBy.ToList();

                    selectExpression.ClearOrderBy();
                    selectExpression.AddToOrderBy(orderBy.Skip(querySourceOrdering.Count));
                    selectExpression.AddToOrderBy(orderBy.Take(querySourceOrdering.Count));
                }
            }

            if (index == queryModel.BodyClauses.Count - 1)
            {
                var queriesWithOrdering
                    = _queriesBySource
                        .Where(kv => kv.Value.OrderBy.Any())
                        .Select(kv => kv.Value)
                        .ToList();

                if (queriesWithOrdering.Count == 1
                    && queriesWithOrdering[0].OrderBy.Count == queryModel.BodyClauses
                        .OfType<OrderByClause>()
                        .SelectMany(ob => ob.Orderings)
                        .Count())
                {
                    queriesWithOrdering[0].RemoveFromProjection(queriesWithOrdering[0].OrderBy);

                    Expression = _preOrderingExpression;
                }
                else
                {
                    foreach (var selectExpression in _queriesBySource.Values)
                    {
                        selectExpression.ClearOrderBy();
                    }
                }
            }
        }

        public override Expression BindMemberToValueReader(MemberExpression memberExpression, Expression expression)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(expression, "expression");

            return BindMemberExpression(
                memberExpression,
                (property, querySource, selectExpression) =>
                    {
                        var projectionIndex = selectExpression.GetProjectionIndex(property, querySource);

                        Contract.Assert(projectionIndex > -1);

                        return BindReadValueMethod(memberExpression.Type, expression, projectionIndex);
                    });
        }

        public override Expression BindMethodCallToValueReader(MethodCallExpression methodCallExpression, Expression expression)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");
            Check.NotNull(expression, "expression");

            return BindMethodCallExpression(
                methodCallExpression,
                (property, querySource, selectExpression) =>
                    {
                        var projectionIndex = selectExpression.GetProjectionIndex(property, querySource);

                        Contract.Assert(projectionIndex > -1);

                        return BindReadValueMethod(methodCallExpression.Type, expression, projectionIndex);
                    });
        }

        private void BindMemberExpression(
            MemberExpression memberExpression,
            Action<IProperty, IQuerySource, SelectExpression> memberBinder)
        {
            BindMemberExpression(memberExpression, null,
                (property, querySource, selectExpression) =>
                    {
                        memberBinder(property, querySource, selectExpression);

                        return default(object);
                    });
        }

        private TResult BindMemberExpression<TResult>(
            MemberExpression memberExpression,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            return BindMemberExpression(memberExpression, null, memberBinder);
        }

        private TResult BindMemberExpression<TResult>(
            MemberExpression memberExpression,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            return base.BindMemberExpression(memberExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property));
        }

        private void BindMethodCallExpression(
            MethodCallExpression methodCallExpression,
            Action<IProperty, IQuerySource, SelectExpression> memberBinder)
        {
            BindMethodCallExpression(methodCallExpression, null,
                (property, querySource, selectExpression) =>
                    {
                        memberBinder(property, querySource, selectExpression);

                        return default(object);
                    });
        }

        private TResult BindMethodCallExpression<TResult>(
            MethodCallExpression methodCallExpression,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            return BindMethodCallExpression(methodCallExpression, null, memberBinder);
        }

        private TResult BindMethodCallExpression<TResult>(
            MethodCallExpression methodCallExpression,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder)
        {
            return base.BindMethodCallExpression(methodCallExpression, querySource,
                (property, qs) => BindMemberOrMethod(memberBinder, qs, property));
        }

        private TResult BindMemberOrMethod<TResult>(
            Func<IProperty, IQuerySource, SelectExpression, TResult> memberBinder,
            IQuerySource querySource,
            IProperty property)
        {
            var selectExpression = TryGetSelectExpression(querySource);

            if (selectExpression != null)
            {
                return memberBinder(property, querySource, selectExpression);
            }

            selectExpression
                = _parentQueryModelVisitor != null
                    ? _parentQueryModelVisitor.TryGetSelectExpression(querySource)
                    : null;

            if (selectExpression != null)
            {
                selectExpression.AddToProjection(property, querySource);
            }

            return default(TResult);
        }

        public static readonly MethodInfo CreateValueReaderMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("CreateValueReader");

        [UsedImplicitly]
        private static QuerySourceScope<IValueReader> CreateValueReader(
            IQuerySource querySource,
            QueryContext queryContext,
            QuerySourceScope parentQuerySourceScope,
            DbDataReader dataReader)
        {
            return new QuerySourceScope<IValueReader>(
                querySource,
                ((RelationalQueryContext)queryContext).ValueReaderFactory.Create(dataReader),
                parentQuerySourceScope);
        }

        public static readonly MethodInfo CreateEntityMethodInfo
            = typeof(RelationalQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod("CreateEntity");

        [UsedImplicitly]
        private static QuerySourceScope<TEntity> CreateEntity<TEntity>(
            IQuerySource querySource,
            QueryContext queryContext,
            QuerySourceScope parentQuerySourceScope,
            DbDataReader dataReader,
            int readerOffset,
            IEntityType entityType)
        {
            var valueReader
                = ((RelationalQueryContext)queryContext).ValueReaderFactory
                    .Create(dataReader);

            if (readerOffset > 0)
            {
                valueReader = new OffsetValueReaderDecorator(valueReader, readerOffset);
            }

            return new QuerySourceScope<TEntity>(
                querySource,
                (TEntity)queryContext.QueryBuffer.GetEntity(entityType, valueReader),
                parentQuerySourceScope);
        }

        private class OffsetValueReaderDecorator : IValueReader
        {
            private readonly IValueReader _valueReader;
            private readonly int _offset;

            public OffsetValueReaderDecorator(IValueReader valueReader, int offset)
            {
                _valueReader = valueReader;
                _offset = offset;
            }

            public bool IsNull(int index)
            {
                return _valueReader.IsNull(_offset + index);
            }

            public T ReadValue<T>(int index)
            {
                return _valueReader.ReadValue<T>(_offset + index);
            }

            public int Count
            {
                get { return _valueReader.Count; }
            }
        }

        private class RelationalEntityQueryableExpressionTreeVisitor : EntityQueryableExpressionTreeVisitor
        {
            private static readonly ParameterExpression _readerParameter
                = Expression.Parameter(typeof(DbDataReader));

            private readonly IQuerySource _querySource;

            public RelationalEntityQueryableExpressionTreeVisitor(
                RelationalQueryModelVisitor queryModelVisitor, IQuerySource querySource)
                : base(queryModelVisitor)
            {
                _querySource = querySource;
            }

            private new RelationalQueryModelVisitor QueryModelVisitor
            {
                get { return (RelationalQueryModelVisitor)base.QueryModelVisitor; }
            }

            protected override Expression VisitMemberExpression(MemberExpression memberExpression)
            {
                QueryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
                        (property, querySource, selectExpression)
                            => selectExpression.AddToProjection(property, querySource));

                return base.VisitMemberExpression(memberExpression);
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                QueryModelVisitor
                    .BindMethodCallExpression(
                        methodCallExpression,
                        (property, querySource, selectExpression)
                            => selectExpression.AddToProjection(property, querySource));

                return base.VisitMethodCallExpression(methodCallExpression);
            }

            protected override Expression VisitEntityQueryable(Type elementType)
            {
                var queryMethodInfo = CreateValueReaderMethodInfo;
                var entityType = QueryModelVisitor.QueryCompilationContext.Model.GetEntityType(elementType);

                var selectExpression = new SelectExpression();
                var tableName = entityType.TableName();

                selectExpression
                    .AddTable(
                        new TableExpression(
                            tableName,
                            entityType.Schema(),
                            _querySource.ItemName.StartsWith("<generated>_")
                                ? tableName.First().ToString().ToLower()
                                : _querySource.ItemName,
                            _querySource));

                QueryModelVisitor._queriesBySource.Add(_querySource, selectExpression);

                var queryMethodArguments
                    = new List<Expression>
                        {
                            Expression.Constant(_querySource),
                            QueryContextParameter,
                            QuerySourceScopeParameter,
                            _readerParameter
                        };

                if (QueryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
                {
                    foreach (var property in entityType.Properties)
                    {
                        selectExpression.AddToProjection(property, _querySource);
                    }

                    queryMethodInfo = CreateEntityMethodInfo.MakeGenericMethod(elementType);

                    queryMethodArguments.Add(Expression.Constant(0));
                    queryMethodArguments.Add(Expression.Constant(entityType));
                }

                return Expression.Call(
                    QueryModelVisitor.QueryCompilationContext.QueryMethodProvider.QueryMethod
                        .MakeGenericMethod(queryMethodInfo.ReturnType),
                    QueryContextParameter,
                    Expression.Constant(new CommandBuilder(selectExpression, QueryModelVisitor.QueryCompilationContext)),
                    Expression.Lambda(
                        Expression.Call(queryMethodInfo, queryMethodArguments),
                        _readerParameter));
            }
        }

        private class RelationalProjectionSubQueryExpressionTreeVisitor : ProjectionExpressionTreeVisitor
        {
            public RelationalProjectionSubQueryExpressionTreeVisitor(RelationalQueryModelVisitor queryModelVisitor)
                : base(queryModelVisitor)
            {
            }

            private new RelationalQueryModelVisitor QueryModelVisitor
            {
                get { return (RelationalQueryModelVisitor)base.QueryModelVisitor; }
            }

            protected override Expression VisitMemberExpression(MemberExpression memberExpression)
            {
                QueryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
                        (property, querySource, selectExpression)
                            => selectExpression.AddToProjection(property, querySource));

                return base.VisitMemberExpression(memberExpression);
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                QueryModelVisitor
                    .BindMethodCallExpression(
                        methodCallExpression,
                        (property, querySource, selectExpression)
                            => selectExpression.AddToProjection(property, querySource));

                return base.VisitMethodCallExpression(methodCallExpression);
            }
        }

        private class RelationalOrderingExpressionTreeVisitor : DefaultQueryExpressionTreeVisitor
        {
            private readonly Ordering _ordering;

            public RelationalOrderingExpressionTreeVisitor(
                RelationalQueryModelVisitor queryModelVisitor, Ordering ordering)
                : base(queryModelVisitor)
            {
                _ordering = ordering;
            }

            private new RelationalQueryModelVisitor QueryModelVisitor
            {
                get { return (RelationalQueryModelVisitor)base.QueryModelVisitor; }
            }

            protected override Expression VisitMemberExpression(MemberExpression memberExpression)
            {
                QueryModelVisitor
                    .BindMemberExpression(
                        memberExpression,
                        (property, querySource, selectExpression)
                            => selectExpression
                                .AddToProjection(
                                    selectExpression
                                        .AddToOrderBy(property, querySource, _ordering.OrderingDirection)));

                return base.VisitMemberExpression(memberExpression);
            }

            protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
            {
                QueryModelVisitor
                    .BindMethodCallExpression(
                        methodCallExpression,
                        (property, querySource, selectExpression)
                            => selectExpression
                                .AddToProjection(
                                    selectExpression
                                        .AddToOrderBy(property, querySource, _ordering.OrderingDirection)));

                return base.VisitMethodCallExpression(methodCallExpression);
            }
        }
    }
}
