// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TableMapping : TableMappingBase, ITableMapping
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TableMapping(
        IEntityType entityType,
        Table table,
        bool includesDerivedTypes)
        : base(entityType, table, includesDerivedTypes)
    {
    }

    /// <inheritdoc />
    public new virtual ITable Table
        => (ITable)base.Table;

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
        get => ColumnMappings.Cast<IColumnMapping>();
    }
}
