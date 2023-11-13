// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class PropertyNameComparer : IComparer<string>, IEqualityComparer<string>
{
    private readonly IReadOnlyEntityType? _entityType;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public PropertyNameComparer(IReadOnlyTypeBase typeBase)
    {
        _entityType = typeBase as IReadOnlyEntityType;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(string? x, string? y)
    {
        var xIndex = -1;
        var yIndex = -1;

        var properties = _entityType?.FindPrimaryKey()?.Properties;
        if (properties != null)
        {
            for (var i = 0; i < properties.Count; i++)
            {
                var name = properties[i].Name;

                if (name == x)
                {
                    xIndex = i;
                }

                if (name == y)
                {
                    yIndex = i;
                }
            }
        }

        // Neither property is part of the Primary Key
        // Compare the property names
        if (xIndex == -1
            && yIndex == -1)
        {
            return StringComparer.Ordinal.Compare(x, y);
        }

        // Both properties are part of the Primary Key
        // Compare the indices
        if (xIndex > -1
            && yIndex > -1)
        {
            return xIndex - yIndex;
        }

        // One property is part of the Primary Key
        // The primary key property is first
        return xIndex > yIndex
            ? -1
            : 1;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool Equals(string? x, string? y)
        => StringComparer.Ordinal.Equals(x, y);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int GetHashCode(string obj)
        => StringComparer.Ordinal.GetHashCode(obj);
}
