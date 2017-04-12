// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        }

        private new RelationalQueryModelVisitor QueryModelVisitor
            => (RelationalQueryModelVisitor)base.QueryModelVisitor;
        
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

            var sqlExpression = TranslateExpression(methodCallExpression);

            var objectExpression
                = EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method)
                    ? methodCallExpression.Arguments[0].RemoveConvert()
                    : methodCallExpression.Object;

            var handledExpression
                = TryHandleMemberOrMethodCallExpression(
                    methodCallExpression,
                    objectExpression,
                    sqlExpression);

            if (handledExpression != null)
            {
                return handledExpression;
            }

            QueryModelVisitor.RequiresClientProjection = true;

            return methodCallExpression;
        }

        /// <summary>
        ///     Visit a member expression.
        /// </summary>
        /// <param name="memberExpression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated member.
        /// </returns>
        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            var sqlExpression = TranslateExpression(memberExpression);

            var handledExpression
                = TryHandleGroupingKeyExpression(
                    memberExpression,
                    sqlExpression)
                ?? TryHandleMemberOrMethodCallExpression(
                    memberExpression,
                    memberExpression.Expression.RemoveConvert(),
                    sqlExpression);

            if (handledExpression != null)
            {
                return handledExpression;
            }

            QueryModelVisitor.RequiresClientProjection = true;

            return memberExpression;
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

            if (newExpression.Type == typeof(AnonymousObject))
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

                    if (_sourceExpressionProjectionMapping.ContainsKey(sourceExpression))
                    {
                        var memberInfo = newExpression.Members?[i]
                            ?? (newExpression.Arguments[i] as MemberExpression)?.Member;

                        if (memberInfo != null)
                        {
                            selectExpression.SetProjectionForMemberInfo(
                                memberInfo,
                                _sourceExpressionProjectionMapping[sourceExpression]);
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
                return null;
            }

            if (expression is ConstantExpression
                || expression is NewExpression
                || expression is MemberExpression
                || expression is MethodCallExpression)
            {
                return base.Visit(expression);
            }

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression == null)
            {
                return base.Visit(expression);
            }

            var previousProjectionCount = selectExpression.Projection.Count;
            var sqlExpression = TranslateExpression(expression);

            if (sqlExpression == null)
            {
                if (expression is QuerySourceReferenceExpression qsre)
                {
                    if (QueryModelVisitor.ParentQueryModelVisitor != null
                        && selectExpression.HandlesQuerySource(qsre.ReferencedQuerySource))
                    {
                        selectExpression.ProjectStarTable = selectExpression.GetTableForQuerySource(qsre.ReferencedQuerySource);
                    }
                }
            }
            else
            {
                selectExpression.RemoveRangeFromProjection(previousProjectionCount);

                var targetExpression
                    = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                        .GetExpression(_querySource);

                if (targetExpression.Type == typeof(ValueBuffer))
                {
                    return CreateReadValueExpression(expression, targetExpression, selectExpression, sqlExpression);
                }
                else if (targetExpression.Type == _valueBufferGroupingType)
                {
                    targetExpression = GroupingShaper.CreateValueBufferAccessExpression(targetExpression);

                    return CreateReadValueExpression(expression, targetExpression, selectExpression, sqlExpression);
                }
            }

            return base.Visit(expression);
        }

        private Expression TranslateExpression(Expression expression)
            => _sqlTranslatingExpressionVisitorFactory
                .Create(QueryModelVisitor, inProjection: true)
                .Visit(expression);

        private Expression TryHandleMemberOrMethodCallExpression(
            Expression node, Expression objectExpression, Expression sqlExpression)
        {
            if (sqlExpression == null)
            {
                return null;
            }

            Expression targetExpression = null;
            SelectExpression targetQuery = null;

            if (objectExpression is QuerySourceReferenceExpression qsre)
            {
                targetExpression
                    = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                        .GetExpression(qsre.ReferencedQuerySource);

                targetQuery
                    = QueryModelVisitor.TryGetQuery(qsre.ReferencedQuerySource);
            }

            if (targetQuery == null)
            {
                targetExpression
                    = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                        .GetExpression(_querySource);

                targetQuery
                    = QueryModelVisitor.TryGetQuery(_querySource);
            }

            return targetQuery != null && targetExpression.Type == typeof(ValueBuffer)
                ? CreateReadValueExpression(node, targetExpression, targetQuery, sqlExpression)
                : node;
        }

        private Expression CreateReadValueExpression(
            Expression expression,
            Expression targetExpression,
            SelectExpression selectExpression,
            Expression sqlExpression)
        {
            if (sqlExpression is NullableExpression nullableExpression)
            {
                sqlExpression = nullableExpression.Operand;
            }

            _sourceExpressionProjectionMapping[expression] = sqlExpression;

            var index = selectExpression.AddToProjection(sqlExpression);

            if (expression is SubQueryExpression subQueryExpression
                && subQueryExpression.QueryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                // Compensate for possible nulls
                return Expression.Convert(
                    Expression.Coalesce(
                        _entityMaterializerSource.CreateReadValueCallExpression(targetExpression, index),
                        Expression.Default(expression.Type)),
                    expression.Type);
            }

            return _entityMaterializerSource.CreateReadValueExpression(
                targetExpression, 
                expression.Type, 
                index, 
                (sqlExpression as ColumnExpression)?.Property);
        }

        private Expression TryHandleGroupingKeyExpression(MemberExpression node, Expression sqlExpression)
        {
            if (!(sqlExpression is CompositeExpression compositeExpression))
            {
                return null;
            }

            var top = node;
            var path = new List<MemberInfo>();
            var root = (Expression)null;

            while (node != null)
            {
                path.Add(node.Member);
                root = node.Expression;
                node = root as MemberExpression;
            }

            if (!root.Type.IsGrouping() || path.Last().Name != "Key")
            {
                return null;
            }

            var targetExpression
                = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                    .GetExpression(_querySource);

            if (targetExpression.Type != _valueBufferGroupingType)
            {
                return null;
            }

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression == null)
            {
                return null;
            }
            
            var keyAccessExpression 
                = Expression.MakeMemberAccess(
                    targetExpression, 
                    _valueBufferGroupingKeyProperty);

            if (compositeExpression.Expressions.Count == 1)
            {
                var keySqlExpression 
                    = selectExpression.PushDownColumnReferences(
                        compositeExpression.Expressions[0], 
                        _querySource);

                return CreateReadValueExpression(
                    top,
                    keyAccessExpression,
                    selectExpression,
                    keySqlExpression);
            }

            var groupSubQuery
                = (root.TryGetQuerySource() as FromClauseBase)
                    ?.FromExpression as SubQueryExpression;

            var groupResultOperator
                = groupSubQuery?.QueryModel.ResultOperators
                    .LastOrDefault() as GroupResultOperator;

            if (groupResultOperator == null)
            {
                return null;
            }

            QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(
                groupSubQuery.TryGetSelectorQuerySource(),
                keyAccessExpression);

            var keySelector = Visit(groupResultOperator.KeySelector);

            if (path.Count == 1)
            {
                return keySelector;
            }

            if (keySelector is NewExpression newExpression)
            {
                foreach (var member in path.AsEnumerable().Reverse().Skip(1))
                {
                    var argumentIndex = newExpression.Members.IndexOf(member);
                    var argumentExpression = newExpression.Arguments[argumentIndex];

                    newExpression = argumentExpression as NewExpression;

                    if (newExpression == null)
                    {
                        return argumentExpression;
                    }
                }
            }

            return null;
        }

        private static readonly Type _valueBufferGroupingType
            = typeof(IGrouping<ValueBuffer, ValueBuffer>);

        private static readonly PropertyInfo _valueBufferGroupingKeyProperty
            = _valueBufferGroupingType.GetTypeInfo().GetDeclaredProperty("Key");
    }
}
