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

        var result = y.Property.IsPrimaryKey().CompareTo(x.Property.IsPrimaryKey());
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(x.Property.Name, y.Property.Name);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(x.Column.Name, y.Column.Name);
        if (result != 0)
        {
            return result;
        }

        result = EntityTypeFullNameComparer.Instance.Compare(x.TableMapping.EntityType, y.TableMapping.EntityType);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(x.Column.Table.Name, y.Column.Table.Name);
        if (result != 0)
        {
            return result;
        }

        return StringComparer.Ordinal.Compare(x.Column.Table.Schema, y.Column.Table.Schema);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool Equals(IColumnMappingBase? x, IColumnMappingBase? y)
        => ReferenceEquals(x, y)
            || (x is not null && y is not null
                && x.Property == y.Property && x.Column == y.Column);

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

        return hashCode.ToHashCode();
    }
}
