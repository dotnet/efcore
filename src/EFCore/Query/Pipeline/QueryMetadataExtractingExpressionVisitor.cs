// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
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
                var genericMethodDefinition = method.GetGenericMethodDefinition();
                if (genericMethodDefinition == EntityFrameworkQueryableExtensions.AsTrackingMethodInfo
                    || genericMethodDefinition == EntityFrameworkQueryableExtensions.AsNoTrackingMethodInfo)
                {
                    var innerQueryable = Visit(methodCallExpression.Arguments[0]);
                    _queryCompilationContext.TrackQueryResults
                        = genericMethodDefinition == EntityFrameworkQueryableExtensions.AsTrackingMethodInfo;

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
