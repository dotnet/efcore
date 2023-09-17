// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="Int128" /> values.
/// </summary>
public sealed class JsonInt128ReaderWriter : JsonValueReaderWriter<Int128>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonInt128ReaderWriter Instance { get; } = new();

    private JsonInt128ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override Int128 FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        if (!Int128.TryParse(manager.CurrentReader.GetString(), out Int128 result))
        {
            throw new FormatException("Int128");
        }
        return result;
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, Int128 value)
        => writer.WriteStringValue(value.ToString("R", CultureInfo.InvariantCulture));
}
