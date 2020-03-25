// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    public class CosmosQueryMetadataExtractingExpressionVisitor : ExpressionVisitor
    {
        private readonly CosmosQueryCompilationContext _cosmosQueryCompilationContext;

        public CosmosQueryMetadataExtractingExpressionVisitor([NotNull] CosmosQueryCompilationContext cosmosQueryCompilationContext)
        {
            Check.NotNull(cosmosQueryCompilationContext, nameof(cosmosQueryCompilationContext));
            _cosmosQueryCompilationContext = cosmosQueryCompilationContext;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == CosmosQueryableExtensions.WithPartitionKeyMethodInfo)
            {
                var innerQueryable = Visit(methodCallExpression.Arguments[0]);

                _cosmosQueryCompilationContext.PartitionKey = (string)((ConstantExpression)methodCallExpression.Arguments[1]).Value;

                return innerQueryable;
            }

            return base.VisitMethodCall(methodCallExpression);
        }

    }
}
