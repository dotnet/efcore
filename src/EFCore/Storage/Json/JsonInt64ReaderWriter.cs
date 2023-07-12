// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="long" /> values.
/// </summary>
public sealed class JsonInt64ReaderWriter : JsonValueReaderWriter<long>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonInt64ReaderWriter Instance { get; } = new();

    private JsonInt64ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override long FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetInt64();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, long value)
        => writer.WriteNumberValue(value);
}
