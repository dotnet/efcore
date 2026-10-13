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
public sealed class TableMappingBaseComparer : IEqualityComparer<ITableMappingBase>, IComparer<ITableMappingBase>
{
    private TableMappingBaseComparer()
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly TableMappingBaseComparer Instance = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int Compare(ITableMappingBase? x, ITableMappingBase? y)
        => x is null
            ? y is null ? 0 : -1
            : y is null ? 1 : x.CompareTo(y);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool Equals(ITableMappingBase? x, ITableMappingBase? y)
        => ReferenceEquals(x, y)
            || (x is not null
                && y is not null
                && x.TypeBase == y.TypeBase
                && x.Table == y.Table
                && x.IncludesDerivedTypes == y.IncludesDerivedTypes
                && x.ColumnMappings.SequenceEqual(y.ColumnMappings));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public int GetHashCode(ITableMappingBase obj)
    {
        var hashCode = new HashCode();
        hashCode.Add(obj.TypeBase, TypeBaseNameComparer.Instance);
        hashCode.Add(obj.Table.Name);
        hashCode.Add(obj.Table.Schema);
        if (obj.Table is IStoreFunction function)
        {
            foreach (var parameter in function.Parameters)
            {
                hashCode.Add(parameter.StoreType);
                hashCode.Add(parameter.Name);
            }
        }
        else if (obj.Table is IStoreStoredProcedure sproc)
        {
            foreach (var parameter in sproc.Parameters)
            {
                hashCode.Add(parameter.StoreType);
                hashCode.Add(parameter.Name);
            }
        }

        foreach (var columnMapping in obj.ColumnMappings)
        {
            hashCode.Add(columnMapping.Property.Name);
            hashCode.Add(columnMapping.Column.Name);
        }

        hashCode.Add(obj.IncludesDerivedTypes);
        return hashCode.ToHashCode();
    }
}
