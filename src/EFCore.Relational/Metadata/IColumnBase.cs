// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

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
    ///     Gets the type mapping for the column-like object.
    /// </summary>
    RelationalTypeMapping StoreTypeMapping { get; }

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
    ///     Gets the <see cref="ValueComparer" /> for this column.
    /// </summary>
    /// <returns>The comparer.</returns>
    ValueComparer ProviderValueComparer
        => PropertyMappings.First().Property.GetProviderValueComparer();

    /// <summary>
    ///     Returns the property mapping for the given entity type.
    /// </summary>
    /// <param name="entityType">An entity type.</param>
    /// <returns>The property mapping or <see langword="null" /> if not found.</returns>
    IColumnMappingBase? FindColumnMapping(IReadOnlyEntityType entityType)
    {
        for (var i = 0; i < PropertyMappings.Count; i++)
        {
            var mapping = PropertyMappings[i];
            if (mapping.Property.DeclaringType.IsAssignableFrom(entityType))
            {
                return mapping;
            }
        }

        return null;
    }

    /// <summary>
    ///     <para>
    ///         Creates a human-readable representation of the given metadata.
    ///     </para>
    ///     <para>
    ///         Warning: Do not rely on the format of the returned string.
    ///         It is designed for debugging only and may change arbitrarily between releases.
    ///     </para>
    /// </summary>
    /// <param name="options">Options for generating the string.</param>
    /// <param name="indent">The number of indent spaces to use before each new line.</param>
    /// <returns>A human-readable representation.</returns>
    string ToDebugString(MetadataDebugStringOptions options = MetadataDebugStringOptions.ShortDefault, int indent = 0)
    {
        var builder = new StringBuilder();
        var indentString = new string(' ', indent);

        builder.Append(indentString);

        var singleLine = (options & MetadataDebugStringOptions.SingleLine) != 0;
        if (singleLine)
        {
            builder.Append($"Column: {Table.Name}.");
        }

        builder.Append(Name).Append(" (");
        builder.Append(StoreType).Append(')');
        builder.Append(IsNullable ? " Nullable" : " NonNullable");
        builder.Append(')');

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
