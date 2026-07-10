// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="ulong" /> values.
/// </summary>
public sealed class JsonUInt64ReaderWriter : JsonValueReaderWriter<ulong>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonUInt64ReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonUInt64ReaderWriter Instance { get; } = new();

    private JsonUInt64ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override ulong FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetUInt64();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, ulong value)
        => writer.WriteNumberValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
