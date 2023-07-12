// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="int" /> values.
/// </summary>
public sealed class JsonInt32ReaderWriter : JsonValueReaderWriter<int>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonInt32ReaderWriter Instance { get; } = new();

    private JsonInt32ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override int FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetInt32();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, int value)
        => writer.WriteNumberValue(value);
}
