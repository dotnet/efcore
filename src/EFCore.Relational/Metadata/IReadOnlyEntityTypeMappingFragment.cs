// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents entity type mapping for a particular table-like store object.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyEntityTypeMappingFragment : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the entity type for which the fragment is defined.
    /// </summary>
    IReadOnlyEntityType EntityType { get; }

    /// <summary>
    ///     Gets store object for which the configuration is applied.
    /// </summary>
    StoreObjectIdentifier StoreObject { get; }

    /// <summary>
    ///     Gets a value indicating whether the associated table is ignored by Migrations.
    /// </summary>
    /// <returns>A value indicating whether the associated table is ignored by Migrations.</returns>
    bool? IsTableExcludedFromMigrations { get; }

    /// <summary>
    ///     Gets a value indicating whether a row might not exist for this fragment's store object even when the
    ///     principal row exists in the main table for the entity type.
    /// </summary>
    /// <returns><see langword="true" /> if a row for this fragment is optional; <see langword="false" /> otherwise.</returns>
    bool IsOptional
        => false;

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
            .Append("Fragment: ")
            .Append(StoreObject.DisplayName());

        if (IsTableExcludedFromMigrations == true)
        {
            builder.Append("ExcludedFromMigrations");
        }

        if (IsOptional)
        {
            builder.Append(" Optional");
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
