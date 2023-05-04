// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a column in a table.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IColumn : IColumnBase
{
    /// <summary>
    ///     Gets the containing table.
    /// </summary>
    new ITable Table { get; }

    /// <summary>
    ///     Gets the property mappings.
    /// </summary>
    new IReadOnlyList<IColumnMapping> PropertyMappings { get; }

    /// <summary>
    ///     Gets the maximum length of data that is allowed in this column. For example, if the property is a <see cref="string" /> '
    ///     then this is the maximum number of characters.
    /// </summary>
    int? MaxLength
        => PropertyMappings.First().Property.GetMaxLength(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Gets the precision of data that is allowed in this column. For example, if the property is a <see cref="decimal" /> '
    ///     then this is the maximum number of digits.
    /// </summary>
    int? Precision
        => PropertyMappings.First().Property.GetPrecision(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Gets the scale of data that is allowed in this column. For example, if the property is a <see cref="decimal" /> '
    ///     then this is the maximum number of decimal places.
    /// </summary>
    int? Scale
        => PropertyMappings.First().Property.GetScale(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Gets a value indicating whether or not the property can persist Unicode characters.
    /// </summary>
    bool? IsUnicode
        => PropertyMappings.First().Property.IsUnicode(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Returns a flag indicating whether the property is capable of storing only fixed-length data, such as strings.
    /// </summary>
    bool? IsFixedLength
        => PropertyMappings.First().Property.IsFixedLength(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Indicates whether or not this column acts as an automatic concurrency token by generating a different value
    ///     on every update in the same vein as 'rowversion'/'timestamp' columns on SQL Server.
    /// </summary>
    bool IsRowVersion
        => PropertyMappings.First().Property.IsConcurrencyToken
            && PropertyMappings.First().Property.ValueGenerated == ValueGenerated.OnAddOrUpdate;

    /// <summary>
    ///     Gets the column order.
    /// </summary>
    /// <value> The column order. </value>
    int? Order
        => PropertyMappings.First().Property.GetColumnOrder(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Returns the object that is used as the default value for this column.
    /// </summary>
    object? DefaultValue
    {
        get
        {
            TryGetDefaultValue(out var defaultValue);
            return defaultValue;
        }
    }

    /// <summary>
    ///     Gets the object that is used as the default value for this column.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>True if the default value was explicitly set; false otherwise.</returns>
    bool TryGetDefaultValue(out object? defaultValue)
    {
        foreach (var mapping in PropertyMappings)
        {
            var property = mapping.Property;
            if (!property.TryGetDefaultValue(StoreObjectIdentifier.Table(Table.Name, Table.Schema), out defaultValue))
            {
                continue;
            }

            var converter = mapping.TypeMapping.Converter;
            if (converter != null)
            {
                defaultValue = converter.ConvertToProvider(defaultValue);
            }

            return true;
        }

        defaultValue = null;
        return false;
    }

    /// <summary>
    ///     Returns the SQL expression that is used as the default value for this column.
    /// </summary>
    string? DefaultValueSql
        => PropertyMappings.First().Property
            .GetDefaultValueSql(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Returns the SQL expression that is used as the computed value for this column.
    /// </summary>
    string? ComputedColumnSql
        => PropertyMappings.First().Property
            .GetComputedColumnSql(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Returns whether the value of the computed column this property is mapped to is stored in the database, or calculated when
    ///     it is read.
    /// </summary>
    bool? IsStored
        => PropertyMappings.First().Property
            .GetIsStored(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Comment for this column
    /// </summary>
    string? Comment
        => PropertyMappings.First().Property
            .GetComment(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Collation for this column
    /// </summary>
    string? Collation
        => PropertyMappings.First().Property
            .GetCollation(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    /// <summary>
    ///     Returns the property mapping for the given entity type.
    /// </summary>
    /// <param name="entityType">An entity type.</param>
    /// <returns>The property mapping or <see langword="null" /> if not found.</returns>
    new IColumnMapping? FindColumnMapping(IReadOnlyEntityType entityType)
        => (IColumnMapping?)((IColumnBase)this).FindColumnMapping(entityType);

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
    string IColumnBase.ToDebugString(MetadataDebugStringOptions options, int indent)
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
