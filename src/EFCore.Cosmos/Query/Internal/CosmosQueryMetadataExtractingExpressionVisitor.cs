// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

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
    public CosmosQueryMetadataExtractingExpressionVisitor(CosmosQueryCompilationContext cosmosQueryCompilationContext)
    {
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

            _cosmosQueryCompilationContext.PartitionKeyFromExtension = methodCallExpression.Arguments[1].GetConstantValue<string>();

            return innerQueryable;
        }

        return base.VisitMethodCall(methodCallExpression);
    }
}
