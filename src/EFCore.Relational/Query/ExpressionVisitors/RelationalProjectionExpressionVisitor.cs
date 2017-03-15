// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated method call.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            if (IncludeCompiler.IsIncludeMethod(node))
            {
                return node;
            }

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
                    var sourceExpression = expression.Arguments[i];

                    if (_sourceExpressionProjectionMapping.ContainsKey(sourceExpression))
                    {
                        var memberInfo = expression.Members?[i]
                            ?? (expression.Arguments[i] as MemberExpression)?.Member;

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
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression to the translated input expression.
        /// </returns>
        public override Expression Visit(Expression node)
        {
            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (node != null
                && !(node is ConstantExpression)
                && selectExpression != null)
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
                        if (QueryModelVisitor.ParentQueryModelVisitor != null
                            && selectExpression.HandlesQuerySource(qsre.ReferencedQuerySource))
                        {
                            selectExpression.ProjectStarTable = selectExpression.GetTableForQuerySource(qsre.ReferencedQuerySource);
                        }
                    }
                }
                else
                {
                    selectExpression.RemoveRangeFromProjection(existingProjectionsCount);

                    if (!(node is NewExpression))
                    {
                        if (!(node is QuerySourceReferenceExpression))
                        {
                            if (sqlExpression is NullableExpression nullableExpression)
                            {
                                sqlExpression = nullableExpression.Operand;
                            }

                            if (sqlExpression is ColumnExpression)
                            {
                                selectExpression.AddToProjection(sqlExpression);

                                _sourceExpressionProjectionMapping[node] = sqlExpression;

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
                                var index = selectExpression.AddToProjection(sqlExpression);

                                _sourceExpressionProjectionMapping[node] = sqlExpression;

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
