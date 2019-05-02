// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.Pipeline
{
    public class QueryMetadataExtractingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryCompilationContext2 _queryCompilationContext;

        public QueryMetadataExtractingExpressionVisitor(QueryCompilationContext2 queryCompilationContext)
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
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
