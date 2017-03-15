// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

// ReSharper disable ImplicitlyCapturedClosure
namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class IncludeExpressionVisitor : ExpressionVisitorBase
    {
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly ICompositePredicateExpressionVisitorFactory _compositePredicateExpressionVisitorFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IQuerySource _querySource;
        private readonly IReadOnlyList<INavigation> _navigationPath;
        private readonly RelationalQueryCompilationContext _queryCompilationContext;
        private readonly IReadOnlyList<int> _queryIndexes;
        private readonly bool _querySourceRequiresTracking;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IncludeExpressionVisitor(
            [NotNull] SelectExpressionDependencies selectExpressionDependencies,
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] ICompositePredicateExpressionVisitorFactory compositePredicateExpressionVisitorFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] IShaperCommandContextFactory shaperCommandContextFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] IQuerySource querySource,
            [NotNull] IReadOnlyList<INavigation> navigationPath,
            [NotNull] RelationalQueryCompilationContext queryCompilationContext,
            [NotNull] IReadOnlyList<int> queryIndexes,
            bool querySourceRequiresTracking)
        {
            Check.NotNull(selectExpressionDependencies, nameof(selectExpressionDependencies));
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(compositePredicateExpressionVisitorFactory, nameof(compositePredicateExpressionVisitorFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(shaperCommandContextFactory, nameof(shaperCommandContextFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(navigationPath, nameof(navigationPath));
            Check.NotNull(queryCompilationContext, nameof(queryCompilationContext));
            Check.NotNull(queryIndexes, nameof(queryIndexes));

            SelectExpressionDependencies = selectExpressionDependencies;

            _selectExpressionFactory = selectExpressionFactory;
            _compositePredicateExpressionVisitorFactory = compositePredicateExpressionVisitorFactory;
            _materializerFactory = materializerFactory;
            _shaperCommandContextFactory = shaperCommandContextFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
            _querySource = querySource;
            _navigationPath = navigationPath;
            _queryCompilationContext = queryCompilationContext;
            _queryIndexes = queryIndexes;
            _querySourceRequiresTracking = querySourceRequiresTracking;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="SelectExpression" />
        /// </summary>
        protected virtual SelectExpressionDependencies SelectExpressionDependencies { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (methodCallExpression.Method.MethodIsClosedFormOf(
                _queryCompilationContext.QueryMethodProvider.ShapedQueryMethod))
            {
                var shaper
                    = ((ConstantExpression)methodCallExpression.Arguments[2]).Value
                        as Shaper;

                if (shaper != null
                    && shaper.IsShaperForQuerySource(_querySource))
                {
                    var resultType = methodCallExpression.Method.GetGenericArguments()[0];
                    var entityAccessor = shaper.GetAccessorExpression(_querySource);

                    return
                        Expression.Call(
                            _queryCompilationContext.QueryMethodProvider.IncludeMethod.MakeGenericMethod(resultType),
                            Expression.Convert(methodCallExpression.Arguments[0], typeof(RelationalQueryContext)),
                            methodCallExpression,
                            entityAccessor,
                            Expression.Constant(_navigationPath),
                            (Expression)_createRelatedEntitiesLoadersMethodInfo
                                .MakeGenericMethod(_queryCompilationContext.QueryMethodProvider.RelatedEntitiesLoaderType)
                                .Invoke(this, new object[] { _querySource, _navigationPath }),
                            Expression.Constant(_querySourceRequiresTracking));
                }
            }
            else if (methodCallExpression.Method.MethodIsClosedFormOf(
                _queryCompilationContext.QueryMethodProvider.GroupJoinMethod))
            {
                var newMethodCallExpression = TryMatchGroupJoinShaper(methodCallExpression, 2);

                if (!ReferenceEquals(methodCallExpression, newMethodCallExpression))
                {
                    return newMethodCallExpression;
                }

                newMethodCallExpression = TryMatchGroupJoinShaper(methodCallExpression, 3);

                if (!ReferenceEquals(methodCallExpression, newMethodCallExpression))
                {
                    return newMethodCallExpression;
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        private Expression TryMatchGroupJoinShaper(MethodCallExpression methodCallExpression, int shaperArgumentIndex)
        {
            var shaper
                = ((ConstantExpression)methodCallExpression.Arguments[shaperArgumentIndex]).Value
                    as Shaper;

            if (shaper != null
                && shaper.IsShaperForQuerySource(_querySource))
            {
                var groupJoinIncludeArgumentIndex = shaperArgumentIndex + 4;

                var existingGroupJoinIncludeArgument = methodCallExpression.Arguments[groupJoinIncludeArgumentIndex];
                var existingGroupJoinIncludeWithAccessor = existingGroupJoinIncludeArgument as MethodCallExpression;

                var withAccessorMethodInfo
                    = _queryCompilationContext.QueryMethodProvider.GroupJoinIncludeType
                        .GetTypeInfo()
                        .GetDeclaredMethod(nameof(GroupJoinInclude.WithEntityAccessor));

                var existingGroupJoinIncludeExpression
                    = existingGroupJoinIncludeWithAccessor != null
                          && existingGroupJoinIncludeWithAccessor.Method.Equals(withAccessorMethodInfo)
                    ? existingGroupJoinIncludeWithAccessor.Object
                    : existingGroupJoinIncludeArgument;

                var relatedEntitiesLoaders
                    = Expression.Lambda<Func<object>>(
                        (Expression)_createRelatedEntitiesLoadersMethodInfo
                            .MakeGenericMethod(_queryCompilationContext.QueryMethodProvider.RelatedEntitiesLoaderType)
                            .Invoke(this, new object[]
                            {
                                _querySource,
                                _navigationPath
                            }))
                            .Compile()
                            .Invoke();

                var groupJoinInclude
                    = _queryCompilationContext.QueryMethodProvider
                        .CreateGroupJoinInclude(
                            _navigationPath,
                            _querySourceRequiresTracking,
                            (existingGroupJoinIncludeExpression as ConstantExpression)?.Value,
                            relatedEntitiesLoaders);

                if (groupJoinInclude != null)
                {
                    var groupJoinIncludeExpression = (Expression)Expression.Constant(groupJoinInclude);
                    var accessorLambda = shaper.GetAccessorExpression(_querySource) as LambdaExpression;

                    if (accessorLambda != null
                        && accessorLambda.Parameters.Single().Type.GetTypeInfo().IsValueType)
                    {
                        groupJoinIncludeExpression
                            = Expression.Call(
                                groupJoinIncludeExpression,
                                withAccessorMethodInfo,
                                shaper.GetAccessorExpression(_querySource));
                    }

                    var newArguments = methodCallExpression.Arguments.ToList();

                    newArguments[groupJoinIncludeArgumentIndex] = groupJoinIncludeExpression;

                    return methodCallExpression.Update(methodCallExpression.Object, newArguments);
                }
            }

            return methodCallExpression;
        }

        private static readonly MethodInfo _createRelatedEntitiesLoadersMethodInfo
            = typeof(IncludeExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateRelatedEntitiesLoaders));

        [UsedImplicitly]
        private NewArrayExpression CreateRelatedEntitiesLoaders<TRelatedEntitiesLoader>(
            IQuerySource querySource, IEnumerable<INavigation> navigationPath)
        {
            var queryContextParameter = Expression.Parameter(typeof(QueryContext));

            var relatedEntitiesLoaders = new List<Expression<Func<QueryContext, TRelatedEntitiesLoader>>>();

            var selectExpression
                = _queryCompilationContext.FindSelectExpression(querySource);

            var compositePredicateExpressionVisitor
                = _compositePredicateExpressionVisitorFactory.Create();

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
                var targetTableAlias
                    = _queryCompilationContext
                        .CreateUniqueTableAlias(targetTableName[0].ToString().ToLowerInvariant());

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
                            ? selectExpression.AddInnerJoin(joinedTableExpression)
                            : selectExpression.AddLeftOuterJoin(joinedTableExpression);

                    var oldPredicate = selectExpression.Predicate;

                    var materializer
                        = _materializerFactory
                            .CreateMaterializer(
                                targetEntityType,
                                selectExpression,
                                (p, se) => se.AddToProjection(
                                               _relationalAnnotationProvider.For(p).ColumnName,
                                               p,
                                               joinedTableExpression,
                                               querySource) - valueBufferOffset,
                                /*querySource:*/ null,
                                out var _);

                    if (selectExpression.Predicate != oldPredicate)
                    {
                        compositePredicateExpressionVisitor.Visit(selectExpression);

                        var newJoinExpression = AdjustJoinExpression(selectExpression, joinExpression);

                        selectExpression.Predicate = oldPredicate;
                        selectExpression.RemoveTable(joinExpression);
                        selectExpression.AddTable(newJoinExpression);
                        joinExpression = newJoinExpression;
                    }

                    joinExpression.Predicate
                        = BuildJoinEqualityExpression(
                            navigation,
                            navigation.IsDependentToPrincipal() ? targetTableExpression : joinExpression,
                            navigation.IsDependentToPrincipal() ? joinExpression : targetTableExpression,
                            querySource);

                    targetTableExpression = joinedTableExpression;

                    relatedEntitiesLoaders.Add(
                        Expression.Lambda<Func<QueryContext, TRelatedEntitiesLoader>>(
                            Expression.Call(
                                _queryCompilationContext.QueryMethodProvider
                                    .CreateReferenceRelatedEntitiesLoaderMethod,
                                Expression.Constant(valueBufferOffset),
                                Expression.Constant(queryIndex),
                                materializer),
                            queryContextParameter));
                }
                else
                {
                    var principalTable
                        = selectExpression.Tables.Count == 1
                          && selectExpression.Tables
                              .OfType<SelectExpression>()
                              .Any(s => s.Tables.Any(t => t.QuerySource == querySource))
                            // true when select is wrapped e.g. when RowNumber paging is enabled
                            ? selectExpression.Tables[0]
                            : selectExpression.Tables.Last(t => t.QuerySource == querySource);

                    var canGenerateExists
                        = selectExpression.Offset == null
                          && selectExpression.Limit == null
                          && !IsOrderingOnNonPrincipalKeyProperties(
                              selectExpression.OrderBy,
                              navigation.ForeignKey.PrincipalKey.Properties);

                    foreach (var property in navigation.ForeignKey.PrincipalKey.Properties)
                    {
                        selectExpression
                            .AddToOrderBy(
                                property,
                                principalTable,
                                querySource,
                                OrderingDirection.Asc);
                    }

                    var targetSelectExpression = _selectExpressionFactory.Create(_queryCompilationContext);

                    targetTableExpression
                        = new TableExpression(
                            targetTableName,
                            _relationalAnnotationProvider.For(targetEntityType).Schema,
                            targetTableAlias,
                            querySource);

                    targetSelectExpression.AddTable(targetTableExpression);

                    var materializer
                        = _materializerFactory
                            .CreateMaterializer(
                                targetEntityType,
                                targetSelectExpression,
                                (p, se) => se.AddToProjection(
                                    p,
                                    querySource),
                                /*querySource:*/ null,
                                out var _);

                    if (canGenerateExists)
                    {
                        var subqueryExpression = selectExpression.Clone();
                        subqueryExpression.ClearProjection();
                        subqueryExpression.ClearOrderBy();
                        subqueryExpression.IsProjectStar = false;

                        var subqueryTable
                            = subqueryExpression.Tables.Count == 1
                              && subqueryExpression.Tables
                                  .OfType<SelectExpression>()
                                  .Any(s => s.Tables.Any(t => t.QuerySource == querySource))
                                // true when select is wrapped e.g. when RowNumber paging is enabled
                                ? subqueryExpression.Tables[0]
                                : subqueryExpression.Tables.Last(t => t.QuerySource == querySource);

                        var existsPredicateExpression = new ExistsExpression(subqueryExpression);

                        targetSelectExpression.AddToPredicate(existsPredicateExpression);

                        subqueryExpression.AddToPredicate(
                            BuildJoinEqualityExpression(navigation, targetTableExpression, subqueryTable, querySource));

                        compositePredicateExpressionVisitor.Visit(subqueryExpression);

                        var pkPropertiesToFkPropertiesMap = navigation.ForeignKey.PrincipalKey.Properties
                            .Zip(navigation.ForeignKey.Properties, (k, v) => new { PkProperty = k, FkProperty = v })
                            .ToDictionary(x => x.PkProperty, x => x.FkProperty);

                        foreach (var ordering in selectExpression.OrderBy)
                        {
                            var principalKeyProperty
                                = TryGetProperty(ordering.Expression);

                            var referencedForeignKeyProperty = pkPropertiesToFkPropertiesMap[principalKeyProperty];

                            targetSelectExpression
                                .AddToOrderBy(
                                    referencedForeignKeyProperty,
                                    targetTableExpression,
                                    querySource,
                                    ordering.OrderingDirection);
                        }
                    }
                    else
                    {
                        var innerJoinSelectExpression
                            = selectExpression.Clone(principalTable.Alias);

                        innerJoinSelectExpression.ClearProjection();

                        var innerJoinExpression = targetSelectExpression.AddInnerJoin(innerJoinSelectExpression);

                        LiftOrderBy(innerJoinSelectExpression, targetSelectExpression, innerJoinExpression);

                        innerJoinSelectExpression.IsDistinct = true;

                        innerJoinExpression.Predicate
                            = BuildJoinEqualityExpression(
                                navigation,
                                targetTableExpression,
                                innerJoinExpression,
                                querySource);
                    }

                    compositePredicateExpressionVisitor.Visit(targetSelectExpression);

                    selectExpression = targetSelectExpression;

                    relatedEntitiesLoaders.Add(
                        Expression.Lambda<Func<QueryContext, TRelatedEntitiesLoader>>(
                            Expression.Call(
                                _queryCompilationContext.QueryMethodProvider
                                    .CreateCollectionRelatedEntitiesLoaderMethod,
                                queryContextParameter,
                                Expression.Constant(
                                    _shaperCommandContextFactory.Create(() =>
                                            SelectExpressionDependencies.QuerySqlGeneratorFactory.CreateDefault(targetSelectExpression))),
                                Expression.Constant(queryIndex),
                                materializer
                            ),
                            queryContextParameter));
                }
            }

            return Expression.NewArrayInit(
                typeof(Func<QueryContext, TRelatedEntitiesLoader>),
                relatedEntitiesLoaders);
        }

        private PredicateJoinExpressionBase AdjustJoinExpression(
            SelectExpression selectExpression, PredicateJoinExpressionBase joinExpression)
        {
            var subquery
                = new SelectExpression(SelectExpressionDependencies, _queryCompilationContext)
                {
                    Alias = joinExpression.Alias
                };

            subquery.AddTable(joinExpression.TableExpression);
            subquery.ProjectStarTable = joinExpression;
            subquery.IsProjectStar = true;
            subquery.Predicate = selectExpression.Predicate;

            var newJoinExpression
                = joinExpression is LeftOuterJoinExpression
                    ? (PredicateJoinExpressionBase)new LeftOuterJoinExpression(subquery)
                    : new InnerJoinExpression(subquery);

            newJoinExpression.QuerySource = joinExpression.QuerySource;
            newJoinExpression.Alias = joinExpression.Alias;

            return newJoinExpression;
        }

        private static void LiftOrderBy(
            SelectExpression innerJoinSelectExpression,
            SelectExpression targetSelectExpression,
            TableExpressionBase innerJoinExpression)
        {
            var orderings = innerJoinSelectExpression.OrderBy.ToList();

            foreach (var ordering in orderings)
            {
                targetSelectExpression.AddToOrderBy(
                    new Ordering(
                        innerJoinSelectExpression.Projection[innerJoinSelectExpression.AddToProjection(ordering.Expression)]
                            .LiftExpressionFromSubquery(innerJoinExpression), ordering.OrderingDirection));
            }

            if (innerJoinSelectExpression.Limit == null
                && innerJoinSelectExpression.Offset == null)
            {
                innerJoinSelectExpression.ClearOrderBy();
            }
        }

        private Expression BuildJoinEqualityExpression(
            INavigation navigation,
            TableExpressionBase targetTableExpression,
            TableExpressionBase joinExpression,
            IQuerySource querySource)
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

            var candidates
                = projections
                    .Where(p => TryGetProperty(p) == property)
                    .ToList();

            return candidates.Count == 1
                ? candidates[0].LiftExpressionFromSubquery(tableExpression)
                : candidates.Last(c => TryGetQuerySource(c) == querySource).LiftExpressionFromSubquery(tableExpression);
        }

        private static IEnumerable<Expression> ExtractProjections(TableExpressionBase tableExpression)
        {
            var selectExpression = tableExpression as SelectExpression;

            if (selectExpression != null)
            {
                return
                    selectExpression.IsProjectStar
                        ? selectExpression.Tables.SelectMany(ExtractProjections)
                        : selectExpression.Projection.ToList();
            }

            var joinExpression = tableExpression as JoinExpressionBase;

            return joinExpression != null
                ? ExtractProjections(joinExpression.TableExpression)
                : Enumerable.Empty<Expression>();
        }

        private static bool IsOrderingOnNonPrincipalKeyProperties(
                IEnumerable<Ordering> orderings, IReadOnlyList<IProperty> properties)
            => orderings
                .Select(ordering => TryGetProperty(ordering.Expression))
                .Any(property => property == null || !properties.Contains(property));

        private static IProperty TryGetProperty(Expression expression)
        {
            switch (expression)
            {
                case ColumnExpression columnExpression:
                    return columnExpression.Property;
                case AliasExpression aliasExpression:
                    return TryGetProperty(aliasExpression.Expression);
                case ColumnReferenceExpression columnReferenceExpression:
                    return TryGetProperty(columnReferenceExpression.Expression);
            }

            return null;
        }

        private IQuerySource TryGetQuerySource(Expression expression)
        {
            switch (expression)
            {
                case ColumnExpression columnExpression:
                    return columnExpression.Table.QuerySource;
                case AliasExpression aliasExpression:
                    return TryGetQuerySource(aliasExpression.Expression);
                case ColumnReferenceExpression columnReferenceExpression:
                    return columnReferenceExpression.Table.QuerySource;
            }

            return null;
        }
    }
}
