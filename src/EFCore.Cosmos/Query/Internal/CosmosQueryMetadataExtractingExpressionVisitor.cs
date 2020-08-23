// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CosmosQueryMetadataExtractingExpressionVisitor : ExpressionVisitor
    {
        private readonly CosmosQueryCompilationContext _cosmosQueryCompilationContext;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CosmosQueryMetadataExtractingExpressionVisitor([NotNull] CosmosQueryCompilationContext cosmosQueryCompilationContext)
        {
            Check.NotNull(cosmosQueryCompilationContext, nameof(cosmosQueryCompilationContext));
            _cosmosQueryCompilationContext = cosmosQueryCompilationContext;
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
                && methodCallExpression.Method.GetGenericMethodDefinition() == CosmosQueryableExtensions.WithPartitionKeyMethodInfo)
            {
                var innerQueryable = Visit(methodCallExpression.Arguments[0]);

                _cosmosQueryCompilationContext.PartitionKeyFromExtension =
                    (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;

                return innerQueryable;
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
