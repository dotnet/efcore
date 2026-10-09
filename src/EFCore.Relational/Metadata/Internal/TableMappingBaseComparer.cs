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

        var result = 0;
        if (y.IsSharedTablePrincipal == null)
        {
            if (x.IsSharedTablePrincipal != null)
            {
                return 1;
            }
        }
        else
        {
            if (x.IsSharedTablePrincipal == null)
            {
                return -1;
            }

            result = y.IsSharedTablePrincipal.Value.CompareTo(x.IsSharedTablePrincipal.Value);
            if (result != 0)
            {
                return result;
            }
        }

        if (y.IncludesDerivedTypes == null)
        {
            if (x.IncludesDerivedTypes != null)
            {
                return -1;
            }
        }
        else
        {
            if (x.IncludesDerivedTypes == null)
            {
                return 1;
            }

            result = y.IncludesDerivedTypes.Value.CompareTo(x.IncludesDerivedTypes.Value);
            if (result != 0)
            {
                return result;
            }
        }

        if (y.IsSplitEntityTypePrincipal == null)
        {
            if (x.IsSplitEntityTypePrincipal != null)
            {
                return -1;
            }
        }
        else
        {
            if (x.IsSplitEntityTypePrincipal == null)
            {
                return 1;
            }

            result = y.IsSplitEntityTypePrincipal.Value.CompareTo(x.IsSplitEntityTypePrincipal.Value);
            if (result != 0)
            {
                return result;
            }
        }

        result = TypeBaseNameComparer.Instance.Compare(x.TypeBase, y.TypeBase);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(x.Table.Name, y.Table.Name);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(x.Table.Schema, y.Table.Schema);
        if (result != 0)
        {
            return result;
        }

        if (x.Table is IStoreFunction xFunction)
        {
            if (y.Table is not IStoreFunction)
            {
                return 1;
            }

            var yFunction = (IStoreFunction)y.Table;
            if (xFunction.Parameters is IReadOnlyList<IStoreFunctionParameter> xParamsList
                && yFunction.Parameters is IReadOnlyList<IStoreFunctionParameter> yParamsList)
            {
                result = xParamsList.Count.CompareTo(yParamsList.Count);
                if (result != 0)
                {
                    return result;
                }

                for (var i = 0; i < xParamsList.Count; i++)
                {
                    var xp = xParamsList[i];
                    var yp = yParamsList[i];
                    result = StringComparer.Ordinal.Compare(xp.StoreType, yp.StoreType);
                    if (result != 0)
                    {
                        return result;
                    }

                    result = StringComparer.Ordinal.Compare(xp.Name, yp.Name);
                    if (result != 0)
                    {
                        return result;
                    }
                }
            }
            else
            {
                result = xFunction.Parameters.Count().CompareTo(yFunction.Parameters.Count());
                if (result != 0)
                {
                    return result;
                }

                result = xFunction.Parameters.Zip(
                        yFunction.Parameters, (xp, yp) =>
                        {
                            var paramResult = StringComparer.Ordinal.Compare(xp.StoreType, yp.StoreType);
                            return paramResult != 0 ? paramResult : StringComparer.Ordinal.Compare(xp.Name, yp.Name);
                        })
                    .FirstOrDefault(r => r != 0);

                if (result != 0)
                {
                    return result;
                }
            }
        }
        else if (y.Table is IStoreFunction)
        {
            return -1;
        }

        if (x.Table is IStoreStoredProcedure xSproc)
        {
            if (y.Table is not IStoreStoredProcedure)
            {
                return 1;
            }

            var ySproc = (IStoreStoredProcedure)y.Table;
            result = xSproc.Parameters.Count.CompareTo(ySproc.Parameters.Count);
            if (result != 0)
            {
                return result;
            }

            for (var i = 0; i < xSproc.Parameters.Count; i++)
            {
                var xp = xSproc.Parameters[i];
                var yp = ySproc.Parameters[i];
                result = StringComparer.Ordinal.Compare(xp.StoreType, yp.StoreType);
                if (result != 0)
                {
                    return result;
                }

                result = StringComparer.Ordinal.Compare(xp.Name, yp.Name);
                if (result != 0)
                {
                    return result;
                }
            }
        }
        else if (y.Table is IStoreStoredProcedure)
        {
            return -1;
        }

        result = x.ColumnMappings.Count().CompareTo(y.ColumnMappings.Count());
        return result != 0
            ? result
            : x.ColumnMappings.Zip(
                    y.ColumnMappings, (xc, yc) =>
                    {
                        var columnResult = StringComparer.Ordinal.Compare(xc.Property.Name, yc.Property.Name);
                        return columnResult != 0 ? columnResult : StringComparer.Ordinal.Compare(xc.Column.Name, yc.Column.Name);
                    })
                .FirstOrDefault(r => r != 0);
    }

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
