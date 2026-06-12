// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public sealed class ComplexPropertyNameComparer(IReadOnlyTypeBase typeBase) : IComparer<string>, IEqualityComparer<string>
{
    private readonly IReadOnlyTypeBase _typeBase = typeBase;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(string? x, string? y)
    {
        if (StringComparer.Ordinal.Equals(x, y))
        {
            return 0;
        }

        var primaryKeyProperties = _typeBase.ContainingEntityType?.FindPrimaryKey()?.Properties;
        if (primaryKeyProperties is { Count: > 0 })
        {
            var xContainsKey = x != null && ContainsKeyProperty(x, primaryKeyProperties);
            var yContainsKey = y != null && ContainsKeyProperty(y, primaryKeyProperties);

            if (xContainsKey != yContainsKey)
            {
                return xContainsKey ? -1 : 1;
            }
        }

        return StringComparer.Ordinal.Compare(x, y);
    }

    private bool ContainsKeyProperty(
        string complexPropertyName,
        IReadOnlyList<IReadOnlyProperty> primaryKeyProperties)
    {
        for (var i = 0; i < primaryKeyProperties.Count; i++)
        {
            var keyProperty = primaryKeyProperties[i];
            for (var current = keyProperty.DeclaringType as IReadOnlyComplexType;
                 current != null;
                 current = current.ComplexProperty.DeclaringType as IReadOnlyComplexType)
            {
                if (current.ComplexProperty.DeclaringType == _typeBase
                    && current.ComplexProperty.Name == complexPropertyName)
                {
                    return true;
                }
            }
        }

        return false;
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
