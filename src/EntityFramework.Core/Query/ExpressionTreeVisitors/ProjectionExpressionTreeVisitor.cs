// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionTreeVisitors
{
    public class ProjectionExpressionTreeVisitor : DefaultQueryExpressionTreeVisitor
    {
        public ProjectionExpressionTreeVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IQuerySource outerQuerySource)
            : base(
                Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)),
                Check.NotNull(outerQuerySource, nameof(outerQuerySource)))
        {
        }

        protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
        {
            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            var subExpression = queryModelVisitor.Expression;

            var resultItemType
                = queryModelVisitor.StreamedSequenceInfo?.ResultItemType
                  ?? subExpression.Type;

            var resultItemTypeInfo = resultItemType.GetTypeInfo();

            if (resultItemTypeInfo.IsGenericType
                && resultItemTypeInfo.GetGenericTypeDefinition() == typeof(QuerySourceScope<>))
            {
                resultItemType = resultItemTypeInfo.GenericTypeArguments[0];

                subExpression
                    = Expression.Call(
                        (queryModelVisitor.StreamedSequenceInfo != null
                            ? QueryModelVisitor.QueryCompilationContext.LinqOperatorProvider
                                .UnwrapQueryResults
                            : _unwrapSingleResult)
                            .MakeGenericMethod(resultItemType),
                        subExpression);
            }

            if (queryModelVisitor.StreamedSequenceInfo == null)
            {
                return subExpression;
            }

            if (subExpression.Type != subQueryExpression.Type)
            {
                var subQueryExpressionTypeInfo = subQueryExpression.Type.GetTypeInfo();

                if (typeof(IQueryable).GetTypeInfo().IsAssignableFrom(subQueryExpressionTypeInfo))
                {
                    subExpression
                        = Expression.Call(
                            QueryModelVisitor.LinqOperatorProvider.ToQueryable
                                .MakeGenericMethod(resultItemType),
                            subExpression);
                }
                else if (subQueryExpressionTypeInfo.IsGenericType)
                {
                    var genericTypeDefinition = subQueryExpressionTypeInfo.GetGenericTypeDefinition();

                    if (genericTypeDefinition == typeof(IOrderedEnumerable<>))
                    {
                        subExpression
                            = Expression.Call(
                                QueryModelVisitor.LinqOperatorProvider.ToOrdered
                                    .MakeGenericMethod(resultItemType),
                                subExpression);
                    }
                    else if (genericTypeDefinition == typeof(IEnumerable<>))
                    {
                        subExpression
                            = Expression.Call(
                                QueryModelVisitor.LinqOperatorProvider.ToEnumerable
                                    .MakeGenericMethod(resultItemType),
                                subExpression);
                    }
                }
            }

            return subExpression;
        }

        private static readonly MethodInfo _unwrapSingleResult
            = typeof(ProjectionExpressionTreeVisitor)
                .GetTypeInfo().GetDeclaredMethod("UnwrapSingleResult");

        [UsedImplicitly]
        private static TResult UnwrapSingleResult<TResult>(
            QuerySourceScope<TResult> querySourceScope)
            where TResult : class
        {
            // ReSharper disable once MergeConditionalExpression
            return querySourceScope != null ? querySourceScope.Result : default(TResult);
        }
    }
}
