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
public partial class CosmosShapedQueryCompilingExpressionVisitor
{
    private static PartitionKey GeneratePartitionKey(
        IEntityType rootEntityType,
        List<Expression> partitionKeyPropertyValues,
        IReadOnlyDictionary<string, object?> parameterValues)
    {
        if (partitionKeyPropertyValues.Count == 0)
        {
            return PartitionKey.None;
        }

        var builder = new PartitionKeyBuilder();

        var partitionKeyProperties = rootEntityType.GetPartitionKeyProperties();

        int i;
        for (i = 0; i < partitionKeyPropertyValues.Count; i++)
        {
            if (i >= partitionKeyProperties.Count)
            {
                break;
            }

            var property = partitionKeyProperties[i];

            switch (partitionKeyPropertyValues[i])
            {
                case SqlConstantExpression constant:
                    builder.Add(constant.Value, property);
                    continue;

                // If WithPartitionKey() was used, its second argument is a params object[] array, which gets parameterized as a single
                // parameter. Extract the object[] and iterate over the values within here.
                case SqlParameterExpression parameter when parameter.Type == typeof(object[]):
                {
                    if (!parameterValues.TryGetValue(parameter.Name, out var value)
                        || value is not object[] remainingValuesArray)
                    {
                        throw new UnreachableException("Couldn't find partition key parameter value");
                    }

                    for (var j = 0; j < remainingValuesArray.Length; j++, i++)
                    {
                        builder.Add(remainingValuesArray[j], partitionKeyProperties[i]);
                    }

                    goto End;
                }

                case SqlParameterExpression parameter:
                {
                    builder.Add(
                        parameterValues.TryGetValue(parameter.Name, out var value)
                            ? value
                            : throw new UnreachableException("Couldn't find partition key parameter value"),
                        property);
                    continue;
                }

                default:
                    throw new UnreachableException();
            }
        }

        End:
        if (i != partitionKeyProperties.Count)
        {
            throw new InvalidOperationException(
                CosmosStrings.IncorrectPartitionKeyNumber(rootEntityType.DisplayName(), i, partitionKeyProperties.Count));
        }

        return builder.Build();
    }
}
