// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class IncludeExpressionVisitor : ExpressionVisitorBase
    {
        private readonly IQuerySource _querySource;
        private readonly IReadOnlyList<INavigation> _navigationPath;
        private readonly RelationalQueryCompilationContext _queryCompilationContext;
        private readonly IReadOnlyList<int> _readerIndexes;
        private readonly bool _querySourceRequiresTracking;

        private bool _foundCreateEntityForQuerySource;

        public IncludeExpressionVisitor(
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] IReadOnlyList<int> readerIndexes,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _querySource = querySource;
            _navigationPath = navigationPath;
            _queryCompilationContext = queryCompilationContext;
            _readerIndexes = readerIndexes;
            _querySourceRequiresTracking = querySourceRequiresTracking;
        }

        protected override Expression VisitMethodCall([NotNull] MethodCallExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo)
                && ((ConstantExpression)expression.Arguments[0]).Value == _querySource)
            {
                _foundCreateEntityForQuerySource = true;
            }

            if (expression.Method.MethodIsClosedFormOf(_queryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
            {
                _foundCreateEntityForQuerySource = false;

                var newExpression = base.VisitMethodCall(expression);

                if (_foundCreateEntityForQuerySource)
                {
                    return
                        Expression.Call(
                            _queryCompilationContext.QueryMethodProvider.IncludeMethod
                                .MakeGenericMethod(expression.Method.GetGenericArguments()[0]),
                            Expression.Convert(expression.Arguments[0], typeof(RelationalQueryContext)),
                            expression,
                            Expression.Constant(_querySource),
                            Expression.Constant(_navigationPath),
                            Expression.NewArrayInit(
                                _queryCompilationContext.QueryMethodProvider.IncludeRelatedValuesFactoryType,
                                CreateIncludeRelatedValuesStrategyFactories(_querySource, _navigationPath)),
                            Expression.Constant(_querySourceRequiresTracking));
                }

                return newExpression;
            }

            return base.VisitMethodCall(expression);
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

            var includeReferenceCount = 0;
            foreach (var navigation in navigationPath)
            {
                var targetEntityType = navigation.GetTargetType();
                var targetTableName = _queryCompilationContext.GetTableName(targetEntityType);
                var targetTableAlias = targetTableName[0].ToString().ToLower();

                if (!navigation.IsCollection())
                {
                    var joinedTableExpression
                        = new TableExpression(
                            targetTableName,
                            _queryCompilationContext.GetSchema(targetEntityType),
                            targetTableAlias,
                            querySource);

                    var valueBufferOffset = selectExpression.Projection.Count;

                    canProduceInnerJoin
                        = canProduceInnerJoin
                          && (navigation.ForeignKey.IsRequired
                              && navigation.PointsToPrincipal());

                    var joinExpression
                        = canProduceInnerJoin
                            ? selectExpression
                                .AddInnerJoin(joinedTableExpression)
                            : selectExpression
                                .AddOuterJoin(joinedTableExpression);

                    var materializer
                        = new MaterializerFactory(
                            _queryCompilationContext.EntityMaterializerSource)
                            .CreateMaterializer(
                                targetEntityType,
                                selectExpression,
                                projectionAdder:
                                    (p, se) => se.AddToProjection(
                                        new AliasExpression(
                                            new ColumnExpression(
                                                _queryCompilationContext.GetColumnName(p),
                                                p,
                                                joinedTableExpression))) - valueBufferOffset,
                                querySource: null);

                    joinExpression.Predicate
                        = BuildJoinEqualityExpression(
                            navigation,
                            (navigation.PointsToPrincipal()
                                ? targetEntityType
                                : navigation.EntityType)
                                .GetPrimaryKey().Properties,
                            navigation.PointsToPrincipal() ? targetTableExpression : joinExpression,
                            navigation.PointsToPrincipal() ? joinExpression : targetTableExpression);

                    targetTableExpression = joinedTableExpression;

                    var readerIndex = _readerIndexes[includeReferenceCount];
                    includeReferenceCount++;

                    yield return
                        Expression.Lambda(
                            Expression.Call(
                                _queryCompilationContext.QueryMethodProvider
                                    .CreateReferenceIncludeRelatedValuesStrategyMethod,
                                Expression.Convert(
                                    EntityQueryModelVisitor.QueryContextParameter,
                                    typeof(RelationalQueryContext)),
                                Expression.Constant(valueBufferOffset),
                                Expression.Constant(readerIndex),
                                materializer));
                }
                else
                {
                    var principalTable
                        = selectExpression.Tables.Last(t => t.QuerySource == querySource);

                    foreach (var property in navigation.EntityType.GetPrimaryKey().Properties)
                    {
                        selectExpression
                            .AddToOrderBy(
                                _queryCompilationContext.GetColumnName(property),
                                property,
                                principalTable,
                                OrderingDirection.Asc);
                    }

                    var targetSelectExpression = new SelectExpression();

                    targetTableExpression
                        = new TableExpression(
                            targetTableName,
                            _queryCompilationContext.GetSchema(targetEntityType),
                            targetTableAlias,
                            querySource);

                    targetSelectExpression.AddTable(targetTableExpression);

                    var materializer
                        = new MaterializerFactory(
                            _queryCompilationContext.EntityMaterializerSource)
                            .CreateMaterializer(
                                targetEntityType,
                                targetSelectExpression,
                                (p, se) => se.AddToProjection(
                                    _queryCompilationContext.GetColumnName(p),
                                    p,
                                    querySource),
                                querySource: null);

                    var innerJoinSelectExpression
                        = selectExpression.Clone(
                            selectExpression.OrderBy.Select(o => o.Expression).Last(o => o.IsAliasWithColumnExpression())
                                .TryGetColumnExpression().TableAlias);

                    innerJoinSelectExpression.IsDistinct = true;
                    innerJoinSelectExpression.ClearProjection();

                    foreach (var expression
                        in innerJoinSelectExpression.OrderBy
                            .Select(o => o.Expression))
                    {
                        innerJoinSelectExpression.AddToProjection(expression);
                    }

                    innerJoinSelectExpression.ClearOrderBy();

                    var primaryKeyProperties = navigation.EntityType.GetPrimaryKey().Properties;

                    var innerJoinExpression
                        = targetSelectExpression.AddInnerJoin(innerJoinSelectExpression);

                    targetSelectExpression.UpdateOrderByColumnBinding(selectExpression.OrderBy, innerJoinExpression);

                    innerJoinExpression.Predicate
                        = BuildJoinEqualityExpression(
                            navigation,
                            primaryKeyProperties,
                            targetTableExpression,
                            innerJoinExpression);

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
                                        new CommandBuilder(
                                            () => _queryCompilationContext.CreateSqlQueryGenerator(targetSelectExpression),
                                            _queryCompilationContext.ValueBufferFactoryFactory))),
                                materializer));
                }
            }
        }

        private Expression BuildJoinEqualityExpression(
            INavigation navigation,
            IReadOnlyList<IProperty> primaryKeyProperties,
            TableExpressionBase targetTableExpression,
            TableExpressionBase joinExpression)
        {
            Expression joinPredicateExpression = null;

            var targetTableProjections = ExtractProjections(targetTableExpression).ToList();
            var joinTableProjections = ExtractProjections(joinExpression).ToList();

            for (var i = 0; i < navigation.ForeignKey.Properties.Count; i++)
            {
                var primaryKeyProperty = primaryKeyProperties[i];
                var foreignKeyProperty = navigation.ForeignKey.Properties[i];

                var foreignKeyColumnExpression
                    = BuildColumnExpression(targetTableProjections, targetTableExpression, foreignKeyProperty);

                var primaryKeyColumnExpression
                    = BuildColumnExpression(joinTableProjections, joinExpression, primaryKeyProperty);

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
            IProperty property)
        {
            Check.NotNull(property, nameof(property));

            if (projections.Count == 0)
            {
                return new ColumnExpression(
                    _queryCompilationContext.GetColumnName(property),
                    property,
                    tableExpression);
            }

            var matchingColumnExpression
                = projections
                    .OfType<AliasExpression>()
                    .Last(p => p.TryGetColumnExpression()?.Property == property);

            return new ColumnExpression(
                matchingColumnExpression.Alias ?? matchingColumnExpression.TryGetColumnExpression().Name,
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
