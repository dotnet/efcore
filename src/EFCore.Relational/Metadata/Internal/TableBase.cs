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
    public virtual bool IsShared { get; private set; }

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
    public virtual SortedSet<ITableMappingBase> ComplexTypeMappings { get; }
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
    public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? RowInternalForeignKeys { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SortedDictionary<IEntityType, IEnumerable<IForeignKey>>? ReferencingRowInternalForeignKeys { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DisallowNull]
    public virtual Dictionary<ITypeBase, bool>? OptionalTypes { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddTypeMapping(ITableMappingBase tableMapping, bool optional)
    {
        OptionalTypes ??= new Dictionary<ITypeBase, bool>();

        OptionalTypes.Add(tableMapping.TypeBase, optional);

        if (tableMapping.TypeBase is IEntityType)
        {
            EntityTypeMappings.Add(tableMapping);
        }
        else
        {
            ComplexTypeMappings.Add(tableMapping);
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddRowInternalForeignKey(IEntityType entityType, IForeignKey foreignKey)
    {
        if (RowInternalForeignKeys == null)
        {
            RowInternalForeignKeys = new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypeFullNameComparer.Instance);
            IsShared = true;
        }

        if (!RowInternalForeignKeys.TryGetValue(entityType, out var foreignKeys))
        {
            foreignKeys = new SortedSet<IForeignKey>(ForeignKeyComparer.Instance);
            RowInternalForeignKeys[entityType] = foreignKeys;
        }

        ((SortedSet<IForeignKey>)foreignKeys).Add(foreignKey);

        var principalEntityType = foreignKey.PrincipalEntityType;
        if (ReferencingRowInternalForeignKeys == null)
        {
            ReferencingRowInternalForeignKeys =
                new SortedDictionary<IEntityType, IEnumerable<IForeignKey>>(EntityTypeFullNameComparer.Instance);
            IsShared = true;
        }

        if (!ReferencingRowInternalForeignKeys.TryGetValue(principalEntityType, out var referencingForeignKeys))
        {
            referencingForeignKeys = new SortedSet<IForeignKey>(ForeignKeyComparer.Instance);
            ReferencingRowInternalForeignKeys[principalEntityType] = referencingForeignKeys;
        }

        ((SortedSet<IForeignKey>)referencingForeignKeys).Add(foreignKey);
    }

    /// <inheritdoc />
    public virtual bool IsOptional(ITypeBase typeBase)
    {
        if (OptionalTypes == null)
        {
            CheckMappedType(typeBase);
            return false;
        }

        return !OptionalTypes.TryGetValue(typeBase, out var optional)
            ? throw new InvalidOperationException(
                RelationalStrings.TableNotMappedEntityType(typeBase.DisplayName(), ((ITableBase)this).SchemaQualifiedName))
            : optional;
    }

    private void CheckMappedType(ITypeBase typeBase)
    {
        if (EntityTypeMappings.All(m => m.TypeBase != typeBase)
            && ComplexTypeMappings.All(m => m.TypeBase != typeBase))
        {
            throw new InvalidOperationException(
                RelationalStrings.TableNotMappedEntityType(typeBase.DisplayName(), ((ITableBase)this).SchemaQualifiedName));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string ToString()
        => ((ITableBase)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

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
    IEnumerable<ITableMappingBase> ITableBase.ComplexTypeMappings
    {
        [DebuggerStepThrough]
        get => ComplexTypeMappings;
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

        CheckMappedType(entityType);
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

        CheckMappedType(entityType);
        return Enumerable.Empty<IForeignKey>();
    }
}
