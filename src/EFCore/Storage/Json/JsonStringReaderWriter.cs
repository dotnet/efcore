// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="string" /> values.
/// </summary>
public sealed class JsonStringReaderWriter : JsonValueReaderWriter<string>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonStringReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonStringReaderWriter Instance { get; } = new();

    private JsonStringReaderWriter()
    {
    }

    /// <inheritdoc />
    public override string FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetString()!;

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, string value)
        => writer.WriteStringValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression => Expression.Property(null, InstanceProperty);
}
