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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetIsSharedTablePrincipal(bool isSharedTablePrincipal)
        => throw new NotImplementedException();

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
