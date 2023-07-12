// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="Guid" /> values.
/// </summary>
public sealed class JsonGuidReaderWriter : JsonValueReaderWriter<Guid>
{
    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonGuidReaderWriter Instance { get; } = new();

    private JsonGuidReaderWriter()
    {
    }

    /// <inheritdoc />
    public override Guid FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetGuid();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, Guid value)
        => writer.WriteStringValue(value);
}
