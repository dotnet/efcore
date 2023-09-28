// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class TableBase : Annotatable, ITableBase
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public TableBase(string name, string? schema, RelationalModel model)
    {
        Schema = schema;
        Name = name;
        Model = model;
    }

    /// <inheritdoc />
    public virtual string? Schema { get; }

    /// <inheritdoc />
    public virtual string Name { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalModel Model { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsReadOnly
        => Model.IsReadOnly;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool IsShared { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedSet<ITableMappingBase> EntityTypeMappings { get; }
        = new(TableMappingBaseComparer.Instance);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedDictionary<string, IColumnBase> Columns { get; protected set; }
        = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public virtual IColumnBase? FindColumn(string name)
        => Columns.TryGetValue(name, out var column)
            ? column
            : null;

    /// <inheritdoc />
    public virtual IColumnBase? FindColumn(IProperty property)
        => property.GetDefaultColumnMappings()
            .FirstOrDefault(cm => cm.TableMapping.Table == this)
            ?.Column;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DisallowNull]
    public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? RowInternalForeignKeys { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? ReferencingRowInternalForeignKeys { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DisallowNull]
    public virtual Dictionary<IEntityType, bool>? OptionalEntityTypes { get; set; }

    /// <inheritdoc />
    public virtual bool IsOptional(IEntityType entityType)
    {
        if (OptionalEntityTypes == null)
        {
            CheckMappedEntityType(entityType);
            return false;
        }

        return !OptionalEntityTypes.TryGetValue(entityType, out var optional)
            ? throw new InvalidOperationException(
                RelationalStrings.TableNotMappedEntityType(entityType.DisplayName(), ((ITableBase)this).SchemaQualifiedName))
            : optional;
    }

    private void CheckMappedEntityType(IEntityType entityType)
    {
        if (EntityTypeMappings.All(m => m.EntityType != entityType))
        {
            throw new InvalidOperationException(
                RelationalStrings.TableNotMappedEntityType(entityType.DisplayName(), ((ITableBase)this).SchemaQualifiedName));
        }
    }

    /// <inheritdoc />
    IRelationalModel ITableBase.Model
    {
        [DebuggerStepThrough]
        get => Model;
    }

    /// <inheritdoc />
    IEnumerable<ITableMappingBase> ITableBase.EntityTypeMappings
    {
        [DebuggerStepThrough]
        get => EntityTypeMappings;
    }

    /// <inheritdoc />
    IEnumerable<IColumnBase> ITableBase.Columns
    {
        [DebuggerStepThrough]
        get => Columns.Values;
    }

    /// <inheritdoc />
    IEnumerable<IForeignKey> ITableBase.GetRowInternalForeignKeys(IEntityType entityType)
    {
        if (RowInternalForeignKeys != null
            && RowInternalForeignKeys.TryGetValue(entityType, out var foreignKeys))
        {
            return foreignKeys;
        }

        CheckMappedEntityType(entityType);
        return Enumerable.Empty<IForeignKey>();
    }

    /// <inheritdoc />
    IEnumerable<IForeignKey> ITableBase.GetReferencingRowInternalForeignKeys(IEntityType entityType)
    {
        if (ReferencingRowInternalForeignKeys != null
            && ReferencingRowInternalForeignKeys.TryGetValue(entityType, out var foreignKeys))
        {
            return foreignKeys;
        }

        CheckMappedEntityType(entityType);
        return Enumerable.Empty<IForeignKey>();
    }
}
