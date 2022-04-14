// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.EntityFrameworkCore.Migrations.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MigrationsIdGenerator : IMigrationsIdGenerator
{
    private const string Format = "yyyyMMddHHmmss";

    private DateTime _lastTimestamp = DateTime.MinValue;
    private readonly object _lock = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GetName(string id)
        => id[(Format.Length + 1)..];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsValidId(string value)
        => Regex.IsMatch(
            value,
            string.Format(CultureInfo.InvariantCulture, "^[0-9]{{{0}}}_.+", Format.Length),
            default,
            TimeSpan.FromMilliseconds(1000.0));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string GenerateId(string name)
    {
        var now = DateTime.UtcNow;
        var timestamp = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        lock (_lock)
        {
            if (timestamp <= _lastTimestamp)
            {
                timestamp = _lastTimestamp.AddSeconds(1);
            }

            _lastTimestamp = timestamp;
        }

        return timestamp.ToString(Format, CultureInfo.InvariantCulture) + "_" + name;
    }
}
