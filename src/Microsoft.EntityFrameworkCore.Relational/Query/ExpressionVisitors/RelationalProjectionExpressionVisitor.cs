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

            if (EntityQueryModelVisitor.IsPropertyMethod(node.Method))
            {
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

            return base.VisitMethodCall(node);
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

            var newNewExpression = base.VisitNew(expression);

            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (selectExpression != null)
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
            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if ((node != null)
                && !(node is ConstantExpression)
                && (selectExpression != null))
            {
                var existingProjectionsCount = selectExpression.Projection.Count;

                var sqlExpression
                    = _sqlTranslatingExpressionVisitorFactory
                        .Create(QueryModelVisitor, selectExpression, inProjection: true)
                        .Visit(node);

                if (sqlExpression == null)
                {
                    var qsre = node as QuerySourceReferenceExpression;
                    if (qsre == null)
                    {
                        QueryModelVisitor.RequiresClientProjection = true;
                    }
                    else
                    {
                        if (QueryModelVisitor.ParentQueryModelVisitor != null)
                        {
                            selectExpression.ProjectStarAlias = selectExpression.GetTableForQuerySource(qsre.ReferencedQuerySource).Alias;
                        }
                    }
                }
                else
                {
                    selectExpression.RemoveRangeFromProjection(existingProjectionsCount);

                    if (!(node is NewExpression))
                    {
                        AliasExpression aliasExpression;

                        int index;

                        if (!(node is QuerySourceReferenceExpression))
                        {
                            var columnExpression = sqlExpression.TryGetColumnExpression();

                            if (columnExpression != null)
                            {
                                index = selectExpression.AddToProjection(sqlExpression);

                                aliasExpression = selectExpression.Projection[index] as AliasExpression;

                                if (aliasExpression != null)
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

                            if (targetExpression.Type == typeof(ValueBuffer))
                            {
                                index = selectExpression.AddToProjection(sqlExpression);

                                aliasExpression = selectExpression.Projection[index] as AliasExpression;

                                if (aliasExpression != null)
                                {
                                    aliasExpression.SourceExpression = node;
                                }

                                var readValueExpression
                                    = _entityMaterializerSource
                                        .CreateReadValueCallExpression(targetExpression, index);

                                var outputDataInfo
                                    = (node as SubQueryExpression)?.QueryModel
                                        .GetOutputDataInfo();

                                if (outputDataInfo is StreamedScalarValueInfo)
                                {
                                    // Compensate for possible nulls
                                    readValueExpression
                                        = Expression.Coalesce(
                                            readValueExpression,
                                            Expression.Default(node.Type));
                                }

                                return Expression.Convert(readValueExpression, node.Type);
                            }

                            return node;
                        }
                    }
                }
            }

            return base.Visit(node);
        }
    }
}
