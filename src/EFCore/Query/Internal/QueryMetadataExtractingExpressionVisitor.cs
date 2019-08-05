// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class QueryMetadataExtractingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryCompilationContext _queryCompilationContext;

        public QueryMetadataExtractingExpressionVisitor(QueryCompilationContext queryCompilationContext)
        {
            _queryCompilationContext = queryCompilationContext;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var method = methodCallExpression.Method;
            if (method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && method.IsGenericMethod)
            {
                // We visit innerQueryable first so that we can get information in the same order operators are applied.
                var genericMethodDefinition = method.GetGenericMethodDefinition();
                if (genericMethodDefinition == EntityFrameworkQueryableExtensions.AsTrackingMethodInfo
                    || genericMethodDefinition == EntityFrameworkQueryableExtensions.AsNoTrackingMethodInfo)
                {
                    var innerQueryable = Visit(methodCallExpression.Arguments[0]);
                    _queryCompilationContext.IsTracking
                        = genericMethodDefinition == EntityFrameworkQueryableExtensions.AsTrackingMethodInfo;

                    return innerQueryable;
                }

                if (genericMethodDefinition == EntityFrameworkQueryableExtensions.TagWithMethodInfo)
                {
                    var innerQueryable = Visit(methodCallExpression.Arguments[0]);
                    _queryCompilationContext.AddTag((string)((ConstantExpression)methodCallExpression.Arguments[1]).Value);

                    return innerQueryable;
                }

                if (genericMethodDefinition == EntityFrameworkQueryableExtensions.IgnoreQueryFiltersMethodInfo)
                {
                    var innerQueryable = Visit(methodCallExpression.Arguments[0]);

                    _queryCompilationContext.IgnoreQueryFilters = true;

                    return innerQueryable;
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
