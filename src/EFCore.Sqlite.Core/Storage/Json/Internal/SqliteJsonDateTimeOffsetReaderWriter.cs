// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Json.Internal;

/// <summary>
///     The Sqlite-specific JsonValueReaderWrite for DateTime. Generates a ISO8601 string representation with a space instead of a T
///     separating the date and time components, in order to match our SQLite non-JSON representation.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public sealed class SqliteJsonDateTimeOffsetReaderWriter : JsonValueReaderWriter<DateTimeOffset>
{
    private const string DateTimeOffsetFormatConst = @"{0:yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz}";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqliteJsonDateTimeOffsetReaderWriter Instance { get; } = new();

    private SqliteJsonDateTimeOffsetReaderWriter()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DateTimeOffset FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        // => manager.CurrentReader.GetDateTimeOffset();
        => DateTimeOffset.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void ToJsonTyped(Utf8JsonWriter writer, DateTimeOffset value)
        // We use UnsafeRelaxedJsonEscaping to prevent the DateTimeOffset plus (+) sign from getting escaped
        => writer.WriteStringValue(
            JsonEncodedText.Encode(
                string.Format(CultureInfo.InvariantCulture, DateTimeOffsetFormatConst, value),
                JavaScriptEncoder.UnsafeRelaxedJsonEscaping));
}
