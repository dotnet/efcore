// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
///     Represents an index on a set of properties.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information and examples.
/// </remarks>
public interface IReadOnlyIndex : IReadOnlyAnnotatable
{
    /// <summary>
    ///     Gets the properties that this index is defined on.
    /// </summary>
    IReadOnlyList<IReadOnlyPropertyBase> Properties { get; }

    /// <summary>
    ///     Gets the name of this index.
    /// </summary>
    string? Name { get; }

    /// <summary>
    ///     Gets a value indicating whether the values assigned to the indexed properties are unique.
    /// </summary>
    bool IsUnique { get; }

    /// <summary>
    ///     A set of values indicating whether each corresponding index column has descending sort order.
    /// </summary>
    IReadOnlyList<bool>? IsDescending { get; }

    /// <summary>
    ///     Gets the complex-collection indices traversed to reach each indexed property.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When non-<see langword="null" />, this list has the same length as <see cref="Properties" />.
    ///         Each entry corresponds to the property at the same position and is either:
    ///     </para>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     <see langword="null" />, indicating the property is not reached through any complex collection.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     A list with one entry per complex-collection segment between the entity root and the property,
    ///                     ordered outermost-first (the entry at index 0 resolves the complex collection closest to the
    ///                     entity root). A <see langword="null" /> entry means the index applies to all elements of that
    ///                     collection (e.g. <c>Posts.Select(p => p.Title)</c>); a non-<see langword="null" /> entry means
    ///                     the index applies only to the element at that fixed position (e.g. <c>Posts[0].Title</c>).
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         A <see langword="null" /> top-level value means no property in this index traverses any complex collection.
    ///     </para>
    /// </remarks>
    IReadOnlyList<IReadOnlyList<int?>?>? CollectionIndices { get; }

    /// <summary>
    ///     Gets the entity type the index is defined on. This may be different from the type that <see cref="Properties" />
    ///     are defined on when the index is defined a derived type in an inheritance hierarchy (since the properties
    ///     may be defined on a base type).
    /// </summary>
    IReadOnlyEntityType DeclaringEntityType { get; }

    /// <summary>
    ///     Gets the friendly display name for the given <see cref="IReadOnlyIndex" />, returning its <see cref="Name" /> if one is defined,
    ///     or a string representation of its <see cref="Properties" /> if this is an unnamed index.
    /// </summary>
    /// <returns>The display name.</returns>
    [DebuggerStepThrough]
    string DisplayName()
        => Name is null ? Properties.Format() : $"'{Name}'";

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
            builder.Append("Index: ");
        }

        builder
            .AppendJoin(
                ", ",
                Properties.Select(p => singleLine
                    ? p.DeclaringType.DisplayName(omitSharedType: true) + "." + p.Name
                    : p.Name));

        if (Name != null)
        {
            builder.Append(" " + Name);
        }

        if (IsUnique)
        {
            builder.Append(" Unique");
        }

        if (!singleLine
            && (options & MetadataDebugStringOptions.IncludeAnnotations) != 0)
        {
            builder.Append(AnnotationsToDebugString(indent + 2));
        }

        return builder.ToString();
    }
}
