// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="null" /> values.
/// </summary>
public sealed class JsonNullReaderWriter : JsonValueReaderWriter<object?>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonNullReaderWriter Instance { get; } = new();

    private JsonNullReaderWriter()
    {
    }

    /// <inheritdoc />
    public override object? FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => null;

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, object? value)
        => writer.WriteNullValue();
}
