// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class ProjectionExpressionVisitor : DefaultQueryExpressionVisitor
    {
        public ProjectionExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
        }

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            var subExpression = queryModelVisitor.Expression;

            var resultItemType
                = queryModelVisitor.StreamedSequenceInfo?.ResultItemType
                  ?? subExpression.Type;

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
    }
}
