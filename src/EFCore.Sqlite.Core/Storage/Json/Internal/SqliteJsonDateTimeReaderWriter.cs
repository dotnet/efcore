// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
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
public sealed class SqliteJsonDateTimeReaderWriter : JsonValueReaderWriter<DateTime>
{
    private const string DateTimeFormatConst = @"{0:yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF}";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static SqliteJsonDateTimeReaderWriter Instance { get; } = new();

    private SqliteJsonDateTimeReaderWriter()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
        => DateTime.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
        => writer.WriteStringValue(string.Format(CultureInfo.InvariantCulture, DateTimeFormatConst, value));
}
