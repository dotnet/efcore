// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="short" /> values.
/// </summary>
public sealed class JsonInt16ReaderWriter : JsonValueReaderWriter<short>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonInt16ReaderWriter Instance { get; } = new();

    private JsonInt16ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override short FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetInt16();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, short value)
        => writer.WriteNumberValue(value);
}
