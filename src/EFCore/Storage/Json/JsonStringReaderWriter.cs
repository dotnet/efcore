// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Encodings.Web;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see langword="string" /> values.
/// </summary>
public sealed class JsonStringReaderWriter : JsonValueReaderWriter<string>
{
    private static readonly PropertyInfo InstanceProperty = typeof(JsonStringReaderWriter).GetProperty(nameof(Instance))!;

    private static readonly bool UseOldBehavior32152 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue32152", out var enabled32152) && enabled32152;

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
    {
        if (UseOldBehavior32152)
        {
            writer.WriteStringValue(value);
        }
        else
        {
            writer.WriteStringValue(JsonEncodedText.Encode(value, JavaScriptEncoder.UnsafeRelaxedJsonEscaping));
        }
    }

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => Expression.Property(null, InstanceProperty);
}
