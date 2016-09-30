// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A projection expression visitor.
    /// </summary>
    public class ProjectionExpressionVisitor : DefaultQueryExpressionVisitor
    {
        /// <summary>
        ///     Initializes a new instance of the Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.ProjectionExpressionVisitor class.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The entity query model visitor. </param>
        public ProjectionExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor)
            : base(Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)))
        {
        }

        /// <summary>
        ///     Visit a subquery.
        /// </summary>
        /// <param name="expression"> The subquery expression. </param>
        /// <returns>
        ///     A compiled query expression fragment representing the input subquery expression.
        /// </returns>
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
                            subExpression,
                            EntityQueryModelVisitor.QueryContextParameter);
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
