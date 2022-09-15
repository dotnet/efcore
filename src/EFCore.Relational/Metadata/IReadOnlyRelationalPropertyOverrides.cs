// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents property facet overrides for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyRelationalPropertyOverrides : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the property that the overrides are for.
    /// </summary>
    IReadOnlyProperty Property { get; }

    /// <summary>
    ///     The id of the table-like store object that these overrides are for.
    /// </summary>
    StoreObjectIdentifier StoreObject { get; }

    /// <summary>
    ///     Gets the column that the property maps to when targeting the specified table-like store object.
    /// </summary>
    string? ColumnName { get; }

    /// <summary>
    ///     Gets a value indicating whether the column name is overriden.
    /// </summary>
    bool IsColumnNameOverridden { get; }

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

        builder
            .Append(indentString)
            .Append("Override: ")
            .Append(StoreObject.DisplayName());

        if (IsColumnNameOverridden)
        {
            builder.Append(" ColumnName: ")
                .Append(ColumnName);
        }

        if ((options & MetadataDebugStringOptions.SingleLine) == 0)
        {
            if ((options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
            {
                builder.Append(AnnotationsToDebugString(indent: indent + 2));
            }
        }

        return builder.ToString();
    }
}
