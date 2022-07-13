// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class TableBaseIdentityComparer : IEqualityComparer<ITableBase>
{
    private TableBaseIdentityComparer()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly TableBaseIdentityComparer Instance = new();

    /// <inheritdoc />
    public bool Equals(ITableBase? x, ITableBase? y)
        => ReferenceEquals(x, y)
            || (x is null
                ? y is null
                : y is not null && x.Name == y.Name && x.Schema == y.Schema);

    /// <inheritdoc />
    public int GetHashCode(ITableBase obj)
        => HashCode.Combine(obj.Name, obj.Schema);
}
