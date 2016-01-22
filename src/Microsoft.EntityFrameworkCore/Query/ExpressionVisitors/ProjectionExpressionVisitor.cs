// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionVisitors.Internal;
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

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var queryModelVisitor = CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(expression.QueryModel);

            var subExpression = queryModelVisitor.Expression;

            if (subExpression.Type != expression.Type)
            {
                var subQueryExpressionTypeInfo = expression.Type.GetTypeInfo();

                if (typeof(IQueryable).GetTypeInfo().IsAssignableFrom(expression.Type.GetTypeInfo()))
                {
                    subExpression
                        = Expression.Call(
                            QueryModelVisitor.LinqOperatorProvider.ToQueryable
                                .MakeGenericMethod(expression.Type.GetSequenceType()),
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
                                    .MakeGenericMethod(expression.Type.GetSequenceType()),
                                subExpression);
                    }
                    else if (genericTypeDefinition == typeof(IEnumerable<>))
                    {
                        subExpression
                            = Expression.Call(
                                QueryModelVisitor.LinqOperatorProvider.ToEnumerable
                                    .MakeGenericMethod(expression.Type.GetSequenceType()),
                                subExpression);
                    }
                }
            }

            return subExpression;
        }
    }
}
