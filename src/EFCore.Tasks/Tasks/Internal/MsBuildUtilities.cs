// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NET472
using System.Configuration;
#endif

namespace Microsoft.EntityFrameworkCore.Tasks.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
internal class MsBuildUtilities
{
    public static string[] Split(string s)
        => !string.IsNullOrEmpty(s)
            ? s.Split(';')
                .Select(entry => entry.Trim())
                .Where(entry => entry.Length != 0)
                .ToArray()
            : [];

    public static string? TrimAndGetNullForEmpty(string? s)
    {
        if (s == null)
        {
            return null;
        }

        s = s.Trim();

        return s.Length == 0 ? null : s;
    }

    public static string[] TrimAndExcludeNullOrEmpty(string?[]? strings)
        => strings == null
            ? []
            : strings
                .Select(TrimAndGetNullForEmpty)
                .Where(s => s != null)
                .Cast<string>()
                .ToArray();

    public static bool IsTrue(string? value) => bool.TrueString.Equals(TrimAndGetNullForEmpty(value), StringComparison.OrdinalIgnoreCase);

    public static bool IsTrueOrEmpty(string? value) => TrimAndGetNullForEmpty(value) == null || IsTrue(value);

    public static bool? GetBooleanOrNull(string? value) => bool.TryParse(value, out var result) ? result : null;

    public static string? ToMsBuild(string? value) => value?.Replace(',', ';');
}
