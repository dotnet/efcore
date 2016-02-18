// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class IncludeExpressionVisitor : ExpressionVisitorBase
    {
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IQuerySqlGeneratorFactory _querySqlGeneratorFactory;
        private readonly IQuerySource _querySource;
        private readonly IReadOnlyList<INavigation> _navigationPath;
        private readonly RelationalQueryCompilationContext _queryCompilationContext;
        private readonly LambdaExpression _accessorLambda;
        private readonly IReadOnlyList<int> _queryIndexes;
        private readonly bool _querySourceRequiresTracking;

        public IncludeExpressionVisitor(
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] IShaperCommandContextFactory shaperCommandContextFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IQuerySqlGeneratorFactory querySqlGeneratorFactory,
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] LambdaExpression accessorLambda,
            [NotNull] IReadOnlyList<int> queryIndexes,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(shaperCommandContextFactory, nameof(shaperCommandContextFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(querySqlGeneratorFactory, nameof(querySqlGeneratorFactory));
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            Check.NotNull(accessorLambda, nameof(accessorLambda));
            Check.NotNull(queryIndexes, nameof(queryIndexes));

            _selectExpressionFactory = selectExpressionFactory;
            _materializerFactory = materializerFactory;
            _shaperCommandContextFactory = shaperCommandContextFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
            _querySqlGeneratorFactory = querySqlGeneratorFactory;
            _querySource = querySource;
            _navigationPath = navigationPath;
            _queryCompilationContext = queryCompilationContext;
            _accessorLambda = accessorLambda;
            _queryIndexes = queryIndexes;
            _querySourceRequiresTracking = querySourceRequiresTracking;
        }

        [CallsMakeGenericMethod(nameof(QueryMethodProvider._Include), typeof(TypeArgumentCategory.EntityTypes), TargetType = typeof(QueryMethodProvider))]
        [CallsMakeGenericMethod(nameof(AsyncQueryMethodProvider._Include), typeof(TypeArgumentCategory.EntityTypes), TargetType = typeof(AsyncQueryMethodProvider))]
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var resultType = methodCallExpression.Type.TryGetSequenceType();

            if (resultType != null
                && methodCallExpression.Method.Name != "_ToSequence")
            {
                return
                    Expression.Call(
                        _queryCompilationContext.QueryMethodProvider.IncludeMethod
                            .MakeGenericMethod(resultType),
                        Expression.Convert(
                            EntityQueryModelVisitor.QueryContextParameter,
                            typeof(RelationalQueryContext)),
                        methodCallExpression,
                        _accessorLambda,
                        Expression.Constant(_navigationPath),
                        Expression.NewArrayInit(
                            _queryCompilationContext.QueryMethodProvider.IncludeRelatedValuesFactoryType,
                            CreateIncludeRelatedValuesStrategyFactories(_querySource, _navigationPath)),
                        Expression.Constant(_querySourceRequiresTracking));
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private IEnumerable<Expression> CreateIncludeRelatedValuesStrategyFactories(
            IQuerySource querySource,
            IEnumerable<INavigation> navigationPath)
        {
            var selectExpression
                = _queryCompilationContext.FindSelectExpression(querySource);

            var targetTableExpression
                = selectExpression.GetTableForQuerySource(querySource);

            var canProduceInnerJoin = true;
            var navigationCount = 0;

            foreach (var navigation in navigationPath)
            {
                var queryIndex = _queryIndexes[navigationCount];

                navigationCount++;

                var targetEntityType = navigation.GetTargetType();
                var targetTableName = _relationalAnnotationProvider.For(targetEntityType).TableName;
                var targetTableAlias = targetTableName[0].ToString().ToLower();

                if (!navigation.IsCollection())
                {
                    var joinedTableExpression
                        = new TableExpression(
                            targetTableName,
                            _relationalAnnotationProvider.For(targetEntityType).Schema,
                            targetTableAlias,
                            querySource);

                    var valueBufferOffset = selectExpression.Projection.Count;

                    canProduceInnerJoin
                        = canProduceInnerJoin
                          && navigation.ForeignKey.IsRequired
                          && navigation.IsDependentToPrincipal();

                    var joinExpression
                        = canProduceInnerJoin
                            ? selectExpression
                                .AddInnerJoin(joinedTableExpression)
                            : selectExpression
                                .AddLeftOuterJoin(joinedTableExpression);

                    var oldPredicate = selectExpression.Predicate;

                    var materializer
                        = CreateReferenceMaterializer(
                            targetEntityType,
                            selectExpression,
                            joinedTableExpression,
                            valueBufferOffset);

                    if (selectExpression.Predicate != oldPredicate)
                    {
                        var newJoinExpression = AdjustJoinExpression(selectExpression, joinExpression);

                        selectExpression.Predicate = oldPredicate;
                        selectExpression.RemoveTable(joinExpression);
                        selectExpression.AddTable(newJoinExpression);
                        joinExpression = newJoinExpression;
                    }

                    SetJoinPredicate(querySource, joinExpression, navigation, targetTableExpression);

                    targetTableExpression = joinedTableExpression;

                    yield return
                        Expression.Lambda(
                            Expression.Call(
                                _queryCompilationContext.QueryMethodProvider
                                    .CreateReferenceIncludeRelatedValuesStrategyMethod,
                                Expression.Convert(
                                    EntityQueryModelVisitor.QueryContextParameter,
                                    typeof(RelationalQueryContext)),
                                Expression.Constant(valueBufferOffset),
                                Expression.Constant(queryIndex),
                                materializer));
                }
                else
                {
                    EnsurePrincipalOrdering(querySource, selectExpression, navigation);

                    var targetSelectExpression = selectExpression.Clone();

                    if (targetSelectExpression.Predicate != null
                        || targetSelectExpression.Limit != null
                        || targetSelectExpression.Offset != null
                        || targetSelectExpression.IsDistinct)
                    {
                        var innerSelectExpression = targetSelectExpression.PushDownSubquery();
                        
                        LiftOrderBy(innerSelectExpression, targetSelectExpression);

                        targetTableExpression = innerSelectExpression;
                    }

                    TableExpressionBase joinedTableExpression
                        = new TableExpression(
                            targetTableName,
                            _relationalAnnotationProvider.For(targetEntityType).Schema,
                            targetTableAlias,
                            querySource);

                    var joinSelectExpression = _selectExpressionFactory.Create();

                    var materializer
                        = _materializerFactory
                            .CreateMaterializer(
                                targetEntityType,
                                joinSelectExpression,
                                (p, se) => se.AddToProjection(
                                    new AliasExpression(
                                        new ColumnExpression(
                                            _relationalAnnotationProvider.For(p).ColumnName,
                                            p,
                                            // ReSharper disable once AccessToModifiedClosure
                                            joinedTableExpression))),
                                querySource: null);

                    foreach (var expression in joinSelectExpression.Projection)
                    {
                        targetSelectExpression.AddToProjection(expression);
                    }

                    if (joinSelectExpression.Predicate != null)
                    {
                        joinSelectExpression.Alias = joinedTableExpression.Alias;
                        joinSelectExpression.AddTable(joinedTableExpression);
                        joinSelectExpression.ClearProjection();
                        joinSelectExpression.IsProjectStar = true;
                        joinSelectExpression.QuerySource = querySource;

                        joinedTableExpression = joinSelectExpression;
                    }

                    var joinExpression
                        = targetSelectExpression.AddLeftOuterJoin(joinedTableExpression);

                    var primaryKeyColumns = new List<Expression>();

                    SetJoinPredicate(
                        querySource, joinExpression, navigation, targetTableExpression, primaryKeyColumns);

                    for (var i = 0; i < navigation.ForeignKey.Properties.Count; i++)
                    {
                        targetSelectExpression
                            .ReplaceInProjection(
                                navigation.ForeignKey.Properties[i],
                                primaryKeyColumns[i]);

                        Expression notNullExpression
                            = Expression.Not(new IsNullExpression(primaryKeyColumns[i]));

                        targetSelectExpression.Predicate
                            = targetSelectExpression.Predicate == null
                                ? notNullExpression
                                : Expression.AndAlso(targetSelectExpression.Predicate, notNullExpression);
                    }

                    targetTableExpression = joinedTableExpression;
                    selectExpression = targetSelectExpression;

                    yield return
                        Expression.Lambda(
                            Expression.Call(
                                _queryCompilationContext.QueryMethodProvider
                                    .CreateCollectionIncludeRelatedValuesStrategyMethod,
                                Expression.Call(
                                    _queryCompilationContext.QueryMethodProvider.QueryMethod,
                                    EntityQueryModelVisitor.QueryContextParameter,
                                    Expression.Constant(
                                        _shaperCommandContextFactory.Create(
                                            () => _querySqlGeneratorFactory
                                                .CreateDefault(targetSelectExpression))),
                                    Expression.Constant(queryIndex, typeof(int?))),
                                materializer));
                }
            }
        }

        private LambdaExpression CreateReferenceMaterializer(
            IEntityType targetEntityType,
            SelectExpression selectExpression,
            TableExpressionBase joinedTableExpression,
            int valueBufferOffset)
        {
            return _materializerFactory
                .CreateMaterializer(
                    targetEntityType,
                    selectExpression,
                    (p, se) => se.AddToProjection(
                        new AliasExpression(
                            new ColumnExpression(
                                _relationalAnnotationProvider.For(p).ColumnName,
                                p,
                                joinedTableExpression))) - valueBufferOffset,
                    querySource: null);
        }

        private void SetJoinPredicate(
            IQuerySource querySource,
            JoinExpressionBase joinExpression,
            INavigation navigation,
            TableExpressionBase targetTableExpression,
            ICollection<Expression> primaryKeyColumns = null)
        {
            joinExpression.Predicate
                = BuildJoinEqualityExpression(
                    navigation,
                    navigation.IsDependentToPrincipal() ? targetTableExpression : joinExpression,
                    navigation.IsDependentToPrincipal() ? joinExpression : targetTableExpression,
                    querySource,
                    primaryKeyColumns);
        }

        private void EnsurePrincipalOrdering(
            IQuerySource querySource,
            SelectExpression selectExpression,
            INavigation navigation)
        {
            var principalTable
                = selectExpression.Tables.Count == 1
                  && selectExpression.Tables
                      .OfType<SelectExpression>()
                      .Any(s => s.Tables.Any(t => t.QuerySource == querySource))
                    // true when select is wrapped e.g. when RowNumber paging is enabled
                    ? selectExpression.Tables[0]
                    : selectExpression.Tables.Last(t => t.QuerySource == querySource);

            foreach (var property in navigation.ForeignKey.PrincipalKey.Properties)
            {
                selectExpression
                    .AddToOrderBy(
                        _relationalAnnotationProvider.For(property).ColumnName,
                        property,
                        principalTable,
                        OrderingDirection.Asc);
            }
        }

        private static void LiftOrderBy(
            SelectExpression innerSelectExpression, SelectExpression targetSelectExpression)
        {
            foreach (var ordering in innerSelectExpression.OrderBy)
            {
                var orderingExpression = ordering.Expression;
                var aliasExpression = ordering.Expression as AliasExpression;

                if (aliasExpression?.Alias != null)
                {
                    var columnExpression = aliasExpression.TryGetColumnExpression();

                    if (columnExpression != null)
                    {
                        orderingExpression
                            = new ColumnExpression(
                                aliasExpression.Alias,
                                columnExpression.Property,
                                columnExpression.Table);
                    }
                }

                var index = innerSelectExpression.AddToProjection(orderingExpression);
                var expression = innerSelectExpression.Projection[index];

                var newExpression
                    = targetSelectExpression.UpdateColumnExpression(expression, innerSelectExpression);

                targetSelectExpression
                    .AddToOrderBy(new Ordering(newExpression, ordering.OrderingDirection));
            }

            if (innerSelectExpression.Limit == null
                && innerSelectExpression.Offset == null)
            {
                innerSelectExpression.ClearOrderBy();
            }
        }

        private JoinExpressionBase AdjustJoinExpression(
            SelectExpression selectExpression, JoinExpressionBase joinExpression)
        {
            var subquery = new SelectExpression(_querySqlGeneratorFactory, joinExpression.Alias);

            subquery.AddTable(joinExpression.TableExpression);
            subquery.IsProjectStar = true;
            subquery.Predicate = selectExpression.Predicate;

            var newJoinExpression = joinExpression is LeftOuterJoinExpression
                ? (JoinExpressionBase)new LeftOuterJoinExpression(subquery)
                : new InnerJoinExpression(subquery);

            newJoinExpression.QuerySource = joinExpression.QuerySource;
            newJoinExpression.Alias = joinExpression.Alias;

            return newJoinExpression;
        }

        private Expression BuildJoinEqualityExpression(
            INavigation navigation,
            TableExpressionBase targetTableExpression,
            TableExpressionBase joinExpression,
            IQuerySource querySource,
            ICollection<Expression> primaryKeyColumns = null)
        {
            Expression joinPredicateExpression = null;

            var targetTableProjections = ExtractProjections(targetTableExpression).ToList();
            var joinTableProjections = ExtractProjections(joinExpression).ToList();

            for (var i = 0; i < navigation.ForeignKey.Properties.Count; i++)
            {
                var principalKeyProperty = navigation.ForeignKey.PrincipalKey.Properties[i];
                var foreignKeyProperty = navigation.ForeignKey.Properties[i];

                var foreignKeyColumnExpression
                    = BuildColumnExpression(
                        targetTableProjections, targetTableExpression, foreignKeyProperty, querySource);

                var primaryKeyColumnExpression
                    = BuildColumnExpression(
                        joinTableProjections, joinExpression, principalKeyProperty, querySource);

                primaryKeyColumns?.Add(primaryKeyColumnExpression);

                var primaryKeyExpression = primaryKeyColumnExpression;

                if (foreignKeyColumnExpression.Type != primaryKeyExpression.Type)
                {
                    if (foreignKeyColumnExpression.Type.IsNullableType()
                        && !primaryKeyExpression.Type.IsNullableType())
                    {
                        primaryKeyExpression
                            = Expression.Convert(primaryKeyExpression, foreignKeyColumnExpression.Type);
                    }
                    else if (primaryKeyExpression.Type.IsNullableType()
                             && !foreignKeyColumnExpression.Type.IsNullableType())
                    {
                        foreignKeyColumnExpression
                            = Expression.Convert(foreignKeyColumnExpression, primaryKeyColumnExpression.Type);
                    }
                }

                var equalExpression
                    = Expression.Equal(foreignKeyColumnExpression, primaryKeyExpression);

                joinPredicateExpression
                    = joinPredicateExpression == null
                        ? equalExpression
                        : Expression.AndAlso(joinPredicateExpression, equalExpression);
            }

            return joinPredicateExpression;
        }

        private Expression BuildColumnExpression(
            IReadOnlyCollection<Expression> projections,
            TableExpressionBase tableExpression,
            IProperty property,
            IQuerySource querySource)
        {
            Check.NotNull(property, nameof(property));

            if (projections.Count == 0)
            {
                return new ColumnExpression(
                    _relationalAnnotationProvider.For(property).ColumnName,
                    property,
                    tableExpression);
            }

            var aliasExpressions
                = projections
                    .OfType<AliasExpression>()
                    .Where(p => p.TryGetColumnExpression()?.Property == property)
                    .ToList();

            var aliasExpression
                = aliasExpressions.Count == 1
                    ? aliasExpressions[0]
                    : aliasExpressions.Last(ae => ae.TryGetColumnExpression().Table.QuerySource == querySource);

            return new ColumnExpression(
                aliasExpression.Alias ?? aliasExpression.TryGetColumnExpression().Name,
                property,
                tableExpression);
        }

        private static IEnumerable<Expression> ExtractProjections(TableExpressionBase tableExpression)
        {
            var selectExpression = tableExpression as SelectExpression;

            if (selectExpression != null)
            {
                return selectExpression.Projection.ToList();
            }

            var joinExpression = tableExpression as JoinExpressionBase;

            return joinExpression != null
                ? ExtractProjections(joinExpression.TableExpression)
                : Enumerable.Empty<Expression>();
        }
    }
}
