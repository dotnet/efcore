// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="char" /> values.
/// </summary>
public sealed class JsonCharReaderWriter : JsonValueReaderWriter<char>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonCharReaderWriter Instance { get; } = new();

    private JsonCharReaderWriter()
    {
    }

    /// <inheritdoc />
    public override char FromJsonTyped(ref Utf8JsonReaderManager manager)
        => manager.CurrentReader.GetString()![0];

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, char value)
        => writer.WriteStringValue(value.ToString());
}
