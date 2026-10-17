// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class CosmosJsonTimeOnlyReaderWriter : JsonValueReaderWriter<TimeOnly>
{
    private static readonly PropertyInfo InstanceProperty = typeof(CosmosJsonTimeOnlyReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CosmosJsonTimeOnlyReaderWriter Instance { get; } = new();

    private CosmosJsonTimeOnlyReaderWriter()
    {
    }

    /// <inheritdoc />
    public override TimeOnly FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => TimeOnly.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TimeOnly value)
        => writer.WriteStringValue(value.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture));

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
