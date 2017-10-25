// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     An expression visitor for translating relational LINQ query projections.
    /// </summary>
    public class RelationalProjectionExpressionVisitor : ProjectionExpressionVisitor
    {
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IQuerySource _querySource;
        private bool _topLevelProjection;

        private readonly Dictionary<Expression, Expression> _sourceExpressionProjectionMapping = new Dictionary<Expression, Expression>();

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalProjectionExpressionVisitor" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="querySource"> The query source. </param>
        public RelationalProjectionExpressionVisitor(
            [NotNull] RelationalProjectionExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)))
        {
            Check.NotNull(dependencies, nameof(dependencies));
            Check.NotNull(querySource, nameof(querySource));

            _sqlTranslatingExpressionVisitorFactory = dependencies.SqlTranslatingExpressionVisitorFactory;
            _entityMaterializerSource = dependencies.EntityMaterializerSource;
            _querySource = querySource;
            _topLevelProjection = true;
        }

        private new RelationalQueryModelVisitor QueryModelVisitor
            => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        /// <summary>
        ///     Visit a member init expression.
        /// </summary>
        /// <param name="memberInitExpression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated member init.
        /// </returns>
        protected override Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            var newMemberInitExpression = base.VisitMemberInit(memberInitExpression);

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression != null)
            {
                foreach (var sourceBinding in memberInitExpression.Bindings)
                {
                    if (sourceBinding is MemberAssignment memberAssignment)
                    {
                        var sourceExpression = memberAssignment.Expression;

                        if (_sourceExpressionProjectionMapping.TryGetValue(sourceExpression, out var sqlExpression))
                        {
                            var memberInfo = memberAssignment.Member;

                            selectExpression.SetProjectionForMemberInfo(
                                memberInfo,
                                sqlExpression);
                        }
                    }
                }
            }

            return newMemberInitExpression;
        }

        /// <summary>
        ///     Visit a method call expression.
        /// </summary>
        /// <param name="methodCallExpression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated method call.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            if (IncludeCompiler.IsIncludeMethod(methodCallExpression))
            {
                return methodCallExpression;
            }

            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                var newArg0 = Visit(methodCallExpression.Arguments[0]);

                if (newArg0 != methodCallExpression.Arguments[0])
                {
                    return Expression.Call(
                        methodCallExpression.Method,
                        newArg0,
                        methodCallExpression.Arguments[1]);
                }

                return methodCallExpression;
            }

            return base.VisitMethodCall(methodCallExpression);
        }

        /// <summary>
        ///     Visit a new expression.
        /// </summary>
        /// <param name="newExpression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated new expression.
        /// </returns>
        protected override Expression VisitNew(NewExpression newExpression)
        {
            Check.NotNull(newExpression, nameof(newExpression));

            if (newExpression.Type == typeof(AnonymousObject)
                || newExpression.Type == typeof(MaterializedAnonymousObject))
            {
                var propertyCallExpressions
                    = ((NewArrayExpression)newExpression.Arguments.Single()).Expressions;

                foreach (var propertyCallExpression in propertyCallExpressions)
                {
                    Visit(propertyCallExpression.RemoveConvert());
                }

                return newExpression;
            }

            var newNewExpression = base.VisitNew(newExpression);

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression != null)
            {
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var sourceExpression = newExpression.Arguments[i];

                    if (_sourceExpressionProjectionMapping.TryGetValue(sourceExpression, out var sqlExpression))
                    {
                        var memberInfo = newExpression.Members?[i];

                        if (memberInfo != null)
                        {
                            selectExpression.SetProjectionForMemberInfo(
                                memberInfo,
                                sqlExpression);
                        }
                    }
                }
            }

            return newNewExpression;
        }

        /// <summary>
        ///     Visits the given node.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression to the translated input expression.
        /// </returns>
        public override Expression Visit(Expression expression)
        {
            if (_topLevelProjection
                && (QueryModelVisitor.Expression as MethodCallExpression)?.Method.MethodIsClosedFormOf(
                    QueryModelVisitor.QueryCompilationContext.QueryMethodProvider.GroupByMethod) == true)
            {
                var translation = new GroupByAggregateTranslatingExpressionVisitor(this)
                    .Translate(expression);

                if (translation != null)
                {
                    QueryModelVisitor.RequiresStreamingGroupResultOperator = false;
                    return translation;
                }
            }

            _topLevelProjection = false;

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (expression != null
                && !(expression is ConstantExpression)
                && !(expression is NewExpression)
                && !(expression is MemberInitExpression)
                && selectExpression != null)
            {
                var existingProjectionsCount = selectExpression.Projection.Count;

                var sqlExpression
                    = _sqlTranslatingExpressionVisitorFactory
                        .Create(QueryModelVisitor, selectExpression, inProjection: true)
                        .Visit(expression);

                if (sqlExpression == null)
                {
                    switch (expression)
                    {
                        case MethodCallExpression methodCallExpression when IncludeCompiler.IsIncludeMethod(methodCallExpression):
                            return base.Visit(expression);

                        case QuerySourceReferenceExpression qsre:
                            if (selectExpression.HandlesQuerySource(qsre.ReferencedQuerySource))
                            {
                                selectExpression.ProjectStarTable = selectExpression.GetTableForQuerySource(qsre.ReferencedQuerySource);
                            }
                            break;

                        default:
                            QueryModelVisitor.RequiresClientProjection = true;
                            break;
                    }
                }
                else
                {
                    selectExpression.RemoveRangeFromProjection(existingProjectionsCount);

                    if (!(expression is QuerySourceReferenceExpression))
                    {
                        if (sqlExpression is NullableExpression nullableExpression)
                        {
                            sqlExpression = nullableExpression.Operand;
                        }

                        if (sqlExpression is ColumnExpression)
                        {
                            var index = selectExpression.AddToProjection(sqlExpression);

                            _sourceExpressionProjectionMapping[expression] = selectExpression.Projection[index];

                            return expression;
                        }
                    }

                    if (!(sqlExpression is ConstantExpression))
                    {
                        var targetExpression
                            = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                                .GetExpression(_querySource);

                        if (targetExpression.Type == typeof(ValueBuffer))
                        {
                            var index = selectExpression.AddToProjection(sqlExpression);

                            _sourceExpressionProjectionMapping[expression] = selectExpression.Projection[index];

                            var readValueExpression
                                = _entityMaterializerSource
                                    .CreateReadValueExpression(
                                        targetExpression,
                                        expression.Type.MakeNullable(),
                                        index,
                                        sqlExpression.FindProperty(expression.Type));

                            var outputDataInfo
                                = (expression as SubQueryExpression)?.QueryModel
                                .GetOutputDataInfo();

                            if (outputDataInfo is StreamedScalarValueInfo)
                            {
                                // Compensate for possible nulls
                                readValueExpression
                                    = Expression.Coalesce(
                                        readValueExpression,
                                        Expression.Default(expression.Type));
                            }

                            return Expression.Convert(readValueExpression, expression.Type);
                        }

                        return expression;
                    }
                }
            }

            return base.Visit(expression);
        }

        private class GroupByAggregateTranslatingExpressionVisitor : RelinqExpressionVisitor
        {
            private readonly RelationalProjectionExpressionVisitor _projectionExpressionVisitor;
            private readonly RelationalQueryModelVisitor _queryModelVisitor;
            private readonly IQuerySource _groupQuerySource;
            private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
            private readonly IEntityMaterializerSource _entityMaterializerSource;
            private readonly SelectExpression _selectExpression;
            private readonly Dictionary<Expression, Expression> _sqlMapping = new Dictionary<Expression, Expression>();
            private bool _translateToSql;
            private Expression _keySelector;
            private const string KeyName = nameof(IGrouping<object, object>.Key);

            private static readonly List<Type> _aggregateResultOperators = new List<Type>
            {
                typeof(AverageResultOperator),
                typeof(CountResultOperator),
                typeof(LongCountResultOperator),
                typeof(MaxResultOperator),
                typeof(MinResultOperator),
                typeof(SumResultOperator)
            };

            public GroupByAggregateTranslatingExpressionVisitor(RelationalProjectionExpressionVisitor projectionExpressionVisitor)
            {
                _projectionExpressionVisitor = projectionExpressionVisitor;
                _queryModelVisitor = projectionExpressionVisitor.QueryModelVisitor;
                _groupQuerySource = projectionExpressionVisitor._querySource;
                _sqlTranslatingExpressionVisitorFactory = projectionExpressionVisitor._sqlTranslatingExpressionVisitorFactory;
                _entityMaterializerSource = projectionExpressionVisitor._entityMaterializerSource;
                _selectExpression = _queryModelVisitor.TryGetQuery(_groupQuerySource);
            }

            public Expression Translate(Expression expression)
            {
                if (!CanTranslate(expression))
                {
                    return null;
                }

                _translateToSql = true;

                Visit(expression);

                _selectExpression.ClearProjection();
                UpdateGroupQuerySourceParameter(typeof(ValueBuffer));

                _translateToSql = false;

                return Visit(expression);
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!_translateToSql)
                {
                    var sqlExpression = _selectExpression.GetProjectionForMemberInfo(node.Member);
                    if (sqlExpression != null)
                    {
                        return BindSqlToValueBuffer(sqlExpression, node.Type);
                    }

                    if (node.Member.Name == KeyName
                        && node.Expression.TryGetReferencedQuerySource() == _groupQuerySource)
                    {
                        var querySourceFinder = new QuerySourceFindingExpressionVisitor();
                        querySourceFinder.Visit(_keySelector);

                        var currentParameter = _queryModelVisitor.CurrentParameter;

                        foreach (var querySource in querySourceFinder.QuerySources)
                        {
                            _queryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(
                                querySource,
                                currentParameter);
                        }

                        return _projectionExpressionVisitor.Visit(_keySelector);
                    }
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
            {
                if (_translateToSql)
                {
                    var sqlExpression = _sqlTranslatingExpressionVisitorFactory.Create(
                            _queryModelVisitor,
                            _selectExpression,
                            inProjection: true)
                        .Visit(subQueryExpression);
                    _sqlMapping[subQueryExpression] = sqlExpression;

                    return subQueryExpression;
                }

                return BindSqlToValueBuffer(_sqlMapping[subQueryExpression], subQueryExpression.Type);
            }

            private Expression BindSqlToValueBuffer(Expression sqlExpression, Type expressionType)
            {
                var targetExpression
                    = _queryModelVisitor.QueryCompilationContext.QuerySourceMapping
                        .GetExpression(_groupQuerySource);

                if (targetExpression.Type == typeof(ValueBuffer))
                {
                    var index = _selectExpression.AddToProjection(sqlExpression);

                    var readValueExpression
                        = _entityMaterializerSource
                            .CreateReadValueExpression(
                                targetExpression,
                                expressionType,
                                index,
                                sqlExpression.FindProperty(expressionType));

                    return Expression.Convert(readValueExpression, expressionType);
                }

                return null;
            }

            private bool CanTranslate(Expression expression)
            {
                // Check for Query shape
                if (IsAggregateGroupBySelector(expression))
                {
                    var groupByResultOperator =
                        (GroupResultOperator)((SubQueryExpression)((MainFromClause)_groupQuerySource).FromExpression)
                        .QueryModel.ResultOperators
                        .Last();

                    _keySelector = groupByResultOperator.KeySelector;
                    var elementSelector = groupByResultOperator.ElementSelector;

                    if (!(elementSelector is QuerySourceReferenceExpression)
                        && _sqlTranslatingExpressionVisitorFactory.Create(
                                _queryModelVisitor,
                                _selectExpression)
                            .Visit(groupByResultOperator.ElementSelector) == null)
                    {
                        return false;
                    }

                    _selectExpression.ClearOrderBy();
                    _selectExpression.ClearProjection();

                    UpdateGroupQuerySourceParameter(typeof(ValueBuffer));

                    _projectionExpressionVisitor.Visit(_keySelector);
                    var columns = _selectExpression.Projection.ToArray();
                    _selectExpression.ClearProjection();

                    if (!(elementSelector is QuerySourceReferenceExpression))
                    {
                        _projectionExpressionVisitor.Visit(groupByResultOperator.ElementSelector);
                    }

                    if (_selectExpression.Projection.Count > 1)
                    {
                        _selectExpression.ClearProjection();
                    }

                    _selectExpression.AddToGroupBy(columns);

                    var shapedQuery =
                        (MethodCallExpression)((MethodCallExpression)_queryModelVisitor.Expression).Arguments[0];

                    var valueBufferShaper = new ValueBufferShaper(_groupQuerySource);

                    var groupShapedQuery = Expression.Call(
                        _queryModelVisitor.QueryCompilationContext
                            .QueryMethodProvider
                            .ShapedQueryMethod
                            .MakeGenericMethod(valueBufferShaper.Type),
                        shapedQuery.Arguments[0],
                        shapedQuery.Arguments[1],
                        Expression.Constant(valueBufferShaper));

                    _queryModelVisitor.Expression = groupShapedQuery;

                    UpdateGroupQuerySourceParameter(groupShapedQuery.Type);

                    return true;
                }

                return false;
            }

            private class QuerySourceFindingExpressionVisitor : RelinqExpressionVisitor
            {
                public ICollection<IQuerySource> QuerySources { get; } = new HashSet<IQuerySource>();

                protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression querySourceReferenceExpression)
                {
                    QuerySources.Add(querySourceReferenceExpression.TryGetReferencedQuerySource());

                    return base.VisitQuerySourceReference(querySourceReferenceExpression);
                }
            }

            private void UpdateGroupQuerySourceParameter(Type parameterType)
            {
                var currentParameter = Expression.Parameter(
                    parameterType,
                    _groupQuerySource.ItemName);

                _queryModelVisitor.CurrentParameter = currentParameter;
                _queryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(
                    _groupQuerySource,
                    currentParameter);
            }

            private bool IsAggregateGroupBySelector(Expression expression)
            {
                if (expression is NewExpression newExpression)
                {
                    return newExpression.Arguments.All(e => IsAggregateSubQueryExpression(e) || IsKeySelector(e));
                }

                return IsAggregateSubQueryExpression(expression);
            }

            private bool IsAggregateSubQueryExpression(Expression expression)
            {
                if (expression is SubQueryExpression subQuery
                    && subQuery.QueryModel.BodyClauses.Count == 0
                    && subQuery.QueryModel.MainFromClause.FromExpression.TryGetReferencedQuerySource() == _groupQuerySource
                    && subQuery.QueryModel.ResultOperators.Count == 1
                    && !(subQuery.QueryModel.SelectClause.Selector is ConstantExpression)
                    && _aggregateResultOperators.Contains(subQuery.QueryModel.ResultOperators.Single().GetType()))
                {
                    return true;
                }

                return false;
            }

            private bool IsKeySelector(Expression expression)
            {
                while (expression is MemberExpression memberExpression)
                {
                    if (memberExpression.Member.Name == KeyName
                        && memberExpression.Expression.TryGetReferencedQuerySource() == _groupQuerySource)
                    {
                        return true;
                    }

                    expression = memberExpression.Expression;
                }

                return false;
            }
        }
    }
}
