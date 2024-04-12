// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="ushort" /> values.
/// </summary>
public sealed class JsonUInt16ReaderWriter : JsonValueReaderWriter<ushort>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonUInt16ReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonUInt16ReaderWriter Instance { get; } = new();

    private JsonUInt16ReaderWriter()
    {
    }

    /// <inheritdoc />
    public override ushort FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetUInt16();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, ushort value)
        => writer.WriteNumberValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
}
