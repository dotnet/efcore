// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="double" /> values.
/// </summary>
public sealed class JsonDoubleReaderWriter : JsonValueReaderWriter<double>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonDoubleReaderWriter Instance { get; } = new();

    private JsonDoubleReaderWriter()
    {
    }

    /// <inheritdoc />
    public override double FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetDouble();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, double value)
        => writer.WriteNumberValue(value);
}
