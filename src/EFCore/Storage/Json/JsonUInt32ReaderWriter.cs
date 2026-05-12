// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="uint" /> values.
/// </summary>
public sealed class JsonUInt32ReaderWriter : JsonValueReaderWriter<uint>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonUInt32ReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonUInt32ReaderWriter Instance { get; } = new();

    private JsonUInt32ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override uint FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetUInt32();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, uint value)
        => writer.WriteNumberValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
