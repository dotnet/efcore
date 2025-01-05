// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class Column : ColumnBase<ColumnMapping>, IColumn
{
    // Warning: Never access these fields directly as access needs to be thread-safe
    private ColumnAccessors? _accessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public Column(
        string name,
        string type,
        Table table,
        RelationalTypeMapping? storeTypeMapping = null,
        ValueComparer? providerValueComparer = null)
        : base(name, type, table, storeTypeMapping, providerValueComparer)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public new virtual Table Table
        => (Table)base.Table;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ColumnAccessors Accessors
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _accessors, this, static column =>
                RuntimeFeature.IsDynamicCodeSupported
                    ? ColumnAccessorsFactory.Create(column)
                    : throw new InvalidOperationException(CoreStrings.NativeAotNoCompiledModel));
        set => _accessors = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((IColumn)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

    /// <inheritdoc />
    ITable IColumn.Table
    {
        [DebuggerStepThrough]
        get => Table;
    }

    /// <inheritdoc />
    IReadOnlyList<IColumnMapping> IColumn.PropertyMappings
    {
        [DebuggerStepThrough]
        get => PropertyMappings;
    }
}
