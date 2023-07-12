// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON as base64 for <see langword="byte" /> array values.
/// </summary>
public sealed class JsonByteArrayReaderWriter : JsonValueReaderWriter<byte[]>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonByteArrayReaderWriter Instance { get; } = new();

    private JsonByteArrayReaderWriter()
    {
    }

    /// <inheritdoc />
    public override byte[] FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetBytesFromBase64();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, byte[] value)
        => writer.WriteBase64StringValue(value);
}
