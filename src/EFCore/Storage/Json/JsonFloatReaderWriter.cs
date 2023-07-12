// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="float" /> values.
/// </summary>
public sealed class JsonFloatReaderWriter : JsonValueReaderWriter<float>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonFloatReaderWriter Instance { get; } = new();

    private JsonFloatReaderWriter()
    {
    }

    /// <inheritdoc />
    public override float FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetSingle();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, float value)
        => writer.WriteNumberValue(value);
}
