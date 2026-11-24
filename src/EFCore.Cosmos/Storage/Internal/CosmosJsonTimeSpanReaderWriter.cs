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
public sealed class CosmosJsonTimeSpanReaderWriter : JsonValueReaderWriter<TimeSpan>
{
    private static readonly PropertyInfo InstanceProperty = typeof(CosmosJsonTimeSpanReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static CosmosJsonTimeSpanReaderWriter Instance { get; } = new();

    private CosmosJsonTimeSpanReaderWriter()
    {
    }

    /// <inheritdoc />
    public override TimeSpan FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => TimeSpan.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TimeSpan value)
        => writer.WriteStringValue(value.ToString("c", CultureInfo.InvariantCulture));

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
