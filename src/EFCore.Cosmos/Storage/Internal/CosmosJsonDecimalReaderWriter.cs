// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class CosmosJsonDecimalReaderWriter : JsonValueReaderWriter<decimal>
{
    private static readonly PropertyInfo InstanceProperty = typeof(CosmosJsonDecimalReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static CosmosJsonDecimalReaderWriter Instance { get; } = new();

    private CosmosJsonDecimalReaderWriter()
    {
    }

    /// <inheritdoc />
    public override decimal FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => (decimal)manager.CurrentReader.GetDouble(); // Old serializer used to read returned numbers as double and convert to decimal @TODO: Cosmos does not support decimals: #38137

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, decimal value)
        => writer.WriteNumberValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
