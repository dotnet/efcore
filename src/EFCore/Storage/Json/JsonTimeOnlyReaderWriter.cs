// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="TimeOnly" /> values.
/// </summary>
public sealed class JsonTimeOnlyReaderWriter : JsonValueReaderWriter<TimeOnly>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonTimeOnlyReaderWriter Instance { get; } = new();

    private JsonTimeOnlyReaderWriter()
    {
    }

    /// <inheritdoc />
    public override TimeOnly FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => TimeOnly.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, TimeOnly value)
        => writer.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
}
