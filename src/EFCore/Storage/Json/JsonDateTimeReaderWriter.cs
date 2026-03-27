// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="DateTime" /> values.
/// </summary>
public sealed class JsonDateTimeReaderWriter : JsonValueReaderWriter<DateTime>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonDateTimeReaderWriter).GetProperty(nameof(Instance))!;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer.
    /// </summary>
    public static JsonDateTimeReaderWriter Instance { get; } = new();

    private JsonDateTimeReaderWriter()
    {
    }

    /// <inheritdoc />
    public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetDateTime();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
        => writer.WriteStringValue(value);

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
