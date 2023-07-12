// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents a primary or alternate key on an entity type.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyKey : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the properties that make up the key.
    /// </summary>
    IReadOnlyList<IReadOnlyProperty> Properties { get; }

    /// <summary>
    ///     Gets the entity type the key is defined on. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the key is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    IReadOnlyEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets all foreign keys that target a given primary or alternate key.
    /// </summary>
    /// <returns>The foreign keys that reference the given key.</returns>
    IEnumerable<IReadOnlyForeignKey> GetReferencingForeignKeys();

    /// <summary>
    ///     Returns a value indicating whether the key is the primary key.
    /// </summary>
    /// <returns><see langword="true" /> if the key is the primary key.</returns>
    bool IsPrimaryKey()
        => this == DeclaringEntityType.FindPrimaryKey();

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
            builder.Append("Key: ");
        }

        builder.AppendJoin(
            ", ", Properties.Select(
                p => singleLine
                    ? p.DeclaringType.DisplayName(omitSharedType: true) + "." + p.Name
                    : p.Name));

        if (IsPrimaryKey())
        {
            builder.Append(" PK");
        }

        if (!singleLine && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
