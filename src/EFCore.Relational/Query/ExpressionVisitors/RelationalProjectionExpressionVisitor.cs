// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
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
        private readonly SelectExpression _groupAggregateTargetSelectExpression;
        private readonly SelectExpression _targetSelectExpression;
        private bool _isGroupAggregate;

        private readonly Dictionary<Expression, Expression> _sourceExpressionProjectionMapping
            = new Dictionary<Expression, Expression>();

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
            _targetSelectExpression = QueryModelVisitor.TryGetQuery(querySource);

            if (_targetSelectExpression != null)
            {
                _groupAggregateTargetSelectExpression = _targetSelectExpression.Clone();

                _isGroupAggregate = _querySource.ItemType.IsGrouping() && _targetSelectExpression.GroupBy.Count > 0;
                if (_isGroupAggregate)
                {
                    _targetSelectExpression.ClearProjection();
                }
            }
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

            if (_targetSelectExpression != null)
            {
                foreach (var sourceBinding in memberInitExpression.Bindings)
                {
                    if (sourceBinding is MemberAssignment memberAssignment)
                    {
                        var sourceExpression = memberAssignment.Expression;

                        if (_sourceExpressionProjectionMapping.TryGetValue(sourceExpression, out var sqlExpression))
                        {
                            var memberInfo = memberAssignment.Member;

                            _targetSelectExpression.SetProjectionForMemberInfo(
                                memberInfo,
                                sqlExpression);
                        }
                    }
                }
            }

            return newMemberInitExpression;
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

                var newProperyCallExpressions = new List<Expression>();
                foreach (var propertyCallExpression in propertyCallExpressions)
                {
                    newProperyCallExpressions.Add(Visit(propertyCallExpression.RemoveConvert()));
                }

                return Expression.New(
                    newExpression.Type == typeof(AnonymousObject)
                        ? AnonymousObject.AnonymousObjectCtor
                        : MaterializedAnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        newProperyCallExpressions.Select(e => Expression.Convert(e, typeof(object)))));
            }

            var newNewExpression = base.VisitNew(newExpression);

            if (_targetSelectExpression != null)
            {
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var sourceExpression = newExpression.Arguments[i];

                    if (_sourceExpressionProjectionMapping.TryGetValue(sourceExpression, out var sqlExpression))
                    {
                        var memberInfo = newExpression.Members?[i];

                        if (memberInfo != null)
                        {
                            _targetSelectExpression.SetProjectionForMemberInfo(
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
            if (expression == null)
            {
                return expression;
            }

            // Skip over Include and Correlated Collection methods
            // This is checked first because it should not call base when there is not _targetSelectExpression
            if (expression is MethodCallExpression methodCallExpression
                && (IncludeCompiler.IsIncludeMethod(methodCallExpression)
                    || CorrelatedCollectionOptimizingVisitor.IsCorrelatedCollectionMethod(methodCallExpression)))
            {
                return expression;
            }

            if (_targetSelectExpression == null)
            {
                return base.Visit(expression);
            }

            switch (expression)
            {
                // To save mappings so that we can compose afterwards
                case NewExpression newExpression:
                    return VisitNew(newExpression);

                case MemberInitExpression memberInitExpression:
                    return VisitMemberInit(memberInitExpression);

                case QuerySourceReferenceExpression qsre:
                    if (_targetSelectExpression.HandlesQuerySource(qsre.ReferencedQuerySource))
                    {
                        _targetSelectExpression.ProjectStarTable
                            = _targetSelectExpression.GetTableForQuerySource(qsre.ReferencedQuerySource);
                    }

                    return qsre;

                // Group By key translation to cover composite key cases
                case MemberExpression memberExpression
                    when memberExpression.Expression.TryGetReferencedQuerySource() == _querySource
                         && _querySource.ItemType.IsGrouping()
                         && memberExpression.Member.Name == nameof(IGrouping<int, int>.Key)
                         && QueryModelVisitor.IsShapedQueryExpression(QueryModelVisitor.Expression):

                    var groupResultOperator
                        = (GroupResultOperator)((SubQueryExpression)((FromClauseBase)_querySource).FromExpression)
                        .QueryModel.ResultOperators.Last();

                    var sqlTranslation
                        = _sqlTranslatingExpressionVisitorFactory
                            .Create(
                                QueryModelVisitor,
                                _isGroupAggregate ? _groupAggregateTargetSelectExpression : _targetSelectExpression,
                                inProjection: true)
                            .Visit(expression);

                    if (sqlTranslation == null)
                    {
                        // If the key is composite then we need to visit actual keySelector to construct the type.
                        // Since we are mapping translating actual KeySelector now, we need to re-map QuerySources
                        var querySourceFinder = new QuerySourceFindingExpressionVisitor();
                        querySourceFinder.Visit(groupResultOperator.KeySelector);

                        foreach (var querySource in querySourceFinder.QuerySources)
                        {
                            QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(
                                querySource,
                                QueryModelVisitor.CurrentParameter);
                        }

                        _isGroupAggregate = false;
                        var translatedKey = Visit(groupResultOperator.KeySelector);
                        _isGroupAggregate = true;

                        return translatedKey;
                    }

                    break;
            }

            // Fallback
            var sqlExpression
                = _sqlTranslatingExpressionVisitorFactory
                    .Create(
                        QueryModelVisitor,
                        _isGroupAggregate ? _groupAggregateTargetSelectExpression : _targetSelectExpression,
                        inProjection: true)
                    .Visit(expression);

            if (sqlExpression == null)
            {
                QueryModelVisitor.RequiresClientProjection = true;

                return base.Visit(expression);
            }

            if (sqlExpression is ConstantExpression
                && QueryModelVisitor.ParentQueryModelVisitor == null)
            {
                return base.Visit(expression);
            }

            sqlExpression = sqlExpression.UnwrapNullableExpression();

            // We bind with ValueBuffer in GroupByAggregate case straight away
            // Since the expression can be some translation from [g].[Key] which won't bind with MemberAccessBindingEV
            if (!_isGroupAggregate
                && sqlExpression is ColumnExpression)
            {
                var index = _targetSelectExpression.AddToProjection(sqlExpression);

                _sourceExpressionProjectionMapping[expression] = _targetSelectExpression.Projection[index];

                return expression;
            }

            var targetExpression
                = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                    .GetExpression(_querySource);

            if (targetExpression.Type == typeof(ValueBuffer))
            {
                var index = _targetSelectExpression.AddToProjection(sqlExpression);

                _sourceExpressionProjectionMapping[expression] = _targetSelectExpression.Projection[index];

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

        private class QuerySourceFindingExpressionVisitor : RelinqExpressionVisitor
        {
            public ICollection<IQuerySource> QuerySources { get; } = new HashSet<IQuerySource>();

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                QuerySources.Add(querySourceReferenceExpression.TryGetReferencedQuerySource());

                return base.VisitQuerySourceReference(querySourceReferenceExpression);
            }
        }
    }
}
