// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed record class CosmosVectorType(DistanceFunction DistanceFunction, ulong Dimensions, VectorDataType? DataType)
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static VectorDataType CreateDefaultVectorDataType(Type clrType)
    {
        var elementType = clrType.TryGetElementType(typeof(IEnumerable<>))?.UnwrapNullableType();
        return elementType == typeof(sbyte)
            ? VectorDataType.Int8
            : elementType == typeof(byte)
                ? VectorDataType.Uint8
                : elementType == typeof(Half)
                    ? VectorDataType.Float16
                    : elementType == typeof(float)
                        ? VectorDataType.Float32
                        : throw new InvalidOperationException(CosmosStrings.BadVectorDataType(clrType.ShortDisplayName()));
    }
}
