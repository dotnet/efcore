// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a column-like object in a table-like object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IColumnBase : IAnnotatable
{
    /// <summary>
    ///     Gets the column name.
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     Gets the column type.
    /// </summary>
    string StoreType { get; }

    /// <summary>
    ///     Gets the provider type.
    /// </summary>
    Type ProviderClrType { get; }

    /// <summary>
    ///     Gets the value indicating whether the column can contain NULL.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    ///     Gets the containing table-like object.
    /// </summary>
    ITableBase Table { get; }

    /// <summary>
    ///     Gets the property mappings.
    /// </summary>
    IReadOnlyList<IColumnMappingBase> PropertyMappings { get; }

    /// <summary>
    ///     Returns the property mapping for the given entity type.
    /// </summary>
    /// <param name="entityType">An entity type.</param>
    /// <returns>The property mapping or <see langword="null" /> if not found.</returns>
    public virtual IColumnMappingBase? FindColumnMapping(IReadOnlyEntityType entityType)
    {
        for (var i = 0; i < PropertyMappings.Count; i++)
        {
            var mapping = PropertyMappings[i];
            if (mapping.Property.DeclaringEntityType.IsAssignableFrom(entityType))
            {
                return mapping;
            }
        }

        return null;
    }
}
