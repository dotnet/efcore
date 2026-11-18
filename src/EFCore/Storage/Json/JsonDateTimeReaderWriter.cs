// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Storage.Json;

/// <summary>
///     Reads and writes JSON for <see cref="DateTime" /> values.
/// </summary>
public sealed class JsonDateTimeReaderWriter : JsonValueReaderWriter<DateTime>
{
    private const string DefaultFormat = "yyyy-MM-dd'T'HH:mm:ss.fffffffK";

    private static readonly PropertyInfo InstanceProperty = typeof(JsonDateTimeReaderWriter).GetProperty(nameof(Instance))!;

    private static readonly ConstructorInfo Constructor = typeof(JsonDateTimeReaderWriter).GetConstructor([typeof(string)])!;

    private readonly string _format;

    /// <summary>
    ///     The singleton instance of this stateless reader/writer, which writes values using a fixed-width
    ///     ISO 8601 format with seven fractional second digits.
    /// </summary>
    public static JsonDateTimeReaderWriter Instance { get; } = new(DefaultFormat);

    /// <summary>
    ///     Creates a new instance of this reader/writer that writes values using the given format.
    /// </summary>
    /// <remarks>
    ///     Use <c>"yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK"</c> to write values with trailing zero fractional seconds omitted,
    ///     as was done in previous releases.
    /// </remarks>
    /// <param name="format">The <see cref="DateTime" /> format string used when writing values.</param>
    public JsonDateTimeReaderWriter(string format)
    {
        Check.NotEmpty(format);

        _format = format;
    }

    /// <inheritdoc />
    public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => manager.CurrentReader.GetDateTime();

    /// <inheritdoc />
    public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
        => writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));

    /// <inheritdoc />
    public override Expression ConstructorExpression
        => ReferenceEquals(this, Instance)
            ? Expression.Property(null, InstanceProperty)
            : Expression.New(Constructor, Expression.Constant(_format));
}
