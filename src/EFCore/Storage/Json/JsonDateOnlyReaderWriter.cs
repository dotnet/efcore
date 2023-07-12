// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="DateOnly" /> values.
/// </summary>
public sealed class JsonDateOnlyReaderWriter : JsonValueReaderWriter<DateOnly>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonDateOnlyReaderWriter Instance { get; } = new();

    private JsonDateOnlyReaderWriter()
    {
    }

    /// <inheritdoc />
    public override DateOnly FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => DateOnly.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, DateOnly value)
        => writer.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
}
