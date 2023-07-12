// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ColumnMappingBase : Annotatable, IColumnMappingBase
{
    private RelationalTypeMapping? _typeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ColumnMappingBase(
        IProperty property,
        IColumnBase column,
        ITableMappingBase tableMapping)
    {
        Property = property;
        Column = column;
        TableMapping = tableMapping;
    }

    /// <inheritdoc />
    public virtual IProperty Property { get; }

    /// <inheritdoc />
    public virtual IColumnBase Column { get; }

    /// <inheritdoc />
    public virtual RelationalTypeMapping TypeMapping
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _typeMapping, this, static mapping => mapping.GetTypeMapping());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual RelationalTypeMapping GetTypeMapping()
        => Property.GetRelationalTypeMapping();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ITableMappingBase TableMapping { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => ((AnnotatableBase)TableMapping).IsReadOnly;

    /// <inheritdoc />
    ITableMappingBase IColumnMappingBase.TableMapping
    {
        [DebuggerStepThrough]
        get => TableMapping;
    }
}
