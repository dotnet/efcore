// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="sbyte" /> values.
/// </summary>
public sealed class JsonSByteReaderWriter : JsonValueReaderWriter<sbyte>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonSByteReaderWriter Instance { get; } = new();

    private JsonSByteReaderWriter()
    {
    }

    /// <inheritdoc />
    public override sbyte FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetSByte();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, sbyte value)
        => writer.WriteNumberValue(value);
}
