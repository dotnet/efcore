// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="byte" /> values.
/// </summary>
public sealed class JsonByteReaderWriter : JsonValueReaderWriter<byte>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonByteReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonByteReaderWriter Instance { get; } = new();

    private JsonByteReaderWriter()
    {
    }

    /// <inheritdoc />
    public override byte FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetByte();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, byte value)
        => writer.WriteNumberValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
