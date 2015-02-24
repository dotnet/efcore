// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class IncludeExpressionTreeVisitor : ExpressionTreeVisitorBase
    {
        private readonly IQuerySource _querySource;
        private readonly IReadOnlyList<INavigation> _navigationPath;
        private readonly RelationalQueryCompilationContext _queryCompilationContext;
        private readonly bool _querySourceRequiresTracking;

        private bool _foundCreateEntityForQuerySource;

        public IncludeExpressionTreeVisitor(
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));

            _querySource = querySource;
            _navigationPath = navigationPath;
            _queryCompilationContext = queryCompilationContext;
            _querySourceRequiresTracking = querySourceRequiresTracking;
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            if (expression.Method.MethodIsClosedFormOf(RelationalQueryModelVisitor.CreateEntityMethodInfo)
                && ((ConstantExpression)expression.Arguments[0]).Value == _querySource)
            {
                _foundCreateEntityForQuerySource = true;
            }

            if (expression.Method.MethodIsClosedFormOf(_queryCompilationContext.QueryMethodProvider.QueryMethod))
            {
                _foundCreateEntityForQuerySource = false;

                var newExpression = base.VisitMethodCallExpression(expression);

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

            return base.VisitMethodCallExpression(expression);
        }

        private IEnumerable<Expression> CreateIncludeRelatedValuesStrategyFactories(
            IQuerySource querySource,
            IEnumerable<INavigation> navigationPath)
        {
            var selectExpression
                = _queryCompilationContext.FindSelectExpression(querySource);

            var targetTableExpression
                = selectExpression.FindTableForQuerySource(querySource);

            var readerIndex = 0;
            var canProduceInnerJoin = true;

            foreach (var navigation in navigationPath)
            {
                var targetEntityType = navigation.GetTargetType();
                var targetTableName = _queryCompilationContext.GetTableName(targetEntityType);
                var targetTableAlias = targetTableName.First().ToString().ToLower();

                if (!navigation.IsCollection())
                {
                    var joinedTableExpression
                        = new TableExpression(
                            targetTableName,
                            _queryCompilationContext.GetSchema(targetEntityType),
                            targetTableAlias,
                            querySource);

                    var readerOffset = selectExpression.Projection.Count;

                    canProduceInnerJoin
                        = canProduceInnerJoin
                          && (navigation.ForeignKey.IsRequired
                              && navigation.PointsToPrincipal);

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
                                        new ColumnExpression(
                                            _queryCompilationContext.GetColumnName(p),
                                            p,
                                            joinedTableExpression)) - readerOffset,
                                querySource: null);

                    joinExpression.Predicate
                        = BuildJoinEqualityExpression(
                            navigation,
                            (navigation.PointsToPrincipal
                                ? targetEntityType
                                : navigation.EntityType)
                                .GetPrimaryKey().Properties,
                            navigation.PointsToPrincipal ? targetTableExpression : joinExpression,
                            navigation.PointsToPrincipal ? joinExpression : targetTableExpression);

                    targetTableExpression = joinedTableExpression;

                    yield return
                        Expression.Lambda(
                            Expression.Call(
                                _queryCompilationContext.QueryMethodProvider
                                    .CreateReferenceIncludeRelatedValuesStrategyMethod,
                                Expression.Convert(EntityQueryModelVisitor.QueryContextParameter, typeof(RelationalQueryContext)),
                                Expression.Constant(readerIndex),
                                Expression.Constant(readerOffset),
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
                            ((ColumnExpression)selectExpression.OrderBy.Last().Expression).TableAlias);

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

                    var primaryKeyProperties = navigation.EntityType.GetPrimaryKey().Properties;

                    var innerJoinExpression
                        = targetSelectExpression.AddInnerJoin(innerJoinSelectExpression);

                    foreach (var ordering in selectExpression.OrderBy)
                    {
                        var columnExpression = (ColumnExpression)ordering.Expression;

                        targetSelectExpression
                            .AddToOrderBy(
                                columnExpression.Alias ?? columnExpression.Name,
                                columnExpression.Property,
                                innerJoinExpression,
                                ordering.OrderingDirection);
                    }

                    innerJoinExpression.Predicate
                        = BuildJoinEqualityExpression(
                            navigation,
                            primaryKeyProperties,
                            targetTableExpression,
                            innerJoinExpression);

                    var readerParameter = Expression.Parameter(typeof(DbDataReader));

                    selectExpression = targetSelectExpression;
                    readerIndex++;

                    yield return
                        Expression.Lambda(
                            Expression.Call(
                                _queryCompilationContext.QueryMethodProvider
                                    .CreateCollectionIncludeRelatedValuesStrategyMethod,
                                Expression.Call(
                                    _queryCompilationContext.QueryMethodProvider.QueryMethod
                                        .MakeGenericMethod(typeof(IValueReader)),
                                    EntityQueryModelVisitor.QueryContextParameter,
                                    Expression.Constant(new CommandBuilder(targetSelectExpression, _queryCompilationContext)),
                                    Expression.Lambda(
                                        Expression.Call(
                                            _createValueReaderForIncludeMethodInfo,
                                            EntityQueryModelVisitor.QueryContextParameter,
                                            readerParameter,
                                            Expression.Constant(targetEntityType)),
                                        readerParameter)),
                                materializer));
                }
            }
        }

        private static readonly MethodInfo _createValueReaderForIncludeMethodInfo
            = typeof(IncludeExpressionTreeVisitor).GetTypeInfo()
                .GetDeclaredMethod("CreateValueReaderForInclude");

        [UsedImplicitly]
        private static IValueReader CreateValueReaderForInclude(
            QueryContext queryContext, DbDataReader dataReader, IEntityType entityType)
        {
            return ((RelationalQueryContext)queryContext).ValueReaderFactory.Create(dataReader);
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
            IReadOnlyCollection<ColumnExpression> projections,
            TableExpressionBase tableExpression,
            IProperty property)
        {
            if (projections.Count == 0)
            {
                return new ColumnExpression(
                    _queryCompilationContext.GetColumnName(property),
                    property,
                    tableExpression);
            }

            var matchingColumnExpression
                = projections.Last(p => p.Property == property);

            return new ColumnExpression(
                matchingColumnExpression.Alias ?? matchingColumnExpression.Name,
                property,
                tableExpression);
        }

        private static IEnumerable<ColumnExpression> ExtractProjections(TableExpressionBase tableExpression)
        {
            var selectExpression = tableExpression as SelectExpression;

            if (selectExpression != null)
            {
                return selectExpression.Projection.ToList();
            }

            var joinExpression = tableExpression as JoinExpressionBase;

            return joinExpression != null
                ? ExtractProjections(joinExpression.TableExpression)
                : Enumerable.Empty<ColumnExpression>();
        }
    }
}
