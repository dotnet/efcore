// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CosmosQueryMetadataExtractingExpressionVisitor(CosmosQueryCompilationContext cosmosQueryCompilationContext)
    : ExpressionVisitor
{
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

            var firstValue = methodCallExpression.Arguments[1].GetConstantValue<object?>();
            if (firstValue == null)
            {
                cosmosQueryCompilationContext.PartitionKeyValueFromExtension = PartitionKey.None;
            }
            else
            {
                if (innerQueryable is EntityQueryRootExpression rootExpression)
                {
                    var partitionKeyProperties = rootExpression.EntityType.GetPartitionKeyProperties();
                    var allValues = new[] { firstValue }.Concat(methodCallExpression.Arguments[2].GetConstantValue<object[]>()).ToList();
                    var builder = new PartitionKeyBuilder();
                    for (var i = 0; i < allValues.Count; i++)
                    {
                        builder.Add(allValues[i], partitionKeyProperties[i]);
                    }

                    cosmosQueryCompilationContext.PartitionKeyValueFromExtension = builder.Build();
                }
                else
                {
                    throw new InvalidOperationException(CosmosStrings.WithPartitionKeyBadNode);
                }
            }

            return innerQueryable;
        }

        return base.VisitMethodCall(methodCallExpression);
    }
}
