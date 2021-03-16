// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class RelationalQueryMetadataExtractingExpressionVisitor : ExpressionVisitor
    {
        private readonly RelationalQueryCompilationContext _relationalQueryCompilationContext;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public RelationalQueryMetadataExtractingExpressionVisitor(
            [NotNull] RelationalQueryCompilationContext relationalQueryCompilationContext)
        {
            Check.NotNull(relationalQueryCompilationContext, nameof(relationalQueryCompilationContext));

            _relationalQueryCompilationContext = relationalQueryCompilationContext;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == RelationalQueryableExtensions.AsSplitQueryMethodInfo)
            {
                var innerQueryable = Visit(methodCallExpression.Arguments[0]);

                _relationalQueryCompilationContext.QuerySplittingBehavior = QuerySplittingBehavior.SplitQuery;

                return innerQueryable;
            }

            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == RelationalQueryableExtensions.AsSingleQueryMethodInfo)
            {
                var innerQueryable = Visit(methodCallExpression.Arguments[0]);

                _relationalQueryCompilationContext.QuerySplittingBehavior = QuerySplittingBehavior.SingleQuery;

                return innerQueryable;
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
