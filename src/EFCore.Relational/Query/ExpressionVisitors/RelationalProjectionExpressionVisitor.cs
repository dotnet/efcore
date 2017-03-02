// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
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
                = TryHandleMemberOrMethodCallExpression(
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

            var visitedNewExpression = base.VisitNew(newExpression);

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression != null)
            {
                for (var i = 0; i < newExpression.Arguments.Count; i++)
                {
                    var aliasExpression
                        = selectExpression.Projection
                            .OfType<AliasExpression>()
                            .SingleOrDefault(ae => ae.SourceExpression == newExpression.Arguments[i]);

                    if (aliasExpression != null)
                    {
                        aliasExpression.SourceMember
                            = newExpression.Members?[i]
                              ?? (newExpression.Arguments[i] as MemberExpression)?.Member;
                    }
                }
            }

            return visitedNewExpression;
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

            var sqlExpression = TranslateExpression(node);

            if (sqlExpression == null)
            {
                if (node is QuerySourceReferenceExpression qsre)
                {
                    if (QueryModelVisitor.ParentQueryModelVisitor != null)
                    {
                        selectExpression.ProjectStarTable
                            = selectExpression.GetTableForQuerySource(qsre.ReferencedQuerySource);
                    }
                }
                else
                {
                    QueryModelVisitor.RequiresClientProjection = true;
                }

                return base.Visit(node);
            }

            if (!(node is QuerySourceReferenceExpression))
            {
                if (sqlExpression is NullableExpression nullableExpression)
                {
                    sqlExpression = nullableExpression.Operand;
                }

                if (sqlExpression.TryGetColumnExpression() != null)
                {
                    var index = selectExpression.AddToProjection(sqlExpression);

                    if (selectExpression.Projection[index] is AliasExpression aliasExpression)
                    {
                        aliasExpression.SourceExpression = node;
                    }

                    return node;
                }
            }

            if (!(sqlExpression is ConstantExpression))
            {
                var targetExpression
                    = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                        .GetExpression(_querySource);

                return targetExpression.Type == typeof(ValueBuffer)
                    ? CreateReadValueExpression(node, targetExpression, selectExpression, sqlExpression)
                    : node;
            }

            return base.Visit(node);
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
            Expression node,
            Expression targetExpression,
            SelectExpression selectExpression,
            Expression sqlExpression)
        {
            var index = selectExpression.AddToProjection(sqlExpression);

            if (selectExpression.Projection[index] is AliasExpression aliasExpression)
            {
                aliasExpression.SourceExpression = node;
            }

            if (node is SubQueryExpression subQueryExpression
                && subQueryExpression.QueryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
            {
                // Compensate for possible nulls
                return Expression.Convert(
                    Expression.Coalesce(
                        _entityMaterializerSource.CreateReadValueCallExpression(targetExpression, index),
                        Expression.Default(node.Type)),
                    node.Type);
            }
            else
            {
                return _entityMaterializerSource.CreateReadValueExpression(targetExpression, node.Type, index);
            }
        }
    }
}
