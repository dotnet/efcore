// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="UInt128" /> values.
/// </summary>
public sealed class JsonUInt128ReaderWriter : JsonValueReaderWriter<UInt128>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonUInt128ReaderWriter Instance { get; } = new();

    private JsonUInt128ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override UInt128 FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
    {
        if (!UInt128.TryParse(manager.CurrentReader.GetString(), out UInt128 result))
        {
            throw new FormatException("UInt128");
        }
        return result;
    }

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, UInt128 value)
        => writer.WriteStringValue(value.ToString("R", CultureInfo.InvariantCulture));
}
