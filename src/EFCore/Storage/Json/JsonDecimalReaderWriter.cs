// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="decimal" /> values.
/// </summary>
public sealed class JsonDecimalReaderWriter : JsonValueReaderWriter<decimal>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonDecimalReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonDecimalReaderWriter Instance { get; } = new();

    private JsonDecimalReaderWriter()
    {
    }

    /// <inheritdoc />
    public override decimal FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetDecimal();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, decimal value)
        => writer.WriteNumberValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
