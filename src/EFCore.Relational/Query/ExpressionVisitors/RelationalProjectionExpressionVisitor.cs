// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

            if (EntityQueryModelVisitor.IsPropertyMethod(methodCallExpression.Method))
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
            var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

            if (expression != null
                && !(expression is ConstantExpression)
                && !(expression is NewExpression)
                && selectExpression != null)
            {
                var existingProjectionsCount = selectExpression.Projection.Count;

                var sqlExpression
                    = _sqlTranslatingExpressionVisitorFactory
                        .Create(QueryModelVisitor, selectExpression, inProjection: true)
                        .Visit(expression);

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
                    else
                    {
                        QueryModelVisitor.RequiresClientProjection = true;
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
                            selectExpression.AddToProjection(sqlExpression);

                            _sourceExpressionProjectionMapping[expression] = sqlExpression;

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

                            _sourceExpressionProjectionMapping[expression] = sqlExpression;

                            var readValueExpression
                                = _entityMaterializerSource
                                    .CreateReadValueCallExpression(targetExpression, index);

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
    }
}
