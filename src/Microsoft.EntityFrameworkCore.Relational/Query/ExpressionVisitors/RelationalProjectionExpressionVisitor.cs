// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    public class RelationalProjectionExpressionVisitor : ProjectionExpressionVisitor
    {
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IQuerySource _querySource;

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

        public override Expression Visit(Expression node)
        {
            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if ((node != null)
                && !(node is ConstantExpression)
                && (selectExpression != null))
            {
                var sqlExpression
                    = _sqlTranslatingExpressionVisitorFactory
                        .Create(QueryModelVisitor, selectExpression, inProjection: true)
                        .Visit(node);

                if (sqlExpression == null)
                {
                    if (!(node is QuerySourceReferenceExpression))
                    {
                        QueryModelVisitor.RequiresClientProjection = true;
                    }
                }
                else
                {
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
                            index = selectExpression.AddToProjection(sqlExpression);

                            aliasExpression = selectExpression.Projection[index] as AliasExpression;

                            if (aliasExpression != null)
                            {
                                aliasExpression.SourceExpression = node;
                            }

                            var targetExpression
                                = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                                    .GetExpression(_querySource);

                            if (targetExpression.Type == typeof(ValueBuffer))
                            {
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
                            else
                            {
                                return node;
                            }
                        }
                    }
                }
            }

            return base.Visit(node);
        }
    }
}
