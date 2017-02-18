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

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalProjectionExpressionVisitor" />.
        /// </summary>
        /// <param name="sqlTranslatingExpressionVisitorFactory"> The SQL translating expression visitor factory. </param>
        /// <param name="entityMaterializerSource"> The entity materializer source. </param>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="querySource"> The query source. </param>
        public RelationalProjectionExpressionVisitor(
            [NotNull] ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)))
        {
            Check.NotNull(sqlTranslatingExpressionVisitorFactory, nameof(sqlTranslatingExpressionVisitorFactory));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(querySource, nameof(querySource));

            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _entityMaterializerSource = entityMaterializerSource;
            _querySource = querySource;
        }

        private new RelationalQueryModelVisitor QueryModelVisitor
            => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        /// <summary>
        ///     Visit a method call expression.
        /// </summary>
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated method call.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            var sqlTranslator
                = _sqlTranslatingExpressionVisitorFactory.Create(
                    queryModelVisitor: QueryModelVisitor,
                    topLevelPredicate: null,
                    mutateProjections: false,
                    inProjection: true);

            var sqlExpression = sqlTranslator.Visit(node);

            if (EntityQueryModelVisitor.IsPropertyMethod(node.Method))
            {
                var referencedQuerySource 
                    = (node.Arguments[0].RemoveConvert() as QuerySourceReferenceExpression)
                        ?.ReferencedQuerySource;

                if (referencedQuerySource != null)
                {
                    var targetExpression
                        = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                            .GetExpression(referencedQuerySource);

                    var selectExpression = QueryModelVisitor.TryGetQuery(referencedQuerySource);

                    if (selectExpression != null && sqlExpression != null)
                    {
                        if (targetExpression.Type == typeof(ValueBuffer))
                        {
                            return TryReadFromValueBuffer(node, targetExpression, selectExpression, sqlExpression);
                        }
                        else
                        {
                            return node;
                        }
                    }
                }

                var newArg0 = Visit(node.Arguments[0]);

                if (newArg0 != node.Arguments[0])
                {
                    return Expression.Call(
                        node.Method,
                        newArg0,
                        node.Arguments[1]);
                }

                return node;
            }

            if (sqlExpression != null)
            {
                var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

                var targetExpression
                        = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                            .GetExpression(_querySource);

                if (targetExpression.Type == typeof(ValueBuffer))
                {
                    return TryReadFromValueBuffer(node, targetExpression, selectExpression, sqlExpression);
                }
                
                return node;
            }

            QueryModelVisitor.RequiresClientProjection = true;

            return base.VisitMethodCall(node);
        }

        /// <summary>
        ///     Visit a member expression.
        /// </summary>
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated member.
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            Check.NotNull(node, nameof(node));

            var sqlTranslator
                = _sqlTranslatingExpressionVisitorFactory.Create(
                    queryModelVisitor: QueryModelVisitor,
                    topLevelPredicate: null,
                    mutateProjections: false,
                    inProjection: true);

            var sqlExpression = sqlTranslator.Visit(node);

            var groupingKeyExpression = TryHandleGroupingKeyExpression(node, sqlExpression);

            if (groupingKeyExpression != null)
            {
                return groupingKeyExpression;
            }

            var referencedQuerySource = (node.Expression as QuerySourceReferenceExpression)?.ReferencedQuerySource;

            if (referencedQuerySource != null)
            {
                var targetExpression
                    = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                        .GetExpression(referencedQuerySource);

                var selectExpression = QueryModelVisitor.TryGetQuery(referencedQuerySource);

                if (selectExpression != null && sqlExpression != null)
                {
                    if (targetExpression.Type == typeof(ValueBuffer))
                    {
                        return TryReadFromValueBuffer(node, targetExpression, selectExpression, sqlExpression);
                    }
                    else
                    {
                        return node;
                    }
                }
            }

            var subQueryExpression = node.Expression as SubQueryExpression;

            if (subQueryExpression != null)
            {
                if (sqlExpression != null)
                {
                    var targetExpression
                        = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                            .GetExpression(_querySource);

                    var selectExpression 
                        = QueryModelVisitor.TryGetQuery(_querySource);
                    
                    return TryReadFromValueBuffer(node, targetExpression, selectExpression, sqlExpression);
                }

                var subQueryModelVisitor 
                    = QueryModelVisitor.QueryCompilationContext
                        .GetQueryModelVisitor(subQueryExpression.QueryModel);

                var entityType
                    = QueryModelVisitor.QueryCompilationContext.Model
                        .FindEntityType(subQueryExpression.Type);

                var property
                    = entityType?.FindProperty(node.Member.Name);

                var selectorQuerySource
                    = (subQueryExpression.QueryModel.SelectClause.Selector
                        as QuerySourceReferenceExpression)?.ReferencedQuerySource;

                if (subQueryModelVisitor != null && property != null && selectorQuerySource != null)
                {
                    var selectExpression = subQueryModelVisitor.TryGetQuery(selectorQuerySource);

                    var projectionIndex = selectExpression.GetProjectionIndex(property, selectorQuerySource);

                    return _entityMaterializerSource.CreateReadValueExpression(
                        subQueryModelVisitor.Expression,
                        property.ClrType,
                        projectionIndex);
                }
            }

            if (sqlExpression != null)
            {
                var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

                var targetExpression
                        = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                            .GetExpression(_querySource);

                if (targetExpression.Type == typeof(ValueBuffer))
                {
                    return TryReadFromValueBuffer(node, targetExpression, selectExpression, sqlExpression);
                }
                
                return node;
            }

            QueryModelVisitor.RequiresClientProjection = true;

            return base.VisitMember(node);
        }

        /// <summary>
        ///     Visit a new expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated new expression.
        /// </returns>
        protected override Expression VisitNew(NewExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            foreach (var qsre in expression.Arguments.OfType<QuerySourceReferenceExpression>())
            {
                var entityType 
                    = QueryModelVisitor.QueryCompilationContext.Model
                        .FindEntityType(qsre.ReferencedQuerySource.ItemType);

                if (entityType != null)
                {
                    // TODO: Figure out how to make use of a composite/projection shaper
                    QueryModelVisitor.RequiresClientProjection = true;
                    break;
                }
            }

            var newNewExpression = (NewExpression)base.VisitNew(expression);

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression?.Projection.Count > 0)
            {
                for (var i = 0; i < expression.Arguments.Count; i++)
                {
                    var aliasExpression
                        = selectExpression.Projection
                            .OfType<AliasExpression>()
                            .SingleOrDefault(ae => ae.SourceExpression == expression.Arguments[i]);

                    if (aliasExpression != null)
                    {
                        aliasExpression.SourceMember
                            = expression.Members?[i]
                              ?? (expression.Arguments[i] as MemberExpression)?.Member;
                    }
                }
            }

            return newNewExpression;
        }
        
        /// <summary>
        ///     Visits the given node.
        /// </summary>
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression to the translated input expression.
        /// </returns>
        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            if (node is ConstantExpression 
                || node is NewExpression 
                || node is MemberExpression
                || node is MethodCallExpression)
            {
                return base.Visit(node);
            }

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression == null)
            {
                return base.Visit(node);
            }

            var sqlTranslator
                = _sqlTranslatingExpressionVisitorFactory.Create(
                    queryModelVisitor: QueryModelVisitor,
                    topLevelPredicate: null,
                    mutateProjections: false,
                    inProjection: true);

            var sqlExpression = sqlTranslator.Visit(node);

            if (sqlExpression == null)
            {
                var referencedQuerySource = (node as QuerySourceReferenceExpression)?.ReferencedQuerySource;

                if (referencedQuerySource != null && QueryModelVisitor.ParentQueryModelVisitor != null)
                {
                    selectExpression.ProjectStarAlias = selectExpression.GetTableForQuerySource(referencedQuerySource).Alias;
                }

                return base.Visit(node);
            }

            if (!(node is QuerySourceReferenceExpression))
            {
                if (sqlExpression.TryGetColumnExpression() != null)
                {
                    var index = selectExpression.AddToProjection(sqlExpression);

                    var aliasExpression = selectExpression.Projection[index] as AliasExpression;

                    if (aliasExpression != null)
                    {
                        aliasExpression.SourceExpression = node;
                    }

                    var conditionalExpression = node as ConditionalExpression;

                    if (conditionalExpression != null)
                    {
                        // If we got here it's because a null check was removed.
                        var ifTrueConstant = conditionalExpression.IfTrue as ConstantExpression;

                        if (ifTrueConstant != null && ifTrueConstant.Value == null)
                        {
                            return conditionalExpression.IfFalse;
                        }
                        else
                        {
                            return conditionalExpression.IfTrue;
                        }
                    }

                    return node;
                }
            }

            if (!(sqlExpression is CompositeExpression))
            {
                var targetExpression
                    = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                        .GetExpression(_querySource);

                if (targetExpression.Type == typeof(ValueBuffer))
                {
                    return TryReadFromValueBuffer(node, targetExpression, selectExpression, sqlExpression);
                }
                else if (targetExpression.Type == typeof(IGrouping<ValueBuffer, ValueBuffer>))
                {
                    targetExpression = GroupingShaper.CreateValueBufferAccessExpression(targetExpression);

                    return TryReadFromValueBuffer(node, targetExpression, selectExpression, sqlExpression);
                }

                // Why not fall through?
                return node;
            }

            return base.Visit(node);
        }

        /// <summary>
        ///     Visit a subquery expression.
        /// </summary>
        /// <param name="expression"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated subquery expression.
        /// </returns>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var compilationContext = QueryModelVisitor.QueryCompilationContext;

            var subQueryModelVisitor = compilationContext.GetQueryModelVisitor(expression.QueryModel);

            if (subQueryModelVisitor == null)
            {
                subQueryModelVisitor
                    = compilationContext.CreateQueryModelVisitor(
                        expression.QueryModel,
                        QueryModelVisitor);

                // This override only exists to set this here flag
                subQueryModelVisitor.RequiresOuterParameterInjection = true;

                subQueryModelVisitor.VisitQueryModel(expression.QueryModel);
            }

            return base.VisitSubQuery(expression);
        }

        private Expression TryReadFromValueBuffer(
            Expression node,
            Expression targetExpression,
            SelectExpression selectExpression,
            Expression sqlExpression)
        {
            var index = selectExpression.AddToProjection(sqlExpression.MaybeAnonymousSubquery());

            var aliasExpression = selectExpression.Projection[index] as AliasExpression;

            if (aliasExpression != null)
            {
                aliasExpression.SourceExpression = node;
            }

            Expression readValueExpression;

            var outputDataInfo
                = (node as SubQueryExpression)?.QueryModel
                    .GetOutputDataInfo();

            if (outputDataInfo is StreamedScalarValueInfo)
            {
                // Compensate for possible nulls
                readValueExpression
                    = Expression.Coalesce(
                        _entityMaterializerSource
                            .CreateReadValueCallExpression(targetExpression, index),
                        Expression.Default(node.Type));
            }
            else
            {
                readValueExpression
                   = _entityMaterializerSource
                       .CreateReadValueExpression(targetExpression, node.Type, index);
            }

            return Expression.Convert(readValueExpression, node.Type);
        }

        private Expression TryHandleGroupingKeyExpression(MemberExpression node, Expression sqlExpression)
        {
            if (sqlExpression == null)
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

            if (!root.Type.IsGrouping())
            {
                return null;
            }

            var targetExpression
                = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                    .GetExpression(_querySource);

            var groupingType = typeof(IGrouping<ValueBuffer, ValueBuffer>);

            if (targetExpression.Type != groupingType)
            {
                return null;
            }

            if (path.Last().Name != "Key")
            {
                return null;
            }

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression == null)
            {
                return null;
            }

            var compositeExpression = sqlExpression as CompositeExpression;

            if (compositeExpression == null)
            {
                return null;
            }

            var keyProperty = groupingType.GetTypeInfo().GetDeclaredProperty("Key");
            var keyAccessExpression = Expression.MakeMemberAccess(targetExpression, keyProperty);

            if (compositeExpression.Expressions.Count == 1)
            {
                return TryReadFromValueBuffer(
                    top,
                    keyAccessExpression,
                    selectExpression,
                    compositeExpression.Expressions[0]);
            }

            var groupSubQuery
                = ((root as QuerySourceReferenceExpression)
                    ?.ReferencedQuerySource as FromClauseBase)
                        ?.FromExpression as SubQueryExpression;

            var groupResultOperator
                = groupSubQuery?.QueryModel.ResultOperators
                    .LastOrDefault() as GroupResultOperator;

            if (groupResultOperator == null)
            {
                return null;
            }

            QueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(
                ((QuerySourceReferenceExpression)groupSubQuery.QueryModel.SelectClause.Selector).ReferencedQuerySource,
                keyAccessExpression);

            var keySelector = Visit(groupResultOperator.KeySelector);

            if (path.Count == 1)
            {
                return keySelector;
            }

            var newExpression = keySelector as NewExpression;

            if (newExpression == null)
            {
                return null;
            }

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

            return null;
        }
    }
}
