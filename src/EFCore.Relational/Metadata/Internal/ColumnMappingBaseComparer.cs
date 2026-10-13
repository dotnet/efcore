// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class ColumnMappingBaseComparer : IEqualityComparer<IColumnMappingBase>, IComparer<IColumnMappingBase>
{
    private ColumnMappingBaseComparer()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ColumnMappingBaseComparer Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(IColumnMappingBase? x, IColumnMappingBase? y)
        => x is null
            ? y is null ? 0 : -1
            : y is null ? 1 : x.CompareTo(y);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool Equals(IColumnMappingBase? x, IColumnMappingBase? y)
        => ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.Property == y.Property
                && x.Column == y.Column
                && TableMappingBaseComparer.Instance.Equals(x.TableMapping, y.TableMapping));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int GetHashCode(IColumnMappingBase obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.Property);
        hashCode.Add(obj.Column);
        hashCode.Add(obj.TableMapping, TableMappingBaseComparer.Instance);

        return hashCode.ToHashCode();
    }
}
