// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TableMapping : TableMappingBase<ColumnMapping>, ITableMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TableMapping(
        ITypeBase typeBase,
        Table table,
        bool? includesDerivedTypes)
        : base(typeBase, table, includesDerivedTypes)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual ITable Table
        => (ITable)base.Table;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IStoredProcedureMapping? InsertStoredProcedureMapping { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IStoredProcedureMapping? DeleteStoredProcedureMapping { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IStoredProcedureMapping? UpdateStoredProcedureMapping { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void SetIsSharedTablePrincipal(bool isSharedTablePrincipal)
    {
        if (IsSharedTablePrincipal == isSharedTablePrincipal)
        {
            return;
        }

        ((Table)Table).EntityTypeMappings.Remove(this);

        var removedColumnMappings = new List<ColumnMapping>();
        foreach (ColumnMapping columnMapping in ((ITableMapping)this).ColumnMappings)
        {
            ((Column)columnMapping.Column).RemovePropertyMapping(columnMapping);
            var columnMappings = (SortedSet<ColumnMapping>)columnMapping.Property.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableColumnMappings)!;
            columnMappings.Remove(columnMapping);

            removedColumnMappings.Add(columnMapping);
        }

        IsSharedTablePrincipal = isSharedTablePrincipal;

        // Re-add the mappings to update the order
        ((Table)Table).EntityTypeMappings.Add(this);

        foreach (var columnMapping in removedColumnMappings)
        {
            ((Column)columnMapping.Column).AddPropertyMapping(columnMapping);
            var columnMappings = (SortedSet<ColumnMapping>)columnMapping.Property.FindRuntimeAnnotationValue(
                RelationalAnnotationNames.TableColumnMappings)!;
            columnMappings.Add(columnMapping);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((ITableMapping)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    ITableBase ITableMappingBase.Table
    {
        [DebuggerStepThrough]
        get => Table;
    }

    /// <inheritdoc />
    IEnumerable<IColumnMapping> ITableMapping.ColumnMappings
    {
        [DebuggerStepThrough]
        get => ColumnMappings;
    }
}
