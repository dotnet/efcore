// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class QueryFiltersExpectedQueryRewritingVisitor : ExpressionVisitor
    {
        private readonly IModel _model;

        public QueryFiltersExpectedQueryRewritingVisitor(IModel model)
        {
            _model = model;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            var entityType = _model.FindEntityType(memberExpression.Type);
            if (entityType != null)
            {
                var filterLambda = entityType.GetRootType().GetQueryFilter();
                filterLambda = (LambdaExpression)Visit(filterLambda);

                if (filterLambda != null)
                {
                    // x.Entity -> x.Entity != null && filter(x.Entity) ? x.Entity : null
                    var remappedBody = ReplacingExpressionVisitor.Replace(filterLambda.Parameters[0], memberExpression, filterLambda.Body);

                    var result = Expression.Condition(
                        Expression.AndAlso(
                        Expression.NotEqual(
                            memberExpression,
                            Expression.Constant(null, memberExpression.Type)),
                            remappedBody),
                        memberExpression,
                        Expression.Constant(null, memberExpression.Type));

                    return result;
                }
            }

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            if (constantExpression.Type.IsGenericType
                && constantExpression.Type.GetGenericTypeDefinition() == typeof(EnumerableQuery<>))
            {
                var typeArgument = constantExpression.Type.GetGenericArguments()[0];
                var entityType = _model.FindEntityType(typeArgument);
                if (entityType != null)
                {
                    var filterLambda = entityType.GetRootType().GetQueryFilter();
                    filterLambda = (LambdaExpression)Visit(filterLambda);

                    if (filterLambda != null)
                    {
                        // for OfType we need to rewrite the lambda parameter to a proper type
                        if (entityType.GetRootType() != entityType)
                        {
                            var newPrm = Expression.Parameter(entityType.ClrType, "x");
                            var newBody = ReplacingExpressionVisitor.Replace(filterLambda.Parameters[0], newPrm, filterLambda.Body);
                            filterLambda = Expression.Lambda(newBody, newPrm);
                        }

                        var whereMethod = QueryableMethods.Where.MakeGenericMethod(typeArgument);
                        var result = Expression.Call(whereMethod, constantExpression, filterLambda);

                        return result;
                    }
                }
            }

            return base.VisitConstant(constantExpression);
        }
    }
}
