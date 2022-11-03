// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
// Sealed for perf
public sealed class NamedListComparer : IComparer<(string, string?, IReadOnlyList<string>)>,
    IEqualityComparer<(string, string?, IReadOnlyList<string>)>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly NamedListComparer Instance = new();

    private NamedListComparer()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare((string, string?, IReadOnlyList<string>) x, (string, string?, IReadOnlyList<string>) y)
    {
        var (x1, x2, xList) = x;
        var (y1, y2, yList) = y;
        var result = StringComparer.Ordinal.Compare(x1, y1);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(x2, y2);
        if (result != 0)
        {
            return result;
        }

        result = xList.Count - yList.Count;
        if (result != 0)
        {
            return result;
        }

        var index = 0;
        while ((result == 0)
               && (index < xList.Count))
        {
            result = StringComparer.Ordinal.Compare(xList[index], yList[index]);
            index++;
        }

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool Equals((string, string?, IReadOnlyList<string>) x, (string, string?, IReadOnlyList<string>) y)
        => Compare(x, y) == 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int GetHashCode((string, string?, IReadOnlyList<string>) obj)
    {
        var hash = new HashCode();
        var (item1, item2, list) = obj;
        hash.Add(item1);
        hash.Add(item2);
        for (var i = 0; i < list.Count; i++)
        {
            hash.Add(list[i]);
        }

        return hash.ToHashCode();
    }
}
