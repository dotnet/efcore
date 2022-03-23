// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a table-like object in the database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface ITableBase : IAnnotatable
{
    /// <summary>
    ///     Gets the name of the table in the database.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the schema of the table in the database.
    /// </summary>
    string? Schema { get; }

    /// <summary>
    ///     Gets the schema-qualified name of the table in the database.
    /// </summary>
    string SchemaQualifiedTableName
        => Schema == null ? Name : Schema + "." + Name;

    /// <summary>
    ///     Gets the database model.
    /// </summary>
    IRelationalModel Model { get; }

    /// <summary>
    ///     Gets the value indicating whether multiple entity types are sharing the rows in the table.
    /// </summary>
    bool IsShared { get; }

    /// <summary>
    ///     Gets the entity type mappings.
    /// </summary>
    IEnumerable<ITableMappingBase> EntityTypeMappings { get; }

    /// <summary>
    ///     Gets the columns defined for this table.
    /// </summary>
    IEnumerable<IColumnBase> Columns { get; }

    /// <summary>
    ///     Gets the column with the given name. Returns <see langword="null" /> if no column with the given name is defined.
    /// </summary>
    IColumnBase? FindColumn(string name);

    /// <summary>
    ///     Gets the column mapped to the given property. Returns <see langword="null" /> if no column is mapped to the given property.
    /// </summary>
    IColumnBase? FindColumn(IProperty property);

    /// <summary>
    ///     Gets the foreign keys for the given entity type that point to other entity types sharing this table.
    /// </summary>
    IEnumerable<IForeignKey> GetRowInternalForeignKeys(IEntityType entityType);

    /// <summary>
    ///     Gets the foreign keys referencing the given entity type from other entity types sharing this table.
    /// </summary>
    IEnumerable<IForeignKey> GetReferencingRowInternalForeignKeys(IEntityType entityType);

    /// <summary>
    ///     Gets the value indicating whether an entity of the given type might not be present in a row.
    /// </summary>
    bool IsOptional(IEntityType entityType);
}
