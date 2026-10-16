// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TableMappingBase<TColumnMapping> : Annotatable, ITableMappingBase
    where TColumnMapping : class, IColumnMappingBase
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TableMappingBase(
        ITypeBase typeBase,
        TableBase table,
        bool? includesDerivedTypes)
    {
        TypeBase = typeBase;
        Table = table;
        IncludesDerivedTypes = includesDerivedTypes;
    }

    /// <inheritdoc />
    public virtual ITypeBase TypeBase { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TableBase Table { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => Table.IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual List<TColumnMapping> ColumnMappings { get; }
        = [];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool AddColumnMapping(TColumnMapping columnMapping)
    {
        if (ColumnMappings.IndexOf(columnMapping, ColumnMappingBaseComparer.Instance) != -1)
        {
            return false;
        }

        ColumnMappings.Add(columnMapping);
        ColumnMappings.Sort(ColumnMappingBaseComparer.Instance);

        return true;
    }

    /// <inheritdoc />
    public virtual bool? IncludesDerivedTypes { get; }

    /// <inheritdoc />
    public virtual bool? IsSharedTablePrincipal { get; set; }

    /// <inheritdoc />
    public virtual bool? IsSplitEntityTypePrincipal { get; init; }

    /// <inheritdoc />
    public virtual bool IsSplitFragmentOptional { get; init; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetIsSharedTablePrincipal(bool isSharedTablePrincipal)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public virtual int CompareTo(ITableMappingBase? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        var result = 0;
        if (other.IsSharedTablePrincipal == null)
        {
            if (IsSharedTablePrincipal != null)
            {
                return 1;
            }
        }
        else
        {
            if (IsSharedTablePrincipal == null)
            {
                return -1;
            }

            result = other.IsSharedTablePrincipal.Value.CompareTo(IsSharedTablePrincipal.Value);
            if (result != 0)
            {
                return result;
            }
        }

        if (other.IncludesDerivedTypes == null)
        {
            if (IncludesDerivedTypes != null)
            {
                return -1;
            }
        }
        else
        {
            if (IncludesDerivedTypes == null)
            {
                return 1;
            }

            result = other.IncludesDerivedTypes.Value.CompareTo(IncludesDerivedTypes.Value);
            if (result != 0)
            {
                return result;
            }
        }

        if (other.IsSplitEntityTypePrincipal == null)
        {
            if (IsSplitEntityTypePrincipal != null)
            {
                return -1;
            }
        }
        else
        {
            if (IsSplitEntityTypePrincipal == null)
            {
                return 1;
            }

            result = other.IsSplitEntityTypePrincipal.Value.CompareTo(IsSplitEntityTypePrincipal.Value);
            if (result != 0)
            {
                return result;
            }
        }

        result = TypeBaseNameComparer.Instance.Compare(TypeBase, other.TypeBase);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(Table.Name, other.Table.Name);
        if (result != 0)
        {
            return result;
        }

        result = StringComparer.Ordinal.Compare(Table.Schema, other.Table.Schema);
        if (result != 0)
        {
            return result;
        }

        result = CompareTableDetails(other);
        if (result != 0)
        {
            return result;
        }

        result = ColumnMappings.Count.CompareTo(other.ColumnMappings.Count());
        return result != 0
            ? result
            : ColumnMappings.Zip(
                    other.ColumnMappings, (xc, yc) =>
                    {
                        var columnResult = StringComparer.Ordinal.Compare(xc.Property.Name, yc.Property.Name);
                        return columnResult != 0 ? columnResult : StringComparer.Ordinal.Compare(xc.Column.Name, yc.Column.Name);
                    })
                .FirstOrDefault(r => r != 0);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj)
            || (obj is ITableMappingBase other
                && TypeBase == other.TypeBase
                && Table.Equals(other.Table)
                && IncludesDerivedTypes == other.IncludesDerivedTypes
                && ((ITableMappingBase)this).ColumnMappings.SequenceEqual(
                    other.ColumnMappings, ReferenceEqualityComparer.Instance));

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(TypeBase, TypeBaseNameComparer.Instance);
        hashCode.Add(Table);

        foreach (var columnMapping in ((ITableMappingBase)this).ColumnMappings)
        {
            hashCode.Add(columnMapping.Property.Name);
            hashCode.Add(columnMapping.Column.Name);
        }

        hashCode.Add(IncludesDerivedTypes);
        return hashCode.ToHashCode();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual int CompareTableDetails(ITableMappingBase other)
        => other is IFunctionMapping or IStoredProcedureMapping ? -1 : 0;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((ITableMappingBase)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    IEnumerable<IColumnMappingBase> ITableMappingBase.ColumnMappings
    {
        [DebuggerStepThrough]
        get => ColumnMappings;
    }

    ITableBase ITableMappingBase.Table
    {
        [DebuggerStepThrough]
        get => Table;
    }
}
