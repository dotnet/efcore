// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="bool" /> values.
/// </summary>
public sealed class JsonBoolReaderWriter : JsonValueReaderWriter<bool>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonBoolReaderWriter Instance { get; } = new();

    private JsonBoolReaderWriter()
    {
    }

    /// <inheritdoc />
    public override bool FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetBoolean();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, bool value)
        => writer.WriteBooleanValue(value);
}
