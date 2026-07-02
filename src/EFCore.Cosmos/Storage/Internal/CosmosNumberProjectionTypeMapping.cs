// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Numerics;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public static class CosmosNumberProjectionTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CosmosTypeMapping CreateFromType(Type type)
        => (CosmosTypeMapping)typeof(CosmosNumberProjectionTypeMapping<>).MakeGenericType(type).GetProperty(nameof(CosmosTypeMapping.Default))!.GetValue(null)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsRequiredForProjection(Type type)
        => type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(float)
                  || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort);
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     Projections of numbers in cosmos can result in double precision floating point numbers,
///     and thus have to be read as doubles to prevent reader exceptions
/// </remarks>
public class CosmosNumberProjectionTypeMapping<T> : CosmosTypeMapping<T>
    where T : INumber<T>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static new CosmosNumberProjectionTypeMapping<T> Default { get; } = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosNumberProjectionTypeMapping() : base(
        null, null, null,
        JsonValueReaderWriter.CreateFromType(typeof(CosmosJsonNumberProjectionReaderWriter<>).MakeGenericType(typeof(T))))
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CosmosNumberProjectionTypeMapping(CoreTypeMappingParameters parameters) : base(parameters)
    {
    }

    /// <inheritdoc/>
    protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
        => new CosmosNumberProjectionTypeMapping<T>(parameters);
}
