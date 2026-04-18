// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

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
public class CosmosNumberProjectionTypeMapping : CosmosTypeMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    // Note that this default is not valid because because we don't know the projected number type at this time, this is intentionally not valid rather than just being wrong.
    // The fundamental problem here is that type mappings are "required" to have some default now.
    // @TODO: Could create a CosmosNumberProjectionTypeMapping per number type that needs one... Probably better.
    public static new CosmosNumberProjectionTypeMapping Default { get; } = new(typeof(object));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CosmosNumberProjectionTypeMapping(Type numberType) : base(
        numberType, null, null, null,
        JsonValueReaderWriter.CreateFromType(typeof(CosmosJsonNumberProjectionReaderWriter<>).MakeGenericType(numberType)))
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
        => new CosmosNumberProjectionTypeMapping(parameters);
}
