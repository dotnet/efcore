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
public sealed class PropertyListComparer : IComparer<IReadOnlyList<IReadOnlyPropertyBase>>,
    IEqualityComparer<IReadOnlyList<IReadOnlyPropertyBase>>
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly PropertyListComparer Instance = new();

    private PropertyListComparer()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(IReadOnlyList<IReadOnlyPropertyBase>? x, IReadOnlyList<IReadOnlyPropertyBase>? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var result = x.Count - y.Count;
        if (result != 0)
        {
            return result;
        }

        var index = 0;
        while ((result == 0)
               && (index < x.Count))
        {
            result = StringComparer.Ordinal.Compare(x[index].Name, y[index].Name);
            if (result == 0)
            {
                result = StringComparer.Ordinal.Compare(x[index].DeclaringType.Name, y[index].DeclaringType.Name);
            }

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
    public bool Equals(IReadOnlyList<IReadOnlyPropertyBase>? x, IReadOnlyList<IReadOnlyPropertyBase>? y)
        => Compare(x, y) == 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int GetHashCode(IReadOnlyList<IReadOnlyPropertyBase> obj)
    {
        var hash = new HashCode();
        for (var i = 0; i < obj.Count; i++)
        {
            hash.Add(obj[i].Name);
            hash.Add(obj[i].DeclaringType.Name);
        }

        return hash.ToHashCode();
    }
}
